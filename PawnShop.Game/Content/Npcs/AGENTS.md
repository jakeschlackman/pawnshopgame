# Authored NPC content

- Read [README.md](README.md) for the working scaffold, JSON/Ink tags, and CLI workflow. Edit source NPC folders and `../World/shop.npcs.json`, never compiled output.
- `npc.json` owns stable authored identity/sprite references; `conversations.json` selects Ink sources; world placement is separate. Folder names match NPC IDs.
- Add ordinary content through definitions, not C# registration. Preserve stable line/choice/speaker IDs and bounded relative paths.
- Ink is conversation-local only: no gameplay externals, rewards, or durable state in this draft. Narrative owns runtime interpretation; Game/Bootstrap binds it to sessions.
- Normal builds compile Ink through `build/Ink.targets` and generate the current index under obj. Do not check generated JSON into source or revive deleted content from stale output.
- After the root build, use `dotnet run --project PawnShop.Tests --no-build -- --dialogue check` and relevant regressions. The checker is bounded, not exhaustive.
- Common procedural shopper art belongs in Characters/Npcs/Common; keep it separate from authored notable-NPC content.
