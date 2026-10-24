using PawnShop.Core.World;

namespace PawnShop.Game.Bootstrap;

// M01's only simulation is a clock and a doorbell input-capture fixture.
// No inventory, save state, event bus, or graphics resources are created here.
public sealed class GameSession : IDisposable
{
    private readonly RoomState room = new(Guid.NewGuid());
    public bool IsDisposed { get; private set; }
    public RoomView View => room.View;

    internal void Tick(TimeSpan delta, bool ringBell)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        room.Tick(delta, ringBell);
    }

    public void Dispose() => IsDisposed = true;
}
