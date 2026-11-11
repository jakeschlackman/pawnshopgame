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
    internal static void RotaryClickBags()
    {
        foreach (int count in new[] { 1, 2, AudioAssets.RotaryClicks.Count })
            for (int seed = 0; seed < 32; seed++)
            {
                var sequence = new SliderClickSequence(count, new Random(seed));
                var replay = new SliderClickSequence(count, new Random(seed));
                int last = -1;
                var pitches = new HashSet<float>();
                for (int cycle = 0; cycle < 20; cycle++)
                {
                    var seen = new HashSet<int>();
                    for (int i = 0; i < count; i++)
                    {
                        var click = sequence.Update(SliderClickSequence.Cooldown, true);
                        Check(click is not null && click == replay.Update(SliderClickSequence.Cooldown, true));
                        var (sample, pitch) = click.GetValueOrDefault();
                        Check(sample >= 0 && sample < count && seen.Add(sample));
                        Check(count == 1 || sample != last, "No immediate repeat, including across bag boundaries.");
                        double rate = Math.Pow(2, pitch);
                        Check(rate >= .974999 && rate <= 1.025001, "Pitch must mean +/-2.5% rate, not octaves.");
                        pitches.Add(pitch);
                        last = sample;
                    }
                    Check(seen.Count == count);
                }
                Check(pitches.Count > 1);
            }
    }

    internal static void RotaryClickCooldown()
    {
        var sequence = new SliderClickSequence(6, new Random(123));
        var uninterrupted = new SliderClickSequence(6, new Random(123));
        Check(sequence.Update(TimeSpan.Zero, true) == uninterrupted.Update(TimeSpan.Zero, true));
        for (int i = 0; i < 49; i++)
            Check(sequence.Update(TimeSpan.FromMilliseconds(1), true) is null);
        Check(sequence.Update(TimeSpan.FromMilliseconds(1), true)
            == uninterrupted.Update(SliderClickSequence.Cooldown, true), "Dropped requests must not consume the bag.");
        Check(sequence.Update(TimeSpan.Zero, true) is null);
        Check(sequence.Update(TimeSpan.FromSeconds(1), false) is null, "No delayed click when dragging stops.");
        Check(sequence.Update(TimeSpan.Zero, false) is null);
        Check(sequence.Update(TimeSpan.Zero, true) is not null, "A new gesture is responsive after the cooldown.");
        int clicks = 0;
        for (int tick = 0; tick < 1000; tick++)
            if (sequence.Update(TimeSpan.FromMilliseconds(1), true) is not null) clicks++;
        Check(clicks == 20, "Rapid dragging must produce at most 20 clicks per second.");
    }

    internal static void OptionsRotaryFeedback()
    {
        var model = new OptionsModel(new());
        Check(!model.TakeSliderClick());
        for (int row = 0; row < OptionsModel.AudioVolumeCount; row++)
        {
            model.Open();
            Check(!model.TakeSliderClick());
            model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
            model.Update(new(SelectionIndex: row, SliderValue: 80), TimeSpan.Zero, true);
            Check(!model.TakeSliderClick());
            for (int value = 81; value < 85; value++)
            {
                model.Update(new(SliderValue: value), TimeSpan.Zero, true);
                Check(model.Volume(row) == value && model.TakeSliderClick(), "Every changed whole-percent value requests feedback.");
            }
            model.Update(new(SliderValue: 85), TimeSpan.Zero, true);
            Check(model.TakeSliderClick() && !model.TakeSliderClick() && model.TakeCheckboxSwitch() is null);
            model.Update(new(SliderValue: 1000), TimeSpan.Zero, true);
            Check(model.Volume(row) == 100 && model.TakeSliderClick());
            model.Update(new(Right: true), TimeSpan.Zero, true);
            Check(!model.TakeSliderClick(), "Keyboard at the maximum must stay silent.");
            model.Update(new(SliderValue: 1000), TimeSpan.Zero, true);
            Check(!model.TakeSliderClick());
            model.Update(new(Left: true), TimeSpan.Zero, true);
            Check(model.Volume(row) == 95 && model.TakeSliderClick());
            model.Update(new(SliderValue: 0), TimeSpan.Zero, false);
            Check(model.Volume(row) == 95 && !model.TakeSliderClick());
            model.Update(new(SliderValue: 0), TimeSpan.Zero, true);
            Check(model.TakeSliderClick());
            model.Update(new(Left: true), TimeSpan.Zero, true);
            Check(!model.TakeSliderClick(), "Keyboard at zero must stay silent.");
            model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
            Check(model.Volume(row) == 80 && !model.TakeSliderClick(), "Restore Defaults is not a slider gesture.");
            model.Update(new(SelectionIndex: row, SliderValue: 40), TimeSpan.Zero, true);
            model.Cancel();
            Check(model.Draft == model.Current && !model.TakeSliderClick());
        }

        var keyboard = new PawnShop.Framework.Input.KeyboardInput();
        model.Open();
        model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        keyboard.Read(new KeyboardState(), true);
        model.Update(keyboard.Read(new KeyboardState(Keys.Right), true), TimeSpan.Zero, true);
        Check(model.Draft.Master == 85 && model.TakeSliderClick());
        model.Update(keyboard.Read(new KeyboardState(Keys.Right), true), TimeSpan.Zero, true);
        Check(model.Draft.Master == 85 && !model.TakeSliderClick(), "Held keys use existing edge behavior.");
        // Discard and display confirmations capture slider requests as well.
        model.Update(new(Cancel: true), TimeSpan.Zero, true);
        Check(model.ConfirmingDiscard);
        model.Update(new(SelectionIndex: 0, SliderValue: 10), TimeSpan.Zero, true);
        Check(model.Draft.Master == 85 && !model.TakeSliderClick());
        model.Cancel();
        model.Open();
        EditDisplay(model);
        Check(model.Previewing);
        model.Update(new(SelectionIndex: 0, SliderValue: 10), TimeSpan.Zero, true);
        Check(!model.TakeSliderClick());

        // SFX and Master changes use their new gain on the same host update.
        var muted = new OptionsModel(new() { Effects = 0 });
        muted.Open();
        muted.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        muted.Update(new(SelectionIndex: 1, Left: true), TimeSpan.Zero, true);
        Check(muted.TakeSliderClick() && muted.Presentation.AudioMix.Gain(AudioChannel.Effects, true) == 0);
        muted.Update(new(SelectionIndex: 3, Right: true), TimeSpan.Zero, true);
        Check(muted.TakeSliderClick() && Math.Abs(muted.Presentation.AudioMix.Gain(AudioChannel.Effects, true) - .04f) < .0001f);
        Check(muted.Presentation.AudioMix.Gain(AudioChannel.Effects, false) == 0);
        muted.Update(new(SliderValue: 0), TimeSpan.Zero, true);
        Check(muted.TakeSliderClick() && muted.Presentation.AudioMix.Gain(AudioChannel.Effects, true) == 0);
        muted.Update(new(), TimeSpan.FromSeconds(1), true);
        Check(!muted.TakeSliderClick());
        muted.Update(new(Right: true), TimeSpan.Zero, true);
        muted.Update(new(), TimeSpan.Zero, true);
        Check(!muted.TakeSliderClick(), "Unconsumed requests expire with the input frame.");
    }

    internal static void RotarySmallDragFeedback()
    {
        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        app.Update(new(TabIndex: 1), TimeSpan.Zero);
        var pointer = new MenuPointerInput();
        var sequence = new SliderClickSequence(AudioAssets.RotaryClicks.Count, new Random(42));
        var thumb = OptionsMenuLayout.AudioThumb(0, app.Options);
        int x = thumb.X + 14, y = thumb.Center.Y;
        (int Sample, float Pitch)? Move(int offset, bool down, int milliseconds)
        {
            var elapsed = TimeSpan.FromMilliseconds(milliseconds);
            var input = pointer.Read(app, new MouseState(x + offset, y, 0,
                down ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released),
                true, 1280, 720, 1280, 720);
            app.Update(input, elapsed);
            return sequence.Update(elapsed, app.Options.TakeSliderClick());
        }
        Check(Move(0, false, 0) is null);
        Check(Move(0, true, 0) is null && app.Options.Draft.Master == 80);
        Check(Move(1, true, 100) is null && app.Options.Draft.Master == 80,
            "Sub-percent pixel movement must not click.");
        Check(Move(3, true, 100) is not null && app.Options.Draft.Master == 81,
            "An isolated one-percent change must click even inside a five-point band.");
        Check(Move(6, true, 10) is null && app.Options.Draft.Master == 82);
        Check(Move(8, true, 10) is null && app.Options.Draft.Master == 83);
        Check(Move(8, true, 100) is null, "Skipped changes must not play after movement stops.");
        Check(Move(11, true, 100) is not null && app.Options.Draft.Master == 84,
            "The next small change must be audible once the cooldown has expired.");
        Check(Move(8, true, 100) is not null && app.Options.Draft.Master == 83,
            "Small reversals need feedback too.");
        Check(Move(8, false, 100) is null);
    }

    internal static void RotaryClickContent()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string ui = Path.Combine(root, "PawnShop.Audio/Content/UI");
        using (var original = File.OpenRead(Path.Combine(ui, "Sources/RotaryKnob/643563__el_boss__radial-knob-clicks-turning-dial.wav")))
            Check(Convert.ToHexString(SHA256.HashData(original)) == "990CBD06C541BD06F856244226DAA3A6E19EB1DFB2D13FEBB5099098970CE030");
        var hashes = new HashSet<string>();
        var levels = new List<double>();
        foreach (string asset in AudioAssets.RotaryClicks)
        {
            string name = Path.GetFileName(asset);
            byte[] source = File.ReadAllBytes(Path.Combine(ui, name + ".wav"));
            Check(hashes.Add(Convert.ToHexString(SHA256.HashData(source))), "Each sample must have a distinct waveform.");
            using var wav = new BinaryReader(new MemoryStream(source));
            Check(Encoding.ASCII.GetString(wav.ReadBytes(4)) == "RIFF");
            Check(wav.ReadUInt32() == source.Length - 8 && Encoding.ASCII.GetString(wav.ReadBytes(4)) == "WAVE");
            byte[] pcm = [];
            while (wav.BaseStream.Position + 8 <= wav.BaseStream.Length)
            {
                string chunk = Encoding.ASCII.GetString(wav.ReadBytes(4));
                uint size = wav.ReadUInt32();
                long end = wav.BaseStream.Position + size;
                if (chunk == "fmt ")
                {
                    Check(wav.ReadUInt16() == 1 && wav.ReadUInt16() == 1 && wav.ReadInt32() == 44100);
                    Check(wav.ReadInt32() == 88200 && wav.ReadUInt16() == 2 && wav.ReadUInt16() == 16);
                }
                else if (chunk == "data") pcm = wav.ReadBytes((int)size);
                wav.BaseStream.Position = end + (size & 1);
            }
            Check(pcm.Length == 2117 * 2);
            var samples = Enumerable.Range(0, pcm.Length / 2).Select(i => (double)BitConverter.ToInt16(pcm, i * 2) / 32768).ToArray();
            double peak = samples.Max(Math.Abs);
            int attack = Array.FindIndex(samples, value => Math.Abs(value) == peak);
            Check(attack > 44 && attack < 441, "Keep a short preroll and an intact initial attack.");
            Check(peak > .05 && peak < .2 && samples[0] == 0 && Math.Abs(samples[^1]) < .0001);
            levels.Add(20 * Math.Log10(Math.Sqrt(samples.Average(value => value * value))));
            Check(TimeSpan.FromSeconds(samples.Length / 44100d / .975) < SliderClickSequence.Cooldown,
                "Even the slowest pitch must finish its decay before the next allowed click.");

            // Inspect actual MGCB output without constructing an audio device.
            string compiled = Path.Combine(AppContext.BaseDirectory, "Content", asset + ".xnb");
            using var xnb = new BinaryReader(File.OpenRead(compiled));
            Check(Encoding.ASCII.GetString(xnb.ReadBytes(3)) == "XNB");
            xnb.ReadByte(); // Platform.
            Check(xnb.ReadByte() == 5 && xnb.ReadByte() == 0); // Uncompressed XNB v5.
            Check(xnb.ReadInt32() == xnb.BaseStream.Length);
            Check(xnb.Read7BitEncodedInt() == 1 && xnb.ReadString().EndsWith(".SoundEffectReader", StringComparison.Ordinal));
            xnb.ReadInt32(); // Reader version.
            Check(xnb.Read7BitEncodedInt() == 0 && xnb.Read7BitEncodedInt() == 1);
            int formatSize = xnb.ReadInt32();
            long dataPosition = xnb.BaseStream.Position + formatSize;
            Check(xnb.ReadUInt16() == 1 && xnb.ReadUInt16() == 1 && xnb.ReadInt32() == 44100);
            xnb.BaseStream.Position = dataPosition;
            Check(xnb.ReadInt32() == pcm.Length && xnb.ReadBytes(pcm.Length).SequenceEqual(pcm), "MGCB must preserve the PCM payload.");
            string owned = Path.Combine(root, "PawnShop.Audio/Content/bin/DesktopGL/Content/UI", name + ".xnb");
            Check(File.ReadAllBytes(compiled).SequenceEqual(File.ReadAllBytes(owned)));
            Check(!File.Exists(Path.Combine(AppContext.BaseDirectory, "Content", asset + ".wav")));
        }
        Check(hashes.Count == 6 && levels.Max() - levels.Min() < .1, "Match short-click energy without peak normalization.");
        Check(!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Content/Audio/UI/Sources")), "Do not ship the original recording.");
    }
}
