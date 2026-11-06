# Add and edit NPCs

Create an NPC from the repository terminal (PowerShell 7), or run **NPC: Add NPC and conversation** from VS Code's Run Task menu:

```powershell
./npc.ps1 new -Id mechanic -Name "Mechanic" -X 180 -Y 235
```

This creates the folder below and adds a shop placement. No C# registration, Inky installation, voice files, or manual Ink export is needed. Rebuild and restart the game after editing content; hot reload is not implemented. Walk within 48 logical pixels of the NPC, then press Z or click it to talk.

```text
Content/Npcs/mechanic/
  npc.json
  conversations.json
  dialogue/greeting.ink
  sprites/body.png
  sprites/body.json
```

## What to edit

| File | Responsibility |
| --- | --- |
| `npc.json` | Stable ID, display name, sprite references, interaction rectangle, optional conversation-file reference |
| `conversations.json` | Named conversations and the default used when talking |
| `dialogue/*.ink` | Text, choices, branches and conversation-local variables |
| `sprites/body.png` | Transparent sprite sheet |
| `sprites/body.json` | Frame dimensions, grid, foot pivot, display scale and animation clips |
| `Content/World/shop.npcs.json` | Which NPC IDs appear in the shop and their foot positions |

The included visitor is a complete example. JSON schema references provide VS Code completion and diagnostics for NPC definitions, conversation bindings and placements. The loader checks supported fields and cross-file references as well. Use forward-slash paths inside each NPC folder; references cannot escape it. Keep the folder name equal to the stable NPC ID. Changing a display name does not change identity.

An NPC can exist without conversation content: omit `conversations` or set it to `null` in `npc.json`. Talking then returns `NoConversation`. Omit its shop placement to keep a definition available without spawning it. The prototype allows one stationary placement per NPC ID, at x=80..560 and y=170..270. Player movement and Talk proximity are implemented; NPC movement is not. Keep placements clear of the player spawn and furniture. Both Z and clicking revalidate the 48-pixel Talk range.

## Add another conversation to an existing NPC

Create `dialogue/repair.ink`, then add an entry:

```json
{
  "$schema": "../conversations.schema.json",
  "version": 1,
  "default": "greeting",
  "entries": {
    "greeting": "dialogue/greeting.ink",
    "repair": "dialogue/repair.ink"
  }
}
```

Ordinary Talk uses `default`. Change it to `repair` to use that conversation. Code integration may explicitly request `TryTalkTo("mechanic", "repair")`; quest-based selection is not implemented yet. Preview a named conversation with `./npc.ps1 play -Id mechanic -Conversation repair`.

Ink example:

```ink
Hello! What needs fixing? # line: mechanic.greeting # speaker: mechanic
+ [Just looking. # id: leave]
    Come back any time. # line: mechanic.goodbye # speaker: mechanic
    -> END
```

Every displayed line requires stable `line` and `speaker` tags. A speaker must match an NPC definition ID. Choices require a stable `id` **inside the square brackets**; response labels must be one paragraph of at most 80 characters. Optional `emotion` values are `neutral`, `pleased`, `worried`, and `confident`. Unknown/duplicate tags are errors. VS Code snippets `inkline`, `inkchoice` and `inkbranch` are available through Insert Snippet (see the workspace snippet file for current prefixes).

Use Ink knots, conditions and variables for branching. Each interaction starts fresh; conversation variables are not saved. There are no gameplay rewards, external functions, quest actions, or inventory mutations in this draft. Dialogue pauses room simulation, including player movement and NPC animation; X or Escape cancels and closing input cannot also activate the room.

## Replace or animate the sprite

Replace `sprites/body.png` and update its metadata together. The sample is a 24×40 transparent placeholder with one looping `Idle` frame, foot pivot (12,40), and display scale 1. It preserves the previous code-drawn visitor: brown body (123,83,58), skin-colored head (230,183,139). The scaffold's copies under `tools/npcs` are project-authored placeholders, not third-party art.

For multiple frames, set the grid columns/rows, frame dimensions and `Idle.frameCount`. Frames are row-major. PNG dimensions must equal frame size times grid dimensions. `npc.json`'s `sprite.idleAnimation` must name a looping clip. The pivot anchors each frame to the placement's foot position. Keep interaction bounds independent of canvas dimensions. The room owns textures; the session owns animation state. No portraits or emotion-to-sprite switching are wired yet.

## Check and preview

```powershell
./npc.ps1 list
./npc.ps1 check
./npc.ps1 play -Id mechanic
```

These commands build first and run headlessly. `check` validates registered NPC assets/bindings and explores up to 256 dialogue states per conversation, including alternative branches. It is a bounded smoke check, not proof of every random, visit-count or local-variable-dependent path. Add focused authored tests for those cases. PNG checks inspect the header/dimensions; actual decoding and appearance still require a manual game check.

Normal builds compile `.ink` to generated `.ink.json` under `obj/`; output and publish folders receive the generated content plus source assets. Do not edit generated files or files under `bin/`. A current-source index prevents deleted content from being rediscovered in old output directories. `dialogue.ps1` still forwards to this workflow for existing commands.

Quests, likes/dislikes, relationships and awake schedules are deliberately not placeholder fields. Those future features can use the NPC's stable ID and add focused definitions/runtime state when implemented.
