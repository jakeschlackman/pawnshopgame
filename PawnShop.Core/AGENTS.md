# Core

- Owns shared gameplay state/rules, inventory, items, interactions, conditions/effects, atomic transactions, economy, world state, and neutral persistence mechanics.
- Current implementation is the headless clock/bell fixture in `World/RoomState.cs`. Other feature folders are scaffolding; do not describe proposed inventory/economy/save systems as implemented.
- No current package/project dependencies. Keep rules testable without graphics/audio devices, ContentManager, or Game references. Asset references are IDs.
- Before implementing protected state changes, read the relevant [transaction](../docs/ENGINEERING_CONTRACTS.md#commands-events-and-transactions), [inventory/economy](../docs/ENGINEERING_CONTRACTS.md#inventory-and-economy), or [persistence](../docs/ENGINEERING_CONTRACTS.md#persistence-and-content-validation) rules.
- Shared interaction/condition contracts are in [interactions and narrative](../docs/ENGINEERING_CONTRACTS.md#interactions-dialogue-and-quests); Narrative owns dialogue/quest implementations.
- Give each state one owner; expose snapshots. Failed operations change nothing; publish facts after atomic commit. Feature save DTOs stay with features; Core storage cannot reference them.
- Shop workflows use Core operations. Cross-feature wiring belongs in Game/Bootstrap. Add only the requested slice.
- Use root verification commands; test domain changes headlessly with isolated save directories. Existing fixture behavior is covered by lifecycle tests in Tests.
