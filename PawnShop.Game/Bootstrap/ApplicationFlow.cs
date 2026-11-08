using PawnShop.Core.World;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Screens;
using PawnShop.Narrative.Dialogue;
using PawnShop.Characters.Player;
using PawnShop.Characters.Npcs.Common;

namespace PawnShop.Game.Bootstrap;

// Sole session and simulation owner. UI requests pass here before room input.
public sealed class ApplicationFlow : IDisposable
{
    private GameSession? session;
    private Screen confirmationOrigin;
    private bool disposed;
    private Screen optionsOrigin;
    private MenuCommand? pendingTitleCommand;
    private bool titleInteractionPlaying;
    public bool TitleInteractionPlaying => titleInteractionPlaying || OptionsFlight.IsBusy;
    public SpriteAnimator TitleSelector { get; }
    public OptionsSproutFlight OptionsFlight { get; }
    public OptionsModel Options { get; }
    private readonly NpcContentCatalog conversations;
    private readonly CommonNpcCatalog? commonNpcs;
    public string TitleSaying { get; }
    public CommonNpcRoomVisitor? CommonVisitor => session?.CommonVisitor;
    public IReadOnlyList<PlacedNpc> Npcs => session?.Npcs ?? Array.Empty<PlacedNpc>();
    public string? SelectedNpcId => session?.NearbyNpcId;
    public PlayerView? Player => session?.Player;
    public int PlayerFrame => session?.PlayerFrame ?? 0;
    public PlayerSpriteCatalog PlayerSprites { get; }
    public IReadOnlyDictionary<string, int> NpcFrames => session?.NpcFrames ?? System.Collections.Frozen.FrozenDictionary<string, int>.Empty;
    public DialoguePresenter? Dialogue { get; private set; }
    public PanoramaCamera TitleCamera { get; } = new();
    public bool UsesTitleBackground => session is null && Screen is Screen.Title or Screen.Options;
    public bool TitleUiVisible => UsesTitleBackground && !TitleCamera.IsMoving;
    public ApplicationFlow(AppSettings? settings = null, Action<AppSettings>? saveSettings = null, NpcContentCatalog? conversations = null,
        CommonNpcCatalog? commonNpcs = null, TitleSayings? titleSayings = null)
    {
        TitleSaying = (titleSayings ?? TitleSayings.Load(
            Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "title-sayings.json"), new Random())).Next();
        Options = new(settings ?? new(), saveSettings, TitleSaying);
        PlayerSprites = new(Path.Combine(AppContext.BaseDirectory, "Content", "Player"));
        this.conversations = conversations ?? NpcContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content", "Npcs"), Path.Combine(AppContext.BaseDirectory, "Content", "World", "shop.npcs.json"));
        // Only the bundled room gets the example by default. Injected authored NPC
        // catalogs retain their own sprites unless explicitly paired with common art.
        this.commonNpcs = commonNpcs ?? (conversations is null
            ? CommonNpcCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content", "CommonNpcs")) : null);
        var sheet = SpriteSheetDefinition.Load(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "sprout-title.json"));
        if (!sheet.Animations.TryGetValue("Idle", out var idle) || !idle.Loop
            || !sheet.Animations.TryGetValue("Interact", out var interact) || interact.Loop)
            throw new InvalidDataException("sprout-title.json: requires looping Idle and non-looping Interact animations.");
        TitleSelector = new(sheet, "Idle");
        OptionsFlight = new(TitleSelector);
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
        if (OptionsFlight.IsBusy)
        {
            if (!active) return;
            if (input.Cancel && !OptionsFlight.IsReturning) { Options.Cancel(); SetScreen(Screen.Title); return; }
            var before = OptionsFlight.Phase;
            OptionsFlight.Update(delta);
            if (!OptionsFlight.IsReturning && before == OptionsSproutPhase.FinishingPoint && OptionsFlight.Phase == OptionsSproutPhase.Flying)
            {
                optionsOrigin = Screen.Title;
                Options.Open();
                SetScreen(Screen.Options, preserveOptionsFlight: true);
            }
            else if (before == OptionsSproutPhase.Flying) TitleCamera.Update(delta);
            return; // Capture input through arrival, including its completion frame.
        }
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
            if (UsesTitleBackground && active) TitleSelector.Update(delta);
            // Capture the visible anchor before Cancel restores draft settings/scale.
            var returnAnchor = OptionsMenuLayout.SelectorAnchor(Options);
            if (Options.Update(input, delta, active))
            {
                if (optionsOrigin == Screen.Title)
                {
                    OptionsFlight.StartReturn(returnAnchor);
                    SetScreen(Screen.Title, preserveOptionsFlight: true);
                    Menu!.Handle(new(SelectionIndex: 2)); // Land beside the main menu's Options entry.
                }
                else SetScreen(optionsOrigin);
            }
            return;
        }
        if (Screen == Screen.Dialogue)
        {
            if (!active) return;
            var presenter = Dialogue ?? throw new InvalidOperationException("Dialogue screen requires a presenter.");
            presenter.Update(input, delta);
            if (presenter.View.Ended)
            {
                // The runtime has already recorded its end reason; this releases session ownership.
                session!.CloseDialogue(DialogueEndReason.Completed);
                Dialogue = null;
                SetScreen(Screen.Room);
            }
            return; // Never apply opening, choice or closing input to the world.
        }
        if (!active)
        {
            if (Screen == Screen.Room)
            {
                if (Options.Current.PauseUnfocused) SetScreen(Screen.Pause);
                else session!.Tick(delta, false); // No movement or interaction from an unfocused application.
            }
            return;
        }
        if (Screen == Screen.Room)
        {
            if (input.Cancel) { SetScreen(Screen.Pause); return; }
            if (input.TalkTargetId is { } target)
            {
                if (TryTalkTo(target) == TalkResult.Started) return;
            }
            if ((input.Interact || input.NextTab) && SelectedNpcId is { } nearby)
            {
                if (TryTalkTo(nearby) == TalkResult.Started) return;
            }
            session!.Tick(delta, false, new(input.MoveX, input.MoveY));
            return;
        }
        if (input.Cancel)
        {
            switch (Screen)
            {
                case Screen.Pause: SetScreen(Screen.Room); break;
                case Screen.ConfirmExit:
                case Screen.ConfirmReturn: SetScreen(confirmationOrigin); break;
                case Screen.CharacterSelect: SetScreen(Screen.Title); break;
                // Escape on title is deliberately harmless; Exit is a separate menu action.
            }
            return;
        }
        if (Screen == Screen.CharacterSelect && input.Left != input.Right)
            input = input with { Up = input.Left, Down = input.Right };
        var command = Menu?.Handle(input);
        if (Screen == Screen.Title && input.Confirm && input.Up == input.Down)
        {
            if (command == MenuCommand.Settings)
            {
                OptionsFlight.Start();
                return;
            }
            // Even unavailable entries react, but only enabled commands are queued.
            pendingTitleCommand = command;
            titleInteractionPlaying = true;
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
                SetScreen(Screen.CharacterSelect);
                break;
            case MenuCommand.SelectMale:
            case MenuCommand.SelectFemale:
                if (session is not null || Screen != Screen.CharacterSelect) throw new InvalidOperationException("Choose a character before starting a session.");
                session = new GameSession(PlayerSprites.PlaceholderSheet, conversations.ShopNpcs,
                    command == MenuCommand.SelectMale ? PlayerGender.Male : PlayerGender.Female, commonNpcs);
                SessionsCreated++;
                SetScreen(Screen.Room);
                break;
            case MenuCommand.BackToTitle: SetScreen(Screen.Title); break;
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
            session.CloseDialogue(DialogueEndReason.Cancelled);
            Dialogue = null;
            Options.Cancel();
            Confirm(Screen.ConfirmExit);
        }
    }

    // NPC interaction entry point. Input adapters submit identity, never a filename or choice index.
    public TalkResult TryTalkTo(string npcId, string? conversationId = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (session is null) return TalkResult.NoSession;
        if (Screen != Screen.Room) return TalkResult.Busy;
        if (!Npcs.Any(n => n.Id == npcId) || !conversations.TryGet(npcId, out var npc)) return TalkResult.UnknownNpc;
        string? id = conversationId ?? npc.DefaultConversation;
        if (id is null) return TalkResult.NoConversation;
        if (!npc.Conversations.TryGetValue(id, out var conversation)) return TalkResult.UnknownConversation;
        var placed = Npcs.First(n => n.Id == npcId);
        if (!session.CanTalkTo(placed)) return TalkResult.OutOfRange;
        Dialogue = new(session.StartDialogue(conversation.Dialogue), Options.Current);
        SetScreen(Screen.Dialogue);
        return TalkResult.Started;
    }

    private void Confirm(Screen target)
    {
        confirmationOrigin = session is null ? Screen.Title : Screen.Pause;
        SetScreen(target);
    }

    private void SetScreen(Screen screen, bool preserveOptionsFlight = false)
    {
        if (!preserveOptionsFlight) CancelTitleInteraction();
        if (session is null)
        {
            if (screen == Screen.Options) TitleCamera.NavigateToOptions();
            else if (screen == Screen.Title) TitleCamera.NavigateToMainMenu();
        }
        Screen = screen;
        Menu = screen switch
        {
            Screen.Title => TitleMenu(),
            Screen.CharacterSelect => new(new("Male", MenuCommand.SelectMale), new("Female", MenuCommand.SelectFemale), new("Back", MenuCommand.BackToTitle)),
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
        titleInteractionPlaying = false;
        OptionsFlight.Reset();
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
        Dialogue = null;
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
