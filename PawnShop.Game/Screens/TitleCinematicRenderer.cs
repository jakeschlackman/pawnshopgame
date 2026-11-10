using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Game.Bootstrap.Cinematics;

namespace PawnShop.Game.Screens;

// Diagnostic wire car only, not approved pixel art. Yaw and pitch change its
// projected box view independently of travel direction. No generated/imported art.
internal sealed class TitleCinematicRenderer : IDisposable
{
    private readonly SpriteBatch batch;
    private readonly Texture2D pixel;
    private readonly GraphicsDevice device;
    private readonly RasterizerState clipping = new() { ScissorTestEnable = true };
    private Rectangle? previousScissor;

    public TitleCinematicRenderer(GraphicsDevice device)
    {
        this.device = device;
        batch = new(device);
        try
        {
            pixel = new(device, 1, 1);
            pixel.SetData(new[] { Color.White });
        }
        catch { clipping.Dispose(); pixel?.Dispose(); batch.Dispose(); throw; }
    }

    public void DrawCar(CinematicPlayer player, CinematicCoordinates coordinates, Rectangle? clip = null)
    {
        var pose = player.Pose;
        if (!pose.Visible) return;
        float scale = pose.Scale * (float)coordinates.View.Scale;
        Vector2 Project(float x, float z, float height) =>
            coordinates.ProjectCarPoint(new(x, height, z), pose, player.Definition.GroundAnchor);
        var rearLeft = Project(-12, 18, 7);
        var rearRight = Project(12, 18, 7);
        var frontLeft = Project(-12, -18, 7);
        var frontRight = Project(12, -18, 7);
        var roofRearLeft = Project(-10, 9, 25);
        var roofRearRight = Project(10, 9, 25);
        var roofFrontLeft = Project(-10, -9, 25);
        var roofFrontRight = Project(10, -9, 25);
        Begin(clip);
        Line(rearLeft, rearRight, Color.OrangeRed, 3);
        Line(frontLeft, frontRight, Color.LightGoldenrodYellow, 3);
        Line(rearLeft, frontLeft, Color.LightCyan, 2);
        Line(rearRight, frontRight, Color.LightCyan, 2);
        Line(roofRearLeft, roofRearRight, Color.White, 2);
        Line(roofFrontLeft, roofFrontRight, Color.White, 2);
        Line(roofRearLeft, roofFrontLeft, Color.White, 2);
        Line(roofRearRight, roofFrontRight, Color.White, 2);
        Line(rearLeft, roofRearLeft, Color.LightCyan, 2);
        Line(rearRight, roofRearRight, Color.LightCyan, 2);
        Line(frontLeft, roofFrontLeft, Color.LightCyan, 2);
        Line(frontRight, roofFrontRight, Color.LightCyan, 2);
        for (int x = -12; x <= 12; x += 24)
            for (int z = -12; z <= 12; z += 24)
                Line(Project(x, z - 3, 3), Project(x, z + 3, 3), Color.DarkSlateGray, Math.Max(2, 5 * scale));
        End();
    }

    public void Begin(Rectangle? clip = null)
    {
        if (clip is { } bounds)
        {
            previousScissor = device.ScissorRectangle;
            device.ScissorRectangle = bounds;
        }
        batch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: clip is null ? null : clipping);
    }
    public void End()
    {
        batch.End();
        if (previousScissor is { } previous) device.ScissorRectangle = previous;
        previousScissor = null;
    }
    public void Line(Vector2 a, Vector2 b, Color color, float thickness = 2)
    {
        var delta = b - a;
        batch.Draw(pixel, a, null, color, MathF.Atan2(delta.Y, delta.X), Vector2.Zero,
            new Vector2(delta.Length(), thickness), SpriteEffects.None, 0);
    }
    public void Handle(Vector2 position, Color color, int radius = 5) =>
        batch.Draw(pixel, new Rectangle((int)position.X - radius, (int)position.Y - radius, radius * 2, radius * 2), color);

    public void Dispose() { clipping.Dispose(); pixel.Dispose(); batch.Dispose(); }
}
