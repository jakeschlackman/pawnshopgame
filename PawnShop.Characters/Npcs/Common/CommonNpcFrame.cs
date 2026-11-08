namespace PawnShop.Characters.Npcs.Common;

public enum CommonNpcFacing { South, North, East, West }
public enum CommonNpcPose { Standing, Walking, Settling }
public sealed record CommonNpcPixel(int X, int Y);
public sealed record CommonNpcLayer(string PartId, string Texture, int X, int Y, int Width, int Height);

// Layers share a pose and one whole-character mirror. Hidden pixels apply only to
// the body before clothing is drawn, in native character coordinates before mirroring.
// The rendering owner advances the clock; resolving a frame never advances state.
public sealed record CommonNpcFrame(IReadOnlyList<CommonNpcLayer> Layers,
    IReadOnlyList<CommonNpcPixel> HiddenBodyPixels, bool MirrorHorizontally,
    int Width, int Height, int PivotX, int PivotY);
