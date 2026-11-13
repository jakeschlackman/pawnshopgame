using System.Collections.Frozen;
using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;

namespace PawnShop.Characters.Player.Create;

// Selection-only art; gameplay appearance and common-NPC catalogs remain independent.
public sealed class CharacterSelectionCatalog
{
    public string Root { get; }
    public IReadOnlyDictionary<PlayerGender, SpriteSheetDefinition> Sheets { get; }
    public SpriteSheetDefinition Tree { get; }
    public static IReadOnlyList<string> ClipNames { get; } = Array.AsReadOnly(new[]
    {
        "LeaningIdle", "PushOff", "WalkSouth", "WalkNorth", "TurnToTree", "TurnToCamera",
        "Flourish", "SelectedIdle", "Settle"
    });

    public CharacterSelectionCatalog(string root)
    {
        Root = root;
        var sheets = new Dictionary<PlayerGender, SpriteSheetDefinition>();
        foreach (var gender in Enum.GetValues<PlayerGender>())
        {
            string name = gender == PlayerGender.Male ? "male" : "female";
            string metadata = Path.Combine(root, name + ".json");
            var sheet = SpriteSheetDefinition.Load(metadata);
            if (sheet.FrameWidth != 48 || sheet.FrameHeight != 64 || sheet.Pivot != new Vector2(24, 60)
                || sheet.DisplayScale != 2)
                throw new InvalidDataException($"{metadata}: requires native 48x64 frames, pivot (24,60), displayScale 2.");
            foreach (string clip in ClipNames)
                if (!sheet.Animations.TryGetValue(clip, out var animation)
                    || animation.Loop != (clip is "LeaningIdle" or "SelectedIdle" or "WalkSouth" or "WalkNorth"))
                    throw new InvalidDataException($"{metadata}: animations.{clip}: missing clip or incorrect loop policy.");
            sheet.ValidatePng(Path.Combine(root, name + ".png"), metadata);
            sheets.Add(gender, sheet);
        }
        Sheets = sheets.ToFrozenDictionary();
        string treeMetadata = Path.Combine(root, "tree.json");
        Tree = SpriteSheetDefinition.Load(treeMetadata);
        Tree.ValidatePng(Path.Combine(root, "tree.png"), treeMetadata);
        if (Tree.Columns != 1 || Tree.Rows != 1)
            throw new InvalidDataException($"{treeMetadata}: requires a single independent tree sprite.");
    }
}
