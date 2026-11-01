using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PawnShop.Framework.Rendering;

// A single application-owned content scope shared by renderers, not one font per screen.
public sealed class FontLibrary : IDisposable
{
    private readonly ContentManager content;
    private readonly FontCatalog catalog;
    private readonly Dictionary<string, SpriteFont> assets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<char, SpriteFont.Glyph>> glyphs = new(StringComparer.Ordinal);

    public FontLibrary(IServiceProvider services, string contentRoot, FontCatalog catalog)
    {
        this.catalog = catalog;
        content = new ContentManager(services, contentRoot);
        try
        {
            foreach (var (name, definition) in catalog.Fonts)
            {
                if (assets.ContainsKey(definition.Asset)) continue;
                try
                {
                    var font = content.Load<SpriteFont>(definition.Asset);
                    assets.Add(definition.Asset, font);
                    glyphs.Add(definition.Asset, font.GetGlyphs());
                }
                catch (ContentLoadException e)
                {
                    throw new InvalidDataException($"fonts.{name}.asset '{definition.Asset}': could not load the compiled SpriteFont.", e);
                }
            }
        }
        catch { content.Dispose(); throw; }
    }

    public void Draw(SpriteBatch batch, FontText text, Vector2 position, Color color, float uiScale)
    {
        var definition = catalog.Get(text.FontName);
        var font = assets[definition.Asset];
        batch.DrawString(font, text.Value, position, color, 0, Vector2.Zero,
            definition.Size / font.LineSpacing * uiScale, SpriteEffects.None, 0);
    }

    public Vector2 Measure(FontText text, float uiScale)
    {
        var definition = catalog.Get(text.FontName);
        var font = assets[definition.Asset];
        return font.MeasureString(text.Value) * (definition.Size / font.LineSpacing * uiScale);
    }

    // Align adornments to the drawn glyphs, excluding font leading/line padding.
    public float MeasureVerticalCenter(FontText text, float uiScale)
    {
        var definition = catalog.Get(text.FontName);
        var font = assets[definition.Asset];
        var characters = glyphs[definition.Asset];
        float top = float.PositiveInfinity, bottom = float.NegativeInfinity;
        int lineY = 0;
        foreach (char character in text.Value)
        {
            if (character == '\n') { lineY += font.LineSpacing; continue; }
            if (char.IsWhiteSpace(character)) continue;
            if (!characters.TryGetValue(character, out var glyph))
            {
                if (font.DefaultCharacter is not { } fallback) continue;
                glyph = characters[fallback];
            }
            top = Math.Min(top, lineY + glyph.Cropping.Y);
            bottom = Math.Max(bottom, lineY + glyph.Cropping.Y + glyph.BoundsInTexture.Height);
        }
        return float.IsPositiveInfinity(top) ? 0
            : (top + bottom) / 2 * (definition.Size / font.LineSpacing * uiScale);
    }

    public void Dispose() => content.Dispose();
}
