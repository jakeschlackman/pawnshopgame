using System.Numerics;
using PawnShop.Characters.Movement;

namespace PawnShop.Game.Bootstrap;

// One authored geometry shared by simulation and room presentation.
public static class ShopRoomLayout
{
    public static MovementBounds Floor { get; } = new(32, 80, 576, 224);
    public static MovementBounds Counter { get; } = new(68, 104, 190, 48);
    public static Vector2 PlayerSpawn { get; } = new(325, 215);
    public const float TalkDistance = 48;
    public static MovementBounds NpcFootprint(PlacedNpc npc) => new(npc.X - 8, npc.Y - 6, 16, 6);
}
