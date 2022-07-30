namespace Deck;

public enum ShuffleStrategy
{
    /// <summary>
    /// Shuffle the current cards that remain in the deck.
    /// </summary>
    Current,

    /// <summary>
    /// Shuffle all the cards in the deck.
    /// </summary>
    Deck
}