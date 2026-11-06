using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using PawnShop.Characters.Npcs;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Game.Bootstrap;

public sealed record NpcConversation(string Id, string SourceFile, string SourceRelativePath, DialogueDefinition Dialogue);
public sealed record NpcContent(NpcDefinition Definition, NpcSpriteAsset Sprite, string? DefaultConversation,
    IReadOnlyDictionary<string, NpcConversation> Conversations)
{
    public string Id => Definition.Id;
    public string Name => Definition.Name;
}
public sealed record PlacedNpc(NpcContent Content, int X, int Y)
{
    public string Id => Content.Id;
    public string Name => Content.Name;
}
public enum TalkResult { Started, NoSession, Busy, UnknownNpc, NoConversation, UnknownConversation }

// Composition boundary: Characters owns definitions, Narrative owns conversations,
// Framework owns sprite metadata, and Game resolves their content references.
public sealed class NpcContentCatalog
{
    private readonly FrozenDictionary<string, NpcContent> byId;
    public IReadOnlyList<NpcContent> All { get; }
    public IReadOnlyList<PlacedNpc> ShopNpcs { get; }

    private NpcContentCatalog(IEnumerable<NpcContent> npcs, string placementFile)
    {
        All = Array.AsReadOnly(npcs.ToArray());
        byId = All.ToFrozenDictionary(n => n.Id, StringComparer.Ordinal);
        var placements = Read<PlacementFile>(placementFile);
        if (placements.Version != 1 || placements.Spawns is null)
            throw new InvalidDataException($"{placementFile}: Expected version 1 and a spawns array.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var placed = new List<PlacedNpc>();
        foreach (var spawn in placements.Spawns)
        {
            if (spawn is null || spawn.NpcId is null || !byId.TryGetValue(spawn.NpcId, out var npc))
                throw new InvalidDataException($"{placementFile} npcId: Unknown NPC '{spawn?.NpcId}'.");
            if (!seen.Add(spawn.NpcId)) throw new InvalidDataException($"{placementFile} npcId: Duplicate placement '{spawn.NpcId}'.");
            if (spawn.X is < 80 or > 560 || spawn.Y is < 170 or > 270)
                throw new InvalidDataException($"{placementFile} [{spawn.NpcId}] position: Room foot position must be x=80..560, y=170..270.");
            placed.Add(new(npc, spawn.X, spawn.Y));
        }
        ShopNpcs = placed.AsReadOnly();
    }

    public bool TryGet(string id, [NotNullWhen(true)] out NpcContent? npc) => byId.TryGetValue(id, out npc);

    public static NpcContentCatalog Load(string directory, string placementFile)
    {
        directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        string indexFile = Path.Combine(directory, "content.index");
        if (!File.Exists(indexFile)) throw new InvalidDataException($"{indexFile}: Missing content index. Build the Game project first.");
        var content = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string entry in File.ReadAllLines(indexFile).Where(e => e.Length > 0))
        {
            string fullPath = ResolvePath(directory, entry, indexFile);
            if (!content.Add(fullPath)) throw new InvalidDataException($"{indexFile}: Duplicate content path '{entry}'.");
        }
        string Referenced(string folder, string reference, string field)
        {
            string path = ResolvePath(folder, reference, field);
            if (!content.Contains(path) || !File.Exists(path)) throw new InvalidDataException($"{field}: Missing indexed asset '{reference}'.");
            return path;
        }
        var definitions = new List<(string Folder, string File, NpcDefinition Definition)>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string file in content.Where(f => Path.GetFileName(f) == "npc.json").Order(StringComparer.Ordinal))
        {
            var definition = NpcDefinition.Load(file);
            if (!ids.Add(definition.Id)) throw new InvalidDataException($"{file} id: Duplicate NPC ID '{definition.Id}'.");
            string folder = Path.GetDirectoryName(file)!;
            if (Path.GetDirectoryName(folder) != directory || Path.GetFileName(folder) != definition.Id)
                throw new InvalidDataException($"{file} id: Put this definition in Npcs/{definition.Id}/npc.json.");
            definitions.Add((folder, file, definition));
        }
        var speakers = definitions.Select(d => new DialogueSpeaker(d.Definition.Id, d.Definition.Name)).ToArray();
        var npcs = new List<NpcContent>();
        foreach (var (folder, file, definition) in definitions)
        {
            var sprite = NpcSpriteAsset.Load(Referenced(folder, definition.Sprite.Texture, file + " sprite.texture"),
                Referenced(folder, definition.Sprite.Metadata, file + " sprite.metadata"), definition.Sprite.IdleAnimation);
            string? defaultConversation = null;
            var conversations = new Dictionary<string, NpcConversation>(StringComparer.Ordinal);
            if (definition.Conversations is { } reference)
            {
                string bindingFile = Referenced(folder, reference, file + " conversations");
                var bindings = ConversationBindings.Load(bindingFile);
                defaultConversation = bindings.Default;
                foreach (var (id, sourceReference) in bindings.Entries)
                {
                    string source = Referenced(folder, sourceReference, bindingFile + " entries." + id);
                    if (!File.Exists(source + ".json")) throw new InvalidDataException($"{source}: Missing compiled Ink. Build the Game project first.");
                    conversations.Add(id, new(id, source, Path.GetRelativePath(directory, source).Replace('\\', '/'),
                        new DialogueDefinition($"dialogue.{definition.Id}.{id}", source, File.ReadAllText(source + ".json"), speakers)));
                }
            }
            npcs.Add(new(definition, sprite, defaultConversation, conversations.ToFrozenDictionary(StringComparer.Ordinal)));
        }
        return new(npcs, placementFile);
    }

    private static string ResolvePath(string folder, string reference, string field)
    {
        if (string.IsNullOrWhiteSpace(reference) || reference.Contains('\\') || reference.Contains(':') || Path.IsPathRooted(reference)
            || reference.Split('/').Any(part => part is "" or "." or ".."))
            throw new InvalidDataException($"{field}: Use a relative path inside the NPC folder, with forward slashes and no '..'.");
        string path = Path.GetFullPath(Path.Combine(folder, reference));
        if (!path.StartsWith(Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"{field}: Path leaves its content folder.");
        return path;
    }

    private static T Read<T>(string file)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(file), new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = false, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            }) ?? throw new InvalidDataException($"{file}: Expected a JSON object.");
        }
        catch (JsonException e) { throw new InvalidDataException($"{file}: {e.Path}: {e.Message}", e); }
    }
    private sealed record PlacementFile
    {
        [JsonPropertyName("$schema")] public string? Schema { get; init; }
        public required int Version { get; init; }
        public required Spawn[] Spawns { get; init; }
    }
    private sealed record Spawn
    {
        public required string NpcId { get; init; }
        public required int X { get; init; }
        public required int Y { get; init; }
    }
}
