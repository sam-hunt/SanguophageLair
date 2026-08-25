using System.Linq;
using RimWorld;
using Verse;

namespace SanguophageLair;

// GenStep of SL_PlaceStairwellExit (1.6/Defs/GenStepDefs/PlaceStairwellExit.xml), the SL_Lair
// pocket map's exit-placement step. Spawns the portal's declared exit (def.portal.exitDef, i.e.
// SL_AncientStairwellExit) in the same stuff as the entrance that is generating this map, and
// makes it the player start spot, which is where JobDriver_EnterPortal drops arriving pawns
// (MapPortal.GetDestinationLocation returns exit.Position; the PocketMapExit parent def is
// Standable, so the 7x7 foot of the stairs is itself the arrival area).
//
// Replaces vanilla GenStep_PlaceCaveExit in the SL_Lair genstep list, which hardcodes the
// 3x3 rope CaveExit (decompile-verified, 1.6): same algorithm, resized for a 7x7 exit and
// reading the exit def and stuff from the generating portal instead of a constant.
// - Site: a random standable cell far enough from the map edge that the whole exit plus the
//   clearing fits (PlaceCaveExit uses 5.5 for a 3x3 exit; 8.5 here). Standable at the center
//   is enough because everything destroyable in the clearing radius is removed before the
//   spawn, rock walls included, exactly as PlaceCaveExit does; the pocket map's rough rock
//   floor already has the Heavy affordance the exit needs.
// - Linking is vanilla: PocketMapExit.SpawnSetup reads PocketMapUtility.currentlyGeneratingPortal
//   and wires entrance.exit / exit.entrance, so this step only has to spawn the thing while
//   that static is set, which is the case for the whole of MapPortal.GeneratePocketMap.
// - No generating portal (the dev-menu "GeneratePocketMap" action on SL_Lair, or a future
//   caller that forgets the portal) falls back to SL_AncientStairwellExit in the default stony
//   stuff so the map is still generated and walkable; PocketMapExit.SpawnSetup logs its own
//   "could not find map portal" error for that case.
public class GenStep_PlaceStairwellExit : GenStep
{
    // Radius covering a 7x7 footprint (half-diagonal ~4.95) plus a two-cell apron so the foot
    // of the stairs opens onto floor rather than into a rock face.
    private const float ClearRadius = 6.5f;

    // Center cell distance to the map edge: ClearRadius plus the two-cell margin PlaceCaveExit
    // keeps beyond its own clearing.
    private const float MinDistToEdge = 8.5f;

    public override int SeedPart => 811640213;

    public override void Generate(Map map, GenStepParams parms)
    {
        MapPortal entrance = PocketMapUtility.currentlyGeneratingPortal;
        ThingDef exitDef = entrance?.def.portal?.exitDef ?? SL_DefOf.SL_AncientStairwellExit;
        ThingDef stuff = entrance?.def.MadeFromStuff == true
            ? entrance.Stuff
            : GenStuff.DefaultStuffFor(exitDef);

        if (!CellFinder.TryFindRandomCell(map, c => c.Standable(map) && c.DistanceToEdge(map) > MinDistToEdge, out IntVec3 site))
        {
            // Every cell failed (a map made entirely of rock); take the center and let the
            // clearing below carve the room. Same last resort as PlaceCaveExit's unchecked
            // TryFindRandomCell result.
            site = map.Center;
        }

        foreach (IntVec3 c in GenRadial.RadialCellsAround(site, ClearRadius, useCenter: true))
        {
            if (!c.InBounds(map))
            {
                continue;
            }
            foreach (Thing t in c.GetThingList(map).ToList().Where(t => t.def.destroyable))
            {
                t.Destroy();
            }
        }

        GenSpawn.Spawn(ThingMaker.MakeThing(exitDef, exitDef.MadeFromStuff ? stuff : null), site, map);
        MapGenerator.PlayerStartSpot = site;
    }
}
