using Microsoft.Xna.Framework.Input;

namespace PawnShop.Framework.Input;

public sealed class KeyboardInput
{
    private readonly InputEdges edges = new();
    private bool wasActive;
    public Keys InteractKey { get; set; } = Keys.Space;
    public Keys PauseKey { get; set; } = Keys.Escape;
    public bool InMenus { get; set; } = true;
    public KeyboardState State { get; private set; }

    public InputFrame Read(bool active)
    {
        var keys = Keyboard.GetState();
        State = keys;
        InputButtons held = InputButtons.None;
        if (keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W)) held |= InputButtons.Up;
        if (keys.IsKeyDown(Keys.Down) || keys.IsKeyDown(Keys.S)) held |= InputButtons.Down;
        if (keys.IsKeyDown(Keys.Enter) || keys.IsKeyDown(Keys.Space)) held |= InputButtons.Confirm;
        if (keys.IsKeyDown(InMenus ? Keys.Escape : PauseKey)) held |= InputButtons.Cancel;
        if (keys.IsKeyDown(InteractKey)) held |= InputButtons.Interact;
        if (keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.A)) held |= InputButtons.Left;
        if (keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D)) held |= InputButtons.Right;
        if (keys.IsKeyDown(Keys.Tab)) held |= InputButtons.NextTab;
        if (!active || !wasActive) edges.Synchronize(held);
        wasActive = active;
        return edges.Read(held);
    }
}
