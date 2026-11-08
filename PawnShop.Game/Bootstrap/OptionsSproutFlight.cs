using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Bootstrap;

public enum OptionsSproutPhase { None, FinishingPoint, Flying, Arrived }

// Application presentation only. Each boundary is presented before the next phase;
// excess time is deliberately consumed, never forwarded into an Options action.
public sealed class OptionsSproutFlight
{
    private readonly SpriteAnimator animator;
    private TimeSpan remainingPoint;
    private TimeSpan flightElapsed;
    public OptionsSproutPhase Phase { get; private set; }
    public bool IsReturning { get; private set; }
    public Vector2 ReturnAnchor { get; private set; }
    public bool IsBusy => Phase is not (OptionsSproutPhase.None or OptionsSproutPhase.Arrived);
    public float Progress => (float)(flightElapsed.TotalSeconds / PanoramaCamera.TransitionDuration.TotalSeconds);

    public OptionsSproutFlight(SpriteAnimator animator)
    {
        this.animator = animator;
        if (!animator.Definition.Animations.TryGetValue("Idle", out var clip) || !clip.Loop)
            throw new InvalidDataException("sprout-title.json: requires looping Idle for the Options float.");
    }

    public void Start()
    {
        if (IsBusy) return;
        IsReturning = false;
        remainingPoint = animator.Remaining;
        flightElapsed = TimeSpan.Zero;
        Phase = OptionsSproutPhase.FinishingPoint;
    }

    public void StartReturn(Vector2 anchor)
    {
        if (IsBusy) return;
        ReturnAnchor = anchor;
        IsReturning = true;
        remainingPoint = flightElapsed = TimeSpan.Zero;
        Phase = OptionsSproutPhase.Flying;
        // Keep the current pointing frame/clock; no new takeoff or restart.
    }

    public Vector2 GetAnchor(Vector2 menuAnchor, Vector2 firstOptionAnchor) => Vector2.SmoothStep(
        IsReturning ? ReturnAnchor : menuAnchor, IsReturning ? menuAnchor : firstOptionAnchor, Progress);

    public void Reset()
    {
        Phase = OptionsSproutPhase.None;
        IsReturning = false;
        ReturnAnchor = Vector2.Zero;
        remainingPoint = flightElapsed = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        switch (Phase)
        {
            case OptionsSproutPhase.FinishingPoint:
                if (remainingPoint == TimeSpan.Zero) { Phase = OptionsSproutPhase.Flying; break; }
                // Idle loops normally. Stop one tick short of its wrap so the final
                // frame is displayed before travel; Idle continues throughout the float.
                if (delta >= remainingPoint)
                {
                    animator.Update(remainingPoint - TimeSpan.FromTicks(1));
                    remainingPoint = TimeSpan.Zero;
                }
                else { animator.Update(delta); remainingPoint -= delta; }
                break;
            case OptionsSproutPhase.Flying:
                var step = delta < PanoramaCamera.TransitionDuration - flightElapsed ? delta : PanoramaCamera.TransitionDuration - flightElapsed;
                flightElapsed += step;
                animator.Update(step);
                if (flightElapsed == PanoramaCamera.TransitionDuration) Phase = OptionsSproutPhase.Arrived;
                break;
        }
    }
}
