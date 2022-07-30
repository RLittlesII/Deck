using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;
using Deck.Randomize;
using DynamicData;

namespace Deck;

public class CardDeck<T> : IReadOnlyCollection<T>
where T : ICard
{
    public CardDeck(IEnumerable<T> cards)
    {
        _fullDeck = cards.ToList().AsReadOnly();

        _deckCache
            .Connect()
            .Random(this, deck => deck._shuffler, false)
            .UpdateIndex()
            .Bind(out _remainingCards)
            .Subscribe();

        Reset();
    }

    public void Shuffle(ShuffleStrategy strategy = ShuffleStrategy.Current)
    {
        if (strategy == ShuffleStrategy.Deck)
        {
            Reset();
        }

        _shuffler.OnNext(Unit.Default);
    }

    public void Cut(int stacks = 2)
    {
        if (stacks > _deckCache.Count)
        {
            throw new Exception("Not enough cards for the number of stacks requested.");
        }

        var chunkSize = _deckCache.Count / stacks == 1 ? stacks + 1 : stacks;
        var stack = new Stack<T>();

        foreach (var chunk in Chunk(_deckCache.Items.Chunk(chunkSize)))
        {
            foreach (var card in chunk.Reverse())
            {
                stack.Push(card);
            }
        }

        _deckCache.Edit(update => update.AddOrUpdate(stack));

        IEnumerable<T[]> Chunk(IEnumerable<T[]> chunks)
        {
            var list = chunks.ToList();
            var count = list.Count;
            for (var i = count; i >= 0; i--)
            {
                yield return list[Random.Shared.Next(i)];
            }
        }
    }

    public IReadOnlyCollection<T> Draw(int number = 1)
    {
        var cards = _remainingCards.ToList();

        if (_remainingCards.Count < number)
        {
            throw new Exception("You ain't got no cards!");
        }

        var drawn = cards.Where(x => x.Index < number).ToList();

        _deckCache.RemoveKeys(drawn.Select(x => x.Identifier));

        return drawn.AsReadOnly();
    }

    public IEnumerator<T> GetEnumerator() => _remainingCards.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _remainingCards.Count;

    private void Reset() =>
        _deckCache.Edit(update => update.Load(_fullDeck));

    private ReadOnlyObservableCollection<T> _remainingCards;
    private readonly IReadOnlyCollection<T> _fullDeck;
    private readonly ISourceCache<T, string> _deckCache = new SourceCache<T, string>(x => x.Identifier);
    private readonly Subject<Unit> _shuffler = new Subject<Unit>();
}