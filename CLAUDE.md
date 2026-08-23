# CLAUDE.md

Guidance for Claude Code (claude.ai/code) when working in this repository.

## Project Overview

**Sanguophage Lair** is a RimWorld 1.6 mod adding a rare world quest site: a sanguophage lair
entered through a custom portal onto a generated pocket map (a custom MapGeneratorDef subtree).
Requires the Biotech DLC (hard dependency — the lair is built on Biotech's
sanguophage xenotype, genes, and hemogen systems).

**Key technologies:** C# (.NET Framework 4.7.2), RimWorld modding API, XML defs.

### Where documentation lives

**This file holds only cross-cutting rules and rationale.** Per-item values, tuning numbers and
decompile-verified call paths live in the header comment of the file they describe — every `.cs`
file and every non-obvious def carries one. When adding or changing something, put the *why*
there and only add a line here if it constrains work in other files. Do not restate def values
or call paths here; they drift.

## Build Commands

```bash
# Build the mod (outputs to 1.6/Assemblies/ AND atomically redeploys to the RimWorld Mods folder)
dotnet build SanguophageLair.sln -c Release

# Build only the main project (also triggers the deploy)
dotnet build Source/1.6/SanguophageLair.csproj

# Run tests
dotnet test Tests/1.6/SanguophageLair.Tests.csproj

# Clean build artifacts
dotnet clean SanguophageLair.sln

# Stage the mod into an arbitrary folder (used by CI; same manifest as the local deploy)
dotnet build Source/1.6/SanguophageLair.csproj -c Release \
  -t:StageMod -p:StageDir=/path/to/output/SanguophageLair

# Override RimWorld install path
RIMWORLD_PATH="/path/to/RimWorld" dotnet build SanguophageLair.sln -c Release
# Or: dotnet build -p:RimWorldPath="/path/to/RimWorld"
```

The build system auto-detects the RimWorld installation path on Windows/Linux/Mac (including WSL
targeting a Windows install). For CI builds without RimWorld installed, it falls back to the
`Krafs.Rimworld.Ref` NuGet package. For local development and api inspection (monodis, ilspycmd
etc), the local installation should be preferred as the source of truth.

### Deployment

The repo lives in `~/dev/SanguophageLair`, separate from the RimWorld Mods folder. Every local
build redeploys automatically and atomically — there is no separate clean step to remember.

- **Single source of truth:** what ships is the `_ModFiles` ItemGroup in the `StageMod` target
  (`Source/1.6/SanguophageLair.csproj`) — the only place to edit the manifest. It whitelists by
  file type per content folder (`About`, `Assemblies`, `Defs`, `Patches`, `Languages`, plus
  `Textures`/`Sounds` if ever added), matched at the root, under any version folder, and (for
  `Defs`/`Languages`) under gated compat load roots (`<version>/Mods/<Mod Name>/`, see
  LoadFolders.xml), so a new content folder of an existing type deploys automatically and only a
  brand-new file type needs a new line. Only game-loaded types are listed (e.g. `.xml`), so stray
  dev notes (`README.md`, `RESEARCH.md`) never ship.
- **Self-cleaning:** `StageMod` wipes `$(StageDir)` and recopies from source, so renamed/deleted
  files never linger. The post-build `DeployToModFolder` target calls it with
  `StageDir = $RIMWORLD_PATH/Mods/SanguophageLair` (only when a local RimWorld install is
  detected).
- **CI reuses the same target:** `.github/workflows/release.yml` invokes `StageMod` with
  `-p:StageDir=<release dir>` instead of its own `cp` list, so the release zip can't drift from
  the local deploy. Triggers on `v*.*.*` tags.
- **Stop hook (`.claude/hooks/sync-mod.sh`, gitignored/local-only):** after each turn,
  rebuilds+redeploys only when mod source/content actually changed (doc-only turns are a fast
  no-op) and warns on build failure rather than leaving a stale DLL. Mechanism details are in the
  script's own header.

**Releases:** Push a tag matching `v*.*.*` to trigger the release workflow
(`.github/workflows/release.yml`), or use the `release` skill which walks the whole process.

**`.claude/` is only partly gitignored.** `.gitignore` carries `.claude/*` followed by
`!.claude/skills/`, so the skills are tracked and shared while hooks and settings are local
per-machine. Editing a skill is therefore a committed, team-visible change and must keep in step
with whatever it automates: `/release`'s step 5 encodes this repo's CHANGELOG layout, and
`/translate`'s glossary encodes per-language terminology decisions. Changing the thing without
changing the skill leaves an instruction pointing at something that no longer exists, and nothing
fails until the next release run.

**WSL Setup:** Requires `RIMWORLD_PATH` env var in `~/.bashrc` pointing to the Windows RimWorld
install (e.g., `/mnt/c/Program Files (x86)/Steam/steamapps/common/RimWorld`). The csproj
auto-detects `RimWorldWin64_Data` when the Linux data folder isn't found.

### Tests

xUnit suite under `Tests/1.6/` (a separate project, never shipped). Run natively:

```bash
dotnet test Tests/1.6/SanguophageLair.Tests.csproj
```

vstest hosts the net472 suite via mono automatically. CI builds but doesn't run it.

Run tests natively from WSL — never build from the Windows toolchain: it corrupts the WSL-side
incremental state (shared `obj/` seen under different path roots). The test csproj copies the
live install's runtime DLLs beside the test DLL so tests may load Verse types (see the
Assembly-CSharp-firstpass comment there — mono resolves field types eagerly where the Windows CLR
is lazy); `OutputPath` is Release-gated so Debug `dotnet test` builds can't overwrite the shipped
DLL in `1.6/Assemblies`.

If a native test failure looks runtime-flavored, suspect assembly resolution first: a DLL missing
from the test bin copy target throws `BadImageFormatException`/`TypeLoadException` under mono
only — and one such failure can poison static state and surface as unrelated value mismatches
downstream.

**Startup smoke test (pre-release):** `python3 Scripts/integration-smoke-test.py` (game closed)
boots the mod on its pinned minimal list, then classifies Player.log errors by origin and fails on
anything attributed to this mod. Run before every release (wired into the release skill); thin
shim over the shared engine in `l10n/smoke/` (born from the BetterTradersGuild v1.1.0 CWTL
incident).

## Architecture

### Entry Point

`Source/1.6/Core/SanguophageLairMod.cs` — `Mod` subclass whose constructor wires settings.
`Source/1.6/Core/SL_Startup.cs` (`[StaticConstructorOnStartup]`) runs after defs load and applies
settings that override def fields. **Settings access:** `SanguophageLairMod.Settings`.

**Harmony-free by policy:** the mod ships no Harmony wiring at all — no package reference, no
About.xml dependency, no `PatchAll`. The planned feature set is achievable through defs and
vanilla extension points. If a patch ever becomes genuinely unavoidable, the same change that
adds the first patch class must re-add Lib.Harmony (`ExcludeAssets=runtime`) to the csproj and
`brrainz.harmony` to About.xml (modDependencies + loadAfter), and observe the hazard below. The
patch-pattern rules later in this file (Prefix discipline, private patch targets, `*Patches`
namespaces) apply from that moment.

**Patch-timing hazard (other mods' methods):** `PatchAll()` run from a `Mod` subclass
constructor executes BEFORE any defs are loaded. Applying a detour JIT-compiles the target and
runs its declaring type's static ctor, so a patch targeting ANOTHER MOD's method can permanently
break that mod when its cctor resolves defs (the BetterTradersGuild v1.1.0 CWTL incident). If
patches ever land here, keep foreign-target patches off the ctor-time pass — worked example:
BetterTradersGuild's `Core/DeferredModPatches.cs`.

### Naming conventions

- Def prefix and Keyed prefix: `SL_`. Defs live at `1.6/Defs/<DefTypeName>Defs/<Name>.xml`, one
  def (or small tight group) per file. Patches at `1.6/Patches/<Target>_<OptionalModSuffix>.xml`.
- English Keyed strings: `1.6/Languages/English/Keyed/SL_UI.xml`; split by surface
  (`SL_Combat.xml`, ...) only when a second surface appears.
- Settings are partial classes: `Core/SanguophageLairSettings.cs` owns the window frame and row
  helpers; `Core/Settings/Settings_<Area>.cs` each own their section's fields, scribe entries,
  defaults, Apply* def-writes and draw method.
- DefOf handles: `Core/SL_DefOf.cs` for general defs; a feature-scoped `SL_<Feature>DefOf` beside
  the feature when one grows large (UMW precedent). Only add defs the code actually looks up,
  each field commented with its exact C# consumer.

### Key Patterns

**Namespace Convention:** Use `*Patches` suffix for patch namespaces to avoid RimWorld type
conflicts (e.g., `SitePatches`, `MapGenerationPatches`).

**Comments:** In `Source/`, use plain `//` comments only — do **not** write XML doc comments
(`///`, `<summary>`, `<param>`, etc.); nothing consumes them there, so they add ceremony without
benefit. `Tests/` is exempt: XML doc comments are fine there (they read cleanly on the test
helpers and are the most likely place to adopt a tool that parses them).

**No `?.`/`??` on Unity objects:** Never use null propagation or null coalescing on receivers
deriving from `UnityEngine.Object` (`Material`, `Texture`, `RenderTexture`, `GameObject`, ...).
Unity overloads `==` so destroyed objects compare equal to null; `?.` bypasses the overload with
a raw reference check and then throws `MissingReferenceException` on the member access. Use
explicit `== null`/`!= null` guards for those types. Verse types (`Thing`, `Pawn`, `ThingComp`,
defs) are plain classes where `?.` is fine. Enforced at build time by UNT0007/UNT0008
(Microsoft.Unity.Analyzers). Corollary: never bulk-apply Roslynator's RCS1146 (use conditional
access) fixer to Unity-typed receivers; see the note in `.editorconfig`.

**Logging:** Prefix mod-specific logs with the mod name — `Log.Message("[Sanguophage Lair] ...")`.

**Serialized Fields:** Use camelCase for fields serialized via `Scribe_Values.Look` to match save
file XML element names (per .editorconfig). PascalCase for all other public members.

**Settings Triple Invariant:** Every settings field must appear in three places with matching
defaults: (1) field declaration, (2) `Reset*Settings()`, (3) `Expose*Settings()`'s
`Scribe_Values.Look` default. Missing a spot fails silently — drops from save, skips reset, or
drifts from declared default. Each section's partial file keeps all three in the UI's display
order so a diff across the three blocks lines up row-for-row.

**Label casing (vanilla convention):** thing/def labels placed mid-sentence in player-facing text
use the lowercase form (`LabelShort`, `.label`) — vanilla renders "Pick up revolver x1", never
"Pick up Revolver x1". Keyed strings carry their own sentence-start capital; where a `{0}`
placeholder can begin the sentence, `CapitalizeFirst()` the composed string instead of
capitalizing the argument. `LabelCap`/`LabelShortCap` is for standalone display (list rows, name
fields) and proper nouns.

**No em dashes in player-facing text** (def labels/descriptions, `Keyed/`, `About.xml`) — reflow
the sentence instead. This file, code comments and def comments are unaffected.

**Prefix discipline:** Prefer Postfix. A Prefix should only set flags or pre-align state,
returning void/true. Skipping the original (`return false`) is a mod-compat hazard — other mods'
transpilers and the original's side effects silently die with it — so before writing one, check
whether an additive Postfix can express the change. When a skip is genuinely unavoidable, scope
it tightly to SL-owned defs/state and preserve any vanilla effects other patches may rely on.

**Private patch targets:** resolve via a cached `AccessTools.Method` with
`[HarmonyPrepare]`/`[HarmonyTargetMethod]`, never a string-named attribute: on API drift, Prepare
skips just that one patch instead of `PatchAll` throwing and aborting every later patch.

**Reflection self-checks:** when the first string-named reflection site lands, adopt the family
pattern (see BetterTradersGuild's `Core/ReflectionVerification.cs` and its CLAUDE.md section):
each reflecting class caches its own `FieldInfo`/`MethodInfo` and exposes `VerifyReflection()`;
one central `VerifyAll()` triggers them from `SL_Startup` so API drift surfaces at startup.

### Headline feature: the lair quest site

Partially built. The planned shape: a rare opportunity-site quest (`QuestScriptDef` +
`SitePartDef` + `WorldObjectDef`) places the lair on the world map; the surface map holds a
custom portal building (a `MapPortal` subclass) that generates and links a pocket map; a
dedicated `MapGeneratorDef` subtree (custom `GenStepDef`s, layout defs) builds the lair itself.

**Shipped so far:** the portal pair (`SL_AncientStairwell`/`SL_AncientStairwellExit`, stony-
stuffable — `1.6/Defs/ThingDefs/AncientStairwell.xml` + `Source/1.6/MapPortals/`) and a
scaffold pocket-map generator (`SL_Lair`, a bare Core-only cave) — decompile-verified call
paths and the stuff/two-layer-draw rationale live in those files' headers. Still pending: the
quest/site defs, the real lair `MapGeneratorDef` subtree (which replaces the scaffold's
`PlaceCaveExit` with a genstep spawning `portal.exitDef` with lair-matched stuff), and the
site GenStep that spawns the entrance with its stone.

**In-family precedent — read before designing:** BetterTradersGuild's smuggler's den quest →
cargo vault pocket map is the full worked example
(`../BetterTradersGuild/Source/1.6/`): `QuestNodes/QuestNode_BTG_SmugglersDen_CreateSite.cs`,
`MapPortals/CargoVaultHatch.cs` (a `MapPortal` that overrides `GetExtraGenSteps` and tears the
pocket map down on `DeSpawn`), `LayoutWorkers/CargoVault/` (vanilla layout workers fail at
pocket-map sizes), `GenSteps/`, and the `Patches/MapParent/MapParentMapGeneratorDef.cs` swap.
UniqueMeleeWeapons' warband quest (`../UniqueMeleeWeapons/Source/1.6/Quests/`) is the cleaner
minimal opportunity-site quest. The quest-weight settings dial is already wired
(`Core/Settings/Settings_Quests.cs`) and switches from `GetNamedSilentFail` to an `SL_DefOf`
handle once the quest def ships.

## Localization

English (Keyed files + def fields) is the source of truth; other languages derive from it via the
`/translate` skill (`.claude/skills/translate/SKILL.md` — this mod's translation surface,
grounding domain, and glossary; family-wide process lives in the `l10n/` submodule, see below)
and are validated deterministically by `python3 Scripts/check-translations.py` (also a CI release
gate). The DefInjected expected set is the checked-in sidecar `Scripts/expected-injections.json`:
a dump of every injection point the *live* game sees for this mod — including vanilla-inherited
fields and C#-default comp strings that never appear in this repo's XML — produced by
`Scripts/refresh-translation-expectations.py` driving the L10nProbe dev mod (source lives at
`l10n/probe/`; build/deploy it only from the canonical `~/dev/rimworld-l10n` checkout) through
the game's own walker. The checker refuses to run against stale expectations, so new content
forces a regen; the release skill regenerates every release. The public language roster lives in
CONTRIBUTING.md and must move in the same commit as any language change. New-machine note: the
probe only dumps mods ticked in its own settings
(`Config/Mod_L10nProbe_L10nProbeMod.xml` on the Windows side) — an absent dump despite a correct
pinned list means the packageId isn't registered there.

- **Shared l10n toolkit (`l10n/` submodule):** the family-wide translation process, per-language
  mechanics references, cross-language lessons, Workshop conventions, and the checker/refresh
  script engines live in the `rimworld-l10n` repo, consumed here as the `l10n/` git submodule
  (canonical working checkout: `~/dev/rimworld-l10n`). `Scripts/check-translations.py` and
  `Scripts/refresh-translation-expectations.py` are thin per-repo config shims over its engines.
  If `l10n/` is empty, run `git submodule update --init`. Never edit `l10n/` in place here:
  mod-independent learnings go upstream in the canonical checkout, then the pin is bumped in each
  consuming repo; mod-specific learnings go in this repo's skill/glossary.
- **Workshop title coupling:** each language's `SL_SettingsCategory` Keyed value is the localized
  Steam Workshop title and must equal the title line (line 1) of
  `.steamworkshop/Description/<Language>.txt` — always change the two together (English keeps
  `Sanguophage Lair` in both).
- **Optional-DLC content:** MayRequire is honored on defs but IGNORED on DefInjected entries, so
  content whose strings depend on an optional DLC/mod must ship from a LoadFolders-gated compat
  root (`1.6/Mods/<Name>/` with its own `Defs`/`Languages` inside). None exist today — Biotech is
  a hard dependency — but a compat root's language files must never reuse a main-tree file's
  language-relative path (the game dedups per mod by that path and silently skips one whole
  file); suffix compat-root filenames with the gate's name.
- **Policy:** translation generation passes run only on explicit request (they are
  token-expensive), one language at a time. Infra/tooling changes are always fine.

## Debugging

For reading `Player.log` or disassembling the RimWorld API, use the `rimworld-logs` skill.
Logging convention: `Log.Message("[Sanguophage Lair] ...")`.
