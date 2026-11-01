using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;

namespace PawnShop.Game.Bootstrap;

public enum WindowMode { Windowed, Borderless, Fullscreen }
public enum DialogueSpeed { Slow, Normal, Fast, Instant }
public enum AudioChannel { Music, Ambient, Effects }
public readonly record struct DisplaySettings(int Width = 1280, int Height = 720, WindowMode Mode = WindowMode.Windowed);

// Application preferences are independent of gameplay saves and session lifetimes.
public sealed record AppSettings
{
    public int Version { get; init; } = 1;
    public DisplaySettings Display { get; init; } = new(1280, 720);
    public int UiScale { get; init; } = 100;
    public int Master { get; init; } = 80;
    public int Music { get; init; } = 80;
    public int Ambient { get; init; } = 80;
    public int Effects { get; init; } = 80;
    public bool PauseUnfocused { get; init; } = true;
    public bool MuteUnfocused { get; init; } = true;
    public DialogueSpeed Dialogue { get; init; } = DialogueSpeed.Normal;
    public Keys InteractKey { get; init; } = Keys.Space;
    public Keys PauseKey { get; init; } = Keys.Escape;

    [JsonIgnore]
    public bool IsValid => Version == 1 && Enum.IsDefined(Display.Mode)
        && Resolutions.Contains((Display.Width, Display.Height)) && UiScale is 100 or 125 or 150
        && new[] { Master, Music, Ambient, Effects }.All(v => v is >= 0 and <= 100)
        && Enum.IsDefined(Dialogue) && ValidKey(InteractKey) && ValidKey(PauseKey) && InteractKey != PauseKey;

    public static bool ValidKey(Keys key) => Enum.IsDefined(key) && key != Keys.None
        && key is not (Keys.LeftWindows or Keys.RightWindows or Keys.F4);

    public static IReadOnlyList<(int Width, int Height)> Resolutions { get; } = Array.AsReadOnly(new[]
    {
        (960, 540), (1280, 720), (1366, 768), (1600, 900), (1920, 1080),
        (1920, 1200), (2560, 1440), (2560, 1600), (3440, 1440), (3840, 2160)
    });

    public float Gain(AudioChannel channel, bool active) => !active && MuteUnfocused ? 0
        : Master / 100f * (channel switch
        {
            AudioChannel.Music => Music, AudioChannel.Ambient => Ambient, _ => Effects
        }) / 100f;

    public int RevealedCharacters(TimeSpan elapsed, int length) => Dialogue == DialogueSpeed.Instant ? length
        : Math.Clamp((int)(elapsed.TotalSeconds * (Dialogue switch
        {
            DialogueSpeed.Slow => 15, DialogueSpeed.Fast => 60, _ => 30
        })), 0, length);
}

public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };

    public AppSettings Load(out string? warning)
    {
        warning = null;
        if (!File.Exists(path)) return new();
        try
        {
            var result = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOptions);
            if (result is null || !result.IsValid) throw new InvalidDataException("Invalid or unsupported preferences.");
            return result;
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
        {
            warning = "Could not load options. Defaults loaded; original kept until you apply.";
            return new();
        }
    }

    public void Save(AppSettings settings)
    {
        if (!settings.IsValid) throw new InvalidDataException("Invalid preferences.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(settings, JsonOptions));
        if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
        else File.Move(temporary, path);
    }
}
