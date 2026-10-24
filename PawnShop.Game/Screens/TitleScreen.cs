using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;

namespace PawnShop.Game.Screens;

// Application-owned artwork: returning to title reuses it without creating a session.
internal sealed class TitleScreen : IDisposable
{
    private readonly PixelRenderer renderer;
    private readonly List<Texture2D> textures = [];
    private readonly Texture2D background;
    private readonly Texture2D shop;
    private readonly Texture2D salvage;
    private readonly Texture2D logo;

    public TitleScreen(GraphicsDevice device)
    {
        renderer = new PixelRenderer(device, 1920, 1080, integerScaling: false);
        try
        {
            background = Load(device, "background-town.png", 1672, 941);
            shop = Load(device, "pawnshop.png", 1396, 1127);
            salvage = Load(device, "foreground-salvage.png", 2172, 724);
            logo = Load(device, "logo-rummage-hollow.png", 1774, 887);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private Texture2D Load(GraphicsDevice device, string name, int width, int height)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", name);
        using var stream = File.OpenRead(path);
        // PNGs use straight alpha. Convert once for SpriteBatch's premultiplied AlphaBlend.
        var texture = Texture2D.FromStream(device, stream, pixels =>
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                int alpha = pixels[i + 3];
                pixels[i] = (byte)(pixels[i] * alpha / 255);
                pixels[i + 1] = (byte)(pixels[i + 1] * alpha / 255);
                pixels[i + 2] = (byte)(pixels[i + 2] * alpha / 255);
            }
        });
        textures.Add(texture);
        if (texture.Width != width || texture.Height != height)
            throw new InvalidDataException($"Title asset '{name}' must be {width}x{height}; got {texture.Width}x{texture.Height}.");
        return texture;
    }

    public void Draw(Menu<MenuCommand> menu)
    {
        renderer.Begin(new Color(45, 33, 52));
        renderer.DrawTexture(background, Bounds(0, 0, 1, 1));
        //renderer.DrawTexture(shop, Bounds(0.552083f, 0.444444f, 0.375f, 0.537963f));
        //renderer.DrawTexture(salvage, Bounds(0.135417f, 0.675926f, 0.729167f, 0.432407f));
        renderer.DrawTexture(logo, Bounds(0.260417f, 0.037037f, 0.479167f, 0.425926f));

        for (int i = 0; i < menu.Items.Count; i++)
        {
            var item = menu.Items[i];
            float y = 0.572222f + i * 0.076852f;
            bool selected = i == menu.SelectedIndex;
            if (selected)
            {
                // A steady diamond keeps keyboard focus visible even on unavailable entries.
                int radius = Y(0.011111f);
                for (int row = -radius; row <= radius; row++)
                {
                    int halfWidth = radius - Math.Abs(row);
                    renderer.Fill(X(0.153125f) - halfWidth, Y(y + 0.021296f) + row,
                        halfWidth * 2 + 1, 1, new Color(255, 209, 108));
                }
            }
            var color = !item.Enabled ? new Color(181, 174, 158)
                : selected ? new Color(255, 246, 222) : new Color(255, 240, 193);
            Text(item.Label, 0.16875f, y, color, 0.04537f);
            if (!item.Enabled) Text("UNAVAILABLE", 0.359375f, y + 0.014815f,
                new Color(221, 198, 162), 0.019444f);
        }

        // Keep help readable against the foreground without obscuring the composition.
        var helpBounds = Bounds(0, 0.905556f, 1, 0.094444f);
        renderer.Fill(helpBounds.X, helpBounds.Y, helpBounds.Width, helpBounds.Height, Color.Black * 0.6f);
        Text(menu.Selected.DisabledReason ?? "Every treasure deserves a second chance.",
            0.15625f, 0.922222f, new Color(255, 240, 193), 0.019444f);
        Text("UP/DOWN: SELECT   ENTER/SPACE: CONFIRM", 0.15625f, 0.962963f,
            new Color(221, 198, 162), 0.019444f);
        renderer.Present();
    }

    private int X(float fraction) => (int)MathF.Round(renderer.Width * fraction);
    private int Y(float fraction) => (int)MathF.Round(renderer.Height * fraction);

    private Rectangle Bounds(float left, float top, float width, float height) =>
        new(X(left), Y(top), X(width), Y(height));

    private void Text(string text, float left, float top, Color color, float height)
    {
        int x = X(left), y = Y(top);
        // The existing font has seven pixel rows; retain an integer grid within the canvas.
        int scale = Math.Max(1, (int)MathF.Round(renderer.Height * height / 7));
        renderer.Text(text, x + X(0.001563f), y + Y(0.003704f), new Color(52, 33, 50), scale);
        renderer.Text(text, x, y, color, scale);
    }

    public void Dispose()
    {
        foreach (var texture in textures) texture.Dispose();
        textures.Clear();
        renderer.Dispose();
    }
}
