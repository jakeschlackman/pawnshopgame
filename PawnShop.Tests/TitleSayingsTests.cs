using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void TitleSayingsContent()
    {
        const string json = """
            {"version":1,"sayings":["First saying.","Second saying.","Third saying."]}
            """;
        var first = TitleSayings.Parse(json, "test-sayings.json", new Random(17));
        var second = TitleSayings.Parse(json, "test-sayings.json", new Random(17));
        string? previous = null;
        var seen = new HashSet<string>();
        for (int i = 0; i < 30; i++)
        {
            string saying = first.Next();
            Check(saying == second.Next() && saying != previous);
            Check(new[] { "First saying.", "Second saying.", "Third saying." }.Contains(saying));
            seen.Add(saying);
            previous = saying;
        }
        Check(seen.Count == 3);
        var single = TitleSayings.Parse("""{"version":1,"sayings":["Only saying."]}""", "single.json", new Random(0));
        for (int i = 0; i < 10; i++) Check(single.Next() == "Only saying.");
        foreach (string invalid in new[]
        {
            "{broken", "null", "{}", "[]", "{\"version\":2,\"sayings\":[\"Hi\"]}",
            "{\"version\":1,\"sayings\":null}", "{\"version\":1,\"sayings\":[]}",
            "{\"version\":1,\"sayings\":[null]}", "{\"version\":1,\"sayings\":[\" \"]}",
            "{\"version\":1,\"sayings\":[\"Same\",\"Same\"]}",
            "{\"version\":1,\"sayings\":[\"First\\nSecond\"]}",
            "{\"version\":1,\"sayings\":[\"" + new string('W', 81) + "\"]}",
            json.Replace("\"version\":1", "\"version\":1,\"unknown\":true")
        })
        {
            try { TitleSayings.Parse(invalid, "test-sayings.json", new Random(0)); throw new Exception("Expected invalid saying rejection."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("test-sayings.json")); }
        }
        var built = TitleSayings.Load(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "title-sayings.json"), new Random(1));
        Check(!string.IsNullOrWhiteSpace(built.Next()));

        var pair = TitleSayings.Parse("""{"version":1,"sayings":["First saying.","Second saying."]}""", "pair.json", new Random(0));
        var expected = TitleSayings.Parse("""{"version":1,"sayings":["First saying.","Second saying."]}""", "pair.json", new Random(0));
        using var app = new ApplicationFlow(titleSayings: pair);
        string initial = app.TitleSaying;
        Check(initial == expected.Next() && app.Options.SampleText == initial);
        for (int i = 0; i < 10; i++) app.Update(default, TimeSpan.FromSeconds(.1));
        app.Update(new(SelectionIndex: 1), TimeSpan.Zero);
        Check(app.TitleSaying == initial && app.Menu!.Selected.DisabledReason is not null);
        Confirm(app);
        Check(app.TitleSaying == initial && app.Screen == Screen.Title);
        OpenOptions(app); FinishOptionsFlight(app);
        Check(app.TitleSaying == initial && app.Options.SampleText == initial);
        app.Update(new(TabIndex: 2), TimeSpan.Zero);
        app.Update(new(SelectionIndex: 2, Right: true), TimeSpan.Zero);
        Check(app.Options.Draft.Dialogue == DialogueSpeed.Fast && app.Options.TextElapsed == TimeSpan.Zero);
        app.Update(default, TimeSpan.FromSeconds(10));
        Check(app.Options.Draft.RevealedCharacters(app.Options.TextElapsed, app.Options.SampleText.Length) == initial.Length);
        app.Update(new(SelectionIndex: app.Options.FieldCount + 1, Confirm: true), TimeSpan.Zero);
        Check(!app.Options.IsDirty && app.Options.SampleText == initial && app.Options.TextElapsed == TimeSpan.Zero);
        Back(app);
        Check(app.TitleSaying == initial && app.Screen == Screen.Title);
        FinishOptionsFlight(app);
        app.Update(default, TimeSpan.FromSeconds(1));
        Check(app.TitleSaying == initial);
        app.Update(new(SelectionIndex: 0), TimeSpan.Zero);
        Confirm(app);
        Check(app.Screen == Screen.CharacterSelect && app.TitleSaying == initial);
        Back(app);
        Check(app.Screen == Screen.Title && app.TitleSaying == initial && app.LiveSessions == 0);
        Begin(app); ReturnPrompt(app); Discard(app);
        Check(app.Screen == Screen.Title && app.TitleSaying == initial && app.LiveSessions == 0);
        OpenOptions(app); FinishOptionsFlight(app);
        Check(app.Options.SampleText == initial && app.TitleSaying == initial);
        // Navigating and reopening Options must not consume another random selection.
        Check(pair.Next() == expected.Next());
    }
}
