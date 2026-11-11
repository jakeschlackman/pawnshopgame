# Characters

- Owns player/NPC identity, movement, AI/behaviors, combat, mech, and companion features. References Core and Framework; never Game.
- Current code: `Player/PlayerState.cs`, `Movement/MovementBounds.cs`, `Npcs/NpcDefinition.cs`, and `Npcs/Common`. Other actor folders are scaffolding.
- Creation presentation: read `Player/Create/AGENTS.md`. Common-NPC appearance/entrance work: read `Npcs/Common/AGENTS.md`. General authored NPC content is composed by Game.
- Before changing mount, companion, combat, or location ownership, read [actor rules](../docs/ENGINEERING_CONTRACTS.md#actors-and-exploration). Player, rider/mech, and small companion remain distinct identities.
- Movement uses held intent, normalized diagonals, swept footprints independent of sprite canvas, and one simulation owner. Preserve the dominant-axis facing policy and shared animation clock.
- Human art work must follow the root artwork routing and body-specific approvals. Sprout's existing assets/implementation remain frozen unless the user explicitly authorizes the change.
- Core owns item/inventory state; Game/Bootstrap wires sessions and cross-feature actions. Keep seeded appearance generation headless and runtime-offline.
- Regressions: player tests and `PawnShop.Tests/Characters`. Use root build/runner commands; do not infer visual fit from numeric checks.
