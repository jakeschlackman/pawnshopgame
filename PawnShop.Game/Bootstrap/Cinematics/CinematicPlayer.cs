using Microsoft.Xna.Framework;

namespace PawnShop.Game.Bootstrap.Cinematics;

public readonly record struct CinematicPose(Vector2 GroundPosition, float Scale, float OrientationDegrees, bool Visible, float PitchDegrees = 0);

// Small runtime player, deliberately independent of the temporary editor and GPU.
public sealed class CinematicPlayer
{
    private const int SamplesPerSegment = 64;
    private readonly Vector2[] samples;
    private readonly double[] distances;
    public CinematicDefinition Definition { get; }
    public double Progress { get; private set; }
    public bool Playing { get; set; }
    public double Length => distances[^1];
    public IReadOnlyList<Vector2> Path { get; }
    public CinematicPose Pose => Evaluate(Progress);

    public CinematicPlayer(CinematicDefinition definition)
    {
        Definition = definition.Validate("cinematic");
        samples = new Vector2[(Definition.Points.Count - 1) * SamplesPerSegment + 1];
        distances = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            int segment = Math.Min(i / SamplesPerSegment, Definition.Points.Count - 2);
            float t = (i - segment * SamplesPerSegment) / (float)SamplesPerSegment;
            samples[i] = Interpolate(segment, t);
            if (i > 0) distances[i] = distances[i - 1] + Vector2.Distance(samples[i - 1], samples[i]);
        }
        Path = Array.AsReadOnly(samples);
    }

    private Vector2 Interpolate(int segment, float t)
    {
        Vector2 Point(int i) => new(Definition.Points[i].X, Definition.Points[i].Y);
        Vector2 Tangent(int i)
        {
            if (i == 0) return Point(1) - Point(0);
            if (i == Definition.Points.Count - 1) return Point(i) - Point(i - 1);
            var a = Point(i) - Point(i - 1);
            var b = Point(i + 1) - Point(i);
            // Component-wise monotone Hermite slopes: zero at extrema, harmonic
            // mean elsewhere. Each segment stays inside its endpoint rectangle.
            static float Slope(float a, float b) => a * b <= 0 ? 0 : 2 * a * b / (a + b);
            return new(Slope(a.X, b.X), Slope(a.Y, b.Y));
        }
        return Vector2.Hermite(Point(segment), Tangent(segment), Point(segment + 1), Tangent(segment + 1), t);
    }

    public Vector2 PositionAt(double progress)
    {
        if (!double.IsFinite(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
        double distance = Math.Clamp(progress, 0, 1) * Length;
        int index = Array.BinarySearch(distances, distance);
        if (index >= 0) return samples[index];
        index = ~index;
        if (index <= 0) return samples[0];
        if (index >= samples.Length) return samples[^1];
        double span = distances[index] - distances[index - 1];
        return Vector2.Lerp(samples[index - 1], samples[index], span == 0 ? 0 : (float)((distance - distances[index - 1]) / span));
    }

    public CinematicPose Evaluate(double progress)
    {
        if (!double.IsFinite(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
        progress = Math.Clamp(progress, 0, 1);
        var keys = Definition.Keys;
        int i = 0;
        while (i < keys.Count - 2 && progress >= keys[i + 1].Progress) i++;
        var a = keys[i];
        var b = keys[i + 1];
        float t = (float)((progress - a.Progress) / (b.Progress - a.Progress));
        // Yaw takes the shortest arc. 0 = rear, 45 = rear three-quarter,
        // 90 = side, 180 = front. It is not a screen-space sprite rotation.
        float turn = ((b.OrientationDegrees - a.OrientationDegrees) % 360 + 540) % 360 - 180;
        return new(PositionAt(progress), MathHelper.Lerp(a.Scale, b.Scale, t),
            a.OrientationDegrees + turn * t, progress >= b.Progress ? b.Visible : a.Visible,
            MathHelper.Lerp(a.PitchDegrees, b.PitchDegrees, t));
    }

    public void Seek(double progress)
    {
        if (!double.IsFinite(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
        Progress = Math.Clamp(progress, 0, 1);
    }

    public void Restart() { Progress = 0; Playing = false; }

    public void Update(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        if (!Playing) return;
        Seek(Progress + delta.TotalSeconds * Definition.Speed / Definition.DurationSeconds);
        if (Progress == 1) Playing = false;
    }
}
