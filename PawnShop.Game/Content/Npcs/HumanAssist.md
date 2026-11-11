# Working on authored NPC content
Use [README.md](README.md) for the established NPC scaffold, JSON schemas, Ink tags, branch checking, and terminal preview.

Edit npc.json for identity/sprite references, conversations.json for named conversations, dialogue/*.ink for text/choices, and ../World/shop.npcs.json for placement. Keep folder/NPC IDs aligned and preserve stable line, speaker, and choice IDs.

Normal Game builds compile Ink and generate an index; do not maintain compiled JSON beside the sources or edit obj/bin. Current conversations are local to each opening, without gameplay rewards, externals, or saved Ink variables.

After building, check all registered conversations:

```text
dotnet run --project PawnShop.Tests --no-build -- --dialogue check
```

The check is bounded smoke coverage. Manually inspect layout and play authored branches that use conditions/history/randomness. Common procedural shopper appearances belong in Characters/Npcs/Common, not these authored definitions.
