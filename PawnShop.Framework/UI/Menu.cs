using PawnShop.Framework.Input;

namespace PawnShop.Framework.UI;

public sealed record MenuItem<TCommand>(string Label, TCommand Command, string? DisabledReason = null)
    where TCommand : struct, Enum
{
    public bool Enabled => DisabledReason is null;
}

// Pure focus/selection model shared by title, pause and confirmation views.
// Disabled items remain focusable so keyboard users can read their reasons.
public sealed class Menu<TCommand> where TCommand : struct, Enum
{
    public IReadOnlyList<MenuItem<TCommand>> Items { get; }
    public int SelectedIndex { get; private set; }
    public MenuItem<TCommand> Selected => Items[SelectedIndex];

    public Menu(params MenuItem<TCommand>[] items)
    {
        if (items.Length == 0) throw new ArgumentException("A menu needs an item.", nameof(items));
        Items = Array.AsReadOnly((MenuItem<TCommand>[])items.Clone());
    }

    public TCommand? Handle(InputFrame input)
    {
        if (input.Up != input.Down)
        {
            SelectedIndex = (SelectedIndex + (input.Down ? 1 : Items.Count - 1)) % Items.Count;
            return null; // Navigation and confirm together never activate an unseen selection.
        }
        return input.Confirm && Selected.Enabled ? Selected.Command : null;
    }
}
