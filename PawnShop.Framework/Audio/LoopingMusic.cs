using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace PawnShop.Framework.Audio;

// Device boundary for one owned track; lifecycle tests need no audio device.
public interface IMusicTrack : IDisposable
{
    TimeSpan Duration { get; }
    TimeSpan Position { get; }
    void SetVolume(float volume);
    void PlayLooped();
    void Stop();
}

// Start/stop only on screen changes. Fades use the audio cursor, never simulation time.
public sealed class LoopingMusic(IMusicTrack track) : IDisposable
{
    public static readonly TimeSpan LoopFadeDuration = TimeSpan.FromSeconds(1.5);
    private bool playing, disposed, released;
    private bool repeated;
    private TimeSpan previousPosition;
    private float lastVolume = -1;
    public string? Error { get; private set; }

    public void Update(bool enabled, float volume)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!float.IsFinite(volume) || volume is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(volume));
        if (Error is not null) return;
        try
        {
            if (enabled)
            {
                if (!playing)
                {
                    repeated = false;
                    previousPosition = TimeSpan.Zero;
                    SetVolume(volume); // No entrance fade: only the loop boundary fades.
                    track.PlayLooped();
                    playing = true;
                }
                else SetVolume(volume * LoopGain());
            }
            else if (playing) { track.Stop(); playing = false; }
        }
        catch (Exception e) when (IsPlaybackFailure(e))
        {
            Error = $"Menu music unavailable: {e.Message}";
            ReleaseTrack(); // No retry loop against a missing asset or failed device.
        }
    }

    private float LoopGain()
    {
        var duration = track.Duration;
        if (duration <= TimeSpan.Zero) throw new InvalidOperationException("Music duration must be positive.");
        var position = TimeSpan.FromTicks(Math.Clamp(track.Position.Ticks, 0, duration.Ticks));
        if (position < previousPosition) repeated = true;
        previousPosition = position;
        double fade = Math.Min(LoopFadeDuration.TotalSeconds, duration.TotalSeconds / 2);
        double gain = Math.Clamp((duration - position).TotalSeconds / fade, 0, 1);
        if (repeated) gain = Math.Min(gain, Math.Clamp(position.TotalSeconds / fade, 0, 1));
        return (float)(gain * gain * (3 - 2 * gain));
    }

    private void SetVolume(float volume)
    {
        if (lastVolume == volume) return;
        track.SetVolume(volume);
        lastVolume = volume;
    }

    private static bool IsPlaybackFailure(Exception e) => e is NoAudioHardwareException
        or ContentLoadException or IOException or UnauthorizedAccessException or InvalidOperationException;

    private void ReleaseTrack()
    {
        if (released) return;
        released = true;
        try { track.Dispose(); }
        catch (Exception e) when (IsPlaybackFailure(e))
        {
            Error ??= $"Menu music unavailable: {e.Message}";
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ReleaseTrack();
    }
}
