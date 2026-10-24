namespace PawnShop.Core.World;

public readonly record struct RoomView(Guid SessionId, TimeSpan Elapsed, int BellRings);

// The existing M01 room fixture, independent of input devices and graphics.
public sealed class RoomState(Guid sessionId)
{
    private TimeSpan elapsed;
    private int bellRings;
    public RoomView View => new(sessionId, elapsed, bellRings);

    public void Tick(TimeSpan delta, bool ringBell)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        elapsed += delta;
        if (ringBell) bellRings++;
    }
}
