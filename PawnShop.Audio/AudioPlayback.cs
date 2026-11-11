namespace PawnShop.Audio;

// Application-owned playback. Call once per host update after consuming UI
// requests. This project has no dependency on Game screens or settings storage.
public sealed class AudioPlayback(IServiceProvider services, string contentRoot) : IDisposable
{
    public const float MenuThemeGain = .5f;
    public const float SliderClickGain = .8f;
    public const float CheckboxSwitchGain = .15f;
    public const float DropdownClickGain = .15f;
    private readonly SoundEffectPlayer checkbox = new(services, contentRoot, AudioAssets.SwitchOn, AudioAssets.SwitchOff);
    private readonly SoundEffectPlayer rotary = new(services, contentRoot, [.. AudioAssets.RotaryClicks]);
    private readonly SliderClickSequence rotarySequence = new(AudioAssets.RotaryClicks.Count, new Random());
    private readonly SoundEffectPlayer dropdown = new(services, contentRoot, [.. AudioAssets.DropdownClicks]);
    private readonly ShuffledSoundBag dropdownBag = new(AudioAssets.DropdownClicks.Count, new Random());
    private readonly SoundEffectPlayer button = new(services, contentRoot, AudioAssets.ButtonClick);
    private readonly SoundEffectPlayer tab = new(services, contentRoot, AudioAssets.TabClick);
    private readonly LoopingMusic music = new(new SongMusicTrack(services, contentRoot, AudioAssets.MenuTheme));
    private readonly HashSet<string> reportedErrors = [];
    private bool disposed;

    public void Update(TimeSpan delta, bool playMenuTheme, AudioMix mix, bool active,
        bool? checkboxState = null, bool buttonClick = false, bool tabClick = false, bool sliderClick = false,
        bool dropdownClick = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        float effectsGain = mix.Gain(AudioChannel.Effects, active);
        float rotaryGain = SliderClickGain * effectsGain;
        if (rotarySequence.Update(delta, sliderClick && rotaryGain > 0) is { } click)
            rotary.Play(rotaryGain, click.Pitch, click.Sample);
        float checkboxGain = CheckboxSwitchGain * effectsGain;
        if (checkboxState is { } isChecked && checkboxGain > 0)
            checkbox.Play(checkboxGain, sample: isChecked ? 0 : 1);
        float dropdownGain = DropdownClickGain * effectsGain;
        if (dropdownClick && dropdownGain > 0)
            dropdown.Play(dropdownGain, sample: dropdownBag.Next());
        if (buttonClick) button.Play(effectsGain);
        if (tabClick) tab.Play(effectsGain);
        checkbox.SetVolume(checkboxGain);
        dropdown.SetVolume(dropdownGain);
        rotary.SetVolume(rotaryGain);
        button.SetVolume(effectsGain);
        tab.SetVolume(effectsGain);
        music.Update(playMenuTheme, MenuThemeGain * mix.Gain(AudioChannel.Music, active));
    }

    // Keep errors available until the application has an appropriate UI to show
    // them, and report each failure once rather than repeating it every frame.
    public string? TakeError() => NewError(rotary.Error) ?? NewError(checkbox.Error) ?? NewError(button.Error)
        ?? NewError(tab.Error) ?? NewError(dropdown.Error) ?? NewError(music.Error);

    private string? NewError(string? error) => error is not null && reportedErrors.Add(error) ? error : null;

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        checkbox.Dispose();
        rotary.Dispose();
        dropdown.Dispose();
        button.Dispose();
        tab.Dispose();
        music.Dispose();
    }
}
