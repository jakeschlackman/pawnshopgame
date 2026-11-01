using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

public static class TitleMenuLayout
{
    public const float FirstRow = 0.572222f, RowSpacing = 0.076852f;
    public static Rectangle Row(int index) => new(240, (int)MathF.Round((FirstRow + index * RowSpacing) * 1080) - 6, 1340, 78);
}

public static class OptionsMenuLayout
{
    public static Rectangle Row(int index) => new(42, 158 + index * 43, 1190, 36);
    public static Rectangle Tab(int index) => new(50 + index * 300, 102, 280, 36);
    public static Rectangle Keep => new(90, 470, 450, 40);
    public static Rectangle Revert => new(650, 470, 450, 40);
}

// Pointer clicks translate to the same menu actions as keyboard input; no settings rules here.
public sealed class MenuPointerInput
{
    private bool wasDown;
    private bool wasActive;

    public InputFrame Read(ApplicationFlow flow, MouseState mouse, bool active, int clientWidth, int clientHeight,
        int backBufferWidth, int backBufferHeight)
    {
        bool down = mouse.LeftButton == ButtonState.Pressed;
        bool clicked = active && wasActive && down && !wasDown;
        wasDown = down;
        wasActive = active;
        if (!clicked || clientWidth <= 0 || clientHeight <= 0) return default;
        int x = (int)((double)mouse.X * backBufferWidth / clientWidth);
        int y = (int)((double)mouse.Y * backBufferHeight / clientHeight);
        return HitTest(flow, x, y, backBufferWidth, backBufferHeight);
    }

    public static InputFrame HitTest(ApplicationFlow flow, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0 || flow.TitleInteractionPlaying || flow.UsesTitleBackground && !flow.TitleUiVisible) return default;
        bool options = flow.Screen == Screen.Options;
        if (options && flow.Options.CapturingKey) return default;
        int canvasWidth = options ? 1280 : flow.Screen == Screen.Title ? 1920 : 640;
        int canvasHeight = options ? 720 : flow.Screen == Screen.Title ? 1080 : 360;
        double scale = Math.Min((double)width / canvasWidth, (double)height / canvasHeight);
        if (!options && flow.Screen != Screen.Title && scale >= 1) scale = Math.Floor(scale);
        int outputWidth = (int)(canvasWidth * scale), outputHeight = (int)(canvasHeight * scale);
        var output = new Rectangle((width - outputWidth) / 2, (height - outputHeight) / 2, outputWidth, outputHeight);
        if (!output.Contains(x, y)) return default;
        var point = new Point((int)((x - output.X) * (double)canvasWidth / outputWidth),
            (int)((y - output.Y) * (double)canvasHeight / outputHeight));
        if (options)
        {
            if (flow.Options.Previewing)
            {
                if (OptionsMenuLayout.Keep.Contains(point)) return new(Confirm: true);
                if (OptionsMenuLayout.Revert.Contains(point)) return new(Cancel: true);
                return default;
            }
            for (int i = 0; i < OptionsModel.Tabs.Count; i++)
                if (OptionsMenuLayout.Tab(i).Contains(point)) return new(TabIndex: i);
            for (int i = 0; i < flow.Options.FieldCount + 3; i++)
                if (OptionsMenuLayout.Row(i).Contains(point)) return new(Confirm: true, SelectionIndex: i);
        }
        else if (flow.Menu is { } menu)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var row = flow.Screen == Screen.Title ? TitleMenuLayout.Row(i) : new Rectangle(44, 129 + i * 32, 540, 27);
                if (row.Contains(point)) return new(Confirm: true, SelectionIndex: i);
            }
        }
        return default;
    }
}
