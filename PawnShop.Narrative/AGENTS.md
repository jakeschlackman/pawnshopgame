# Narrative

- Owns dialogue, quests, relationships, story flags, and progression. References Core and the pinned Ink runtime; never Game, graphics, or audio devices.
- Implemented: `Dialogue/DialogueDefinition.cs`, `DialogueSession.cs`, `ConversationBindings.cs`, and `DialogueContentValidator.cs`. Quest/relationship/progression folders remain scaffolding.
- Read [dialogue/quest rules](../docs/ENGINEERING_CONTRACTS.md#interactions-dialogue-and-quests) when changing narrative behavior. Use stable choice IDs and session/revision validation; presentation consumes immutable views.
- Ink currently owns conversation-local branching only. Each opening starts fresh; there are no gameplay externals, rewards, durable variables, or quest effects. Adding those is a separately scoped domain integration.
- Authored NPC content lives in Game/Content/Npcs. Follow its [instructions](../PawnShop.Game/Content/Npcs/AGENTS.md) and [authoring guide](../PawnShop.Game/Content/Npcs/README.md) for content changes.
- Game/Bootstrap owns content binding, session composition, presenter lifetime, and pause/input capture. Keep these separate from Ink evaluation.
- Regressions/CLI: `PawnShop.Tests/Dialogue`; Ink compiles through `build/Ink.targets` during normal Game builds. Use root verification commands; the bounded checker is not proof of every possible Ink path.
