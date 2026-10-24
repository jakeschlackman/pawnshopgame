# Pawn Shop Game — Game Design

Document status: proposed design baseline, grounded in the supplied development rules and available project conversations. Updated 2026-10-24. This document does not report implemented features.

Companions: [ARCHITECTURE.md](ARCHITECTURE.md) describes systems and contracts; [ROADMAP.md](ROADMAP.md) sequences implementation and acceptance checks. [agents.md](agents.md) owns durable development rules. All four documents live at the repository root.

The approved repository organization is `PawnShopSimulator.sln` with root-level Framework, Core, Narrative, Characters, Shop, Exploration, Game, and Tests projects, each prefixed `PawnShop.`. Core holds shared gameplay models/rules; the feature libraries own their respective systems; Framework provides reusable infrastructure; Game/Bootstrap composes sessions and Game/Screens presents them. The existing M01 behavior is preserved. Empty feature folders are scaffolding, not implemented gameplay. This organization supersedes earlier project-layout proposals and changes no gameplay decisions or milestone acceptance requirements. See README.md for current build/run commands.

## 1. Evidence, status, and design authority

### 1.1 Sources used

| Source | Established direction carried forward |
| --- | --- |
| Supplied `agents.md`, especially sections 1, 6, 9–13, and 15 | Product requirements, reusable systems, distinct robot roles, inventory/economy correctness, narrative rules, asset-scale caution, and title-screen-first development |
| “Pawnshop Game Idea,” September 13, 2026 | Cozy Stardew-like pawn-shop premise and a true top-down pixel-art gameplay presentation |
| “Generate Walking Spritesheets,” October 9, 2026 | Searchable and improvable junk piles as the farming-like activity; cleaning, repair, restoration, abandoned malls, and monster-only combat |
| “Pawn Shop Robot Concept” and “Image generation failed,” October 22–23, 2026 | Large rideable egg-shaped mech, smaller expressive companion, less-creepy brain/memory visual motif, confidence giving way to fear, comic dizziness, separate rider artwork, walking and emotion sheets |
| Current architecture and setup discussion, October 24, 2026 | C#/MonoGame, reusable components, common inventory, authored conversations/quests, start screen first, and concern about companion size and 48×48 downscaling |

The supplied file and visible conversation excerpts are the primary design evidence. Retrieved history summaries add context but are not a complete export of every chat in the project folder. The original design did not establish implementation status; current repository verification and remaining M01 manual checks are recorded in [the evidence ledger](docs/VERIFICATION_M01.md).

### 1.2 Reading the decision labels

- **Established:** required by the supplied `agents.md` or explicit visible user direction. This describes intent, not completed implementation.
- **Working baseline:** a concrete proposal used to make this design buildable. It may be tuned or replaced through an explicit design decision.
- **Open:** a consequential choice not settled by the sources. The document gives a safe prototype default where possible.

The attached development rules take precedence over older brainstorms. Names, exact prices, schedules, plot revelations, pixel dimensions, and example quest content are not established canon merely because they appear here. Future implementation should take one authorized roadmap slice, not treat the entire document as permission to build every feature.

## 2. Game identity and boundaries

### 2.1 Player promise

You run a small pawn shop where apparently worthless objects become useful, valuable, or personally meaningful through your judgment and effort. You search and improve a salvage yard, explore abandoned places in a ridiculous walking mech, and return with finds worth cleaning, fixing, collecting, or selling. A very small robot provides a disproportionate amount of confidence and commentary.

The emotional rhythm is discovery, curiosity, competent repair, a satisfying sale, and a reason to try another expedition. Combat creates occasional pressure around valuable finds; it is not the game's primary identity.

### 2.2 Design pillars

| Pillar | What the player experiences | What to avoid |
| --- | --- | --- |
| Value through care | A recognizable object improves through inspection, cleaning, repair, and presentation | Every activity collapsing into collecting an abstract currency |
| A salvage yard worth tending | Better sorting, access, and equipment make familiar piles more productive | A farm reskinned with unexplained scrap growing from soil |
| Cozy mechanical comedy | The mech communicates through body language; the companion overestimates its courage | Mean-spirited cruelty, constant screaming, or jokes blocking control |
| Interconnected activities | Exploration supplies the shop; profits improve tools, cargo, and restoration options | Several unrelated minigames with separate inventories and progression |
| Authored human-scale stories | Customers, objects, and places reveal short, memorable stories | A mandatory epic plot that takes over the business simulation |
| Readable and forgiving play | Clear interactions, deliberate choices, recoverable mistakes | Hidden irreversible costs or avoidable inventory loss |

### 2.3 Scope

**Established:** local single-player, top-down pixel art, C# with MonoGame; a shop, improvable junk piles, abandoned-location exploration, player, rideable mech, small companion, authored dialogue, quests, and a modest storyline.

**Outside the baseline:** multiplayer, online services, runtime AI-generated conversations, telemetry, mod support, a general-purpose game engine, and a custom world/editor platform. Collateral loans and redemption deadlines are not implied by the word “pawnshop.” Romance, farming crops, seasons, survival hunger, and voice acting are likewise not assumed requirements.

## 3. Core loop and player decisions

The repeatable loop is:

1. **Find or acquire:** search a junk pile, recover an expedition object, or buy a customer's offered item.
2. **Choose and transport:** decide what fits, what deserves cargo space, and what can wait.
3. **Inspect:** identify useful properties, condition, missing parts, and uncertain value.
4. **Improve or dismantle:** clean, repair, restore, keep as-is, or recover materials.
5. **Use, deliver, display, or sell:** choose the destination of the actual object.
6. **Reinvest:** improve workstations, yard organization, storage, shop facilities, tools, or mech capabilities.

Not every item must pass through every step. An intact lamp may sell after cleaning. A broken radio may need a part. A corroded duplicate may be more useful as components. The decision should be understandable before resources are committed.

### 3.1 A representative day

**Working baseline:** the player checks finished station jobs, serves a customer, tends one yard pile, and chooses whether to spend the rest of the day restoring stock or exploring. After an expedition, they transfer cargo, inspect notable finds, and prepare tomorrow's sale items. Sleep advances the calendar through a deliberate action.

This is a cadence, not a mandatory checklist. A player may spend an entire session operating the shop. Story requests should create interesting opportunities without making ordinary trade impossible when ignored.

### 3.2 Where meaningful choice lives

- Limited capacity makes the shape of a haul matter.
- Inspection reduces uncertainty before expensive work.
- Restoration trades materials and game time for condition, function, or presentation.
- A customer request can make an ordinary object valuable for reasons beyond its sale price.
- Upgrades improve a specific inconvenience instead of providing invisible percentage inflation alone.
- Leaving an expedition early protects progress; going deeper offers additional opportunity.

Do not require a unique minigame for every verb. The first versions can use an interaction, readable preview, and timed operation. Add hands-on minigames only after the underlying decision is fun.

## 4. Items, inventory, and appraisal

### 4.1 Item categories

These are **working content categories**, not a required class hierarchy:

| Category | Example | Design role |
| --- | --- | --- |
| Functional goods | Radio, lamp, appliance | Repairable stock and customer requests |
| Decorative or collectible objects | Sign, figurine, old display piece | Condition and provenance matter; exact rarity is a later tuning choice |
| Materials and standard parts | Wire, fasteners, compatible components | Explicitly fungible resources for jobs and upgrades |
| Tools or equipment | Cleaning tool, repair tool, mech attachment | Enable actions or improve capability |
| Significant objects | An individually tracked customer object | Support a modest authored story without requiring every item to contain lore |

Unique goods retain their identity through transport and restoration. Two radios can have the same definition while differing in defects, cleanliness, modifications, discovery state, and history. They are not automatically a stack. Stackable materials require matching properties.

### 4.2 Object state

**Working baseline:** distinguish cosmetic dirt, physical condition, operational state, known defects, modifications, and the player's inspection knowledge. These values answer different questions; one generic “quality” number should not stand in for all of them.

Cleaning reveals or improves presentation but does not silently repair electronics. Repair restores function or addresses defects. Restoration is a higher-effort improvement appropriate to the object. Dismantling consumes the original active object and creates defined outputs with traceable provenance; it cannot also leave the original available for sale.

The interface should show known information and uncertainty separately. A low appraisal skill can produce a wider value estimate, not change the item's underlying identity or true properties.

### 4.3 One inventory experience

The player bag, mech cargo, shop storage, shelves, station slots, world loot containers, and customer holdings use a shared inventory model with different capacity and acceptance rules. A consistent transfer panel presents both sides, the selected quantity, the destination, and reasons a transfer cannot occur.

Every active unique item has exactly one authoritative location. A sale listing references the stock item; it is not another copy. A reserved repair object cannot simultaneously be sold or handed to a quest giver.

**Working baseline for the first slice:** visible slot capacity plus item acceptance rules. Support weight constraints in the architecture, but do not require weight balancing before there is evidence that slots alone are insufficient. Large cargo and body size must remain distinct from sprite-canvas dimensions.

### 4.4 Fairness rules

- Full inventories leave objects where they were; they do not destroy them.
- Failed purchases, transfers, and jobs leave all affected resources unchanged.
- Partial transfers occur only through an explicit player choice, not an unexpected half-success.
- Significant/quest-marked objects receive clear warnings or protection before sale or dismantling.
- An item leaving player-accessible storage can make a possession-based objective no longer ready.
- Storing a radio in a chest and taking it back is not discovering a second radio.

## 5. Junk piles: the farming-like system

### 5.1 Pile identity

**Established:** piles are searchable and improvable, and skills/tools improve gathering, cleaning, repair, or restoration. **Working baseline:** a pile is a managed source with a known material profile, search opportunities, access level, organization condition, and a replenishment record.

A mixed household pile, electronics pile, and industrial bin can offer different material mixes. These are proposed themes; the first playable version needs only one pile with an understandable identity.

### 5.2 Player verbs

| Action | Immediate benefit | Longer-term reason to do it |
| --- | --- | --- |
| Search | Reveal a bounded find or material batch | Supplies stock and components |
| Sort | Separate useful categories and expose access | Better predictability and less wasted search effort |
| Clear access | Open a blocked section or safe work area | Makes new search opportunities available |
| Maintain | Keep the working area usable | Preserves the benefit of prior improvements |
| Improve equipment | Change what can be safely examined or recovered | Opens capabilities rather than simply multiplying random rewards |

Searching should offer feedback: an interesting silhouette, the clink of a component, a short inspection result, or the companion's opinion. Repeated actions should not become a long sequence of identical progress bars.

### 5.3 Replenishment and persistence

**Working baseline:** piles replenish through an explicit world event such as a new delivery, a newly accessible layer, or the next day's permitted intake. They do not instantly regenerate when the player leaves the screen. Show the source of new material through a brief delivery indicator or changed pile appearance.

Store what has been searched, what remains, and which replenishment event has already been applied. Reloading, visiting a different room, or retrying an interrupted transition must not create additional loot.

Do not mix replenishment with full simulation of every unloaded yard object. Day advancement can apply a bounded, deterministic update. Exact intake rates, exhaustion limits, and maintenance frequency remain balancing decisions.

### 5.4 Avoiding busywork and softlocks

Provide a low-risk path to obtain basic repair resources or a small saleable object even after an unsuccessful expedition. Maintenance should not punish a player for spending several days exploring. Higher-grade finds can remain scarce while ordinary business stays recoverable.

The prototype should test whether tending a pile creates anticipation and planning. If the best strategy is clicking the same spot without thought, change the choices or feedback before adding more pile types.

## 6. Cleaning, repair, restoration, and dismantling

### 6.1 Workstation flow

**Working baseline:** select the actual item, inspect compatible jobs, preview required materials/time and expected changes, and confirm. The job reserves the item and commits its costs once. Its progress uses game time, never elapsed real-world time while the application is closed.

Completion changes the item's authoritative state once. The finished object remains in the station's reserved slot until collected; a full bag blocks collection, not completion or item existence. A repeated completion callback cannot produce a second object.

### 6.2 Useful distinctions

| Operation | Primary purpose | What it must not imply |
| --- | --- | --- |
| Inspect | Learn properties, defects, and work options | Guaranteed exact appraisal of every hidden property |
| Clean | Remove surface dirt and improve presentation | Automatic repair or a new item identity |
| Repair | Address a compatible defect or restore function | Every broken item has the same recipe |
| Restore | Invest more effort in a suitable object's condition | Always the most profitable decision |
| Dismantle | Trade an unusable/unwanted item for specified parts | Keep the original and also obtain its components |

Recipe compatibility is explained to the player. “Missing compatible component” is more useful than an unexplained disabled button.

### 6.3 Early content

The first economic slice uses one unique `item.radio`, a simple cleaning action, `recipe.radio_repair`, a small fungible input, and a station. It proves identification, reservation, time, collection, and sale. Additional recipes should reuse that process rather than create a dedicated radio-only inventory or UI.

Cancellation/refund behavior is **open**. Until deliberately designed, the prototype should clearly prevent cancelling a committed job instead of offering a misleading button or issuing accidental refunds. An interrupted application must resume from the saved job state.

## 7. The shop and economy

### 7.1 Basic business

**Working baseline:** buy offered goods, prepare them, list stock, let a customer select an eligible listing, and complete a sale. Shelves are inventory destinations with presentation, not duplicate stock records. The prototype can use one scripted customer and one sale interaction before implementing a browsing simulation.

Use the actual item condition in appraisal and sale eligibility. A player who repairs a listed item must do so through an allowed state transition; the listing cannot retain stale promises about the stock.

### 7.2 Price and profit

Money uses integer smallest units. Currency name, starting balance, prices, daily revenue targets, and margins are **open**.

For design evaluation, compare sale proceeds with acquisition cost, repair inputs, and the opportunity cost of time/cargo. This is a balancing model, not a promise that every item guarantees profit. Cleaning a readily saleable object may be worthwhile while an extravagant restoration is not.

Appraisal can present an estimated range and confidence. Customer preferences may later influence acceptable prices, but a clear accept/reject trade is enough initially. Haggling is a later proposal, not a prerequisite.

### 7.3 Transaction promises

At confirmation, validate item ownership, reservation/listing state, agreed price, customer funds, and receiving capacity. The sale changes money, stock ownership, and the listing together or changes none of them. A visible offer is not permission to skip that final validation.

Failures return readable outcomes: unavailable stock, changed offer, insufficient funds, full receiving inventory, or invalid reservation. Conversation animations and receipt sounds happen after a committed trade; they do not control whether the economic operation succeeded.

### 7.4 Growth

Proposed upgrades improve storage organization, workstation capability, display capacity, and access to a wider range of requests. Reputation and relationships should improve opportunity or flavor, not multiply every price through hidden rules. Rent, taxes, debt, and bankruptcy are outside the first slice and require deliberate approval before becoming pressures.

## 8. Player, mech, and companion

### 8.1 Three separate identities

| Actor | Role | Working presentation |
| --- | --- | --- |
| Player | Operates the shop, chooses actions, owns progression | Human-scale body with readable tool/use animations |
| Large salvage mech | Carries the mounted player, handles dangerous/heavy exploration, provides cargo | Egg-shaped, two-legged, retro-industrial, expressive arms and body language |
| Small companion | Follows, comments, reacts, and eventually helps with bounded interactions | Visibly smaller than player; boastful until challenged; readable face/core and exaggerated reactions |

The large mech's working direction is expressive, primarily nonverbal character acting. The companion uses authored text dialogue and barks; this does not require recorded voice acting. Specific names, including “Sprout” from earlier concept discussion, remain provisional.

### 8.2 Mounted play

Mounting redirects movement and action control to the mech without replacing the player's identity or inventory. The rider's drawing attaches to a named seat/back-container position; it is not baked into every mech frame. The mech uses its own collision footprint and cargo access rules. The attached rider does not also collide or move independently on the ground.

The prototype refuses dismounting when there is no safe nearby position. It does not place the player inside a wall or an enemy. Mounted incoming combat damage is owned by the mech in the working baseline; hitting the same attack on both mech and rider is not intended double damage.

### 8.3 Companion behavior and comedy

Begin with following, idle observation, a confident reaction, a challenged/frightened reaction, a celebration, and a brief dizzy reaction. Environmental assistance is a small authored interaction, not an unlimited autonomous puzzle solver or a second player-controlled inventory workflow.

The less-creepy brain/memory motif is a visual storytelling device. It does not establish a literal human brain, a transplanted person, recoverable memory mechanics, or a mandatory mystery. The companion can insist it understands a device and then hide behind the mech when the device moves.

**Working baseline:** a harmless head tap triggers short comic dizziness with a cooldown; it grants no farmable reward, causes no lasting damage, and does not demand repeated punishment. No companion permadeath. Barks must not interrupt important dialogue, obscure the scene, or trigger endlessly after every tiny movement.

Use bounded catch-up behavior when the companion becomes stuck. Never let companion pathfinding block a doorway or make a critical interaction impossible.

## 9. Exploration and combat

### 9.1 Location structure

**Established themes:** abandoned malls, junkyards, forgotten electronics stores, and industrial spaces. Start with an authored `location.abandoned_mall_01` made from a small number of readable rooms and passages. A dungeon does not need procedural generation to be a dungeon.

The first location needs a safe arrival, a clear return route, salvage, a modest obstacle, and a monster encounter. Furniture and shopfronts give the mall identity without demanding an enormous decorative asset library.

### 9.2 Expedition decisions

Prepare usable cargo space, enter with the mech and companion, search rooms, evaluate finds, handle hazards, and decide when to return. Heavy or awkward loot can justify mech cargo. A retreat should remain a legitimate choice, not a failed completion grade.

A useful first encounter has an obvious approach, a telegraphed threat, an avoidable attack, and a safe recovery interval. Do not begin with a large arsenal, many enemy families, or a boss requiring multiple new subsystems.

### 9.3 Enemy boundary

Combat opponents are monsters, never human opponents. Avoid humanoid monster silhouettes; skeletons are the explicitly permitted exception, not a requirement to populate every location with skeletons. Possible **unapproved examples** include a many-legged scrap creature, a rolling debris beast, or an animated non-humanoid machine hazard.

Customers and ordinary people are not combat targets. Industrial humor and exaggerated impacts fit the tone; gore and realistic human violence do not.

### 9.4 Risk, retreat, and failure

**Working baseline:** a deliberate exit preserves carried findings and returns to a safe destination. A disabled mech triggers a recoverable rescue/retreat path; the prototype has no permanent character loss. Exact repair costs, time penalties, or salvage-loss rules remain **open** and must be surfaced before deployment.

For the first slice, use no automatic item deletion on defeat. A configurable time/repair penalty can be tested later, with a guaranteed way to recover basic function. Never require a functioning mech to obtain the only resources capable of repairing that mech.

### 9.5 Persistence and replay

Re-entering the same expedition restores its looted containers, defeated/active entities, opened paths, and remaining significant objects. Scene transitions and reloads do not reset loot. An explicit future reset/new-expedition rule must distinguish a new run from re-entering the old one.

Procedural assembly is a later gated proposal. If adopted, generation must produce reachable required content and exits, use bounded retries, and preserve a layout/state or compatible generator version plus changes. Randomness alone is not a persistence policy.

## 10. Conversations, quests, and story

### 10.1 Conversation experience

Conversations are authored branches with identifiable speakers, text, optional portraits/emotions, and choices. One interaction can reveal more text; a separate action chooses or advances. Skipping a typewriter effect is not automatically selecting the first answer.

Choice availability can depend on inventory, a quest state, a relationship, a story fact, or location access. If the state changes before selection, the choice is checked again and explains why it is no longer available. Text itself does not contain executable business logic.

Full conversations pause the simulation under the working baseline. Ambient companion remarks do not. A cancelled conversation preserves already committed choices but does not award future effects. Reopening chooses a suitable node from current facts rather than replaying irreversible “on entry” rewards.

### 10.2 Objective semantics

| Objective type | Meaning | Example |
| --- | --- | --- |
| State | Something is true now; reevaluate after relevant changes and loading | A serviceable radio is accessible for delivery |
| Event | A qualifying committed action occurred under the objective's activation rules | Three qualifying radio sales happened after acceptance |
| Explicit interaction/delivery | A domain operation actually completed | The selected radio changed ownership in a valid quest turn-in |

The “three sales” example is a test fixture for event semantics, not a required quest. State objectives may accept work completed before the quest was offered. Event objectives do not retroactively count arbitrary history unless their definition deliberately says so.

### 10.3 Reference quest: a serviceable radio

This is **proposed integration content**, not established canon.

| Element | Placeholder ID or behavior |
| --- | --- |
| Customer | `npc.repair_customer` |
| Conversation | `dialogue.repair_customer` |
| Quest | `quest.radio_request` |
| Requested good | A serviceable instance of `item.radio` |
| Compatible job | `recipe.radio_repair` |
| Possible source | Yard stock or `location.abandoned_mall_01`; a pre-existing serviceable radio is valid |

The customer asks for a working radio. The player accepts, declines, or returns later. They may acquire and fix one, or use an eligible object already owned. The journal explains what makes an item acceptable and where it may be found without mandating one hidden route. This reference quest's eligible inventory scope is `PlayerBackpack`: a radio in mech cargo or shop storage must be retrieved before turn-in. Future quests may declare other explicit scopes. Readiness regresses if the eligible radio is moved out, sold, reserved elsewhere, or damaged; the quest does not permanently latch readiness just because a radio once qualified.

On return, the player selects the actual item to deliver. A single validated turn-in transfers it, grants defined rewards, records completion/claim state, and applies associated story facts together. Evaluate reward capacity against the staged post-exchange inventory, including space freed by handing over the radio. A full bag may still support a one-item-for-one-item exchange. If the complete reward still cannot fit, the whole operation remains uncommitted: the player keeps the radio, the reward remains available, and the interface explains how to retry. Test a reward variant needing more space than delivery frees, as well as a valid full-bag exchange. Reopening dialogue or loading a save cannot grant a second reward.

### 10.4 Modest storyline

**Working narrative proposal:** establishing a dependable shop gradually reconnects neglected places, useful objects, and local customers. A small series of requests introduces the yard, a repair process, a first expedition, and an optional restoration of something meaningful to a customer.

Suggested structure, not canon:

1. Establish the shop and first useful sale.
2. Help a recurring customer, giving an ordinary repair personal context.
3. Explore a newly accessible commercial space for better supplies or a request.
4. Complete a larger restoration with a visible shop/customer payoff.

The robot relationship supplies humor and continuity. A memory-origin mystery is not required; it remains an optional future story decision. The storyline should enrich the regular loop and permit continued business after a chapter resolves.

### 10.5 Narrative safety rails

Avoid mandatory expiration timers initially. Ordinary repair and sale actions continue without a quest being active. Any location gate needs a clear explanation and a recoverable prerequisite. Essential progression must not depend on permanently missing one random drop, selling an unmarked object, or exhausting every source of a required material.

## 11. Progression and pacing

**Working baseline:** progress is capability-driven first. Upgrade values, experience curves, and unlock thresholds remain tuning data, not fixed specifications.

| Track | Early capability | Later proposal | Dependencies to watch |
| --- | --- | --- | --- |
| Salvaging | Search and inspect a simple pile | Better sorting, specialized recovery, additional pile types | Basic supplies must stay obtainable |
| Restoration | Clean and repair one category | More compatible defects, materials, and stations | Avoid circular tool/part requirements |
| Shop | Sell a few actual stock items | Better displays, storage, customer variety | Do not require all story quests for ordinary income |
| Mech | Move, carry, mount/dismount, one action | Cargo and utility improvements | Failure recovery cannot require an inaccessible expedition |
| Relationships/story | Short request and follow-up | Repeated customer arcs and place-specific scenes | Rewards and access changes must persist once |

Upgrade choices should create recognizable play differences: more cargo for long runs, better inspection for careful valuation, or a station capable of another useful object category. Avoid introducing five separate progression currencies before their purpose is demonstrated.

Exact day length, travel costs, search times, job times, customer arrival cadence, and expedition length are **open**. Evaluate them in playable slices before committing to numerical balance tables.

## 12. Time, saving, and menu behavior

### 12.1 Time policy

The following is a **working baseline**, shared with the architecture:

| Situation | World simulation and calendar | Presentation/UI |
| --- | --- | --- |
| Ordinary movement and exploration | Advance | Advance |
| Pause, modal inventory, trade, or full dialogue | Pause | Remain responsive |
| Ambient companion bark | Continue | Text/animation plays without taking movement control |
| Travel or sleep | Advance through an explicit validated time-skip operation | Transition presentation runs |
| Title screen or closed application | No gameplay progression | Title UI may animate while open |

Workstation progress follows game time, including deliberate time skips. Unloaded locations use a documented dormant/time-skip policy; they do not accidentally keep full combat and customer AI running.

Paused timers do not prohibit the open UI's authorized domain actions. Transfers, trade confirmation, dialogue choices, and reward claims still execute through their normal validated operations. The modal captures world input while permitting those deliberate actions.

### 12.2 Title and session flow

The first playable screen is the title screen. New Game creates a fresh session. Continue remains unavailable with a clear reason until loading and a valid compatible save exist. Settings either change real behavior or are explicitly unavailable. Returning to title ends the session cleanly; opening another session does not retain the old inventory, quests, or subscriptions.

### 12.3 Save policy

**Working baseline:** manual saves at safe boundaries, outside active dialogue, location transitions, and combat. Saving from the pause menu is allowed when these conditions hold; the mere presence of an overlay does not prohibit saving. There is no requirement to resume halfway through a dialogue node in the first slice. After loading, conversation availability is reconstructed from committed facts.

The save includes player and robot state, ownership, money, clock, location changes, active jobs, quest/claim state, story facts, and relevant expedition state as features arrive. Loading cannot respawn collected loot or duplicate completed work.

If saving is currently blocked, explain why. Save-and-return must not pretend success; returning without saving requires a clear unsaved-progress decision. Unsupported or corrupt saves remain recoverable and are not silently overwritten. Autosaves and additional slot policies may be added later after their lifecycle is designed.

## 13. Art, sound, and usability

### 13.1 Visual readability

Use a consistent top-down pixel-art perspective and readable silhouettes. The companion must look smaller than the player; the rideable mech must communicate enough size and structure to carry the player. A 48×48 source canvas says nothing by itself about body size, collision footprint, or in-world scale.

Earlier tile sizes, body canvases, sprite-sheet counts, and display-resolution suggestions remain provisional. Test the player, mech, companion, a doorway, a shelf, and one loot object together at actual gameplay scale before standardizing assets. Nearest-neighbor sampling keeps pixel edges sharp, but downscaling can still remove eyes, outlines, and motion cues. Simplify a small gameplay sprite when necessary.

Keep original source art, production sprite sheets, and generated build content separate. Validate transparent padding, frame rectangles, duration, foot/origin positions, facing, attachment points, and rider occlusion. Generated images are concept inputs until those checks pass.

### 13.2 Animation priorities

Movement, interactions, reactions, and dialogue expressions need deliberate priority. A panic reaction should not be overwritten every frame by a walking state; a cosmetic bonk must not permanently block following. Separately layered riders need facing/frame-specific placement and a cockpit rim/mask where appropriate.

Use placeholders for the first lifecycle and mechanics slices. Build only the states needed to evaluate current behavior; a complete polished animation library is not a prerequisite to a working title screen or inventory.

### 13.3 Usability baseline

- Legible dialogue with adjustable text speed or instant text.
- Clear focus and input capture; closing a menu cannot also swing a tool or select a world action.
- Text/icon cues alongside color for unavailable actions and item condition.
- Readable inventory capacity, reservations, job progress, and transaction errors.
- Separate audio-volume controls once audio is present.
- Limited camera shake and reduced-motion options when such effects are introduced.
- Keyboard navigation from the first title/menu slice; additional device support follows an explicit input decision.

Use short mechanical sounds and reactions to distinguish successful, blocked, and completed actions. Audio is feedback, not the only indicator of a required event. Recorded dialogue, complex adaptive music, and large cinematic sequences are outside the baseline.

## 14. First integrated playable slice

The integrated target is reached at roadmap milestone **M08**, after smaller playable milestones have demonstrated each dependency.

The player launches to title, starts or continues, walks around `location.shop`, interacts with one customer, and optionally accepts `quest.radio_request`. They visit a simple salvage source, manage actual inventory, clean/repair an object, and sell stock. With the mech and companion available, they enter one authored mall location, recover a find, experience one readable monster encounter and companion reaction, and return. They can deliver an eligible radio, receive its reward once, save at a safe point, return to title, and continue with the same world state.

Minimum content is intentionally small: one shop room, one yard source, one repair station/recipe, one customer/conversation/request, the three distinct actors, one compact dungeon, one monster type, a small item/material set, and basic feedback. These counts are a scope proposal, not a promise that more content has been approved.

Do not require procedural generation, full town simulation, complex haggling, collateral loans, romance, a memory-recovery system, or polished final sprites to prove this loop.

### 14.1 Milestone alignment

| Milestone | Player-facing result |
| --- | --- |
| M00 | Repository/build reality established; no feature completion presumed |
| M01 | Title → New Game → minimal room → pause/resume → return/restart |
| M02 | Movement, time rules, interactions, and a placeholder scale comparison |
| M03 | Shared inventory and conserved unique-item ownership |
| M04 | Safe minimal save/load, expanded with every later persistent system |
| M05 | Local acquisition, restoration, and sale loop |
| M06 | Reusable dialogue, a request, objective semantics, and one-time rewards |
| M07 | Mounted mech control, separate rider drawing, companion following/reactions |
| M08 | Authored dungeon joins the end-to-end loop |
| M09 | Yard renewal, upgrades, day cadence, customer variety, and modest story expansion |
| M10 | Hardening and a verified release candidate |

## 15. Decisions and playtest questions

### 15.1 Open decisions with prototype defaults

| Decision | Prototype default / reason to defer |
| --- | --- |
| Final title and character names | Descriptive roles and stable placeholder IDs; avoid accidental lore commitments |
| Literal meaning of the companion's core | Cute visual motif only; no obligatory memory quest or human origin |
| Exact sprite/tile/world dimensions | Compare placeholders in-game; do not infer body scale from 48×48 canvas |
| Day length, time costs, and job durations | Configurable values; evaluate the loop before fixing numbers |
| Defeat penalty | Recoverable retreat, no automatic item deletion in first slice; tune costs later |
| Mech energy/resource model | No mandatory fuel economy until it adds a useful decision |
| Job cancellation/refund rules | Explicitly unavailable after commitment until a coherent policy is implemented |
| Pawn loans/redemption | Not implemented; direct acquisition and sale are sufficient |
| Haggling and advanced customer behavior | One clear offer/accept/reject flow first |
| Dungeon reset and procedural generation | Persist the authored run; no implicit reset on re-entry; decide expansion separately |
| Broad relationship/skill systems | Small explicit unlocks and facts; expand only when content uses them |
| Autosave and mid-dialogue resume | Safe manual saves and fact-based conversation reopening first |

### 15.2 Hypotheses to test before expanding

1. Finding a recognizable object is more satisfying than receiving an anonymous reward bundle.
2. Inspection and repair make an object's improvement understandable and worth the effort.
3. Pile maintenance changes decisions without becoming a chore tax.
4. Cargo capacity produces interesting expedition choices without excessive walking back and forth.
5. The companion is amusing in short bursts and remains readable at its actual smaller size.
6. The mech feels different from walking without making the shop or interface cumbersome.
7. A short authored request enriches normal play without forcing one exact activity order.
8. Players understand why a transaction, quest turn-in, or dismount is unavailable.

Capture observed behavior and friction, not only whether someone says the idea sounds good. Revise the relevant working baseline and corresponding contracts before adding more content to a weak loop. Detailed measurable acceptance criteria belong in [ROADMAP.md](ROADMAP.md).
