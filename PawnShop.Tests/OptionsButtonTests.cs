using PawnShop.Audio;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionalButtonSound()
    {
        var calls = new List<string>();
        var silent = new Button(() => calls.Add("action"));
        Check(silent.TryClick() && calls.SequenceEqual(new[] { "action" }));
        calls.Clear();
        var audible = new Button(() => calls.Add("action"), onClickSound: () => calls.Add("sound"));
        audible.Enabled = false;
        Check(!audible.TryClick() && calls.Count == 0, "Disabled buttons must neither act nor request audio.");
        audible.Enabled = true;
        Check(audible.TryClick() && calls.SequenceEqual(new[] { "action", "sound" }));
        calls.Clear();
        Check(audible.TryClick() && calls.Count == 2, "Each activation should request exactly one click.");
    }

    internal static void OptionsButtonSounds()
    {
        for (int tab = 0; tab < 4; tab++)
            for (int action = 0; action < 3; action++)
            {
                var model = new OptionsModel(new());
                model.Open();
                model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
                var activate = new InputFrame(SelectionIndex: model.FieldCount + action, Confirm: true);
                model.FooterButtons[action].Enabled = false;
                var before = model.Draft;
                Check(!model.Update(activate, TimeSpan.Zero, true));
                Check(model.Draft == before && !model.TakeButtonClick());
                model.FooterButtons[action].Enabled = true;
                Check(model.Update(activate, TimeSpan.Zero, true) == (action == 2));
                Check(model.TakeButtonClick() && !model.TakeButtonClick(), "Footer activation must emit one click.");
            }

        for (int row = 0; row < OptionsModel.ControlActions.Count; row++)
            for (int slot = 0; slot < 2; slot++)
            {
                var model = new OptionsModel(new());
                model.Open();
                model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
                var activate = new InputFrame(SelectionIndex: row, BindingIndex: slot, Confirm: true);
                model.BindingButton(row, slot).Enabled = false;
                model.Update(activate, TimeSpan.Zero, true);
                Check(!model.CapturingKey && !model.TakeButtonClick());
                model.BindingButton(row, slot).Enabled = true;
                model.Update(activate, TimeSpan.Zero, false);
                Check(!model.CapturingKey && !model.TakeButtonClick());
                model.Update(activate, TimeSpan.Zero, true);
                Check(model.CapturingKey && model.Selected == row && model.SelectedBinding == slot);
                Check(model.TakeButtonClick() && !model.TakeButtonClick());
                model.Update(activate, TimeSpan.Zero, true);
                Check(!model.TakeButtonClick(), "Key capture must block further activations.");
                model.Capture(Array.Empty<Keys>());
                model.Capture(new[] { Keys.Escape });
                Check(!model.CapturingKey && !model.TakeButtonClick());
            }

        var options = new OptionsModel(new());
        options.Open();
        options.Update(new(Confirm: true), TimeSpan.Zero, true); // Open Display dropdown.
        options.Update(new(ChoiceIndex: 1, Confirm: true), TimeSpan.Zero, true);
        Check(!options.TakeButtonClick() && options.TakeDropdownClick(), "Dropdowns use their own cue.");
        options.Update(new(SelectionIndex: options.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(options.Previewing && options.TakeButtonClick());
        options.Update(new(Confirm: true), TimeSpan.Zero, true); // Keep display preview.
        Check(!options.TakeButtonClick(), "Confirmation buttons are not opted in.");
        options.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        options.Update(new(SelectionIndex: 0, SliderValue: 40), TimeSpan.Zero, true);
        Check(options.TakeSliderClick() && options.TakeCheckboxSwitch() is null && !options.TakeButtonClick());
        options.Update(new(SelectionIndex: OptionsModel.AudioVolumeCount, Confirm: true), TimeSpan.Zero, true);
        Check(!options.TakeButtonClick() && !options.TakeSliderClick() && options.TakeCheckboxSwitch() is false,
            "Unchecking uses the off-switch cue instead of button or slider feedback.");
        options.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        options.Update(new(SelectionIndex: 0, Confirm: true), TimeSpan.Zero, true);
        options.Update(new(ScrollDelta: 10), TimeSpan.Zero, true);
        Check(!options.TakeButtonClick());
        options.Update(new(SelectionIndex: options.FieldCount + 2, Confirm: true), TimeSpan.Zero, true);
        Check(options.ConfirmingDiscard && options.TakeButtonClick());
        options.Update(new(SelectionIndex: options.FieldCount + 1, Confirm: true, ChoiceIndex: 0), TimeSpan.Zero, true);
        Check(!options.ConfirmingDiscard && !options.TakeButtonClick(), "Modals must block underlying buttons.");
        options.Update(new(SelectionIndex: options.FieldCount, Confirm: true), TimeSpan.Zero, false);
        Check(!options.TakeButtonClick());

        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        var pointer = new MenuPointerInput();
        InputFrame Read(int x, int y, bool down) => pointer.Read(app,
            new MouseState(x, y, 0, down ? ButtonState.Pressed : ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
            true, 1280, 720, 1280, 720);
        var footer = OptionsMenuLayout.Footer(1).Center;
        Read(footer.X, footer.Y, false);
        app.Update(Read(footer.X, footer.Y, true), TimeSpan.Zero);
        Check(app.Options.TakeButtonClick());
        app.Update(Read(footer.X, footer.Y, true), TimeSpan.Zero);
        Check(!app.Options.TakeButtonClick(), "Holding the pointer must not replay clicks.");
        Read(footer.X, footer.Y, false);
        app.Update(new(TabIndex: 3), TimeSpan.Zero);
        var cell = OptionsMenuLayout.Cell(0, 1, app.Options).Center;
        app.Update(Read(cell.X, cell.Y, true), TimeSpan.Zero);
        Check(app.Options.SelectedBinding == 1 && app.Options.CapturingKey && app.Options.TakeButtonClick());
        app.Options.Cancel();
        app.Update(new(SelectionIndex: app.Options.FieldCount + 2, Confirm: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Title && app.Options.TakeButtonClick(), "Back audio must survive closing Options.");
        app.Options.Open();
        Check(!app.Options.TakeButtonClick(), "Reopening must not retain stale requests.");
    }

    internal static void ButtonClickContent()
    {
        Check(AudioAssets.ButtonClick == "Audio/UI/sub_click");
        CheckClickContent("sub_click", "9375936391B2EA1F51875429A2F1DB607C8DC51966E2ACAAB9260D757AAAD807");
    }

    internal static void TabClickContent()
    {
        Check(AudioAssets.TabClick == "Audio/UI/tab_click");
        CheckClickContent("tab_click", "6F785180AE5DE592A5D8DF3A4F5A822A79B3DC6A7CA6360E15BFC01AAE671F9A");
    }

    private static void CheckClickContent(string name, string hash)
    {
        string output = Path.Combine(AppContext.BaseDirectory, "Content", "Audio", "UI");
        string compiled = Path.Combine(output, name + ".xnb");
        using var xnb = File.OpenRead(compiled);
        Span<byte> header = stackalloc byte[3];
        xnb.ReadExactly(header);
        Check(header.SequenceEqual("XNB"u8));
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        using var source = File.OpenRead(Path.Combine(root, "PawnShop.Audio/Content/UI", name + ".wav"));
        Check(Convert.ToHexString(SHA256.HashData(source)) == hash);
        Check(!File.Exists(Path.Combine(output, name + ".wav")), "Ship compiled content, not the source WAV.");
    }

    internal static void OptionsTabSounds()
    {
        var model = new OptionsModel(new());
        model.Open();
        Check(!model.TakeTabClick(), "Opening Options must not sound like clicking Display.");
        for (int tab = 0; tab < OptionsModel.Tabs.Count; tab++)
        {
            int before = model.Tab;
            model.TabButtons[tab].Enabled = false;
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            Check(model.Tab == before && !model.TakeTabClick());
            model.TabButtons[tab].Enabled = true;
            model.Update(new(TabIndex: tab), TimeSpan.Zero, false);
            Check(model.Tab == before && !model.TakeTabClick());
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            Check(model.Tab == tab && model.TakeTabClick() && !model.TakeTabClick());
            Check(!model.TakeButtonClick() && model.TakeCheckboxSwitch() is null);
        }
        var edges = new InputEdges();
        model.Update(edges.Read(InputButtons.NextTab), TimeSpan.Zero, true);
        Check(model.Tab == 0 && model.TakeTabClick(), "Keyboard Tab should use the same cue and wrap to Display.");
        model.Update(edges.Read(InputButtons.NextTab), TimeSpan.Zero, true);
        Check(model.Tab == 0 && !model.TakeTabClick(), "Held Tab must not repeat.");
        model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
        Check(model.TakeTabClick());
        model.Update(new(SelectionIndex: 0, BindingIndex: 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.TakeButtonClick() && !model.TakeTabClick());
        model.Update(new(TabIndex: 0, NextTab: true), TimeSpan.Zero, true);
        Check(model.Tab == 3 && !model.TakeTabClick(), "Key capture blocks tab buttons.");
        model.Capture(Array.Empty<Keys>());
        model.Capture(new[] { Keys.Escape });
        model.Open();
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(model.DropdownRow is not null && !model.TakeTabClick());
        model.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        Check(model.Tab == 2 && model.DropdownRow is null && model.TakeTabClick());
        model.Update(new(SelectionIndex: 0, Confirm: true), TimeSpan.Zero, true);
        Check(!model.TakeTabClick() && !model.TakeButtonClick());
        model.Update(new(SelectionIndex: model.FieldCount + 2, Confirm: true), TimeSpan.Zero, true);
        Check(model.ConfirmingDiscard && model.TakeButtonClick());
        model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        Check(model.Tab == 2 && !model.TakeTabClick(), "Discard confirmation blocks tabs.");
        model.Cancel();
        model.Open();
        model.Update(new(SelectionIndex: 0, Right: true), TimeSpan.Zero, true);
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(model.Previewing && model.TakeButtonClick());
        model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
        Check(model.Tab == 0 && !model.TakeTabClick(), "Display confirmation blocks tabs.");
        model.Cancel();
        model.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        Check(model.TakeTabClick());
        model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        model.Cancel();
        Check(!model.TakeTabClick(), "Closing clears stale tab requests.");

        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        Check(!app.Options.TakeTabClick(), "The entry flight must not emit a tab click.");
        var pointer = new MenuPointerInput();
        InputFrame Read(bool down, bool active = true) => pointer.Read(app,
            new MouseState(OptionsMenuLayout.Tab(1).Center.X, OptionsMenuLayout.Tab(1).Center.Y, 0,
                down ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released),
            active, 1280, 720, 1280, 720);
        Read(false);
        app.Update(Read(true), TimeSpan.Zero);
        Check(app.Options.Tab == 1 && app.Options.TakeTabClick() && !app.Options.TakeButtonClick());
        app.Update(Read(true), TimeSpan.Zero);
        Check(!app.Options.TakeTabClick());
        Read(true, active: false);
        app.Update(Read(true), TimeSpan.Zero);
        Check(!app.Options.TakeTabClick(), "Returning focus with a held pointer must stay silent.");
    }
}
