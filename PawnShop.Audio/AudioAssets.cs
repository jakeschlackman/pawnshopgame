namespace PawnShop.Audio;

// IDs are relative to the application's ContentManager root. Source files and
// the MGCB definition live in this project; only compiled content ships.
public static class AudioAssets
{
    public const string MenuTheme = "Audio/Music/Forgotten_Treasures";
    public const string SwitchOn = "Audio/UI/switch_on";
    public const string SwitchOff = "Audio/UI/switch_off";
    public const string ButtonClick = "Audio/UI/sub_click";
    public const string TabClick = "Audio/UI/tab_click";
    public static IReadOnlyList<string> DropdownClicks { get; } = Array.AsReadOnly(new[]
    {
        "Audio/UI/dropdown_click_01", "Audio/UI/dropdown_click_02",
        "Audio/UI/dropdown_click_03", "Audio/UI/dropdown_click_04"
    });
    public static IReadOnlyList<string> RotaryClicks { get; } = Array.AsReadOnly(new[]
    {
        "Audio/UI/rotary_click_01", "Audio/UI/rotary_click_02", "Audio/UI/rotary_click_03",
        "Audio/UI/rotary_click_04", "Audio/UI/rotary_click_05", "Audio/UI/rotary_click_06"
    });
}
