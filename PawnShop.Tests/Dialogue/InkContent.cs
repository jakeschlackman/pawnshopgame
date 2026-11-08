using Ink;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Tests;

internal static class InkContent
{
    internal static string Compile(string source, string filename)
    {
        var diagnostics = new List<string>();
        var compiler = new Compiler(source, new Compiler.Options
        {
            sourceFilename = filename,
            errorHandler = (message, type) => diagnostics.Add($"{type}: {message}")
        });
        var story = compiler.Compile();
        if (story is null || diagnostics.Count != 0)
            throw new InvalidDataException($"{filename}: {string.Join(Environment.NewLine, diagnostics)}");
        return story.ToJson();
    }

    internal static DialogueDefinition Definition(string source) => new("test", "test.ink",
        Compile(source, "test.ink"), [new DialogueSpeaker("visitor", "Visitor")]);

}
