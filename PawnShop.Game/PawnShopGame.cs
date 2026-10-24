using Microsoft.Xna.Framework;
using PawnShop.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Game;

public sealed class PawnShopGame : Microsoft.Xna.Framework.Game
{
    private const int Width = 640, Height = 360;
    private readonly ApplicationFlow flow = new();
    private readonly KeyboardInput input = new();
    private PixelRenderer? renderer;
    private GameScreens? screens;

    public PawnShopGame()
    {
        _ = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Width * 2,
            PreferredBackBufferHeight = Height * 2,
            SynchronizeWithVerticalRetrace = true
        };
        Window.Title = "Rummage Hollow";
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
    }

    protected override void LoadContent()
    {
        renderer = new PixelRenderer(GraphicsDevice, Width, Height);
        screens = new GameScreens(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        flow.Update(input.Read(IsActive), gameTime.ElapsedGameTime, IsActive);
        screens?.SynchronizeRoom(flow.Room);
        if (flow.ExitRequested) Exit();
        base.Update(gameTime);
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
        screens = null;
        renderer = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            flow.Dispose();
            ReleaseGraphics();
        }
        base.Dispose(disposing);
    }
}
