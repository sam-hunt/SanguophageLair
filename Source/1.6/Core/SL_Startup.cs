using Verse;

namespace SanguophageLair;

// Runs once on the main thread after all defs are loaded, translations injected and DefOf fields
// injected — the earliest point where settings that override def fields can be
// applied. (The Mod constructor is too early: it runs while mod assemblies are
// still loading, before any def exists.)
//
// Once per PROCESS, not per play-data load: StaticConstructorOnStartupUtility.CallAll goes through
// RuntimeHelpers.RunClassConstructor, and a type initializer never runs twice. An in-process
// play-data reload (mid-session language change, dev-mode def hot reload) rebuilds the DefDatabase
// WITHOUT re-running this, so until the next restart the def-field overrides revert to their
// shipped XML values (re-applied on the next settings-window close, which calls the same Apply
// methods). (Decompile-verified in the family, RimWorld 1.6.)
[StaticConstructorOnStartup]
public static class SL_Startup
{
    static SL_Startup()
    {
        SanguophageLairMod.Settings.ApplyLairQuestWeight();
    }
}
