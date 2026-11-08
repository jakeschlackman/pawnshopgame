using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text.Json.Nodes;

namespace PawnShop.Tests;

internal static class PlayerAssetTests
{
    internal static void Run()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Asset regression checks require Windows; they have not run on this platform.");
        RunWindows();
    }

    [SupportedOSPlatform("windows")]
    private static void RunWindows()
    {
        string temp = Path.Combine(Path.GetTempPath(), "PawnShopAssets-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string source = Path.Combine(temp, "source"), packed = Path.Combine(temp, "packed");
            Directory.CreateDirectory(source);
            var sources = new JsonArray();
            string[] directions = ["north", "east", "south", "west"];
            foreach (string body in new[] { "male", "female" })
            foreach (string direction in directions)
            for (int frame = 0; frame < 4; frame++)
            {
                string relative = $"Sources/{body}/{direction}/{frame}.png";
                string path = Path.Combine(source, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var image = new Bitmap(48, 64);
                int x = direction == "east" ? 23 : 24;
                image.SetPixel(x, 20, Color.Bisque);
                image.SetPixel(x, 32, Color.FromArgb(255, 80 + frame, 90, 100));
                image.Save(path, ImageFormat.Png);
                sources.Add(new JsonObject { ["path"] = relative, ["sha256"] = PlayerAssets.Hash(path), ["bodyType"] = body,
                    ["direction"] = direction, ["frame"] = frame, ["usage"] = "active" });
            }
            var manifest = new JsonObject { ["version"] = 1, ["mode"] = "skeleton-v3", ["status"] = "candidate_needs_visual_approval",
                ["directions"] = new JsonArray(directions.Select(d => (JsonNode?)JsonValue.Create(d)).ToArray()),
                ["frameCount"] = 4, ["frameDurationMs"] = 100, ["loop"] = true,
                ["canvas"] = new JsonObject { ["width"] = 48, ["height"] = 64 },
                ["pivot"] = new JsonObject { ["x"] = 24, ["y"] = 58 },
                ["sourceCanvas"] = new JsonObject { ["width"] = 48, ["height"] = 64 },
                ["sourceCrop"] = new JsonObject { ["x"] = 0, ["y"] = 0 },
                ["sourcePivot"] = new JsonObject { ["x"] = 24, ["y"] = 58 },
                ["sources"] = sources, ["bodies"] = new JsonArray(
                    new JsonObject { ["bodyType"] = "male", ["sheet"] = "male/walk.png" },
                    new JsonObject { ["bodyType"] = "female", ["sheet"] = "female/walk.png" }) };
            string manifestPath = Path.Combine(source, "walk-manifest.json");
            PlayerAssets.WriteJson(manifestPath, manifest);
            string before = PlayerAssets.Hash(manifestPath);
            PlayerAssets.Pack(source, packed);
            PlayerAssets.Require(PlayerAssets.Hash(manifestPath) == before, "Packing changed source manifest.");
            PlayerAssets.ValidateWalk(packed);
            Reject(() => PlayerAssets.Pack(source, packed), "already exists");
            Reject(() => PlayerAssets.Pack(source, Path.Combine(source, "nested")), "outside");
            Reject(() => PlayerAssets.Within(source, "../escape.png"), "escapes");

            string metadataPath = Path.Combine(packed, "walk-sprite.json");
            var metadata = PlayerAssets.Read(metadataPath);
            metadata.AsObject().Remove("displayScale");
            PlayerAssets.WriteJson(metadataPath, metadata);
            Reject(() => PlayerAssets.ValidateWalk(packed), "scale");
            using (var metadataErrors = new StringWriter())
                PlayerAssets.Require(PlayerAssets.Run(["walk", packed], metadataErrors) == 1 && metadataErrors.ToString().Contains("scale"), "CLI lost the metadata diagnostic.");
            metadata["displayScale"] = 1;
            PlayerAssets.WriteJson(metadataPath, metadata);

            string attirePath = Path.Combine(packed, "attire-template.json");
            var attire = PlayerAssets.Read(attirePath);
            Reject(() => PlayerAssets.ValidateAttire(packed, attirePath), "missing path");
            foreach (var variant in PlayerAssets.Array(attire, "variants"))
            {
                string body = PlayerAssets.Text(variant!, "bodyType");
                using var bodySheet = PlayerAssets.Open(Path.Combine(packed, body, "walk.png"));
                using var layer = new Bitmap(192, 256);
                using var coverage = new Bitmap(192, 256);
                using var protect = new Bitmap(192, 256);
                using var composite = new Bitmap(bodySheet);
                for (int row = 0; row < 4; row++)
                for (int frame = 0; frame < 4; frame++)
                {
                    int x = frame * 48 + (row == 1 ? 23 : 24), y = row * 64;
                    layer.SetPixel(x, y + 32, Color.Teal);
                    coverage.SetPixel(x, y + 32, Color.White);
                    protect.SetPixel(x, y + 20, Color.White);
                    composite.SetPixel(x, y + 32, Color.Teal);
                }
                foreach (var (field, image) in new[] { ("layerSheet", layer), ("coverageMask", coverage), ("protectedMask", protect), ("compositeSheet", composite) })
                {
                    string path = $"{body}/{field}.png";
                    image.Save(Path.Combine(packed, path), ImageFormat.Png);
                    variant![field] = path;
                }
            }
            PlayerAssets.WriteJson(attirePath, attire);
            PlayerAssets.ValidateAttire(packed, attirePath);
            void InvalidAttire(Action<JsonNode> mutate, string diagnostic)
            {
                var changed = attire.DeepClone(); mutate(changed); PlayerAssets.WriteJson(attirePath, changed);
                Reject(() => PlayerAssets.ValidateAttire(packed, attirePath), diagnostic);
                PlayerAssets.WriteJson(attirePath, attire);
            }
            InvalidAttire(a => a["variants"]![0]!["baseSha256"] = new string('0', 64), "hash");
            InvalidAttire(a => a["frameDurationMs"] = 1, "timing");
            InvalidAttire(a => a["variants"]!.AsArray().RemoveAt(1), "both bodies");
            InvalidAttire(a => a["variants"]![0]!["layerSheet"] = "male/protectedMask.png", "protected");
            InvalidAttire(a => a["variants"]![0]!["compositeSheet"] = "male/walk.png", "composite");
            using (var image = PlayerAssets.Open(Path.Combine(packed, "male/layerSheet.png")))
            {
                image.SetPixel(2, 2, Color.FromArgb(128, 100, 100, 100));
                image.Save(Path.Combine(packed, "partial.png"), ImageFormat.Png);
            }
            InvalidAttire(a => a["variants"]![0]!["layerSheet"] = "partial.png", "partial alpha");
            using (var image = new Bitmap(192, 256)) image.Save(Path.Combine(packed, "empty.png"), ImageFormat.Png);
            InvalidAttire(a => a["variants"]![0]!["coverageMask"] = "empty.png", "empty");
            File.WriteAllBytes(Path.Combine(packed, "bad.png"), [137, 80, 78, 71]);
            Reject(() => { using var image = PlayerAssets.Open(Path.Combine(packed, "bad.png")); }, "decode");

            string review = Path.Combine(temp, "review");
            PlayerAssets.Review(packed, review);
            using (var native = PlayerAssets.Open(Path.Combine(review, "review-native.png"), 192, 512))
            using (var large = PlayerAssets.Open(Path.Combine(review, "review-4x.png"), 768, 2048))
                PlayerAssets.Require(PlayerAssets.Same(native.GetPixel(24, 20), large.GetPixel(96, 80)), "Review enlargement changed pixels.");
            PlayerAssets.Require(!File.ReadAllText(Path.Combine(review, "review.html")).Contains("__COUNT__"), "Unexpanded review token.");
            Reject(() => PlayerAssets.Review(packed, review), "already exists");

            string imported = Path.Combine(temp, "imported");
            PlayerAssets.Import(manifestPath, source, imported);
            PlayerAssets.Require(PlayerAssets.Hash(Path.Combine(imported, "walk-manifest.json")) == before, "Import changed manifest.");
            string first = Path.Combine(source, PlayerAssets.Text(sources[0]!, "path"));
            byte[] original = File.ReadAllBytes(first);
            File.WriteAllBytes(first, [0]);
            Reject(() => PlayerAssets.Import(manifestPath, source, Path.Combine(temp, "bad-import")), "hash");
            PlayerAssets.Require(!Directory.Exists(Path.Combine(temp, "bad-import")), "Failed import created output.");
            PlayerAssets.Require(PlayerAssets.Hash(manifestPath) == before, "Import blessed a changed hash.");
            File.WriteAllBytes(first, original);
            manifest["sources"]!.AsArray().Add(sources[1]!.DeepClone());
            PlayerAssets.WriteJson(manifestPath, manifest);
            // Duplicate active poses are rejected independently of source hashing.
            Reject(() => { using var image = PlayerAssets.Assemble(source, manifest, "male"); }, "duplicate");
            manifest["sources"]!.AsArray().RemoveAt(sources.Count - 1);
            using (var image = PlayerAssets.Open(first))
            {
                image.SetPixel(0, 0, Color.White);
                image.Save(Path.Combine(source, "border.png"), ImageFormat.Png);
            }
            sources[0]!["path"] = "border.png";
            Reject(() => { using var image = PlayerAssets.Assemble(source, manifest, "male"); }, "border");
            sources[0]!["path"] = "Sources/male/north/0.png";
            using (var image = new Bitmap(48, 64)) image.Save(Path.Combine(source, "empty.png"), ImageFormat.Png);
            sources[0]!["path"] = "empty.png";
            Reject(() => { using var image = PlayerAssets.Assemble(source, manifest, "male"); }, "empty");
            sources[0]!["path"] = "Sources/male/north/0.png";
            using (var image = new Bitmap(50, 64))
            {
                image.SetPixel(24, 20, Color.Bisque);
                image.SetPixel(49, 20, Color.White);
                image.Save(Path.Combine(source, "crop.png"), ImageFormat.Png);
            }
            manifest["sourceCanvas"]!["width"] = 50;
            sources[0]!["path"] = "crop.png";
            Reject(() => { using var image = PlayerAssets.Assemble(source, manifest, "male"); }, "crop discards");
            manifest["sourceCanvas"]!["width"] = 48;
            sources[0]!["path"] = "Sources/male/north/0.png";
            using (var image = PlayerAssets.Open(Path.Combine(source, "Sources/male/east/0.png")))
            {
                image.SetPixel(10, 10, Color.Red);
                image.Save(Path.Combine(source, "not-mirrored.png"), ImageFormat.Png);
            }
            sources[4]!["path"] = "not-mirrored.png";
            Reject(() => { using var image = PlayerAssets.Assemble(source, manifest, "male"); }, "mirror");
            using var errors = new StringWriter();
            PlayerAssets.Require(PlayerAssets.Run(["walk", Path.Combine(temp, "absent")], errors) == 1 && errors.ToString().Contains("ERROR:"), "CLI did not report missing content.");
        }
        finally { Directory.Delete(temp, recursive: true); }
    }

    private static void Reject(Action action, string diagnostic)
    {
        try { action(); }
        catch (InvalidDataException e) when (e.Message.Contains(diagnostic, StringComparison.OrdinalIgnoreCase)) { return; }
        throw new Exception($"Expected rejection containing '{diagnostic}'.");
    }
}
