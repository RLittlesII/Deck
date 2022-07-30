using DynamicData.Binding;

namespace Deck;

public interface ICard : IIndexAware
{
    public string Identifier { get; }
}