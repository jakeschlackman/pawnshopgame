namespace PawnShop.Framework.Input;

[Flags]
public enum InputButtons { None = 0, Up = 1, Down = 2, Confirm = 4, Cancel = 8, Interact = 16 }
public readonly record struct InputFrame(bool Up = false, bool Down = false, bool Confirm = false,
    bool Cancel = false, bool Interact = false);

public sealed class InputEdges
{
    private InputButtons previous;
    public InputFrame Read(InputButtons held)
    {
        var pressed = held & ~previous;
        previous = held;
        return new(pressed.HasFlag(InputButtons.Up), pressed.HasFlag(InputButtons.Down),
            pressed.HasFlag(InputButtons.Confirm), pressed.HasFlag(InputButtons.Cancel),
            pressed.HasFlag(InputButtons.Interact));
    }

    // Seed with the held keys when focus returns; do not replay input from another app.
    public void Synchronize(InputButtons held) => previous = held;
}
