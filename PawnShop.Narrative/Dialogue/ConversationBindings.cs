using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PawnShop.Narrative.Dialogue;

// Conversation identities and source references; independent of NPC sprites and placement.
public sealed class ConversationBindings
{
    public string Default { get; }
    public IReadOnlyDictionary<string, string> Entries { get; }
    private ConversationBindings(string defaultId, Dictionary<string, string> entries)
    {
        Default = defaultId;
        Entries = entries.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public static ConversationBindings Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<BindingFile>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = false, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            if (data is null || data.Version != 1 || data.Entries is null || data.Entries.Count == 0)
                throw new InvalidDataException($"{path}: Expected version 1 and at least one conversation entry.");
            using var document = JsonDocument.Parse(json);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in document.RootElement.GetProperty("entries").EnumerateObject())
                if (!ids.Add(entry.Name)) throw new InvalidDataException($"{path} entries.{entry.Name}: Duplicate conversation ID.");
            foreach (var entry in data.Entries)
                if (!Regex.IsMatch(entry.Key, "^[a-z][a-z0-9_]*$") || string.IsNullOrWhiteSpace(entry.Value) || !entry.Value.EndsWith(".ink", StringComparison.Ordinal))
                    throw new InvalidDataException($"{path} entries.{entry.Key}: Expected a stable ID mapped to an .ink source path.");
            if (data.Default is null || !data.Entries.ContainsKey(data.Default))
                throw new InvalidDataException($"{path} default: Must reference one of the entries.");
            return new(data.Default, data.Entries);
        }
        catch (JsonException e) { throw new InvalidDataException($"{path}: {e.Path}: {e.Message}", e); }
    }

    private sealed record BindingFile
    {
        [JsonPropertyName("$schema")] public string? Schema { get; init; }
        public required int Version { get; init; }
        public required string Default { get; init; }
        public required Dictionary<string, string> Entries { get; init; }
    }
}
