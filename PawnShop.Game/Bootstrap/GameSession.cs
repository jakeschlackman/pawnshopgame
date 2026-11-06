using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using System.Collections.ObjectModel;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Game.Bootstrap;

// Owns the room clock/bell fixture and the current transient conversation.
// No inventory, save state, event bus, or graphics resources are created here.
public sealed class GameSession : IDisposable
{
    private readonly RoomState room = new(Guid.NewGuid());
    public bool IsDisposed { get; private set; }
    public RoomView View => room.View;
    public DialogueSession? Dialogue { get; private set; }

    public IReadOnlyList<PlacedNpc> Npcs { get; }
    private readonly Dictionary<string, SpriteAnimator> animators = new();
    private readonly Dictionary<string, int> frames = new();
    public IReadOnlyDictionary<string, int> NpcFrames { get; }

    public GameSession(IReadOnlyList<PlacedNpc>? npcs = null)
    {
        Npcs = Array.AsReadOnly((npcs ?? Array.Empty<PlacedNpc>()).ToArray());
        foreach (var npc in Npcs)
        {
            var asset = npc.Content.Sprite;
            var animator = new SpriteAnimator(asset.Sheet, asset.IdleAnimation);
            animators.Add(npc.Id, animator);
            frames.Add(npc.Id, animator.FrameIndex);
        }
        NpcFrames = new ReadOnlyDictionary<string, int>(frames);
    }

    internal DialogueSession StartDialogue(DialogueDefinition definition)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (Dialogue is { EndReason: null }) throw new InvalidOperationException("A conversation is already open.");
        Dialogue?.Dispose();
        Dialogue = new DialogueSession(definition);
        return Dialogue;
    }

    internal void CloseDialogue(DialogueEndReason reason)
    {
        Dialogue?.End(reason);
        Dialogue = null;
    }

    internal void Tick(TimeSpan delta, bool ringBell)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        room.Tick(delta, ringBell);
        foreach (var (id, animator) in animators) { animator.Update(delta); frames[id] = animator.FrameIndex; }
    }

    public void Dispose()
    {
        CloseDialogue(DialogueEndReason.SessionDisposed);
        IsDisposed = true;
    }
}
