using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace PawnShop.Audio;

// One voice, including when using several samples. Instances and content are owned here.
public sealed class SoundEffectPlayer(IServiceProvider services, string contentRoot, params string[] assetIds) : IDisposable
{
    private readonly ContentManager content = new(services, contentRoot);
    private readonly SoundEffectInstance?[] instances = new SoundEffectInstance?[assetIds.Length];
    private SoundEffectInstance? instance;
    private bool disposed;
    public string? Error { get; private set; }

    public void Play(float volume, float pitch = 0, int sample = 0)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Error is not null) return;
        string assetId = assetIds[sample];
        try
        {
            // Stop the previous sample before switching, so variations cannot stack.
            instance?.Stop();
            instance = instances[sample] ??= content.Load<SoundEffect>(assetId).CreateInstance();
            instance.Volume = volume;
            instance.Pitch = pitch;
            instance.Play();
        }
        catch (Exception e) when (e is NoAudioHardwareException or InvalidOperationException or ContentLoadException)
        {
            Error = $"Sound '{assetId}' unavailable: {e.Message}";
        }
    }

    public void SetVolume(float volume)
    {
        if (instance is not null) instance.Volume = volume;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (var owned in instances) owned?.Dispose();
        instance = null;
        content.Dispose();
    }
}
