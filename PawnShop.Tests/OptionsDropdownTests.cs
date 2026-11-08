using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsDropdowns()
    {
        int writes = 0;
        var model = new OptionsModel(new(), _ => writes++);
        model.Open();
        var original = model.Draft;
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(model.DropdownRow == 0 && model.DropdownCount == 3 && model.DropdownChoice == 0);
        model.Update(new(Down: true), TimeSpan.Zero, true);
        Check(model.DropdownChoice == 1 && model.Draft == original && writes == 0);
        Check(!model.Update(new(Cancel: true), TimeSpan.Zero, true) && model.DropdownRow is null);
        Check(model.Draft == original && model.Selected == 0 && model.IndicatorRow == 0);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        model.Update(new(Confirm: true, ChoiceIndex: 2), TimeSpan.Zero, true);
        Check(model.DropdownRow is null && model.Draft.Display.Mode == WindowMode.Fullscreen);
        Check(model.DisplayForHost == original.Display && model.Current == original && writes == 0);
        Select(model, model.FieldCount);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(model.Previewing && writes == 0);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(!model.Previewing && writes == 1 && model.Current.Display.Mode == WindowMode.Fullscreen);

        foreach (int scale in new[] { 100, 125, 150 })
            for (int row = 0; row < 2; row++)
            {
                var sized = new OptionsModel(new() { UiScale = scale });
                sized.Open();
                sized.Update(new(SelectionIndex: row, Confirm: true), TimeSpan.Zero, true);
                Check(OptionsMenuLayout.Viewport.Contains(OptionsMenuLayout.Dropdown(sized)));
                for (int i = 0; i < sized.DropdownCount + 1; i++)
                {
                    sized.Update(new(Down: true), TimeSpan.Zero, true);
                    Check(sized.DropdownChoice >= sized.DropdownFirst
                        && sized.DropdownChoice < sized.DropdownFirst + OptionsMenuLayout.DropdownVisibleCount(sized));
                }
                Check(sized.Draft == sized.Current);
                sized.Update(new(Confirm: true, ChoiceIndex: 999), TimeSpan.Zero, true);
                Check(sized.DropdownRow == row && sized.Draft == sized.Current);
                sized.Update(default, TimeSpan.Zero, false);
                Check(sized.DropdownRow is null && sized.Draft == sized.Current);
            }

        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        InputFrame Click(int x, int y) => MenuPointerInput.HitTest(app, x, y, 1280, 720);
        var opener = OptionsMenuLayout.ValueBounds(0, app.Options).Center;
        app.Update(Click(opener.X, opener.Y), TimeSpan.Zero);
        var first = OptionsMenuLayout.DropdownItem(0, app.Options).Center;
        var hit = Click(first.X, first.Y);
        Check(hit.ChoiceIndex == 0 && hit.SelectionIndex is null);
        Check(MenuPointerInput.HitTest(app, 0, 0, 3440, 1440).Cancel);
        var apply = OptionsMenuLayout.Footer(0).Center;
        app.Update(Click(apply.X, apply.Y), TimeSpan.Zero);
        Check(app.Options.DropdownRow is null && app.Options.Selected == 0 && !app.Options.Previewing);
        app.Update(new(Confirm: true, SelectionIndex: 1), TimeSpan.Zero);
        int bodyOffset = app.Options.Scroll.Offset;
        var pointer = new MenuPointerInput();
        var list = OptionsMenuLayout.Dropdown(app.Options);
        InputFrame Read(int wheel, bool down = false, int? x = null, int? y = null) => pointer.Read(app,
            new MouseState(x ?? list.Center.X, y ?? list.Center.Y, wheel,
                down ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720);
        Read(0);
        app.Update(Read(-120), TimeSpan.Zero);
        Check(app.Options.DropdownFirst > 0 && app.Options.Scroll.Offset == bodyOffset);
        var thumb = OptionsMenuLayout.DropdownThumb(app.Options);
        Read(-120, true, thumb.Center.X, thumb.Center.Y);
        app.Update(Read(-120, true, thumb.Center.X, list.Bottom), TimeSpan.Zero);
        Check(app.Options.DropdownFirst == app.Options.DropdownCount - OptionsMenuLayout.DropdownVisibleCount(app.Options));
        Read(-120);
        for (int i = 0; i < app.Options.DropdownCount; i++)
            app.Update(new(ScrollDelta: 48), TimeSpan.Zero);
        int lastVisible = OptionsMenuLayout.DropdownVisibleCount(app.Options) - 1;
        var last = OptionsMenuLayout.DropdownItem(lastVisible, app.Options).Center;
        foreach (var size in new[] { (960, 540), (1280, 720), (2560, 1440), (3440, 1440) })
        {
            double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
            int x = (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(last.X * ratio);
            int y = (size.Item2 - (int)(720 * ratio)) / 2 + (int)(last.Y * ratio);
            Check(MenuPointerInput.HitTest(app, x, y, size.Item1, size.Item2).ChoiceIndex == AppSettings.Resolutions.Count - 1);
        }
        app.Update(Click(last.X, last.Y), TimeSpan.Zero);
        Check(app.Options.Draft.Display.Width == 3840 && app.Options.Draft.Display.Height == 2160);
        Check(app.Options.Current.Display.Width == 1280 && app.Options.IndicatorRow == 1);
        app.Update(new(SelectionIndex: 1, Confirm: true), TimeSpan.Zero);
        app.Update(new(NextTab: true), TimeSpan.Zero);
        Check(app.Options.Tab == 1 && app.Options.DropdownRow is null && app.Options.Scroll.Offset == 0);
    }
}
