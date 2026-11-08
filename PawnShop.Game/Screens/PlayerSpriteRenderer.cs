using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

// Application-owned textures shared by character selection and the session room.
internal sealed class PlayerSpriteRenderer : IDisposable
{
    private readonly PlayerSpriteCatalog catalog;
    private readonly Dictionary<PlayerGender, Texture2D> textures = new();
    private readonly Dictionary<(PlayerGender, PlayerPlaceholderLayer), Texture2D> placeholders = new();

    public PlayerSpriteRenderer(GraphicsDevice device, PlayerSpriteCatalog catalog)
    {
        this.catalog = catalog;
        try
        {
            foreach (var (gender, path) in catalog.Textures)
            {
                using var stream = File.OpenRead(path);
                textures.Add(gender, Texture2D.FromStream(device, stream));
                foreach (var layer in Enum.GetValues<PlayerPlaceholderLayer>())
                {
                    var texture = new Texture2D(device, catalog.PlaceholderSheet.ImageWidth, catalog.PlaceholderSheet.ImageHeight);
                    placeholders.Add((gender, layer), texture);
                    texture.SetData(PlayerPlaceholderSprites.CreatePixels(gender, layer, catalog.PlaceholderSheet));
                }
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(PixelRenderer renderer, PlayerGender gender, Facing facing, float x, float y, float scale = 1)
    {
        var destination = PlayerSpriteCatalog.Bounds(catalog.Sheet, facing, x, y, scale);
        renderer.DrawTexture(textures[gender], destination, catalog.Frame(facing), PlayerSpriteCatalog.Effects(facing));
    }

    public void DrawPlaceholder(PixelRenderer renderer, PlayerView player, int phase)
    {
        var destination = PlayerSpriteCatalog.Bounds(catalog.PlaceholderSheet, player.Facing, player.Position.X, player.Position.Y);
        var source = PlayerPlaceholderSprites.Frame(catalog.PlaceholderSheet, player.Facing, phase);
        var effects = PlayerSpriteCatalog.Effects(player.Facing);
        foreach (var layer in PlayerPlaceholderSprites.DrawOrder(player.Facing))
            renderer.DrawTexture(placeholders[(player.Gender, layer)], destination, source, effects);
    }

    public void Dispose()
    {
        foreach (var texture in textures.Values) texture.Dispose();
        textures.Clear();
        foreach (var texture in placeholders.Values) texture.Dispose();
        placeholders.Clear();
    }
}
