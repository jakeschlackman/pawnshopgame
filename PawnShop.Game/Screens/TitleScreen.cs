using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

internal static class TitleScreen
{
    public static void Draw(PixelRenderer renderer)
    {
        renderer.Fill(18, 18, 604, 308, new Color(36, 51, 51));
        renderer.Fill(36, 36, 568, 4, Gold);
        renderer.Text("PAWN SHOP", 48, 54, Cream, 4);
        renderer.Text("EVERYTHING DESERVES A SECOND CHANCE.", 49, 98, Gold);
        renderer.Text("EVEN THIS EMPTY SHOP.", 49, 113, Cream);
    }
}
