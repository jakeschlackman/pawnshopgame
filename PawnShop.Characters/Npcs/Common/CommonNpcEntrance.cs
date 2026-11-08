using PawnShop.Characters.Movement;

namespace PawnShop.Characters.Npcs.Common;

// A bounded example entrance, not a shopper navigation/AI system. The session
// supplies simulation time and collision bounds. West reuses the approved East rig.
public sealed class CommonNpcEntrance
{
    public const float Speed = 80;
    public float X { get; private set; }
    public int Y { get; }
    private readonly float destinationX;
    private double walkMilliseconds;
    private double arrivalMilliseconds;
    private bool reachedDestination;
    public int SpriteFrame { get; private set; } = 1;
    public bool Arrived => reachedDestination && arrivalMilliseconds >= 200;

    public CommonNpcEntrance(float startX, float destinationX, int y)
    {
        if (!float.IsFinite(startX) || !float.IsFinite(destinationX) || startX <= destinationX)
            throw new ArgumentException("Entrance must start to the right of its destination.");
        X = startX;
        this.destinationX = destinationX;
        Y = y;
    }

    public void Update(TimeSpan delta, IReadOnlyList<MovementBounds> obstacles)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        if (delta == TimeSpan.Zero || Arrived) return;
        double milliseconds = delta.TotalMilliseconds;
        if (!reachedDestination)
        {
            float before = X;
            float next = (float)Math.Max(destinationX, X - Speed * delta.TotalSeconds);
            foreach (var obstacle in obstacles)
            {
                if (Y <= obstacle.Top || Y - 6 >= obstacle.Bottom) continue;
                if (X - 8 >= obstacle.Right && next - 8 < obstacle.Right)
                    next = Math.Max(next, obstacle.Right + 8);
            }
            X = next;
            if (X > destinationX)
            {
                if (X < before)
                {
                    walkMilliseconds = (walkMilliseconds + milliseconds) % 800;
                    SpriteFrame = 1 + (int)(walkMilliseconds / 100);
                }
                else SpriteFrame = 0; // Wait for the path to clear; never push the player.
                return;
            }
            reachedDestination = true;
            milliseconds = Math.Max(0, milliseconds - (before - destinationX) / Speed * 1000);
        }
        arrivalMilliseconds = Math.Min(200, arrivalMilliseconds + milliseconds);
        // Accepted stop sequence: walk-1, middle settling, standing.
        SpriteFrame = arrivalMilliseconds < 100 ? 1 : arrivalMilliseconds < 200 ? 9 : 0;
    }
}
