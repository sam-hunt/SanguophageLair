#!/usr/bin/env python3
# Pre-release startup smoke test: boots the real game once with SL plus its
# hard dep, on a pinned minimal list where the baseline is a clean log, then
# classifies every Player.log error/warning by origin and fails on anything
# attributed to SL. Thin shim over the shared engine in
# l10n/smoke/startup_smoke.py (see its header for mechanics and the
# BetterTradersGuild v1.1.0 CWTL incident this exists to catch).
#
# Run this before every release, with the game closed:
#   python3 Scripts/integration-smoke-test.py              # boot + scan
#   python3 Scripts/integration-smoke-test.py --no-launch  # rescan last log
#   python3 Scripts/integration-smoke-test.py --strict     # any error fails

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "smoke"))
import startup_smoke as engine  # noqa: E402

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.sanguophagelair"

# RATIONALE: this is exactly SL's l10n CANONICAL_ACTIVE_MODS - Biotech is the
# hard dep the lair's sanguophage xenotype/genes/hemogen systems are built
# on; the mod is Harmony-free by policy (see CLAUDE.md). No optional
# integration mods exist yet, so there is no seam to name; this is a
# clean-startup-log gate. Probe last (auto-quit).
engine.SMOKE_ACTIVE_MODS = [
    "ludeon.rimworld",
    "ludeon.rimworld.biotech",
    "shunter.sanguophagelair",
    "shunter.l10nprobe",
]

engine.OWN_PATTERNS = ["SanguophageLair", "SL_"]

# No integration mods yet - every gated error is therefore attributed to SL.
engine.INTEGRATION_PATTERNS = {}

raise SystemExit(engine.main())
