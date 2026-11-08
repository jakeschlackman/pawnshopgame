using Ink.Runtime;

namespace PawnShop.Narrative.Dialogue;

public enum DialogueResult { Applied, StaleView, UnknownChoice, AdvanceRequired, ChoiceRequired, Ended }
public enum DialogueEndReason { Completed, Cancelled, SessionDisposed, ParticipantUnavailable }
public sealed record DialogueChoice(string Id, string Text);
public sealed record DialogueView(Guid SessionId, int Revision, string LineId, DialogueSpeaker Speaker,
    string Emotion, string Text, IReadOnlyList<DialogueChoice> Choices, bool CanContinue, bool Ended);

// Owns all Ink state. Callers cannot mutate variables or choose by Ink's transient index.
// Ink variables in this slice are conversation-local, never inventory/quest authority.
public sealed class DialogueSession : IDisposable
{
    private readonly DialogueDefinition definition;
    private readonly Story story;
    private readonly Guid id = Guid.NewGuid();
    private int revision;
    public DialogueView View { get; private set; }
    public DialogueEndReason? EndReason { get; private set; }

    public DialogueSession(DialogueDefinition definition)
    {
        this.definition = definition;
        story = definition.CreateStory();
        View = new(id, 0, "", definition.Speakers.Values.First(), "neutral", "", Array.Empty<DialogueChoice>(), false, false);
        ReadNext();
    }

    // Ephemeral authoring validation only. This is not a gameplay save/load contract.
    internal DialogueSession ForkForValidation()
    {
        var copy = new DialogueSession(definition);
        copy.story.state.LoadJson(story.state.ToJson());
        copy.revision = revision;
        copy.EndReason = EndReason;
        copy.View = View with { SessionId = copy.id };
        return copy;
    }

    internal string ValidationKey()
    {
        using var state = System.Text.Json.JsonDocument.Parse(story.state.ToJson());
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            View.LineId, View.Text, View.Speaker, View.Emotion, View.Choices, View.CanContinue,
            Variables = state.RootElement.GetProperty("variablesState")
        });
    }

    public DialogueResult TryAdvance(Guid sessionId, int expectedRevision)
    {
        var result = Check(sessionId, expectedRevision);
        if (result != DialogueResult.Applied) return result;
        if (View.Choices.Count != 0) return DialogueResult.ChoiceRequired;
        if (story.canContinue) ReadNext();
        else End(DialogueEndReason.Completed);
        return DialogueResult.Applied;
    }

    public DialogueResult TryChoose(Guid sessionId, string choiceId, int expectedRevision)
    {
        var result = Check(sessionId, expectedRevision);
        if (result != DialogueResult.Applied) return result;
        if (story.canContinue) return DialogueResult.AdvanceRequired;
        // Resolve against the current runtime choices immediately before selecting.
        var choice = story.currentChoices.SingleOrDefault(c => ChoiceId(c) == choiceId);
        if (choice is null) return DialogueResult.UnknownChoice;
        story.ChooseChoiceIndex(choice.index);
        ReadNext();
        return DialogueResult.Applied;
    }

    private DialogueResult Check(Guid sessionId, int expectedRevision) => EndReason is not null
        ? DialogueResult.Ended : sessionId != id || expectedRevision != revision
            ? DialogueResult.StaleView : DialogueResult.Applied;

    private void ReadNext()
    {
        try
        {
            // Bounded blank-line and evaluation work; malformed loops fail with a diagnostic.
            for (int line = 0; line < 128 && story.canContinue; line++)
            {
                story.ContinueAsync(5);
                for (int step = 0; !story.asyncContinueComplete && step < 20; step++) story.ContinueAsync(5);
                if (!story.asyncContinueComplete) throw definition.Invalid("flow", "Evaluation budget exceeded; check for a loop without text.");
                string text = story.currentText.Trim();
                if (text.Length == 0) continue;
                var tags = Tags(story.currentTags, "line", "speaker", "emotion");
                string lineId = Required(tags, "line", "line");
                string speakerId = Required(tags, "speaker", lineId);
                if (!definition.Speakers.TryGetValue(speakerId, out var speaker))
                    throw definition.Invalid($"line {lineId}/speaker", $"Unknown speaker '{speakerId}'.");
                string emotion = tags.GetValueOrDefault("emotion", "neutral");
                if (emotion is not ("neutral" or "pleased" or "worried" or "confident"))
                    throw definition.Invalid($"line {lineId}/emotion", $"Unknown expression '{emotion}'.");
                var choices = story.canContinue ? Array.Empty<DialogueChoice>() : story.currentChoices
                    .Select(c => new DialogueChoice(ChoiceId(c), c.text.Trim())).ToArray();
                if (choices.Any(c => string.IsNullOrWhiteSpace(c.Text)) || choices.Select(c => c.Id).Distinct().Count() != choices.Length)
                    throw definition.Invalid($"line {lineId}/choices", "Choice IDs must be unique and labels nonempty.");
                if (choices.Any(c => c.Text.Length > 80 || c.Text.Contains('\n')))
                    throw definition.Invalid($"line {lineId}/choices", "Prototype response labels must be one paragraph of at most 80 characters.");
                View = new(id, ++revision, lineId, speaker, emotion, text, Array.AsReadOnly(choices), story.canContinue, false);
                return;
            }
            if (story.canContinue) throw definition.Invalid("flow", "Too many blank lines without dialogue.");
            if (story.currentChoices.Count > 0) throw definition.Invalid("flow", "Choices need a preceding tagged line.");
            End(DialogueEndReason.Completed);
        }
        catch (StoryException e) { throw definition.Invalid("flow", e.Message); }
    }

    private string ChoiceId(Choice choice) => Required(Tags(choice.tags, "id"), "id", $"choice '{choice.text}'");

    private Dictionary<string, string> Tags(IEnumerable<string>? tags, params string[] allowed)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var tag in tags ?? [])
        {
            var parts = tag.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !allowed.Contains(parts[0]) || string.IsNullOrWhiteSpace(parts[1]) || !result.TryAdd(parts[0], parts[1]))
                throw definition.Invalid("tags", $"Unknown, empty or duplicate tag '{tag}'.");
        }
        return result;
    }

    private string Required(Dictionary<string, string> tags, string key, string context) => tags.TryGetValue(key, out var value)
        ? value : throw definition.Invalid($"{context}/{key}", "Required stable ID is missing.");

    public void End(DialogueEndReason reason)
    {
        if (EndReason is not null) return;
        EndReason = reason;
        View = View with { Revision = ++revision, Ended = true, CanContinue = false, Choices = Array.Empty<DialogueChoice>() };
    }

    public void Dispose() => End(DialogueEndReason.SessionDisposed);
}
