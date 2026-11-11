using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using PawnShop.Audio;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void AudioMixSnapshot()
    {
        var settings = new AppSettings { Master = 50, Music = 20, Ambient = 30, Effects = 40,
            Combat = 60, Monster = 70, MuteUnfocused = true };
        var mix = settings.AudioMix;
        foreach (var (channel, expected) in new[] { (AudioChannel.Music, .1f), (AudioChannel.Ambient, .15f),
            (AudioChannel.Effects, .2f), (AudioChannel.Combat, .3f), (AudioChannel.Monster, .35f) })
        {
            Check(Math.Abs(mix.Gain(channel, true) - expected) < .0001f);
            Check(settings.Gain(channel, true) == mix.Gain(channel, true));
            Check(mix.Gain(channel, false) == 0);
            Check((mix with { MuteUnfocused = false }).Gain(channel, false) == mix.Gain(channel, true));
        }
        var changed = settings with { Master = 0 };
        Check(changed.AudioMix.Gain(AudioChannel.Effects, true) == 0 && mix.Gain(AudioChannel.Effects, true) > 0);
        using var serialized = JsonDocument.Parse(JsonSerializer.Serialize(settings));
        Check(!serialized.RootElement.TryGetProperty("AudioMix", out _), "The audio snapshot must not change the settings format.");
        Check(serialized.RootElement.GetProperty("Effects").GetInt32() == 40);
        Check(typeof(AudioMix).Assembly.GetName().Name == "PawnShop.Audio");
    }

    internal static void AudioProjectContent()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string project = Path.Combine(root, "PawnShop.Audio");
        var xml = XDocument.Load(Path.Combine(project, "PawnShop.Audio.csproj"));
        Check(!xml.Descendants("ProjectReference").Any(), "Audio must not depend on Game or domain projects.");
        Check(!XDocument.Load(Path.Combine(root, "PawnShop.Core/PawnShop.Core.csproj"))
            .Descendants("ProjectReference").Any(), "Core must stay independent of presentation audio.");
        foreach (string category in new[] { "Music", "UI", "Environmental", "Combat", "Monster", "Speech" })
            Check(Directory.Exists(Path.Combine(project, "Content", category)));
        Check(!File.Exists(Path.Combine(root, "PawnShop.Framework/Audio/LoopingMusic.cs")));
        Check(!File.Exists(Path.Combine(root, "PawnShop.Game/Content/Audio/Forgotten_Treasures.wav")));
        var assets = new[]
        {
            (AudioAssets.MenuTheme, "D5AAD4009AC742BF84CDE2697391A8D57E7D78C4D9123AB23AF7A8B61FE6F5EA"),
            (AudioAssets.SwitchOn, "B4239F1FA57B1D9747E2E000E5870812DAF306CB7FD29DE2D9C868AE9FD889BF"),
            (AudioAssets.SwitchOff, "3FD1940FFF0FC29798AAEFB46A856402C959055A2145E6460BDA513C47CFA30E"),
            (AudioAssets.ButtonClick, "9375936391B2EA1F51875429A2F1DB607C8DC51966E2ACAAB9260D757AAAD807"),
            (AudioAssets.TabClick, "6F785180AE5DE592A5D8DF3A4F5A822A79B3DC6A7CA6360E15BFC01AAE671F9A")
        };
        foreach (var (asset, hash) in assets)
        {
            string relative = asset["Audio/".Length..];
            string source = Path.Combine(project, "Content", relative + ".wav");
            using var wav = File.OpenRead(source);
            Check(Convert.ToHexString(SHA256.HashData(wav)) == hash, "Audio extraction must preserve original WAVs.");
            string compiled = Path.Combine(AppContext.BaseDirectory, "Content", asset + ".xnb");
            string owned = Path.Combine(project, "Content/bin/DesktopGL/Content", relative + ".xnb");
            Check(File.ReadAllBytes(compiled).SequenceEqual(File.ReadAllBytes(owned)), "Audio-owned compiled content must reach consumers.");
            Check(!File.Exists(Path.Combine(AppContext.BaseDirectory, "Content", asset + ".wav")));
        }
    }
}
