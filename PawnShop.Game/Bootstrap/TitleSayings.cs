using System.Text.Json;
using System.Text.Json.Serialization;

namespace PawnShop.Game.Bootstrap;

// Small presentation-only text pool. The application selects once at startup, never during Draw.
public sealed class TitleSayings
{
    private readonly string[] sayings;
    private readonly Random random;
    private int current = -1;

    private TitleSayings(string[] sayings, Random random)
    {
        this.sayings = sayings;
        this.random = random;
    }

    public static TitleSayings Load(string path, Random random) => Parse(File.ReadAllText(path), path, random);

    public static TitleSayings Parse(string json, string source, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        try
        {
            var data = JsonSerializer.Deserialize<SayingsData>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            if (data is null || data.Version != 1)
                throw new InvalidDataException($"{source}: version must be 1.");
            if (data.Sayings is null || data.Sayings.Length == 0)
                throw new InvalidDataException($"{source}: sayings must contain at least one entry.");
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < data.Sayings.Length; i++)
            {
                string? text = data.Sayings[i]?.Trim();
                if (string.IsNullOrEmpty(text) || text.Length > 80 || text.Any(c => c is < ' ' or > '~'))
                    throw new InvalidDataException($"{source}: sayings[{i}] must be 1-80 printable ASCII characters on one line.");
                if (!unique.Add(text)) throw new InvalidDataException($"{source}: sayings[{i}] duplicates another entry.");
                data.Sayings[i] = text;
            }
            return new(data.Sayings, random);
        }
        catch (JsonException e) { throw new InvalidDataException($"{source}: invalid sayings JSON: {e.Message}", e); }
    }

    public string Next()
    {
        bool skipCurrent = current >= 0 && sayings.Length > 1;
        int next = random.Next(sayings.Length - (skipCurrent ? 1 : 0));
        if (skipCurrent && next >= current) next++;
        current = next;
        return sayings[current];
    }

    private sealed class SayingsData
    {
        public int Version { get; set; }
        public string[]? Sayings { get; set; }
    }
}
