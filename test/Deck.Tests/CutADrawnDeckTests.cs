using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deck.Tests;

internal class CutADrawnDeckTests : IEnumerable<object[]>
{
    private readonly IEnumerable<PlayingCard> _fullDeck = new TestCardDeck().OrderBy(x => x.Identifier);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { _fullDeck, 3, 6 };
        yield return new object[] { _fullDeck, 4, 8 };
        yield return new object[] { _fullDeck, 5, 10 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}