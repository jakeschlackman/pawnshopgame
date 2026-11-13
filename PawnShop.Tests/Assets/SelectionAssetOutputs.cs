using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text.Json.Nodes;
using PawnShop.Characters.Player.Create;
using PawnShop.Framework.Rendering;

namespace PawnShop.Tests;

internal static partial class PlayerAssets
{
    // This small packing recipe is for the fixed protagonist selection art, not modular NPC attire.
    // Copies source pixels exactly; no resizing, palette changes or per-frame recentering.
    [SupportedOSPlatform("windows")]
    internal static void PackSelection(string sourceRoot, string destination)
    {
        string output = NewOutput(sourceRoot, destination);
        var manifest = Read(Path.Combine(sourceRoot, "selection-sources.json"));
        Require(Number(manifest, "version") == 1, "selection-sources.json: unsupported version.");
        var bodies = Array(manifest, "bodies");
        Require(bodies.Select(b => Text(b!, "sex")).Order().SequenceEqual(new[] { "female", "male" }),
            "selection-sources.json: requires both protagonists exactly once.");
        // Validate and assemble everything in memory before writing a fresh destination.
        var packed = new List<(string Sex, Bitmap Image, JsonObject Metadata)>();
        var evidence = new JsonArray();
        try
        {
            foreach (var body in bodies)
            {
                string sex = Text(body!, "sex");
                var clips = Array(body!, "clips");
                Require(clips.Select(c => Text(c!, "name")).Order().SequenceEqual(CharacterSelectionCatalog.ClipNames.Order()),
                    $"{sex}: requires each selection clip exactly once.");
                int count = clips.Sum(c => Number(c!, "count") - (c!["skipFirst"]?.GetValue<int>() ?? 0));
                Require(count is > 0 and <= 256, $"{sex}: invalid total frame count.");
                var atlas = new Bitmap(48 * 8, 64 * ((count + 7) / 8), PixelFormat.Format32bppArgb);
                var animations = new JsonObject();
                var metadata = new JsonObject { ["version"] = 1, ["frameWidth"] = 48, ["frameHeight"] = 64,
                    ["columns"] = 8, ["rows"] = (count + 7) / 8, ["pivotX"] = 24, ["pivotY"] = 60,
                    ["displayScale"] = 2, ["animations"] = animations };
                packed.Add((sex, atlas, metadata));
                int first = 0;
                foreach (var clip in clips)
                {
                    string name = Text(clip!, "name"), relative = Text(clip!, "file"), path = Within(sourceRoot, relative);
                    CheckHash(path, Text(clip!, "sha256"));
                    using var source = Open(path);
                    int size = Number(clip!, "cell"), columns = Number(clip!, "columns"), frames = Number(clip!, "count");
                    int skip = clip!["skipFirst"]?.GetValue<int>() ?? 0;
                    Require(size >= 64 && size % 2 == 0 && frames > 0 && columns > 0
                        && source.Width == columns * size && source.Height == ((frames + columns - 1) / columns) * size,
                        $"{path}: invalid cell grid.");
                    Require(skip >= 0 && skip < frames, $"{path}: skipFirst must retain at least one frame.");
                    // One common ground anchor for the whole clip; preserve all within-clip motion.
                    int sourceBottom = -1;
                    for (int frame = skip; frame < frames; frame++)
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        if (source.GetPixel(frame % columns * size + x, frame / columns * size + y).A != 0)
                            sourceBottom = Math.Max(sourceBottom, y);
                    int pivotY = clip!["pivotY"]?.GetValue<int>() ?? sourceBottom + 1;
                    int cropX = size / 2 - 24, cropY = pivotY - 60;
                    Require(cropX >= 0 && cropY >= 0 && cropX + 48 <= size && cropY + 64 <= size,
                        $"{path}: native crop is outside its cell.");
                    int bottom = -1, top = size, opaque = 0;
                    for (int frame = skip; frame < frames; frame++)
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        var pixel = source.GetPixel(frame % columns * size + x, frame / columns * size + y);
                        Alpha(pixel, $"{path} frame {frame} ({x},{y})");
                        if (pixel.A == 0) continue;
                        Require(x >= cropX && x < cropX + 48 && y >= cropY && y < cropY + 64,
                            $"{path} frame {frame}: native 48x64 crop would remove opaque pixel ({x},{y}); fix the source/pivot, never resize it.");
                        top = Math.Min(top, y - cropY); bottom = Math.Max(bottom, y - cropY); opaque++;
                        int target = first + frame - skip;
                        atlas.SetPixel(target % 8 * 48 + x - cropX, target / 8 * 64 + y - cropY, pixel);
                    }
                    Require(opaque > 0, $"{path}: empty animation.");
                    animations[name] = new JsonObject { ["firstFrame"] = first, ["frameCount"] = frames - skip,
                        ["frameDurationMs"] = Number(clip!, "frameDurationMs"),
                        ["loop"] = name is "LeaningIdle" or "SelectedIdle" or "WalkSouth" or "WalkNorth" };
                    evidence.Add(new JsonObject { ["sex"] = sex, ["clip"] = name, ["file"] = relative,
                        ["sourceSha256"] = Hash(path), ["cropX"] = cropX, ["cropY"] = cropY, ["pivotY"] = pivotY,
                        ["visibleTop"] = top, ["visibleBottom"] = bottom, ["copiedOpaquePixels"] = opaque, ["skipFirst"] = skip });
                    first += frames - skip;
                }
                _ = SpriteSheetDefinition.Parse(metadata.ToJsonString(), sex + ".json");
            }
            var tree = manifest["tree"] ?? throw new InvalidDataException("selection-sources.json: missing tree.");
            string treePath = Within(sourceRoot, Text(tree, "file"));
            CheckHash(treePath, Text(tree, "sha256"));
            using var treeImage = Open(treePath, 192, 192);
            bool hasTransparency = false;
            for (int y = 0; y < treeImage.Height; y++)
            for (int x = 0; x < treeImage.Width; x++)
            {
                var pixel = treeImage.GetPixel(x, y);
                Alpha(pixel, $"{treePath} ({x},{y})");
                hasTransparency |= pixel.A == 0;
            }
            Require(hasTransparency, $"{treePath}: tree must be separate on transparency.");
            Directory.CreateDirectory(output);
            foreach (var entry in packed)
            {
                string imagePath = Path.Combine(output, entry.Sex + ".png");
                entry.Image.Save(imagePath, ImageFormat.Png);
                using var decoded = Open(imagePath, entry.Image.Width, entry.Image.Height);
                for (int y = 0; y < decoded.Height; y++)
                for (int x = 0; x < decoded.Width; x++)
                    Require(Same(decoded.GetPixel(x, y), entry.Image.GetPixel(x, y)), $"{imagePath}: encoded pixels changed.");
                WriteJson(Path.Combine(output, entry.Sex + ".json"), entry.Metadata);
            }
            File.Copy(treePath, Path.Combine(output, "tree.png"));
            WriteJson(Path.Combine(output, "tree.json"), new { version = 1, frameWidth = 192, frameHeight = 192,
                columns = 1, rows = 1, pivotX = 96, pivotY = 178, displayScale = 2,
                animations = new { Idle = new { firstFrame = 0, frameCount = 1, frameDurationMs = 1000, loop = true } } });
            WriteJson(Path.Combine(output, "packing.json"), new { sourceManifestSha256 = Hash(Path.Combine(sourceRoot, "selection-sources.json")), frames = evidence });
            _ = new CharacterSelectionCatalog(output);
        }
        finally { foreach (var entry in packed) entry.Image.Dispose(); }
    }
}
