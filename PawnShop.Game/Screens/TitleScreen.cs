using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;

namespace PawnShop.Game.Screens;

// Application-owned artwork: returning to title reuses it without creating a session.
internal sealed class TitleScreen : IDisposable
{
    private readonly PixelRenderer renderer;
    private readonly PanoramicBackground background;
    private readonly Texture2D logo;
    private readonly Texture2D sprout;
    public Action DrawBackground { get; }

    public TitleScreen(GraphicsDevice device, PanoramaCamera camera, FontLibrary fonts, SpriteSheetDefinition titleSheet)
    {
        renderer = new PixelRenderer(device, 1920, 1080, integerScaling: false, fonts: fonts);
        try
        {
            background = new PanoramicBackground(device,
                Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "title-screen-panorama.png"));
            logo = LoadTexture(device, "logo-pickers-hollow.png", 1774, 887);
            sprout = LoadTexture(device, "sprout-title-menu.png", titleSheet.ImageWidth, titleSheet.ImageHeight);
            DrawBackground = () => background.Draw(camera);
        }
        catch
        {
            logo?.Dispose();
            sprout?.Dispose();
            background?.Dispose();
            renderer.Dispose();
            throw;
        }
    }

    public void Draw(Menu<MenuCommand> menu, float textScale, SpriteAnimator selector, bool drawSelector = true)
    {
        renderer.TextScale = textScale;
        renderer.Begin(Color.Transparent);
        renderer.DrawTexture(logo, Bounds(0.260417f, 0.037037f, 0.479167f, 0.425926f));

        for (int i = 0; i < menu.Items.Count; i++)
        {
            var item = menu.Items[i];
            float y = TitleMenuLayout.FirstRow + i * TitleMenuLayout.RowSpacing;
            bool selected = i == menu.SelectedIndex;
            var color = !item.Enabled ? new Color(181, 174, 158)
                : selected ? new Color(255, 246, 222) : new Color(255, 240, 193);
            Text(item.Label, 0.16875f, y, color, "TitleMenu");
            if (!item.Enabled) Text("UNAVAILABLE", 0.52f, y + 0.014815f,
                new Color(221, 198, 162), "TitleBadge");
        }

        // Keep help readable against the foreground without obscuring the composition.
        var helpBounds = Bounds(0, 0.905556f, 1, 0.094444f);
        renderer.Fill(helpBounds.X, helpBounds.Y, helpBounds.Width, helpBounds.Height, Color.Black * 0.6f);
        Text(menu.Selected.DisabledReason ?? "Every treasure deserves a second chance.",
            0.15625f, 0.922222f, new Color(255, 240, 193), "TitleHelp");
        Text("CLICK OR UP/DOWN: SELECT   ENTER: CONFIRM", 0.15625f, 0.962963f,
            new Color(221, 198, 162), "TitleHelp");
        renderer.Present(clearBackBuffer: false, drawBackground: DrawBackground);
        if (drawSelector) DrawSelector(selector, SelectorAnchor(menu.SelectedIndex, menu.Selected.Label, textScale));
    }

    public Vector2 SelectorAnchor(int index, string label, float textScale)
    {
        renderer.TextScale = textScale;
        float y = TitleMenuLayout.FirstRow + index * TitleMenuLayout.RowSpacing;
        return new Vector2(X(0.145833f), Y(y) + renderer.MeasureTextVerticalCenter(label.Font("TitleMenu")) + 9);
    }

    public void DrawSelector(SpriteAnimator selector, Vector2 anchor, float rotation = 0)
    {
        var sheet = selector.Definition;
        // Keep the stable authored pivot, and avoid resampling the sprite through two targets.
        renderer.DrawPixelOverlay(sprout, sheet.GetFrame(selector.FrameIndex),
            anchor, sheet.Pivot, sheet.DisplayScale, rotation);
    }

    private int X(float fraction) => (int)MathF.Round(renderer.Width * fraction);
    private int Y(float fraction) => (int)MathF.Round(renderer.Height * fraction);

    private Rectangle Bounds(float left, float top, float width, float height) =>
        new(X(left), Y(top), X(width), Y(height));

    private void Text(string text, float left, float top, Color color, string fontName)
    {
        int x = X(left), y = Y(top);
        var label = text.Font(fontName);
        renderer.Text(label, x + X(0.001563f), y + Y(0.003704f), new Color(52, 33, 50));
        renderer.Text(label, x, y, color);
    }

    private static Texture2D LoadTexture(GraphicsDevice device, string name, int width, int height)
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", name));
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
        if (texture.Width == width && texture.Height == height) return texture;
        string diagnostic = $"Title asset '{name}' must be {width}x{height}; got {texture.Width}x{texture.Height}.";
        texture.Dispose();
        throw new InvalidDataException(diagnostic);
    }

    public void Dispose()
    {
        logo.Dispose();
        sprout.Dispose();
        background.Dispose();
        renderer.Dispose();
    }
}
