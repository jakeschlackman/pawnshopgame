namespace PawnShop.Audio;

// Each sample once per shuffle, with no consecutive repeat across bag boundaries.
public sealed class ShuffledSoundBag
{
    private readonly Random random;
    private readonly int[] bag;
    private int next;
    private int last = -1;

    public ShuffledSoundBag(int sampleCount, Random random)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleCount);
        ArgumentNullException.ThrowIfNull(random);
        this.random = random;
        bag = Enumerable.Range(0, sampleCount).ToArray();
        next = bag.Length;
    }

    public int Next()
    {
        if (next == bag.Length)
        {
            for (int i = bag.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (bag[i], bag[j]) = (bag[j], bag[i]);
            }
            if (bag.Length > 1 && bag[0] == last)
            {
                int j = random.Next(1, bag.Length);
                (bag[0], bag[j]) = (bag[j], bag[0]);
            }
            next = 0;
        }
        last = bag[next++];
        return last;
    }
}
