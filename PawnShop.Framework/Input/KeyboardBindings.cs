using Microsoft.Xna.Framework.Input;

namespace PawnShop.Framework.Input;

public enum ControlAction { Interact, PauseBack, MoveUp, MoveLeft, MoveDown, MoveRight }
public enum MouseBinding { None, Left, Right }

public readonly record struct KeyBinding(Keys Primary, Keys Alternate = Keys.None, MouseBinding AlternateMouse = MouseBinding.None)
{
    public bool IsDown(KeyboardState state) => state.IsKeyDown(Primary) || Alternate != Keys.None && state.IsKeyDown(Alternate);
    public bool IsDown(KeyboardState keys, MouseState mouse) => IsDown(keys) || (AlternateMouse switch
    {
        MouseBinding.Left => mouse.LeftButton == ButtonState.Pressed,
        MouseBinding.Right => mouse.RightButton == ButtonState.Pressed,
        _ => false
    });
}

// Immutable device mappings; game preferences project into this reusable input contract.
public sealed record KeyboardBindings
{
    public KeyBinding Interact { get; init; } = new(Keys.Z, AlternateMouse: MouseBinding.Left);
    public KeyBinding PauseBack { get; init; } = new(Keys.X, AlternateMouse: MouseBinding.Right);
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
