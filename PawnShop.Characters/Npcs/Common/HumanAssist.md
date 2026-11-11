# Working on common NPC appearances
Start with [AUTHORING.md](AUTHORING.md): it documents the working asset layout, outfit/hair/hat additions, compatibility, examples, and checker.

The catalog and seeded generator realize ordinary shoppers/townies from approved body-specific parts. Stable identity is supplied separately from the random seed. Main-player and notable-character art are separate.

Edit Characters-owned Assets and metadata, not Game output copies. Keep provenance, same-body fits, pose clocks, mirroring, and hair/headgear compatibility explicit. General browsing, trade AI, schedules, and saves remain future work.

After the normal root build, validate with:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets common PawnShop.Characters/Npcs/Common/Assets
```

Then run the ordinary regression suite for affected code/content. Numeric checks do not grant art approval; review all required poses at native size.
