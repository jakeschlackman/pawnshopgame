using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class OptionsScreen(GraphicsDevice device, FontLibrary fonts) : IDisposable
{
    private readonly PixelRenderer renderer = new(device, 1280, 720, integerScaling: false, fonts: fonts);
    private static readonly Color Cream = new(255, 240, 210), Gold = new(255, 209, 108);

    // Return title-canvas coordinates so the sprite keeps the same display scale
    // while crossing from the 1920x1080 title UI to the 1280x720 Options UI.
    public Vector2 SelectorAnchor(OptionsModel model, bool first = false)
    {
        renderer.TextScale = model.Presentation.UiScale / 100f;
        int selected = first ? 0 : model.Selected;
        var fields = model.Fields;
        string label = selected < fields.Count ? fields[selected].Label : "APPLY";
        return new Vector2(30, 166 + selected * 43 + renderer.MeasureTextVerticalCenter(label.Font("OptionsBody")) + 6) * 1.5f;
    }

    public void Draw(OptionsModel model, Action? drawBackground = null)
    {
        bool overPanorama = drawBackground is not null;
        renderer.TextScale = model.Presentation.UiScale / 100f;
        renderer.Begin(overPanorama ? Color.Transparent : new Color(26, 36, 38));
        renderer.Fill(24, 24, 1232, 672, new Color(36, 51, 51) * (overPanorama ? 0.82f : 1));
        Text("OPTIONS", 50, 46, Gold, "OptionsHeading");
        for (int i = 0; i < OptionsModel.Tabs.Count; i++)
            Text(OptionsModel.Tabs[i], 50 + i * 300, 110, i == model.Tab ? Gold : Cream);
        var fields = model.Fields;
        for (int i = 0; i < fields.Count + 3; i++)
        {
            int y = 166 + i * 43;
            if (i == model.Selected) renderer.Fill(42, y - 8, 1190, 36, new Color(62, 78, 72));
            string label = i < fields.Count ? fields[i].Label : (i - fields.Count) switch
            {
                0 => "APPLY", 1 => "RESTORE DEFAULTS", _ => "BACK - DISCARD UNAPPLIED EDITS"
            };
            Text(label, 60, y, i == model.Selected ? Gold : Cream);
            if (i < fields.Count) Text(fields[i].Value, 800, y, Cream);
            if (model.Tab == 1 && i < 4)
            {
                int volume = i switch { 0 => model.Draft.Master, 1 => model.Draft.Music,
                    2 => model.Draft.Ambient, _ => model.Draft.Effects };
                renderer.Fill(500, y + 4, 250, 8, new Color(20, 29, 33));
                renderer.Fill(500, y + 4, volume * 250 / 100, 8, Gold);
            }
        }
        if (model.Tab == 0)
        {
            Text("BORDERLESS USES DESKTOP SIZE. RESOLUTION IS RETAINED", 60, 540, Cream, "OptionsHelp");
            Text("FOR WINDOWED / FULLSCREEN. APPLY TO TEST FOR 15 SECONDS.", 60, 569, Cream, "OptionsHelp");
        }
        else if (model.Tab == 1)
            Text("PREVIEW TONES ONLY - GAME AUDIO IS NOT YET AUTHORED.", 60, 540, Cream, "OptionsHelp");
        else if (model.Tab == 2)
        {
            int count = model.Draft.RevealedCharacters(model.TextElapsed, OptionsModel.SampleText.Length);
            Text(OptionsModel.SampleText[..count], 60, 540, Gold);
            Text("TEXT PREVIEW - CONVERSATIONS ARRIVE IN A LATER SLICE.", 60, 578, Cream, "OptionsHelp");
        }
        else
        {
            Text("ENTER: REBIND. ESC CANCELS CAPTURE. DUPLICATES REJECTED.", 60, 540, Cream, "OptionsHelp");
            Text("MENU KEYS STAY FIXED. MOVEMENT IS NOT IMPLEMENTED YET.", 60, 578, Cream, "OptionsHelp");
        }
        string message = model.Message ?? "TAB: CATEGORY   UP/DOWN: SELECT   LEFT/RIGHT: CHANGE";
        if (renderer.MeasureText(message.Font("OptionsHelp")).X > 1160)
        {
            int split = message.LastIndexOf(' ');
            while (split > 0 && renderer.MeasureText(message[..split].Font("OptionsHelp")).X > 1160)
                split = message.LastIndexOf(' ', split - 1);
            if (split > 0)
            {
                Text(message[..split], 60, 615, Gold, "OptionsHelp");
                Text(message[(split + 1)..], 60, 643, Gold, "OptionsHelp");
            }
            else Text(message, 60, 628, Gold, "OptionsHelp");
        }
        else Text(message, 60, 628, Gold, "OptionsHelp");
        Text("ENTER: ACTIVATE   ESC: BACK / DISCARD", 60, 677, Cream, "OptionsHelp");
        if (model.Previewing)
        {
            renderer.Fill(30, 160, 1220, 370, new Color(20, 29, 33));
            Text("KEEP DISPLAY CHANGES?", 90, 220, Gold, "OptionsHeading");
            Text($"REVERTING IN {(int)Math.Ceiling(model.PreviewRemaining.TotalSeconds)} SECONDS", 90, 310, Cream);
            Text("ENTER: KEEP AND SAVE   ESC: REVERT", 90, 385, Cream);
            Text("LOSING FOCUS ALSO REVERTS.", 90, 435, Cream);
            foreach (var button in new[] { (OptionsMenuLayout.Keep, "KEEP"), (OptionsMenuLayout.Revert, "REVERT") })
            {
                var bounds = button.Item1;
                renderer.Fill(bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(62, 78, 72));
                Text(button.Item2, bounds.X + 20, bounds.Y + 10, Gold);
            }
        }
        renderer.Present(clearBackBuffer: !overPanorama, drawBackground: drawBackground);
    }

    private void Text(string text, int x, int y, Color color, string fontName = "OptionsBody") =>
        renderer.Text(text.Font(fontName), x, y, color);

    public void Dispose() => renderer.Dispose();
}
