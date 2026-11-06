using Microsoft.Xna.Framework.Input;

namespace PawnShop.Framework.Input;

public enum ControlAction { Interact, PauseBack, MoveUp, MoveLeft, MoveDown, MoveRight }

public readonly record struct KeyBinding(Keys Primary, Keys Alternate = Keys.None)
{
    public bool IsDown(KeyboardState state) => state.IsKeyDown(Primary) || Alternate != Keys.None && state.IsKeyDown(Alternate);
}

// Immutable device mappings; game preferences project into this reusable input contract.
public sealed record KeyboardBindings
{
    public KeyBinding Interact { get; init; } = new(Keys.Z);
    public KeyBinding PauseBack { get; init; } = new(Keys.X);
    public KeyBinding MoveUp { get; init; } = new(Keys.W, Keys.Up);
    public KeyBinding MoveLeft { get; init; } = new(Keys.A, Keys.Left);
    public KeyBinding MoveDown { get; init; } = new(Keys.S, Keys.Down);
    public KeyBinding MoveRight { get; init; } = new(Keys.D, Keys.Right);

    public KeyBinding Get(ControlAction action) => action switch
    {
        ControlAction.Interact => Interact, ControlAction.PauseBack => PauseBack,
        ControlAction.MoveUp => MoveUp, ControlAction.MoveLeft => MoveLeft,
        ControlAction.MoveDown => MoveDown, ControlAction.MoveRight => MoveRight,
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
}
