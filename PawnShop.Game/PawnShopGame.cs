using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using PawnShop.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Framework.Audio;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Game;

public sealed class PawnShopGame : Microsoft.Xna.Framework.Game
{
    private const int Width = 640, Height = 360;
    private readonly ApplicationFlow flow;
    private readonly GraphicsDeviceManager graphics;
    private readonly KeyboardInput input = new();
    private readonly MenuPointerInput pointer = new();
    private readonly PreviewAudio audio = new();
    private DisplaySettings appliedDisplay = new(1280, 720);
    private AudioChannel previewChannel = AudioChannel.Effects;
    private string? startupWarning;
    private (int Width, int Height) desktopSize;
    private PixelRenderer? renderer;
    private GameScreens? screens;
    private FontLibrary? fonts;

    public PawnShopGame()
    {
        var store = new SettingsStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RummageHollow", "settings.json"));
        var settings = store.Load(out startupWarning);
        flow = new(settings, store.Save);
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Width * 2,
            PreferredBackBufferHeight = Height * 2,
            SynchronizeWithVerticalRetrace = true
        };
        Window.Title = "Picker's Hollow";
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
    }

    protected override void LoadContent()
    {
        var desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        desktopSize = (desktop.Width, desktop.Height);
        string contentRoot = Path.Combine(AppContext.BaseDirectory, "Content");
        var catalog = FontCatalog.Load(Path.Combine(contentRoot, "Fonts", "fonts.json"));
        fonts = new FontLibrary(Services, contentRoot, catalog);
        renderer = new PixelRenderer(GraphicsDevice, Width, Height, fonts: fonts);
        screens = new GameScreens(GraphicsDevice, flow.TitleCamera, fonts, flow.TitleSelector.Definition);
    }

    protected override void Update(GameTime gameTime)
    {
        input.InteractKey = flow.Options.Current.InteractKey;
        input.PauseKey = flow.Options.Current.PauseKey;
        input.InMenus = flow.Screen != Screen.Room;
        bool capturing = flow.Screen == Screen.Options && flow.Options.CapturingKey;
        var frame = input.Read(IsActive);
        var bounds = GraphicsDevice.PresentationParameters;
        var click = pointer.Read(flow, Mouse.GetState(), IsActive, Window.ClientBounds.Width, Window.ClientBounds.Height,
            bounds.BackBufferWidth, bounds.BackBufferHeight);
        if (click != default) frame = click; // One UI intent per frame; pointer and keyboard cannot double-activate.
        if (capturing && IsActive) flow.Options.Capture(input.State.GetPressedKeys());
        flow.Update(capturing ? default : frame, gameTime.ElapsedGameTime, IsActive);
        if (flow.Screen == Screen.Options && startupWarning is { } warning)
        {
            flow.Options.Report(warning);
            startupWarning = null;
        }
        SynchronizeDisplay();
        if (flow.Options.TakeTone() is { } channel)
        {
            previewChannel = channel;
            audio.Play(flow.Options.Presentation.Gain(channel, IsActive));
            if (audio.Error is { } error) flow.Options.Report(error);
        }
        var audioSettings = flow.Screen == Screen.Options ? flow.Options.Presentation : flow.Options.Current;
        audio.SetVolume(audioSettings.Gain(previewChannel, IsActive));
        screens?.SynchronizeRoom(flow.Room);
        if (flow.ExitRequested) Exit();
        base.Update(gameTime);
    }

    private void SynchronizeDisplay()
    {
        var desired = flow.Options.DisplayForHost;
        if (desired == appliedDisplay) return;
        try
        {
            ApplyDisplay(desired);
            appliedDisplay = desired;
        }
        catch (Exception e) when (e is InvalidOperationException or ArgumentException or NoSuitableGraphicsDeviceException)
        {
            flow.Options.RejectDisplay("Display mode unavailable. Previous display restored.");
            if (flow.Options.Current.Display != appliedDisplay)
            {
                flow.Options.UseSafeDisplay();
                appliedDisplay = new(1280, 720);
                startupWarning = "Saved display unavailable. Windowed defaults restored.";
            }
            // A driver may have partially switched before reporting failure.
            ApplyDisplay(appliedDisplay);
        }
    }

    private void ApplyDisplay(DisplaySettings display)
    {
        // Retain the desktop size: CurrentDisplayMode can change in exclusive fullscreen.
        var desktop = desktopSize;
        if (display.Mode == WindowMode.Windowed && (display.Width > desktop.Width || display.Height > desktop.Height))
            throw new ArgumentException("Window exceeds the desktop.");
        if (display.Mode == WindowMode.Fullscreen && !GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
            .Any(mode => mode.Width == display.Width && mode.Height == display.Height))
            throw new ArgumentException("Fullscreen resolution is not supported by this display.");
        graphics.HardwareModeSwitch = display.Mode == WindowMode.Fullscreen;
        graphics.IsFullScreen = display.Mode != WindowMode.Windowed;
        Window.IsBorderless = display.Mode == WindowMode.Borderless;
        graphics.PreferredBackBufferWidth = display.Mode == WindowMode.Borderless ? desktop.Width : display.Width;
        graphics.PreferredBackBufferHeight = display.Mode == WindowMode.Borderless ? desktop.Height : display.Height;
        graphics.ApplyChanges();
        Window.AllowUserResizing = false;
    }

    protected override void Draw(GameTime gameTime)
    {
        if (renderer is null || screens is null) return;
        screens.Draw(renderer, flow);
        base.Draw(gameTime);
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        if (!flow.ExitRequested)
        {
            flow.RequestExit();
            args.Cancel = !flow.ExitRequested;
        }
        base.OnExiting(sender, args);
    }

    protected override void UnloadContent()
    {
        ReleaseGraphics();
        base.UnloadContent();
    }

    private void ReleaseGraphics()
    {
        screens?.Dispose();
        renderer?.Dispose();
        fonts?.Dispose();
        screens = null;
        renderer = null;
        fonts = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            flow.Dispose();
            audio.Dispose();
            ReleaseGraphics();
        }
        base.Dispose(disposing);
    }
}
