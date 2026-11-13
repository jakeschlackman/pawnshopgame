using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Characters.Player.Create;
using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class GameScreens : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly TitleScreen title;
    private readonly OptionsScreen options;
    private readonly PlayerSpriteRenderer players;
    private readonly PlayerModel_SelectionScreen selection;
    private RoomScreen? room;

    public GameScreens(GraphicsDevice device, PanoramaCamera camera, Func<double> titleDescent, FontLibrary fonts,
        SpriteSheetDefinition titleSheet, PlayerSpriteCatalog playerCatalog, CharacterSelectionCatalog selectionCatalog)
    {
        this.device = device;
        try
        {
            title = new(device, camera, titleDescent, fonts, titleSheet);
            options = new(device, fonts);
            players = new(device, playerCatalog);
            selection = new(device, selectionCatalog);
        }
        catch
        {
            players?.Dispose();
            selection?.Dispose();
            options?.Dispose();
            title?.Dispose();
            throw;
        }
    }

    public void SynchronizeRoom(RoomView? view, IReadOnlyList<PlacedNpc> npcs, CommonNpcRoomVisitor? visitor)
    {
        Guid? id = view?.SessionId;
        if (room?.SessionId == id) return;
        room?.Dispose();
        room = null;
        if (id is { } sessionId) room = new RoomScreen(device, sessionId, npcs, players, visitor);
    }

    public void Draw(PixelRenderer renderer, ApplicationFlow flow)
    {
        if (flow.UsesTitleBackground)
        {
            if (flow.OptionsFlight.IsBusy)
            {
                if (flow.Screen == Screen.Options) options.Draw(flow.Options, title.DrawBackground);
                else if (flow.Menu is { } departureMenu)
                    title.Draw(departureMenu, flow.Options.Current.UiScale / 100f, flow.TitleSelector, flow.TitleSaying, drawSelector: false);
                DrawOptionsFlight(flow);
                return;
            }
            if (flow.Screen == Screen.CharacterSelect && flow.Menu is { } selectionMenu)
            {
                renderer.TextScale = flow.Options.Current.UiScale / 100f;
                renderer.Begin(Color.Transparent);
                selection.Draw(renderer, selectionMenu, flow.CharacterSelection, flow.TitleCamera.Position,
                    showUi: !flow.CharacterSelectionFlying);
                renderer.Present(clearBackBuffer: false, drawBackground: title.DrawBackground);
                var departure = title.SelectorAnchor(renderer, 0, "New Game", renderer.TextScale);
                var anchor = flow.CharacterSelectionFlying
                    ? CharacterSelectionLayout.FlightAnchor(departure, flow.TitleCamera.Position, renderer.TextScale)
                    : CharacterSelectionLayout.SelectorAnchor(selectionMenu.SelectedIndex, renderer.TextScale);
                title.DrawSelector(flow.TitleSelector, anchor, target: renderer);
                return;
            }
            if (!flow.TitleUiVisible)
            {
                if (flow.StudioLogoOpacity > 0) title.DrawStudioLogo(flow.StudioLogoOpacity);
                else title.DrawBackground();
                return;
            }
            if (flow.Screen == Screen.Options)
            {
                options.Draw(flow.Options, title.DrawBackground);
                if (!flow.Options.HasModal) title.DrawSelector(flow.TitleSelector, options.SelectorAnchor(flow.Options));
            }
            else if (flow.Menu is { } titleMenu)
                title.Draw(titleMenu, flow.Options.Current.UiScale / 100f, flow.TitleSelector, flow.TitleSaying, opacity: flow.TitleMenuOpacity);
            return;
        }
        if (flow.Screen == Screen.Options)
        {
            options.Draw(flow.Options, () => DrawRoomBackground(renderer, flow));
            return;
        }
        renderer.TextScale = flow.Options.Current.UiScale / 100f;
        renderer.Begin(new Color(26, 36, 38));
        if (flow.Room is { } view && room is not null && flow.Player is { } player)
            room.Draw(renderer, view, player, flow.Options.Current, flow.SelectedNpcId, flow.NpcFrames, flow.PlayerFrame);
        if (flow.Menu is { } menu)
        {
            MenuScreen.Draw(renderer, flow, menu);
        }
        if (flow.Dialogue is { } dialogue) DialogueScreen.Draw(renderer, dialogue);
        renderer.Text("PROTOTYPE - NO SAVING", 18, 341, new Color(173, 184, 172));
        renderer.Present();
    }

    private void DrawRoomBackground(PixelRenderer renderer, ApplicationFlow flow)
    {
        renderer.TextScale = flow.Options.Current.UiScale / 100f;
        renderer.Begin(new Color(26, 36, 38));
        if (flow.Room is { } view && room is not null && flow.Player is { } player)
            room.Draw(renderer, view, player, flow.Options.Current, flow.SelectedNpcId, flow.NpcFrames, flow.PlayerFrame);
        renderer.Present();
    }

    private void DrawOptionsFlight(ApplicationFlow flow)
    {
        var flight = flow.OptionsFlight;
        var menuAnchor = title.SelectorAnchor(2, "Options", flow.Options.Current.UiScale / 100f);
        var optionAnchor = options.SelectorAnchor(flow.Options, first: true);
        title.DrawSelector(flow.TitleSelector, flight.GetAnchor(menuAnchor, optionAnchor));
    }

    public void Dispose()
    {
        room?.Dispose();
        room = null;
        title.Dispose();
        options.Dispose();
        players.Dispose();
        selection.Dispose();
    }
}
