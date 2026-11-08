using System.Text.Json;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private sealed class DialogueFixture : IDisposable
    {
        public string Directory { get; } = Path.Combine(Path.GetTempPath(), "PawnShopDialogue-" + Guid.NewGuid());
        private readonly List<string> entries = [];
        private readonly List<object> spawns = [];
        public string Placements => Path.Combine(Directory, "shop.npcs.json");
        public DialogueFixture() => System.IO.Directory.CreateDirectory(Directory);
        public NpcContentCatalog Load() => NpcContentCatalog.Load(Directory, Placements);
        public string Write(string name, string text)
        {
            string path = Path.Combine(Directory, name);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }
        public string Add(string id, string name, int x, string? source = null)
        {
            source ??= $"Hello from {name}. # line: {id}.greeting # speaker: {id}\n-> END";
            string path = Write(id + "/dialogue/greeting.ink", source);
            Write(id + "/dialogue/greeting.ink.json", InkContent.Compile(source, path));
            string manifest = Write(id + "/npc.json", JsonSerializer.Serialize(new
            {
                version = 1, id, name,
                sprite = new { texture = "sprites/body.png", metadata = "sprites/body.json", idleAnimation = "Idle" },
                interactionBounds = new { x = -18, y = -40, width = 36, height = 58 },
                conversations = "conversations.json"
            }));
            Write(id + "/conversations.json", "{\"version\":1,\"default\":\"greeting\",\"entries\":{\"greeting\":\"dialogue/greeting.ink\"}}");
            string built = Path.Combine(AppContext.BaseDirectory, "Content", "Npcs", "visitor", "sprites");
            Write(id + "/sprites/body.json", File.ReadAllText(Path.Combine(built, "body.json")));
            File.Copy(Path.Combine(built, "body.png"), Path.Combine(Directory, id, "sprites", "body.png"));
            foreach (string entry in new[] { "npc.json", "conversations.json", "dialogue/greeting.ink", "sprites/body.json", "sprites/body.png" }) entries.Add(id + "/" + entry);
            Write("content.index", string.Join('\n', entries));
            spawns.Add(new { npcId = id, x, y = 230 });
            Write("shop.npcs.json", JsonSerializer.Serialize(new { version = 1, spawns }));
            return manifest;
        }
        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }

    internal static void NpcConversations()
    {
        using var fixture = new DialogueFixture();
        fixture.Add("first", "First NPC", 180);
        fixture.Add("second", "Second NPC", 450);
        var catalog = fixture.Load();
        using var app = new ApplicationFlow(conversations: catalog);
        Check(app.TryTalkTo("first") == TalkResult.NoSession);
        Begin(app);
        Approach(app, "second");
        var before = app.Room;
        Check(app.TryTalkTo("missing") == TalkResult.UnknownNpc && app.Room == before && app.Screen == Screen.Room);
        var click = MenuPointerInput.HitTest(app, 450, 210, 640, 360);
        Check(click.TalkTargetId == "second");
        app.Update(click, TimeSpan.FromSeconds(1));
        Check(app.Dialogue!.View.Speaker.Id == "second" && app.Dialogue.View.Text.Contains("Second NPC") && app.Room == before);
        Check(app.TryTalkTo("first") == TalkResult.Busy);
        Back(app);
        Check(app.Screen == Screen.Room && app.SelectedNpcId == "second");
        app.Update(new(NextTab: true), TimeSpan.Zero);
        Check(app.Dialogue!.View.Speaker.Id == "second");
        Back(app);
        Approach(app, "first");
        app.Update(new(NextTab: true), TimeSpan.Zero);
        Check(app.Dialogue!.View.Speaker.Id == "first");

        // An NPC without a dialogue binding remains valid and interactable.
        string second = Path.Combine(fixture.Directory, "second", "npc.json");
        File.WriteAllText(second, File.ReadAllText(second).Replace("\"conversations\":\"conversations.json\"", "\"conversations\":null"));
        using var quiet = new ApplicationFlow(conversations: fixture.Load());
        Begin(quiet);
        Check(quiet.TryTalkTo("second") == TalkResult.NoConversation && quiet.Screen == Screen.Room);
        Check(quiet.TryTalkTo("first", "missing") == TalkResult.UnknownConversation);
        Approach(quiet, "first");
        Check(quiet.TryTalkTo("first", "greeting") == TalkResult.Started);

        fixture.Write("shop.npcs.json", "{\"version\":1,\"spawns\":[]}");
        using var empty = new ApplicationFlow(conversations: fixture.Load());
        Begin(empty);
        empty.Update(new(Right: true, NextTab: true), TimeSpan.Zero);
        Check(empty.SelectedNpcId is null && empty.TryTalkTo("first") == TalkResult.UnknownNpc);
    }

    internal static void NpcContentValidation()
    {
        using var fixture = new DialogueFixture();
        string path = fixture.Add("visitor", "Visitor", 400);
        string valid = File.ReadAllText(path);
        foreach (string invalid in new[]
        {
            "null", "{broken", "{}", valid.Replace("\"version\":1", "\"version\":2"),
            valid.Replace("\"id\":\"visitor\"", "\"id\":null"),
            valid.Replace("sprites/body.png", "../outside.png"), valid.Replace("sprites/body.png", "missing.png"),
            valid.Replace("\"width\":36", "\"width\":0"), valid.Replace("\"name\":\"Visitor\"", "\"unknown\":true")
        })
        {
            File.WriteAllText(path, invalid);
            try { fixture.Load(); throw new Exception("Expected NPC error."); }
            catch (InvalidDataException e) { Check(e.Message.Contains("npc.json")); }
        }
        File.WriteAllText(path, valid);
        string binding = Path.Combine(fixture.Directory, "visitor", "conversations.json");
        string goodBinding = File.ReadAllText(binding);
        File.WriteAllText(binding, goodBinding.Replace("\"default\":\"greeting\"", "\"default\":\"missing\""));
        try { fixture.Load(); throw new Exception("Expected binding error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("conversations.json")); }
        File.WriteAllText(binding, goodBinding);
        string metadata = Path.Combine(fixture.Directory, "visitor", "sprites", "body.json");
        string goodMetadata = File.ReadAllText(metadata);
        File.WriteAllText(metadata, goodMetadata.Replace("\"frameWidth\": 24", "\"frameWidth\": 25"));
        try { fixture.Load(); throw new Exception("Expected dimensions error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("body.png")); }
        File.WriteAllText(metadata, goodMetadata);
        fixture.Write("content.index", "visitor/npc.json");
        try { fixture.Load(); throw new Exception("Expected removed asset error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("Missing indexed asset")); }
    }

    internal static void NpcAssetsAndBindings()
    {
        using var fixture = new DialogueFixture();
        fixture.Add("visitor", "Visitor", 400);
        string binding = Path.Combine(fixture.Directory, "visitor", "conversations.json");
        const string alternate = "A different greeting. # line: alternate # speaker: visitor\n-> END";
        string source = fixture.Write("visitor/dialogue/alternate.ink", alternate);
        fixture.Write("visitor/dialogue/alternate.ink.json", InkContent.Compile(alternate, source));
        File.AppendAllText(Path.Combine(fixture.Directory, "content.index"), "\nvisitor/dialogue/alternate.ink");
        fixture.Write("visitor/conversations.json", "{\"version\":1,\"default\":\"alternate\",\"entries\":{\"greeting\":\"dialogue/greeting.ink\",\"alternate\":\"dialogue/alternate.ink\"}}");
        // Reuse the 24x40 image as two 24x20 frames to exercise animation ownership headlessly.
        fixture.Write("visitor/sprites/body.json", "{\"version\":1,\"frameWidth\":24,\"frameHeight\":20,\"columns\":1,\"rows\":2,\"pivotX\":12,\"pivotY\":20,\"displayScale\":1,\"animations\":{\"Idle\":{\"firstFrame\":0,\"frameCount\":2,\"frameDurationMs\":100,\"loop\":true}}}");
        using var app = new ApplicationFlow(conversations: fixture.Load());
        Begin(app);
        Approach(app, "visitor");
        app.Update(default, TimeSpan.FromMilliseconds(100));
        Check(app.NpcFrames["visitor"] == 1);
        Check(app.TryTalkTo("visitor") == TalkResult.Started && app.Dialogue!.View.Text == "A different greeting.");
        app.Update(default, TimeSpan.FromMilliseconds(100));
        Check(app.NpcFrames["visitor"] == 1);
        Back(app);
        Check(app.TryTalkTo("visitor", "greeting") == TalkResult.Started && app.Dialogue!.View.Text.Contains("Hello from"));
        Back(app);
        app.Update(default, TimeSpan.FromMilliseconds(100));
        Check(app.NpcFrames["visitor"] == 0);
        Back(app); // Pause freezes NPC animation too.
        app.Update(default, TimeSpan.FromMilliseconds(100));
        Check(app.NpcFrames["visitor"] == 0);

        string validBinding = File.ReadAllText(binding);
        File.WriteAllText(binding, validBinding.Replace("\"greeting\":\"dialogue/greeting.ink\"", "\"alternate\":\"dialogue/greeting.ink\""));
        try { fixture.Load(); throw new Exception("Expected duplicate conversation error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("Duplicate conversation ID")); }
        File.WriteAllText(binding, validBinding);
        fixture.Write("shop.npcs.json", "{\"version\":1,\"spawns\":[{\"npcId\":\"missing\",\"x\":400,\"y\":230}]}");
        try { fixture.Load(); throw new Exception("Expected unknown placement error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("Unknown NPC")); }
        // Current source index, rather than leftover output files, determines which NPCs exist.
        fixture.Write("shop.npcs.json", "{\"version\":1,\"spawns\":[]}");
        fixture.Write("content.index", "");
        Check(fixture.Load().All.Count == 0);
    }

    internal static void AuthoringCommands()
    {
        using var fixture = new DialogueFixture();
        fixture.Add("visitor", "Visitor", 400,
            "Hello. # line: hi # speaker: visitor\n+ [Leave # id: leave]\n    Bye. # line: bye # speaker: visitor\n    -> END");
        var catalog = fixture.Load();
        using var output = new StringWriter();
        Check(DialogueAuthoring.Run(["list"], new StringReader(""), output, catalog) == 0);
        Check(output.ToString().Contains("PawnShop.Game/Content/Npcs/visitor/dialogue/greeting.ink") && !output.ToString().Contains("bin/"));
        output.GetStringBuilder().Clear();
        Check(DialogueAuthoring.Run(["check"], new StringReader(""), output, catalog) == 0);
        Check(output.ToString().Contains("PASS visitor"));
        output.GetStringBuilder().Clear();
        Check(DialogueAuthoring.Run(["play", "visitor"], new StringReader("99\n1\n\n"), output, catalog) == 0);
        Check(output.ToString().Contains("numbered responses") && output.ToString().Contains("Bye."));
        Check(DialogueAuthoring.Run(["play", "missing"], new StringReader(""), output, catalog) == 1);

        var invalid = InkContent.Definition("Hello. # line: hi # speaker: visitor\n+ [Good # id: good]\n    -> END\n+ [Bad # id: bad]\n    Missing metadata.\n    -> END");
        try { DialogueContentValidator.Check(invalid); throw new Exception("Expected alternate-branch error."); }
        catch (InvalidDataException e) { Check(e.Message.Contains("line")); }

        // Build output must include every registered source and only current bindings.
        var built = NpcContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content", "Npcs"), Path.Combine(AppContext.BaseDirectory, "Content", "World", "shop.npcs.json"));
        foreach (var npc in built.All)
            foreach (var conversation in npc.Conversations.Values) Check(DialogueContentValidator.Check(conversation.Dialogue) > 0);
    }
}
