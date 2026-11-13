using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.UI;

namespace PawnShop.Characters.Player.Create;

public static class CharacterSelectionLayout
{
    public static Rectangle Card(int index) => new(222 + index * 100, 309, 96, 22);
    public static Rectangle Back => new(280, 333, 80, 20);
    public static Vector2 TreeFoot(PlayerGender gender) => new(gender == PlayerGender.Male ? 286 : 354, 260);
    public static Vector2 ForegroundFoot(PlayerGender gender) => new(gender == PlayerGender.Male ? 270 : 370, 304);
    public static Rectangle ActorBounds(SelectionActor actor) => new((int)MathF.Round(actor.Position.X) - 48,
        (int)MathF.Round(actor.Position.Y) - 120, 96, 128);
    public static Rectangle ActorHitBounds(SelectionActor actor) => new((int)MathF.Round(actor.Position.X) - 28,
        (int)MathF.Round(actor.Position.Y) - 114, 56, 116);

    public static Vector2 SelectorAnchor(int index, float textScale)
    {
        var bounds = index == 2 ? Back : Card(index);
        string label = index switch { 0 => "MALE", 1 => "FEMALE", 2 => "BACK", _ => throw new ArgumentOutOfRangeException(nameof(index)) };
        float y = bounds.Y + 5;
        return new(bounds.Center.X - label.Length * 6 * textScale / 2 - 14, y + 3.5f * textScale);
    }

    // Camera position already contains the easing; use it directly instead of running a second clock.
    public static Vector2 FlightAnchor(Vector2 departure, double cameraPosition, float textScale) =>
        Vector2.Lerp(departure, SelectorAnchor(0, textScale), (float)Math.Clamp(-cameraPosition, 0, 1));
}

public sealed class PlayerModel_SelectionScreen : IDisposable
{
    private static readonly Color Cream = new(241, 225, 191);
    private static readonly Color Gold = new(237, 184, 89);

    private readonly CharacterSelectionCatalog catalog;
    private readonly Dictionary<PlayerGender, Texture2D> people = new();
    private readonly Texture2D tree;

    public PlayerModel_SelectionScreen(GraphicsDevice device, CharacterSelectionCatalog catalog)
    {
        this.catalog = catalog;
        try
        {
            tree = Load("tree.png");
            people.Add(PlayerGender.Male, Load("male.png"));
            people.Add(PlayerGender.Female, Load("female.png"));
        }
        catch { Dispose(); throw; }
        Texture2D Load(string name)
        {
            using var stream = File.OpenRead(Path.Combine(catalog.Root, name));
            return Texture2D.FromStream(device, stream, DefaultColorProcessors.PremultiplyAlpha);
        }
    }

    public void Draw<TCommand>(PixelRenderer renderer, Menu<TCommand> menu,
        CharacterSelectionPresentation presentation, double cameraPosition, bool showUi)
        where TCommand : struct, Enum
    {
        // Follow the existing eased camera: the independent tree and actors enter from the left.
        int offset = (int)Math.Round(-640 * Math.Clamp(1 + cameraPosition, 0, 1));
        var treeSheet = catalog.Tree;
        renderer.DrawTexture(tree, new Rectangle(320 + offset - (int)(treeSheet.Pivot.X * treeSheet.DisplayScale),
            276 - (int)(treeSheet.Pivot.Y * treeSheet.DisplayScale),
            (int)(treeSheet.FrameWidth * treeSheet.DisplayScale), (int)(treeSheet.FrameHeight * treeSheet.DisplayScale)));
        // Feet determine depth. Stable order resolves equal resting anchors.
        if (presentation.Male.Position.Y <= presentation.Female.Position.Y)
        { DrawActor(presentation.Male); DrawActor(presentation.Female); }
        else { DrawActor(presentation.Female); DrawActor(presentation.Male); }
        if (!showUi) return;
        void DrawActor(SelectionActor actor)
        {
            var sheet = catalog.Sheets[actor.Gender];
            var bounds = CharacterSelectionLayout.ActorBounds(actor);
            bounds.X += offset;
            renderer.DrawTexture(people[actor.Gender], bounds, sheet.GetFrame(actor.Frame));
        }
        void Center(string text, int centerX, int y, Color color, int size = 1) =>
            renderer.Text(text, centerX - (int)(text.Length * 6 * size * renderer.TextScale / 2), y, color, size);
        Center("CHOOSE YOUR CHARACTER", 320, 20, Gold, 2);
        for (int i = 0; i < 2; i++)
        {
            var bounds = CharacterSelectionLayout.Card(i);
            bool selected = menu.SelectedIndex == i;
            renderer.Fill(bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(26, 36, 38) * .7f);
            Center(menu.Items[i].Label.ToUpperInvariant(), bounds.Center.X, bounds.Y + 5, selected ? Gold : Cream);
        }
        var back = CharacterSelectionLayout.Back;
        renderer.Fill(back.X, back.Y, back.Width, back.Height, menu.SelectedIndex == 2 ? new Color(62, 78, 72) : new Color(36, 51, 51));
        Center("BACK", back.Center.X, back.Y + 5, menu.SelectedIndex == 2 ? Gold : Cream);
        if (presentation.IsConfirming) Center("GETTING READY...", 320, 44, Cream);
    }

    public void Dispose()
    {
        foreach (var texture in people.Values) texture.Dispose();
        people.Clear();
        tree?.Dispose();
    }
}
