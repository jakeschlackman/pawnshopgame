using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Core.World;
using PawnShop.Game.Bootstrap;
using PawnShop.Framework.Rendering;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

// A location graphics scope, distinct from session state and application-wide UI assets.
internal sealed class RoomScreen : IDisposable
{
    public Guid SessionId { get; }
    public Texture2D Floor { get; }

    private readonly Dictionary<string, Texture2D> textures = new();

    public RoomScreen(GraphicsDevice device, Guid sessionId, IReadOnlyList<PlacedNpc> npcs)
    {
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

    public void Draw(PixelRenderer renderer, RoomView view, AppSettings settings, IReadOnlyList<PlacedNpc> npcs, string? selectedNpcId, IReadOnlyDictionary<string, int> frames)
    {
        for (int y = 80; y < 304; y += 32)
            for (int x = 32; x < 608; x += 32)
                renderer.DrawTexture(Floor, x, y);
        renderer.Fill(32, 44, 576, 36, new Color(90, 111, 99));
        renderer.Fill(68, 104, 190, 48, new Color(56, 40, 35));
        renderer.Fill(68, 104, 190, 9, Gold);
        renderer.Text("COUNTER - BELL", 85, 124, Cream);
        renderer.Fill(315, 184, 20, 26, new Color(45, 66, 78));
        renderer.Fill(317, 176, 16, 14, new Color(230, 183, 139));
        renderer.Fill(317, 210, 6, 5, Color.Black);
        renderer.Fill(329, 210, 6, 5, Color.Black);
        renderer.Text("YOUR SHOP", 44, 58, Cream, 2);
        renderer.Text($"ROOM TIME: {(int)view.Elapsed.TotalSeconds} SECONDS", 36, 16, Cream);
        renderer.Text($"BELL RINGS: {view.BellRings}", 410, 16, Gold);
        renderer.Text($"ARROWS: NPC  TAB: TALK  {settings.InteractKey}: BELL  {settings.PauseKey}: PAUSE", 36, 315, Cream);
        foreach (var npc in npcs)
        {
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
    }

    public void Dispose()
    {
        foreach (var texture in textures.Values) texture.Dispose();
        textures.Clear();
        Floor.Dispose();
    }
}
