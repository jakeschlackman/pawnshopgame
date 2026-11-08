using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PawnShop.Characters.Npcs;

public sealed record NpcSpriteReference(string Texture, string Metadata, string IdleAnimation);
public sealed record NpcInteractionBounds(int X, int Y, int Width, int Height);

// Immutable authored character identity/capabilities. No dialogue engine, texture,
// world position, clock, or mutable relationship/quest state belongs in this definition.
public sealed record NpcDefinition
{
    [JsonPropertyName("$schema")] public string? Schema { get; init; }
    public required int Version { get; init; }
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required NpcSpriteReference Sprite { get; init; }
    public required NpcInteractionBounds InteractionBounds { get; init; }
    public string? Conversations { get; init; }

    public static NpcDefinition Load(string file)
    {
        try
        {
            var data = JsonSerializer.Deserialize<NpcDefinition>(File.ReadAllText(file), new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = false, RespectRequiredConstructorParameters = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            void Require(bool valid, string field, string message)
            {
                if (!valid) throw new InvalidDataException($"{file} {field}: {message}");
            }
            if (data is null) throw new InvalidDataException($"{file}: Expected an NPC definition.");
            Require(data.Version == 1, "version", "Expected version 1.");
            Require(data.Id is not null && Regex.IsMatch(data.Id, "^[a-z][a-z0-9_]*$"), "id", "Use a stable lowercase ID.");
            Require(!string.IsNullOrWhiteSpace(data.Name) && data.Name.Length <= 24, "name", "Use a display name of 1-24 characters.");
            Require(data.Sprite is not null && !string.IsNullOrWhiteSpace(data.Sprite.Texture)
                && !string.IsNullOrWhiteSpace(data.Sprite.Metadata) && !string.IsNullOrWhiteSpace(data.Sprite.IdleAnimation),
                "sprite", "Texture, metadata and idleAnimation references are required.");
            Require(data.InteractionBounds is { Width: > 0 and <= 256, Height: > 0 and <= 256, X: >= -256 and <= 256, Y: >= -256 and <= 256 },
                "interactionBounds", "Use a positive local interaction rectangle with dimensions/offsets within 256 pixels.");
            Require(data.Conversations is null || !string.IsNullOrWhiteSpace(data.Conversations), "conversations", "Use a JSON file reference or null for an NPC without dialogue.");
            return data;
        }
        catch (JsonException e) { throw new InvalidDataException($"{file}: {e.Path}: {e.Message}", e); }
    }
}
