using System.Security.Cryptography;
using Microsoft.Xna.Framework.Input;
using PawnShop.Audio;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void DropdownSoundBags()
    {
        for (int seed = 0; seed < 64; seed++)
        {
            var bag = new ShuffledSoundBag(AudioAssets.DropdownClicks.Count, new Random(seed));
            var replay = new ShuffledSoundBag(AudioAssets.DropdownClicks.Count, new Random(seed));
            int last = -1;
            for (int cycle = 0; cycle < 100; cycle++)
            {
                var seen = new HashSet<int>();
                for (int i = 0; i < AudioAssets.DropdownClicks.Count; i++)
                {
                    int sample = bag.Next();
                    Check(sample == replay.Next() && seen.Add(sample));
                    Check(sample >= 0 && sample < 4 && sample != last,
                        "Every adjacent open/selection pair must differ, including across reshuffles.");
                    last = sample;
                }
                Check(seen.Count == 4);
            }
        }
    }

    internal static void OptionsDropdownSounds()
    {
        var bag = new ShuffledSoundBag(4, new Random(42));
        foreach (var (tab, row) in new[] { (0, 0), (0, 1), (2, 2) })
        {
            var model = new OptionsModel(new());
            model.Open();
            Check(!model.TakeDropdownClick());
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick());
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, false);
            Check(model.DropdownRow is null && !model.TakeDropdownClick());
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
            Check(model.TakeDropdownClick() && !model.TakeDropdownClick());
            model.Update(new(Down: true), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick(), "Navigating an open list is not a committed selection.");
            model.Update(new(ChoiceIndex: 999, Confirm: true), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick() && model.DropdownRow == row);
            model.Update(new(Cancel: true), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick() && model.DropdownRow is null);

            // Real keyboard edges: open and select without any elapsed time or
            // cooldown. Held Enter must not activate the highlighted option.
            var keyboard = new PawnShop.Framework.Input.KeyboardInput();
            keyboard.Read(new KeyboardState(), true);
            model.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero, true);
            Check(model.TakeDropdownClick());
            int opening = bag.Next();
            model.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick() && model.DropdownRow == row);
            keyboard.Read(new KeyboardState(), true);
            model.Update(keyboard.Read(new KeyboardState(Keys.Enter), true), TimeSpan.Zero, true);
            Check(model.TakeDropdownClick() && model.DropdownRow is null);
            Check(bag.Next() != opening, "Opening and confirming the current option still need distinct sounds.");
            model.Update(new(Right: true), TimeSpan.Zero, true);
            Check(model.TakeDropdownClick(), "Closed dropdown keyboard adjustments also select an option.");
            model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
            Check(!model.TakeDropdownClick(), "Restore Defaults must not play dropdown sounds.");
            model.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
            model.Cancel();
            Check(!model.TakeDropdownClick());

            using var app = new ApplicationFlow();
            OpenOptions(app); FinishOptionsFlight(app);
            app.Update(new(TabIndex: tab), TimeSpan.Zero);
            var pointer = new MenuPointerInput();
            InputFrame Read(int x, int y, bool down) => pointer.Read(app,
                new MouseState(x, y, 0, down ? ButtonState.Pressed : ButtonState.Released,
                    ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
                true, 1280, 720, 1280, 720);
            var opener = OptionsMenuLayout.ValueBounds(row, app.Options).Center;
            Read(opener.X, opener.Y, false);
            app.Update(Read(opener.X, opener.Y, true), TimeSpan.Zero);
            Check(app.Options.TakeDropdownClick());
            opening = bag.Next();
            app.Update(Read(opener.X, opener.Y, true), TimeSpan.Zero);
            Check(!app.Options.TakeDropdownClick());
            var item = OptionsMenuLayout.DropdownItem(0, app.Options).Center;
            Read(item.X, item.Y, false);
            app.Update(Read(item.X, item.Y, true), TimeSpan.Zero);
            Check(app.Options.TakeDropdownClick() && bag.Next() != opening);
            Check(!app.Options.TakeButtonClick() && !app.Options.TakeSliderClick()
                && app.Options.TakeCheckboxSwitch() is null, "Dropdowns must not borrow another control's cue.");
        }
        var modal = new OptionsModel(new());
        EditDisplay(modal);
        Check(modal.Previewing);
        modal.Update(new(SelectionIndex: 0, Confirm: true), TimeSpan.Zero, true);
        Check(!modal.TakeDropdownClick(), "Display confirmation must not trigger dropdown audio.");
        modal.Open();
        modal.Update(new(Confirm: true), TimeSpan.Zero, true);
        modal.Update(new(), TimeSpan.Zero, false);
        Check(!modal.TakeDropdownClick() && modal.DropdownRow is null);
    }

    internal static void DropdownClickContent()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string content = Path.Combine(root, "PawnShop.Audio/Content");
        var sourceHashes = new[]
        {
            "2D7B28870ADD4158C63302CCCA030CD0EC1C2F206D61EBAB9B9EA6EBA174BDB2",
            "7AAFA8AD71221461AE1BF30749F251702A1A5B00EE495904A3770A6D0528C9DD",
            "486E1A5F6F94A8BD2099F58804941EC42EBA080815657C14F59250C33BEA3870",
            "BEEF0E657F837D173B68D3EE8D6BBC9ECC293CD895BD4D9D884034851BA4231C"
        };
        var runtimeHashes = new[]
        {
            "E06F9F1C037357C9C0BBA0AB1BB4BA6C32397EB1491A305C65378D49878C1E70",
            "81928B3ADCBA713FCC900F2544BA37B784C702425C0DDAF4F2B2F11869181271",
            "42DB9204AB7F81FC3FE3081B80F6F5DB93E2C8192DE477E82F79B83437226F3E",
            "768FFAE8CDFC05AAE7CCCA45994BB6E49F044FB6278B7403138344B111958733"
        };
        Check(AudioAssets.DropdownClicks.Count == 4 && runtimeHashes.Distinct().Count() == 4);
        for (int i = 0; i < 4; i++)
        {
            string asset = AudioAssets.DropdownClicks[i];
            CheckClickContent(Path.GetFileName(asset), runtimeHashes[i]);
            string original = Directory.GetFiles(Path.Combine(content, "UI/Sources/DropdownClicks"), $"*-{i + 1}.wav").Single();
            Check(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(original))) == sourceHashes[i]);
            string compiled = Path.Combine(AppContext.BaseDirectory, "Content", asset + ".xnb");
            string owned = Path.Combine(content, "bin/DesktopGL/Content/UI", Path.GetFileName(asset) + ".xnb");
            Check(File.ReadAllBytes(compiled).SequenceEqual(File.ReadAllBytes(owned)));
        }
        Check(!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Content/Audio/UI/Sources")));
    }
}
