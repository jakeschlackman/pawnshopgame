using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Characters.Movement;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using Color = Microsoft.Xna.Framework.Color;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void CardinalFacing()
    {
        var actor = new PlayerState(PlayerGender.Female, new(1000, 1000));
        var area = new MovementBounds(0, 0, 2000, 2000);
        void Move(float x, float y, Facing expected)
        {
            actor.Move(new(x, y), TimeSpan.FromMilliseconds(10), area, []);
            Check(actor.View.Facing == expected);
        }
        Move(0, -1, Facing.North);
        Move(1, -1, Facing.East); // New horizontal key wins.
        Move(1, -1, Facing.East); // Holding does not oscillate.
        Move(1, 0, Facing.East);
        Move(1, 1, Facing.South); // New vertical key wins.
        Move(-1, 1, Facing.West); // Horizontal reversal wins.
        Move(0, 0, Facing.West); // Stopping retains facing.
        Move(-1, -1, Facing.West); // Simultaneous keys retain last axis.
        Move(.2f, -1, Facing.North); // Analog dominance is deterministic.
        Move(-1, -.2f, Facing.West);
        Check(Enum.GetValues<Facing>().Length == 4);
    }

    internal static void PlayerAnimation()
    {
        using var app = new ApplicationFlow();
        Begin(app);
        Check(app.PlayerFrame == 0);
        app.Update(new(MoveY: 1), TimeSpan.FromMilliseconds(100));
        Check(app.PlayerFrame == 1 && app.Player!.Value.Moving);
        app.Update(new(MoveX: 1), TimeSpan.FromMilliseconds(50));
        Check(app.PlayerFrame == 1 && app.Player!.Value.Facing == Facing.East);
        app.Update(new(MoveX: -1), TimeSpan.FromMilliseconds(50));
        Check(app.PlayerFrame == 2 && app.Player!.Value.Facing == Facing.West);
        Back(app);
        app.Update(default, TimeSpan.FromSeconds(2));
        Check(app.PlayerFrame == 2 && app.Screen == Screen.Pause);
        Confirm(app);
        Check(app.PlayerFrame == 2);
        app.Update(new(MoveY: 1), TimeSpan.FromMilliseconds(200));
        Check(app.PlayerFrame == 0); // Four phases loop; no direction reset.
        app.Update(new(MoveY: 1), TimeSpan.FromMilliseconds(100));
        Check(app.PlayerFrame == 1);
        app.Update(default, TimeSpan.FromMilliseconds(20));
        Check(app.PlayerFrame == 0 && !app.Player!.Value.Moving);
        app.Update(new(MoveY: 1), TimeSpan.FromSeconds(10));
        app.Update(new(MoveY: 1), TimeSpan.FromMilliseconds(100));
        Check(app.PlayerFrame == 0 && !app.Player!.Value.Moving); // Fully blocked -> idle.
        ReturnPrompt(app); Discard(app);
        Begin(app);
        Check(app.PlayerFrame == 0);
        app.Update(new(MoveX: 1), TimeSpan.FromMilliseconds(1300));
        Check(app.SelectedNpcId == "visitor" && app.PlayerFrame == 1);
        app.Update(new(Interact: true), TimeSpan.FromSeconds(1));
        app.Update(default, TimeSpan.FromSeconds(2));
        Check(app.Screen == Screen.Dialogue && app.PlayerFrame == 1);
        Back(app);
        Check(app.PlayerFrame == 1);
        app.Update(default, TimeSpan.FromSeconds(1), active: false);
        Check(app.PlayerFrame == 1 && app.Screen == Screen.Pause);
    }

    internal static void PlayerLayers()
    {
        var catalog = new PlayerSpriteCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Player"));
        var sheet = catalog.PlaceholderSheet;
        Check(catalog.Frame(Facing.West) == catalog.Frame(Facing.East));
        Check(PlayerSpriteCatalog.Effects(Facing.West) == SpriteEffects.FlipHorizontally);
        foreach (var facing in new[] { Facing.South, Facing.North, Facing.East })
            Check(PlayerSpriteCatalog.Effects(facing) == SpriteEffects.None);
        var maleJacket = PlayerPlaceholderSprites.CreatePixels(PlayerGender.Male, PlayerPlaceholderLayer.Jacket, sheet);
        var femaleJacket = PlayerPlaceholderSprites.CreatePixels(PlayerGender.Female, PlayerPlaceholderLayer.Jacket, sheet);
        Check(!maleJacket.SequenceEqual(femaleJacket), "Body-specific jacket shapes must not silently share one fit.");

        foreach (var gender in Enum.GetValues<PlayerGender>())
        {
            var layers = Enum.GetValues<PlayerPlaceholderLayer>().ToDictionary(layer => layer,
                layer => PlayerPlaceholderSprites.CreatePixels(gender, layer, sheet));
            foreach (var pixels in layers.Values)
            {
                Check(pixels.Length == sheet.ImageWidth * sheet.ImageHeight);
                Check(pixels.All(p => p.A is 0 or 255), "Diagnostic pixel art must have hard alpha edges.");
            }
            foreach (var facing in Enum.GetValues<Facing>())
            for (int phase = 0; phase < 4; phase++)
            {
                var source = PlayerPlaceholderSprites.Frame(sheet, facing, phase);
                Color Pixel(PlayerPlaceholderLayer layer, int x, int y) => layers[layer][(source.Y + y) * sheet.ImageWidth + source.X + x];
                Check(Enumerable.Range(0, 48).Any(x => Pixel(PlayerPlaceholderLayer.Body, x, 57).A == 255));
                Check(Enumerable.Range(0, 48).All(x => Pixel(PlayerPlaceholderLayer.Body, x, 58).A == 0), "Feet anchor drifted.");
                var hand = PlayerPlaceholderSprites.Hand(gender, facing, phase);
                Check(Pixel(PlayerPlaceholderLayer.Body, hand.X, hand.Y).A == 255, "Hand anchor missed the body.");
                Check(Pixel(PlayerPlaceholderLayer.Wrench, hand.X, hand.Y).A == 255, "Tool slipped from the hand.");
                var order = PlayerPlaceholderSprites.DrawOrder(facing).ToArray();
                Check(order.Length == 5 && order.Distinct().Count() == 5);
                int bodyIndex = Array.IndexOf(order, PlayerPlaceholderLayer.Body);
                Check((Array.IndexOf(order, PlayerPlaceholderLayer.Backpack) > bodyIndex) == (facing == Facing.North));
                Check((Array.IndexOf(order, PlayerPlaceholderLayer.Wrench) < bodyIndex) == (facing == Facing.North));
                Check(Array.IndexOf(order, PlayerPlaceholderLayer.Jacket) > bodyIndex);
            }

            // Composite-then-mirror must equal mirror-each-full-canvas-then-composite, including occlusion.
            for (int phase = 0; phase < 4; phase++)
            {
                var east = PlayerPlaceholderSprites.Frame(sheet, Facing.East, phase);
                Check(east == PlayerPlaceholderSprites.Frame(sheet, Facing.West, phase));
                Color[] Composite(Facing facing)
                {
                    var result = new Color[48 * 64];
                    var source = PlayerPlaceholderSprites.Frame(sheet, facing, phase);
                    bool flip = PlayerSpriteCatalog.Effects(facing) == SpriteEffects.FlipHorizontally;
                    foreach (var layer in PlayerPlaceholderSprites.DrawOrder(facing))
                    for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 48; x++)
                    {
                        var pixel = layers[layer][(source.Y + y) * sheet.ImageWidth + source.X + (flip ? 47 - x : x)];
                        if (pixel.A == 0) continue;
                        result[y * 48 + x] = pixel;
                    }
                    return result;
                }
                var eastPixels = Composite(Facing.East);
                var westPixels = Composite(Facing.West);
                for (int y = 0; y < 64; y++)
                for (int x = 0; x < 48; x++) Check(eastPixels[y * 48 + x] == westPixels[y * 48 + 47 - x]);
                var eastBounds = PlayerSpriteCatalog.Bounds(sheet, Facing.East, 100, 200, 2);
                var westBounds = PlayerSpriteCatalog.Bounds(sheet, Facing.West, 100, 200, 2);
                var hand = PlayerPlaceholderSprites.Hand(gender, Facing.East, phase);
                float eastHand = eastBounds.X + (hand.X + .5f) * 2;
                float westHand = westBounds.X + (48 - hand.X - .5f) * 2;
                Check(eastHand + westHand == 200 && eastBounds.Bottom == 212 && westBounds.Bottom == 212);
            }
        }
        // Reflection must also preserve the world anchor with a noncentral source pivot.
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Content", "Player", "placeholder-sprite.json"));
        var offCenter = SpriteSheetDefinition.Parse(json.Replace("\"pivotX\": 24", "\"pivotX\": 20"));
        var e = PlayerSpriteCatalog.Bounds(offCenter, Facing.East, 100, 200);
        var w = PlayerSpriteCatalog.Bounds(offCenter, Facing.West, 100, 200);
        Check(e.X == 80 && w.Right == 120 && e.Y == w.Y);
    }
}
