using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PawnShop.Framework.Rendering;

// Owns one unmodified source image and draws it uniformly scaled, clipped by the viewport.
public sealed class PanoramicBackground : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly Texture2D texture;
    private readonly SpriteBatch batch;

    public PanoramicBackground(GraphicsDevice device, string path)
    {
        this.device = device;
        using var stream = File.OpenRead(path);
        texture = Texture2D.FromStream(device, stream);
        try { batch = new SpriteBatch(device); }
        catch { texture.Dispose(); throw; }
    }

    public void Draw(PanoramaCamera camera, int topExtension = 0, double descent = 1, int leftExtension = 0)
    {
        device.SetRenderTarget(null);
        var viewport = device.Viewport;
        var view = camera.GetView(texture.Width, texture.Height, viewport.Width, viewport.Height, topExtension, descent, leftExtension);
        device.Clear(Color.Black);
        batch.Begin(blendState: BlendState.Opaque, samplerState: SamplerState.PointClamp);
        // One uniform scale avoids rounding a source rectangle into a stretched image.
        batch.Draw(texture, new Vector2((float)(-view.X * view.Scale), (float)(-view.Y * view.Scale)),
            null, Color.White, 0, Vector2.Zero, (float)view.Scale, SpriteEffects.None, 0);
        batch.End();
    }

    public void Dispose()
    {
        batch.Dispose();
        texture.Dispose();
    }
}
