using DynamicData;

namespace Deck.Randomize;

internal class RandomChangeSet<TObject, TKey> : ChangeSet<TObject, TKey>, ISortedChangeSet<TObject, TKey>
{
    public new static readonly ISortedChangeSet<TObject, TKey> Empty = new RandomChangeSet<TObject, TKey>();

    public IKeyValueCollection<TObject, TKey> RandomizedItems { get; }

    public RandomChangeSet(IKeyValueCollection<TObject, TKey> randomizedItems,
        IEnumerable<Change<TObject, TKey>> updates)
        : base(updates)
    {
        RandomizedItems = randomizedItems;
    }

    private RandomChangeSet()
    {
        RandomizedItems = new KeyValueCollection<TObject, TKey>();
    }

    public bool Equals(RandomChangeSet<TObject, TKey> other)
    {
        return RandomizedItems.SequenceEqual(other.RandomizedItems);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RandomChangeSet<TObject, TKey>) obj);
    }

    public override int GetHashCode()
    {
        return RandomizedItems?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return $"RandomChangeSet. Count= {RandomizedItems.Count}. Updates = {Count}";
    }

    IKeyValueCollection<TObject, TKey> ISortedChangeSet<TObject, TKey>.SortedItems => RandomizedItems;
}