using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework.Input;
using PawnShop.Audio;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void CheckboxSwitchFeedback()
    {
        foreach (var (tab, row) in new[] { (1, 6), (2, 0), (2, 1) })
        {
            var model = new OptionsModel(new());
            bool Value() => tab == 1 ? model.Draft.MuteUnfocused : row == 0 ? model.Draft.PauseUnfocused : model.Draft.ScreenShake;
            Check(model.TakeCheckboxSwitch() is null);
            model.Open();
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            model.Update(new(SelectionIndex: row), TimeSpan.Zero, true);
            Check(Value() && model.TakeCheckboxSwitch() is null);
            foreach (var input in new InputFrame[] { new(Confirm: true), new(Left: true), new(Right: true), new(Confirm: true) })
            {
                bool previous = Value();
                model.Update(input, TimeSpan.Zero, true);
                Check(Value() != previous && model.TakeCheckboxSwitch() == Value());
                Check(model.TakeCheckboxSwitch() is null && !model.TakeSliderClick() && !model.TakeButtonClick());
            }
            model.Update(new(Confirm: true), TimeSpan.Zero, false);
            Check(Value() && model.TakeCheckboxSwitch() is null, "Inactive controls must not toggle or sound.");
            model.Update(new(Confirm: true), TimeSpan.Zero, true);
            model.Cancel();
            Check(Value() && model.TakeCheckboxSwitch() is null, "Discarding settings is silent.");
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
            model.TakeCheckboxSwitch();
            model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
            Check(Value() && model.TakeCheckboxSwitch() is null, "Restore Defaults is silent for switches.");
            model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
            Check(model.TakeCheckboxSwitch() is null, "Apply must not replay switch feedback.");
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
            model.Update(new(Cancel: true), TimeSpan.Zero, true);
            Check(model.ConfirmingDiscard && model.TakeCheckboxSwitch() is null);
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
            Check(model.TakeCheckboxSwitch() is null, "Modal input must not reach a checkbox.");

            using var app = new ApplicationFlow();
            OpenOptions(app); FinishOptionsFlight(app);
            app.Update(new(TabIndex: tab), TimeSpan.Zero);
            app.Options.Scroll.MoveTo(app.Options.Scroll.Maximum);
            var point = OptionsMenuLayout.Checkbox(row, app.Options).Center;
            var pointer = new MenuPointerInput();
            InputFrame Read(bool down) => pointer.Read(app, new MouseState(point.X, point.Y, 0,
                down ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720);
            Read(false);
            app.Update(Read(true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is false, "Pointer uncheck must select off, not be mistaken for no request.");
            app.Update(Read(true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is null, "Held pointer must not repeat the switch.");
            Read(false);
            app.Update(Read(true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is true);
            var keyboard = new PawnShop.Framework.Input.KeyboardInput();
            keyboard.Read(new KeyboardState(), true);
            app.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is false);
            app.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is null, "Held confirmation must not repeat the switch.");
            keyboard.Read(new KeyboardState(), true);
            app.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero);
            Check(app.Options.TakeCheckboxSwitch() is true);
        }
        var muted = new OptionsModel(new() { Effects = 0 });
        muted.Open();
        muted.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        muted.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(muted.TakeCheckboxSwitch() is false && muted.Presentation.AudioMix.Gain(AudioChannel.Effects, true) == 0);
        muted.Update(new(), TimeSpan.Zero, true);
        Check(muted.TakeCheckboxSwitch() is null, "Muted requests must not be queued.");
    }

    internal static void CheckboxSwitchContent()
    {
        CheckClickContent("switch_on", "B4239F1FA57B1D9747E2E000E5870812DAF306CB7FD29DE2D9C868AE9FD889BF");
        CheckClickContent("switch_off", "3FD1940FFF0FC29798AAEFB46A856402C959055A2145E6460BDA513C47CFA30E");
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string ui = Path.Combine(root, "PawnShop.Audio/Content/UI");
        string sources = Path.Combine(ui, "Sources/OnOffSwitch");
        Check(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(sources, "874829__cb9382__onoff-switch.flac"))))
            == "04CDC7E8E251E575D795A53FD624937830DE28435E87DE051FFE02788188115E");
        byte[] full = ReadSwitchWave(Path.Combine(sources, "onoff-switch.wav"));
        Check(full.Length == 80800 * 4);
        foreach (var (name, start, frames) in new[] { ("switch_on", 13056, 4224), ("switch_off", 57024, 4800) })
        {
            byte[] pcm = ReadSwitchWave(Path.Combine(ui, name + ".wav"));
            Check(pcm.Length == frames * 4);
            // Compare the unfaded middle to its exact source window to prove order
            // and preserve the first/on and second/off attacks without synthesis.
            Check(pcm.AsSpan(48 * 4, (frames - 48 - 384) * 4)
                .SequenceEqual(full.AsSpan((start + 48) * 4, (frames - 48 - 384) * 4)));
            Check(BitConverter.ToInt16(pcm, 0) == 0 && BitConverter.ToInt16(pcm, pcm.Length - 2) == 0);
            double peak = Enumerable.Range(0, pcm.Length / 2).Max(i => Math.Abs((double)BitConverter.ToInt16(pcm, i * 2)) / 32768);
            Check(peak > .9 && peak < .99);
        }
    }

    private static byte[] ReadSwitchWave(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        Check(Encoding.ASCII.GetString(reader.ReadBytes(4)) == "RIFF");
        Check(reader.ReadUInt32() == reader.BaseStream.Length - 8 && Encoding.ASCII.GetString(reader.ReadBytes(4)) == "WAVE");
        bool validFormat = false;
        byte[] pcm = [];
        while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
        {
            string chunk = Encoding.ASCII.GetString(reader.ReadBytes(4));
            uint size = reader.ReadUInt32();
            long end = reader.BaseStream.Position + size;
            if (chunk == "fmt ")
            {
                Check(reader.ReadUInt16() == 1 && reader.ReadUInt16() == 2 && reader.ReadInt32() == 48000);
                Check(reader.ReadInt32() == 192000 && reader.ReadUInt16() == 4 && reader.ReadUInt16() == 16);
                validFormat = true;
            }
            else if (chunk == "data") pcm = reader.ReadBytes((int)size);
            reader.BaseStream.Position = end + (size & 1);
        }
        Check(validFormat && pcm.Length > 0 && pcm.Length % 4 == 0);
        return pcm;
    }
}
