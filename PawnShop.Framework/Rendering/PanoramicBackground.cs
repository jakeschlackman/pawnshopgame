using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PawnShop.Framework.Rendering;

// Owns one unmodified source image and draws it uniformly scaled, clipped by the viewport.
public sealed class PanoramicBackground : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly Texture2D texture;
    private readonly SpriteBatch batch;
    private readonly RasterizerState clipping = new() { ScissorTestEnable = true };

    public PanoramicBackground(GraphicsDevice device, string path)
    {
        this.device = device;
        using var stream = File.OpenRead(path);
        texture = Texture2D.FromStream(device, stream);
        try { batch = new SpriteBatch(device); }
        catch { texture.Dispose(); throw; }
    }

    public void Draw(PanoramaCamera camera)
    {
        device.SetRenderTarget(null);
        var viewport = device.Viewport;
        var view = camera.GetView(texture.Width, texture.Height, viewport.Width, viewport.Height);
        Draw(view, viewport.Bounds);
    }

    // An explicit source-space view also supports an authoring canvas around the
    // player's crop, including empty space beyond the original image edges.
    public void Draw(PanoramaView view, Rectangle viewport)
    {
        device.SetRenderTarget(null);
        device.Clear(new Color(22, 27, 34));
        var previousScissor = device.ScissorRectangle;
        device.ScissorRectangle = viewport;
        batch.Begin(blendState: BlendState.Opaque, samplerState: SamplerState.PointClamp, rasterizerState: clipping);
        // One uniform scale avoids rounding a source rectangle into a stretched image.
        batch.Draw(texture, new Vector2((float)(viewport.X - view.X * view.Scale), (float)(viewport.Y - view.Y * view.Scale)),
            null, Color.White, 0, Vector2.Zero, (float)view.Scale, SpriteEffects.None, 0);
        batch.End();
        device.ScissorRectangle = previousScissor;
    }

    public void Dispose()
    {
        clipping.Dispose();
        batch.Dispose();
        texture.Dispose();
    }
}
