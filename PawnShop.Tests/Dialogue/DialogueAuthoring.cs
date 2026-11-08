using PawnShop.Game.Bootstrap;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Tests;

internal static class DialogueAuthoring
{
    internal static int Run(string[] args, TextReader input, TextWriter output, NpcContentCatalog? catalog = null)
    {
        try
        {
            catalog ??= NpcContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content", "Npcs"), Path.Combine(AppContext.BaseDirectory, "Content", "World", "shop.npcs.json"));
            if (args is ["list"])
            {
                foreach (var npc in catalog.All)
                {
                    output.WriteLine($"{npc.Id} | {npc.Name} | PawnShop.Game/Content/Npcs/{npc.Id}/npc.json");
                    foreach (var conversation in npc.Conversations.Values)
                        output.WriteLine($"  {conversation.Id}: PawnShop.Game/Content/Npcs/{conversation.SourceRelativePath}");
                }
                return 0;
            }
            if (args is ["check"])
            {
                foreach (var npc in catalog.All)
                foreach (var conversation in npc.Conversations.Values)
                {
                    string compiled = InkContent.Compile(File.ReadAllText(conversation.SourceFile), conversation.SourceFile);
                    if (compiled != File.ReadAllText(conversation.SourceFile + ".json"))
                        throw new InvalidDataException($"{conversation.SourceFile}: Output is stale. Build before checking.");
                    int states = DialogueContentValidator.Check(conversation.Dialogue);
                    output.WriteLine($"PASS {npc.Id}: {states} dialogue states checked.");
                }
                output.WriteLine("All registered conversations passed the bounded branch check.");
                return 0;
            }
            if (args.Length is 2 or 3 && args[0] == "play")
            {
                string id = args[1];
                if (!catalog.TryGet(id, out var npc)) throw new InvalidDataException($"Unknown NPC '{id}'. Use ./npc.ps1 list to find IDs.");
                string? conversationId = args.Length == 3 ? args[2] : npc.DefaultConversation;
                if (conversationId is null || !npc.Conversations.TryGetValue(conversationId, out var selected))
                    throw new InvalidDataException($"NPC '{id}' has no matching conversation '{conversationId}'.");
                using var session = new DialogueSession(selected.Dialogue);
                output.WriteLine($"Text preview: {npc.Name}. Type q to leave. State resets when this preview ends.");
                while (!session.View.Ended)
                {
                    var view = session.View;
                    output.WriteLine($"\n{view.Speaker.Name}: {view.Text}");
                    for (int i = 0; i < view.Choices.Count; i++) output.WriteLine($"  {i + 1}. {view.Choices[i].Text}");
                    output.Write(view.Choices.Count == 0 ? "Enter: continue | q: quit > " : "Response number | q: quit > ");
                    string? answer = input.ReadLine();
                    if (answer is null || answer.Trim().Equals("q", StringComparison.OrdinalIgnoreCase)) break;
                    if (view.Choices.Count == 0) session.TryAdvance(view.SessionId, view.Revision);
                    else if (int.TryParse(answer, out int index) && index >= 1 && index <= view.Choices.Count)
                        session.TryChoose(view.SessionId, view.Choices[index - 1].Id, view.Revision);
                    else output.WriteLine("Choose one of the numbered responses.");
                }
                output.WriteLine("Conversation ended.");
                return 0;
            }
            output.WriteLine("Usage: ./npc.ps1 list | check | play -Id <npc-id>");
            return 1;
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            string copiedContent = Path.Combine(AppContext.BaseDirectory, "Content", "Npcs") + Path.DirectorySeparatorChar;
            output.WriteLine("ERROR: " + e.Message.Replace(copiedContent, "PawnShop.Game/Content/Npcs/"));
            return 1;
        }
    }
}
