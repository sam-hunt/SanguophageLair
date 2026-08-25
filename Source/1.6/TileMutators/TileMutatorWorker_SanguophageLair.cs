using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SanguophageLair;

// Worker of the SL_SanguophageLair tile mutator (Odyssey compat root,
// 1.6/Mods/Odyssey/Defs/TileMutatorDefs/). When a world tile carries the mutator, every map
// generated on it gets the lair's surface entrance: the SL_StairwellSurface prefab, stuffed in
// the map's dominant rock, with the SL_AncientStairwell portal at its center. The portal then
// owns the descent (its def.portal fields point at the SL_Lair pocket-map generator).
//
// Pattern source: Odyssey's TileMutatorWorker_AncientUplink (decompile-verified, 1.6). Same
// hook and same placement skeleton:
// - GeneratePostFog is the mutator hook run by GenStep_MutatorFinal (order 1600), after
//   terrain, structures, plants and fog. Running post-fog is what lets the validator reject
//   fogged cells, so the stairwell never lands inside a hill's interior where the player
//   could only reach it by mining.
// - MapGenUtility.TryGetRandomClearRect finds a thing-free rect of the prefab's size (its
//   default maxElevation 0.7 keeps it off rock), falling back to a random cell whose centered
//   rect passes the same validator. The validator also reserves the rect against the shared
//   MapGenerator "UsedRects" list so it can't overlap another mutator's structure (uplink,
//   stockpile hatch, ...) and uses PrefabUtility.CanSpawnPrefab with canWipeEdifices false so
//   nothing built is bulldozed.
// - PrefabUtility.SpawnPrefab is the vanilla prefab spawner; its overrideSpawnData delegate is
//   the documented way to substitute per-thing stuff at spawn time, which is how the whole
//   prefab (stairwell, columns, chunks) comes out in one stone (see StuffOverride).
// Departures from the uplink: no fixed terrain pre-paint (the uplink lays AncientTile; we lay
// the rock's own smooth floor so the pad matches the stonework), and the whole thing is gated
// on ModsConfig.OdysseyActive purely for symmetry with vanilla mutator workers: the def only
// loads from the Odyssey compat root, so the worker can't be constructed without it.
//
// Not a TileMutatorWorker_AncientStructure: that worker drives a full StructureLayoutDef
// (procedural rooms, walls, doors) via def.structureGenParms and is the upgrade path if the
// surface site ever grows into a ruin around the stairwell; for a single prefab pad the uplink
// pattern is the whole job.
public class TileMutatorWorker_SanguophageLair : TileMutatorWorker
{
    public TileMutatorWorker_SanguophageLair(TileMutatorDef def) : base(def)
    {
    }

    public override void GeneratePostFog(Map map)
    {
        if (!ModsConfig.OdysseyActive)
        {
            return;
        }
        PrefabDef prefab = SL_DefOf.SL_StairwellSurface;
        List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");

        bool Validator(CellRect r)
        {
            return r.InBounds(map)
                && !r.Cells.Any(c => c.Fogged(map))
                && PrefabUtility.CanSpawnPrefab(prefab, map, PrefabRoot(r), Rot4.North, canWipeEdifices: false)
                && !usedRects.Any(used => used.Overlaps(r));
        }

        if (!MapGenUtility.TryGetRandomClearRect(prefab.size.x, prefab.size.z, out CellRect rect, -1, -1, Validator))
        {
            if (!CellFinder.TryFindRandomCell(map, c => Validator(CellRect.CenteredOn(c, prefab.size)), out IntVec3 cell))
            {
                Log.Warning("[Sanguophage Lair] No room for the stairwell surface prefab on " + map + "; the lair entrance was not spawned.");
                return;
            }
            rect = CellRect.CenteredOn(cell, prefab.size);
        }

        ThingDef rock = LairRock.DominantRock(map);
        ThingDef blocks = LairRock.BlocksFor(rock);
        TerrainDef floor = LairRock.SmoothFloorFor(rock);
        if (floor != null)
        {
            foreach (IntVec3 c in rect)
            {
                map.terrainGrid.SetTerrain(c, floor);
            }
        }
        PrefabUtility.SpawnPrefab(prefab, map, PrefabRoot(rect), Rot4.North,
            overrideSpawnData: data => StuffOverride(data, rock, blocks));
        usedRects.Add(rect);
    }

    // Every made-from-stuff thing in the prefab takes the map's stone (the XML stuff values are
    // granite placeholders), and any stone chunk becomes the rock's own chunk (its mineable
    // drop). Returning null leaves a thing exactly as authored. The tuple is (thingDef, stuff):
    // PrefabUtility spawns Item1 with Item2, so a def swap and a stuff swap are the same
    // mechanism.
    private static Tuple<ThingDef, ThingDef> StuffOverride(PrefabThingData data, ThingDef rock, ThingDef blocks)
    {
        if (data.def.IsWithinCategory(ThingCategoryDefOf.StoneChunks) && rock.building.mineableThing != null)
        {
            return Tuple.Create(rock.building.mineableThing, (ThingDef)null);
        }
        if (data.def.MadeFromStuff && blocks.stuffProps.CanMake(data.def))
        {
            return Tuple.Create(data.def, blocks);
        }
        return null;
    }

    // PrefabUtility places a prefab by its root cell (the prefab's center); for even sizes the
    // center rounds toward the rect's min corner, exactly as TileMutatorWorker_AncientUplink
    // computes it, so the spawned footprint is the validated rect.
    private static IntVec3 PrefabRoot(CellRect rect)
    {
        IntVec3 root = rect.CenterCell;
        if (rect.Width % 2 == 0)
        {
            root.x--;
        }
        if (rect.Height % 2 == 0)
        {
            root.z--;
        }
        return root;
    }
}
