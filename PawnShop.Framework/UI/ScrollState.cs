namespace PawnShop.Framework.UI;

// Presentation state only; all dimensions use the owning UI canvas coordinates.
public sealed class ScrollState
{
    public int Offset { get; private set; }
    public int Maximum { get; private set; }
    public void Configure(int contentHeight, int viewportHeight)
    {
        Maximum = Math.Max(0, contentHeight - viewportHeight);
        MoveTo(Offset);
    }
    public void MoveTo(int offset) => Offset = Math.Clamp(offset, 0, Maximum);
    public void Reveal(int top, int height, int viewportHeight)
    {
        if (top < Offset) MoveTo(top);
        else if (top + height > Offset + viewportHeight) MoveTo(top + height - viewportHeight);
    }
}
