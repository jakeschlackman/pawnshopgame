using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using PawnShop.Characters.Npcs.Common.Generation;
using PawnShop.Framework.Rendering;

namespace PawnShop.Characters.Npcs.Common;

// A graphics-free catalog of the approved rig. Files are validated once at load.
// Asset paths remain relative IDs; Game owns packaging and texture resources.
public sealed class CommonNpcCatalog
{
    private readonly FrozenDictionary<string, BodyData> bodies;
    private readonly FrozenDictionary<string, ItemData> items;
    public int Version { get; }
    public int WalkFrameDurationMs => 100;
    public IReadOnlyList<string> BodyIds { get; }

    private CommonNpcCatalog(CatalogData data)
    {
        Version = data.ContentVersion;
        bodies = data.Bodies.ToFrozenDictionary(b => b.Id, StringComparer.Ordinal);
        items = data.Items.ToFrozenDictionary(i => i.Id, StringComparer.Ordinal);
        BodyIds = Array.AsReadOnly(bodies.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    public static CommonNpcCatalog Load(string assetsDirectory)
    {
        string file = Path.Combine(assetsDirectory, "catalog.json");
        try
        {
            string json = File.ReadAllText(file);
            using var document = JsonDocument.Parse(json);
            RejectDuplicateProperties(document.RootElement, file);
            var data = JsonSerializer.Deserialize<CatalogData>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                RespectRequiredConstructorParameters = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            Require(data is not null, file, "Expected a catalog.");
            if (data is null) throw new InvalidDataException(file);
            Require(data.Version == 1 && data.ContentVersion > 0 && data.Rig == "human_48x64_v1" && data.FrameWidth == 48
                && data.FrameHeight == 64 && data.PivotX == 24 && data.PivotY == 58
                && data.WalkFrameDurationMs == 100, file, "Unsupported version or rig contract.");
            Require(data.Textures is { Count: > 0 } && data.Bodies is { Count: > 0 }, file, "Textures and bodies are required.");
            var textures = new Dictionary<string, TextureData>(StringComparer.Ordinal);
            foreach (var texture in data.Textures!)
            {
                Require(texture is not null, file, "Null texture.");
                if (texture is null) continue;
                string path = texture.Path;
                Require(!string.IsNullOrWhiteSpace(path) && !path.Contains('\\') && !path.Contains(':')
                    && !Path.IsPathRooted(path) && path.Split('/').All(p => p is not ("" or "." or ".."))
                    && path.EndsWith(".png", StringComparison.Ordinal), file, "Use a relative PNG path inside Assets.");
                Require(textures.TryAdd(path, texture), file, $"Duplicate texture '{path}'.");
                Require(texture.Columns is >= 1 and <= 16, file + " " + path, "Invalid columns.");
                Require(texture.Sha256 is { Length: 64 } && texture.Sha256.All(Uri.IsHexDigit), file + " " + path, "Invalid SHA256.");
                string absolute = Path.Combine(assetsDirectory, path);
                var sheet = SpriteSheetDefinition.Parse(JsonSerializer.Serialize(new
                {
                    version = 1, frameWidth = 48, frameHeight = 64, columns = texture.Columns, rows = 1,
                    pivotX = 24, pivotY = 58, displayScale = 1,
                    animations = new { All = new { firstFrame = 0, frameCount = texture.Columns, frameDurationMs = 100, loop = true } }
                }), file + " " + path);
                sheet.ValidatePng(absolute, file);
                using var stream = File.OpenRead(absolute);
                Require(Convert.ToHexString(SHA256.HashData(stream)).Equals(texture.Sha256, StringComparison.OrdinalIgnoreCase),
                    file + " " + path, "Asset hash differs from the approved catalog.");
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var body in data.Bodies!)
            {
                Require(body is not null, file, "Null body.");
                if (body is null) continue;
                Require(!string.IsNullOrWhiteSpace(body.Id) && body.Id.All(c => char.IsAsciiLetterOrDigit(c) || c == '_')
                    && ids.Add(body.Id), file, "Body IDs must be unique nonempty identifiers.");
                string field = file + " bodies." + body.Id;
                Require(body.Parts is { Count: > 0 } && body.Parts.All(p => p is not null && Identifier(p.Id))
                    && body.Parts.Select(p => p.Id).Distinct(StringComparer.Ordinal).Count() == body.Parts.Count
                    && body.Parts.Any(p => p.Id == "body"), field, "Requires unique part IDs and a body part.");
                foreach (var part in body.Parts!)
                {
                    Require(part.Views is { Count: 3 } && part.Views.All(v => v is not null)
                        && part.Views.Select(v => v.Direction).Order().SequenceEqual(new[] { "east", "north", "south" }),
                        field + "." + part.Id, "Requires South, North and East exactly once; West mirrors East.");
                    foreach (var view in part.Views!)
                    {
                        void Clip(ClipData? clip, int count, string name)
                        {
                            Require(clip is not null && clip.Texture is not null && textures.ContainsKey(clip.Texture), field + "." + part.Id + "." + name, "Missing clip/texture.");
                            if (clip is null) return;
                            Require(clip.FirstFrame >= 0 && clip.FrameCount == count
                                && (long)clip.FirstFrame + count <= textures[clip.Texture].Columns,
                                field + "." + part.Id + "." + name, "Invalid frame range.");
                        }
                        Clip(view.Standing, 1, view.Direction + ".standing");
                        Clip(view.Walking, 8, view.Direction + ".walking");
                        if (view.Direction == "east") Clip(view.Settling, 1, "east.settling");
                        else Require(view.Settling is null, field, "Only side settling is approved.");
                    }
                }
            }
            Require(data.Items is { Count: > 0 } && data.Items.All(i => i is not null && Identifier(i.Id))
                && data.Items.Select(i => i.Id).Distinct(StringComparer.Ordinal).Count() == data.Items.Count,
                file, "Requires unique item IDs.");
            foreach (var item in data.Items)
            {
                string field = file + " items." + item.Id;
                Require(item.Slot is "torso" or "legs" or "feet" or "hair" or "headgear", field, "Unknown appearance slot.");
                Require(item.Fits is { Count: > 0 } && item.Fits.All(f => f is not null && f.BodyId is not null)
                    && item.Fits.Select(f => f.BodyId).Distinct(StringComparer.Ordinal).Count() == item.Fits.Count,
                    field, "Requires unique body fits.");
                foreach (var fit in item.Fits)
                {
                    var body = data.Bodies.SingleOrDefault(b => b.Id == fit.BodyId);
                    Require(body is not null, field, $"Unknown body '{fit.BodyId}'.");
                    bool HasPart(string? id) => id != "body" && body.Parts.Any(p => p.Id == id);
                    Require(HasPart(fit.PartId), field, $"Missing fitted part '{fit.PartId}' for '{fit.BodyId}'.");
                    Require(fit.HeadgearParts is not null && fit.BodyOcclusion is not null, field, "Compatibility and occlusion must be specified, even when empty.");
                    Require(item.Slot == "hair" || fit.HeadgearParts.Count == 0, field, "Only hair declares headgear-compatible variants.");
                    foreach (var (headgear, part) in fit.HeadgearParts)
                        Require(HasPart(part) && data.Items.Any(i => i.Id == headgear && i.Slot == "headgear"
                            && i.Fits is not null && i.Fits.Any(f => f is not null && f.BodyId == fit.BodyId)),
                            field, $"Invalid hair compatibility for headgear '{headgear}'.");
                    Require(item.Slot == "headgear" || fit.BodyOcclusion.Count == 0, field, "Only headgear declares body occlusion.");
                    Require(fit.BodyOcclusion.All(o => o is not null)
                        && fit.BodyOcclusion.Select(o => o.Direction).Distinct(StringComparer.Ordinal).Count() == fit.BodyOcclusion.Count,
                        field, "Duplicate/null occlusion direction.");
                    foreach (var occlusion in fit.BodyOcclusion)
                    {
                        Require(occlusion.Direction is "south" or "north" or "east"
                            && occlusion.Pixels is not null && occlusion.WalkOffsets is { Count: 8 }, field, "Invalid occlusion data.");
                        Require(occlusion.Pixels.All(p => p is not null && p.X is >= 0 and < 48 && p.Y is >= 0 and < 64
                            && occlusion.WalkOffsets.All(y => (long)p.Y + y is >= 0 and < 64)), field, "Occlusion leaves the native body canvas.");
                    }
                }
            }
            foreach (var body in data.Bodies)
                foreach (string slot in new[] { "torso", "legs", "feet" })
                    Require(data.Items.Any(i => i.Slot == slot && i.Fits.Any(f => f.BodyId == body.Id)),
                        file + " bodies." + body.Id, $"No compatible {slot} item; cannot generate a clothed shopper.");
            return new(data);
        }
        catch (JsonException e) { throw new InvalidDataException($"{file}: {e.Path}: {e.Message}", e); }
        catch (IOException e) { throw new InvalidDataException($"{file}: Cannot load common NPC assets: {e.Message}", e); }
    }

    public CommonNpcFrame Resolve(CommonNpcAppearance appearance, CommonNpcFacing facing, CommonNpcPose pose, int walkFrame = 0)
    {
        ArgumentNullException.ThrowIfNull(appearance);
        if (!Enum.IsDefined(facing) || !Enum.IsDefined(pose) || walkFrame is < 0 or > 7
            || (pose != CommonNpcPose.Walking && walkFrame != 0)) throw new ArgumentOutOfRangeException(nameof(walkFrame), "Invalid direction, pose or frame.");
        if (pose == CommonNpcPose.Settling && facing is not (CommonNpcFacing.East or CommonNpcFacing.West))
            throw new ArgumentException("Settling is only authored for East and mirrored West.", nameof(pose));
        if (appearance.BodyId is null || !bodies.TryGetValue(appearance.BodyId, out var body))
            throw new ArgumentException("Appearance contains unknown or incompatible common-NPC parts.", nameof(appearance));
        FitData Fit(string? id, string slot)
        {
            if (id is null || !items.TryGetValue(id, out var item) || item.Slot != slot
                || item.Fits.SingleOrDefault(f => f.BodyId == body.Id) is not { } fit)
                throw new ArgumentException($"Unknown or incompatible {slot} item '{id}' for '{body.Id}'.", nameof(appearance));
            return fit;
        }
        string direction = facing == CommonNpcFacing.West ? "east" : facing.ToString().ToLowerInvariant();
        var partIds = new List<string> { "body", Fit(appearance.FeetId, "feet").PartId,
            Fit(appearance.LegsId, "legs").PartId, Fit(appearance.TorsoId, "torso").PartId };
        var headgearFit = appearance.HeadgearId is null ? null : Fit(appearance.HeadgearId, "headgear");
        if (appearance.HairId is not null)
        {
            var hairFit = Fit(appearance.HairId, "hair");
            if (appearance.HeadgearId is null) partIds.Add(hairFit.PartId);
            else if (hairFit.HeadgearParts.TryGetValue(appearance.HeadgearId, out string? part)) partIds.Add(part);
            else throw new ArgumentException("Hair has no approved fit with this headgear.", nameof(appearance));
        }
        if (headgearFit is not null) partIds.Add(headgearFit.PartId);
        var layers = new List<CommonNpcLayer>();
        foreach (string id in partIds)
        {
            var view = body.Parts.Single(p => p.Id == id).Views.Single(v => v.Direction == direction);
            var clip = pose switch { CommonNpcPose.Standing => view.Standing, CommonNpcPose.Walking => view.Walking, _ => view.Settling! };
            int frame = clip.FirstFrame + (pose == CommonNpcPose.Walking ? walkFrame : 0);
            layers.Add(new(id, clip.Texture, frame * 48, 0, 48, 64));
        }
        var hidden = new List<CommonNpcPixel>();
        var occlusion = headgearFit?.BodyOcclusion.SingleOrDefault(o => o.Direction == direction);
        if (occlusion is not null)
        {
            int offset = pose == CommonNpcPose.Walking ? occlusion.WalkOffsets[walkFrame] : 0;
            hidden.AddRange(occlusion.Pixels.Select(p => new CommonNpcPixel(p.X, p.Y + offset)));
        }
        return new(layers.AsReadOnly(), hidden.AsReadOnly(), facing == CommonNpcFacing.West, 48, 64, 24, 58);
    }

    // Sorted stable IDs keep selection independent of JSON authoring order. A missing
    // hair/headgear compatibility entry excludes that combination rather than clipping blindly.
    public IReadOnlyList<string> Choices(string bodyId, string slot, string? headgearId = null)
    {
        if (bodyId is null || !bodies.ContainsKey(bodyId)) throw new ArgumentException("Unknown body.", nameof(bodyId));
        if (slot is not ("torso" or "legs" or "feet" or "hair" or "headgear")) throw new ArgumentException("Unknown slot.", nameof(slot));
        if (headgearId is not null && (slot != "hair" || !items.TryGetValue(headgearId, out var headgear)
            || headgear.Slot != "headgear" || !headgear.Fits.Any(f => f.BodyId == bodyId)))
            throw new ArgumentException("Headgear filtering requires a compatible headgear ID and the hair slot.", nameof(headgearId));
        return Array.AsReadOnly(items.Values.Where(i => i.Slot == slot && i.Fits.Any(f => f.BodyId == bodyId
            && (headgearId is null || f.HeadgearParts.ContainsKey(headgearId))))
            .Select(i => i.Id).Order(StringComparer.Ordinal).ToArray());
    }

    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value)
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');

    private static void Require([DoesNotReturnIf(false)] bool valid, string source, string message)
    {
        if (!valid) throw new InvalidDataException($"{source}: {message}");
    }

    private static void RejectDuplicateProperties(JsonElement element, string field)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                Require(names.Add(property.Name), field, $"Duplicate property '{property.Name}'.");
                RejectDuplicateProperties(property.Value, field + "." + property.Name);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray()) RejectDuplicateProperties(child, field);
    }

    private sealed record CatalogData(int Version, int ContentVersion, string Rig, int FrameWidth, int FrameHeight, int PivotX, int PivotY,
        int WalkFrameDurationMs, List<TextureData> Textures, List<BodyData> Bodies, List<ItemData> Items);
    private sealed record TextureData(string Path, string Sha256, int Columns);
    private sealed record BodyData(string Id, List<PartData> Parts);
    private sealed record ItemData(string Id, string Slot, List<FitData> Fits);
    private sealed record FitData(string BodyId, string PartId, Dictionary<string, string> HeadgearParts, List<OcclusionData> BodyOcclusion);
    private sealed record OcclusionData(string Direction, List<CommonNpcPixel> Pixels, List<int> WalkOffsets);
    private sealed record PartData(string Id, List<ViewData> Views);
    private sealed record ViewData(string Direction, ClipData Standing, ClipData Walking, ClipData? Settling);
    private sealed record ClipData(string Texture, int FirstFrame, int FrameCount);
}
