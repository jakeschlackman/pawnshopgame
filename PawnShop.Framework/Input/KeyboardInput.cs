using Microsoft.Xna.Framework.Input;

namespace PawnShop.Framework.Input;

public sealed class KeyboardInput
{
    private readonly InputEdges edges = new();
    private bool wasActive;
    public KeyboardBindings Bindings { get; set; } = new();
    public KeyboardState State { get; private set; }

    public InputFrame Read(bool active)
    {
        var keys = Keyboard.GetState();
        return Read(keys, active);
    }

    public InputFrame Read(MouseState mouse, bool active) => Read(Keyboard.GetState(), mouse, active);

    // Device-independent overload lets headless tests exercise the actual action mapping.
    public InputFrame Read(KeyboardState keys, bool active) => Read(keys, default, active);

    public InputFrame Read(KeyboardState keys, MouseState mouse, bool active)
    {
        bool ready = active && wasActive;
        State = keys;
        InputButtons held = InputButtons.None;
        if (Bindings.MoveUp.IsDown(keys)) held |= InputButtons.Up;
        if (Bindings.MoveDown.IsDown(keys)) held |= InputButtons.Down;
        if (Bindings.Interact.IsDown(keys) || keys.IsKeyDown(Keys.Enter) || keys.IsKeyDown(Keys.Space)) held |= InputButtons.Confirm;
        if (Bindings.PauseBack.IsDown(keys, mouse) || keys.IsKeyDown(Keys.Escape)) held |= InputButtons.Cancel;
        // Mouse confirmation stays with UI hit testing; a blank click must not activate a menu.
        if (Bindings.Interact.IsDown(keys, mouse)) held |= InputButtons.Interact;
        if (Bindings.MoveLeft.IsDown(keys)) held |= InputButtons.Left;
        if (Bindings.MoveRight.IsDown(keys)) held |= InputButtons.Right;
        if (keys.IsKeyDown(Keys.Tab)) held |= InputButtons.NextTab;
        if (!active || !wasActive) edges.Synchronize(held);
        wasActive = active;
        var frame = edges.Read(held);
        if (!ready) return default;
        return frame with
        {
            MoveX = (Bindings.MoveRight.IsDown(keys) ? 1 : 0) - (Bindings.MoveLeft.IsDown(keys) ? 1 : 0),
            MoveY = (Bindings.MoveDown.IsDown(keys) ? 1 : 0) - (Bindings.MoveUp.IsDown(keys) ? 1 : 0)
        };
    }
}
