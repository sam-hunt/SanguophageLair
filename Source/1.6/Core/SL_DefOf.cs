using RimWorld;
using Verse;

namespace SanguophageLair;

// Static handles for this mod's own defs that C# needs to reference by identity. Populated by
// RimWorld at startup (fields match defName). Only add defs the code actually looks up, each with
// a comment naming its exact C# consumer; defs referenced only from XML never get a handle here.
// DLC-gated or MayRequire-gated defs carry the matching [MayRequireX] attribute so DefOf init
// doesn't error when the gate is closed (the field stays null; consumers null-guard).
//
// Expected later residents: the lair QuestScriptDef and SitePartDef (see Settings_Quests.cs,
// which switches from GetNamedSilentFail to a handle here once the quest def ships).
[DefOf]
public static class SL_DefOf
{
    // GenStep_PlaceStairwellExit: fallback exit when no portal is generating the pocket map.
    public static ThingDef SL_AncientStairwellExit;

    // TileMutatorWorker_SanguophageLair: the surface pad spawned around the stairwell entrance.
    public static PrefabDef SL_StairwellSurface;

    static SL_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SL_DefOf));
    }
}
