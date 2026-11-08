using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Characters.Npcs.Common;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Screens;

// Room-owned graphics for a resolved pose. All file I/O and body occlusion happen
// at load, not Draw. Original textures/art are never changed in place.
internal sealed class CommonNpcSpriteRenderer : IDisposable
{
    private readonly IReadOnlyList<CommonNpcFrame> frames;
    private readonly Dictionary<string, Texture2D> textures = new(StringComparer.Ordinal);
    private readonly List<(Texture2D Texture, Rectangle Source)>[] layers;
    private readonly List<Texture2D> maskedBodies = new();

    public CommonNpcSpriteRenderer(GraphicsDevice device, string assetsDirectory, IReadOnlyList<CommonNpcFrame> frames)
    {
        this.frames = frames;
        layers = new List<(Texture2D, Rectangle)>[frames.Count];
        try
        {
            for (int index = 0; index < frames.Count; index++)
            {
                var frame = frames[index];
                var poseLayers = layers[index] = new();
                foreach (var layer in frame.Layers)
                {
                    if (!textures.TryGetValue(layer.Texture, out var texture))
                    {
                        using var stream = File.OpenRead(Path.Combine(assetsDirectory, layer.Texture));
                        texture = Texture2D.FromStream(device, stream);
                        textures.Add(layer.Texture, texture);
                    }
                    var source = new Rectangle(layer.X, layer.Y, layer.Width, layer.Height);
                    if (layer.PartId == "body" && frame.HiddenBodyPixels.Count > 0)
                    {
                        var pixels = new Color[frame.Width * frame.Height];
                        texture.GetData(0, source, pixels, 0, pixels.Length);
                        foreach (var pixel in frame.HiddenBodyPixels) pixels[pixel.Y * frame.Width + pixel.X] = Color.Transparent;
                        var maskedBody = new Texture2D(device, frame.Width, frame.Height);
                        maskedBodies.Add(maskedBody);
                        maskedBody.SetData(pixels);
                        poseLayers.Add((maskedBody, new(0, 0, frame.Width, frame.Height)));
                    }
                    else poseLayers.Add((texture, source));
                }
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(PixelRenderer renderer, int x, int y, int frameIndex)
    {
        var frame = frames[frameIndex];
        int pivotX = frame.MirrorHorizontally ? frame.Width - frame.PivotX : frame.PivotX;
        var destination = new Rectangle(x - pivotX, y - frame.PivotY, frame.Width, frame.Height);
        var effects = frame.MirrorHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        foreach (var (texture, source) in layers[frameIndex]) renderer.DrawTexture(texture, destination, source, effects);
    }

    public void Dispose()
    {
        foreach (var maskedBody in maskedBodies) maskedBody.Dispose();
        maskedBodies.Clear();
        foreach (var texture in textures.Values) texture.Dispose();
        textures.Clear();
        foreach (var pose in layers) pose?.Clear();
    }
}
