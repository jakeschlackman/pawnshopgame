using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class OptionsScreen : IDisposable
{
    private readonly PixelRenderer renderer;
    private readonly Texture2D frame, button, socket, paper;
    private readonly Texture2D[] tabArtwork;
    private static readonly Color Ink = new(57, 40, 31), Cream = new(255, 240, 204);
    private static readonly Color Teal = new(130, 180, 165);

    public OptionsScreen(GraphicsDevice device, FontLibrary fonts)
    {
        renderer = new(device, 1280, 720, integerScaling: false, fonts: fonts);
        frame = Load(device, "options-panel-source.png");
        button = Load(device, "brass-button.png");
        socket = Load(device, "teal-socket.png");
        paper = Load(device, "parchment.png");
        tabArtwork = [Load(device, "tab-inactive.png"), Load(device, "tab-selected.png"),
            Load(device, "tab-hovered.png"), Load(device, "tab-pressed.png")];
    }

    private static Texture2D Load(GraphicsDevice device, string name)
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Content", "UI", name));
        return Texture2D.FromStream(device, stream, DefaultColorProcessors.PremultiplyAlpha);
    }

    public Vector2 SelectorAnchor(OptionsModel model, bool first = false)
    {
        renderer.TextScale = model.Presentation.UiScale / 100f;
        int selected = first ? 0 : model.Selected;
        var bounds = selected < model.FieldCount ? OptionsMenuLayout.Field(selected, model)
            : OptionsMenuLayout.Footer(selected - model.FieldCount);
        // Keep the existing selector outside the viewport and away from clipped fields.
        return new Vector2(bounds.X - 44, Math.Clamp(bounds.Center.Y, 240, 640)) * 1.5f;
    }

    public void Draw(OptionsModel model, Action? drawBackground = null)
    {
        renderer.TextScale = model.Presentation.UiScale / 100f;
        renderer.Begin(Color.Transparent);
        renderer.DrawTexture(frame, OptionsMenuLayout.Panel);
        for (int tab = 0; tab < OptionsModel.Tabs.Count; tab++)
        {
            var bounds = OptionsMenuLayout.Tab(tab);
            Button(bounds, OptionsModel.Tabs[tab], model.Tab == tab, model.HoveredTab == tab, model.PointerDown,
                categoryTab: true);
        }
        // The complete supplied image owns the header, divider and inner surface.
        renderer.BeginClip(OptionsMenuLayout.Viewport);
        try
        {
            int headerY = OptionsMenuLayout.Viewport.Y + 4 - model.Scroll.Offset;
            Text(model.Tab == 3 ? "BINDINGS" : "PREFERENCES", 188, headerY, Cream, "OptionsHelp");
            if (model.Tab == 3)
            {
                Text("PRIMARY", 652, headerY, Cream, "OptionsHelp");
                Text("ALTERNATE", 882, headerY, Cream, "OptionsHelp");
            }
            var fields = model.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                var row = OptionsMenuLayout.Field(i, model);
                if (!row.Intersects(OptionsMenuLayout.Viewport)) continue;
                bool selected = i == model.Selected;
                Text(fields[i].Label, 188, row.Y + 14, Cream);
                if (selected) Text(">", 176, row.Y + 14, Cream);
                if (model.Tab == 3)
                {
                    var binding = model.Binding(i);
                    for (int slot = 0; slot < 2; slot++)
                    {
                        bool focus = selected && slot == model.SelectedBinding;
                        string label = model.CapturingKey && focus ? "PRESS KEY" :
                            OptionsModel.KeyLabel(slot == 0 ? binding.Primary : binding.Alternate);
                        Button(OptionsMenuLayout.Cell(i, slot, model), label, focus,
                            model.HoveredSelection == i, model.PointerDown);
                    }
                }
                else if (model.Tab == 1 && i < 4)
                {
                    var track = new Rectangle(660, row.Y + 20, 278, 20);
                    NineSlice.Draw(renderer, socket, socket.Bounds, track, 4, 2);
                    int volume = i switch { 0 => model.Draft.Master, 1 => model.Draft.Music,
                        2 => model.Draft.Ambient, _ => model.Draft.Effects };
                    NineSlice.Draw(renderer, button, ButtonSource, new(660 + volume * 246 / 100, row.Y + 12, 32, 36), 6, 2);
                    Text(fields[i].Value, 1020, row.Y + 14, Cream);
                }
                else
                {
                    Button(new(642, row.Y, 446, row.Height), fields[i].Value, selected,
                        model.HoveredSelection == i, model.PointerDown);
                }
                // Divider comes from the generated brass button edge, with no stretched corners.
                NineSlice.Tile(renderer, button, new(20, 5, 32, 2), new(188, row.Bottom + 4, 888, 2), 1, Color.White);
            }
            int helpY = OptionsMenuLayout.Viewport.Y + 44 + fields.Count * OptionsMenuLayout.RowHeight(model.Draft.UiScale) - model.Scroll.Offset;
            string help = model.Tab switch
            {
                0 => "Borderless uses desktop size. Resolution is retained for windowed / fullscreen. Apply to test for 15 seconds.",
                1 => "Preview tones only. Game audio is not yet authored. Select a volume and use Left / Right to adjust.",
                2 => OptionsModel.SampleText[..model.Draft.RevealedCharacters(model.TextElapsed, OptionsModel.SampleText.Length)],
                _ => "Left / Right: slot. Enter: rebind. Escape: cancel capture. Delete clears an alternate. Apply saves both keys."
            };
            Wrap(help, 188, helpY, 888, Cream);
        }
        finally { renderer.EndClip(); }
        NineSlice.Draw(renderer, socket, socket.Bounds, OptionsMenuLayout.Track, 4, 2);
        if (model.Scroll.Maximum > 0)
            NineSlice.Draw(renderer, button, ButtonSource, OptionsMenuLayout.Thumb(model), 4, 2);
        Wrap(model.Message ?? "Tab: category   Up / Down: select   Left / Right: change   Mouse wheel: scroll",
            176, 492, 912, Cream, maxLines: 2);
        for (int action = 0; action < 3; action++)
            Button(OptionsMenuLayout.Footer(action), action switch { 0 => "APPLY", 1 => "RESTORE DEFAULTS", _ => "BACK" },
                model.Selected == model.FieldCount + action, model.HoveredSelection == model.FieldCount + action, model.PointerDown);
        if (model.Previewing)
        {
            var panel = OptionsMenuLayout.Panel;
            renderer.Fill(panel.X, panel.Y, panel.Width, panel.Height, Color.Black * 0.65f);
            renderer.DrawTexture(frame, OptionsMenuLayout.Panel);
            NineSlice.Tile(renderer, paper, paper.Bounds, new(184, 248, 912, 232), 2, Color.White);
            Text("KEEP DISPLAY CHANGES?", 200, 254, Ink, "OptionsHeading");
            Text($"Reverting in {(int)Math.Ceiling(model.PreviewRemaining.TotalSeconds)} seconds", 200, 330, Ink);
            Text("Enter: keep and save. Escape / focus loss: revert.", 200, 386, Ink, "OptionsHelp");
            Button(OptionsMenuLayout.Keep, "KEEP", true, false, false);
            Button(OptionsMenuLayout.Revert, "REVERT", false, false, false);
        }
        renderer.Present(clearBackBuffer: drawBackground is null, drawBackground: drawBackground);
    }

    private Rectangle ButtonSource => new(4, 3, 88, 26);
    private void Button(Rectangle bounds, string text, bool selected, bool hover, bool pressed, bool categoryTab = false)
    {
        Color tint = selected ? new Color(255, 232, 161) : hover ? Color.White : new Color(200, 211, 202);
        if (hover && pressed) tint = Teal;
        if (categoryTab)
        {
            int state = hover && pressed ? 3 : hover ? 2 : selected ? 1 : 0;
            int width = (int)MathF.Round(bounds.Width * 0.828f);
            int height = (int)MathF.Round(bounds.Height * 0.828f);
            var artworkBounds = new Rectangle(bounds.X + (bounds.Width - width) / 2,
                bounds.Y + (bounds.Height - height) / 2, width, height);
            renderer.DrawTexture(tabArtwork[state], artworkBounds);
        }
        else NineSlice.Draw(renderer, button, ButtonSource, bounds, 6, 2, tint);
        var size = renderer.MeasureText(text.Font("OptionsHelp"));
        Text(text, bounds.X + (int)(bounds.Width - size.X) / 2,
            bounds.Y + (int)(bounds.Height - size.Y) / 2 + (hover && pressed ? 2 : 0), Cream, "OptionsHelp");
    }

    private void Wrap(string text, int x, int y, int width, Color color, int maxLines = 5)
    {
        string line = "";
        int lines = 0;
        foreach (string word in text.Split(' '))
        {
            string next = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && renderer.MeasureText(next.Font("OptionsHelp")).X > width)
            {
                Text(line, x, y, color, "OptionsHelp");
                y += (int)(24 * renderer.TextScale);
                if (++lines >= maxLines) return;
                line = word;
            }
            else line = next;
        }
        if (line.Length > 0) Text(line, x, y, color, "OptionsHelp");
    }
    private void Text(string text, int x, int y, Color color, string font = "OptionsBody")
    {
        if (color == Cream) renderer.Text(text.Font(font), x + 1, y + 1, Color.Black * 0.85f);
        renderer.Text(text.Font(font), x, y, color);
    }
    public void Dispose()
    {
        frame.Dispose(); button.Dispose(); socket.Dispose(); paper.Dispose(); renderer.Dispose();
        foreach (var texture in tabArtwork) texture.Dispose();
    }
}
