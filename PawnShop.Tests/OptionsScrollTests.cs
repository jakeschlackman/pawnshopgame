using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsFooterActions()
    {
        Check(OptionsMenuLayout.Footer(0).Right == OptionsMenuLayout.Footer(2).Left);
        for (int action = 0; action < 3; action++)
            Check(OptionsMenuLayout.Footer(action).Bottom == OptionsMenuLayout.BottomBarTop);
        for (int tab = 0; tab < 4; tab++)
        {
            int writes = 0;
            var initial = new AppSettings { Master = 50 };
            using var app = new ApplicationFlow(initial, _ => writes++);
            OpenOptions(app); FinishOptionsFlight(app);
            void Click(int action)
            {
                var point = OptionsMenuLayout.Footer(action).Center;
                int indicatorRow = app.Options.IndicatorRow;
                app.Update(MenuPointerInput.HitTest(app, point.X, point.Y, 1280, 720), TimeSpan.Zero);
                Check(app.Options.IndicatorRow == indicatorRow, "Footer clicks must leave Sprout at his last option.");
            }
            app.Update(new(TabIndex: 1), TimeSpan.Zero);
            app.Update(new(Left: true), TimeSpan.Zero);
            Check(app.Options.Draft.Master == 45 && app.Options.Current == initial);
            app.Update(new(TabIndex: tab), TimeSpan.Zero);
            app.Update(new(SelectionIndex: 1), TimeSpan.Zero);
            Check(app.Options.IndicatorRow == 1);
            app.Options.Scroll.MoveTo(app.Options.Scroll.Maximum);
            Click(1);
            var expected = new AppSettings { Master = tab == 1 ? 80 : 45 };
            Check(app.Options.Draft == expected && app.Options.Current == initial && writes == 0);
            Click(0);
            Check(app.Options.Current == expected && writes == 1);
            // Rectangle's exclusive right edge belongs to Back at the shared seam.
            var seam = OptionsMenuLayout.Footer(2);
            Check(MenuPointerInput.HitTest(app, seam.Left, seam.Center.Y, 1280, 720).SelectionIndex
                == app.Options.FieldCount + 2);
            app.Update(new(TabIndex: 1), TimeSpan.Zero);
            app.Update(new(Left: true), TimeSpan.Zero);
            Check(app.Options.Draft != app.Options.Current);
            app.Update(new(TabIndex: tab), TimeSpan.Zero);
            app.Update(new(SelectionIndex: 1), TimeSpan.Zero);
            app.Options.Scroll.MoveTo(app.Options.Scroll.Maximum);
            Click(2);
            Check(app.Screen == Screen.Options && app.Options.ConfirmingDiscard && app.Options.IsDirty);
            app.Update(new(Confirm: true, ChoiceIndex: 1), TimeSpan.Zero);
            Check(app.Screen == Screen.Title && app.Options.Draft == app.Options.Current && writes == 1);
        }
    }

    internal static void OptionsPanelSource()
    {
        byte[] png = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Content", "UI", "options-panel-source.png"));
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)) == 1586);
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)) == 992);
        Check(Math.Abs(OptionsMenuLayout.Panel.Width / (double)OptionsMenuLayout.Panel.Height - 1586d / 992) < .003);
        Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Header));
        Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Viewport));
        Check(OptionsMenuLayout.Viewport.Left > OptionsMenuLayout.Header.Left + 40
            && OptionsMenuLayout.Track.Right < OptionsMenuLayout.Header.Right - 16);
        for (int action = 0; action < 3; action++)
            Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Footer(action)));
    }

    internal static void OptionsScroll()
    {
        string art = Path.Combine(AppContext.BaseDirectory, "Content", "UI");
        using (var provenance = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(art, "provenance.json"))))
            foreach (var asset in provenance.RootElement.GetProperty("assets").EnumerateArray())
            {
                byte[] png = File.ReadAllBytes(Path.Combine(art, asset.GetProperty("file").GetString()!));
                Check(png.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
                Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)) == asset.GetProperty("width").GetInt32());
                Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)) == asset.GetProperty("height").GetInt32());
                Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(png)).Equals(
                    asset.GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase));
            }
        foreach (int scale in new[] { 100, 125, 150 })
        {
            var model = new OptionsModel(new() { UiScale = scale });
            model.Open();
            for (int tab = 0; tab < 4; tab++)
            {
                Check(OptionsMenuLayout.Header.Contains(OptionsMenuLayout.Tab(tab)));
                Check(OptionsMenuLayout.Header.Bottom < OptionsMenuLayout.Viewport.Top);
                model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
                Check(model.Scroll.Offset == 0);
                Check((model.Scroll.Maximum > 0) == (tab is 1 or 3));
                Check(OptionsMenuLayout.Viewport.Bottom == OptionsMenuLayout.Footer(0).Top - 8);
                Check(OptionsMenuLayout.Track.Top == OptionsMenuLayout.Viewport.Top
                    && OptionsMenuLayout.Track.Bottom == OptionsMenuLayout.Viewport.Bottom);
                var footer = OptionsMenuLayout.Footer(0);
                model.Update(new(ScrollDelta: 10000), TimeSpan.Zero, true);
                Check(model.Scroll.Offset == model.Scroll.Maximum);
                Check(OptionsMenuLayout.Footer(0) == footer);
                Check(!OptionsMenuLayout.Viewport.Intersects(footer));
                Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Thumb(model)));
                Check(OptionsMenuLayout.Thumb(model).Top > OptionsMenuLayout.Track.Top + 10
                    && OptionsMenuLayout.Thumb(model).Bottom < OptionsMenuLayout.Track.Bottom - 10);
                Check(OptionsMenuLayout.Thumb(model).Height == 42);
                if (model.Scroll.Maximum > 0)
                    Check(OptionsMenuLayout.Thumb(model).Bottom == OptionsMenuLayout.ThumbTravel.Bottom);
                model.Update(new(ScrollDelta: -10000), TimeSpan.Zero, true);
                Check(model.Scroll.Offset == 0);
                Check(OptionsMenuLayout.Thumb(model).Top == OptionsMenuLayout.ThumbTravel.Top);
                for (int row = 1; row < model.FieldCount; row++)
                {
                    model.Update(new(Down: true), TimeSpan.Zero, true);
                    Check(OptionsMenuLayout.Viewport.Contains(OptionsMenuLayout.Field(row, model)));
                    Check(model.IndicatorRow == row);
                    Check(OptionsMenuLayout.Field(row, model).Contains(OptionsMenuLayout.ValueBounds(row, model)));
                    for (int slot = 0; slot < 2; slot++)
                        Check(OptionsMenuLayout.ValueBounds(row, model).Contains(OptionsMenuLayout.Cell(row, slot, model)));
                }
                int lastOption = model.IndicatorRow;
                model.Update(new(Down: true), TimeSpan.Zero, true);
                Check(model.Selected == model.FieldCount && model.IndicatorRow == lastOption);
            }
        }

        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        app.Update(new(TabIndex: 3), TimeSpan.Zero);
        var pointer = new MenuPointerInput();
        MouseState Mouse(int x, int y, int wheel = 0, bool down = false) =>
            new(x, y, wheel, down ? ButtonState.Pressed : ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        InputFrame Read(MouseState mouse, bool active = true) => pointer.Read(app, mouse, active, 1280, 720, 1280, 720);
        app.Update(new(TabIndex: 0), TimeSpan.Zero);
        Read(Mouse(600, 540));
        Check(Read(Mouse(600, 540, -120)) == default && app.Options.Scroll.Maximum == 0);
        var track = OptionsMenuLayout.Track;
        Check(Read(Mouse(track.Center.X, track.Center.Y, -120, true)) == default);
        Read(Mouse(track.Center.X, track.Center.Y, -120));
        app.Update(new(TabIndex: 3), TimeSpan.Zero);
        Check(Read(Mouse(600, 540, -240)).ScrollDelta == 48); // Newly available lower viewport accepts wheel input.
        Read(Mouse(600, 360));
        var scroll = Read(Mouse(600, 360, -120));
        Check(scroll.ScrollDelta == 48);
        app.Update(scroll, TimeSpan.Zero);
        Check(app.Options.Scroll.Offset == 48);
        Check(Read(Mouse(600, 160, -240)) == default); // Fixed tab bar does not scroll.
        Read(Mouse(600, 360, -360), active: false);
        Check(Read(Mouse(600, 360, -480)) == default); // Focus return does not replay wheel input.
        var thumb = OptionsMenuLayout.Thumb(app.Options);
        Read(Mouse(thumb.Center.X, thumb.Center.Y, -480, true));
        var drag = Read(Mouse(thumb.Center.X, 700, -480, true));
        app.Update(drag, TimeSpan.Zero);
        Check(app.Options.Scroll.Offset == app.Options.Scroll.Maximum);
        Read(Mouse(600, 700, -480));
        var bottomThumb = OptionsMenuLayout.Thumb(app.Options);
        app.Update(Read(Mouse(bottomThumb.Center.X, bottomThumb.Center.Y, -480, true)), TimeSpan.Zero);
        Check(app.Options.Scroll.Offset == app.Options.Scroll.Maximum, "Grabbing the bottom knob must not shift the scroll position.");
        app.Update(Read(Mouse(bottomThumb.Center.X, OptionsMenuLayout.Track.Top, -480, true)), TimeSpan.Zero);
        Check(app.Options.Scroll.Offset == 0);
        app.Update(Read(Mouse(bottomThumb.Center.X, OptionsMenuLayout.Track.Bottom, -480, true)), TimeSpan.Zero);
        Check(app.Options.Scroll.Offset == app.Options.Scroll.Maximum);
        Read(Mouse(600, 700, -480));
        var rail = OptionsMenuLayout.Track;
        Check(Read(Mouse(rail.Center.X, rail.Center.Y, -600)).ScrollDelta == 48);
        Check(app.Options.HoveredScrollbar);
        Read(Mouse(rail.Center.X, rail.Center.Y, -600), active: false);
        Check(!app.Options.HoveredScrollbar);
        var shiftedWatch = OptionsMenuLayout.Thumb(app.Options);
        Read(Mouse(shiftedWatch.Right - 1, shiftedWatch.Center.Y, -600));
        Check(Read(Mouse(shiftedWatch.Right - 1, shiftedWatch.Center.Y, -600, true)).ScrollPosition is not null);
        Read(Mouse(shiftedWatch.Right - 1, shiftedWatch.Center.Y, -600));
        // A partially scrolled row cannot receive clicks above the clipped viewport.
        var hidden = OptionsMenuLayout.Field(0, app.Options);
        Check(hidden.Top < OptionsMenuLayout.Viewport.Top);
        Check(MenuPointerInput.HitTest(app, hidden.Center.X, hidden.Top + 1, 1280, 720).SelectionIndex is null);
        var last = OptionsMenuLayout.Cell(5, 1, app.Options);
        app.Options.Scroll.MoveTo(36 + app.Options.FieldCount * OptionsMenuLayout.RowHeight(app.Options.Draft.UiScale) - OptionsMenuLayout.Viewport.Height);
        last = OptionsMenuLayout.Cell(5, 1, app.Options);
        foreach (var size in new[] { (960, 540), (1280, 720), (2560, 1440), (3440, 1440) })
        {
            double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
            int x = (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(last.Center.X * ratio);
            int y = (size.Item2 - (int)(720 * ratio)) / 2 + (int)(last.Center.Y * ratio);
            var click = MenuPointerInput.HitTest(app, x, y, size.Item1, size.Item2);
            Check(click.SelectionIndex == 5 && click.BindingIndex == 1);
        }
    }
}
