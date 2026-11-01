using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PawnShop.Framework.Rendering;

// Owns application-wide drawing resources; screens own their location textures.
public sealed class PixelRenderer : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly SpriteBatch batch;
    private readonly Texture2D pixel;
    private readonly RenderTarget2D canvas;
    private readonly PixelFont font;
    private readonly bool integerScaling;
    private readonly FontLibrary? fonts;
    public int Width { get; }
    public int Height { get; }
    public float TextScale { get; set; } = 1;

    public PixelRenderer(GraphicsDevice device, int width, int height, bool integerScaling = true, FontLibrary? fonts = null)
    {
        this.fonts = fonts;
        this.device = device;
        Width = width;
        Height = height;
        this.integerScaling = integerScaling;
        batch = new SpriteBatch(device);
        pixel = new Texture2D(device, 1, 1);
        pixel.SetData(new[] { Color.White });
        canvas = new RenderTarget2D(device, width, height);
        font = new PixelFont(pixel);
    }

    public void Begin(Color background)
    {
        device.SetRenderTarget(canvas);
        device.Clear(background);
        batch.Begin(samplerState: SamplerState.PointClamp);
    }

    public void Present(bool clearBackBuffer = true, Action? drawBackground = null)
    {
        batch.End();
        device.SetRenderTarget(null);
        if (clearBackBuffer) device.Clear(Color.Black);
        // Composite only after returning from the UI target; never rely on backbuffer
        // preservation across render-target switches (DiscardContents is the default).
        drawBackground?.Invoke();
        var destination = GetPresentationBounds();
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(canvas, destination, Color.White);
        batch.End();
    }

    // Call after Present: scale small pixel sprites once, directly from their source
    // sheet to the backbuffer, while preserving the canvas's contained UI coordinates.
    public void DrawPixelOverlay(Texture2D texture, Rectangle source, Vector2 anchor, Vector2 pivot, float logicalScale)
    {
        var destination = GetPresentationBounds();
        float outputScale = (float)destination.Width / Width;
        var position = new Vector2(destination.X, destination.Y) + (anchor - pivot * logicalScale) * outputScale;
        position = new Vector2(MathF.Round(position.X), MathF.Round(position.Y));
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(texture, position, source, Color.White, 0, Vector2.Zero,
            logicalScale * outputScale, SpriteEffects.None, 0);
        batch.End();
    }

    private Rectangle GetPresentationBounds()
    {
        var bounds = device.PresentationParameters;
        float scale = Math.Min((float)bounds.BackBufferWidth / Width, (float)bounds.BackBufferHeight / Height);
        // Detailed title illustrations can shrink to fit; gameplay retains an integer pixel grid.
        if (integerScaling && scale >= 1) scale = MathF.Floor(scale);
        int width = (int)(Width * scale), height = (int)(Height * scale);
        return new Rectangle((bounds.BackBufferWidth - width) / 2,
            (bounds.BackBufferHeight - height) / 2, width, height);
    }

    public void Fill(int x, int y, int width, int height, Color color) =>
        batch.Draw(pixel, new Rectangle(x, y, width, height), color);

    public void Text(string text, int x, int y, Color color, int scale = 1) =>
        font.Draw(batch, text, x, y, color, scale * TextScale);

    public void Text(FontText text, int x, int y, Color color) =>
        RequiredFonts.Draw(batch, text, new Vector2(x, y), color, TextScale);

    public Vector2 MeasureText(FontText text) => RequiredFonts.Measure(text, TextScale);

    public float MeasureTextVerticalCenter(FontText text) => RequiredFonts.MeasureVerticalCenter(text, TextScale);

    private FontLibrary RequiredFonts => fonts ?? throw new InvalidOperationException("This renderer needs a FontLibrary to draw named fonts.");

    public void DrawTexture(Texture2D texture, int x, int y) =>
        batch.Draw(texture, new Vector2(x, y), Color.White);

    public void DrawTexture(Texture2D texture, Rectangle destination) =>
        batch.Draw(texture, destination, Color.White);

    public void DrawTexture(Texture2D texture, Rectangle destination, Rectangle source) =>
        batch.Draw(texture, destination, source, Color.White);

    public void Dispose()
    {
        canvas.Dispose();
        pixel.Dispose();
        batch.Dispose();
    }
}
