using RimWorld;
using UnityEngine;
using Verse;

namespace SanguophageLair;

// Pocket-map end of the lair portal (thingClass of SL_AncientStairwellExit).
//
// The exit draws as two stacked layers: the def's own graphic is the stone ring (the mouth of
// the shaft, printed into the map mesh and stuff-tinted automatically), and this class draws
// the spiral stairs descending from it just below the ring. The stairs can't be a
// CompProperties_DrawAdditionalGraphics overlay because that comp draws the raw GraphicData
// (never the thing's stuff color), and this whole building is made-from-stuff: both layers
// must carry the same stone tint. Pattern copied from vanilla CaveExit, which prints its base
// graphic and draws its rope overlay in a DrawAt override without calling base (the def's
// drawerType MapMeshAndRealTime makes both paths run; a base call here would double-draw the
// ring).
//
// Linking to the entrance is vanilla: PocketMapExit.SpawnSetup grabs
// PocketMapUtility.currentlyGeneratingPortal and wires entrance/exit both ways, so the lair
// map generator only has to spawn this thing (with stuff matching the lair rock) during
// pocket-map generation.
public class AncientStairwellExit : PocketMapExit
{
    // Just below the ring (the printed base graphic at the def's altitude), so the ring's rim
    // overlaps the top of the stairs, but still above floors/filth.
    private static readonly Vector3 StairsDrawOffset = new Vector3(0f, -Altitudes.AltInc, 0f);

    [Unsaved]
    private Graphic cachedStairsGraphic;

    private Graphic StairsGraphic
    {
        get
        {
            if (cachedStairsGraphic == null)
            {
                cachedStairsGraphic = GraphicDatabase.Get<Graphic_Single>(
                    "Things/Building/Misc/AncientStairwellExit_Stairs",
                    ShaderDatabase.Cutout,
                    def.graphicData.drawSize,
                    DrawColor);
            }
            return cachedStairsGraphic;
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        StairsGraphic.Draw(DrawPos + StairsDrawOffset, Rot4.North, this);
    }
}
