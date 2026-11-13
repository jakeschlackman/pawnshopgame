using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text.Json.Nodes;
using PawnShop.Characters.Player.Create;
using static PawnShop.Tests.RegressionTests;

namespace PawnShop.Tests;

internal static class SelectionAssetTests
{
    internal static void Run()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Selection asset checks require Windows.");
        RunWindows();
    }

    [SupportedOSPlatform("windows")]
    private static void RunWindows()
    {
        string temp = Path.Combine(Path.GetTempPath(), "SelectionAssets-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string source = Path.Combine(temp, "sources"), output = Path.Combine(temp, "output");
            Directory.CreateDirectory(source);
            var bodies = new JsonArray();
            foreach (string sex in new[] { "male", "female" })
            {
                using var image = new Bitmap(80, 80);
                image.SetPixel(40, 13, Color.Coral);
                image.SetPixel(40, 67, Color.Brown);
                image.Save(Path.Combine(source, sex + ".png"), ImageFormat.Png);
                var clips = new JsonArray();
                foreach (string name in CharacterSelectionCatalog.ClipNames)
                    clips.Add(new JsonObject { ["name"] = name, ["file"] = sex + ".png", ["cell"] = 80,
                        ["columns"] = 1, ["count"] = 1, ["frameDurationMs"] = 100,
                        ["sha256"] = PlayerAssets.Hash(Path.Combine(source, sex + ".png")) });
                bodies.Add(new JsonObject { ["sex"] = sex, ["clips"] = clips });
            }
            using (var tree = new Bitmap(192, 192))
            { tree.SetPixel(96, 177, Color.Olive); tree.Save(Path.Combine(source, "tree.png"), ImageFormat.Png); }
            var manifest = new JsonObject { ["version"] = 1, ["bodies"] = bodies,
                ["tree"] = new JsonObject { ["file"] = "tree.png", ["sha256"] = PlayerAssets.Hash(Path.Combine(source, "tree.png")) } };
            string manifestPath = Path.Combine(source, "selection-sources.json");
            PlayerAssets.WriteJson(manifestPath, manifest);
            string before = PlayerAssets.Hash(Path.Combine(source, "male.png"));
            PlayerAssets.PackSelection(source, output);
            Check(before == PlayerAssets.Hash(Path.Combine(source, "male.png")));
            using (var image = PlayerAssets.Open(Path.Combine(output, "male.png"), 384, 128))
            {
                Check(image.GetPixel(24, 5).ToArgb() == Color.Coral.ToArgb());
                Check(image.GetPixel(24, 59).ToArgb() == Color.Brown.ToArgb());
                Check(image.GetPixel(0, 0).A == 0);
            }
            Reject(() => PlayerAssets.PackSelection(source, output)); // No overwrite.
            bodies[0]!["clips"]![0]!["sha256"] = new string('0', 64);
            PlayerAssets.WriteJson(manifestPath, manifest);
            string badHash = Path.Combine(temp, "bad-hash");
            Reject(() => PlayerAssets.PackSelection(source, badHash));
            Check(!Directory.Exists(badHash));
            bodies[0]!["clips"]![0]!["sha256"] = before;
            bodies[0]!["clips"]![0]!["pivotY"] = 60; // Would trim the foot at source row 67.
            PlayerAssets.WriteJson(manifestPath, manifest);
            string cropped = Path.Combine(temp, "cropped");
            Reject(() => PlayerAssets.PackSelection(source, cropped));
            Check(!Directory.Exists(cropped));
        }
        finally { Directory.Delete(temp, recursive: true); }
    }

    private static void Reject(Action action)
    {
        bool rejected = false;
        try { action(); }
        catch (InvalidDataException) { rejected = true; }
        Check(rejected, "Invalid selection content was accepted.");
    }
}
