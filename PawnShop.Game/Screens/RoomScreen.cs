using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Core.World;
using PawnShop.Game.Bootstrap;
using PawnShop.Framework.Rendering;
using PawnShop.Characters.Player;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

// A location graphics scope, distinct from session state and application-wide UI assets.
internal sealed class RoomScreen : IDisposable
{
    public Guid SessionId { get; }
    public Texture2D Floor { get; }

    private readonly Dictionary<string, Texture2D> textures = new();
    private readonly PlayerSpriteRenderer players;
    private readonly PlacedNpc[] drawOrder;

    public RoomScreen(GraphicsDevice device, Guid sessionId, IReadOnlyList<PlacedNpc> npcs, PlayerSpriteRenderer players)
    {
        this.players = players;
        drawOrder = npcs.OrderBy(n => n.Y).ThenBy(n => n.Id, StringComparer.Ordinal).ToArray();
        SessionId = sessionId;
        Floor = new Texture2D(device, 32, 32);
        var pixels = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                pixels[y * 32 + x] = y % 16 == 0 || x == (y < 16 ? 0 : 16)
                    ? new Color(67, 50, 44) : new Color(101, 77, 56);
        Floor.SetData(pixels);
        try
        {
            foreach (var npc in npcs)
            {
                string path = npc.Content.Sprite.TexturePath;
                if (textures.ContainsKey(path)) continue;
                using var stream = File.OpenRead(path);
                textures.Add(path, Texture2D.FromStream(device, stream));
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(PixelRenderer renderer, RoomView view, PlayerView player, AppSettings settings, string? selectedNpcId, IReadOnlyDictionary<string, int> frames)
    {
        var floor = ShopRoomLayout.Floor;
        for (int y = (int)floor.Top; y < floor.Bottom; y += 32)
            for (int x = (int)floor.Left; x < floor.Right; x += 32)
                renderer.DrawTexture(Floor, x, y);
        renderer.Fill(32, 44, 576, 36, new Color(90, 111, 99));
        var counter = ShopRoomLayout.Counter;
        renderer.Fill((int)counter.Left, (int)counter.Top, (int)counter.Width, (int)counter.Height, new Color(56, 40, 35));
        renderer.Fill((int)counter.Left, (int)counter.Top, (int)counter.Width, 9, Gold);
        renderer.Text("COUNTER", 85, 124, Cream);
        renderer.Text("YOUR SHOP", 44, 58, Cream, 2);
        renderer.Text($"ROOM TIME: {(int)view.Elapsed.TotalSeconds} SECONDS", 36, 16, Cream);
        renderer.Text(player.Gender == PlayerGender.Male ? "MALE" : "FEMALE", 480, 16, Gold);
        renderer.Text($"MOVE KEYS: MOVE  {settings.InteractKey}: TALK  {settings.PauseKey}: PAUSE / BACK", 36, 315, Cream);
        if (selectedNpcId is null) renderer.Text("WALK NEAR THE VISITOR TO TALK", 36, 326, Cream);
        else renderer.Text($"{settings.InteractKey}: TALK TO THE VISITOR", 36, 326, Gold);
        bool playerDrawn = false;
        foreach (var npc in drawOrder)
        {
            if (!playerDrawn && player.Position.Y <= npc.Y) { DrawPlayer(renderer, player); playerDrawn = true; }
            if (npc.Id == selectedNpcId) renderer.Fill(npc.X - 13, npc.Y + 1, 26, 3, Gold);
            var asset = npc.Content.Sprite;
            var sheet = asset.Sheet;
            var destination = new Rectangle((int)MathF.Round(npc.X - sheet.Pivot.X * sheet.DisplayScale),
                (int)MathF.Round(npc.Y - sheet.Pivot.Y * sheet.DisplayScale),
                (int)MathF.Round(sheet.FrameWidth * sheet.DisplayScale), (int)MathF.Round(sheet.FrameHeight * sheet.DisplayScale));
            renderer.DrawTexture(textures[asset.TexturePath], destination, sheet.GetFrame(frames[npc.Id]));
            int labelWidth = (int)(npc.Name.Length * 6 * renderer.TextScale);
            int labelX = Math.Clamp(npc.X - labelWidth / 2, 36, 604 - labelWidth);
            renderer.Text(npc.Name, labelX, npc.Y + 11, npc.Id == selectedNpcId ? Gold : Cream);
        }
        if (!playerDrawn) DrawPlayer(renderer, player);
    }

    private void DrawPlayer(PixelRenderer renderer, PlayerView player) =>
        players.Draw(renderer, player.Gender, player.Facing, player.Position.X, player.Position.Y);

    public void Dispose()
    {
        foreach (var texture in textures.Values) texture.Dispose();
        textures.Clear();
        Floor.Dispose();
    }
}
