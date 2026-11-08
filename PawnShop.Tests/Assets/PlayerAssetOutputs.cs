using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PawnShop.Tests;

internal static partial class PlayerAssets
{
    internal static string NewOutput(string sourceRoot, string destination)
    {
        string source = Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar);
        string output = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar);
        Require(!output.Equals(source, StringComparison.OrdinalIgnoreCase) &&
            !output.StartsWith(source + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "Output must be outside the source directory.");
        Require(!Directory.Exists(output) && !File.Exists(output), $"{output}: output already exists; nothing was overwritten.");
        // Check existing ancestors before creating the new destination.
        _ = Within(Path.GetDirectoryName(output)!, Path.GetFileName(output));
        return output;
    }

    internal static void WriteJson(string path, object value) => File.WriteAllText(path,
        JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

    internal static void CopySources(string source, string output, JsonNode manifest)
    {
        foreach (var entry in Array(manifest, "sources"))
        {
            string relative = Text(entry!, "path");
            string target = Within(output, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(Within(source, relative), target, overwrite: false);
            CheckHash(target, Text(entry!, "sha256"));
        }
    }

    // Download/export with PixelLab's existing tools first. This local importer never changes
    // an expected hash, retries a paid job, fetches arbitrary URLs or replaces an existing file.
    [SupportedOSPlatform("windows")]
    internal static void Import(string manifestPath, string sources, string destination)
    {
        string manifestRoot = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        Require(Path.GetFileName(manifestPath) == "walk-manifest.json", "Expected walk-manifest.json.");
        var manifest = WalkManifest(manifestRoot);
        string output = NewOutput(sources, destination);
        _ = NewOutput(manifestRoot, destination);
        ValidateSources(sources, manifest);
        foreach (var body in Array(manifest, "bodies")) { using var sheet = Assemble(sources, manifest, Text(body!, "bodyType")); }
        Directory.CreateDirectory(output);
        CopySources(sources, output, manifest);
        File.Copy(manifestPath, Path.Combine(output, "walk-manifest.json"));
    }

    [SupportedOSPlatform("windows")]
    internal static void Pack(string root, string destination)
    {
        var manifest = WalkManifest(root);
        string output = NewOutput(root, destination);
        ValidateSources(root, manifest);
        var sheets = new Dictionary<string, Bitmap>();
        try
        {
            foreach (var body in Array(manifest, "bodies"))
                sheets.Add(Text(body!, "bodyType"), Assemble(root, manifest, Text(body!, "bodyType")));
            // Validate destination names before writing anything; source images stay immutable.
            var paths = Array(manifest, "sources").Select(s => Within(output, Text(s!, "path"))).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (string reserved in new[] { "walk-manifest.json", "walk-sprite.json", "attire-template.json" })
                Require(paths.Add(Within(output, reserved)), $"{reserved}: source/output collision.");
            foreach (var body in Array(manifest, "bodies"))
                Require(paths.Add(Within(output, Text(body!, "sheet"))), "Body sheet collides with another output.");
            Directory.CreateDirectory(output);
            CopySources(root, output, manifest);
            File.Copy(Path.Combine(root, "walk-manifest.json"), Path.Combine(output, "walk-manifest.json"));
            foreach (var body in Array(manifest, "bodies"))
            {
                string path = Within(output, Text(body!, "sheet"));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                sheets[Text(body!, "bodyType")].Save(path, ImageFormat.Png);
            }
            int count = Number(manifest, "frameCount"), duration = Number(manifest, "frameDurationMs");
            var clips = new Dictionary<string, object>();
            for (int row = 0; row < 4; row++)
            {
                string direction = Array(manifest, "directions")[row]!.GetValue<string>();
                clips.Add("Walk" + char.ToUpperInvariant(direction[0]) + direction[1..],
                    new { firstFrame = row * count, frameCount = count, frameDurationMs = duration, loop = true });
            }
            WriteJson(Path.Combine(output, "walk-sprite.json"), new { version = 1, frameWidth = 48, frameHeight = 64,
                columns = count, rows = 4, pivotX = 24, pivotY = 58, displayScale = 1, animations = clips });
            WriteJson(Path.Combine(output, "attire-template.json"), new { version = 1, status = "incomplete_template_not_walk_compatible",
                directions = new[] { "north", "east", "south", "west" }, frameCount = count, frameDurationMs = duration,
                pivot = new { x = 24, y = 58 }, asymmetricEastException = (string?)null,
                variants = Array(manifest, "bodies").Select(b => new { bodyType = Text(b!, "bodyType"), baseSheet = Text(b!, "sheet"),
                    baseSha256 = Hash(Within(output, Text(b!, "sheet"))), layerSheet = (string?)null, coverageMask = (string?)null,
                    protectedMask = (string?)null, compositeSheet = (string?)null,
                    visualApproval = new { north = "pending", east = "pending", south = "pending", west = "pending" } }).ToArray() });
            ValidateWalk(output);
        }
        finally { foreach (var sheet in sheets.Values) sheet.Dispose(); }
    }

    [SupportedOSPlatform("windows")]
    internal static void Review(string root, string destination)
    {
        string output = NewOutput(root, destination);
        ValidateWalk(root);
        var manifest = WalkManifest(root);
        int count = Number(manifest, "frameCount");
        using var review = new Bitmap(count * 48, 512);
        string html = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "player-walk-review.html"));
        for (int index = 0; index < 2; index++)
        {
            var body = Array(manifest, "bodies")[index]!;
            string path = Within(root, Text(body, "sheet"));
            using var sheet = Open(path, count * 48, 256);
            for (int y = 0; y < sheet.Height; y++)
            for (int x = 0; x < sheet.Width; x++) review.SetPixel(x, index * 256 + y, sheet.GetPixel(x, y));
            html = html.Replace("__" + Text(body, "bodyType").ToUpperInvariant() + "__", Convert.ToBase64String(File.ReadAllBytes(path)));
        }
        html = html.Replace("__WIDTH__", "48").Replace("__HEIGHT__", "64").Replace("__COUNT__", count.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("__DURATION__", Number(manifest, "frameDurationMs").ToString(System.Globalization.CultureInfo.InvariantCulture));
        using var enlarged = new Bitmap(review.Width * 4, review.Height * 4);
        for (int y = 0; y < enlarged.Height; y++)
        for (int x = 0; x < enlarged.Width; x++) enlarged.SetPixel(x, y, review.GetPixel(x / 4, y / 4));
        Directory.CreateDirectory(output);
        review.Save(Path.Combine(output, "review-native.png"), ImageFormat.Png);
        enlarged.Save(Path.Combine(output, "review-4x.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(output, "review.html"), html);
    }
}
