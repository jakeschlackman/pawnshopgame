namespace PawnShop.Framework.UI;

// Input owners call TryClick once per activation. Sound is opt-in presentation
// feedback; the callback should request playback at the host's update boundary.
public sealed class Button
{
    private readonly Action onClick;
    private readonly Action? onClickSound;
    public bool Enabled { get; set; } = true;

    public Button(Action onClick, Action? onClickSound = null)
    {
        ArgumentNullException.ThrowIfNull(onClick);
        this.onClick = onClick;
        this.onClickSound = onClickSound;
    }

    public bool TryClick()
    {
        if (!Enabled) return false;
        onClick();
        onClickSound?.Invoke();
        return true;
    }
}
