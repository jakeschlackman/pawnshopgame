using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsPanelSource()
    {
        byte[] png = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Content", "UI", "options-panel-source.png"));
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)) == 1586);
        Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)) == 992);
        Check(Math.Abs(OptionsMenuLayout.Panel.Width / (double)OptionsMenuLayout.Panel.Height - 1586d / 992) < .003);
        Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Header));
        Check(OptionsMenuLayout.Panel.Contains(OptionsMenuLayout.Viewport));
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
                Check(model.Scroll.Offset == 0 && model.Scroll.Maximum > 0);
                var footer = OptionsMenuLayout.Footer(0);
                model.Update(new(ScrollDelta: 10000), TimeSpan.Zero, true);
                Check(model.Scroll.Offset == model.Scroll.Maximum);
                Check(OptionsMenuLayout.Footer(0) == footer);
                Check(!OptionsMenuLayout.Viewport.Intersects(footer));
                Check(OptionsMenuLayout.Track.Contains(OptionsMenuLayout.Thumb(model)));
                model.Update(new(ScrollDelta: -10000), TimeSpan.Zero, true);
                Check(model.Scroll.Offset == 0);
                for (int row = 1; row < model.FieldCount; row++)
                {
                    model.Update(new(Down: true), TimeSpan.Zero, true);
                    Check(OptionsMenuLayout.Viewport.Contains(OptionsMenuLayout.Field(row, model)));
                }
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
        // An offscreen row cannot receive clicks outside the clipped viewport.
        var hidden = OptionsMenuLayout.Field(0, app.Options).Center;
        Check(MenuPointerInput.HitTest(app, hidden.X, hidden.Y, 1280, 720).SelectionIndex is null);
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
