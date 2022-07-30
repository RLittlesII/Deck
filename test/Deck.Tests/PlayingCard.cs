using System;

namespace Deck.Tests;

public class PlayingCard : ICard, IComparable<ICard>
{
    public PlayingCard(int identifier)
    {
        Identifier = (Index = identifier).ToString();
    }

    public PlayingCard()
    {
        Identifier = Guid.NewGuid().ToString();
    }
    public string Identifier { get; }

    public int Index { get; set; }

    public int CompareTo(object? obj) => CompareTo((ICard?) obj);
    public int CompareTo(ICard? other) =>
        string.Compare(Identifier, other?.Identifier, StringComparison.InvariantCultureIgnoreCase);
}