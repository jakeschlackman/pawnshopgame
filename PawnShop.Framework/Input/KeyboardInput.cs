using Microsoft.Xna.Framework.Input;

namespace PawnShop.Framework.Input;

public sealed class KeyboardInput
{
    private readonly InputEdges edges = new();
    private bool wasActive;

    public InputFrame Read(bool active)
    {
        var keys = Keyboard.GetState();
        InputButtons held = InputButtons.None;
        if (keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W)) held |= InputButtons.Up;
        if (keys.IsKeyDown(Keys.Down) || keys.IsKeyDown(Keys.S)) held |= InputButtons.Down;
        if (keys.IsKeyDown(Keys.Enter) || keys.IsKeyDown(Keys.Space)) held |= InputButtons.Confirm;
        if (keys.IsKeyDown(Keys.Escape)) held |= InputButtons.Cancel;
        if (keys.IsKeyDown(Keys.Space)) held |= InputButtons.Interact;
        if (!active || !wasActive) edges.Synchronize(held);
        wasActive = active;
        return edges.Read(held);
    }
}
