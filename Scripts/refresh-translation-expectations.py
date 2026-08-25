#!/usr/bin/env python3
# SanguophageLair's config shim over the shared sidecar-refresh engine
# (l10n/refresh/refresh_expectations.py — the rimworld-l10n submodule),
# which drives the L10nProbe dev mod (source at l10n/probe/; build/deploy it
# only from the canonical ~/dev/rimworld-l10n checkout). The engine holds all
# logic; this file holds only this repo's config and the rationale behind it.
# Usage is unchanged (game must be closed):
#   python3 Scripts/refresh-translation-expectations.py [--no-launch]
# If l10n/ is empty, run: git submodule update --init

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "refresh"))
import refresh_expectations as engine  # noqa: E402  (import after sys.path edit)

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.sanguophagelair"

# RATIONALE: Biotech is the mod's only hard dependency (the sanguophage lair
# quest site is built on Biotech's sanguophage xenotype, genes, and hemogen
# systems; without Biotech the defs do not load). Odyssey is the one optional
# seam: the landmark tile mutator (a labelled def) lives in the
# 1.6/Mods/Odyssey compat root, so its injection points only exist for the
# probe when Odyssey is active. The mod is Harmony-free by
# policy (see CLAUDE.md), so brrainz.harmony is deliberately absent. This
# repo is standalone — it is not part of a sibling family that boots
# together — so the list is the minimal deterministic set. See the engine's
# header for the general membership rule, the lowercase-id warning, and the
# pinning rationale; order is load order, the probe last.
engine.CANONICAL_ACTIVE_MODS = [
    "ludeon.rimworld",
    "ludeon.rimworld.biotech",
    "ludeon.rimworld.odyssey",
    "shunter.sanguophagelair",
    "shunter.l10nprobe",
]

raise SystemExit(engine.main())
