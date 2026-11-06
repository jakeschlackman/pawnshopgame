using System.Buffers.Binary;
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
        Span<byte> header = stackalloc byte[24];
        try
        {
            using var stream = File.OpenRead(texturePath);
            stream.ReadExactly(header);
        }
        catch (IOException e) { throw new InvalidDataException($"{texturePath}: Cannot read PNG dimensions.", e); }
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (!header[..8].SequenceEqual(signature) || !header.Slice(12, 4).SequenceEqual("IHDR"u8))
            throw new InvalidDataException($"{texturePath}: Expected a PNG image.");
        int width = BinaryPrimitives.ReadInt32BigEndian(header.Slice(16, 4));
        int height = BinaryPrimitives.ReadInt32BigEndian(header.Slice(20, 4));
        if (width != sheet.ImageWidth || height != sheet.ImageHeight)
            throw new InvalidDataException($"{texturePath}: PNG is {width}x{height}; {metadataPath} requires {sheet.ImageWidth}x{sheet.ImageHeight}.");
        return new(texturePath, sheet, animation);
    }
}
