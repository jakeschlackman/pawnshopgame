# Ink 1.2.1

Pinned upstream dependency for the user-requested Inky/Ink conversation engine.
The unmodified managed assemblies were extracted from the official Windows release:
https://github.com/inkle/ink/releases/tag/v1.2.1

Download: https://github.com/inkle/ink/releases/download/v1.2.1/inklecate_windows.zip

| File | SHA-256 |
| --- | --- |
| Release ZIP (not committed) | `96BC130F57D134FAF3D52019F36CE0879EA015FA7E84D280CCC1A9C8D376843F` |
| `ink-engine-runtime.dll` | `8AB9E923E6563AC94E5A2A9289FD8FE4F97ACCCF1C20CD9AA05C4E20E250D51A` |
| `ink_compiler.dll` | `930A4BD20D8927C97551D05677DE5FBE512341165FD019B4FCB74BAA35722D1E` |

Source: https://github.com/inkle/ink/tree/v1.2.1
License: unmodified MIT license from that tag, included as LICENSE.txt and copied
into game output under Content/Licenses/Ink.txt.

Narrative references only the runtime. MSBuild's Ink task uses the compiler for
normal content builds; Tests reference it for authoring checks. The game does not ship the compiler or the
downloaded Windows executable. No extra solution projects, package feeds, Unity
integration, or system installation are needed. Normal builds are offline with
respect to Ink; existing MonoGame dependencies retain their own restore workflow.

Both assemblies must be updated together in an explicitly reviewed dependency
change. Recompile story assets and rerun narrative checks when updating Ink.
