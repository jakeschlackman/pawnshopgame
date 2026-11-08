namespace PawnShop.Framework.Rendering;

// Presentation-time frame selection only. Draw reads FrameIndex without advancing it.
public sealed class SpriteAnimator
{
    private SpriteClip clip;
    private TimeSpan elapsed;
    public SpriteSheetDefinition Definition { get; }
    public string AnimationName { get; private set; }
    public bool IsComplete => !clip.Loop && elapsed == clip.Duration;
    public TimeSpan Remaining => clip.Duration - elapsed;
    public int FrameIndex => clip.FirstFrame + (int)Math.Min(clip.FrameCount - 1,
        elapsed.Ticks / (TimeSpan.TicksPerMillisecond * clip.FrameDurationMs));

    public SpriteAnimator(SpriteSheetDefinition definition, string initialAnimation)
    {
        Definition = definition;
        AnimationName = initialAnimation;
        clip = GetClip(initialAnimation);
    }

    public void Play(string animation, bool restart = false)
    {
        if (!restart && animation == AnimationName) return;
        clip = GetClip(animation);
        AnimationName = animation;
        elapsed = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        long duration = clip.Duration.Ticks;
        if (!clip.Loop) elapsed = delta >= clip.Duration - elapsed ? clip.Duration : elapsed + delta;
        else
        {
            long increment = delta.Ticks % duration;
            long remaining = duration - elapsed.Ticks;
            elapsed = TimeSpan.FromTicks(increment >= remaining ? increment - remaining : elapsed.Ticks + increment);
        }
    }

    private SpriteClip GetClip(string name) => Definition.Animations.TryGetValue(name, out var result) ? result
        : throw new InvalidDataException($"Sprite animation '{name}' is not defined.");
}
