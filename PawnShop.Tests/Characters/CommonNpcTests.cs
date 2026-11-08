using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using PawnShop.Characters.Npcs.Common;
using PawnShop.Characters.Npcs.Common.Generation;

namespace PawnShop.Tests;

internal static class CommonNpcTests
{
    private static string Assets => Path.Combine(AppContext.BaseDirectory, "Content", "CommonNpcs");
    private static readonly Guid ShopperId = Guid.Parse("a1aa746e-1830-4573-97aa-91ce83ee17d9");

    internal static void Generation()
    {
        var catalog = CommonNpcCatalog.Load(Assets);
        var generator = new CommonNpcGenerator(catalog);
        var reference = generator.Create(ShopperId, 123456);
        if (catalog.Version == 1)
            Check(reference.Appearance == new CommonNpcAppearance("male", "teal_work_jacket", "dark_blue_jeans",
                "brown_leather_work_boots", null, null), "Version 1 seeded reference changed; content changes require a version bump.");
        Check(reference.GeneratorVersion == 1 && reference.CatalogVersion == catalog.Version, "Missing versions.");
        Check(reference == generator.Create(ShopperId, 123456), "Equal seed/identity must reproduce the result.");
        var other = generator.Create(Guid.NewGuid(), 123456);
        Check(other.Id != reference.Id && other.Appearance == reference.Appearance, "Appearance must not own identity.");
        Check(JsonSerializer.Deserialize<CommonNpc>(JsonSerializer.Serialize(reference)) == reference, "Realized data must round-trip without a catalog or graphics device.");
        Expect<ArgumentException>(() => generator.Create(Guid.Empty, 1));
        var outcomes = new HashSet<CommonNpcAppearance>();
        for (ulong seed = 0; seed < 256; seed++)
        {
            var npc = generator.Create(ShopperId, seed);
            outcomes.Add(npc.Appearance);
            foreach (var facing in Enum.GetValues<CommonNpcFacing>())
                for (int frame = 0; frame < 8; frame++) catalog.Resolve(npc.Appearance, facing, CommonNpcPose.Walking, frame);
        }
        Check(outcomes.Count > 1 && catalog.BodyIds.All(id => outcomes.Any(a => a.BodyId == id)), "Body/appearance variety must be reachable.");
    }

    internal static void FramesAndMigration()
    {
        var catalog = CommonNpcCatalog.Load(Assets);
        foreach (string body in catalog.BodyIds)
        foreach (bool hair in new[] { false, true })
        foreach (bool hat in new[] { false, true })
        {
            var appearance = new CommonNpcAppearance(body, "teal_work_jacket", "dark_blue_jeans",
                "brown_leather_work_boots", hair ? "short_chestnut" : null, hat ? "charcoal_welding_mask" : null);
            foreach (var facing in Enum.GetValues<CommonNpcFacing>())
            foreach (var pose in Enum.GetValues<CommonNpcPose>())
            {
                if (pose == CommonNpcPose.Settling && facing is CommonNpcFacing.South or CommonNpcFacing.North) continue;
                for (int frame = 0; frame < (pose == CommonNpcPose.Walking ? 8 : 1); frame++)
                {
                    var result = catalog.Resolve(appearance, facing, pose, frame);
                    string[] order = ["body", "brown_leather_work_boots", "dark_blue_jeans", "teal_work_jacket"];
                    Check(result.Layers.Take(4).Select(l => l.PartId).SequenceEqual(order), "Boot/hem/body order changed.");
                    Check(result.Layers.Count == 4 + (hair ? 1 : 0) + (hat ? 1 : 0), "Optional layer count.");
                    foreach (var layer in result.Layers)
                    {
                        int expected = pose == CommonNpcPose.Settling ? 9 : pose == CommonNpcPose.Standing ? 0
                            : facing == CommonNpcFacing.South && !layer.PartId.StartsWith("short_chestnut", StringComparison.Ordinal) ? frame : frame + 1;
                        Check(layer.X == expected * 48 && layer.Y == 0 && layer.Width == 48 && layer.Height == 64, "Pose alignment changed.");
                    }
                    Check(result.MirrorHorizontally == (facing == CommonNpcFacing.West) && result.PivotX == 24 && result.PivotY == 58,
                        "Whole-character mirror/pivot changed.");
                    if (facing == CommonNpcFacing.West)
                        Check(result.Layers.SequenceEqual(catalog.Resolve(appearance, CommonNpcFacing.East, pose, frame).Layers), "West must reuse the complete East pose.");
                    if (hair) Check(result.Layers[4].PartId == (hat ? "short_chestnut_masked" : "short_chestnut"), "Hat-compatible hair not selected.");
                    int hidden = body == "male" && facing == CommonNpcFacing.South && hat ? 5 : 0;
                    Check(result.HiddenBodyPixels.Count == hidden, "Body occlusion scope changed.");
                    if (hidden > 0)
                    {
                        int offset = pose == CommonNpcPose.Walking && frame is 2 or 3 or 6 or 7 ? -1 : 0;
                        Check(result.HiddenBodyPixels[0] == new CommonNpcPixel(17, 13 + offset), "Ear mask lost native gait offset.");
                    }
                }
            }
            Expect<ArgumentException>(() => catalog.Resolve(appearance, CommonNpcFacing.North, CommonNpcPose.Settling));
            Expect<ArgumentOutOfRangeException>(() => catalog.Resolve(appearance, CommonNpcFacing.South, CommonNpcPose.Walking, 8));
            Expect<ArgumentException>(() => catalog.Resolve(appearance with { TorsoId = "unknown" }, CommonNpcFacing.South, CommonNpcPose.Standing));
        }
        string root = RepositoryRoot();
        var migration = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "PawnShop.Characters/Npcs/Common/Assets/migration.json")))!;
        Check(migration["files"]!.AsArray().Count > 0, "Missing migration provenance.");
        foreach (var entry in migration["files"]!.AsArray())
        {
            string source = entry!["source"]!.GetValue<string>(), destination = entry["destination"]!.GetValue<string>();
            string hash = entry["sha256"]!.GetValue<string>();
            Check(Hash(Path.Combine(root, source)) == hash && Hash(Path.Combine(root, destination)) == hash, "Migration changed source pixels.");
            string relative = destination.Split("/Assets/", StringSplitOptions.None)[1];
            Check(Hash(Path.Combine(Assets, relative)) == hash, "Packaged asset differs from source.");
        }
        Check(!File.Exists(Path.Combine(Assets, "migration.json")), "Authoring provenance should not ship as runtime content.");
    }

    internal static void ExtensibleContent()
    {
        WithCatalog((directory, original) =>
        {
            var expanded = original.DeepClone();
            var items = expanded["items"]!.AsArray();
            JsonNode Add(string originalId, string newId)
            {
                var item = items.Single(i => i!["id"]!.GetValue<string>() == originalId)!.DeepClone();
                item["id"] = newId;
                items.Add(item);
                return item;
            }
            // Test fixtures reuse existing pixels; real items require separately approved fits.
            Add("teal_work_jacket", "test_coat");
            Add("charcoal_welding_mask", "test_hat");
            var newHair = Add("short_chestnut", "test_hair");
            foreach (var fit in newHair["fits"]!.AsArray()) fit!["headgearParts"]!["test_hat"] = "short_chestnut_masked";
            expanded["contentVersion"] = 2;
            Save(directory, expanded);
            var catalog = CommonNpcCatalog.Load(directory);
            Check(catalog.Choices("male", "torso").Contains("test_coat"), "New outfit requires code changes.");
            Check(catalog.Choices("female", "headgear").Contains("test_hat"), "New hat requires code changes.");
            Check(catalog.Choices("male", "hair", "test_hat").SequenceEqual(new[] { "test_hair" }), "Incompatible hair must be excluded.");
            var generator = new CommonNpcGenerator(catalog);
            var appearances = Enumerable.Range(0, 512).Select(i => generator.Create(ShopperId, (ulong)i)).ToArray();
            Check(appearances.All(n => n.CatalogVersion == 2), "Content version not retained.");
            Check(appearances.Any(n => n.Appearance.TorsoId == "test_coat") && appearances.Any(n => n.Appearance.HairId == "test_hair")
                && appearances.Any(n => n.Appearance.HeadgearId == "test_hat"), "Added data never selected.");
            foreach (var npc in appearances) catalog.Resolve(npc.Appearance, CommonNpcFacing.South, CommonNpcPose.Walking, 3);
            var incompatible = appearances[0].Appearance with { HairId = "short_chestnut", HeadgearId = "test_hat" };
            Expect<ArgumentException>(() => catalog.Resolve(incompatible, CommonNpcFacing.South, CommonNpcPose.Standing));
            // Reordering authoring arrays must not change seeded output.
            expanded["items"] = new JsonArray(items.Reverse().Select(i => i!.DeepClone()).ToArray());
            var bodies = expanded["bodies"]!.AsArray();
            expanded["bodies"] = new JsonArray(bodies.Reverse().Select(i => i!.DeepClone()).ToArray());
            Save(directory, expanded);
            var reordered = new CommonNpcGenerator(CommonNpcCatalog.Load(directory));
            for (int i = 0; i < appearances.Length; i++) Check(appearances[i] == reordered.Create(ShopperId, (ulong)i), "JSON order changed selection.");
            // A fit for just one body never leaks onto the other body.
            newHair = expanded["items"]!.AsArray().Single(i => i!["id"]!.GetValue<string>() == "test_hair")!;
            newHair["fits"] = new JsonArray(newHair["fits"]!.AsArray().Single(f => f!["bodyId"]!.GetValue<string>() == "male")!.DeepClone());
            Save(directory, expanded);
            Check(!CommonNpcCatalog.Load(directory).Choices("female", "hair").Contains("test_hair"), "Body fit filtering failed.");
        });
    }

    internal static void InvalidContent()
    {
        WithCatalog((directory, original) =>
        {
            CommonNpcCatalog.Load(directory);
            Action<JsonNode>[] mutations = [
                j => j["version"] = 2,
                j => j["textures"]![0]!["path"] = "../escape.png",
                j => j["textures"]![0]!["sha256"] = new string('0', 64),
                j => j["textures"]![0]!["columns"] = 1,
                j => j["bodies"]![0]!["parts"]![0]!["views"]!.AsArray().RemoveAt(0),
                j => j["bodies"]![0]!["parts"]![0]!["views"]![0]!["walking"]!["firstFrame"] = int.MaxValue,
                j => j["bodies"]![0]!["parts"]![0]!["views"]![0]!["walking"]!["texture"] = "missing.png",
                j => j["items"]!.AsArray().Add(j["items"]![0]!.DeepClone()),
                j => j["items"]![0]!["fits"]![0]!["partId"] = "unknown",
                j => j["items"]![0]!["fits"]![0]!["bodyId"] = "unknown",
                j => j["items"]!.AsArray().RemoveAt(0),
                j => j["items"]![3]!["fits"]![0]!["headgearParts"]!["missing_hat"] = "short_chestnut",
                j => j["items"]![4]!["fits"]![0]!["bodyOcclusion"]![0]!["pixels"]![0]!["x"] = 48,
                j => j["items"]![0]!["fits"] = null,
                j => j["bodies"]![0]!["parts"] = null,
                j => j["items"]![0]!["unknownField"] = true
            ];
            foreach (var mutation in mutations)
            {
                var changed = original.DeepClone(); mutation(changed); Save(directory, changed);
                Expect<InvalidDataException>(() => CommonNpcCatalog.Load(directory));
            }
            Save(directory, original);
            string file = Path.Combine(directory, "catalog.json"), text = File.ReadAllText(file);
            File.WriteAllText(file, text.Replace("\"version\": 1", "\"version\": 1, \"version\": 1", StringComparison.Ordinal));
            Expect<InvalidDataException>(() => CommonNpcCatalog.Load(directory));
            Save(directory, original);
            File.Delete(Path.Combine(directory, original["textures"]![0]!["path"]!.GetValue<string>()));
            Expect<InvalidDataException>(() => CommonNpcCatalog.Load(directory));
        });
        using var output = new StringWriter();
        Check(PlayerAssets.Run(["common", Assets], output) == 0 && output.ToString().Contains("headgear=charcoal_welding_mask", StringComparison.Ordinal),
            "Existing authoring runner must expose the content-only validation command.");
    }

    private static void WithCatalog(Action<string, JsonNode> test)
    {
        string temp = Path.Combine(Path.GetTempPath(), "PawnShopCommonNpc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            foreach (string source in Directory.EnumerateFiles(Assets, "*", SearchOption.AllDirectories))
            {
                string destination = Path.Combine(temp, Path.GetRelativePath(Assets, source));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(source, destination);
            }
            test(temp, JsonNode.Parse(File.ReadAllText(Path.Combine(temp, "catalog.json")))!);
        }
        finally { Directory.Delete(temp, true); }
    }
    private static void Save(string directory, JsonNode value) => File.WriteAllText(Path.Combine(directory, "catalog.json"), value.ToJsonString(new() { WriteIndented = true }));
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PawnShopSimulator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate repository for provenance checks.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Expect<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
