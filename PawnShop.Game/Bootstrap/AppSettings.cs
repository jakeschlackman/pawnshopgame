using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;

namespace PawnShop.Game.Bootstrap;

public enum WindowMode { Windowed, Borderless, Fullscreen }
public enum DialogueSpeed { Slow, Normal, Fast, Instant }
public enum AudioChannel { Music, Ambient, Effects, Combat, Monster }
public readonly record struct DisplaySettings(int Width = 1280, int Height = 720, WindowMode Mode = WindowMode.Windowed);

// Application preferences are independent of gameplay saves and session lifetimes.
public sealed record AppSettings
{
    public const int MinimumWidth = 1280, MinimumHeight = 720;
    public int Version { get; init; } = 1;
    // Distinguish old unbound defaults from alternates intentionally cleared later.
    public int ActionAlternateDefaultsVersion { get; init; } = 2;
    public DisplaySettings Display { get; init; } = new(1280, 720);
    public int UiScale { get; init; } = 100;
    public int Master { get; init; } = 80;
    public int Music { get; init; } = 80;
    public int Ambient { get; init; } = 80;
    public int Effects { get; init; } = 80;
    public int Combat { get; init; } = 80;
    public int Monster { get; init; } = 80;
    public bool PauseUnfocused { get; init; } = true;
    public bool ScreenShake { get; init; } = true;
    public bool MuteUnfocused { get; init; } = true;
    public DialogueSpeed Dialogue { get; init; } = DialogueSpeed.Normal;
    public Keys InteractKey { get; init; } = Keys.Z;
    public Keys PauseKey { get; init; } = Keys.X;
    public Keys InteractAlternateKey { get; init; } = Keys.None;
    public Keys PauseAlternateKey { get; init; } = Keys.None;
    public MouseBinding InteractAlternateMouse { get; init; } = MouseBinding.Left;
    public MouseBinding PauseAlternateMouse { get; init; } = MouseBinding.Right;
    public KeyBinding MoveUp { get; init; } = new(Keys.W, Keys.Up);
    public KeyBinding MoveLeft { get; init; } = new(Keys.A, Keys.Left);
    public KeyBinding MoveDown { get; init; } = new(Keys.S, Keys.Down);
    public KeyBinding MoveRight { get; init; } = new(Keys.D, Keys.Right);

    [JsonIgnore]
    public KeyboardBindings Bindings => new()
    {
        Interact = new(InteractKey, InteractAlternateKey, InteractAlternateMouse),
        PauseBack = new(PauseKey, PauseAlternateKey, PauseAlternateMouse),
        MoveUp = MoveUp, MoveLeft = MoveLeft, MoveDown = MoveDown, MoveRight = MoveRight
    };

    public AppSettings WithBinding(ControlAction action, int slot, Keys key)
    {
        if (slot is not (0 or 1)) throw new ArgumentOutOfRangeException(nameof(slot));
        KeyBinding binding = Bindings.Get(action);
        binding = slot == 0 ? binding with { Primary = key } : binding with { Alternate = key };
        return action switch
        {
            ControlAction.Interact => this with { InteractKey = binding.Primary, InteractAlternateKey = binding.Alternate,
                InteractAlternateMouse = slot == 1 ? MouseBinding.None : binding.AlternateMouse },
            ControlAction.PauseBack => this with { PauseKey = binding.Primary, PauseAlternateKey = binding.Alternate,
                PauseAlternateMouse = slot == 1 ? MouseBinding.None : binding.AlternateMouse },
            ControlAction.MoveUp => this with { MoveUp = binding }, ControlAction.MoveLeft => this with { MoveLeft = binding },
            ControlAction.MoveDown => this with { MoveDown = binding }, ControlAction.MoveRight => this with { MoveRight = binding },
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
    }

    public AppSettings WithMouseBinding(ControlAction action, MouseBinding button) => action switch
    {
        ControlAction.Interact => this with { InteractAlternateKey = Keys.None, InteractAlternateMouse = button },
        ControlAction.PauseBack => this with { PauseAlternateKey = Keys.None, PauseAlternateMouse = button },
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };

    [JsonIgnore]
    public bool IsValid => Version == 1 && ActionAlternateDefaultsVersion == 2 && Enum.IsDefined(Display.Mode)
        && Resolutions.Contains((Display.Width, Display.Height)) && UiScale is 100 or 125 or 150
        && new[] { Master, Music, Ambient, Effects, Combat, Monster }.All(v => v is >= 0 and <= 100)
        && Enum.IsDefined(Dialogue) && ValidBindings;

    [JsonIgnore]
    private bool ValidBindings
    {
        get
        {
            var bindings = Bindings;
            var used = new HashSet<Keys>();
            var usedMouse = new HashSet<MouseBinding>();
            foreach (var action in Enum.GetValues<ControlAction>())
            {
                var binding = bindings.Get(action);
                if (!ValidKey(binding.Primary) || !used.Add(binding.Primary)) return false;
                if (binding.Alternate != Keys.None && (!ValidKey(binding.Alternate) || !used.Add(binding.Alternate))) return false;
                if (!Enum.IsDefined(binding.AlternateMouse)) return false;
                if (binding.AlternateMouse != MouseBinding.None && (binding.Alternate != Keys.None
                    || action is not (ControlAction.Interact or ControlAction.PauseBack) || !usedMouse.Add(binding.AlternateMouse))) return false;
                if (!AllowedFor(action, binding.Primary) || !AllowedFor(action, binding.Alternate)) return false;
            }
            return true;
        }
    }

    public static bool AllowedFor(ControlAction action, Keys key) => key switch
    {
        Keys.Tab => false,
        Keys.Escape => action == ControlAction.PauseBack,
        Keys.Enter or Keys.Space => action == ControlAction.Interact,
        Keys.Back or Keys.Delete => false,
        _ => true
    };

    public static bool ValidKey(Keys key) => Enum.IsDefined(key) && key != Keys.None
        && key is not (Keys.LeftWindows or Keys.RightWindows or Keys.F4);

    public static IReadOnlyList<(int Width, int Height)> Resolutions { get; } = Array.AsReadOnly(new[]
    {
        (MinimumWidth, MinimumHeight), (1366, 768), (1600, 900), (1920, 1080),
        (1920, 1200), (2560, 1440), (2560, 1600), (3440, 1440), (3840, 2160)
    });

    public float Gain(AudioChannel channel, bool active) => !active && MuteUnfocused ? 0
        : Master / 100f * (channel switch
        {
            AudioChannel.Music => Music, AudioChannel.Ambient => Ambient,
            AudioChannel.Combat => Combat, AudioChannel.Monster => Monster, _ => Effects
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
        if (!File.Exists(path))
        {
            var defaults = new AppSettings();
            try { Save(defaults); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                warning = "Could not create default options. Defaults loaded; Apply to retry saving.";
            }
            return defaults;
        }
        try
        {
            string json = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (result is null) throw new InvalidDataException("Invalid preferences.");
            // Retain other preferences from the formerly supported small preset.
            if (result.Display.Width == 960 && result.Display.Height == 540)
                result = result with { Display = result.Display with { Width = AppSettings.MinimumWidth, Height = AppSettings.MinimumHeight } };
            using var document = JsonDocument.Parse(json);
            bool legacy = !new[] { "MoveUp", "MoveLeft", "MoveDown", "MoveRight", "InteractAlternateKey", "PauseAlternateKey" }
                .Any(name => document.RootElement.TryGetProperty(name, out _));
            if (legacy)
            {
                // Add movement defaults without overwriting old custom actions or the original file.
                if (result.InteractKey == Keys.Space && result.PauseKey == Keys.Escape)
                    result = result with { InteractKey = Keys.Z, PauseKey = Keys.X };
                var used = new HashSet<Keys> { result.InteractKey, result.PauseKey };
                foreach (var action in new[] { ControlAction.MoveUp, ControlAction.MoveLeft, ControlAction.MoveDown, ControlAction.MoveRight })
                {
                    var binding = result.Bindings.Get(action);
                    Keys primary = binding.Primary, alternate = binding.Alternate;
                    if (used.Contains(primary))
                        primary = !used.Contains(alternate) ? alternate
                            : new[] { Keys.I, Keys.J, Keys.K, Keys.L }.First(k => !used.Contains(k));
                    if (used.Contains(alternate) || alternate == primary) alternate = Keys.None;
                    if (new KeyBinding(primary, alternate) != binding)
                        warning = "Existing action keys kept. Conflicting movement defaults adjusted; review Controls.";
                    result = result.WithBinding(action, 0, primary).WithBinding(action, 1, alternate);
                    used.Add(primary);
                    if (alternate != Keys.None) used.Add(alternate);
                }
            }
            int oldAlternateVersion = document.RootElement.TryGetProperty(nameof(AppSettings.ActionAlternateDefaultsVersion), out var marker)
                ? marker.GetInt32() : 0;
            // Upgrade earlier default alternates to mouse buttons, preserving custom keys
            // and deliberate clears from marked saves. Loading never rewrites the file.
            foreach (var action in new[] { ControlAction.Interact, ControlAction.PauseBack })
            {
                string field = action == ControlAction.Interact ? "InteractAlternateKey" : "PauseAlternateKey";
                string mouseField = action == ControlAction.Interact ? "InteractAlternateMouse" : "PauseAlternateMouse";
                if (document.RootElement.TryGetProperty(mouseField, out _)) continue;
                var current = result.Bindings.Get(action);
                Keys defaultPrimary = action == ControlAction.Interact ? Keys.Z : Keys.X;
                Keys oldDefault = action == ControlAction.Interact ? Keys.Enter : Keys.Escape;
                bool oldDefaultAlternate = oldAlternateVersion < 2 && current.Primary == defaultPrimary
                    && (current.Alternate == oldDefault || oldAlternateVersion == 0 && current.Alternate == Keys.None);
                if (!document.RootElement.TryGetProperty(field, out _) || oldDefaultAlternate)
                {
                    var button = action == ControlAction.Interact ? MouseBinding.Left : MouseBinding.Right;
                    bool conflict = Enum.GetValues<ControlAction>().Any(other => other != action
                        && result.Bindings.Get(other).AlternateMouse == button);
                    result = result.WithMouseBinding(action, conflict ? MouseBinding.None : button);
                }
                else result = result.WithBinding(action, 1, current.Alternate);
            }
            if (oldAlternateVersion < 2) result = result with { ActionAlternateDefaultsVersion = 2 };
            if (!result.IsValid) throw new InvalidDataException("Invalid or unsupported preferences.");
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
