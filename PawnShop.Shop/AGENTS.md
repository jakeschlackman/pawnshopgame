# Shop

- Intended owner: customers, appraisal, restoration, displays, and shop workflows. References Core only.
- This project currently contains scaffolding folders, not an implemented economy or customer simulation. Inspect the requested folder and Core before adding the smallest authorized slice.
- Read relevant [transaction](../docs/ENGINEERING_CONTRACTS.md#commands-events-and-transactions), [inventory/economy](../docs/ENGINEERING_CONTRACTS.md#inventory-and-economy), and [save](../docs/ENGINEERING_CONTRACTS.md#persistence-and-content-validation) rules before implementing them.
- Reuse Core inventory/currency/atomic operations; do not create a second stock ledger. Listings reference real items; appraisal uncertainty is separate from authoritative properties.
- Character appearance/behavior belongs in Characters; conversations/quests in Narrative; Game/Bootstrap composes them with Shop. Do not add rendering dependencies here.
- No pawn-loan/redemption system is implied. Do not fill empty folders or invent prices/schedules without a requested feature.
- For code changes use root verification commands and add relevant headless failure/atomicity tests in Tests.
