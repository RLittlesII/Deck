using System.Collections;
using System.Collections.Generic;

namespace Deck.Tests;

public class FullDeckTests : IEnumerable<object[]>
{
    private readonly IEnumerable<PlayingCard> _fullDeck = new TestCardDeck();

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { _fullDeck };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}