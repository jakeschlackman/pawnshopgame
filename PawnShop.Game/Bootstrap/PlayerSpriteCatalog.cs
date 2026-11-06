using System.Collections.Frozen;
using Microsoft.Xna.Framework;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Bootstrap;

// Validated, graphics-free references. Source/provenance manifests are not runtime state.
public sealed class PlayerSpriteCatalog
{
    public SpriteSheetDefinition Sheet { get; }
    public IReadOnlyDictionary<PlayerGender, string> Textures { get; }
    private readonly FrozenDictionary<Facing, int> frames;

    public PlayerSpriteCatalog(string contentDirectory)
    {
        string metadata = Path.Combine(contentDirectory, "player-sprite.json");
        Sheet = SpriteSheetDefinition.Load(metadata);
        var poses = new Dictionary<Facing, int>();
        foreach (var facing in Enum.GetValues<Facing>())
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
    }

    public Rectangle Frame(Facing facing) => Sheet.GetFrame(frames[facing]);
}
