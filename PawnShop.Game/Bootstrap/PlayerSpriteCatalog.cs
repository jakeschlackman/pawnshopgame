using System.Collections.Frozen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Bootstrap;

// Validated, graphics-free references. Source/provenance manifests are not runtime state.
public sealed class PlayerSpriteCatalog
{
    public SpriteSheetDefinition Sheet { get; }
    public SpriteSheetDefinition PlaceholderSheet { get; }
    public IReadOnlyDictionary<PlayerGender, string> Textures { get; }
    private readonly FrozenDictionary<Facing, int> frames;

    public PlayerSpriteCatalog(string contentDirectory)
    {
        string metadata = Path.Combine(contentDirectory, "player-sprite.json");
        Sheet = SpriteSheetDefinition.Load(metadata);
        var poses = new Dictionary<Facing, int>();
        foreach (var facing in new[] { Facing.South, Facing.North, Facing.East })
        {
            string name = "Idle" + facing;
            if (!Sheet.Animations.TryGetValue(name, out var clip) || !clip.Loop || clip.FrameCount != 1)
                throw new InvalidDataException($"{metadata} animations.{name}: requires one looping standing pose.");
            poses.Add(facing, clip.FirstFrame);
        }
        frames = poses.ToFrozenDictionary();
        var textures = new Dictionary<PlayerGender, string>();
        foreach (var gender in Enum.GetValues<PlayerGender>())
        {
            string path = Path.Combine(contentDirectory, "Bases", gender == PlayerGender.Male ? "male" : "female", "base.png");
            Sheet.ValidatePng(path, metadata);
            textures.Add(gender, path);
        }
        Textures = textures.ToFrozenDictionary();
        string previewMetadata = Path.Combine(contentDirectory, "placeholder-sprite.json");
        PlaceholderSheet = SpriteSheetDefinition.Load(previewMetadata);
        if (PlaceholderSheet.FrameWidth != 48 || PlaceholderSheet.FrameHeight != 64
            || PlaceholderSheet.Columns != 4 || PlaceholderSheet.Rows != 3
            || PlaceholderSheet.Pivot != new Vector2(24, 58) || PlaceholderSheet.DisplayScale != 1
            || !PlaceholderSheet.Animations.TryGetValue("Idle", out var idle) || idle.FirstFrame != 0 || idle.FrameCount != 1 || !idle.Loop
            || !PlaceholderSheet.Animations.TryGetValue("Walk", out var walk) || walk.FirstFrame != 0 || walk.FrameCount != 4 || !walk.Loop)
            throw new InvalidDataException($"{previewMetadata}: requires 48x64 frames, four columns, S/N/E rows, pivot (24,58), scale 1, Idle (0,1) and Walk (0,4) loops.");
    }

    public Rectangle Frame(Facing facing) => Sheet.GetFrame(frames[facing == Facing.West ? Facing.East : facing]);

    public static SpriteEffects Effects(Facing facing) => facing == Facing.West ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

    // All layers use this same canvas and feet pivot. Reflect the pivot too, even if a later rig is off-center.
    public static Rectangle Bounds(SpriteSheetDefinition sheet, Facing facing, float x, float y, float scale = 1)
    {
        float pivotX = facing == Facing.West ? sheet.FrameWidth - sheet.Pivot.X : sheet.Pivot.X;
        return new((int)MathF.Round(x - pivotX * scale), (int)MathF.Round(y - sheet.Pivot.Y * scale),
            (int)MathF.Round(sheet.FrameWidth * scale), (int)MathF.Round(sheet.FrameHeight * scale));
    }
}
