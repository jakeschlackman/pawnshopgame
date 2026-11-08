using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PawnShop.Framework.Rendering;

public readonly record struct FontText(string Value, string FontName);

public static class FontTextExtensions
{
    public static FontText Font(this string text, string name)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new(text, name);
    }
}

public sealed record FontDefinition(string Asset, float Size);

// Named presentation roles only. MonoGame owns font processing, metrics and drawing.
public sealed class FontCatalog
{
    private readonly string source;
    public IReadOnlyDictionary<string, FontDefinition> Fonts { get; }
    private FontCatalog(string source, Dictionary<string, FontDefinition> fonts)
    {
        this.source = source;
        Fonts = new ReadOnlyDictionary<string, FontDefinition>(fonts);
    }

    public FontDefinition Get(string name) => Fonts.TryGetValue(name, out var definition) ? definition
        : throw new InvalidDataException($"{source}: fonts.{name}: unknown font name.");

    public static FontCatalog Load(string path) => Parse(File.ReadAllText(path), path);

    public static FontCatalog Parse(string json, string source = "fonts.json")
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            };
            var data = JsonSerializer.Deserialize<CatalogData>(json, options);
            if (data is null || data.Version != 1) throw new InvalidDataException($"{source}: version: expected 1.");
            if (data.Fonts is null || data.Fonts.Count == 0) throw new InvalidDataException($"{source}: fonts: at least one entry is required.");
            // Dictionary deserialization otherwise silently accepts duplicate JSON names.
            using var document = JsonDocument.Parse(json);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in document.RootElement.GetProperty("fonts").EnumerateObject())
                if (!names.Add(entry.Name)) throw new InvalidDataException($"{source}: fonts.{entry.Name}: duplicate name.");
            foreach (var (name, font) in data.Fonts)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new InvalidDataException($"{source}: fonts: names cannot be empty.");
                if (font is null || string.IsNullOrWhiteSpace(font.Asset) || Path.IsPathRooted(font.Asset)
                    || font.Asset.IndexOfAny(['\\', ':']) >= 0 || Path.HasExtension(font.Asset)
                    || font.Asset.Split('/').Any(segment => segment is "" or "." or ".."))
                    throw new InvalidDataException($"{source}: fonts.{name}.asset: expected a relative MonoGame content ID without an extension.");
                if (!float.IsFinite(font.Size) || font.Size <= 0 || font.Size > 512)
                    throw new InvalidDataException($"{source}: fonts.{name}.size: expected a pixel height greater than 0 and at most 512.");
            }
            return new(source, new Dictionary<string, FontDefinition>(data.Fonts, StringComparer.Ordinal));
        }
        catch (JsonException e)
        {
            throw new InvalidDataException($"{source}: {e.Path}: {e.Message}", e);
        }
    }

    private sealed record CatalogData(int Version, Dictionary<string, FontDefinition>? Fonts);
}
