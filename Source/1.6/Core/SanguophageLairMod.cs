using HarmonyLib;
using UnityEngine;
using Verse;

namespace SanguophageLair;

// Mod entry point. Wires up settings and applies all Harmony patches at startup.
// Add patch classes under the SanguophageLair.Patches namespace (use a *Patches
// suffix on patch sub-namespaces to avoid RimWorld type conflicts); PatchAll
// discovers them automatically via their [HarmonyPatch] attributes.
public class SanguophageLairMod : Mod
{
    public static SanguophageLairSettings Settings { get; private set; }

    // This mod's own content pack, so code can ask whether a def is ours without a
    // defName convention.
    public static ModContentPack ContentPack { get; private set; }

    public SanguophageLairMod(ModContentPack content) : base(content)
    {
        ContentPack = content;
        Settings = GetSettings<SanguophageLairSettings>();
        var harmony = new Harmony("shunter.sanguophagelair");
        harmony.PatchAll();
        Log.Message($"[Sanguophage Lair] Initialized with {harmony.GetPatchedMethods().EnumerableCount()} patches.");
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoWindowContents(inRect);
    }

    // Called when the settings window closes. Settings that override def fields
    // re-apply here so a change takes effect without a restart.
    public override void WriteSettings()
    {
        base.WriteSettings();
        Settings.ApplyLairQuestWeight();
    }

    public override string SettingsCategory() => "SL_SettingsCategory".Translate();
}
