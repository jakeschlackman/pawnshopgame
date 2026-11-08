using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

internal sealed class OptionsScreen : IDisposable
{
    private readonly PixelRenderer renderer;
    private readonly Texture2D frame, button, confirmationPanel, confirmationSurface;
    private readonly Texture2D[] tabArtwork;
    private readonly Texture2D[] actionArtwork;
    private readonly Texture2D[] dropdownArtwork;
    private readonly Texture2D[] bindingArtwork;
    private readonly Texture2D dropdownPanel, dropdownHighlight, dropdownArrow;
    private readonly Texture2D audioTrack, audioFill;
    private readonly Texture2D[] audioWatch;
    private readonly Texture2D[] muteCheckbox;
    private readonly Texture2D scrollRail;
    private readonly Texture2D[] scrollKnob;
    private static readonly Color Cream = new(255, 240, 204);
    private static readonly Color Teal = new(130, 180, 165);

    public OptionsScreen(GraphicsDevice device, FontLibrary fonts)
    {
        renderer = new(device, 1280, 720, integerScaling: false, fonts: fonts);
        frame = Load(device, "options-panel-source.png");
        confirmationPanel = Load(device, "ivywood-confirmation.png");
        confirmationSurface = Load(device, "modal-brown-gradient.png");
        button = Load(device, "brass-button.png");
        tabArtwork = [Load(device, "tab-inactive.png"), Load(device, "tab-selected.png"),
            Load(device, "tab-hovered.png"), Load(device, "tab-pressed.png")];
        actionArtwork = [Load(device, "action-rail-inactive.png"), Load(device, "action-rail-selected.png"),
            Load(device, "action-rail-hovered.png"), Load(device, "action-rail-pressed.png")];
        dropdownArtwork = [Load(device, "dropdown-inactive.png"), Load(device, "dropdown-selected.png"),
            Load(device, "dropdown-hovered.png"), Load(device, "dropdown-pressed.png")];
        dropdownPanel = Load(device, "dropdown-list.png");
        bindingArtwork = [Load(device, "binding-inactive.png"), Load(device, "binding-selected.png"),
            Load(device, "binding-hovered.png"), Load(device, "binding-pressed.png")];
        dropdownHighlight = Load(device, "dropdown-highlight.png");
        dropdownArrow = Load(device, "dropdown-arrow.png");
        audioTrack = Load(device, "audio-track.png");
        audioFill = Load(device, "audio-fill.png");
        audioWatch = [Load(device, "audio-watch-inactive.png"), Load(device, "audio-watch-selected.png"),
            Load(device, "audio-watch-hovered.png"), Load(device, "audio-watch-pressed.png")];
        muteCheckbox = [Load(device, "checkbox-steel-unchecked.png"), Load(device, "checkbox-gold-checked.png"),
            Load(device, "checkbox-steel-unchecked-hovered.png"), Load(device, "checkbox-gold-checked-hovered.png")];
        scrollRail = Load(device, "scroll-walnut-rail.png");
        scrollKnob = [Load(device, "scroll-compass-inactive.png"), Load(device, "scroll-compass-hovered.png"),
            Load(device, "scroll-compass-pressed.png")];
    }

    private static Texture2D Load(GraphicsDevice device, string name)
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Content", "UI", name));
        return Texture2D.FromStream(device, stream, DefaultColorProcessors.PremultiplyAlpha);
    }

    public Vector2 SelectorAnchor(OptionsModel model, bool first = false) => OptionsMenuLayout.SelectorAnchor(model, first);

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
            int textX = OptionsMenuLayout.Viewport.X + 12;
            int textWidth = OptionsMenuLayout.Viewport.Width - 24;
            string labelFont = model.Tab is 0 or 1 ? "OptionsDropdownLabel" : "OptionsLabel";
            int labelLineHeight = 28;
            string helpFont = "OptionsHelp";
            int helpLineHeight = 22;
            if (model.Tab == 3)
            {
                Text("PRIMARY", OptionsMenuLayout.Cell(0, 0, model).X + 10, headerY, Cream, "OptionsHelp");
                Text("ALTERNATE", OptionsMenuLayout.Cell(0, 1, model).X + 10, headerY, Cream, "OptionsHelp");
            }
            var fields = model.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                var row = OptionsMenuLayout.Field(i, model);
                if (!row.Intersects(OptionsMenuLayout.Viewport)) continue;
                bool selected = i == model.Selected;
                int labelWidth = OptionsMenuLayout.ValueBounds(i, model).X - textX - 20;
                var labelSize = renderer.MeasureText(fields[i].Label.Font(labelFont));
                int labelY = row.Y + (labelSize.X <= labelWidth ? (row.Height - (int)labelSize.Y) / 2 : 4);
                Wrap(fields[i].Label, textX, labelY, labelWidth, Cream, maxLines: 2,
                    font: labelFont, lineHeight: labelLineHeight);
                if (model.Tab == 3)
                {
                    var binding = model.Binding(i);
                    for (int slot = 0; slot < 2; slot++)
                    {
                        bool focus = selected && slot == model.SelectedBinding;
                        string label = model.CapturingKey && focus ? "Press Input" :
                            slot == 0 ? OptionsModel.KeyLabel(binding.Primary) : OptionsModel.AlternateLabel(binding);
                        Button(OptionsMenuLayout.Cell(i, slot, model), label, focus,
                            model.HoveredSelection == i && model.HoveredBinding == slot,
                            model.PointerDown || model.CapturingKey && focus, bindingSlot: true);
                    }
                }
                else if (model.Tab == 1 && i < OptionsModel.AudioVolumeCount)
                {
                    var value = OptionsMenuLayout.ValueBounds(i, model);
                    var track = OptionsMenuLayout.AudioTrack(i, model);
                    var fill = OptionsMenuLayout.AudioFill(i, model);
                    int filledWidth = fill.Width * model.Volume(i) / 100;
                    NineSlice.Draw(renderer, audioTrack, audioTrack.Bounds, track, 6, 1);
                    if (filledWidth > 0)
                        NineSlice.Tile(renderer, audioFill, new(2, 0, audioFill.Width - 4, audioFill.Height),
                            new(fill.X, fill.Y, filledWidth, fill.Height), 1, Color.White);
                    bool hover = model.HoveredSelection == i;
                    int state = hover && model.PointerDown ? 3 : hover ? 2 : selected ? 1 : 0;
                    renderer.DrawTexture(audioWatch[state], OptionsMenuLayout.AudioThumb(i, model));
                    var size = renderer.MeasureText(fields[i].Value.Font("OptionsDropdown"));
                    Text(fields[i].Value, value.Right - 68, value.Center.Y - (int)size.Y / 2, Cream, "OptionsDropdown");
                }
                else if (model.Tab == 1 || model.Tab == 2 && i is 0 or 1)
                {
                    bool hover = model.HoveredSelection == i;
                    bool enabled = model.Tab == 1 ? model.Draft.MuteUnfocused
                        : i == 0 ? model.Draft.PauseUnfocused : model.Draft.ScreenShake;
                    int state = (enabled ? 1 : 0) + (hover ? 2 : 0);
                    var texture = muteCheckbox[state];
                    renderer.DrawTexture(texture, OptionsMenuLayout.Checkbox(i, model), texture.Bounds,
                        hover && model.PointerDown ? new Color(220, 220, 220) : Color.White);
                }
                else if (model.IsDropdown(i))
                {
                    DropdownButton(OptionsMenuLayout.ValueBounds(i, model), fields[i].Value,
                        selected || model.DropdownRow == i, model.HoveredSelection == i, model.PointerDown);
                }
                else
                {
                    Button(OptionsMenuLayout.ValueBounds(i, model), fields[i].Value, selected,
                        model.HoveredSelection == i, model.PointerDown,
                        fontRole: model.Tab == 1 ? "OptionsDropdown" : null);
                }
                // Divider comes from the generated brass button edge, with no stretched corners.
                NineSlice.Tile(renderer, button, new(20, 5, 32, 2), new(textX, row.Bottom + 4, textWidth, 2), 1, Color.White);
            }
            int helpY = OptionsMenuLayout.Viewport.Y + OptionsMenuLayout.HelpTop(model) - model.Scroll.Offset;
            string help = model.Tab switch
            {
                0 or 1 => string.Empty,
                2 => model.SampleText[..model.Draft.RevealedCharacters(model.TextElapsed, model.SampleText.Length)],
                _ => string.Empty
            };
            Wrap(help, textX, helpY, textWidth, Cream, maxLines: OptionsMenuLayout.HelpLines(model),
                font: helpFont, lineHeight: helpLineHeight);
            if (model.Message is { } message)
                Wrap(message, textX, helpY + (OptionsMenuLayout.HelpLines(model) + 1)
                    * OptionsMenuLayout.HelpLineHeight(model), textWidth, Cream, maxLines: 2,
                    font: helpFont, lineHeight: helpLineHeight);
            if (model.DropdownRow is not null) DrawDropdown(model);
        }
        finally { renderer.EndClip(); }
        if (model.Scroll.Maximum > 0)
        {
            NineSlice.Draw(renderer, scrollRail, scrollRail.Bounds, OptionsMenuLayout.Track, 16, 1);
            int state = model.HoveredScrollbar ? model.PointerDown ? 2 : 1 : 0;
            renderer.DrawTexture(scrollKnob[state], OptionsMenuLayout.Thumb(model));
        }
        for (int action = 0; action < 3; action++)
            Button(OptionsMenuLayout.Footer(action), action switch { 0 => "APPLY", 1 => "RESTORE DEFAULTS", _ => "BACK" },
                model.Selected == model.FieldCount + action, model.HoveredSelection == model.FieldCount + action, model.PointerDown,
                footerAction: true);
        if (model.HasModal)
        {
            bool discard = model.ConfirmingDiscard;
            var dialog = OptionsMenuLayout.DisplayConfirmation;
            renderer.Fill(0, 0, renderer.Width, renderer.Height, Color.Black * 0.55f);
            // This inset fills the transparent frame center. Its native height
            // matches the surface; only horizontal repetitions preserve the gradient.
            NineSlice.Tile(renderer, confirmationSurface, confirmationSurface.Bounds,
                new(dialog.X + 21, dialog.Y + 24, dialog.Width - 42, dialog.Height - 47), 1, Color.White);
            // Large corner slices retain the complete ivy clusters. Only plain rail
            // and quiet inner pixels repeat; corner artwork stays at native scale.
            NineSlice.Draw(renderer, confirmationPanel, confirmationPanel.Bounds, dialog, 64, 1);
            CenterDialogText(discard ? "Unapplied Changes" : "Keep Display Changes?", dialog.Top + 32, "OptionsHeading");
            CenterDialogText(discard ? "Leave without applying your changes?"
                : $"Reverting in {(int)Math.Ceiling(model.PreviewRemaining.TotalSeconds)} seconds",
                dialog.Top + 114, "OptionsLabel");
            CenterDialogText(discard ? "Enter: Select   Escape: Keep Editing" : "Enter: Keep   Escape: Revert",
                dialog.Top + 172, "OptionsLabel");
            Button(OptionsMenuLayout.Keep, discard ? "Keep Editing" : "Keep", !discard || model.DiscardChoice == 0,
                model.HoveredModalAction == 0,
                model.PointerDown, fontRole: "OptionsLabel", confirmationAction: true);
            Button(OptionsMenuLayout.Revert, discard ? "Discard Changes" : "Revert", discard && model.DiscardChoice == 1,
                model.HoveredModalAction == 1,
                model.PointerDown, fontRole: "OptionsLabel", confirmationAction: true);
        }
        renderer.Present(clearBackBuffer: drawBackground is null, drawBackground: drawBackground);
    }

    private void CenterDialogText(string text, int y, string font)
    {
        var size = renderer.MeasureText(text.Font(font));
        Text(text, OptionsMenuLayout.DisplayConfirmation.Center.X - (int)size.X / 2, y, Cream, font);
    }

    private Rectangle ButtonSource => new(4, 3, 88, 26);
    private void DropdownButton(Rectangle bounds, string text, bool selected, bool hover, bool pressed)
    {
        int state = hover && pressed ? 3 : hover ? 2 : selected ? 1 : 0;
        var texture = dropdownArtwork[state];
        NineSlice.Draw(renderer, texture, texture.Bounds, bounds, 8, 2);
        var size = renderer.MeasureText(text.Font("OptionsDropdown"));
        Text(text, bounds.X + 18, bounds.Y + (bounds.Height - (int)size.Y) / 2
            + 2 + (hover && pressed ? 2 : 0), Cream, "OptionsDropdown");
        renderer.DrawTexture(dropdownArrow, new(bounds.Right - dropdownArrow.Width * 2 - 16,
            bounds.Center.Y - dropdownArrow.Height, dropdownArrow.Width * 2, dropdownArrow.Height * 2));
    }

    private void DrawDropdown(OptionsModel model)
    {
        var list = OptionsMenuLayout.Dropdown(model);
        NineSlice.Draw(renderer, dropdownPanel, dropdownPanel.Bounds, list, 8, 2);
        int visible = OptionsMenuLayout.DropdownVisibleCount(model);
        for (int row = 0; row < visible; row++)
        {
            int index = model.DropdownFirst + row;
            var bounds = OptionsMenuLayout.DropdownItem(row, model);
            if (index == model.HoveredChoice || index == model.DropdownChoice)
                NineSlice.Draw(renderer, dropdownHighlight, dropdownHighlight.Bounds, bounds, 4, 2);
            string label = model.ChoiceLabel(index);
            var size = renderer.MeasureText(label.Font("OptionsDropdown"));
            Text(label, bounds.X + 12, bounds.Y + (bounds.Height - (int)size.Y) / 2 + 2, Cream, "OptionsDropdown");
        }
        if (model.DropdownCount > visible)
        {
            NineSlice.Tile(renderer, dropdownPanel, new(12, 12, 3, 4),
                OptionsMenuLayout.DropdownTrack(model), 2, Color.White);
            NineSlice.Tile(renderer, dropdownHighlight, new(6, 6, 3, 4),
                OptionsMenuLayout.DropdownThumb(model), 2, Color.White);
        }
    }

    private void Button(Rectangle bounds, string text, bool selected, bool hover, bool pressed,
        bool categoryTab = false, bool footerAction = false, string? fontRole = null, bool bindingSlot = false,
        bool confirmationAction = false)
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
        else if (footerAction)
        {
            int state = hover && pressed ? 3 : hover ? 2 : selected ? 1 : 0;
            var texture = actionArtwork[state];
            // The source omits its lower trim: only the original panel rail
            // supplies the bottom border where these bounds meet it.
            // Include the inset shadow and its corner pixels in the fixed sides
            // so neither seams nor corner dots repeat through a wider button.
            NineSlice.Draw(renderer, texture, texture.Bounds, bounds, 6, 2);
        }
        else if (bindingSlot || confirmationAction)
        {
            int state = pressed && (hover || bindingSlot && selected) ? 3 : hover ? 2 : selected ? 1 : 0;
            var texture = bindingArtwork[state];
            // Confirmation actions reuse the complete closed walnut/brass border.
            NineSlice.Draw(renderer, texture, texture.Bounds, bounds, 6, 2);
        }
        else NineSlice.Draw(renderer, button, ButtonSource, bounds, 6, 2, tint);
        string font = categoryTab ? "OptionsTab" : footerAction ? "OptionsAction" : fontRole ?? "OptionsBody";
        var size = renderer.MeasureText(text.Font(font));
        Text(text, bounds.X + (int)(bounds.Width - size.X) / 2,
            bounds.Y + (int)(bounds.Height - size.Y) / 2 + (footerAction ? 3 : 0)
                + (hover && pressed ? 2 : 0), Cream, font);
    }

    private void Wrap(string text, int x, int y, int width, Color color, int maxLines = 5,
        string font = "OptionsHelp", int lineHeight = 28)
    {
        string line = "";
        int lines = 0;
        foreach (string word in text.Split(' '))
        {
            string next = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && renderer.MeasureText(next.Font(font)).X > width)
            {
                Text(line, x, y, color, font);
                y += (int)(lineHeight * renderer.TextScale);
                if (++lines >= maxLines) return;
                line = word;
            }
            else line = next;
        }
        if (line.Length > 0) Text(line, x, y, color, font);
    }
    private void Text(string text, int x, int y, Color color, string font = "OptionsBody")
    {
        if (color == Cream) renderer.Text(text.Font(font), x + 1, y + 1, Color.Black * 0.85f);
        renderer.Text(text.Font(font), x, y, color);
    }
    public void Dispose()
    {
        frame.Dispose(); button.Dispose(); confirmationPanel.Dispose(); confirmationSurface.Dispose(); renderer.Dispose();
        foreach (var texture in tabArtwork) texture.Dispose();
        foreach (var texture in actionArtwork) texture.Dispose();
        foreach (var texture in dropdownArtwork) texture.Dispose();
        foreach (var texture in bindingArtwork) texture.Dispose();
        dropdownPanel.Dispose(); dropdownHighlight.Dispose(); dropdownArrow.Dispose();
        audioTrack.Dispose(); audioFill.Dispose();
        foreach (var texture in audioWatch) texture.Dispose();
        foreach (var texture in muteCheckbox) texture.Dispose();
        scrollRail.Dispose();
        foreach (var texture in scrollKnob) texture.Dispose();
    }
}
