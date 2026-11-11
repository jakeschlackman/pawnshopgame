namespace PawnShop.Audio;

// Cosmetic slider feedback only. Missed detents are dropped, never queued.
public sealed class SliderClickSequence
{
    public static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(50);
    private readonly Random random;
    private readonly ShuffledSoundBag bag;
    private TimeSpan remaining;

    public SliderClickSequence(int sampleCount, Random random)
    {
        bag = new(sampleCount, random);
        this.random = random;
    }

    public (int Sample, float Pitch)? Update(TimeSpan delta, bool requested)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        remaining = delta >= remaining ? TimeSpan.Zero : remaining - delta;
        if (!requested || remaining > TimeSpan.Zero) return null;
        int sample = bag.Next();
        remaining = Cooldown;
        // MonoGame Pitch is in octaves: rate = 2^Pitch, not 1 + Pitch.
        float pitch = (float)Math.Log2(.975 + random.NextDouble() * .05);
        return (sample, pitch);
    }
}
