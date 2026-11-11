using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;

namespace PawnShop.Characters.Player.Create;

public static class CharacterSelectionLayout
{
    public static Rectangle Card(int index) => new(130 + index * 210, 95, 170, 165);
    public static Rectangle Back => new(240, 279, 160, 30);

    public static Vector2 SelectorAnchor(int index, float textScale)
    {
        var bounds = index == 2 ? Back : Card(index);
        string label = index switch { 0 => "MALE", 1 => "FEMALE", 2 => "BACK", _ => throw new ArgumentOutOfRangeException(nameof(index)) };
        float y = index == 2 ? bounds.Y + 10 : bounds.Y + 145;
        return new(bounds.Center.X - label.Length * 6 * textScale / 2 - 14, y + 3.5f * textScale);
    }

    // Camera position already contains the easing; use it directly instead of running a second clock.
    public static Vector2 FlightAnchor(Vector2 departure, double cameraPosition, float textScale) =>
        Vector2.Lerp(departure, SelectorAnchor(0, textScale), (float)Math.Clamp(-cameraPosition, 0, 1));
}

public static class PlayerModel_SelectionScreen
{
    private static readonly Color Cream = new(241, 225, 191);
    private static readonly Color Gold = new(237, 184, 89);

    // The host supplies preview drawing so shared player textures retain their application lifetime.
    public static void Draw<TCommand>(PixelRenderer renderer, Menu<TCommand> menu,
        Action<PixelRenderer, PlayerGender, Facing, float, float, float> drawPlayer)
        where TCommand : struct, Enum
    {
        void Center(string text, int centerX, int y, Color color, int size = 1) =>
            renderer.Text(text, centerX - (int)(text.Length * 6 * size * renderer.TextScale / 2), y, color, size);
        Center("CHOOSE YOUR CHARACTER", 320, 36, Gold, 2);
        Center("SELECT A GENDER TO START YOUR NEW GAME", 320, 66, Cream);
        for (int i = 0; i < 2; i++)
        {
            var bounds = CharacterSelectionLayout.Card(i);
            bool selected = menu.SelectedIndex == i;
            // Translucent panels keep the wooded clearing visible behind both models.
            renderer.Fill(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4, (selected ? Gold : new Color(62, 78, 72)) * .55f);
            renderer.Fill(bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(26, 36, 38) * .55f);
            drawPlayer(renderer, i == 0 ? PlayerGender.Male : PlayerGender.Female, Facing.South,
                bounds.Center.X, bounds.Y + 130, 2);
            Center(menu.Items[i].Label.ToUpperInvariant(), bounds.Center.X, bounds.Y + 145, selected ? Gold : Cream);
        }
        var back = CharacterSelectionLayout.Back;
        renderer.Fill(back.X, back.Y, back.Width, back.Height, menu.SelectedIndex == 2 ? new Color(62, 78, 72) : new Color(36, 51, 51));
        Center("BACK", back.Center.X, back.Y + 10, menu.SelectedIndex == 2 ? Gold : Cream);
        Center("MOVE KEYS: SELECT   ENTER: CHOOSE   ESC: BACK", 320, 321, Cream);
    }
}
