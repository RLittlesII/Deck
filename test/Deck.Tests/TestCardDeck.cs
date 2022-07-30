using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deck.Tests;

public class TestCardDeck : IEnumerable<PlayingCard>
{
    public IEnumerator<PlayingCard> GetEnumerator() => _fullDeck.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private readonly IEnumerable<PlayingCard> _fullDeck =
        Enumerable.Range(0, 54).Select(identifier => new PlayingCard(identifier));
}