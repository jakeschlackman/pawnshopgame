using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;
using PawnShop.Game.Bootstrap;
using static PawnShop.Game.Screens.ScreenColors;

namespace PawnShop.Game.Screens;

internal static class MenuScreen
{
    public static void Draw(PixelRenderer renderer, ApplicationFlow flow, Menu<MenuCommand> menu)
    {
        bool title = flow.Screen == Screen.Title;
        int top = title ? 148 : 136;
        if (!title)
        {
            renderer.Fill(0, 0, renderer.Width, renderer.Height, Color.Black * 0.65f);
            renderer.Fill(100, 64, 440, 230, new Color(36, 51, 51));
            string heading = flow.Screen switch
            {
                Screen.Pause => "PAUSED",
                Screen.ConfirmReturn => "RETURN TO TITLE?",
                _ => "EXIT GAME?"
            };
            renderer.Text(heading, 120, 82, Gold, 2);
            renderer.Text(flow.Screen == Screen.Pause ? "THE SHOP CAN WAIT." : "UNSAVED PROGRESS WILL BE LOST.", 120, 111, Cream);
        }
        int left = title ? 48 : 120;
        for (int i = 0; i < menu.Items.Count; i++)
        {
            var item = menu.Items[i];
            int y = top + i * 32;
            if (i == menu.SelectedIndex)
            {
                renderer.Fill(left - 6, y - 7, title ? 540 : 404, 27, new Color(62, 78, 72));
                // This animation uses presentation time and continues while paused.
                renderer.Text(">", left, y, (int)(flow.PresentationTime.TotalSeconds * 2) % 2 == 0 ? Gold : Cream);
            }
            renderer.Text(item.Label, left + 18, y, item.Enabled ? Cream : new Color(166, 179, 168), 1);
        }
        if (menu.Selected.DisabledReason is { } reason) renderer.Text(reason, left, 287, Gold);
        renderer.Text("UP/DOWN: SELECT   ENTER/SPACE: CONFIRM   ESC: BACK", title ? 48 : 120,
            title ? 310 : 270, Cream);
    }
}
