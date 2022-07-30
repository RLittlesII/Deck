using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deck.Tests;

internal class DrawFromAFullDeckTests : IEnumerable<object[]>
{
    private readonly IEnumerable<PlayingCard> _fullDeck = new TestCardDeck().OrderBy(x => x.Identifier);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { _fullDeck, 1 };
        yield return new object[] { _fullDeck, 2 };
        yield return new object[] { _fullDeck, 3 };
        yield return new object[] { _fullDeck, 4 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}