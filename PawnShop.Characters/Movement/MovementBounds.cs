namespace PawnShop.Characters.Movement;

// Ground-space bounds, independent of a sprite's transparent canvas.
public readonly record struct MovementBounds
{
    public float Left { get; }
    public float Top { get; }
    public float Width { get; }
    public float Height { get; }
    public float Right => Left + Width;
    public float Bottom => Top + Height;

    public MovementBounds(float left, float top, float width, float height)
    {
        if (!float.IsFinite(left) || !float.IsFinite(top) || !float.IsFinite(width) || !float.IsFinite(height)
            || width <= 0 || height <= 0 || !float.IsFinite(left + width) || !float.IsFinite(top + height))
            throw new ArgumentOutOfRangeException(nameof(width), "Movement bounds require finite coordinates and positive dimensions.");
        Left = left; Top = top; Width = width; Height = height;
    }

    public bool Intersects(MovementBounds other) => Left < other.Right && Right > other.Left
        && Top < other.Bottom && Bottom > other.Top;
    public bool Contains(MovementBounds other) => other.Left >= Left && other.Right <= Right
        && other.Top >= Top && other.Bottom <= Bottom;
}
