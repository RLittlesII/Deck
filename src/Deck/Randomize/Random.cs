using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;

namespace Deck.Randomize;

internal sealed class Random<TObject, TKey> where TKey : notnull
{
    private readonly IObservable<IChangeSet<TObject, TKey>> _source;
    private readonly SortOptimisations _sortOptimisations;
    private readonly bool _randomizeImmediately;
    private readonly IObservable<Unit>? _randomizer;
    private readonly int _resetThreshold;
    private readonly int _boundary;

    public Random(IObservable<IChangeSet<TObject, TKey>> source,
        SortOptimisations sortOptimisations = SortOptimisations.None,
        IObservable<Unit>? randomizer = null,
        bool randomizeImmediately = true,
        int boundary = 0,
        int resetThreshold = -1)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _sortOptimisations = sortOptimisations;
        _randomizeImmediately = randomizeImmediately;
        _randomizer = randomizer ?? Observable.Never<Unit>();
        _boundary = boundary;
        _resetThreshold = resetThreshold;
    }

    public IObservable<ISortedChangeSet<TObject, TKey>> Run() =>
        Observable.Create<ISortedChangeSet<TObject, TKey>>(observer =>
        {
            var sorter = new Randomizer(_sortOptimisations, _boundary, _randomizeImmediately, _resetThreshold);
            var locker = new object();

            //check for nulls so we can prevent a lock when not required
            if (_randomizer == null)
            {
                return _source
                    .Select(changeSet => sorter.Random(changeSet))
                    .Where(result => result != null)
                    .SubscribeSafe(observer);
            }

            var sortAgain =
                _randomizer
                    .Synchronize(locker)
                    .Select(_ => sorter.Random());

            var dataChanged =
                _source
                    .Synchronize(locker)
                    .Select(changeSet => sorter.Random(changeSet));

            return dataChanged
                .Merge(sortAgain)
                .Where(result => result != null)
                .SubscribeSafe(observer);
        });

    private class Randomizer
    {
        private readonly ChangeAwareCache<TObject, TKey> _cache = new ChangeAwareCache<TObject, TKey>();
        private readonly SortOptimisations _optimisations;
        private readonly bool _randomizeImmediately;
        private readonly int _resetThreshold;

        private readonly RandomComparer<TObject, TKey> _comparer;
        private IKeyValueCollection<TObject, TKey> _sorted = new KeyValueCollection<TObject, TKey>();
        private bool _haveReceivedData;
        private bool _initialised;
        private RandomizeCalculator<TObject, TKey> _calculator;

        public Randomizer(SortOptimisations optimisations, int boundary, bool randomizeImmediately = true,
            int resetThreshold = -1)
        {
            _optimisations = optimisations;
            _randomizeImmediately = randomizeImmediately;
            _resetThreshold = resetThreshold;
            _comparer = new RandomComparer<TObject, TKey>(boundary);
            if (randomizeImmediately)
            {
                DoRandom(SortReason.InitialLoad);
            }
        }

        public Randomizer(SortOptimisations optimisations, int boundary, int resetThreshold = -1) :
            this(optimisations, resetThreshold, false, boundary)
        {
        }

        /// <summary>
        /// Sorts the specified changes. Will return null if there are no changes
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <returns></returns>
        public ISortedChangeSet<TObject, TKey> Random(IChangeSet<TObject, TKey> changes) =>
            DoRandom(SortReason.DataChanged, changes);

        /// <summary>
        /// Sorts all data using the current comparer
        /// </summary>
        /// <returns></returns>
        public ISortedChangeSet<TObject, TKey> Random() => DoRandom(SortReason.Reorder);

        /// <summary>
        /// Sorts using the specified sorter. Will return null if there are no changes
        /// </summary>
        /// <param name="sortReason">The sort reason.</param>
        /// <param name="changes">The changes.</param>
        /// <returns></returns>
        private ISortedChangeSet<TObject, TKey> DoRandom(SortReason sortReason,
            IChangeSet<TObject, TKey>? changes = null)
        {
            if (changes != null)
            {
                _cache.Clone(changes);
                changes = _cache.CaptureChanges();
                _haveReceivedData = true;
                if (_comparer == null)
                {
                    return null;
                }
            }

            //if the comparer is not set, return nothing
            if (_comparer == null || !_haveReceivedData)
            {
                return null;
            }

            if (!_initialised)
            {
                sortReason = SortReason.InitialLoad;
                _initialised = true;
            }
            else if (changes != null && (_resetThreshold > 0 && changes.Count >= _resetThreshold))
            {
                sortReason = SortReason.Reset;
            }

            IChangeSet<TObject, TKey> changeSet;
            switch (sortReason)
            {
                case SortReason.InitialLoad:
                {
                    //For the first batch, changes may have arrived before the comparer was set.
                    //therefore infer the first batch of changes from the cache
                    _calculator = new RandomizeCalculator<TObject, TKey>(_comparer, _optimisations);
                    changeSet = _calculator.Load(_cache, _randomizeImmediately);
                }

                    break;
                case SortReason.Reset:
                {
                    _calculator.Reset(_cache);
                    changeSet = changes;
                }

                    break;
                case SortReason.DataChanged:
                {
                    changeSet = _calculator.Calculate(changes);
                }

                    break;

                case SortReason.ComparerChanged:
                {
                    changeSet = _calculator.ChangeComparer(_comparer);
                    if (_resetThreshold > 0 && _cache.Count >= _resetThreshold)
                    {
                        sortReason = SortReason.Reset;
                        _calculator.Reset(_cache);
                    }
                    else
                    {
                        sortReason = SortReason.Reorder;
                        changeSet = _calculator.Reorder();
                    }
                }

                    break;

                case SortReason.Reorder:
                {
                    changeSet = _calculator.Reorder();
                }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortReason));
            }

            Debug.Assert(changeSet != null, "changeSet != null");
            if (sortReason is SortReason.InitialLoad or SortReason.DataChanged && changeSet.Count == 0)
            {
                return null;
            }

            if (sortReason == SortReason.Reorder && changeSet.Count == 0)
            {
                return null;
            }

            _sorted = new KeyValueCollection<TObject, TKey>(_calculator.List.ToList(), _comparer, sortReason,
                _optimisations);
            return new RandomChangeSet<TObject, TKey>(_sorted, changeSet);
        }
    }
}