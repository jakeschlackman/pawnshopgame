namespace PawnShop.Game.Bootstrap;

// Presentation-only startup sequence. All three outputs share one clock.
internal sealed class TitleIntro
{
    public static readonly TimeSpan PanDuration = TimeSpan.FromSeconds(3.2);
    public static readonly TimeSpan MenuFadeDuration = TimeSpan.FromSeconds(0.8);
    private static readonly TimeSpan StudioFadeIn = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan StudioHold = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan StudioFadeOut = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan PanStart = StudioFadeIn + StudioHold + StudioFadeOut;
    private static readonly TimeSpan MenuFadeStart = PanStart + PanDuration - TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan Duration = MenuFadeStart + MenuFadeDuration;
    private static readonly TimeSpan SkipDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan SkipLogoFade = TimeSpan.FromSeconds(0.8);
    private static readonly TimeSpan SkipMenuFade = TimeSpan.FromSeconds(0.4);
    private TimeSpan elapsed;
    private bool skipping;
    private double skipFromDescent;
    private float skipFromStudio;
    private float skipFromMenu;

    public TitleIntro(bool play) => elapsed = play ? TimeSpan.Zero : Duration;

    public bool IsPlaying => elapsed < (skipping ? SkipDuration : Duration);
    public double Descent => skipping
        ? skipFromDescent + (1 - skipFromDescent) * Smooth(Progress(elapsed, SkipDuration))
        : Smooth(Progress(elapsed - PanStart, PanDuration));
    public float StudioOpacity => skipping
        ? skipFromStudio * (1 - (float)Smooth(Progress(elapsed, SkipLogoFade)))
        : (float)Math.Min(Smooth(Progress(elapsed, StudioFadeIn)),
            1 - Smooth(Progress(elapsed - StudioFadeIn - StudioHold, StudioFadeOut)));
    public float MenuOpacity
    {
        get
        {
            if (!skipping) return (float)EaseOut(Progress(elapsed - MenuFadeStart, MenuFadeDuration));
            double t = EaseOut(Progress(elapsed - SkipDuration + SkipMenuFade, SkipMenuFade));
            return (float)(skipFromMenu + (1 - skipFromMenu) * t);
        }
    }

    public void Update(TimeSpan delta, bool skip)
    {
        if (skip && !skipping)
        {
            skipFromDescent = Descent;
            skipFromStudio = StudioOpacity;
            skipFromMenu = MenuOpacity;
            skipping = true;
            elapsed = TimeSpan.Zero;
        }
        var remaining = (skipping ? SkipDuration : Duration) - elapsed;
        elapsed += delta < remaining ? delta : remaining;
    }

    private static double Progress(TimeSpan elapsed, TimeSpan duration) =>
        Math.Clamp((double)elapsed.Ticks / duration.Ticks, 0, 1);
    private static double Smooth(double t) => t * t * (3 - 2 * t);
    private static double EaseOut(double t) => t * (2 - t);
}
