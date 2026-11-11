using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace PawnShop.Framework.Audio;

// The application's sole MediaPlayer owner. ContentManager owns the loaded Song.
public sealed class SongMusicTrack(IServiceProvider services, string contentRoot, string assetId) : IMusicTrack
{
    private readonly ContentManager content = new(services, contentRoot);
    private Song? song;
    private bool disposed, subscribed;
    private long startedTimestamp;
    public TimeSpan Duration => song?.Duration ?? TimeSpan.Zero;
    // DesktopGL's PlayPosition includes decoder read-ahead. Native starts/repeats
    // reset this presentation clock so queued buffers cannot skip the fade-in.
    public TimeSpan Position => song is null ? TimeSpan.Zero : Stopwatch.GetElapsedTime(Interlocked.Read(ref startedTimestamp));

    public void SetVolume(float volume) => MediaPlayer.Volume = volume;

    public void PlayLooped()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        song ??= content.Load<Song>(assetId);
        if (!subscribed)
        {
            MediaPlayer.ActiveSongChanged += OnSongStarted;
            subscribed = true;
        }
        Interlocked.Exchange(ref startedTimestamp, Stopwatch.GetTimestamp());
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
    }

    public void Stop() => MediaPlayer.Stop();

    // The native streaming thread may notify a repeat. Publish only a timestamp;
    // playback commands, volume and the envelope remain owned by the host Update.
    private void OnSongStarted(object? sender, EventArgs args) =>
        Interlocked.Exchange(ref startedTimestamp, Stopwatch.GetTimestamp());

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (subscribed) MediaPlayer.ActiveSongChanged -= OnSongStarted;
        try { if (song is not null) Stop(); }
        finally { content.Dispose(); }
    }
}
