using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using System.Collections.ObjectModel;
using PawnShop.Narrative.Dialogue;
using PawnShop.Characters.Player;
using PawnShop.Characters.Movement;
using System.Numerics;

namespace PawnShop.Game.Bootstrap;

// Owns player movement, player/NPC animation, the room clock/bell fixture and the transient conversation.
// No inventory, save state, event bus, or graphics resources are created here.
public sealed class GameSession : IDisposable
{
    private readonly RoomState room = new(Guid.NewGuid());
    public bool IsDisposed { get; private set; }
    public RoomView View => room.View;
    public DialogueSession? Dialogue { get; private set; }
    private readonly PlayerState player;
    private readonly MovementBounds[] obstacles;
    public PlayerView Player => player.View;
    private readonly SpriteAnimator playerAnimator;
    public int PlayerFrame => playerAnimator.FrameIndex;

    public IReadOnlyList<PlacedNpc> Npcs { get; }
    private readonly Dictionary<string, SpriteAnimator> animators = new();
    private readonly Dictionary<string, int> frames = new();
    public IReadOnlyDictionary<string, int> NpcFrames { get; }

    public GameSession(SpriteSheetDefinition playerSheet, IReadOnlyList<PlacedNpc>? npcs = null, PlayerGender gender = PlayerGender.Male)
    {
        playerAnimator = new(playerSheet, "Idle");
        Npcs = Array.AsReadOnly((npcs ?? Array.Empty<PlacedNpc>()).ToArray());
        player = new(gender, ShopRoomLayout.PlayerSpawn);
        obstacles = new[] { ShopRoomLayout.Counter }.Concat(Npcs.Select(ShopRoomLayout.NpcFootprint)).ToArray();
        if (!ShopRoomLayout.Floor.Contains(player.Footprint) || obstacles.Any(o => o.Intersects(player.Footprint)))
            throw new InvalidDataException("Shop player spawn overlaps an obstacle or is outside the walk area.");
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

    public bool CanTalkTo(PlacedNpc npc) => Vector2.DistanceSquared(player.View.Position, new(npc.X, npc.Y))
        <= ShopRoomLayout.TalkDistance * ShopRoomLayout.TalkDistance;

    public string? NearbyNpcId
    {
        get
        {
            string? id = null;
            float nearest = ShopRoomLayout.TalkDistance * ShopRoomLayout.TalkDistance;
            foreach (var npc in Npcs)
            {
                float distance = Vector2.DistanceSquared(player.View.Position, new(npc.X, npc.Y));
                if (distance <= nearest) { nearest = distance; id = npc.Id; }
            }
            return id;
        }
    }

    internal void Tick(TimeSpan delta, bool ringBell, Vector2 movement = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        player.Move(movement, delta, ShopRoomLayout.Floor, obstacles);
        playerAnimator.Play(player.View.Moving ? "Walk" : "Idle");
        playerAnimator.Update(delta);
        room.Tick(delta, ringBell);
        foreach (var (id, animator) in animators) { animator.Update(delta); frames[id] = animator.FrameIndex; }
    }

    public void Dispose()
    {
        CloseDialogue(DialogueEndReason.SessionDisposed);
        IsDisposed = true;
    }
}
