using System.Security.Cryptography;
using PawnShop.Audio;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private static void SynchronizeMusic(ApplicationFlow app, LoopingMusic music, bool active = true)
    {
        var settings = app.Screen == Screen.Options ? app.Options.Presentation : app.Options.Current;
        music.Update(app.PlaysTitleMusic,
            AudioPlayback.MenuThemeGain * settings.Gain(AudioChannel.Music, active));
    }

    internal static void MenuMusicNavigation()
    {
        using var app = new ApplicationFlow(playTitleIntro: true);
        var track = new TestMusicTrack();
        using var music = new LoopingMusic(track);
        SynchronizeMusic(app, music);
        Check(track.Plays == 1 && track.Playing && app.TitleIntroPlaying);
        app.Update(default, TimeSpan.FromSeconds(10));
        SynchronizeMusic(app, music);
        OpenOptions(app);
        SynchronizeMusic(app, music); // Outgoing panorama/selector travel.
        FinishOptionsFlight(app);
        SynchronizeMusic(app, music);
        Back(app);
        SynchronizeMusic(app, music); // Return travel also retains the same playback.
        FinishOptionsFlight(app);
        SynchronizeMusic(app, music);
        Check(track.Plays == 1 && track.Stops == 0);

        app.Update(new(SelectionIndex: 0), TimeSpan.Zero);
        app.Update(new(Confirm: true), TimeSpan.Zero);
        SynchronizeMusic(app, music);
        Check(app.CharacterSelectionFlying && track.Plays == 1 && track.Stops == 0 && track.Playing);
        app.Update(default, PawnShop.Framework.Rendering.PanoramaCamera.TransitionDuration);
        SynchronizeMusic(app, music);
        Check(app.Screen == Screen.CharacterSelect && track.Plays == 1 && track.Stops == 0 && track.Playing);
        SynchronizeMusic(app, music, active: false);
        Check(track.Volume == 0 && track.Playing && track.Stops == 0);
        SynchronizeMusic(app, music);
        Check(track.Volume > 0 && track.Plays == 1);
        Back(app);
        SynchronizeMusic(app, music);
        Check(track.Plays == 1 && track.Stops == 0 && track.Playing);
        Begin(app);
        SynchronizeMusic(app, music);
        Check(app.Screen == Screen.Room && track.Stops == 1 && !track.Playing);
        Back(app);
        app.Update(new(SelectionIndex: 3, Confirm: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Options && app.LiveSessions == 1);
        SynchronizeMusic(app, music);
        Check(track.Plays == 1 && !track.Playing, "Pause-menu Options must not start title music.");
        Back(app);
        Back(app); // Resume before the normal return-to-title confirmation.
        ReturnPrompt(app);
        Discard(app);
        SynchronizeMusic(app, music);
        Check(app.Screen == Screen.Title && track.Plays == 2 && track.Playing);
        app.RequestExit();
        SynchronizeMusic(app, music);
        Check(track.Stops == 2 && !track.Playing);
        music.Dispose();
        music.Dispose();
        Check(track.Disposals == 1);
        bool rejected = false;
        try { music.Update(true, 1); }
        catch (ObjectDisposedException) { rejected = true; }
        Check(rejected);
    }

    internal static void MenuMusicSettings()
    {
        using var app = new ApplicationFlow(new() { Master = 50, Music = 80 });
        var track = new TestMusicTrack();
        using var music = new LoopingMusic(track);
        SynchronizeMusic(app, music);
        Check(Math.Abs(track.Volume - .2f) < .0001f && track.VolumeAtStart == track.Volume);
        OpenOptions(app);
        FinishOptionsFlight(app);
        app.Options.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        app.Options.Update(new(SelectionIndex: 1, SliderValue: 20), TimeSpan.Zero, true);
        SynchronizeMusic(app, music);
        Check(Math.Abs(track.Volume - .05f) < .0001f && app.Options.Current.Music == 80);
        SynchronizeMusic(app, music, active: false);
        Check(track.Volume == 0 && track.Playing && track.Stops == 0);
        SynchronizeMusic(app, music);
        Check(Math.Abs(track.Volume - .05f) < .0001f && track.Plays == 1);

        app.Options.Cancel(); // Discard restores the saved gain without restarting.
        SynchronizeMusic(app, music);
        Check(Math.Abs(track.Volume - .2f) < .0001f);
        app.Options.Update(new(SelectionIndex: 0, SliderValue: 25), TimeSpan.Zero, true);
        app.Options.Update(new(SelectionIndex: 1, SliderValue: 40), TimeSpan.Zero, true);
        app.Options.Update(new(SelectionIndex: 6, Confirm: true), TimeSpan.Zero, true); // Mute Unfocused off.
        SynchronizeMusic(app, music, active: false);
        Check(Math.Abs(track.Volume - .05f) < .0001f && !app.Options.Draft.MuteUnfocused);
        app.Options.Update(new(SelectionIndex: app.Options.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(app.Options.Current.Master == 25 && app.Options.Current.Music == 40);
        Back(app);
        FinishOptionsFlight(app);
        SynchronizeMusic(app, music);
        Check(Math.Abs(track.Volume - .05f) < .0001f && track.Plays == 1 && track.Stops == 0);
        music.Dispose();
        Check(track.Disposals == 1 && !track.Playing);

        var failedTrack = new TestMusicTrack { FailOnPlay = true };
        using var failed = new LoopingMusic(failedTrack);
        failed.Update(true, .5f);
        failed.Update(true, .5f);
        failed.Update(false, .5f);
        Check(failed.Error!.Contains("Injected missing music") && failedTrack.Plays == 1 && failedTrack.Disposals == 1);
    }

    internal static void MenuMusicContent()
    {
        string name = Path.GetFileName(AudioAssets.MenuTheme);
        string output = Path.Combine(AppContext.BaseDirectory, "Content", "Audio", "Music");
        string ogg = Path.Combine(output, name + ".ogg");
        Check(File.Exists(Path.Combine(output, name + ".xnb")) && File.Exists(ogg));
        using var stream = File.OpenRead(ogg);
        Span<byte> header = stackalloc byte[4];
        stream.ReadExactly(header);
        Check(header.SequenceEqual("OggS"u8) && stream.Length > 0 && stream.Length < 37044044);
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string source = Path.Combine(root, "PawnShop.Audio", "Content", "Music", name + ".wav");
        using var wav = File.OpenRead(source);
        Check(Convert.ToHexString(SHA256.HashData(wav)) == "D5AAD4009AC742BF84CDE2697391A8D57E7D78C4D9123AB23AF7A8B61FE6F5EA",
            "The imported source WAV must remain byte-identical to the supplied theme.");
        Check(!File.Exists(Path.Combine(output, name + ".wav")), "Ship the compiled stream; keep the source WAV in the repository.");
    }

    internal static void MenuMusicLoopFades()
    {
        var track = new TestMusicTrack();
        using var music = new LoopingMusic(track);
        music.Update(true, .8f);
        track.PlaybackPosition = TimeSpan.FromSeconds(.75);
        music.Update(true, .8f);
        Check(track.Volume == .8f, "Initial entry must not fade in.");
        track.PlaybackPosition = track.Duration - LoopingMusic.LoopFadeDuration;
        music.Update(true, .8f);
        Check(track.Volume == .8f);
        track.PlaybackPosition = track.Duration - LoopingMusic.LoopFadeDuration / 2;
        music.Update(true, .8f);
        Check(Math.Abs(track.Volume - .4f) < .0001f);
        music.Update(true, .5f);
        Check(Math.Abs(track.Volume - .25f) < .0001f, "Volume previews must multiply the active fade.");
        music.Update(true, 0);
        Check(track.Volume == 0 && track.Playing);
        track.PlaybackPosition = track.Duration;
        music.Update(true, .8f);
        Check(track.Volume == 0);
        track.PlaybackPosition = TimeSpan.Zero; // Backend repeats the song, without a new Play command.
        music.Update(true, .8f);
        Check(track.Volume == 0 && track.Plays == 1);
        track.PlaybackPosition = LoopingMusic.LoopFadeDuration / 2;
        music.Update(true, .8f);
        Check(Math.Abs(track.Volume - .4f) < .0001f);
        track.PlaybackPosition = LoopingMusic.LoopFadeDuration;
        music.Update(true, .8f);
        Check(track.Volume == .8f && track.Stops == 0 && track.Plays == 1);
        track.PlaybackPosition = track.Duration - TimeSpan.FromSeconds(.1);
        music.Update(true, .8f);
        track.PlaybackPosition = LoopingMusic.LoopFadeDuration / 2; // Polls need not hit the exact loop endpoint.
        music.Update(true, .8f);
        Check(Math.Abs(track.Volume - .4f) < .0001f && track.Plays == 1);
        music.Update(false, .8f);
        Check(!track.Playing && track.Stops == 1, "Leaving the menu stops immediately, without an exit fade.");
        music.Update(true, .8f);
        Check(track.VolumeAtStart == .8f && track.Plays == 2, "Re-entry must not inherit the last loop fade.");
    }

    private sealed class TestMusicTrack : IMusicTrack
    {
        public int Plays, Stops, Disposals;
        public float Volume, VolumeAtStart;
        public bool Playing, FailOnPlay;
        public TimeSpan PlaybackPosition;
        public TimeSpan Duration => TimeSpan.FromSeconds(140);
        public TimeSpan Position => PlaybackPosition;
        public void SetVolume(float volume) => Volume = volume;
        public void PlayLooped()
        {
            Plays++;
            if (FailOnPlay) throw new IOException("Injected missing music");
            VolumeAtStart = Volume;
            PlaybackPosition = TimeSpan.Zero;
            Playing = true;
        }
        public void Stop() { Stops++; Playing = false; }
        public void Dispose() { Disposals++; Playing = false; }
    }
}
