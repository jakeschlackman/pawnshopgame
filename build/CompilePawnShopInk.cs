using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

// Built by MSBuild's Roslyn task factory; deliberately not a ninth solution project.
public sealed class CompilePawnShopInk : Task
{
    [Required] public ITaskItem[] Sources { get; set; }
    [Required] public ITaskItem[] Npcs { get; set; }
    [Required] public string OutputRoot { get; set; }
    [Required] public string InkDirectory { get; set; }
    [Output] public ITaskItem[] CompiledFiles { get; set; }

    public override bool Execute()
    {
        var outputs = new List<ITaskItem>();
        var index = new List<string>();
        foreach (var source in Sources) index.Add((source.GetMetadata("RecursiveDir") + Path.GetFileName(source.ItemSpec)).Replace('\\', '/'));
        foreach (var npc in Npcs) index.Add((npc.GetMetadata("RecursiveDir") + Path.GetFileName(npc.ItemSpec)).Replace('\\', '/'));
        string indexPath = Path.GetFullPath(Path.Combine(OutputRoot, "content.index"));
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath));
        string indexText = string.Join("\n", index);
        if (!File.Exists(indexPath) || File.ReadAllText(indexPath) != indexText) File.WriteAllText(indexPath, indexText);
        var indexItem = new TaskItem(indexPath);
        indexItem.SetMetadata("Link", "Content/Npcs/content.index");
        indexItem.SetMetadata("TargetPath", "Content/Npcs/content.index");
        outputs.Add(indexItem);
        // Resolve both pinned assemblies in the same load context. Roslyn inline task
        // references otherwise load Ink's shared delegate types in different contexts.
        // Only these fixed compiler APIs are invoked; content never supplies CLR names.
        Assembly.LoadFrom(Path.Combine(InkDirectory, "ink-engine-runtime.dll"));
        var compilerType = Assembly.LoadFrom(Path.Combine(InkDirectory, "ink_compiler.dll")).GetType("Ink.Compiler", true);
        var optionsType = compilerType.GetNestedType("Options");
        foreach (var source in Sources)
        {
            string path = source.GetMetadata("FullPath");
            string relative = source.GetMetadata("RecursiveDir") + Path.GetFileName(path) + ".json";
            string output = Path.GetFullPath(Path.Combine(OutputRoot, relative));
            var item = new TaskItem(output);
            item.SetMetadata("Link", "Content/Npcs/" + relative);
            item.SetMetadata("TargetPath", "Content/Npcs/" + relative);
            outputs.Add(item);
            try
            {
                var options = Activator.CreateInstance(optionsType);
                optionsType.GetField("sourceFilename").SetValue(options, path);
                var compiler = Activator.CreateInstance(compilerType, File.ReadAllText(path), options);
                var story = compilerType.GetMethod("Compile").Invoke(compiler, null);
                if (story == null) throw new InvalidDataException("Ink compilation failed.");
                var storyType = story.GetType();
                storyType.GetProperty("allowExternalFunctionFallbacks").SetValue(story, false);
                storyType.GetMethod("ValidateExternalBindings").Invoke(story, null);
                string json = (string)storyType.GetMethod("ToJson", Type.EmptyTypes).Invoke(story, null);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output) || File.ReadAllText(output) != json) File.WriteAllText(output, json);
                Log.LogMessage(MessageImportance.Low, "Ink: {0}", path);
            }
            catch (Exception e)
            {
                while (e is TargetInvocationException && e.InnerException != null) e = e.InnerException;
                var match = Regex.Match(e.Message, @"line (\d+)");
                int line = match.Success ? int.Parse(match.Groups[1].Value) : 1;
                Log.LogError(null, "INK001", null, path, line, 1, 0, 0, e.Message);
            }
        }
        CompiledFiles = outputs.ToArray();
        return !Log.HasLoggedErrors;
    }
}
