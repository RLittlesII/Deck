using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Deck.Tests;

public class CardDeckTests
{
    public class TheCutMethod
    {
        [Theory]
        [ClassData(typeof(FullDeckTests))]
        public void GivenTooManyStacks_WhenCut_ThenExceptionThrown(IEnumerable<PlayingCard> cards)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            var result = Record.Exception(() => sut.Cut(60));

            // Then
            result.Should().BeOfType<Exception>();
        }

        [Theory]
        [ClassData(typeof(CutAFullDeckTests))]
        public void GivenCards_WhenCut_ThenOrderDifferent(IEnumerable<PlayingCard> cards, int stacks)
        {
            // Given
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(cards);

            // When
            sut.Cut(stacks);

            // Then
            sut.Should().NotBeInAscendingOrder(x => x.Identifier);
        }

        [Theory]
        [ClassData(typeof(CutAFullDeckTests))]
        public void GivenCards_WhenCut_ThenCountSame(IEnumerable<PlayingCard> cards, int stacks)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            sut.Cut(stacks);

            // Then
            sut.Should().HaveCount(playingCards.Count);
        }

        [Theory]
        [ClassData(typeof(CutADrawnDeckTests))]
        public void GivenDrawn_WhenCut_ThenCountCorrect(IEnumerable<PlayingCard> cards, int stacks, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            var drawn = sut.Draw(draw);
            sut.Cut(stacks);

            // Then
            sut.Should()
                .HaveCount(playingCards.Count - draw)
                .And
                .Subject
                .Should()
                .NotContain(drawn);
        }

        [Theory]
        [ClassData(typeof(CutAFullDeckTests))]
        public void GivenShuffled_WhenCut_ThenOrderDifferent(IEnumerable<PlayingCard> deck, int stacks)
        {
            // Given
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(deck);
            sut.Shuffle();

            // When
            sut.Cut(stacks);

            // Then
            sut.Should().NotBeInAscendingOrder(x => x.Identifier);
        }

    }

    public class TheDrawMethod
    {
        [Fact]
        public void GivenNotEnoughCards_WhenDraw_ThenExceptionThrown()
        {
            // Given
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>();

            // When
            var result = Record.Exception(() => sut.Draw(1));

            // Then
            result.Should().BeOfType<Exception>();
        }

        [Theory]
        [ClassData(typeof(DrawFromAFullDeckTests))]
        public void GivenCards_WhenDraw_ThenCountReduced(IEnumerable<PlayingCard> cards, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            sut.Draw(draw);

            // Then
            sut.Should().HaveCount(playingCards.Count - draw);
        }

        [Theory]
        [ClassData(typeof(DrawFromAFullDeckTests))]
        public void GivenCards_WhenDraw_ThenReturnCorrectAmount(IEnumerable<PlayingCard> cards, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            var result = sut.Draw(draw);

            // Then
            result.Should().HaveCount(draw);
        }

        [Fact]
        public void GivenCards_WhenDraw_ThenRemainsInOrder()
        {

            // Given
            var sorted = new[]
                { new PlayingCard { Index = 1 }, new PlayingCard { Index = 2 }, new PlayingCard { Index = 3 } };
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(sorted);

            // When
            var drawn = sut.Draw(1);
            var result = sorted.Except(drawn).OrderBy(x => x.Index).ToList();

            // Then
            sut
                .Should()
                .SatisfyRespectively(first => first.Should()
                        .BeSameAs(result[0]),
                    second => second.Should()
                        .BeSameAs(result[1]));
        }
    }

    public class TheShuffleMethod
    {
        [Theory]
        [ClassData(typeof(FullDeckTests))]
        public void GivenCards_WhenShuffled_ThenOrderHasChanged(IEnumerable<PlayingCard> cards)
        {
            // Given
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(cards);

            // When
            sut.Shuffle();

            // Then
            sut.Should().NotBeInAscendingOrder(x => x.Identifier);
        }

        [Theory]
        [ClassData(typeof(ShuffleADrawnDeckTests))]
        public void GivenDrawn_WhenShuffleCurrent_ThenCountCorrect(IEnumerable<PlayingCard> cards, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            sut.Draw(draw);
            sut.Shuffle();

            // Then
            sut.Should().HaveCount(playingCards.Count - draw);
        }

        [Theory]
        [ClassData(typeof(ShuffleADrawnDeckTests))]
        public void GivenDrawn_WhenShuffleDeck_ThenCountCorrect(IEnumerable<PlayingCard> cards, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            sut.Draw(draw);
            sut.Shuffle(ShuffleStrategy.Deck);

            // Then
            sut.Should().HaveCount(playingCards.Count);
        }
    }

    public class TheCountProperty
    {
        [Theory]
        [ClassData(typeof(FullDeckTests))]
        public void GivenCards_WhenConstructed_ThenHasCount(IEnumerable<PlayingCard> cards)
        {
            // Given
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(cards);

            // When, Then
            sut.Should().HaveCount(54);
        }

        [Theory]
        [ClassData(typeof(DrawFromAFullDeckTests))]
        public void GivenCards_WhenDraw_ThenCountDecreased(IEnumerable<PlayingCard> cards, int draw)
        {
            // Given
            var playingCards = cards.ToList();
            CardDeck<PlayingCard> sut = new CardDeckFixture<PlayingCard>().WithCards(playingCards);

            // When
            sut.Draw(draw);

            // Then
            sut.Should().HaveCount(playingCards.Count - draw);
        }
    }
}