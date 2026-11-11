namespace PawnShop.Framework.Rendering;

public readonly record struct PanoramaView(double X, double Y, double Width, double Height, double Scale);

// Presentation-only horizontal travel and viewport framing.
public sealed class PanoramaCamera
{
    public static readonly TimeSpan TransitionDuration = TimeSpan.FromSeconds(1.1);
    private double start;
    private double target;
    private TimeSpan elapsed;
    public double Position { get; private set; }
    public bool IsMoving => Position != target;

    public void NavigateToMainMenu() => Navigate(0);
    public void NavigateToOptions() => Navigate(1);
    public void NavigateToCharacterSelection() => Navigate(-1);

    private void Navigate(double destination)
    {
        if (target == destination) return; // Repeated navigation must not restart the clock.
        start = Position;
        target = destination;
        elapsed = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        if (!IsMoving) return;
        elapsed = delta >= TransitionDuration - elapsed ? TransitionDuration : elapsed + delta;
        double t = elapsed.TotalSeconds / TransitionDuration.TotalSeconds;
        double eased = t * t * (3 - 2 * t);
        Position = elapsed == TransitionDuration ? target : Math.Clamp(start + (target - start) * eased, -1, 1);
    }

    public PanoramaView GetView(int imageWidth, int imageHeight, int viewportWidth, int viewportHeight,
        int topExtension = 0, double descent = 1, int leftExtension = 0)
    {
        if (imageWidth <= 0 || imageHeight <= 0 || viewportWidth <= 0 || viewportHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Image and viewport dimensions must be positive.");
        if (topExtension < 0 || topExtension >= imageHeight) throw new ArgumentOutOfRangeException(nameof(topExtension));
        if (leftExtension < 0 || leftExtension >= imageWidth) throw new ArgumentOutOfRangeException(nameof(leftExtension));
        if (!(descent >= 0 && descent <= 1)) throw new ArgumentOutOfRangeException(nameof(descent));

        // Frame the town band independently; extensions add travel without zooming the town out.
        int framedHeight = imageHeight - topExtension;
        int framedWidth = imageWidth - leftExtension;
        double scale = Math.Max((double)viewportWidth / framedWidth, (double)viewportHeight / framedHeight);
        double width = Math.Min(framedWidth, viewportWidth / scale);
        double height = Math.Min(framedHeight, viewportHeight / scale);
        double mainX = leftExtension;
        double travel = Position < 0 ? leftExtension : Math.Max(0, framedWidth - width);
        double settledY = topExtension + (framedHeight - height) / 2;
        return new(Math.Clamp(mainX + Position * travel, 0, imageWidth - width), settledY * descent, width, height, scale);
    }
}
