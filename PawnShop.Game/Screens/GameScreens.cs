using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class GameScreens(GraphicsDevice device, PanoramaCamera camera, FontLibrary fonts, SpriteSheetDefinition titleSheet) : IDisposable
{
    private readonly TitleScreen title = new(device, camera, fonts, titleSheet);
    private readonly OptionsScreen options = new(device, fonts);
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
        if (flow.UsesTitleBackground)
        {
            if (!flow.TitleUiVisible) { title.DrawBackground(); return; }
            if (flow.Screen == Screen.Options) options.Draw(flow.Options, title.DrawBackground);
            else if (flow.Menu is { } titleMenu) title.Draw(titleMenu, flow.Options.Current.UiScale / 100f, flow.TitleSelector);
            return;
        }
        if (flow.Screen == Screen.Options)
        {
            options.Draw(flow.Options);
            return;
        }
        renderer.TextScale = flow.Options.Current.UiScale / 100f;
        renderer.Begin(new Color(26, 36, 38));
        if (flow.Room is { } view && room is not null) room.Draw(renderer, view, flow.Options.Current);
        if (flow.Menu is { } menu) MenuScreen.Draw(renderer, flow, menu);
        renderer.Text("M01 PROTOTYPE - NO SAVING", 18, 341, new Color(173, 184, 172));
        renderer.Present();
    }

    public void Dispose()
    {
        room?.Dispose();
        room = null;
        title.Dispose();
        options.Dispose();
    }
}
