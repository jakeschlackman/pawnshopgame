using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private static DialogueDefinition Visitor()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Content", "Npcs", "visitor", "dialogue", "greeting.ink.json");
        return new("dialogue.shop_visitor", path, File.ReadAllText(path), [new DialogueSpeaker("visitor", "Shop visitor")]);
    }

    private static void Advance(DialogueSession session)
    {
        var view = session.View;
        Check(session.TryAdvance(view.SessionId, view.Revision) == DialogueResult.Applied);
    }

    private static void Choose(DialogueSession session, string id)
    {
        var view = session.View;
        Check(session.TryChoose(view.SessionId, id, view.Revision) == DialogueResult.Applied);
    }

    internal static void DialogueContent()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Content", "Npcs", "visitor", "dialogue", "greeting.ink");
        string json = InkContent.Compile(File.ReadAllText(path), path);
        Check(json == File.ReadAllText(path + ".json"), "Ink source and compiled JSON differ. Rebuild the Game project.");
        Check(File.Exists(Path.Combine(AppContext.BaseDirectory, "Content", "Licenses", "Ink.txt")));
        using var session = new DialogueSession(Visitor());
        Check(session.View.LineId == "visitor.greeting");
        Advance(session);
        Check(session.View.Choices.Select(c => c.Id).SequenceEqual(new[] { "ask_radio", "ask_shop", "goodbye" }));
        Choose(session, "ask_shop");
        Check(session.View.LineId == "visitor.shop");
        Advance(session);
        Choose(session, "ask_radio");
        Check(session.View.LineId == "visitor.radio");
        Advance(session);
        Check(session.View.LineId == "visitor.no_job");
        Advance(session);
        Check(session.View.Choices.Select(c => c.Id).SequenceEqual(new[] { "ask_shop", "follow_up", "goodbye" }));
        Choose(session, "follow_up");
        Check(session.View.LineId == "visitor.follow_up");
        Advance(session);
        Choose(session, "goodbye");
        Check(!session.View.Ended && session.View.LineId == "visitor.farewell");
        Advance(session);
        Check(session.View.Ended && session.EndReason == DialogueEndReason.Completed);
        using var reopened = new DialogueSession(Visitor());
        Advance(reopened);
        Check(reopened.View.Choices.Any(c => c.Id == "ask_radio"));
    }

    internal static void DialogueCommands()
    {
        using var session = new DialogueSession(Visitor());
        var greeting = session.View;
        Check(session.TryChoose(greeting.SessionId, "ask_radio", greeting.Revision) == DialogueResult.AdvanceRequired);
        Advance(session);
        var choices = session.View;
        Check(session.TryChoose(greeting.SessionId, "ask_radio", greeting.Revision) == DialogueResult.StaleView);
        Check(session.TryChoose(Guid.NewGuid(), "ask_radio", choices.Revision) == DialogueResult.StaleView);
        Check(session.TryChoose(choices.SessionId, "missing", choices.Revision) == DialogueResult.UnknownChoice);
        Check(session.TryAdvance(choices.SessionId, choices.Revision) == DialogueResult.ChoiceRequired);
        Check(session.View == choices);
        Choose(session, "ask_radio");
        Check(session.TryChoose(choices.SessionId, "ask_radio", choices.Revision) == DialogueResult.StaleView);
        session.End(DialogueEndReason.ParticipantUnavailable);
        session.Dispose();
        Check(session.EndReason == DialogueEndReason.ParticipantUnavailable);
        Check(session.TryAdvance(session.View.SessionId, session.View.Revision) == DialogueResult.Ended);
    }

    internal static void DialogueValidation()
    {
        foreach (string source in new[]
        {
            "Hello. # speaker: visitor\n-> END",
            "Hello. # line: hello # speaker: missing\n-> END",
            "Hello. # line: hello # speaker: visitor # effect: grant_money\n-> END",
            "Hello. # line: hello # speaker: visitor # emotion: missing\n-> END",
            "Hello. # line: hello # speaker: visitor\n* [Missing ID]\n -> END",
            "Hello. # line: hello # speaker: visitor\n* [A # id: same]\n -> END\n* [B # id: same]\n -> END"
        })
        {
            try { using var session = new DialogueSession(InkContent.Definition(source)); throw new Exception("Expected content failure."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("test.ink")); }
        }
        try { InkContent.Compile("-> missing_knot", "broken.ink"); throw new Exception("Expected compile error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("broken.ink") && e.Message.Contains("missing_knot")); }
        try { _ = new DialogueDefinition("bad", "broken.json", "{}", [new("visitor", "Visitor")]); throw new Exception("Expected JSON error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("inkVersion")); }
        foreach (string invalid in new[] { "null", "[]", "{broken", "{\"inkVersion\":999,\"root\":[]}" })
        {
            try { _ = new DialogueDefinition("bad", "broken.json", invalid, [new("visitor", "Visitor")]); throw new Exception("Expected JSON rejection."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("broken.json")); }
        }
        try
        {
            _ = InkContent.Definition("EXTERNAL grant_money()\n~ grant_money()\nHello. # line: hello # speaker: visitor\n-> END");
            throw new Exception("Expected unbound external rejection.");
        }
        catch (InvalidDataException e) { Check(e.Message.Contains("grant_money")); }
        try
        {
            using var loop = new DialogueSession(InkContent.Definition("-> loop\n=== loop ===\n-> loop"));
            throw new Exception("Expected bounded loop rejection.");
        }
        catch (InvalidDataException e) { Check(e.Message.Contains("budget")); }
    }

    internal static void DialoguePresentation()
    {
        using var session = new DialogueSession(Visitor());
        var presenter = new DialoguePresenter(session, new());
        var initial = session.View;
        presenter.Update(new(Confirm: true), TimeSpan.FromSeconds(100));
        Check(presenter.FullyRevealed && session.View == initial);
        presenter.Update(new(Confirm: true), TimeSpan.Zero);
        Check(session.View.LineId == "visitor.topics");
        presenter.Update(new(Confirm: true), TimeSpan.Zero);
        Check(presenter.ChoicesVisible && session.View.LineId == "visitor.topics");
        presenter.Update(new(Down: true, Confirm: true), TimeSpan.Zero);
        Check(presenter.SelectedIndex == 1 && session.View.LineId == "visitor.topics");
        presenter.Update(new(Confirm: true), TimeSpan.Zero);
        Check(session.View.LineId == "visitor.shop");
        presenter.Update(new(Cancel: true, Confirm: true), TimeSpan.Zero);
        Check(session.EndReason == DialogueEndReason.Cancelled);

        foreach (int scale in new[] { 100, 125, 150 })
        {
            using var longSession = new DialogueSession(InkContent.Definition(new string('x', 900) + " # line: long # speaker: visitor\n-> END"));
            var longPresenter = new DialoguePresenter(longSession, new() { UiScale = scale, Dialogue = DialogueSpeed.Instant });
            int count = 0, pages = longPresenter.PageCount;
            while (!longSession.View.Ended)
            {
                Check(longPresenter.PageText.Split('\n').All(line => line.Length <= longPresenter.Columns));
                count += longPresenter.PageText.Replace("\n", "").Length;
                longPresenter.Update(new(Confirm: true), TimeSpan.Zero);
            }
            Check(pages > 1 && count == 900);
        }
        Check(TextPages.Wrap("one two three", 7).SequenceEqual(new[] { "one two", "three" }));
        // The longest supported response fits two lines even at the largest UI scale.
        Check(TextPages.Wrap(new string('a', 40) + " " + new string('b', 39), 57).Count == 2);
    }

    internal static void DialogueLifecycle()
    {
        using var app = new ApplicationFlow(new() { Dialogue = DialogueSpeed.Instant });
        Begin(app);
        Approach(app, "visitor");
        var room = app.Room;
        app.Update(new(NextTab: true, Interact: true, Confirm: true), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Dialogue && app.Room == room && app.Dialogue!.View.LineId == "visitor.greeting");
        app.Update(new(Confirm: true), TimeSpan.FromSeconds(10), active: false);
        Check(app.Room == room && app.Dialogue!.View.LineId == "visitor.greeting");
        app.Update(new(Confirm: true), TimeSpan.Zero);
        var choice = MenuPointerInput.HitTest(app, 80, 200, 640, 360);
        Check(choice.Confirm && choice.SelectionIndex == 0);
        app.Update(choice, TimeSpan.FromSeconds(10));
        Check(app.Room == room && app.Dialogue!.View.LineId == "visitor.radio");
        app.Update(new(Cancel: true, Interact: true), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Room && app.Dialogue is null && app.Room == room);
        app.Update(new(NextTab: true), TimeSpan.Zero);
        app.RequestExit();
        Check(app.Screen == Screen.ConfirmExit && app.Dialogue is null && app.Room == room);
        Back(app);
        Confirm(app);
        app.Update(new(NextTab: true), TimeSpan.Zero);
        Check(app.Dialogue!.View.LineId == "visitor.greeting");
        var live = app.Dialogue;
        app.Dispose();
        Check(live.View.Ended && app.Dialogue is null && app.LiveSessions == 0);

        using var repeated = new ApplicationFlow();
        for (int i = 0; i < 10; i++)
        {
            Begin(repeated);
            Approach(repeated, "visitor");
            repeated.Update(new(NextTab: true), TimeSpan.Zero);
            Check(repeated.Dialogue!.View.LineId == "visitor.greeting");
            Back(repeated);
            ReturnPrompt(repeated);
            Discard(repeated);
            Check(repeated.LiveSessions == 0 && repeated.Dialogue is null);
        }
    }
}
