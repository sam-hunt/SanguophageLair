using RimWorld;
using Verse;

namespace SanguophageLair;

// "Quests" settings section: how often the sanguophage lair site is offered.
public partial class SanguophageLairSettings
{
    // The lair quest's QuestScriptDef defName. Looked up by name until the def ships;
    // once it exists, move the handle into SL_DefOf and reference it by identity.
    private const string LairQuestDefName = "SL_OpportunitySite_SanguophageLair";

    // Selection weight of the lair opportunity-site quest. This is the real default;
    // the XML rootSelectionWeight is overwritten by ApplyLairQuestWeight at startup,
    // so the two only need to agree for documentation's sake.
    public const float LairQuestWeightDefault = 0.4f;
    public const float LairQuestWeightMin = 0f;
    public const float LairQuestWeightMax = 2f;
    public float lairQuestWeight = LairQuestWeightDefault;

    private void ExposeQuestSettings()
    {
        Scribe_Values.Look(ref lairQuestWeight, "lairQuestWeight", LairQuestWeightDefault);
    }

    private void ResetQuestSettings()
    {
        lairQuestWeight = LairQuestWeightDefault;
    }

    // Writes the configured weight onto the live quest def. rootSelectionWeight is
    // read fresh from the def on every opportunity-site roll, so a def-field write
    // is all an override takes. Called after defs load (SL_Startup) and whenever
    // the settings window closes (SanguophageLairMod.WriteSettings).
    // GetNamedSilentFail because the def does not ship yet; this section is the
    // wired-up skeleton the real quest content lands into.
    public void ApplyLairQuestWeight()
    {
        QuestScriptDef lairQuest = DefDatabase<QuestScriptDef>.GetNamedSilentFail(LairQuestDefName);
        if (lairQuest != null)
        {
            lairQuest.rootSelectionWeight = lairQuestWeight;
        }
    }

    private void DrawQuestsSection(Listing_Standard listing)
    {
        // Vanilla-reuse rule: the quests main-tab button's def label is the localized word
        // "quests" in every language, capitalized by LabelCap exactly as the bottom bar
        // renders it. There is no vanilla Keyed key for it (the tab is a MainButtonDef).
        SectionHeader(listing, MainButtonDefOf.Quests.LabelCap);

        // Subject falls back to our own Keyed name until the quest def ships; switch to the
        // site's def label then, so the name tracks that def's translation (UMW precedent:
        // Settings_Quests.cs injects the warband faction's LabelCap).
        QuestScriptDef lairQuest = DefDatabase<QuestScriptDef>.GetNamedSilentFail(LairQuestDefName);
        string subject = lairQuest?.LabelCap ?? "SL_LairQuest".Translate().CapitalizeFirst();

        lairQuestWeight = SliderRow(
            listing, "SL_QuestWeight", "SL_LairQuestWeightDesc",
            subject,
            lairQuestWeight, LairQuestWeightDefault,
            min: LairQuestWeightMin, max: LairQuestWeightMax, step: 0.05f, format: "0.00");

        listing.Gap(SectionGap);
    }
}
