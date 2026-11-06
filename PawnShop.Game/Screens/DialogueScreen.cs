using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;
using PawnShop.Game.Bootstrap;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

internal static class DialogueScreen
{
    public static Rectangle ChoiceRow(int visibleIndex) => new(40, 193 + visibleIndex * 37, 560, 36);

    public static void Draw(PixelRenderer renderer, DialoguePresenter dialogue)
    {
        renderer.Fill(0, 0, 640, 360, Color.Black * 0.65f);
        renderer.Fill(24, 42, 592, 287, new Color(36, 51, 51));
        renderer.Text(dialogue.View.Speaker.Name, 42, 54, Gold);
        renderer.Text(dialogue.View.Emotion, 420, 54, Cream);
        string visible = dialogue.PageText[..dialogue.VisibleCharacters];
        DrawLines(renderer, visible.Split('\n'), 42, 83, Cream);
        if (dialogue.ChoicesVisible)
        {
            for (int row = 0; row < 3; row++)
            {
                int index = dialogue.FirstVisibleChoice + row;
                if (index >= dialogue.View.Choices.Count) break;
                var bounds = ChoiceRow(row);
                bool selected = index == dialogue.SelectedIndex;
                if (selected) renderer.Fill(bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(62, 78, 72));
                renderer.Text(selected ? ">" : " ", 42, bounds.Y + 3, Gold);
                DrawLines(renderer, TextPages.Wrap(dialogue.View.Choices[index].Text, dialogue.Columns - 3),
                    60, bounds.Y + 3, Cream);
            }
        }
        renderer.Text(dialogue.ChoicesVisible ? "MOVE KEYS: CHOOSE  ENTER: OK  ESC: BACK" : "ENTER: REVEAL / NEXT  ESC: BACK", 42, 310, Cream);
        if (dialogue.PageCount > 1) renderer.Text($"{dialogue.Page + 1}/{dialogue.PageCount}", 540, 174, Gold);
    }

    private static void DrawLines(PixelRenderer renderer, IEnumerable<string> lines, int x, int y, Color color)
    {
        foreach (string line in lines)
        {
            renderer.Text(line, x, y, color);
            y += (int)(11 * renderer.TextScale);
        }
    }
}
