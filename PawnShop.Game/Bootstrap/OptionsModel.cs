using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;

namespace PawnShop.Game.Bootstrap;

// Focus, edits, preview rollback and persistence; no window or device required.
public sealed class OptionsModel(AppSettings initial, Action<AppSettings>? save = null)
{
    public AppSettings Current { get; private set; } = initial;
    public AppSettings Draft { get; private set; } = initial;
    public int Tab { get; private set; }
    public int Selected { get; private set; }
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
    public int FieldCount => Tab switch { 0 => 3, 1 => 5, 2 => 3, _ => 2 };

    public void Open()
    {
        Draft = Current;
        Tab = Selected = 0;
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
        if (input.TabIndex is { } tab && tab >= 0 && tab < Tabs.Count)
        {
            Tab = tab;
            Selected = 0;
            Message = null;
            return false;
        }
        if (input.SelectionIndex is { } selection && selection >= 0 && selection < FieldCount + 3)
            Selected = selection;
        if (input.NextTab)
        {
            Tab = (Tab + 1) % Tabs.Count;
            Selected = 0;
            Message = null;
            return false;
        }
        if (input.Up != input.Down)
        {
            Selected = (Selected + (input.Down ? 1 : FieldCount + 2)) % (FieldCount + 3);
            return false;
        }
        if (Selected < FieldCount)
        {
            if (Tab == 3 && input.Confirm)
            {
                CapturingKey = true;
                captureReleased = false;
                Message = "Release keys, then press a new key. ESC cancels.";
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

    public void Capture(IReadOnlyList<Keys> keys)
    {
        if (!CapturingKey) return;
        if (keys.Count == 0) { captureReleased = true; return; }
        if (!captureReleased) return;
        if (keys.Contains(Keys.Escape)) { CapturingKey = false; Message = "Binding unchanged."; return; }
        if (keys.Count != 1 || !AppSettings.ValidKey(keys[0])) { Message = "Choose one supported key."; return; }
        Keys key = keys[0];
        if (key == (Selected == 0 ? Draft.PauseKey : Draft.InteractKey))
        {
            Message = "That key is already used by the other action.";
            return;
        }
        Draft = Selected == 0 ? Draft with { InteractKey = key } : Draft with { PauseKey = key };
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
        _ => new[] { ("INTERACT", Draft.InteractKey.ToString()), ("PAUSE", Draft.PauseKey.ToString()) }
    };
}
