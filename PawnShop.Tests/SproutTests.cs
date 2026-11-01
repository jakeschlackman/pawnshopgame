using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private static string SproutMetadataPath => Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "sprout-title.json");

    internal static void SproutSheet()
    {
        var sheet = SpriteSheetDefinition.Load(SproutMetadataPath);
        Check(sheet.ImageWidth == 192 && sheet.ImageHeight == 384 && sheet.FrameWidth == 48 && sheet.FrameHeight == 48);
        Check(sheet.GetFrame(0) == new Rectangle(0, 0, 48, 48));
        Check(sheet.GetFrame(15) == new Rectangle(144, 144, 48, 48));
        Check(sheet.GetFrame(16) == new Rectangle(0, 192, 48, 48));
        Check(sheet.GetFrame(31) == new Rectangle(144, 336, 48, 48));
        Check(sheet.Pivot == new Vector2(26, 28) && sheet.DisplayScale == 1.5f);
        string json = File.ReadAllText(SproutMetadataPath);
        foreach (string invalid in new[] { "{}", "{broken", json.Replace("\"columns\": 4", "\"columns\": 0"),
            json.Replace("\"frameDurationMs\": 90", "\"frameDurationMs\": 0"),
            json.Replace("\"frameDurationMs\": 50", "\"frameDurationMs\": 0"),
            json.Replace("\"firstFrame\": 16", "\"firstFrame\": 31"),
            json.Replace("\"pivotX\": 26", "\"pivotX\": 49"), json.Replace("\"Interact\"", "\"Idle\"") })
        {
            try { SpriteSheetDefinition.Parse(invalid, "test-sheet.json"); throw new Exception("Expected invalid sheet rejection"); }
            catch (InvalidDataException e) { Check(e.Message.Contains("test-sheet.json")); }
        }
        try { sheet.GetFrame(32); throw new Exception("Expected out-of-bounds rejection"); }
        catch (ArgumentOutOfRangeException) { }
        byte[] png = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "sprout-title.png"));
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)) == sheet.ImageWidth);
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)) == sheet.ImageHeight);
    }

    internal static void SproutTiming()
    {
        var animator = new SpriteAnimator(SpriteSheetDefinition.Load(SproutMetadataPath), "Idle");
        Check(animator.FrameIndex == 0 && !animator.IsComplete);
        animator.Update(TimeSpan.FromMilliseconds(89));
        Check(animator.FrameIndex == 0);
        animator.Update(TimeSpan.FromMilliseconds(1));
        Check(animator.FrameIndex == 1);
        animator.Play("Idle"); // Same state must not restart every update.
        Check(animator.FrameIndex == 1);
        animator.Update(TimeSpan.FromMilliseconds(1350));
        Check(animator.FrameIndex == 0 && !animator.IsComplete);
        animator.Update(TimeSpan.FromMilliseconds(1440 * 100 + 270));
        Check(animator.FrameIndex == 3);
        animator.Play("Interact");
        for (int frame = 16; frame <= 31; frame++)
        {
            Check(animator.FrameIndex == frame && !animator.IsComplete);
            animator.Update(TimeSpan.FromMilliseconds(50));
        }
        Check(animator.FrameIndex == 31 && animator.IsComplete && animator.Remaining == TimeSpan.Zero);
        animator.Update(TimeSpan.MaxValue);
        Check(animator.FrameIndex == 31 && animator.IsComplete);
        animator.Play("Idle");
        animator.Update(TimeSpan.MaxValue);
        Check(animator.FrameIndex is >= 0 and <= 15 && !animator.IsComplete);
        animator.Play("Idle", restart: true);
        Check(animator.FrameIndex == 0);
        try { animator.Play("Missing"); throw new Exception("Expected missing clip rejection"); }
        catch (InvalidDataException) { }
    }

    internal static void SproutSelection()
    {
        using var app = new ApplicationFlow();
        app.Update(new(Confirm: true), TimeSpan.Zero);
        Check(app.TitleInteractionPlaying && app.TitleSelector.FrameIndex == 16 && app.LiveSessions == 0);
        app.Update(new(Down: true, Confirm: true), TimeSpan.FromMilliseconds(50));
        Check(app.Menu!.SelectedIndex == 0 && app.TitleSelector.FrameIndex == 17);
        app.Update(default, TimeSpan.FromSeconds(10), active: false);
        Check(app.LiveSessions == 0 && app.TitleSelector.FrameIndex == 17);
        app.Update(new(Confirm: true, Interact: true), TimeSpan.FromMilliseconds(749));
        Check(app.TitleInteractionPlaying && app.TitleSelector.FrameIndex == 31 && app.LiveSessions == 0);
        app.Update(new(Confirm: true, Interact: true), TimeSpan.FromMilliseconds(1));
        Check(app.Screen == Screen.Room && app.LiveSessions == 1 && app.SessionsCreated == 1);
        Check(app.Room!.Value.BellRings == 0 && app.Room.Value.Elapsed == TimeSpan.Zero);

        using var cancelled = new ApplicationFlow();
        cancelled.Update(new(Confirm: true), TimeSpan.Zero);
        cancelled.Update(new(Cancel: true), TimeSpan.FromMilliseconds(50));
        Check(!cancelled.TitleInteractionPlaying && cancelled.TitleSelector.FrameIndex == 0);
        cancelled.Update(default, TimeSpan.FromSeconds(5));
        Check(cancelled.Screen == Screen.Title && cancelled.LiveSessions == 0);
        Down(cancelled);
        cancelled.Update(new(Confirm: true), TimeSpan.Zero); // Unavailable Continue still reacts.
        Check(cancelled.TitleInteractionPlaying && cancelled.Menu!.Selected.Command == MenuCommand.Continue);
        FinishTitleInteraction(cancelled);
        Check(cancelled.Screen == Screen.Title && !cancelled.TitleInteractionPlaying && cancelled.LiveSessions == 0);
        Down(cancelled);
        cancelled.Update(new(Confirm: true), TimeSpan.Zero); // Options waits before beginning the pan.
        Check(cancelled.Screen == Screen.Title && !cancelled.TitleCamera.IsMoving);
        FinishTitleInteraction(cancelled);
        Check(cancelled.Screen == Screen.Options && cancelled.TitleCamera.IsMoving);

        using var closing = new ApplicationFlow();
        closing.Update(new(Confirm: true), TimeSpan.Zero);
        closing.RequestExit();
        Check(closing.ExitRequested && !closing.TitleInteractionPlaying && closing.LiveSessions == 0);
    }
}
