using PawnShop.Framework.Rendering;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void FontCatalogValidation()
    {
        const string valid = """
            {"version":1,"fonts":{
              "TitleMenu":{"asset":"Fonts/PixelifySans-Bold","size":64},
              "SmallText":{"asset":"Fonts/PixelifySans-Bold","size":22}
            }}
            """;
        var catalog = FontCatalog.Parse(valid, "test-fonts.json");
        Check(catalog.Get("TitleMenu").Size == 64 && catalog.Get("SmallText").Size == 22);
        var text = "New Game".Font("TitleMenu");
        Check(text.Value == "New Game" && text.FontName == "TitleMenu");
        try { catalog.Get("MissingRole"); throw new Exception("Expected unknown font rejection"); }
        catch (InvalidDataException e) { Check(e.Message.Contains("test-fonts.json") && e.Message.Contains("MissingRole")); }
        foreach (string invalid in new[]
        {
            "{broken", "null", "{}", "{\"version\":2,\"fonts\":{}}", "{\"version\":1,\"fonts\":null}",
            valid.Replace("64", "0"), valid.Replace("64", "513"), valid.Replace("64", "-1"),
            valid.Replace("Fonts/PixelifySans-Bold", "../outside"),
            valid.Replace("Fonts/PixelifySans-Bold", "/absolute"),
            valid.Replace("Fonts/PixelifySans-Bold", "Fonts/file.xnb"),
            valid.Replace("Fonts/PixelifySans-Bold", "Fonts//file"),
            valid.Replace("\"SmallText\"", "\"TitleMenu\""),
            valid.Replace("\"size\":64", "\"unknown\":64")
        })
        {
            try { FontCatalog.Parse(invalid, "invalid-fonts.json"); throw new Exception("Expected invalid font catalog rejection"); }
            catch (InvalidDataException e) { Check(e.Message.Contains("invalid-fonts.json")); }
        }
    }

    internal static void FontContent()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Content", "Fonts");
        var catalog = FontCatalog.Load(Path.Combine(directory, "fonts.json"));
        foreach (string role in new[] { "PixelifySansBold", "Silkscreen", "TitleMenu", "TitleBadge", "TitleHelp", "TitleSaying",
            "OptionsHeading", "OptionsLabel", "OptionsDropdownLabel", "OptionsBody", "OptionsDropdown", "OptionsHelp", "OptionsTab", "OptionsAction" })
        {
            var font = catalog.Get(role);
            string expectedAsset = role switch
            {
                "Silkscreen" => "Fonts/Silkscreen-Regular",
                "TitleMenu" or "TitleSaying" or "OptionsHeading" or "OptionsLabel" or "OptionsDropdownLabel" or "OptionsBody"
                    or "OptionsDropdown" or "OptionsHelp" => "Fonts/PixeloidSans-Bold",
                "OptionsTab" or "OptionsAction" => "Fonts/Jersey25-Regular",
                _ => "Fonts/PixelifySans-Bold"
            };
            Check(font.Asset == expectedAsset && font.Size > 0);
            string asset = Path.Combine(AppContext.BaseDirectory, "Content", font.Asset + ".xnb");
            byte[] bytes = File.ReadAllBytes(asset);
            Check(bytes.Length > 10 && bytes[0] == 'X' && bytes[1] == 'N' && bytes[2] == 'B', "Missing compiled MonoGame font.");
        }
        Check(File.ReadAllText(Path.Combine(directory, "OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "Pixeloid-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "Silkscreen-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "AtkinsonHyperlegible-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "Jersey10-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "GeistPixel-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "PressStart2P-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "Jersey25-OFL.txt")).Contains("SIL OPEN FONT LICENSE"));
        Check(File.ReadAllText(Path.Combine(directory, "UpheavalPro-License.txt")).Contains("Commercial use"));
        Check(File.ReadAllBytes(Path.Combine(directory, "Phatone-License.pdf")).AsSpan(0, 4)
            .SequenceEqual("%PDF"u8));
        Check(File.ReadAllText(Path.Combine(directory, "NotJamSlabSerif11-CC0.txt")).Contains("CC0"));
    }
}
