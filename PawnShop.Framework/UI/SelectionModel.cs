using PawnShop.Framework.Input;

namespace PawnShop.Framework.UI;

// Shared focus behavior; the owning view maps the selected row to its stable command ID.
public sealed class SelectionModel
{
    public int Index { get; private set; }
    public void Reset() => Index = 0;
    public bool Handle(InputFrame input, int count)
    {
        if (count <= 0) { Index = 0; return false; }
        Index = Math.Clamp(Index, 0, count - 1);
        if (input.SelectionIndex is { } index && index >= 0 && index < count) Index = index;
        if (input.Up != input.Down)
        {
            Index = (Index + (input.Down ? 1 : count - 1)) % count;
            return false;
        }
        return input.Confirm;
    }
}
