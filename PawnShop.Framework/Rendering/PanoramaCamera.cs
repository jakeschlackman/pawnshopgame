namespace PawnShop.Framework.Rendering;

public readonly record struct PanoramaView(double X, double Y, double Width, double Height, double Scale);

// Presentation-only horizontal travel and viewport framing.
public sealed class PanoramaCamera
{
    public static readonly TimeSpan TransitionDuration = TimeSpan.FromSeconds(0.8);
    private double start;
    private double target;
    private TimeSpan elapsed;
    public double Position { get; private set; }
    public bool IsMoving => Position != target;

    public void NavigateToMainMenu() => Navigate(0);
    public void NavigateToOptions() => Navigate(1);

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
        Position = elapsed == TransitionDuration ? target : Math.Clamp(start + (target - start) * eased, 0, 1);
    }

    public PanoramaView GetView(int imageWidth, int imageHeight, int viewportWidth, int viewportHeight,
        int topExtension = 0, double descent = 1)
    {
        if (imageWidth <= 0 || imageHeight <= 0 || viewportWidth <= 0 || viewportHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Image and viewport dimensions must be positive.");
        if (topExtension < 0 || topExtension >= imageHeight) throw new ArgumentOutOfRangeException(nameof(topExtension));
        if (!(descent >= 0 && descent <= 1)) throw new ArgumentOutOfRangeException(nameof(descent));

        // Use the original band for the cover scale and settled framing. The extension adds travel above it.
        int framedHeight = imageHeight - topExtension;
        double scale = Math.Max((double)viewportWidth / imageWidth, (double)viewportHeight / framedHeight);
        double width = Math.Min(imageWidth, viewportWidth / scale);
        double height = Math.Min(framedHeight, viewportHeight / scale);
        double maxX = Math.Max(0, imageWidth - width);
        double settledY = topExtension + (framedHeight - height) / 2;
        return new(Math.Clamp(Position * maxX, 0, maxX), settledY * descent, width, height, scale);
    }
}
