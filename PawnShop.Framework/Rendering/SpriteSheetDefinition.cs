using System.Collections.ObjectModel;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace PawnShop.Framework.Rendering;

public sealed record SpriteClip(int FirstFrame, int FrameCount, int FrameDurationMs, bool Loop)
{
    public TimeSpan Duration => TimeSpan.FromMilliseconds((long)FrameCount * FrameDurationMs);
}

public sealed class SpriteSheetDefinition
{
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public int ImageWidth => Columns * FrameWidth;
    public int ImageHeight => Rows * FrameHeight;
    public Vector2 Pivot { get; }
    public float DisplayScale { get; }
    public IReadOnlyDictionary<string, SpriteClip> Animations { get; }

    private SpriteSheetDefinition(SheetData data)
    {
        FrameWidth = data.FrameWidth;
        FrameHeight = data.FrameHeight;
        Columns = data.Columns;
        Rows = data.Rows;
        Pivot = new(data.PivotX, data.PivotY);
        DisplayScale = data.DisplayScale;
        Animations = new ReadOnlyDictionary<string, SpriteClip>(new Dictionary<string, SpriteClip>(data.Animations!, StringComparer.Ordinal));
    }

    public Rectangle GetFrame(int index)
    {
        if (index < 0 || index >= Columns * Rows) throw new ArgumentOutOfRangeException(nameof(index));
        return new(index % Columns * FrameWidth, index / Columns * FrameHeight, FrameWidth, FrameHeight);
    }

    public static SpriteSheetDefinition Load(string path) => Parse(File.ReadAllText(path), path);

    // Header-only validation shared by NPC and player content; no graphics device is needed.
    public void ValidatePng(string path, string metadataPath)
    {
        Span<byte> header = stackalloc byte[24];
        try { using var stream = File.OpenRead(path); stream.ReadExactly(header); }
        catch (IOException e) { throw new InvalidDataException($"{path}: Cannot read PNG dimensions.", e); }
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (!header[..8].SequenceEqual(signature) || !header.Slice(12, 4).SequenceEqual("IHDR"u8))
            throw new InvalidDataException($"{path}: Expected a PNG image.");
        int width = BinaryPrimitives.ReadInt32BigEndian(header.Slice(16, 4));
        int height = BinaryPrimitives.ReadInt32BigEndian(header.Slice(20, 4));
        if (width != ImageWidth || height != ImageHeight)
            throw new InvalidDataException($"{path}: PNG is {width}x{height}; {metadataPath} requires {ImageWidth}x{ImageHeight}.");
    }

    public static SpriteSheetDefinition Parse(string json, string source = "sprite-sheet.json")
    {
        try
        {
            var data = JsonSerializer.Deserialize<SheetData>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            if (data is null || data.Version != 1) throw new InvalidDataException($"{source}: version: expected 1.");
            if (data.FrameWidth <= 0 || data.FrameHeight <= 0 || data.Columns <= 0 || data.Rows <= 0
                || (long)data.FrameWidth * data.Columns > 16384 || (long)data.FrameHeight * data.Rows > 16384)
                throw new InvalidDataException($"{source}: frameWidth/frameHeight/columns/rows: expected positive dimensions within a 16384-pixel sheet.");
            if (data.PivotX < 0 || data.PivotX > data.FrameWidth || data.PivotY < 0 || data.PivotY > data.FrameHeight
                || !float.IsFinite(data.DisplayScale) || data.DisplayScale <= 0 || data.DisplayScale > 16)
                throw new InvalidDataException($"{source}: pivot/displayScale: invalid anchor or scale.");
            if (data.Animations is null || data.Animations.Count == 0)
                throw new InvalidDataException($"{source}: animations: at least one clip is required.");
            using var document = JsonDocument.Parse(json);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in document.RootElement.GetProperty("animations").EnumerateObject())
                if (!names.Add(entry.Name)) throw new InvalidDataException($"{source}: animations.{entry.Name}: duplicate name.");
            foreach (var (name, clip) in data.Animations)
                if (string.IsNullOrWhiteSpace(name) || clip is null || clip.FirstFrame < 0 || clip.FrameCount <= 0
                    || (long)clip.FirstFrame + clip.FrameCount > (long)data.Columns * data.Rows
                    || clip.FrameDurationMs <= 0 || clip.FrameDurationMs > 60000)
                    throw new InvalidDataException($"{source}: animations.{name}: invalid frame range or frameDurationMs.");
            return new(data);
        }
        catch (JsonException e) { throw new InvalidDataException($"{source}: {e.Path}: {e.Message}", e); }
    }

    private sealed record SheetData(int Version, int FrameWidth, int FrameHeight, int Columns, int Rows,
        int PivotX, int PivotY, float DisplayScale, Dictionary<string, SpriteClip>? Animations);
}
