using System.Linq;
using RimWorld;
using Verse;

namespace SanguophageLair;

// Which stone the lair's stonework is cut from, and the defs derived from that stone that the
// generators need (blocks stuff for made-from-stuff things, the smooth floor terrain).
//
// One rule for both ends of the portal: the surface mutator picks the rock from the surface map
// and spawns the stairwell in its blocks; the pocket-map exit genstep then copies the entrance's
// stuff, so the staircase is one continuous piece of stonework however the lair's own rock is
// generated. Nothing here is tile-seeded: the decision is made once, on the surface map, and
// travels down through the spawned entrance.
public static class LairRock
{
    // The natural rock most common on the map, counted over its spawned rock walls, so the
    // stairwell reads as carved into the hill it sits against. Flat maps with no rock at all
    // fall back to the tile's first world-gen rock type (the list GenStep_RocksFromGrid draws
    // from), and finally to granite.
    public static ThingDef DominantRock(Map map)
    {
        ThingDef counted = map.listerThings.AllThings
            .Where(t => t.def.IsNonResourceNaturalRock)
            .GroupBy(t => t.def)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        if (counted != null)
        {
            return counted;
        }
        ThingDef fromTile = map.Tile.Valid
            ? Find.World.NaturalRockTypesIn(map.Tile).FirstOrDefault()
            : null;
        return fromTile ?? ThingDefOf.Granite;
    }

    // The stony stuff cut from a natural rock. StuffProperties.SourceNaturalRock is resolved by
    // the engine from the rock's mineable chunk (directly, or through the recipe that cuts
    // blocks from it), so this also covers modded rocks that follow the chunk -> blocks pattern.
    // Rocks with no blocks at all fall back to granite blocks rather than an unstuffed spawn
    // (ThingMaker errors on a made-from-stuff thing with null stuff).
    public static ThingDef BlocksFor(ThingDef rock)
    {
        ThingDef blocks = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d =>
            d.stuffProps?.categories?.Contains(StuffCategoryDefOf.Stony) == true
            && d.stuffProps.SourceNaturalRock == rock);
        return blocks ?? ThingDefOf.BlocksGranite;
    }

    // The smooth floor generated for a rock (TerrainDefGenerator_Stone names the trio
    // <rock>_Rough / _RoughHewn / _Smooth). Null when the rock has none (a modded rock that
    // opts out of floor generation); callers skip the floor in that case.
    public static TerrainDef SmoothFloorFor(ThingDef rock)
    {
        return DefDatabase<TerrainDef>.GetNamedSilentFail(rock.defName + "_Smooth");
    }
}
