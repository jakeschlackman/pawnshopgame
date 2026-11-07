using System.Numerics;
using PawnShop.Characters.Movement;

namespace PawnShop.Characters.Player;

public enum PlayerGender { Male, Female }
public enum Facing { South, East, North, West }
public readonly record struct PlayerView(Guid Id, PlayerGender Gender, Vector2 Position, Facing Facing, bool Moving);

// Session-owned authoritative movement; consumers receive value snapshots only.
public sealed class PlayerState
{
    public const float Speed = 90;
    public const float HalfWidth = 6, FootprintHeight = 6;
    private Vector2 position;
    private Facing facing = Facing.South;
    private bool moving;
    private Vector2 previousIntent;
    private readonly Guid id = Guid.NewGuid();
    private readonly PlayerGender gender;
    public PlayerView View => new(id, gender, position, facing, moving);
    public MovementBounds Footprint => new(position.X - HalfWidth, position.Y - FootprintHeight, HalfWidth * 2, FootprintHeight);

    public PlayerState(PlayerGender gender, Vector2 spawn)
    {
        if (!Enum.IsDefined(gender)) throw new ArgumentOutOfRangeException(nameof(gender));
        if (!float.IsFinite(spawn.X) || !float.IsFinite(spawn.Y)) throw new ArgumentOutOfRangeException(nameof(spawn));
        this.gender = gender;
        position = spawn;
    }

    public void Move(Vector2 intent, TimeSpan delta, MovementBounds walkArea, IReadOnlyList<MovementBounds> obstacles)
    {
        if (delta < TimeSpan.Zero || !float.IsFinite(intent.X) || !float.IsFinite(intent.Y))
            throw new ArgumentOutOfRangeException(nameof(intent));
        if (walkArea.Width < HalfWidth * 2 || walkArea.Height < FootprintHeight)
            throw new ArgumentException("Walk area cannot contain the player's footprint.", nameof(walkArea));
        moving = false;
        if (delta == TimeSpan.Zero) return;
        if (intent == Vector2.Zero) { previousIntent = intent; return; }
        facing = Direction(intent);
        previousIntent = intent;
        // Normalize before speed is applied: diagonals travel at the same speed as cardinals.
        intent = Vector2.Normalize(intent);
        var amount = intent * Speed * (float)delta.TotalSeconds;
        if (!float.IsFinite(amount.X) || !float.IsFinite(amount.Y)) throw new ArgumentOutOfRangeException(nameof(delta));
        var before = position;
        float x = Math.Clamp(position.X + amount.X, walkArea.Left + HalfWidth, walkArea.Right - HalfWidth);
        // Sweep each axis, then slide along the other. Large ticks cannot tunnel through furniture.
        foreach (var obstacle in obstacles)
        {
            if (position.Y <= obstacle.Top || position.Y - FootprintHeight >= obstacle.Bottom) continue;
            if (amount.X > 0 && position.X + HalfWidth <= obstacle.Left && x + HalfWidth > obstacle.Left)
                x = Math.Min(x, obstacle.Left - HalfWidth);
            if (amount.X < 0 && position.X - HalfWidth >= obstacle.Right && x - HalfWidth < obstacle.Right)
                x = Math.Max(x, obstacle.Right + HalfWidth);
        }
        float y = Math.Clamp(position.Y + amount.Y, walkArea.Top + FootprintHeight, walkArea.Bottom);
        foreach (var obstacle in obstacles)
        {
            if (x + HalfWidth <= obstacle.Left || x - HalfWidth >= obstacle.Right) continue;
            if (amount.Y > 0 && position.Y <= obstacle.Top && y > obstacle.Top) y = Math.Min(y, obstacle.Top);
            if (amount.Y < 0 && position.Y - FootprintHeight >= obstacle.Bottom && y - FootprintHeight < obstacle.Bottom)
                y = Math.Max(y, obstacle.Bottom + FootprintHeight);
        }
        position = new(x, y);
        moving = position != before;
    }

    private Facing Direction(Vector2 direction)
    {
        float x = Math.Abs(direction.X), y = Math.Abs(direction.Y);
        bool horizontal = x > y;
        if (x == y)
        {
            bool changedX = Math.Sign(direction.X) != Math.Sign(previousIntent.X);
            bool changedY = Math.Sign(direction.Y) != Math.Sign(previousIntent.Y);
            // A newly pressed axis wins a diagonal tie; otherwise retain the last axis.
            // A fresh simultaneous diagonal starts vertical (the initial South facing).
            horizontal = changedX != changedY ? changedX : facing is Facing.East or Facing.West;
        }
        return horizontal ? direction.X > 0 ? Facing.East : Facing.West
            : direction.Y > 0 ? Facing.South : Facing.North;
    }
}
