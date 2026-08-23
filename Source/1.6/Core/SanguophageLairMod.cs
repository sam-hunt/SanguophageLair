using UnityEngine;
using Verse;

namespace SanguophageLair;

// Mod entry point. Wires up settings. Deliberately Harmony-free: nothing here
// patches anything, and the planned feature set (quest site, portal, pocket-map
// generator) is achievable through defs and vanilla extension points alone. If a
// patch ever becomes unavoidable, re-add Lib.Harmony to the csproj and
// brrainz.harmony to About.xml (modDependencies + loadAfter), and read CLAUDE.md's
// patch-timing hazard before wiring PatchAll into this constructor.
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
