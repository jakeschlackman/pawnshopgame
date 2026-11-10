#if DEBUG
using Microsoft.Xna.Framework;
using PawnShop.Game.Bootstrap.Cinematics;

namespace PawnShop.Game.Screens.CinematicEditor;

// Temporary authoring state. Every edit replaces validated data; drag is one undo.
public sealed class CinematicEditSession
{
    public const int FieldCount = 11, PitchRow = 7, VisibilityRow = 8;
    private readonly List<CinematicDefinition> undo = new();
    public CinematicPlayer Player { get; private set; }
    public int SelectedPoint { get; set; }
    public int SelectedKey { get; private set; }
    public string Status { get; set; } = "DRAFT - align the path to the road before approval.";

    public CinematicEditSession(CinematicDefinition? definition = null) => Player = new(definition ?? Draft());

    public static CinematicDefinition Draft() => new()
    {
        Version = 1, BackgroundAsset = CinematicDefinition.Background, ImageWidth = 2106, ImageHeight = 747,
        CarAsset = "placeholder-wire-car", GroundAnchor = new(32, 40), DurationSeconds = 12, Speed = 1,
        Points = new CinematicPoint[] { new(850, 540), new(800, 500), new(690, 470), new(760, 420), new(650, 370) },
        Keys = new CinematicKey[] { new(0, 2, 45, true), new(.85f, .25f, 20, true), new(1, .05f, 0, false) }
    };

    public void NewDraft()
    {
        Remember();
        Replace(Draft());
        Player.Restart();
        SelectedPoint = SelectedKey = 0;
        Status = "New onscreen draft. Space previews scaling. Ctrl S exports. Ctrl Z restores previous work.";
    }

    public void Remember()
    {
        if (undo.Count == 64) undo.RemoveAt(0);
        undo.Add(Player.Definition);
    }

    private void Replace(CinematicDefinition data)
    {
        var replacement = new CinematicPlayer(data);
        replacement.Seek(Player.Progress);
        replacement.Playing = Player.Playing;
        Player = replacement;
        SelectedPoint = Math.Clamp(SelectedPoint, 0, data.Points.Count - 1);
        SelectedKey = Math.Clamp(SelectedKey, 0, data.Keys.Count - 1);
    }

    public void Undo()
    {
        if (undo.Count == 0) return;
        var data = undo[^1];
        undo.RemoveAt(undo.Count - 1);
        Replace(data);
        Status = "Undid last edit.";
    }

    public int HitPoint(Vector2 imagePoint, float imageRadius)
    {
        for (int i = Player.Definition.Points.Count - 1; i >= 0; i--)
        {
            var p = Player.Definition.Points[i];
            if (Vector2.Distance(imagePoint, new(p.X, p.Y)) <= imageRadius) return i;
        }
        return -1;
    }

    public void AddPoint(Vector2 p)
    {
        if (Player.Definition.Points.Count == 512) return;
        Remember();
        Replace(Player.Definition with { Points = Player.Definition.Points.Append(new CinematicPoint(p.X, p.Y)).ToArray() });
        SelectedPoint = Player.Definition.Points.Count - 1;
    }

    public void MovePoint(Vector2 p)
    {
        var points = Player.Definition.Points.ToArray();
        if (points[SelectedPoint] == new CinematicPoint(p.X, p.Y)) return;
        points[SelectedPoint] = new(p.X, p.Y);
        // Crossing another handle is legal, but collapsing the entire path is not.
        if (points.All(point => point == points[0])) return;
        Replace(Player.Definition with { Points = points });
    }

    public void DeletePoint(int index)
    {
        if (Player.Definition.Points.Count <= 2 || index < 0) { Status = "Keep at least two path points."; return; }
        var points = Player.Definition.Points.Where((_, i) => i != index).ToArray();
        if (points.All(point => point == points[0])) return;
        Remember();
        Replace(Player.Definition with { Points = points });
    }

    public void SelectKey(int delta)
    {
        SelectedKey = (SelectedKey + delta + Player.Definition.Keys.Count) % Player.Definition.Keys.Count;
        Player.Playing = false;
        Player.Seek(Player.Definition.Keys[SelectedKey].Progress);
    }

    public void AddKey()
    {
        var data = Player.Definition;
        if (data.Keys.Count == 512) return;
        float progress = (float)Player.Progress;
        int existing = data.Keys.ToList().FindIndex(k => Math.Abs(k.Progress - progress) < .001);
        if (existing >= 0) { SelectedKey = existing; Status = "Selected existing nearby key."; return; }
        var pose = Player.Pose;
        Remember();
        var keys = data.Keys.Append(new(progress, pose.Scale, pose.OrientationDegrees, pose.Visible, pose.PitchDegrees)).OrderBy(k => k.Progress).ToArray();
        Replace(data with { Keys = keys });
        SelectedKey = Array.FindIndex(keys, k => k.Progress == progress);
    }

    public void DeleteKey()
    {
        if (SelectedKey == 0 || SelectedKey == Player.Definition.Keys.Count - 1) { Status = "Endpoint keys are required."; return; }
        Remember();
        Replace(Player.Definition with { Keys = Player.Definition.Keys.Where((_, i) => i != SelectedKey).ToArray() });
        Player.Seek(Player.Definition.Keys[SelectedKey].Progress);
    }

    // Rows match the tiny overlay. Left/right or +/- buttons adjust one value.
    public void Adjust(int row, int direction, bool fine, bool remember = true)
    {
        var data = Player.Definition;
        var point = data.Points[SelectedPoint];
        var keys = data.Keys.ToArray();
        var key = keys[SelectedKey];
        float step = direction * (fine ? .1f : 1);
        if (remember) Remember();
        switch (row)
        {
            case 0: MovePoint(new(point.X + step * 10, point.Y)); return;
            case 1: MovePoint(new(point.X, point.Y + step * 10)); return;
            case 2: data = data with { DurationSeconds = Math.Clamp(data.DurationSeconds + step, .1, 3600) }; break;
            case 3: data = data with { Speed = Math.Clamp(data.Speed + step * .1, .05, 8) }; break;
            case 4:
                if (SelectedKey > 0 && SelectedKey < keys.Length - 1)
                {
                    float minimum = MathF.BitIncrement(keys[SelectedKey - 1].Progress);
                    float maximum = MathF.BitDecrement(keys[SelectedKey + 1].Progress);
                    if (minimum <= maximum) key = key with { Progress = Math.Clamp(key.Progress + step * .01f, minimum, maximum) };
                }
                break;
            case 5: key = key with { Scale = Math.Clamp(key.Scale + step * .05f, CinematicDefinition.MinimumScale, CinematicDefinition.MaximumScale) }; break;
            case 6: key = key with { OrientationDegrees = Math.Clamp(key.OrientationDegrees + step * 5, -360, 360) }; break;
            case PitchRow: key = key with { PitchDegrees = Math.Clamp(key.PitchDegrees + step, -90, 90) }; break;
            case VisibilityRow: key = key with { Visible = !key.Visible }; break;
            case 9: data = data with { GroundAnchor = data.GroundAnchor with { X = Math.Clamp(data.GroundAnchor.X + step, 0, 64) } }; break;
            case 10: data = data with { GroundAnchor = data.GroundAnchor with { Y = Math.Clamp(data.GroundAnchor.Y + step, 0, 48) } }; break;
        }
        keys[SelectedKey] = key;
        Replace(data with { Keys = keys });
        if (row is >= 4 and <= VisibilityRow) { Player.Playing = false; Player.Seek(key.Progress); }
    }

    public void Load(string path)
    {
        var data = CinematicDefinition.Load(path); // Failed load preserves document and preview.
        Remember();
        Replace(data);
        Player.Restart();
        SelectedKey = 0;
        Status = "Loaded cinematic JSON.";
    }
}
#endif
