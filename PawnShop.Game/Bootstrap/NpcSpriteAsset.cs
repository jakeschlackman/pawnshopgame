using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Bootstrap;

public sealed record NpcSpriteAsset(string TexturePath, SpriteSheetDefinition Sheet, string IdleAnimation)
{
    public static NpcSpriteAsset Load(string texturePath, string metadataPath, string animation)
    {
        var sheet = SpriteSheetDefinition.Load(metadataPath);
        if (!sheet.Animations.TryGetValue(animation, out var idle) || !idle.Loop)
            throw new InvalidDataException($"{metadataPath} animations.{animation}: NPC idle animation must exist and loop.");
        // Header inspection is headless. Texture decoding happens only in the room's graphics scope.
        sheet.ValidatePng(texturePath, metadataPath);
        return new(texturePath, sheet, animation);
    }
}
