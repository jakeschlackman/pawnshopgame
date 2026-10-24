using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

// A location graphics scope, distinct from session state and application-wide UI assets.
internal sealed class RoomScreen : IDisposable
{
    public Guid SessionId { get; }
    public Texture2D Floor { get; }

    public RoomScreen(GraphicsDevice device, Guid sessionId)
    {
        SessionId = sessionId;
        Floor = new Texture2D(device, 32, 32);
        var pixels = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                pixels[y * 32 + x] = y % 16 == 0 || x == (y < 16 ? 0 : 16)
                    ? new Color(67, 50, 44) : new Color(101, 77, 56);
        Floor.SetData(pixels);
    }

    public void Draw(PixelRenderer renderer, RoomView view)
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
        renderer.Text("SPACE: RING BELL    ESC: PAUSE", 36, 315, Cream);
    }

    public void Dispose() => Floor.Dispose();
}
