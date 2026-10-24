using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class GameScreens(GraphicsDevice device) : IDisposable
{
    private RoomScreen? room;

    public void SynchronizeRoom(RoomView? view)
    {
        Guid? id = view?.SessionId;
        if (room?.SessionId == id) return;
        room?.Dispose();
        room = null;
        if (id is { } sessionId) room = new RoomScreen(device, sessionId);
    }

    public void Draw(PixelRenderer renderer, ApplicationFlow flow)
    {
        if (flow.Room is { } view && room is not null) room.Draw(renderer, view);
        else TitleScreen.Draw(renderer);
        if (flow.Menu is { } menu) MenuScreen.Draw(renderer, flow, menu);
        renderer.Text("M01 PROTOTYPE - NO SAVING", 18, 341, new Color(173, 184, 172));
    }

    public void Dispose()
    {
        room?.Dispose();
        room = null;
    }
}
