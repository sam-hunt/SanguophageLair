using RimWorld;

namespace SanguophageLair;

// Static handles for this mod's own defs that C# needs to reference by identity. Populated by
// RimWorld at startup (fields match defName). Only add defs the code actually looks up, each with
// a comment naming its exact C# consumer; defs referenced only from XML never get a handle here.
// DLC-gated or MayRequire-gated defs carry the matching [MayRequireX] attribute so DefOf init
// doesn't error when the gate is closed (the field stays null; consumers null-guard).
//
// Empty until the first defs land. Expected early residents: the lair QuestScriptDef and
// SitePartDef, the portal ThingDef, and the pocket-map MapGeneratorDef (see
// Settings_Quests.cs, which switches from GetNamedSilentFail to a handle here once the
// quest def ships).
[DefOf]
public static class SL_DefOf
{
    static SL_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SL_DefOf));
    }
}
