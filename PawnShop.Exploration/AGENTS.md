# Exploration

- Intended owner: dungeons, salvage, monsters, hazards, generation, and location-specific exploration rules. References Core, Characters, and Framework.
- Currently scaffolding only. There is no dungeon generator, expedition loop, or salvage system to extend yet; implement only the requested slice.
- Read [actor/location/generation rules](../docs/ENGINEERING_CONTRACTS.md#actors-and-exploration) and [persistence rules](../docs/ENGINEERING_CONTRACTS.md#persistence-and-content-validation) when those contracts are affected.
- Reuse Core ownership/transactions for discoveries and collection, and Characters for actor movement/combat. Search and collection are separate; full cargo must retain the find rather than reroll or delete it.
- Combat opponents are monsters, never humans; avoid humanoids except skeletons. Keep exits/spawns reachable for the actual controlled actor footprint.
- Persist run changes; leaving/reloading must not respawn loot. Procedural generation needs approved scope, controlled seeds/versioning, bounded retries, and reachability checks.
- Rendering/session travel orchestration remains at the composition boundary. Use root verification commands and deterministic headless regressions for the implemented rules.
