using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using PawnShop.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Audio;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Game;

public sealed class PawnShopGame : Microsoft.Xna.Framework.Game
{
    private const int Width = 640, Height = 360;
    private readonly ApplicationFlow flow;
    private readonly GraphicsDeviceManager graphics;
    private readonly KeyboardInput input = new();
    private AppSettings? appliedInputSettings;
    private readonly MenuPointerInput pointer = new();
    private AudioPlayback? audio;
    private DisplaySettings appliedDisplay = new(1280, 720);
    private string? startupWarning;
    private (int Width, int Height) desktopSize;
    private PixelRenderer? renderer;
    private GameScreens? screens;
    private FontLibrary? fonts;
    private WindowsWindowIcon? windowIcon;

    public PawnShopGame()
    {
        var store = new SettingsStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PickersHollow", "settings.json"));
        var settings = store.Load(out startupWarning);
        flow = new(settings, store.Save, playTitleIntro: true);
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Width * 2,
            PreferredBackBufferHeight = Height * 2,
            SynchronizeWithVerticalRetrace = true
        };
        Window.Title = "Pickers Hollow";
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
    }

    protected override void Initialize()
    {
        // DesktopGL has created its final SDL window before Initialize is called.
        if (OperatingSystem.IsWindows())
            windowIcon = new WindowsWindowIcon(Window.Handle,
                Path.Combine(AppContext.BaseDirectory, "Content", "Application", "pickers-hollow.ico"));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        var desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        desktopSize = (desktop.Width, desktop.Height);
        string contentRoot = Path.Combine(AppContext.BaseDirectory, "Content");
        audio = new(Services, contentRoot);
        var catalog = FontCatalog.Load(Path.Combine(contentRoot, "Fonts", "fonts.json"));
        fonts = new FontLibrary(Services, contentRoot, catalog);
        renderer = new PixelRenderer(GraphicsDevice, Width, Height, fonts: fonts);
        screens = new GameScreens(GraphicsDevice, flow.TitleCamera, () => flow.TitleDescent, fonts,
            flow.TitleSelector.Definition, flow.PlayerSprites);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!ReferenceEquals(appliedInputSettings, flow.Options.Current))
        {
            appliedInputSettings = flow.Options.Current;
            input.Bindings = appliedInputSettings.Bindings;
        }
        bool capturing = flow.Screen == Screen.Options && flow.Options.CapturingKey;
        var mouse = Mouse.GetState();
        var frame = input.Read(mouse, IsActive);
        var bounds = GraphicsDevice.PresentationParameters;
        var click = pointer.Read(flow, mouse, IsActive, Window.ClientBounds.Width, Window.ClientBounds.Height,
            bounds.BackBufferWidth, bounds.BackBufferHeight);
        if (click != default) frame = click; // One UI intent per frame; pointer and keyboard cannot double-activate.
        if (capturing && IsActive) flow.Options.Capture(input.State.GetPressedKeys(), mouse);
        flow.Update(capturing ? default : frame, gameTime.ElapsedGameTime, IsActive);
        if (flow.Screen == Screen.Options && startupWarning is { } warning)
        {
            flow.Options.Report(warning);
            startupWarning = null;
        }
        SynchronizeDisplay();
        var audioSettings = flow.Screen == Screen.Options ? flow.Options.Presentation : flow.Options.Current;
        bool? checkboxState = flow.Options.TakeCheckboxSwitch();
        bool buttonClick = flow.Options.TakeButtonClick();
        bool tabClick = flow.Options.TakeTabClick();
        bool sliderClick = flow.Options.TakeSliderClick();
        bool dropdownClick = flow.Options.TakeDropdownClick();
        audio?.Update(gameTime.ElapsedGameTime, flow.UsesTitleBackground && !flow.ExitRequested, audioSettings.AudioMix, IsActive,
            checkboxState, buttonClick, tabClick, sliderClick, dropdownClick);
        if (flow.Screen == Screen.Options && audio?.TakeError() is { } audioError) flow.Options.Report(audioError);
        screens?.SynchronizeRoom(flow.Room, flow.Npcs, flow.CommonVisitor);
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
        int width = display.Mode == WindowMode.Borderless ? desktop.Width : display.Width;
        int height = display.Mode == WindowMode.Borderless ? desktop.Height : display.Height;
        if (width < AppSettings.MinimumWidth || height < AppSettings.MinimumHeight)
            throw new ArgumentException("Display resolution must be at least 1280x720.");
        if (display.Mode == WindowMode.Windowed && (display.Width > desktop.Width || display.Height > desktop.Height))
            throw new ArgumentException("Window exceeds the desktop.");
        if (display.Mode == WindowMode.Fullscreen && !GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
            .Any(mode => mode.Width == display.Width && mode.Height == display.Height))
            throw new ArgumentException("Fullscreen resolution is not supported by this display.");
        graphics.HardwareModeSwitch = display.Mode == WindowMode.Fullscreen;
        graphics.IsFullScreen = display.Mode != WindowMode.Windowed;
        Window.IsBorderless = display.Mode == WindowMode.Borderless;
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
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
        audio?.Dispose();
        audio = null;
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
            windowIcon?.Dispose();
            windowIcon = null;
            flow.Dispose();
            audio?.Dispose();
            audio = null;
            ReleaseGraphics();
        }
        base.Dispose(disposing);
    }
}
