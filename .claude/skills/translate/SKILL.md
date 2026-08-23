---
name: translate
description: Generate, update, or audit mod localization (Keyed + DefInjected) for a target language, grounded in vanilla + Biotech RimWorld sanguophage terminology for Sanguophage Lair's quest-site / pocket-map domain. Use when asked to add a language, update translations, or check translation freshness.
argument-hint: "[language, e.g. German | update | check]"
---

# Translate

Produce or refresh localization files for Sanguophage Lair. English is
the source of truth; every other language derives from it.

**The family-wide process lives in the `l10n/` submodule — load these first,
and only these** (progressive disclosure; if `l10n/` is empty, run
`git submodule update --init`):

- `l10n/process.md` — non-negotiables, file/format conventions, terminology
  grounding method, and the generation / update / audit workflows. This is
  the workflow authority; follow it step by step.
- `l10n/languages/<Language>.md` — the target language's engine mechanics,
  style rules, and vanilla-grounded common vocabulary. Read ONLY the target
  language's file.
- `glossary/<Language>.md` (beside this file) — this mod's own coined-term
  table for the target language. Read it in the same pass.
- `l10n/lessons.md` — cross-language lessons; read when generating a new
  language, skim otherwise.
- `l10n/workshop.md` — Steam Workshop description/title conventions;
  `.steamworkshop/README.md` names this mod's anchor term and title-coupling
  key (`SL_SettingsCategory`).

**Where learnings land:** mod-independent findings (engine mechanics, a
language's grammar rule, corpus style facts) go in the `l10n/` submodule —
edit the canonical checkout at `~/dev/rimworld-l10n`, commit there, then bump
the pin here. Mod-specific findings (coined terms, phrasing decisions) go in
`glossary/<Language>.md`.

## This mod's translation surface

- English Keyed source: `1.6/Languages/English/Keyed/SL_UI.xml` (settings
  window). Every key is `SL_`-prefixed. There is no second Keyed file yet.
- Player-facing def text will live in the defs themselves (`1.6/Defs/**`)
  and is translated per language via DefInjected, not Keyed. There is no
  English DefInjected tree at all (English is served by the def XML's own
  `<label>`/`<description>`), so **enumerate the DefInjected target key set
  from the `Scripts/expected-injections.json` sidecar, never from
  `1.6/Languages/English/`**.
- No def types are shipped yet — no defs have landed, so the sidecar is
  currently empty and is regenerated as content lands. If this mod later
  defines its own Def subclasses, their DefInjected folders would need
  namespace-qualified names (`SanguophageLair.<DefClass>`), unlike vanilla
  def types, which resolve bare.
- **No gated compat load roots today** — Biotech is a hard dependency of this
  mod rather than an optional compat surface, so there is no MayRequire-gated
  `1.6/Mods/<Package>/Languages/...` subtree to route translations into.

## This mod's grounding domain

Domain DLC: **Biotech** (plus Core) — the source for this mod's headline
sanguophage-lair quest site. Terms that MUST be grounded before use:
sanguophage, xenotype, hemogen, deathrest, thrall/blood-feeder vocabulary,
gene terms, and quest-site vocabulary (quest, site, opportunity site,
portal, pocket map). The vanilla-grounded answers live in
`l10n/languages/<Language>.md`; this mod's coined terms (lair and portal
vocabulary, def-label phrasing decisions, ...) live in
`glossary/<Language>.md`. No languages are shipped yet — every language
starts from nothing and gets its terms grounded and recorded per
`l10n/process.md`.

## Workflows

Follow `l10n/process.md`'s Initial generation / Update pass / Audit-only
workflows verbatim. This mod's specifics on top:

- The checker: `python3 Scripts/check-translations.py` (`--strict` for new
  languages). Sidecar regen: `python3
  Scripts/refresh-translation-expectations.py` (game must be closed; drives
  the deployed L10nProbe).
- No compat-root routing today (see the surface section above).
- `SL_SettingsCategory` is that language's localized Workshop title and
  must stay in sync with the title line of
  `.steamworkshop/Description/<Language>.txt` — change both together.
- The public roster (and credits) is CONTRIBUTING.md's localization table —
  update it in the same commit as any language addition or native review.
