using PawnShop.Characters.Npcs.Common;
using PawnShop.Characters.Npcs.Common.Generation;
using PawnShop.Characters.Movement;

namespace PawnShop.Game.Bootstrap;

// Presentation replacement for the existing demo visitor, not a second placement
// or a new dialogue speaker. Owned by one session; drawing never regenerates it.
public sealed class CommonNpcRoomVisitor
{
    public const string NpcId = "visitor";
    public CommonNpc Character { get; }
    public IReadOnlyList<CommonNpcFrame> Frames { get; }
    private readonly CommonNpcEntrance entrance;
    public int X => (int)MathF.Ceiling(entrance.X);
    public int Y => entrance.Y;
    public int SpriteFrame => entrance.SpriteFrame;
    public bool Arrived => entrance.Arrived;

    private CommonNpcRoomVisitor(CommonNpc character, CommonNpcCatalog catalog, int destinationX, int y)
    {
        Character = character;
        entrance = new(672, destinationX, y);
        var frames = new List<CommonNpcFrame> { catalog.Resolve(character.Appearance, CommonNpcFacing.West, CommonNpcPose.Standing) };
        for (int i = 0; i < 8; i++) frames.Add(catalog.Resolve(character.Appearance, CommonNpcFacing.West, CommonNpcPose.Walking, i));
        frames.Add(catalog.Resolve(character.Appearance, CommonNpcFacing.West, CommonNpcPose.Settling));
        Frames = frames.AsReadOnly();
    }

    public static CommonNpcRoomVisitor Create(CommonNpcCatalog catalog, Guid instanceId, ulong seed, int destinationX, int y) =>
        new(new CommonNpcGenerator(catalog).Create(instanceId, seed), catalog, destinationX, y);

    internal void Update(TimeSpan delta, IReadOnlyList<MovementBounds> obstacles) => entrance.Update(delta, obstacles);
}
