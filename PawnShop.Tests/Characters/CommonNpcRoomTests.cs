using PawnShop.Characters.Movement;
using PawnShop.Characters.Npcs.Common;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using static PawnShop.Tests.RegressionTests;

namespace PawnShop.Tests;

internal static class CommonNpcRoomTests
{
    internal static void Entrance()
    {
        var actor = new CommonNpcEntrance(672, 479, 210);
        Check(actor.X - 24 > 640 && actor.SpriteFrame == 1 && !actor.Arrived);
        actor.Update(TimeSpan.FromMilliseconds(100), []);
        Check(actor.X == 664 && actor.SpriteFrame == 2);
        actor.Update(TimeSpan.FromMilliseconds(700), []);
        Check(actor.X == 608 && actor.SpriteFrame == 1, "Eight 100 ms frames must loop without standing.");
        var whole = new CommonNpcEntrance(672, 479, 210);
        var split = new CommonNpcEntrance(672, 479, 210);
        whole.Update(TimeSpan.FromSeconds(2), []);
        for (int i = 0; i < 20; i++) split.Update(TimeSpan.FromMilliseconds(100), []);
        Check(Math.Abs(whole.X - split.X) < .001 && whole.SpriteFrame == split.SpriteFrame);
        whole.Update(TimeSpan.FromMilliseconds(412.5), []);
        Check(whole.X == 479 && whole.SpriteFrame == 1 && !whole.Arrived);
        whole.Update(TimeSpan.FromMilliseconds(100), []);
        Check(whole.SpriteFrame == 9 && !whole.Arrived, "Stop must use the approved settling pose.");
        whole.Update(TimeSpan.FromMilliseconds(100), []);
        Check(whole.SpriteFrame == 0 && whole.Arrived);
        whole.Update(TimeSpan.FromDays(1), []);
        Check(whole.X == 479 && whole.SpriteFrame == 0, "Arrived visitor must not drift or restart.");
        var blocked = new CommonNpcEntrance(672, 479, 210);
        MovementBounds player = new(520, 204, 12, 6);
        blocked.Update(TimeSpan.FromSeconds(20), [player]);
        Check(blocked.X == 540 && !new MovementBounds(blocked.X - 8, 204, 16, 6).Intersects(player), "Entrance sweep tunneled through player.");
        blocked.Update(TimeSpan.FromSeconds(1), [player]);
        Check(blocked.X == 540 && blocked.SpriteFrame == 0 && !blocked.Arrived);
        blocked.Update(TimeSpan.FromSeconds(20), []);
        Check(blocked.X == 479 && blocked.Arrived, "Entrance must resume when the path clears.");
    }

    internal static void RoomLifecycle()
    {
        using var app = new ApplicationFlow();
        Check(app.CommonVisitor is null && app.Npcs.Count == 0, "Title must not create a shopper.");
        Begin(app);
        var visitor = app.CommonVisitor ?? throw new InvalidOperationException("Missing generated room visitor.");
        Check(app.Npcs.Count == 1 && app.Npcs[0].Id == "visitor" && app.Npcs[0].X == 672);
        Check(visitor.Frames.Count == 10 && visitor.Frames.All(f => f.MirrorHorizontally));
        Check(visitor.Frames.Select(f => f.Layers[0].X).SequenceEqual(Enumerable.Range(0, 10).Select(i => i * 48)));
        var appearance = visitor.Character;
        var player = app.Player;
        app.Update(default, TimeSpan.FromMilliseconds(100));
        Check(visitor.X == 664 && app.Npcs[0].X == visitor.X && visitor.SpriteFrame == 2 && app.NpcFrames["visitor"] == 2 && app.Player == player);
        Back(app);
        app.Update(default, TimeSpan.FromSeconds(10));
        Check(visitor.X == 664 && visitor.SpriteFrame == 2 && visitor.Character == appearance, "Pause changed the visitor.");
        Confirm(app);
        app.Update(new(MoveX: 1), TimeSpan.FromSeconds(1.8));
        Check(!visitor.Arrived && app.TryTalkTo("visitor") == TalkResult.Started, "Moving visitor should remain talkable nearby.");
        int stoppedX = visitor.X, stoppedFrame = visitor.SpriteFrame;
        app.Update(default, TimeSpan.FromSeconds(10));
        Check(visitor.X == stoppedX && visitor.SpriteFrame == stoppedFrame, "Dialogue must freeze the entrance clock and position.");
        Back(app);
        app.Update(new(MoveX: -1), TimeSpan.FromSeconds(1)); // Clear the entrance path after talking.
        app.Update(default, TimeSpan.FromSeconds(3));
        Check(visitor.Arrived && visitor.X == 479 && app.Npcs[0].X == 479 && visitor.SpriteFrame == 0);
        Approach(app, "visitor");
        var click = MenuPointerInput.HitTest(app, visitor.X, visitor.Y - 52, 640, 360);
        Check(click.TalkTargetId == "visitor", "Click bounds must cover the taller common-NPC head.");
        app.Update(click, TimeSpan.Zero);
        Check(app.Screen == Screen.Dialogue && app.Dialogue!.View.LineId == "visitor.greeting");
        app.Update(default, TimeSpan.FromSeconds(10));
        Check(visitor.Character == appearance && visitor.X == 479 && visitor.SpriteFrame == 0);
        Back(app);
        ReturnPrompt(app); Discard(app);
        Check(app.CommonVisitor is null && app.Npcs.Count == 0);
        Begin(app);
        Check(app.CommonVisitor is { X: 672, SpriteFrame: 1 } && app.CommonVisitor.Character.Id != appearance.Id,
            "New Game must create a new session-owned shopper and restart entrance.");
        var authoredCatalog = NpcContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content/Npcs"),
            Path.Combine(AppContext.BaseDirectory, "Content/World/shop.npcs.json"));
        using var authored = new ApplicationFlow(conversations: authoredCatalog);
        Begin(authored);
        Check(authored.CommonVisitor is null && authored.Npcs[0].X == 479, "Injected authored NPCs must retain their own presentation.");
    }
}
