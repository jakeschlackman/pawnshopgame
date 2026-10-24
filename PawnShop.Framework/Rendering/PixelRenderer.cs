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
    public int Width { get; }
    public int Height { get; }

    public PixelRenderer(GraphicsDevice device, int width, int height)
    {
        this.device = device;
        Width = width;
        Height = height;
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

    public void Present()
    {
        batch.End();
        device.SetRenderTarget(null);
        device.Clear(Color.Black);
        var bounds = device.PresentationParameters;
        int scale = Math.Max(1, Math.Min(bounds.BackBufferWidth / Width, bounds.BackBufferHeight / Height));
        var destination = new Rectangle((bounds.BackBufferWidth - Width * scale) / 2,
            (bounds.BackBufferHeight - Height * scale) / 2, Width * scale, Height * scale);
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(canvas, destination, Color.White);
        batch.End();
    }

    public void Fill(int x, int y, int width, int height, Color color) =>
        batch.Draw(pixel, new Rectangle(x, y, width, height), color);

    public void Text(string text, int x, int y, Color color, int scale = 1) =>
        font.Draw(batch, text, x, y, color, scale);

    public void DrawTexture(Texture2D texture, int x, int y) =>
        batch.Draw(texture, new Vector2(x, y), Color.White);

    public void Dispose()
    {
        canvas.Dispose();
        pixel.Dispose();
        batch.Dispose();
    }
}
