using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Game.Screens;

namespace PawnShop.Game.Bootstrap;

// Focus, edits, preview rollback and persistence; no window or device required.
public sealed class OptionsModel(AppSettings initial, Action<AppSettings>? save = null)
{
    public AppSettings Current { get; private set; } = initial;
    public AppSettings Draft { get; private set; } = initial;
    public int Tab { get; private set; }
    public int Selected { get; private set; }
    public ScrollState Scroll { get; } = new();
    public int? HoveredSelection { get; set; }
    public int? HoveredTab { get; set; }
    public bool PointerDown { get; set; }
    public int SelectedBinding { get; private set; }
    public string? Message { get; private set; }
    public bool CapturingKey { get; private set; }
    private bool captureReleased;
    public TimeSpan PreviewRemaining { get; private set; }
    public bool Previewing => PreviewRemaining > TimeSpan.Zero;
    public DisplaySettings DisplayForHost => Previewing ? Draft.Display : Current.Display;
    public AppSettings Presentation => Draft;
    public const string SampleText = "Every treasure deserves a second chance.";
    public TimeSpan TextElapsed { get; private set; }
    public AudioChannel? RequestedTone { get; private set; }
    public static IReadOnlyList<string> Tabs { get; } = Array.AsReadOnly(new[] { "DISPLAY", "AUDIO", "COMFORT", "CONTROLS" });
    public static IReadOnlyList<ControlAction> ControlActions { get; } = Array.AsReadOnly(Enum.GetValues<ControlAction>());
    public int FieldCount => Tab switch { 0 => 3, 1 => 5, 2 => 3, _ => ControlActions.Count };
    public KeyBinding Binding(int row) => Draft.Bindings.Get(ControlActions[row]);
    public static string ActionLabel(ControlAction action) => action switch
    {
        ControlAction.Interact => "INTERACT", ControlAction.PauseBack => "PAUSE / BACK",
        ControlAction.MoveUp => "MOVE UP", ControlAction.MoveLeft => "MOVE LEFT",
        ControlAction.MoveDown => "MOVE DOWN", ControlAction.MoveRight => "MOVE RIGHT",
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
    public static string KeyLabel(Keys key) => key == Keys.None ? "UNBOUND" : key.ToString().ToUpperInvariant();

    public void Open()
    {
        Draft = Current;
        Scroll.MoveTo(0);
        HoveredSelection = HoveredTab = null;
        Tab = Selected = SelectedBinding = 0;
        ConfigureScroll();
        CapturingKey = false;
        PreviewRemaining = TimeSpan.Zero;
        TextElapsed = TimeSpan.Zero;
        Message = null;
    }

    public void Cancel()
    {
        Draft = Current;
        CapturingKey = false;
        PreviewRemaining = TimeSpan.Zero;
    }

    public void Report(string message) => Message = message;

    // Device failure at startup retains the recoverable file until the user saves.
    public void UseSafeDisplay()
    {
        Current = Current with { Display = new(1280, 720) };
        Draft = Draft with { Display = Current.Display };
        PreviewRemaining = TimeSpan.Zero;
    }

    public bool Update(InputFrame input, TimeSpan delta, bool active)
    {
        ConfigureScroll();
        TextElapsed += delta;
        if (Previewing)
        {
            PreviewRemaining -= delta;
            if (PreviewRemaining <= TimeSpan.Zero || !active || input.Cancel) RejectDisplay("Display changes reverted.");
            else if (input.Confirm) Commit();
            return false;
        }
        if (!active || CapturingKey) return false;
        if (input.Cancel) { Cancel(); return true; }
        if (input.ScrollPosition is { } position) { Scroll.MoveTo(position); return false; }
        if (input.ScrollDelta != 0) { Scroll.MoveTo(Scroll.Offset + input.ScrollDelta); return false; }
        if (input.TabIndex is { } tab && tab >= 0 && tab < Tabs.Count)
        {
            Tab = tab;
            ConfigureScroll();
            Scroll.MoveTo(0);
            Selected = SelectedBinding = 0;
            Message = null;
            return false;
        }
        if (input.SelectionIndex is { } selection && selection >= 0 && selection < FieldCount + 3)
            Selected = selection;
        if (Tab == 3 && input.BindingIndex is 0 or 1) SelectedBinding = input.BindingIndex.Value;
        if (input.NextTab)
        {
            Tab = (Tab + 1) % Tabs.Count;
            ConfigureScroll();
            Scroll.MoveTo(0);
            Selected = SelectedBinding = 0;
            Message = null;
            return false;
        }
        if (input.Up != input.Down)
        {
            Selected = (Selected + (input.Down ? 1 : FieldCount + 2)) % (FieldCount + 3);
            if (Selected < FieldCount)
                Scroll.Reveal(36 + Selected * OptionsMenuLayout.RowHeight(Draft.UiScale),
                    OptionsMenuLayout.RowHeight(Draft.UiScale), OptionsMenuLayout.Viewport.Height);
            return false;
        }
        if (Selected < FieldCount)
        {
            if (Tab == 3 && input.Left != input.Right)
            {
                SelectedBinding = input.Right ? 1 : 0;
                return false;
            }
            if (Tab == 3 && input.Confirm)
            {
                CapturingKey = true;
                captureReleased = false;
                Message = "Release keys, then press a new key. ESC cancels; Delete clears an alternate.";
            }
            else if (input.Left != input.Right || input.Confirm)
                Adjust(input.Left ? -1 : 1);
        }
        else if (input.Confirm)
        {
            switch (Selected - FieldCount)
            {
                case 0:
                    if (Draft.Display != Current.Display) PreviewRemaining = TimeSpan.FromSeconds(15);
                    else Commit();
                    break;
                case 1: Draft = new(); TextElapsed = TimeSpan.Zero; Message = "Defaults selected. Apply to save."; break;
                case 2: Cancel(); return true;
            }
        }
        return false;
    }

    private void ConfigureScroll() =>
        Scroll.Configure(OptionsMenuLayout.ContentHeight(this), OptionsMenuLayout.Viewport.Height);

    public void Capture(IReadOnlyList<Keys> keys)
    {
        if (!CapturingKey) return;
        if (keys.Count == 0) { captureReleased = true; return; }
        if (!captureReleased) return;
        if (keys.Contains(Keys.Escape)) { CapturingKey = false; Message = "Binding unchanged."; return; }
        if (keys.Count != 1 || !AppSettings.ValidKey(keys[0])) { Message = "Choose one supported key."; return; }
        Keys key = keys[0];
        var action = ControlActions[Selected];
        if (key is Keys.Back or Keys.Delete)
        {
            if (SelectedBinding == 0) { Message = "A primary key is required. Select an alternate to clear it."; return; }
            key = Keys.None;
        }
        if (!AppSettings.AllowedFor(action, key))
        {
            Message = "That key is reserved for menu confirmation, back or category navigation.";
            return;
        }
        foreach (var other in ControlActions)
        {
            var binding = Draft.Bindings.Get(other);
            for (int slot = 0; slot < 2; slot++)
            {
                if (other == action && slot == SelectedBinding) continue;
                if (key != Keys.None && key == (slot == 0 ? binding.Primary : binding.Alternate))
                {
                    Message = $"Already used by {ActionLabel(other)} ({(slot == 0 ? "PRIMARY" : "ALTERNATE")}).";
                    return;
                }
            }
        }
        Draft = Draft.WithBinding(action, SelectedBinding, key);
        CapturingKey = false;
        Message = "Binding changed. Apply to save.";
    }

    public AudioChannel? TakeTone()
    {
        var result = RequestedTone;
        RequestedTone = null;
        return result;
    }

    public void RejectDisplay(string reason)
    {
        PreviewRemaining = TimeSpan.Zero;
        Draft = Draft with { Display = Current.Display };
        Message = reason;
    }

    private void Commit()
    {
        try
        {
            save?.Invoke(Draft);
            Current = Draft;
            PreviewRemaining = TimeSpan.Zero;
            Message = "Options saved.";
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            RejectDisplay("Could not save options. Previous settings retained.");
        }
    }

    private void Adjust(int direction)
    {
        int Cycle(int value, int count) => (value + direction + count) % count;
        int Volume(int value) => Math.Clamp(value + direction * 5, 0, 100);
        if (Tab == 0)
        {
            switch (Selected)
            {
                case 0: Draft = Draft with { Display = Draft.Display with { Mode = (WindowMode)Cycle((int)Draft.Display.Mode, 3) } }; break;
                case 1:
                    var choices = AppSettings.Resolutions;
                    int index = Enumerable.Range(0, choices.Count).First(i => choices[i] == (Draft.Display.Width, Draft.Display.Height));
                    var size = choices[Cycle(index, choices.Count)];
                    Draft = Draft with { Display = Draft.Display with { Width = size.Width, Height = size.Height } };
                    break;
                case 2: Draft = Draft with { UiScale = 100 + 25 * Cycle((Draft.UiScale - 100) / 25, 3) }; break;
            }
        }
        else if (Tab == 1)
        {
            Draft = Selected switch
            {
                0 => Draft with { Master = Volume(Draft.Master) },
                1 => Draft with { Music = Volume(Draft.Music) },
                2 => Draft with { Ambient = Volume(Draft.Ambient) },
                3 => Draft with { Effects = Volume(Draft.Effects) },
                _ => Draft with { MuteUnfocused = !Draft.MuteUnfocused }
            };
            RequestedTone = Selected switch { 1 => AudioChannel.Music, 2 => AudioChannel.Ambient, _ => AudioChannel.Effects };
        }
        else if (Tab == 2)
        {
            switch (Selected)
            {
                case 0: Draft = Draft with { PauseUnfocused = !Draft.PauseUnfocused }; break;
                case 1: Draft = Draft with { Dialogue = (DialogueSpeed)Cycle((int)Draft.Dialogue, 4) }; TextElapsed = TimeSpan.Zero; break;
                case 2: TextElapsed = TimeSpan.Zero; break;
            }
        }
        Message = null;
    }

    public IReadOnlyList<(string Label, string Value)> Fields => Tab switch
    {
        0 => new[] { ("WINDOW MODE", Draft.Display.Mode.ToString()),
            ("RESOLUTION", $"{Draft.Display.Width} X {Draft.Display.Height}"), ("UI / TEXT SIZE", $"{Draft.UiScale} PERCENT") },
        1 => new[] { ("MASTER", $"{Draft.Master}"), ("MUSIC", $"{Draft.Music}"),
            ("AMBIENT", $"{Draft.Ambient}"), ("SOUND EFFECTS", $"{Draft.Effects}"), ("MUTE WHEN UNFOCUSED", Draft.MuteUnfocused ? "ON" : "OFF") },
        2 => new[] { ("PAUSE WHEN UNFOCUSED", Draft.PauseUnfocused ? "ON" : "OFF"),
            ("DIALOGUE TEXT SPEED", Draft.Dialogue.ToString()), ("REPLAY TEXT PREVIEW", "ENTER") },
        _ => ControlActions.Select(action => (ActionLabel(action), KeyLabel(Draft.Bindings.Get(action).Primary))).ToArray()
    };
}
