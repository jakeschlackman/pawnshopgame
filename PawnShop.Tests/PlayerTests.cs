using System.Numerics;
using Microsoft.Xna.Framework.Input;
using PawnShop.Characters.Movement;
using PawnShop.Characters.Player;
using PawnShop.Characters.Player.Create;
using PawnShop.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using KeyboardInput = PawnShop.Framework.Input.KeyboardInput;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void CharacterSelection()
    {
        using var app = new ApplicationFlow();
        Confirm(app);
        Check(app.Screen == Screen.CharacterSelect && app.LiveSessions == 0 && app.Player is null);
        app.Update(new(Confirm: true, MoveX: 1, Interact: true), TimeSpan.FromSeconds(5), active: false);
        Check(app.LiveSessions == 0 && app.Screen == Screen.CharacterSelect);
        Back(app);
        Check(app.Screen == Screen.Title && app.SessionsCreated == 0);
        Confirm(app);
        app.Update(new(Right: true, Confirm: true), TimeSpan.Zero);
        Check(app.Screen == Screen.CharacterSelect && app.Menu!.SelectedIndex == 1 && app.LiveSessions == 0);
        app.Update(new(Confirm: true, Interact: true, MoveX: 1), TimeSpan.FromSeconds(1));
        Check(app.LiveSessions == 0 && app.CharacterSelection.IsConfirming);
        FinishCharacterConfirmation(app);
        Check(app.Player is { Gender: PlayerGender.Female } && app.Player.Value.Position == ShopRoomLayout.PlayerSpawn);
        Check(app.Room!.Value.Elapsed == TimeSpan.Zero && app.Dialogue is null);
        Guid player = app.Player!.Value.Id;
        ReturnPrompt(app); Discard(app);
        Confirm(app);
        var back = CharacterSelectionLayout.Back.Center;
        app.Update(MenuPointerInput.HitTest(app, back.X, back.Y, 640, 360), TimeSpan.Zero);
        Check(app.Screen == Screen.Title && app.Player is null);
        Confirm(app);
        var card = CharacterSelectionLayout.Card(0).Center;
        app.Update(MenuPointerInput.HitTest(app, card.X, card.Y, 640, 360), TimeSpan.Zero);
        FinishCharacterConfirmation(app);
        Check(app.Player is { Gender: PlayerGender.Male } && app.Player.Value.Id != player);
        Check(app.LiveSessions == 1 && app.SessionsCreated == 2 && app.SessionsDisposed == 1);
        app.Update(new(MoveX: -1), TimeSpan.FromSeconds(.5));
        var view = app.Player;
        Back(app); Confirm(app);
        Check(app.Player == view && app.Screen == Screen.Room);
        using var closing = new ApplicationFlow();
        Confirm(closing); closing.RequestExit();
        Check(closing.ExitRequested && closing.SessionsCreated == 0);
    }

    internal static void CharacterSelectionPan()
    {
        foreach (int uiScale in new[] { 100, 125, 150 })
        {
            using var app = new ApplicationFlow(new() { UiScale = uiScale });
            var newGame = TitleMenuLayout.Row(0).Center;
            app.Update(MenuPointerInput.HitTest(app, newGame.X, newGame.Y, 1920, 1080), TimeSpan.Zero);
            FinishTitleInteraction(app);
            Check(app.Screen == Screen.CharacterSelect && app.UsesTitleBackground && !app.TitleUiVisible);
            Check(app.TitleCamera.IsMoving && app.TitleCamera.Position == 0 && app.LiveSessions == 0);
            Check(!app.TitleInteractionPlaying && app.TitleSelector.AnimationName == "Idle");
            var expected = new SpriteAnimator(app.TitleSelector.Definition, "Idle");
            app.Update(new(SelectionIndex: 1, Confirm: true, MoveX: 1), PanoramaCamera.TransitionDuration / 4);
            expected.Update(PanoramaCamera.TransitionDuration / 4);
            Check(Math.Abs(app.TitleCamera.Position + .15625) < 1e-9 && app.Menu!.SelectedIndex == 0);
            Check(app.CharacterSelectionFlying && app.TitleSelector.FrameIndex == expected.FrameIndex);
            double frozen = app.TitleCamera.Position;
            app.Update(new(Cancel: true), TimeSpan.FromSeconds(10), active: false);
            Check(app.Screen == Screen.CharacterSelect && app.TitleCamera.Position == frozen
                && app.TitleSelector.FrameIndex == expected.FrameIndex);
            var female = CharacterSelectionLayout.Card(1).Center;
            Check(MenuPointerInput.HitTest(app, female.X * 4, female.Y * 4, 2560, 1440) == default);
            app.Update(new(SelectionIndex: 1, Confirm: true, MoveX: 1), PanoramaCamera.TransitionDuration * 3 / 4);
            expected.Update(PanoramaCamera.TransitionDuration * 3 / 4);
            Check(app.TitleCamera.Position == -1 && !app.TitleCamera.IsMoving && app.TitleUiVisible);
            Check(!app.CharacterSelectionFlying && app.TitleSelector.AnimationName == "Idle"
                && app.TitleSelector.FrameIndex == expected.FrameIndex, "Landing restarted the pointing animation.");
            Check(app.LiveSessions == 0 && app.Menu!.SelectedIndex == 0, "Arrival input leaked into selection.");
            app.Update(default, TimeSpan.FromMilliseconds(90));
            expected.Update(TimeSpan.FromMilliseconds(90));
            Check(app.TitleSelector.FrameIndex == expected.FrameIndex, "Selection must keep Sprout animated.");
            app.Update(MenuPointerInput.HitTest(app, female.X * 4, female.Y * 4, 2560, 1440), TimeSpan.Zero);
            Check(app.LiveSessions == 0 && app.CharacterSelection.IsConfirming);
            FinishCharacterConfirmation(app);
            Check(app.Screen == Screen.Room && app.Player is { Gender: PlayerGender.Female });
            Check(app.Room!.Value.Elapsed == TimeSpan.Zero && app.SessionsCreated == 1);
            ReturnPrompt(app); Discard(app);
            Check(app.TitleCamera.Position == 0 && app.TitleUiVisible && app.SessionsDisposed == 1);
        }

        using var cancelled = new ApplicationFlow();
        cancelled.Update(new(Confirm: true), TimeSpan.Zero);
        FinishTitleInteraction(cancelled);
        cancelled.Update(default, PanoramaCamera.TransitionDuration / 2);
        Check(cancelled.TitleCamera.Position == -.5);
        cancelled.Update(new(Cancel: true), TimeSpan.Zero);
        Check(cancelled.Screen == Screen.Title && cancelled.TitleCamera.Position == -.5 && !cancelled.TitleUiVisible);
        cancelled.Update(new(Confirm: true, SelectionIndex: 3), PanoramaCamera.TransitionDuration, active: false);
        Check(cancelled.TitleCamera.Position == -.5 && !cancelled.ExitRequested);
        cancelled.Update(new(Confirm: true, SelectionIndex: 3), PanoramaCamera.TransitionDuration);
        Check(cancelled.TitleCamera.Position == 0 && cancelled.TitleUiVisible && !cancelled.TitleInteractionPlaying);
        Check(cancelled.Menu!.SelectedIndex == 0 && cancelled.SessionsCreated == 0 && !cancelled.ExitRequested);
        Confirm(cancelled);
        var back = CharacterSelectionLayout.Back.Center;
        cancelled.Update(MenuPointerInput.HitTest(cancelled, back.X, back.Y, 640, 360), TimeSpan.Zero);
        Check(cancelled.Screen == Screen.Title && cancelled.TitleCamera.IsMoving && cancelled.LiveSessions == 0);
        cancelled.Update(default, PanoramaCamera.TransitionDuration);
        Confirm(cancelled);
        cancelled.RequestExit();
        Check(cancelled.ExitRequested && cancelled.SessionsCreated == 0);
    }

    internal static void CreationSelectorFlight()
    {
        Check(PanoramaCamera.TransitionDuration == TimeSpan.FromMilliseconds(1100));
        foreach (int uiScale in new[] { 100, 125, 150 })
        foreach (bool pointer in new[] { false, true })
        {
            using var app = new ApplicationFlow(new() { UiScale = uiScale });
            app.Update(default, TimeSpan.FromMilliseconds(270));
            int departureFrame = app.TitleSelector.FrameIndex;
            Check(departureFrame == 3);
            var row = TitleMenuLayout.Row(0).Center;
            var input = pointer ? MenuPointerInput.HitTest(app, row.X, row.Y, 1920, 1080) : new PawnShop.Framework.Input.InputFrame(Confirm: true);
            app.Update(input, TimeSpan.Zero);
            Check(app.Screen == Screen.CharacterSelect && app.CharacterSelectionFlying && !app.TitleInteractionPlaying);
            Check(app.TitleSelector.AnimationName == "Idle" && app.TitleSelector.FrameIndex == departureFrame,
                "New Game must retain the current pointing phase instead of restarting or playing Interact.");
            float textScale = uiScale / 100f;
            var start = new Microsoft.Xna.Framework.Vector2(93, 208);
            var male = CharacterSelectionLayout.SelectorAnchor(0, textScale);
            var female = CharacterSelectionLayout.SelectorAnchor(1, textScale);
            var back = CharacterSelectionLayout.SelectorAnchor(2, textScale);
            Check(CharacterSelectionLayout.FlightAnchor(start, 0, textScale) == start);
            Check(CharacterSelectionLayout.FlightAnchor(start, -.5, textScale) == Microsoft.Xna.Framework.Vector2.Lerp(start, male, .5f));
            Check(CharacterSelectionLayout.FlightAnchor(start, -1, textScale) == male);
            Check(male.X < CharacterSelectionLayout.Card(0).Center.X && female.X > male.X && back.Y > male.Y);
            app.Update(new(Confirm: true, Right: true), PanoramaCamera.TransitionDuration);
            Check(app.TitleCamera.Position == -1 && !app.CharacterSelectionFlying && app.Menu!.SelectedIndex == 0 && app.LiveSessions == 0);
            Check(app.TitleSelector.AnimationName == "Idle");
            app.Update(new(Right: true), TimeSpan.Zero);
            Check(app.Menu!.SelectedIndex == 1 && app.LiveSessions == 0);
            app.Update(new(Down: true), TimeSpan.Zero);
            Check(app.Menu.SelectedIndex == 2 && app.LiveSessions == 0);
            app.Update(new(Confirm: true), TimeSpan.Zero);
            Check(app.Screen == Screen.Title && app.TitleCamera.IsMoving && !app.CharacterSelectionFlying);
            app.Update(default, PanoramaCamera.TransitionDuration);
            app.Update(input, TimeSpan.Zero);
            Check(app.CharacterSelectionFlying && app.Menu!.SelectedIndex == 0);
        }
    }

    internal static void PlayerInputMapping()
    {
        var input = new KeyboardInput();
        Check(input.Read(new KeyboardState(Keys.Z, Keys.D), true) == default); // First focus frame synchronizes.
        var held = input.Read(new KeyboardState(Keys.Z, Keys.D), true);
        Check(!held.Confirm && !held.Interact && held.MoveX == 1 && held.MoveY == 0);
        input.Read(new KeyboardState(), true);
        var pressed = input.Read(new KeyboardState(Keys.Z, Keys.W, Keys.D), true);
        Check(pressed.Confirm && pressed.Interact && pressed.MoveX == 1 && pressed.MoveY == -1);
        Check(!input.Read(new KeyboardState(Keys.Z, Keys.W, Keys.D), true).Interact);
        var back = input.Read(new KeyboardState(Keys.X), true);
        Check(back.Cancel && !back.Interact && !back.Confirm);
        Check(!input.Read(new KeyboardState(Keys.X), true).Cancel);
        Check(input.Read(new KeyboardState(Keys.A, Keys.Z), false) == default);
        Check(input.Read(new KeyboardState(Keys.A, Keys.Z), true) == default);
        Check(input.Read(new KeyboardState(Keys.A), true).MoveX == -1);
        Check(input.Read(new KeyboardState(Keys.A, Keys.D, Keys.W, Keys.S), true).MoveX == 0);
        Check(input.Read(new KeyboardState(Keys.A, Keys.D, Keys.W, Keys.S), true).MoveY == 0);
    }

    internal static void PlayerMovement()
    {
        var area = new MovementBounds(0, 0, 2000, 2000);
        var origin = new Vector2(1000, 1000);
        foreach (var gender in Enum.GetValues<PlayerGender>())
        {
            var cardinal = new PlayerState(gender, origin);
            var diagonal = new PlayerState(gender, origin);
            cardinal.Move(Vector2.UnitX, TimeSpan.FromSeconds(1), area, []);
            diagonal.Move(new(1, -1), TimeSpan.FromSeconds(1), area, []);
            Check(Math.Abs(Vector2.Distance(cardinal.View.Position, origin) - PlayerState.Speed) < .001);
            Check(Math.Abs(Vector2.Distance(diagonal.View.Position, origin) - PlayerState.Speed) < .001);
            Check(diagonal.View.Facing == Facing.North && diagonal.View.Moving);
            var split = new PlayerState(gender, origin);
            for (int i = 0; i < 120; i++) split.Move(new(1, -1), TimeSpan.FromSeconds(1.0 / 120), area, []);
            Check(Vector2.Distance(split.View.Position, diagonal.View.Position) < .02);
            diagonal.Move(default, TimeSpan.FromSeconds(1), area, []);
            Check(!diagonal.View.Moving && diagonal.View.Facing == Facing.North);
        }
        var directions = new (Vector2 Intent, Facing Facing)[]
        {
            (new(0, 1), Facing.South), (new(1, 1), Facing.South), (new(1, 0), Facing.East),
            (new(1, -1), Facing.North), (new(0, -1), Facing.North), (new(-1, -1), Facing.North),
            (new(-1, 0), Facing.West), (new(-1, 1), Facing.South)
        };
        foreach (var (intent, facing) in directions)
        {
            var actor = new PlayerState(PlayerGender.Male, origin);
            actor.Move(intent, TimeSpan.FromSeconds(.1), area, []);
            Check(actor.View.Facing == facing);
        }
    }

    internal static void PlayerCollision()
    {
        var area = new MovementBounds(0, 0, 400, 400);
        var obstacle = new MovementBounds(100, 40, 16, 20);
        var actor = new PlayerState(PlayerGender.Male, new(50, 50));
        actor.Move(Vector2.UnitX, TimeSpan.FromSeconds(10), area, [obstacle]);
        Check(actor.View.Position.X == 94 && !actor.Footprint.Intersects(obstacle), "Horizontal sweep tunneled.");
        actor.Move(Vector2.UnitY, TimeSpan.FromSeconds(1), area, [obstacle]);
        actor.Move(Vector2.UnitX, TimeSpan.FromSeconds(1), area, [obstacle]);
        Check(actor.View.Position.X > obstacle.Right && !actor.Footprint.Intersects(obstacle));
        var above = new PlayerState(PlayerGender.Female, new(108, 25));
        above.Move(Vector2.UnitY, TimeSpan.FromSeconds(10), area, [obstacle]);
        Check(above.View.Position.Y == 40 && !above.Footprint.Intersects(obstacle), "Vertical sweep tunneled.");
        var below = new PlayerState(PlayerGender.Female, new(108, 100));
        below.Move(-Vector2.UnitY, TimeSpan.FromSeconds(10), area, [obstacle]);
        Check(below.View.Position.Y == 66 && !below.Footprint.Intersects(obstacle));
        foreach (var intent in new[] { Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY })
        {
            actor.Move(intent, TimeSpan.FromSeconds(100), area, []);
            Check(area.Contains(actor.Footprint));
        }
        using var app = new ApplicationFlow();
        Begin(app);
        app.Update(new(MoveX: 1), TimeSpan.FromSeconds(20));
        var npc = app.Npcs.Single();
        Check(!ShopRoomLayout.NpcFootprint(npc).Intersects(new(app.Player!.Value.Position.X - 6, app.Player.Value.Position.Y - 6, 12, 6)));
        app.Update(new(MoveY: -1), TimeSpan.FromSeconds(20));
        Check(app.Player.Value.Position.Y >= ShopRoomLayout.Floor.Top + 6);
    }

    internal static void NearbyInteraction()
    {
        using var app = new ApplicationFlow();
        Begin(app);
        app.Update(default, TimeSpan.FromSeconds(3)); // The demo visitor now enters before waiting at the talk position.
        Check(app.Npcs.Count == 1 && app.SelectedNpcId is null);
        Check(app.TryTalkTo("visitor") == TalkResult.OutOfRange && app.Dialogue is null);
        var npc = app.Npcs.Single();
        app.Update(MenuPointerInput.HitTest(app, npc.X, npc.Y - 10, 640, 360), TimeSpan.Zero);
        Check(app.Screen == Screen.Room && app.Dialogue is null);
        app.Update(new(Interact: true, MoveX: 1), TimeSpan.FromSeconds(1.3));
        Check(app.Screen == Screen.Room && app.SelectedNpcId == "visitor");
        var player = app.Player;
        var room = app.Room;
        app.Update(new(Interact: true, Confirm: true, MoveX: 1), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Dialogue && app.Player == player && app.Room == room);
        app.Update(new(MoveX: -1), TimeSpan.FromSeconds(10));
        Check(app.Player == player && app.Room == room);
        app.Update(new(Cancel: true, Interact: true, MoveX: -1), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Room && app.Player == player && app.Room == room);
        app.Update(new(Cancel: true, MoveX: -1), TimeSpan.FromSeconds(1));
        app.Update(new(MoveY: 1), TimeSpan.FromSeconds(5));
        Check(app.Screen == Screen.Pause && app.Player == player && app.Room == room);
        Confirm(app);
        app.Update(new(MoveX: -1), TimeSpan.FromSeconds(1));
        Check(app.SelectedNpcId is null && app.TryTalkTo("visitor") == TalkResult.OutOfRange);
    }

    internal static void PlayerContent()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Content", "Player");
        var catalog = new PlayerSpriteCatalog(source);
        Check(catalog.Sheet.FrameWidth == 48 && catalog.Sheet.FrameHeight == 64 && catalog.Sheet.Pivot.X == 24 && catalog.Sheet.Pivot.Y == 58);
        Check(catalog.Textures.Count == 2 && Enum.GetValues<Facing>().Select(f => catalog.Frame(f).X).Distinct().Count() == 3);
        foreach (var facing in Enum.GetValues<Facing>()) Check(catalog.Frame(facing).Width == 48 && catalog.Frame(facing).Y == 0);
        string temp = Path.Combine(Path.GetTempPath(), "PawnShopPlayer-" + Guid.NewGuid());
        Directory.CreateDirectory(temp);
        try
        {
            foreach (var sex in new[] { "male", "female" })
            {
                Directory.CreateDirectory(Path.Combine(temp, "Bases", sex));
                File.Copy(Path.Combine(source, "Bases", sex, "base.png"), Path.Combine(temp, "Bases", sex, "base.png"));
            }
            string metadata = File.ReadAllText(Path.Combine(source, "player-sprite.json"));
            File.Copy(Path.Combine(source, "placeholder-sprite.json"), Path.Combine(temp, "placeholder-sprite.json"));
            File.WriteAllText(Path.Combine(temp, "player-sprite.json"), metadata.Replace("\"IdleNorth\"", "\"Unused\""));
            try { _ = new PlayerSpriteCatalog(temp); throw new Exception("Expected missing facing diagnostic."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("IdleNorth")); }
            File.WriteAllText(Path.Combine(temp, "player-sprite.json"), metadata);
            string preview = File.ReadAllText(Path.Combine(source, "placeholder-sprite.json"));
            foreach (string invalid in new[] { preview.Replace("\"pivotX\": 24", "\"pivotX\": 20"),
                preview.Replace("\"Walk\"", "\"Unused\""), preview.Replace("\"frameCount\": 4", "\"frameCount\": 3") })
            {
                File.WriteAllText(Path.Combine(temp, "placeholder-sprite.json"), invalid);
                try { _ = new PlayerSpriteCatalog(temp); throw new Exception("Expected incompatible placeholder contract diagnostic."); }
                catch (InvalidDataException e) { Check(e.Message.Contains("placeholder-sprite.json")); }
            }
            File.WriteAllText(Path.Combine(temp, "placeholder-sprite.json"), preview);
            File.WriteAllBytes(Path.Combine(temp, "Bases", "female", "base.png"), new byte[24]);
            try { _ = new PlayerSpriteCatalog(temp); throw new Exception("Expected invalid PNG diagnostic."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("female") && e.Message.Contains("PNG")); }
        }
        finally { Directory.Delete(temp, recursive: true); }
    }

    internal static void DefaultControlsMigration()
    {
        string temp = Path.Combine(Path.GetTempPath(), "PawnShopBindings-" + Guid.NewGuid());
        Directory.CreateDirectory(temp);
        string path = Path.Combine(temp, "settings.json");
        try
        {
            const string old = "{\"InteractKey\":\"Space\",\"PauseKey\":\"Escape\",\"UiScale\":125,\"Music\":20}";
            File.WriteAllText(path, old);
            var settings = new SettingsStore(path).Load(out var warning);
            Check(settings.InteractKey == Keys.Z && settings.PauseKey == Keys.X && settings.UiScale == 125 && settings.Music == 20 && warning is null);
            Check(File.ReadAllText(path) == old, "Loading controls must not rewrite user preferences.");
            const string custom = "{\"InteractKey\":\"E\",\"PauseKey\":\"P\",\"UiScale\":150}";
            File.WriteAllText(path, custom);
            settings = new SettingsStore(path).Load(out warning);
            Check(settings.InteractKey == Keys.E && settings.PauseKey == Keys.P && settings.UiScale == 150 && warning is null);
            Check(File.ReadAllText(path) == custom);
        }
        finally { Directory.Delete(temp, recursive: true); }
    }
}
