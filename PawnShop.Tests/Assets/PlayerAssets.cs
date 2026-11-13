using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using PawnShop.Characters.Npcs.Common;
using PawnShop.Framework.Rendering;

namespace PawnShop.Tests;

// Offline commands, deliberately hosted by the existing authoring/test executable.
// Version-1 walk manifests describe preserved NESW candidates, not the future SNE runtime format.
internal static partial class PlayerAssets
{
    internal static int Run(string[] args, TextWriter output)
    {
        if (!OperatingSystem.IsWindows())
        {
            output.WriteLine("ERROR: PNG authoring requires Windows (System.Drawing). No graphics device is used.");
            return 1;
        }
        try
        {
            switch (args)
            {
                case ["common", var root]:
                    var catalog = CommonNpcCatalog.Load(root);
                    foreach (string body in catalog.BodyIds)
                        output.WriteLine($"{body}: " + string.Join("; ", new[] { "torso", "legs", "feet", "hair", "headgear" }
                            .Select(slot => slot + "=" + string.Join(",", catalog.Choices(body, slot)))));
                    break;
                case ["bases", var root]: ValidateBases(root); break;
                case ["wearable", var root]: ValidateStaticWearable(root); break;
                case ["walk", var root]: ValidateWalk(root); break;
                case ["walk", var root, var attire]: ValidateWalk(root); ValidateAttire(root, attire); break;
                case ["pack", var root, var destination]: Pack(root, destination); break;
                case ["review", var root, var destination]: Review(root, destination); break;
                case ["import", var manifest, var sources, var destination]: Import(manifest, sources, destination); break;
                case ["selection-pack", var sources, var destination]: PackSelection(sources, destination); break;
                default:
                    output.WriteLine("Usage: --assets common <assets-root> | bases <root> | wearable <root> | walk <root> [attire.json]\n" +
                        "       --assets pack <walk-root> <new-directory> | review <walk-root> <new-directory>\n" +
                        "       --assets import <walk-manifest.json> <download-directory> <new-directory>\n" +
                        "       --assets selection-pack <source-manifest-directory> <new-directory>\n" +
                        "Validation is read-only. Outputs require a new directory. Import uses local exports and existing hashes; no network or promotion.");
                    return args is [] or ["help"] ? 0 : 1;
            }
            output.WriteLine("PASS: asset checks/operation completed. Numerical success does not grant visual approval or production readiness.");
            return 0;
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException or System.Text.Json.JsonException
            or ArgumentException or InvalidOperationException or System.Runtime.InteropServices.ExternalException)
        {
            output.WriteLine("ERROR: " + e.Message);
            return 1;
        }
    }

    internal static JsonNode Read(string path) => JsonNode.Parse(File.ReadAllText(path))
        ?? throw new InvalidDataException($"{path}: empty JSON.");
    internal static string Text(JsonNode? node, string key) => node?[key]?.GetValue<string>()
        ?? throw new InvalidDataException($"{key}: missing value.");
    internal static int Number(JsonNode? node, string key) => node?[key]?.GetValue<int>()
        ?? throw new InvalidDataException($"{key}: missing integer.");
    internal static JsonArray Array(JsonNode? node, string key) => node?[key] as JsonArray
        ?? throw new InvalidDataException($"{key}: missing array.");
    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
    internal static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    internal static void CheckHash(string path, string expected) =>
        Require(expected.Length == 64 && Hash(path).Equals(expected, StringComparison.OrdinalIgnoreCase), $"{path}: missing or changed source/body hash.");

    // Relative source paths may not escape the declared root or traverse junctions/symlinks.
    // Wearable base references use an explicitly chosen common Player root instead.
    internal static string Within(string root, string relative)
    {
        root = Path.GetFullPath(root);
        string path = Path.GetFullPath(Path.Combine(root, relative));
        Require(!Path.IsPathRooted(relative) && path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase), $"{relative}: path escapes {root}.");
        for (string? current = path; current is not null; current = Path.GetDirectoryName(current))
            if (File.Exists(current) || Directory.Exists(current))
                Require((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, $"{current}: linked paths are not supported for authoring.");
        return path;
    }

    [SupportedOSPlatform("windows")]
    internal static Bitmap Open(string path, int? width = null, int? height = null)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var decoded = Image.FromStream(stream, false, true);
            Require(decoded.RawFormat.Guid == ImageFormat.Png.Guid, $"{path}: expected PNG.");
            Require((width is null || decoded.Width == width) && (height is null || decoded.Height == height),
                $"{path}: unexpected dimensions {decoded.Width}x{decoded.Height}; expected {width}x{height}.");
            // Force decoding and detach from the input stream before returning.
            return new Bitmap(decoded);
        }
        catch (Exception e) when (e is ArgumentException or OutOfMemoryException or System.Runtime.InteropServices.ExternalException)
        { throw new InvalidDataException($"{path}: cannot decode PNG.", e); }
    }

    internal static bool Same(Color a, Color b) => a.A == b.A && (a.A == 0 || a.ToArgb() == b.ToArgb());
    internal static void Alpha(Color color, string where) => Require(color.A is 0 or 255, $"{where}: partial alpha.");

    [SupportedOSPlatform("windows")]
    internal static void ValidateBases(string root)
    {
        var manifest = Read(Path.Combine(root, "base-manifest.json"));
        Require(Number(manifest, "version") == 1 && Number(manifest["canvas"]!, "width") == 48 &&
            Number(manifest["canvas"]!, "height") == 64 && Number(manifest["pivot"]!, "x") == 24 &&
            Number(manifest["pivot"]!, "y") == 58, "base-manifest.json: expected 48x64, pivot (24,58), version 1.");
        int count = Array(manifest, "directions").Count;
        var sheets = Array(manifest, "sheets");
        Require(sheets.Select(s => Text(s!, "sex")).Order().SequenceEqual(new[] { "female", "male" }), "base-manifest.json: requires both bodies exactly once.");
        foreach (var entry in sheets)
        {
            string path = Within(root, Text(entry!, "file"));
            using var sheet = Open(path, 48 * count, 64);
            for (int frame = 0; frame < count; frame++)
            {
                int left = 48, right = -1, top = 64, bottom = -1;
                for (int y = 0; y < 64; y++)
                for (int x = 0; x < 48; x++)
                {
                    var color = sheet.GetPixel(frame * 48 + x, y);
                    Alpha(color, $"{path}/{frame} ({x},{y})");
                    if (color.A == 0) continue;
                    Require(x is > 0 and < 47 && y is > 0 and < 63, $"{path}/{frame}: touches border.");
                    left = Math.Min(left, x); right = Math.Max(right, x);
                    top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                }
                Require(top == Number(manifest["baseBounds"]!, "top") && bottom == Number(manifest["baseBounds"]!, "bottom") &&
                    bottom - top + 1 == Number(manifest["visibleBodyHeight"]!, "target") && bottom + 1 == 58 &&
                    Math.Abs((left + right) / 2.0 - 23.5) <= 0.5, $"{path}/{frame}: body bounds, centering or ground pivot changed.");
            }
        }
        foreach (var file in Array(manifest, "files")) { using var image = Open(Within(root, file!.GetValue<string>())); }
    }

    [SupportedOSPlatform("windows")]
    internal static void ValidateStaticWearable(string root)
    {
        var manifest = Read(Path.Combine(root, "wearable.json"));
        // Legacy shirt records remain readable; new assets should use explicit layer/mask manifests.
        string baseManifestPath = Path.GetFullPath(Path.Combine(root, Text(manifest, "baseManifest")));
        string bases = Path.GetDirectoryName(baseManifestPath)!;
        ValidateBases(bases);
        Require(Array(Read(baseManifestPath), "directions")[0]!.GetValue<string>() == "south", "Static wearable expects south in the first base cell.");
        foreach (var entry in Array(manifest, "downloads"))
        {
            string file = Text(entry!, "path");
            var size = file switch { "review-native.png" => (192, 128), "review-4x.png" => (768, 512), _ => (48, 64) };
            using var image = Open(Within(root, file), size.Item1, size.Item2);
        }
        foreach (string body in new[] { "male", "female" })
        {
            string declaredBase = Path.GetFullPath(Path.Combine(root, Text(manifest["bases"]?[body], "atlas")));
            Require(declaredBase.Equals(Within(bases, $"{body}/base.png"), StringComparison.OrdinalIgnoreCase), $"{body}: mismatched canonical base reference.");
            using var layer = Open(Within(root, $"{body}/shirt.png"), 48, 64);
            using var composite = Open(Within(root, $"{body}/preview.png"), 48, 64);
            using var original = Open(Within(bases, $"{body}/base.png"));
            var bounds = manifest["garmentBounds"]?[body] ?? throw new InvalidDataException($"{body}: missing garmentBounds.");
            int opaque = 0;
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 48; x++)
            {
                Color b = original.GetPixel(x, y), l = layer.GetPixel(x, y), c = composite.GetPixel(x, y);
                string where = $"{body} ({x},{y})";
                Alpha(b, where); Alpha(l, where); Alpha(c, where);
                if (l.A != 0)
                {
                    opaque++;
                    Require(x is > 0 and < 47 && y >= Number(bounds, "top") && y <= Number(bounds, "bottom"), $"{where}: outside garment bounds.");
                }
                Require(Same(c, l.A == 0 ? b : l), $"{where}: composite changed base or garment pixels.");
            }
            Require(opaque > 0, $"{body}: empty layer.");
            if (manifest["shoulderCoverage"]?[body] is JsonArray points)
                foreach (var point in points)
                {
                    Color c = layer.GetPixel(point![0]!.GetValue<int>(), point[1]!.GetValue<int>());
                    // Preserve this approved shirt's historical check, never use it to extract arbitrary gear.
                    Require(c.A == 255 && !(c.R > 64 && c.R > c.G + 15 && c.R > c.B + 15), $"{body}: exposed or skin-colored shoulder.");
                }
            if (manifest["coverageRegions"]?[body] is JsonArray regions)
                foreach (var region in regions)
                    for (int y = region![1]!.GetValue<int>(); y <= region[3]!.GetValue<int>(); y++)
                    for (int x = region[0]!.GetValue<int>(); x <= region[2]!.GetValue<int>(); x++)
                        Require(layer.GetPixel(x, y).A == 255, $"{body} ({x},{y}): missing coverage.");
        }
    }

    internal static JsonNode WalkManifest(string root)
    {
        var m = Read(Path.Combine(root, "walk-manifest.json"));
        Require(Number(m, "version") == 1 && Text(m, "mode") == "skeleton-v3" &&
            Array(m, "directions").Select(d => d!.GetValue<string>()).SequenceEqual(new[] { "north", "east", "south", "west" }) &&
            Array(m, "bodies").Select(b => Text(b!, "bodyType")).SequenceEqual(new[] { "male", "female" }) &&
            Number(m["canvas"]!, "width") == 48 && Number(m["canvas"]!, "height") == 64 &&
            Number(m["pivot"]!, "x") == 24 && Number(m["pivot"]!, "y") == 58 &&
            Number(m, "frameCount") is >= 4 and <= 16 && Number(m, "frameDurationMs") is > 0 and <= 60000 &&
            m["loop"]?.GetValue<bool>() == true, "walk-manifest.json: invalid legacy Skeleton v3 NESW/both-body canvas, timing or pivot contract.");
        int x = Number(m["sourceCrop"]!, "x"), y = Number(m["sourceCrop"]!, "y");
        Require(x >= 0 && y >= 0 && x + 48 <= Number(m["sourceCanvas"]!, "width") &&
            y + 64 <= Number(m["sourceCanvas"]!, "height") && Number(m["sourcePivot"]!, "x") - x == 24 &&
            Number(m["sourcePivot"]!, "y") - y == 58, "walk-manifest.json: invalid fixed crop/anchor transform.");
        return m;
    }

    internal static void ValidateWalkMetadata(string root, JsonNode m)
    {
        var sheet = SpriteSheetDefinition.Load(Path.Combine(root, "walk-sprite.json"));
        int count = Number(m, "frameCount");
        Require(sheet.FrameWidth == 48 && sheet.FrameHeight == 64 && sheet.Columns == count && sheet.Rows == 4 &&
            sheet.Pivot.X == 24 && sheet.Pivot.Y == 58 && sheet.DisplayScale == 1 && sheet.Animations.Count == 4, "walk-sprite.json: wrong grid, pivot, scale or clip count.");
        for (int row = 0; row < 4; row++)
        {
            string direction = Array(m, "directions")[row]!.GetValue<string>();
            string name = "Walk" + char.ToUpperInvariant(direction[0]) + direction[1..];
            Require(sheet.Animations.TryGetValue(name, out var clip) && clip.FirstFrame == row * count && clip.FrameCount == count &&
                clip.FrameDurationMs == Number(m, "frameDurationMs") && clip.Loop, $"walk-sprite.json/{name}: frame mapping or timing differs.");
        }
    }

    [SupportedOSPlatform("windows")]
    internal static void ValidateWalk(string root)
    {
        var m = WalkManifest(root);
        ValidateWalkMetadata(root, m);
        ValidateSources(root, m);
        foreach (var body in Array(m, "bodies"))
        {
            string path = Within(root, Text(body!, "sheet"));
            using var actual = Open(path, Number(m, "frameCount") * 48, 256);
            using var expected = Assemble(root, m, Text(body!, "bodyType"));
            for (int y = 0; y < actual.Height; y++)
            for (int x = 0; x < actual.Width; x++)
                Require(Same(actual.GetPixel(x, y), expected.GetPixel(x, y)), $"{path} ({x},{y}): sheet differs from source.");
        }
    }

    internal static void ValidateSources(string root, JsonNode m)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in Array(m, "sources"))
        {
            string path = Within(root, Text(entry!, "path"));
            Require(paths.Add(path), $"{path}: duplicate source path.");
            CheckHash(path, Text(entry!, "sha256"));
        }
    }

    [SupportedOSPlatform("windows")]
    internal static Bitmap Assemble(string root, JsonNode m, string body)
    {
        int count = Number(m, "frameCount"), cropX = Number(m["sourceCrop"]!, "x"), cropY = Number(m["sourceCrop"]!, "y");
        var sheet = new Bitmap(count * 48, 256);
        try
        {
            for (int row = 0; row < 4; row++)
            for (int frame = 0; frame < count; frame++)
            {
                string direction = Array(m, "directions")[row]!.GetValue<string>();
                var entries = Array(m, "sources").Where(s => Text(s!, "bodyType") == body && Text(s!, "direction") == direction &&
                    Number(s!, "frame") == frame && Text(s!, "usage") == "active").ToArray();
                Require(entries.Length == 1, $"{body}/{direction}/{frame}: missing/duplicate active source.");
                string path = Within(root, Text(entries[0]!, "path"));
                using var source = Open(path, Number(m["sourceCanvas"]!, "width"), Number(m["sourceCanvas"]!, "height"));
                int opaque = 0;
                for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    Color color = source.GetPixel(x, y);
                    Alpha(color, path);
                    int localX = x - cropX, localY = y - cropY;
                    bool inside = localX is >= 0 and < 48 && localY is >= 0 and < 64;
                    Require(inside || color.A == 0, $"{path}: crop discards visible pixels.");
                    if (!inside) continue;
                    if (color.A != 0)
                    {
                        opaque++;
                        Require(localX is > 0 and < 47 && localY is > 0 and < 63, $"{path}: touches border.");
                    }
                    sheet.SetPixel(frame * 48 + localX, row * 64 + localY, color);
                }
                Require(opaque > 0, $"{path}: empty body frame.");
            }
            CheckMirror(sheet, count, body);
            return sheet;
        }
        catch { sheet.Dispose(); throw; }
    }

    [SupportedOSPlatform("windows")]
    internal static void CheckMirror(Bitmap sheet, int count, string where)
    {
        for (int frame = 0; frame < count; frame++)
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 48; x++)
            Require(Same(sheet.GetPixel(frame * 48 + x, 64 + y), sheet.GetPixel(frame * 48 + 47 - x, 192 + y)), $"{where}: east/west mirror differs.");
    }

    [SupportedOSPlatform("windows")]
    internal static void ValidateAttire(string root, string manifestPath)
    {
        var m = WalkManifest(root);
        var wear = Read(manifestPath);
        int count = Number(m, "frameCount");
        Require(Number(wear, "version") == 1 && JsonNode.DeepEquals(wear["directions"], m["directions"]) &&
            Number(wear, "frameCount") == count && Number(wear, "frameDurationMs") == Number(m, "frameDurationMs") &&
            Number(wear["pivot"]!, "x") == 24 && Number(wear["pivot"]!, "y") == 58 &&
            Array(wear, "variants").Select(v => Text(v!, "bodyType")).SequenceEqual(new[] { "male", "female" }),
            $"{manifestPath}: attire requires both bodies and identical pose order, timing and pivot.");
        string directory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        foreach (var variant in Array(wear, "variants"))
        {
            string body = Text(variant!, "bodyType");
            var images = new Dictionary<string, Bitmap>();
            try
            {
                foreach (string field in new[] { "baseSheet", "layerSheet", "coverageMask", "protectedMask", "compositeSheet" })
                {
                    string path = variant![field]?.GetValue<string>() ?? throw new InvalidDataException($"{body}/{field}: missing path; attire is incomplete.");
                    // Historical attire templates use paths relative to the garment directory, including ../Bases.
                    images.Add(field, Open(Path.GetFullPath(Path.Combine(directory, path)), count * 48, 256));
                }
                string basePath = Within(root, Text(Array(m, "bodies").Single(b => Text(b!, "bodyType") == body)!, "sheet"));
                CheckHash(basePath, Text(variant!, "baseSha256"));
                CheckHash(Path.GetFullPath(Path.Combine(directory, Text(variant!, "baseSheet"))), Hash(basePath));
                for (int row = 0; row < 4; row++)
                for (int frame = 0; frame < count; frame++)
                {
                    int covered = 0, protectedPixels = 0, cloth = 0;
                    for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 48; x++)
                    {
                        int sx = frame * 48 + x, sy = row * 64 + y;
                        string where = $"{body}/{row}/{frame} ({x},{y})";
                        foreach (var image in images.Values) Alpha(image.GetPixel(sx, sy), where);
                        Color b = images["baseSheet"].GetPixel(sx, sy), l = images["layerSheet"].GetPixel(sx, sy);
                        if (images["coverageMask"].GetPixel(sx, sy).A != 0)
                        { covered++; Require(l.A == 255, $"{where}: missing coverage."); }
                        if (images["protectedMask"].GetPixel(sx, sy).A != 0)
                        { protectedPixels++; Require(b.A != 0 && l.A == 0, $"{where}: protected body pixel covered or missing."); }
                        if (l.A != 0)
                        { cloth++; Require(x is > 0 and < 47 && y is > 0 and < 63, $"{where}: garment touches border."); }
                        Require(Same(images["compositeSheet"].GetPixel(sx, sy), l.A == 0 ? b : l), $"{where}: composite changed body or garment pixels.");
                    }
                    Require(covered > 0 && protectedPixels > 0 && cloth > 0, $"{body}/{row}/{frame}: empty layer or mask.");
                }
                if (string.IsNullOrWhiteSpace(wear["asymmetricEastException"]?.GetValue<string>()))
                    foreach (string field in new[] { "layerSheet", "coverageMask", "protectedMask" }) CheckMirror(images[field], count, field);
            }
            finally { foreach (var image in images.Values) image.Dispose(); }
        }
    }
}
