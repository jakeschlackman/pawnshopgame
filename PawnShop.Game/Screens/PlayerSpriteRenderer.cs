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

    public PlayerSpriteRenderer(GraphicsDevice device, PlayerSpriteCatalog catalog)
    {
        this.catalog = catalog;
        try
        {
            foreach (var (gender, path) in catalog.Textures)
            {
                using var stream = File.OpenRead(path);
                textures.Add(gender, Texture2D.FromStream(device, stream));
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(PixelRenderer renderer, PlayerGender gender, Facing facing, float x, float y, float scale = 1)
    {
        var sheet = catalog.Sheet;
        var destination = new Rectangle((int)MathF.Round(x - sheet.Pivot.X * scale),
            (int)MathF.Round(y - sheet.Pivot.Y * scale),
            (int)MathF.Round(sheet.FrameWidth * scale), (int)MathF.Round(sheet.FrameHeight * scale));
        renderer.DrawTexture(textures[gender], destination, catalog.Frame(facing));
    }

    public void Dispose()
    {
        foreach (var texture in textures.Values) texture.Dispose();
        textures.Clear();
    }
}
