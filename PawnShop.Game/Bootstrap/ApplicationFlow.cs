using PawnShop.Core.World;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Game.Screens;

namespace PawnShop.Game.Bootstrap;

// Sole session and simulation owner. UI requests pass here before room input.
public sealed class ApplicationFlow : IDisposable
{
    private GameSession? session;
    private Screen confirmationOrigin;
    private bool disposed;
    public Screen Screen { get; private set; } = Screen.Title;
    public Menu<MenuCommand>? Menu { get; private set; } = TitleMenu();
    public RoomView? Room => session?.View;
    public bool ExitRequested { get; private set; }
    public TimeSpan PresentationTime { get; private set; }
    public int SessionsCreated { get; private set; }
    public int SessionsDisposed { get; private set; }
    public int LiveSessions => session is null ? 0 : 1;

    public void Update(InputFrame input, TimeSpan delta, bool active = true)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        if (ExitRequested) return;
        PresentationTime += delta;
        if (!active)
        {
            if (Screen == Screen.Room) SetScreen(Screen.Pause);
            return;
        }
        if (Screen == Screen.Room)
        {
            if (input.Cancel) { SetScreen(Screen.Pause); return; }
            session!.Tick(delta, input.Interact);
            return;
        }
        if (input.Cancel)
        {
            switch (Screen)
            {
                case Screen.Pause: SetScreen(Screen.Room); break;
                case Screen.ConfirmExit:
                case Screen.ConfirmReturn: SetScreen(confirmationOrigin); break;
                // Escape on title is deliberately harmless; Exit is a separate menu action.
            }
            return;
        }
        var command = Menu?.Handle(input);
        switch (command)
        {
            case MenuCommand.NewGame:
                if (session is not null) throw new InvalidOperationException("A session is already active.");
                session = new GameSession();
                SessionsCreated++;
                SetScreen(Screen.Room);
                break;
            case MenuCommand.Resume: SetScreen(Screen.Room); break;
            case MenuCommand.ReturnToTitle: Confirm(Screen.ConfirmReturn); break;
            case MenuCommand.Exit:
                if (session is null) ExitRequested = true;
                else Confirm(Screen.ConfirmExit);
                break;
            case MenuCommand.Cancel: SetScreen(confirmationOrigin); break;
            case MenuCommand.Discard:
                bool exit = Screen == Screen.ConfirmExit;
                EndSession();
                SetScreen(Screen.Title);
                ExitRequested = exit;
                break;
        }
        // Even a closing/starting menu consumes this entire frame. No fallthrough to Tick.
    }

    public void RequestExit()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (session is null) ExitRequested = true;
        else if (Screen != Screen.ConfirmExit) Confirm(Screen.ConfirmExit);
    }

    private void Confirm(Screen target)
    {
        confirmationOrigin = session is null ? Screen.Title : Screen.Pause;
        SetScreen(target);
    }

    private void SetScreen(Screen screen)
    {
        Screen = screen;
        Menu = screen switch
        {
            Screen.Title => TitleMenu(),
            Screen.Pause => new(new("Resume", MenuCommand.Resume),
                new("Return to title", MenuCommand.ReturnToTitle), new("Exit game", MenuCommand.Exit)),
            Screen.ConfirmReturn or Screen.ConfirmExit => new(new("Keep playing", MenuCommand.Cancel),
                new(screen == Screen.ConfirmReturn ? "Discard and return" : "Discard and exit", MenuCommand.Discard)),
            _ => null
        };
    }

    private static Menu<MenuCommand> TitleMenu() => new(new("New Game", MenuCommand.NewGame),
        new("Continue", MenuCommand.Continue, "Save and load are not implemented yet."),
        new("Settings", MenuCommand.Settings, "No configurable settings in this prototype."),
        new("Exit", MenuCommand.Exit));

    private void EndSession()
    {
        if (session is null) return;
        session.Dispose();
        session = null;
        SessionsDisposed++;
    }

    public void Dispose()
    {
        if (disposed) return;
        EndSession();
        Menu = null;
        disposed = true;
    }
}
