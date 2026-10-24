using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void Check(bool condition, string message = "Assertion failed")
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    internal static void Step(ApplicationFlow app, InputFrame input = default) => app.Update(input, TimeSpan.FromSeconds(1));
    internal static void Down(ApplicationFlow app) => Step(app, new(Down: true));
    internal static void Confirm(ApplicationFlow app) => Step(app, new(Confirm: true));
    internal static void Back(ApplicationFlow app) => Step(app, new(Cancel: true));
    internal static void Begin(ApplicationFlow app) => Confirm(app);
    internal static void ReturnPrompt(ApplicationFlow app) { Back(app); Down(app); Confirm(app); }
    internal static void Discard(ApplicationFlow app) { Down(app); Confirm(app); }

    internal static void Title()
    {
        using var app = new ApplicationFlow();
        Step(app);
        Check(app.Screen == Screen.Title && app.Room is null && app.LiveSessions == 0 && app.SessionsCreated == 0);
        foreach (var command in new[] { MenuCommand.Continue, MenuCommand.Settings })
        {
            Down(app);
            Check(app.Menu!.Selected.Command == command && !app.Menu.Selected.Enabled);
            Check(!string.IsNullOrWhiteSpace(app.Menu.Selected.DisabledReason));
            Confirm(app);
            Check(app.Room is null && app.Screen == Screen.Title);
        }
        Back(app);
        Check(!app.ExitRequested);
    }
    internal static void Start()
    {
        using var app = new ApplicationFlow();
        Step(app, new(Confirm: true, Interact: true));
        Check(app.Screen == Screen.Room && app.LiveSessions == 1 && app.SessionsCreated == 1);
        Check(app.Room!.Value.Elapsed == TimeSpan.Zero && app.Room.Value.BellRings == 0);
        Confirm(app);
        Check(app.SessionsCreated == 1);
        Step(app, new(Interact: true));
        Check(app.Room.Value.BellRings == 1);
    }
    internal static void Pause()
    {
        using var app = new ApplicationFlow();
        Begin(app);
        Step(app);
        var before = app.Room;
        Step(app, new(Cancel: true, Interact: true));
        var presentation = app.PresentationTime;
        Step(app, new(Interact: true));
        Check(app.Screen == Screen.Pause && app.Room == before && app.PresentationTime > presentation);
        Step(app, new(Confirm: true, Interact: true));
        Check(app.Screen == Screen.Room && app.Room == before);
        Back(app);
        Step(app, new(Cancel: true, Interact: true));
        Check(app.Screen == Screen.Room && app.Room == before);
        Step(app);
        Check(app.Room!.Value.Elapsed == before!.Value.Elapsed + TimeSpan.FromSeconds(1));
    }
    internal static void Focus()
    {
        using var app = new ApplicationFlow();
        Begin(app);
        var before = app.Room;
        app.Update(new(Interact: true), TimeSpan.FromSeconds(8), active: false);
        Check(app.Screen == Screen.Pause && app.Room == before);
        Step(app);
        Check(app.Screen == Screen.Pause && app.Room == before);
    }
    internal static void Return()
    {
        using var app = new ApplicationFlow();
        Begin(app);
        Step(app, new(Interact: true));
        var previous = app.Room;
        ReturnPrompt(app);
        Check(app.Screen == Screen.ConfirmReturn && app.Room == previous);
        Confirm(app); // Safe default is keep playing, back to pause.
        Check(app.Screen == Screen.Pause && app.Room == previous);
        Down(app); Confirm(app);
        Back(app);
        Check(app.Screen == Screen.Pause && app.SessionsDisposed == 0);
        Down(app); Confirm(app); Discard(app);
        Check(app.Screen == Screen.Title && app.Room is null && app.SessionsDisposed == 1 && !app.ExitRequested);
        Begin(app);
        Check(app.Room!.Value.SessionId != previous!.Value.SessionId);
        Check(app.Room.Value.Elapsed == TimeSpan.Zero && app.Room.Value.BellRings == 0);
    }
    internal static void Cycles()
    {
        using var app = new ApplicationFlow();
        var ids = new HashSet<Guid>();
        for (int i = 1; i <= 10; i++)
        {
            Begin(app);
            Check(app.LiveSessions == 1 && ids.Add(app.Room!.Value.SessionId));
            Check(app.Room!.Value.BellRings == 0 && app.Room.Value.Elapsed == TimeSpan.Zero);
            Step(app, new(Interact: true));
            ReturnPrompt(app); Discard(app);
            Check(app.LiveSessions == 0 && app.Room is null);
            Check(app.SessionsCreated == i && app.SessionsDisposed == i);
        }
    }
    internal static void Exit()
    {
        using var title = new ApplicationFlow();
        Step(title, new(Up: true)); Confirm(title);
        Check(title.ExitRequested && title.SessionsCreated == 0);
        using var app = new ApplicationFlow();
        Begin(app);
        var previous = app.Room;
        app.RequestExit();
        Check(app.Screen == Screen.ConfirmExit && !app.ExitRequested);
        Back(app);
        Check(app.Screen == Screen.Pause && app.Room == previous);
        Down(app); Down(app); Confirm(app);
        Check(app.Screen == Screen.ConfirmExit && !app.ExitRequested);
        Discard(app);
        Check(app.ExitRequested && app.Room is null && app.SessionsDisposed == 1);
        Step(app);
        Check(app.SessionsCreated == 1);
    }
    internal static void Disposal()
    {
        var app = new ApplicationFlow();
        Begin(app);
        app.Dispose(); app.Dispose();
        Check(app.Room is null && app.Menu is null && app.SessionsDisposed == 1);
        try { Step(app); throw new Exception("Expected disposed exception"); }
        catch (ObjectDisposedException) { }
    }
}
