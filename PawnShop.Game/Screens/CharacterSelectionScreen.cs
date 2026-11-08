using Microsoft.Xna.Framework;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

public static class CharacterSelectionLayout
{
    public static Rectangle Card(int index) => new(130 + index * 210, 95, 170, 165);
    public static Rectangle Back => new(240, 279, 160, 30);
}

internal static class CharacterSelectionScreen
{
    public static void Draw(PixelRenderer renderer, Menu<MenuCommand> menu, PlayerSpriteRenderer sprites)
    {
        void Center(string text, int centerX, int y, Color color, int size = 1) =>
            renderer.Text(text, centerX - (int)(text.Length * 6 * size * renderer.TextScale / 2), y, color, size);
        Center("CHOOSE YOUR CHARACTER", 320, 36, Gold, 2);
        Center("SELECT A GENDER TO START YOUR NEW GAME", 320, 66, Cream);
        for (int i = 0; i < 2; i++)
        {
            var bounds = CharacterSelectionLayout.Card(i);
            bool selected = menu.SelectedIndex == i;
            renderer.Fill(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4, selected ? Gold : new Color(62, 78, 72));
            renderer.Fill(bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(36, 51, 51));
            sprites.Draw(renderer, i == 0 ? PlayerGender.Male : PlayerGender.Female, Facing.South,
                bounds.Center.X, bounds.Y + 130, 2);
            Center(menu.Items[i].Label.ToUpperInvariant(), bounds.Center.X, bounds.Y + 145, selected ? Gold : Cream);
        }
        var back = CharacterSelectionLayout.Back;
        renderer.Fill(back.X, back.Y, back.Width, back.Height, menu.SelectedIndex == 2 ? new Color(62, 78, 72) : new Color(36, 51, 51));
        Center("BACK", back.Center.X, back.Y + 10, menu.SelectedIndex == 2 ? Gold : Cream);
        Center("MOVE KEYS: SELECT   ENTER: CHOOSE   ESC: BACK", 320, 321, Cream);
    }
}
