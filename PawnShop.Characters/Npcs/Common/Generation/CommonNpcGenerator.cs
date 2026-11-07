namespace PawnShop.Characters.Npcs.Common.Generation;

// Realized appearance is data, not a seed-only save or a collection of textures.
public sealed record CommonNpcAppearance(string BodyId, string TorsoId, string LegsId,
    string FeetId, string? HairId, string? HeadgearId);

public sealed record CommonNpc(Guid Id, ulong Seed, int GeneratorVersion, int CatalogVersion,
    CommonNpcAppearance Appearance);

public sealed class CommonNpcGenerator(CommonNpcCatalog catalog)
{
    private readonly CommonNpcCatalog catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

    // The caller owns identity allocation. Equal seeds reproduce appearance, not identity.
    // Version 1 selects each mandatory slot and optional headgear/hair from the catalog.
    public CommonNpc Create(Guid id, ulong seed)
    {
        if (id == Guid.Empty) throw new ArgumentException("A shopper requires a stable nonempty instance ID.", nameof(id));
        ulong state = seed;
        string body = catalog.BodyIds[(int)(Next(ref state) % (ulong)catalog.BodyIds.Count)];
        string Pick(string slot)
        {
            var choices = catalog.Choices(body, slot);
            return choices[(int)(Next(ref state) % (ulong)choices.Count)];
        }
        string? Optional(string slot, string? headgear = null)
        {
            var choices = catalog.Choices(body, slot, headgear);
            int index = (int)(Next(ref state) % (ulong)(choices.Count + 1));
            return index == choices.Count ? null : choices[index];
        }
        string jacket = Pick("torso"), trousers = Pick("legs"), boots = Pick("feet");
        string? hat = Optional("headgear"), hair = Optional("hair", hat);
        return new(id, seed, 1, catalog.Version, new(body, jacket, trousers, boots, hair, hat));
    }

    // SplitMix64 is specified here to keep results independent of System.Random changes.
    private static ulong Next(ref ulong state)
    {
        unchecked
        {
            ulong value = state += 0x9E3779B97F4A7C15UL;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
