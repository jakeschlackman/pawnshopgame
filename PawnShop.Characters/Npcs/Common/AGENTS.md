# Common NPCs

- Ordinary generated townies/shoppers only; main-player and notable NPC art use separate designs.
- Start with `CommonNpcCatalog.cs` (validation/frame resolution), `Generation/CommonNpcGenerator.cs` (seeded compatible choices), and `CommonNpcEntrance.cs` (bounded room example). General browsing/trading/schedules are not implemented.
- For data/art additions read [AUTHORING.md](AUTHORING.md) and only the relevant wearable workflow. Add approved PNGs/poses to `Assets` and definitions to `Assets/catalog.json`; preserve `migration.json` provenance.
- Outfit/hair/hat additions should be data-driven. Keep body-specific fits and explicit hair/headgear compatibility; missing fits cannot borrow another body's art.
- One pose clock and character-space West mirror apply to body, layers, occlusion, and anchors. Seed reproduces appearance, not identity; caller supplies stable IDs.
- Game packages the library and owns room textures/composition. Keep generators/catalogs device-free; do not change original player/review sources or Sprout.
- After the root build, run `dotnet run --project PawnShop.Tests --no-build -- --assets common PawnShop.Characters/Npcs/Common/Assets` and the ordinary runner when affected. Numeric success does not grant visual approval.
