using PawnShop.Core.World;
using PawnShop.Framework.Rendering;
using System.Collections.ObjectModel;
using PawnShop.Narrative.Dialogue;
using PawnShop.Characters.Player;
using PawnShop.Characters.Movement;
using System.Numerics;
using System.Buffers.Binary;
using PawnShop.Characters.Npcs.Common;

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
    public CommonNpcRoomVisitor? CommonVisitor { get; }
    private readonly PlacedNpc[] placements;
    private readonly int commonVisitorIndex = -1;
    private readonly MovementBounds[] visitorObstacles;
    private readonly Dictionary<string, SpriteAnimator> animators = new();
    private readonly Dictionary<string, int> frames = new();
    public IReadOnlyDictionary<string, int> NpcFrames { get; }

    public GameSession(SpriteSheetDefinition playerSheet, IReadOnlyList<PlacedNpc>? npcs = null, PlayerGender gender = PlayerGender.Male,
        CommonNpcCatalog? commonNpcs = null, ulong? commonVisitorSeed = null)
    {
        playerAnimator = new(playerSheet, "Idle");
        placements = (npcs ?? Array.Empty<PlacedNpc>()).ToArray();
        Npcs = Array.AsReadOnly(placements);
        player = new(gender, ShopRoomLayout.PlayerSpawn);
        obstacles = new[] { ShopRoomLayout.Counter }.Concat(Npcs.Select(ShopRoomLayout.NpcFootprint)).ToArray();
        if (!ShopRoomLayout.Floor.Contains(player.Footprint) || obstacles.Any(o => o.Intersects(player.Footprint)))
            throw new InvalidDataException("Shop player spawn overlaps an obstacle or is outside the walk area.");
        if (commonNpcs is not null && Npcs.Any(n => n.Id == CommonNpcRoomVisitor.NpcId))
        {
            Guid id = Guid.NewGuid();
            // New Game samples once. The realized appearance and seed stay fixed for
            // this session, including pause/dialogue. Tests can supply an exact seed.
            ulong seed = commonVisitorSeed ?? BinaryPrimitives.ReadUInt64LittleEndian(id.ToByteArray());
            commonVisitorIndex = Array.FindIndex(placements, n => n.Id == CommonNpcRoomVisitor.NpcId);
            var destination = placements[commonVisitorIndex];
            CommonVisitor = CommonNpcRoomVisitor.Create(commonNpcs, id, seed, destination.X, destination.Y);
            UpdateVisitorPlacement();
        }
        visitorObstacles = new[] { player.Footprint, ShopRoomLayout.Counter }
            .Concat(Npcs.Where(n => n.Id != CommonNpcRoomVisitor.NpcId).Select(ShopRoomLayout.NpcFootprint)).ToArray();
        foreach (var npc in Npcs)
        {
            if (CommonVisitor is not null && npc.Id == CommonNpcRoomVisitor.NpcId)
            {
                frames.Add(npc.Id, CommonVisitor.SpriteFrame);
                continue;
            }
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
        if (CommonVisitor is not null)
        {
            visitorObstacles[0] = player.Footprint;
            CommonVisitor.Update(delta, visitorObstacles);
            UpdateVisitorPlacement();
            frames[CommonNpcRoomVisitor.NpcId] = CommonVisitor.SpriteFrame;
        }
        player.Move(movement, delta, ShopRoomLayout.Floor, obstacles);
        playerAnimator.Play(player.View.Moving ? "Walk" : "Idle");
        playerAnimator.Update(delta);
        room.Tick(delta, ringBell);
        foreach (var (id, animator) in animators) { animator.Update(delta); frames[id] = animator.FrameIndex; }
    }

    private void UpdateVisitorPlacement()
    {
        if (CommonVisitor is null) return;
        placements[commonVisitorIndex] = placements[commonVisitorIndex] with { X = CommonVisitor.X, Y = CommonVisitor.Y };
        obstacles[commonVisitorIndex + 1] = ShopRoomLayout.NpcFootprint(placements[commonVisitorIndex]);
    }

    public void Dispose()
    {
        CloseDialogue(DialogueEndReason.SessionDisposed);
        IsDisposed = true;
    }
}
