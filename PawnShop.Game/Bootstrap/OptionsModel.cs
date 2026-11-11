using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Game.Screens;

namespace PawnShop.Game.Bootstrap;

// Focus, edits, preview rollback and persistence; no window or device required.
public sealed class OptionsModel
{
    private readonly Action<AppSettings>? save;
    private readonly Button[] bindingButtons;
    private bool requestedButtonClick;
    private bool requestedTabClick;
    private bool requestedSliderClick;
    private bool? requestedCheckboxState;
    private bool requestedDropdownClick;
    private bool closeRequested;
    // Apply, Restore Defaults, Back use a different cue from tabs and sliders.
    public IReadOnlyList<Button> FooterButtons { get; }
    public IReadOnlyList<Button> TabButtons { get; }

    public OptionsModel(AppSettings initial, Action<AppSettings>? save = null,
        string sampleText = "Every treasure deserves a second chance.")
    {
        Current = Draft = initial;
        this.save = save;
        SampleText = sampleText;
        TabButtons = Array.AsReadOnly(Enumerable.Range(0, Tabs.Count)
            .Select(tab => new Button(() => SelectTab(tab), RequestTabClick)).ToArray());
        FooterButtons = Array.AsReadOnly<Button>([
            new(Apply, RequestButtonClick),
            new(RestoreTabDefaults, RequestButtonClick),
            new(() => closeRequested = RequestClose(), RequestButtonClick)
        ]);
        bindingButtons = Enumerable.Range(0, ControlActions.Count * 2)
            .Select(index => new Button(() => BeginCapture(index / 2, index % 2), RequestButtonClick)).ToArray();
    }

    public AppSettings Current { get; private set; }
    public AppSettings Draft { get; private set; }
    public int Tab { get; private set; }
    public int Selected { get; private set; }
    // Sprout stays with the last option while the footer has keyboard/pointer focus.
    public int IndicatorRow { get; private set; }
    public ScrollState Scroll { get; } = new();
    public int? HoveredSelection { get; set; }
    public int? HoveredBinding { get; set; }
    public int? HoveredModalAction { get; set; }
    public int? HoveredTab { get; set; }
    public bool PointerDown { get; set; }
    public bool HoveredScrollbar { get; set; }
    public int SelectedBinding { get; private set; }
    public int? DropdownRow { get; private set; }
    public int DropdownChoice { get; private set; }
    public int DropdownFirst { get; private set; }
    public int? HoveredChoice { get; set; }
    public bool IsDropdown(int row) => (Tab == 0 && row is 0 or 1) || (Tab == 2 && row == 2);
    public int DropdownCount => (Tab, DropdownRow) switch
    {
        (0, 0) => 3, (0, 1) => AppSettings.Resolutions.Count, (2, 2) => 4, _ => 0
    };
    public string ChoiceLabel(int index) => (Tab, DropdownRow) switch
    {
        (0, 0) => ((WindowMode)index).ToString(),
        (0, 1) => $"{AppSettings.Resolutions[index].Width} X {AppSettings.Resolutions[index].Height}",
        (2, 2) => ((DialogueSpeed)index).ToString(),
        _ => throw new InvalidOperationException("No dropdown is open.")
    };
    public string? Message { get; private set; }
    public bool CapturingKey { get; private set; }
    private bool captureReleased;
    public TimeSpan PreviewRemaining { get; private set; }
    public bool Previewing => PreviewRemaining > TimeSpan.Zero;
    public bool IsDirty => Draft != Current;
    public bool ConfirmingDiscard { get; private set; }
    public int DiscardChoice { get; private set; }
    public bool HasModal => Previewing || ConfirmingDiscard;
    public DisplaySettings DisplayForHost => Previewing ? Draft.Display : Current.Display;
    public AppSettings Presentation => Draft;
    public string SampleText { get; }
    public TimeSpan TextElapsed { get; private set; }
    public static IReadOnlyList<string> Tabs { get; } = Array.AsReadOnly(new[] { "DISPLAY", "AUDIO", "COMFORT", "CONTROLS" });
    public static IReadOnlyList<ControlAction> ControlActions { get; } = Array.AsReadOnly(Enum.GetValues<ControlAction>());
    public const int AudioVolumeCount = 6;
    public int FieldCount => Tab switch { 0 => 2, 1 => AudioVolumeCount + 1, 2 => 3, _ => ControlActions.Count };
    public KeyBinding Binding(int row) => Draft.Bindings.Get(ControlActions[row]);
    public Button BindingButton(int row, int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, ControlActions.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 2);
        return bindingButtons[row * 2 + slot];
    }
    public int Volume(int row) => row switch
    {
        0 => Draft.Master, 1 => Draft.Music, 2 => Draft.Ambient, 3 => Draft.Effects,
        4 => Draft.Combat, 5 => Draft.Monster,
        _ => throw new ArgumentOutOfRangeException(nameof(row))
    };
    public static string ActionLabel(ControlAction action) => action switch
    {
        ControlAction.Interact => "INTERACT / ATTACK", ControlAction.PauseBack => "BACK / BLOCK",
        ControlAction.MoveUp => "MOVE UP", ControlAction.MoveLeft => "MOVE LEFT",
        ControlAction.MoveDown => "MOVE DOWN", ControlAction.MoveRight => "MOVE RIGHT",
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
    public static string KeyLabel(Keys key)
    {
        if (key == Keys.None) return "Unbound";
        string name = key.ToString();
        if (name.Length == 1) return name;
        // Turn enum names such as LeftShift and NumPad8 into readable words.
        string words = Regex.Replace(name, "(?<=[a-z])(?=[A-Z0-9])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words.ToLowerInvariant());
    }
    public static string AlternateLabel(KeyBinding binding) => binding.AlternateMouse switch
    {
        MouseBinding.Left => "Left Click", MouseBinding.Right => "Right Click", _ => KeyLabel(binding.Alternate)
    };

    public void Open()
    {
        requestedDropdownClick = false;
        requestedSliderClick = false;
        requestedCheckboxState = null;
        requestedButtonClick = false;
        requestedTabClick = false;
        CloseDropdown();
        Draft = Current;
        Scroll.MoveTo(0);
        HoveredSelection = HoveredTab = null;
        HoveredBinding = null;
        HoveredModalAction = null;
        ConfirmingDiscard = false;
        DiscardChoice = 0;
        HoveredScrollbar = false;
        Tab = Selected = SelectedBinding = IndicatorRow = 0;
        ConfigureScroll();
        CapturingKey = false;
        PreviewRemaining = TimeSpan.Zero;
        TextElapsed = TimeSpan.Zero;
        Message = null;
    }

    public void Cancel()
    {
        requestedDropdownClick = false;
        requestedSliderClick = false;
        requestedCheckboxState = null;
        requestedButtonClick = false;
        requestedTabClick = false;
        CloseDropdown();
        Draft = Current;
        CapturingKey = false;
        PreviewRemaining = TimeSpan.Zero;
        ConfirmingDiscard = false;
        HoveredModalAction = null;
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
        requestedDropdownClick = false;
        requestedSliderClick = false;
        requestedCheckboxState = null;
        ConfigureScroll();
        TextElapsed += delta;
        if (Previewing)
        {
            PreviewRemaining -= delta;
            if (PreviewRemaining <= TimeSpan.Zero || !active || input.Cancel) RejectDisplay(null);
            else if (input.Confirm) Commit();
            return false;
        }
        if (ConfirmingDiscard)
        {
            if (!active) return false;
            if (input.Cancel)
            {
                ConfirmingDiscard = false;
                HoveredModalAction = null;
                return false;
            }
            if (input.ChoiceIndex is { } choice)
            {
                if (choice is not (0 or 1)) return false;
                DiscardChoice = choice;
            }
            else if (input.Left != input.Right || input.Up != input.Down)
                DiscardChoice = 1 - DiscardChoice;
            if (!input.Confirm) return false;
            if (DiscardChoice == 1) { Cancel(); return true; }
            ConfirmingDiscard = false;
            HoveredModalAction = null;
            return false;
        }
        if (!active) { CloseDropdown(); return false; }
        if (CapturingKey) return false;
        if (DropdownRow is not null)
        {
            if (input.Cancel) { CloseDropdown(); return false; }
            if (input.TabIndex is not null || input.NextTab) CloseDropdown();
            else
            {
                UpdateDropdown(input);
                return false; // An open list owns input, including outside clicks.
            }
        }
        if (input.Cancel) return RequestClose();
        if (input.ScrollPosition is { } position) { Scroll.MoveTo(position); return false; }
        if (input.ScrollDelta != 0) { Scroll.MoveTo(Scroll.Offset + input.ScrollDelta); return false; }
        if (input.TabIndex is { } tab && tab >= 0 && tab < Tabs.Count)
        {
            TabButtons[tab].TryClick();
            return false;
        }
        if (input.SelectionIndex is { } selection && selection >= 0 && selection < FieldCount + 3)
        {
            Selected = selection;
            if (selection < FieldCount) IndicatorRow = selection;
        }
        if (Tab == 3 && input.BindingIndex is 0 or 1) SelectedBinding = input.BindingIndex.Value;
        if (Tab == 1 && Selected < AudioVolumeCount && input.SliderValue is { } volume)
        {
            volume = Math.Clamp(volume, 0, 100);
            SetVolume(Selected, volume);
            Message = null;
            return false;
        }
        if (input.NextTab)
        {
            TabButtons[(Tab + 1) % Tabs.Count].TryClick();
            return false;
        }
        if (input.Up != input.Down)
        {
            Selected = (Selected + (input.Down ? 1 : FieldCount + 2)) % (FieldCount + 3);
            if (Selected < FieldCount)
            {
                IndicatorRow = Selected;
                Scroll.Reveal(36 + Selected * OptionsMenuLayout.RowHeight(Draft.UiScale),
                    OptionsMenuLayout.RowHeight(Draft.UiScale), OptionsMenuLayout.Viewport.Height);
            }
            return false;
        }
        if (Selected < FieldCount)
        {
            if (Tab == 3 && input.Left != input.Right)
            {
                SelectedBinding = input.Right ? 1 : 0;
                return false;
            }
            if (IsDropdown(Selected) && input.Confirm)
                OpenDropdown();
            else if (Tab == 3 && input.Confirm)
                BindingButton(Selected, SelectedBinding).TryClick();
            else if (input.Left != input.Right || input.Confirm)
                Adjust(input.Left ? -1 : 1);
        }
        else if (input.Confirm)
        {
            closeRequested = false;
            FooterButtons[Selected - FieldCount].TryClick();
            return closeRequested;
        }
        return false;
    }

    private void Apply()
    {
        if (Draft.Display != Current.Display) PreviewRemaining = TimeSpan.FromSeconds(15);
        else Commit();
    }

    private void SelectTab(int tab)
    {
        Tab = tab;
        ConfigureScroll();
        Scroll.MoveTo(0);
        Selected = SelectedBinding = IndicatorRow = 0;
        Message = null;
    }

    private void BeginCapture(int row, int slot)
    {
        Selected = IndicatorRow = row;
        SelectedBinding = slot;
        CapturingKey = true;
        captureReleased = false;
        Message = "Release keys and mouse, then choose an input. ESC cancels; Delete clears an alternate.";
    }

    private void RequestButtonClick() => requestedButtonClick = true;
    private void RequestTabClick() => requestedTabClick = true;

    public bool TakeTabClick()
    {
        bool result = requestedTabClick;
        requestedTabClick = false;
        return result;
    }

    public bool TakeButtonClick()
    {
        bool result = requestedButtonClick;
        requestedButtonClick = false;
        return result;
    }

    private void RestoreTabDefaults()
    {
        var defaults = new AppSettings();
        Draft = Tab switch
        {
            0 => Draft with { Display = defaults.Display },
            1 => Draft with
            {
                Master = defaults.Master, Music = defaults.Music, Ambient = defaults.Ambient,
                Effects = defaults.Effects, Combat = defaults.Combat, Monster = defaults.Monster,
                MuteUnfocused = defaults.MuteUnfocused
            },
            2 => Draft with
            {
                PauseUnfocused = defaults.PauseUnfocused, ScreenShake = defaults.ScreenShake,
                Dialogue = defaults.Dialogue
            },
            3 => Draft with
            {
                MoveUp = defaults.MoveUp, MoveLeft = defaults.MoveLeft,
                MoveDown = defaults.MoveDown, MoveRight = defaults.MoveRight,
                InteractKey = defaults.InteractKey, InteractAlternateKey = defaults.InteractAlternateKey,
                InteractAlternateMouse = defaults.InteractAlternateMouse,
                PauseKey = defaults.PauseKey, PauseAlternateKey = defaults.PauseAlternateKey,
                PauseAlternateMouse = defaults.PauseAlternateMouse
            },
            _ => throw new InvalidOperationException("Unknown Options tab.")
        };
        if (Tab == 2) TextElapsed = TimeSpan.Zero;
        Message = "Tab defaults selected. Apply to save.";
    }

    private bool RequestClose()
    {
        if (!IsDirty) { Cancel(); return true; }
        ConfirmingDiscard = true;
        DiscardChoice = 0; // Keep Editing is the safe default.
        HoveredModalAction = null;
        return false;
    }

    private void ConfigureScroll() =>
        Scroll.Configure(OptionsMenuLayout.ContentHeight(this), OptionsMenuLayout.Viewport.Height);

    private void OpenDropdown()
    {
        DropdownRow = Selected;
        DropdownChoice = Tab == 2 ? (int)Draft.Dialogue
            : Selected == 0 ? (int)Draft.Display.Mode : Enumerable.Range(0, AppSettings.Resolutions.Count)
            .First(i => AppSettings.Resolutions[i] == (Draft.Display.Width, Draft.Display.Height));
        Scroll.Reveal(36 + Selected * OptionsMenuLayout.RowHeight(Draft.UiScale),
            OptionsMenuLayout.RowHeight(Draft.UiScale), OptionsMenuLayout.Viewport.Height);
        DropdownFirst = 0;
        RevealDropdownChoice();
        requestedDropdownClick = true;
    }

    private void CloseDropdown()
    {
        DropdownRow = null;
        HoveredChoice = null;
    }

    private void RevealDropdownChoice()
    {
        int visible = OptionsMenuLayout.DropdownVisibleCount(this);
        DropdownFirst = Math.Clamp(DropdownFirst, Math.Max(0, DropdownChoice - visible + 1), DropdownChoice);
        DropdownFirst = Math.Min(DropdownFirst, Math.Max(0, DropdownCount - visible));
    }

    private void UpdateDropdown(InputFrame input)
    {
        if (input.ChoiceIndex is { } choice)
        {
            if (choice < 0 || choice >= DropdownCount) return;
            DropdownChoice = choice;
        }
        else if (input.ScrollPosition is { } position)
        {
            int visible = OptionsMenuLayout.DropdownVisibleCount(this);
            DropdownFirst = Math.Clamp(position, 0, Math.Max(0, DropdownCount - visible));
            DropdownChoice = Math.Clamp(DropdownChoice, DropdownFirst, DropdownFirst + visible - 1);
        }
        else if (input.Up != input.Down || input.Left != input.Right)
        {
            int direction = input.Down || input.Right ? 1 : -1;
            DropdownChoice = (DropdownChoice + direction + DropdownCount) % DropdownCount;
            RevealDropdownChoice();
        }
        else if (input.ScrollDelta != 0)
        {
            int visible = OptionsMenuLayout.DropdownVisibleCount(this);
            DropdownFirst = Math.Clamp(DropdownFirst + Math.Sign(input.ScrollDelta), 0, Math.Max(0, DropdownCount - visible));
            DropdownChoice = Math.Clamp(DropdownChoice, DropdownFirst, DropdownFirst + visible - 1);
        }
        if (!input.Confirm) return;
        if (Tab == 2)
        {
            Draft = Draft with { Dialogue = (DialogueSpeed)DropdownChoice };
            TextElapsed = TimeSpan.Zero;
        }
        else if (DropdownRow == 0)
            Draft = Draft with { Display = Draft.Display with { Mode = (WindowMode)DropdownChoice } };
        else
        {
            var size = AppSettings.Resolutions[DropdownChoice];
            Draft = Draft with { Display = Draft.Display with { Width = size.Width, Height = size.Height } };
        }
        Message = null;
        requestedDropdownClick = true;
        CloseDropdown();
    }

    public void Capture(IReadOnlyList<Keys> keys, MouseState mouse = default)
    {
        if (!CapturingKey) return;
        bool left = mouse.LeftButton == ButtonState.Pressed;
        bool right = mouse.RightButton == ButtonState.Pressed;
        if (keys.Count == 0 && !left && !right) { captureReleased = true; return; }
        if (!captureReleased) return;
        if (keys.Contains(Keys.Escape)) { CapturingKey = false; Message = "Binding unchanged."; return; }
        if (left || right)
        {
            if (keys.Count != 0 || left && right) { Message = "Choose one key or mouse button."; return; }
            var mouseAction = ControlActions[Selected];
            if (SelectedBinding != 1 || mouseAction is not (ControlAction.Interact or ControlAction.PauseBack))
            {
                Message = "Mouse buttons are supported in the two action alternates.";
                return;
            }
            var button = left ? MouseBinding.Left : MouseBinding.Right;
            foreach (var other in ControlActions)
                if (other != mouseAction && Draft.Bindings.Get(other).AlternateMouse == button)
                {
                    Message = $"Already used by {ActionLabel(other)} (ALTERNATE).";
                    return;
                }
            Draft = Draft.WithMouseBinding(mouseAction, button);
            CapturingKey = false;
            Message = "Binding changed. Apply to save.";
            return;
        }
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

    // null means no toggle; false is an explicit request for the off sound.
    public bool? TakeCheckboxSwitch()
    {
        var result = requestedCheckboxState;
        requestedCheckboxState = null;
        return result;
    }

    public bool TakeSliderClick()
    {
        bool result = requestedSliderClick;
        requestedSliderClick = false;
        return result;
    }

    public bool TakeDropdownClick()
    {
        bool result = requestedDropdownClick;
        requestedDropdownClick = false;
        return result;
    }

    public void RejectDisplay(string? reason)
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
            Message = null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            RejectDisplay("Could not save options. Previous settings retained.");
        }
    }

    private void SetVolume(int row, int value)
    {
        value = Math.Clamp(value, 0, 100);
        if (value == Volume(row)) return;
        Draft = row switch
        {
            0 => Draft with { Master = value }, 1 => Draft with { Music = value },
            2 => Draft with { Ambient = value }, 3 => Draft with { Effects = value },
            4 => Draft with { Combat = value }, 5 => Draft with { Monster = value },
            _ => throw new ArgumentOutOfRangeException(nameof(row))
        };
        // Pointer input is already quantized to whole percentages. The audio
        // cooldown limits rapid changes without silencing small adjustments.
        requestedSliderClick = true;
    }

    private void Adjust(int direction)
    {
        int Cycle(int value, int count) => (value + direction + count) % count;
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
            }
        }
        else if (Tab == 1)
        {
            if (Selected < AudioVolumeCount) SetVolume(Selected, Volume(Selected) + direction * 5);
            else
            {
                Draft = Draft with { MuteUnfocused = !Draft.MuteUnfocused };
                requestedCheckboxState = Draft.MuteUnfocused;
            }
        }
        else if (Tab == 2)
        {
            switch (Selected)
            {
                case 0:
                    Draft = Draft with { PauseUnfocused = !Draft.PauseUnfocused };
                    requestedCheckboxState = Draft.PauseUnfocused;
                    break;
                case 1:
                    Draft = Draft with { ScreenShake = !Draft.ScreenShake };
                    requestedCheckboxState = Draft.ScreenShake;
                    break;
                case 2: Draft = Draft with { Dialogue = (DialogueSpeed)Cycle((int)Draft.Dialogue, 4) }; TextElapsed = TimeSpan.Zero; break;
            }
        }
        if (IsDropdown(Selected)) requestedDropdownClick = true;
        Message = null;
    }

    public IReadOnlyList<(string Label, string Value)> Fields => Tab switch
    {
        0 => new[] { ("WINDOW MODE", Draft.Display.Mode.ToString()),
            ("RESOLUTION", $"{Draft.Display.Width} X {Draft.Display.Height}") },
        1 => new[] { ("MASTER", $"{Draft.Master}"), ("MUSIC", $"{Draft.Music}"),
            ("AMBIENT", $"{Draft.Ambient}"), ("SOUND EFFECTS", $"{Draft.Effects}"),
            ("COMBAT", $"{Draft.Combat}"), ("MONSTER", $"{Draft.Monster}"),
            ("MUTE WHEN UNFOCUSED", Draft.MuteUnfocused ? "ON" : "OFF") },
        2 => new[] { ("PAUSE WHEN UNFOCUSED", Draft.PauseUnfocused ? "ON" : "OFF"),
            ("SCREEN SHAKE", Draft.ScreenShake ? "ON" : "OFF"), ("DIALOGUE TEXT SPEED", Draft.Dialogue.ToString()) },
        _ => ControlActions.Select(action => (ActionLabel(action), KeyLabel(Draft.Bindings.Get(action).Primary))).ToArray()
    };
}
