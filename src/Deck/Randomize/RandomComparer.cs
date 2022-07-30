namespace Deck.Randomize;

internal class RandomComparer<TObject, TKey> : IComparer<KeyValuePair<TKey, TObject>>
{
    private readonly int _boundary;

    public RandomComparer(int boundary = 0)
    {
        if (boundary == 0)
        {
            _boundary = DefaultBoundary;
        }
        else
        {
            _boundary = boundary;
        }
    }

    public int Compare(KeyValuePair<TKey, TObject> x, KeyValuePair<TKey, TObject> y) =>
        Random.Next(-_boundary, _boundary);


    private static readonly Random Random = new Random();
    private const int DefaultBoundary = 100;
}