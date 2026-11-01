using PawnShop.Core.World;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Screens;

namespace PawnShop.Game.Bootstrap;

// Sole session and simulation owner. UI requests pass here before room input.
public sealed class ApplicationFlow : IDisposable
{
    private GameSession? session;
    private Screen confirmationOrigin;
    private bool disposed;
    private Screen optionsOrigin;
    private MenuCommand? pendingTitleCommand;
    public bool TitleInteractionPlaying { get; private set; }
    public SpriteAnimator TitleSelector { get; }
    public OptionsModel Options { get; }
    public PanoramaCamera TitleCamera { get; } = new();
    public bool UsesTitleBackground => session is null && Screen is Screen.Title or Screen.Options;
    public bool TitleUiVisible => UsesTitleBackground && !TitleCamera.IsMoving;
    public ApplicationFlow(AppSettings? settings = null, Action<AppSettings>? saveSettings = null)
    {
        Options = new(settings ?? new(), saveSettings);
        var sheet = SpriteSheetDefinition.Load(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "sprout-title.json"));
        if (!sheet.Animations.TryGetValue("Idle", out var idle) || !idle.Loop
            || !sheet.Animations.TryGetValue("Interact", out var interact) || interact.Loop)
            throw new InvalidDataException("sprout-title.json: requires looping Idle and non-looping Interact animations.");
        TitleSelector = new(sheet, "Idle");
    }
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
        if (Screen == Screen.Title && !TitleCamera.IsMoving)
        {
            if (active || !TitleInteractionPlaying) TitleSelector.Update(delta);
            if (TitleInteractionPlaying)
            {
                if (!active) return;
                if (input.Cancel) { CancelTitleInteraction(); return; }
                if (!TitleSelector.IsComplete) return;
                var pending = pendingTitleCommand;
                CancelTitleInteraction();
                ExecuteMenuCommand(pending);
                return; // Completing feedback consumes input; never tick a newly created room.
            }
        }
        if (UsesTitleBackground) TitleCamera.Update(delta);
        if (UsesTitleBackground && TitleCamera.IsMoving)
        {
            // Back may reverse an in-progress pan; hidden controls cannot activate.
            if (active && input.Cancel && Screen == Screen.Options)
            {
                Options.Cancel();
                SetScreen(Screen.Title);
            }
            return;
        }
        if (Screen == Screen.Options)
        {
            if (Options.Update(input, delta, active)) SetScreen(optionsOrigin);
            return;
        }
        if (!active)
        {
            if (Screen == Screen.Room)
            {
                if (Options.Current.PauseUnfocused) SetScreen(Screen.Pause);
                else session!.Tick(delta, false);
            }
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
        if (Screen == Screen.Title && input.Confirm && input.Up == input.Down)
        {
            // Even unavailable entries react, but only enabled commands are queued.
            pendingTitleCommand = command;
            TitleInteractionPlaying = true;
            TitleSelector.Play("Interact", restart: true);
            return;
        }
        ExecuteMenuCommand(command);
        // Even a closing/starting menu consumes this entire frame. No fallthrough to Tick.
    }

    private void ExecuteMenuCommand(MenuCommand? command)
    {
        switch (command)
        {
            case MenuCommand.NewGame:
                if (session is not null) throw new InvalidOperationException("A session is already active.");
                session = new GameSession();
                SessionsCreated++;
                SetScreen(Screen.Room);
                break;
            case MenuCommand.Resume: SetScreen(Screen.Room); break;
            case MenuCommand.Settings:
                optionsOrigin = Screen;
                Options.Open();
                SetScreen(Screen.Options);
                break;
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
    }

    public void RequestExit()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        CancelTitleInteraction();
        if (session is null) ExitRequested = true;
        else if (Screen != Screen.ConfirmExit)
        {
            Options.Cancel();
            Confirm(Screen.ConfirmExit);
        }
    }

    private void Confirm(Screen target)
    {
        confirmationOrigin = session is null ? Screen.Title : Screen.Pause;
        SetScreen(target);
    }

    private void SetScreen(Screen screen)
    {
        CancelTitleInteraction();
        if (session is null)
        {
            if (screen == Screen.Options) TitleCamera.NavigateToOptions();
            else if (screen == Screen.Title) TitleCamera.NavigateToMainMenu();
        }
        Screen = screen;
        Menu = screen switch
        {
            Screen.Title => TitleMenu(),
            Screen.Pause => new(new("Resume", MenuCommand.Resume),
                new("Return to title", MenuCommand.ReturnToTitle), new("Exit game", MenuCommand.Exit),
                new("Options", MenuCommand.Settings)),
            Screen.ConfirmReturn or Screen.ConfirmExit => new(new("Keep playing", MenuCommand.Cancel),
                new(screen == Screen.ConfirmReturn ? "Discard and return" : "Discard and exit", MenuCommand.Discard)),
            _ => null
        };
    }

    private void CancelTitleInteraction()
    {
        pendingTitleCommand = null;
        TitleInteractionPlaying = false;
        TitleSelector.Play("Idle", restart: true);
    }

    private static Menu<MenuCommand> TitleMenu() => new(new("New Game", MenuCommand.NewGame),
        new("Continue", MenuCommand.Continue, "Save and load are not implemented yet."),
        new("Options", MenuCommand.Settings),
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
        CancelTitleInteraction();
        EndSession();
        Menu = null;
        disposed = true;
    }
}
