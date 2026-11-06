using System.Collections.Frozen;
using System.Text.Json;
using Ink.Runtime;

namespace PawnShop.Narrative.Dialogue;

public sealed record DialogueSpeaker(string Id, string Name);

// Compiled Ink is immutable application content. Each opening gets a private Story.
public sealed class DialogueDefinition
{
    private readonly string json;
    public string Id { get; }
    public string Source { get; }
    public IReadOnlyDictionary<string, DialogueSpeaker> Speakers { get; }

    public DialogueDefinition(string id, string source, string compiledJson, IEnumerable<DialogueSpeaker> speakers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        Id = id;
        Source = source;
        json = compiledJson;
        try { Speakers = speakers.ToFrozenDictionary(s => s.Id, StringComparer.Ordinal); }
        catch (ArgumentException e) { throw Invalid("speakers", e.Message); }
        if (Speakers.Count == 0 || Speakers.Values.Any(s => string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Name)))
            throw Invalid("speakers", "At least one named speaker with a stable ID is required.");
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object || !document.RootElement.TryGetProperty("inkVersion", out _))
                throw Invalid("inkVersion", "Expected compiled Ink JSON, exported from Inky.");
            _ = CreateStory();
        }
        catch (JsonException e) { throw Invalid("json", e.Message); }
    }

    internal Story CreateStory()
    {
        try
        {
            var story = new Story(json) { allowExternalFunctionFallbacks = false };
            // This first slice exposes no gameplay callbacks or arbitrary external functions.
            story.ValidateExternalBindings();
            return story;
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            throw Invalid("runtime", e.Message);
        }
    }

    internal InvalidDataException Invalid(string field, string reason) => new($"{Source} [{Id}] {field}: {reason}");
}
