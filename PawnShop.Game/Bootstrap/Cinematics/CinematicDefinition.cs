using System.Text.Json;
using System.Text.Json.Serialization;

namespace PawnShop.Game.Bootstrap.Cinematics;

public readonly record struct CinematicPoint([property: JsonRequired] float X, [property: JsonRequired] float Y);
// Optional pitch keeps existing version-1 exports level when the field is absent.
public sealed record CinematicKey([property: JsonRequired] float Progress, [property: JsonRequired] float Scale,
    [property: JsonRequired] float OrientationDegrees, [property: JsonRequired] bool Visible, float PitchDegrees = 0);

// Presentation data only. Progress is normalized time; orientation is authored yaw,
// never inferred from the screen-space path. Visibility holds until the next key.
public sealed record CinematicDefinition
{
    public const string Background = "TitleScreen/title-screen-panorama.png";
    public const float MinimumScale = .01f, MaximumScale = 64;
    public required int Version { get; init; }
    public required string BackgroundAsset { get; init; }
    public required int ImageWidth { get; init; }
    public required int ImageHeight { get; init; }
    public required string CarAsset { get; init; }
    public required CinematicPoint GroundAnchor { get; init; }
    public required double DurationSeconds { get; init; }
    public required double Speed { get; init; }
    public required IReadOnlyList<CinematicPoint> Points { get; init; }
    public required IReadOnlyList<CinematicKey> Keys { get; init; }

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static CinematicDefinition Load(string path) => Parse(File.ReadAllText(path), path);

    public static CinematicDefinition Parse(string json, string source)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CinematicDefinition>(json, Json)
                ?? throw new InvalidDataException($"{source}: empty cinematic.");
            return data.Validate(source);
        }
        catch (JsonException e) { throw new InvalidDataException($"{source}: invalid cinematic JSON: {e.Message}", e); }
    }

    public CinematicDefinition Validate(string source)
    {
        void Require([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool condition, string field)
        {
            if (!condition) throw new InvalidDataException($"{source}: invalid {field}.");
        }
        Require(Version == 1, "version (expected 1)");
        Require(BackgroundAsset == Background && ImageWidth == 2106 && ImageHeight == 747,
            "backgroundAsset/image dimensions (expected the original 2106x747 title panorama)");
        Require(CarAsset == "placeholder-wire-car", "carAsset (Phase 1 supports placeholder-wire-car)");
        Require(double.IsFinite(DurationSeconds) && DurationSeconds is >= .1 and <= 3600, "durationSeconds (.1-3600)");
        Require(double.IsFinite(Speed) && Speed is >= .05 and <= 8, "speed (.05-8)");
        Require(Finite(GroundAnchor) && GroundAnchor.X is >= 0 and <= 64 && GroundAnchor.Y is >= 0 and <= 48,
            "groundAnchor (64x48 placeholder canvas)");
        Require(Points is { Count: >= 2 and <= 512 }, "points (2-512)");
        bool distinct = false;
        for (int i = 0; i < Points.Count; i++)
        {
            Require(Finite(Points[i]) && Math.Abs(Points[i].X) <= 100000 && Math.Abs(Points[i].Y) <= 100000, $"points[{i}]");
            distinct |= Points[i] != Points[0];
        }
        Require(distinct, "points (path must have length)");
        Require(Keys is { Count: >= 2 and <= 512 }, "keys (2-512)");
        for (int i = 0; i < Keys.Count; i++)
        {
            var key = Keys[i];
            Require(key is not null, $"keys[{i}]");
            Require(float.IsFinite(key.Progress) && key.Progress is >= 0 and <= 1 && (i == 0 || key.Progress > Keys[i - 1].Progress), $"keys[{i}].progress (strictly increasing)");
            Require(float.IsFinite(key.Scale) && key.Scale is >= MinimumScale and <= MaximumScale, $"keys[{i}].scale (.01-64)");
            Require(float.IsFinite(key.OrientationDegrees) && key.OrientationDegrees is >= -360 and <= 360, $"keys[{i}].orientationDegrees (-360-360)");
            Require(float.IsFinite(key.PitchDegrees) && key.PitchDegrees is >= -90 and <= 90, $"keys[{i}].pitchDegrees (-90-90)");
        }
        Require(Keys[0].Progress == 0 && Keys[^1].Progress == 1, "keys (endpoints at 0 and 1 required)");
        // Own immutable snapshots even when the caller supplied mutable arrays/lists.
        return this with { Points = Array.AsReadOnly(Points.ToArray()), Keys = Array.AsReadOnly(Keys.ToArray()) };
    }

    private static bool Finite(CinematicPoint p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    public string ToJson() => JsonSerializer.Serialize(this, Json);

    public void Save(string path)
    {
        var valid = Validate(path);
        // Refuse to replace an unreadable/unsupported file; keep it recoverable.
        if (File.Exists(path)) _ = Load(path);
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, valid.ToJson());
        if (Load(temporary).ToJson() != valid.ToJson())
            throw new IOException($"{path}: cinematic round-trip verification failed.");
        if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
        else File.Move(temporary, path);
    }
}
