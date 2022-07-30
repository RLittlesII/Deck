using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deck.Tests;

internal class CutAFullDeckTests : IEnumerable<object[]>
{
    private readonly IEnumerable<PlayingCard> _fullDeck = new TestCardDeck().OrderBy(x => x.Identifier);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { _fullDeck, 3 };
        yield return new object[] { _fullDeck, 4 };
        yield return new object[] { _fullDeck, 5 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}