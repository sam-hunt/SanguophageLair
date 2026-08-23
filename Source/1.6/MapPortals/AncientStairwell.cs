using RimWorld;

namespace SanguophageLair;

// Surface end of the lair portal (thingClass of SL_AncientStairwell).
//
// Vanilla MapPortal is concrete and already does everything the def needs today:
// GetOtherMap() generates the pocket map from def.portal (pocketMapGenerator/pocketMapSize)
// via PocketMapUtility, the enter gizmo + Dialog_EnterPortal are base behavior, the optional
// first-entry letter comes from def.portal.enteredLetter* fields, and teardown is vanilla too
// (Game.DeinitAndRemoveMap destroys any pocket map whose sourceMap is the map being removed,
// so abandoning the site can't leak the lair map).
//
// This subclass exists NOW, while still empty, because saved things record their concrete C#
// class: shipping the def on vanilla MapPortal and swapping thingClass later would strand
// already-spawned portals in old saves on the vanilla class. Lair-specific portal behavior
// (entry gating, quest hooks) lands here when it's needed.
//
// GetExtraGenSteps is deliberately left at its default (empty): all pocket-map generation
// belongs to the SL_Lair MapGeneratorDef, so the map's content is defined in exactly one
// place (BetterTradersGuild's CargoVaultHatch learned this against vanilla AncientHatch's
// injected gensteps).
public class AncientStairwell : MapPortal
{
}
