#!/usr/bin/env python3
# SanguophageLair's config shim over the shared translation checker
# (l10n/checker/check_translations.py — the rimworld-l10n submodule). The
# engine holds all logic; this file holds only this repo's config and the
# rationale behind it. Usage is unchanged:
#   python3 Scripts/check-translations.py [--strict] [--root PATH]
# If l10n/ is empty, run: git submodule update --init

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "checker"))
import check_translations as engine  # noqa: E402  (import after sys.path edit)

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

# No [TranslationCanChangeCount]-style matching-token fields in this repo.
engine.PARITY_EXEMPT_FIELDS = set()

# RATIONALE: Biotech is a hard dependency — the mod's headline content (a
# sanguophage lair world quest site) is built on Biotech's sanguophage
# xenotype, genes, and hemogen systems, so without Biotech the mod's defs do
# not load at all. No other DLC gates any load root today.
engine.REQUIRED_DLCS = {"Biotech"}

# No subclass-declared def types rolled into a base-type database yet.
engine.DEF_TYPE_ALIASES = {}

# This mod ships a real Keyed surface (settings window strings), so a
# missing Languages/ tree is a hard config error, not a legal state.
engine.ALLOW_NO_KEYED_SURFACE = False

# The localized Steam Workshop title lives in this Keyed key (the
# settings-window header); the checker enforces the title-coupling rule
# against each .steamworkshop/Description/<Language>.txt title line.
engine.WORKSHOP_TITLE_KEY = "SL_SettingsCategory"

raise SystemExit(engine.main())
