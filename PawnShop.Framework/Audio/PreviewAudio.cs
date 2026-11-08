using Microsoft.Xna.Framework.Audio;

namespace PawnShop.Framework.Audio;

// Small owned preview sound; no external assets, streams or background work.
public sealed class PreviewAudio : IDisposable
{
    private SoundEffect? tone;
    private SoundEffectInstance? instance;
    public string? Error { get; private set; }

    public void Play(float volume)
    {
        if (Error is not null) return;
        try
        {
            if (tone is null)
            {
                const int rate = 22050, count = rate / 4;
                var data = new byte[count * 2];
                for (int i = 0; i < count; i++)
                {
                    double envelope = Math.Min(1, i / 220.0) * (1 - (double)i / count);
                    short sample = (short)(Math.Sin(2 * Math.PI * 440 * i / rate) * 8000 * envelope);
                    data[i * 2] = (byte)(sample & 255);
                    data[i * 2 + 1] = (byte)(sample >> 8);
                }
                var candidate = new SoundEffect(data, rate, AudioChannels.Mono);
                try { instance = candidate.CreateInstance(); }
                catch { candidate.Dispose(); throw; }
                tone = candidate;
            }
            instance!.Stop();
            instance.Volume = volume;
            instance.Play();
        }
        catch (Exception e) when (e is NoAudioHardwareException or InvalidOperationException)
        {
            Error = "Audio preview unavailable on this device.";
        }
    }

    public void SetVolume(float volume)
    {
        if (instance is not null) instance.Volume = volume;
    }

    public void Dispose()
    {
        instance?.Dispose();
        tone?.Dispose();
    }
}
