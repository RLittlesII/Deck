using System.Collections.Generic;
using System.Linq;
using Rocket.Surgery.Extensions.Testing.Fixtures;

namespace Deck.Tests;

internal sealed class CardDeckFixture<T> : ITestFixtureBuilder
    where T : ICard
{
    public static implicit operator CardDeck<T>(CardDeckFixture<T> fixture) => fixture.Build();

    public CardDeckFixture<T> WithCards(IEnumerable<T> cards) => this.With(ref _cards, cards);
    private CardDeck<T> Build() => new CardDeck<T>(_cards);
    private IEnumerable<T> _cards = Enumerable.Empty<T>();
}