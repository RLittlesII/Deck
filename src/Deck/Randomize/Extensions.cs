using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace Deck.Randomize;

public static class Extensions
{
    /// <summary>
    /// Sorts using the specified comparer.
    /// Returns the underlying ChangeSet as as per the system conventions.
    /// The resulting change set also exposes a sorted key value collection of of the underlying cached data
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="randomizer">The randomizer.</param>
    /// <param name="sortOptimisations">Sort optimisation flags. Specify one or more sort optimisations</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">
    /// source
    /// or
    /// comparer
    /// </exception>
    public static IObservable<ISortedChangeSet<TObject, TKey>> Random<TObject, TKey>(
        this IObservable<IChangeSet<TObject, TKey>> source,
        IObservable<Unit> randomizer,
        SortOptimisations sortOptimisations = SortOptimisations.None)
        where TKey : notnull
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (randomizer == null)
        {
            throw new ArgumentNullException(nameof(randomizer));
        }

        return new Random<TObject, TKey>(source, sortOptimisations, randomizer).Run();
    }

    public static IObservable<ISortedChangeSet<TObject, TKey>> Random<TObject, TKey, TTarget>(
        this IObservable<IChangeSet<TObject, TKey>> source,
        TTarget target,
        Expression<Func<TTarget, IObservable<Unit>>> randomizer,
        bool randomizeImmediately = true,
        SortOptimisations sortOptimisations = SortOptimisations.None)
        where TTarget : class
        where TKey : notnull
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (randomizer == null)
        {
            throw new ArgumentNullException(nameof(randomizer));
        }

        var deferred =
            Observable.Defer(() =>
                target
                    .WhenAnyValue(randomizer)
                    .Do(_ => { })
                    .Select(
                        (Func<IObservable<Unit>, IObservable<Unit>>) (command => command ?? Observable.Empty<Unit>()))
                    .Switch());

        var randomized = source.Publish(_ => deferred.Select(o => o));

        return new Random<TObject, TKey>(source, sortOptimisations, randomized, randomizeImmediately).Run();
    }
}