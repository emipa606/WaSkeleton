using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood", typeof(List<Pawn>),
        typeof(List<ThingDefCount>),
        typeof(int),
        typeof(IgnorePawnsInventoryMode),
        typeof(Faction),
        typeof(WorldPath),
        typeof(float),
        typeof(int),
        typeof(bool))]
    public static class Patch_DaysWorthOfFoodCalculator_ApproxDaysWorthOfFood
    {
        public static void Prefix(ref List<Pawn> pawns, List<ThingDefCount> extraFood, int tile,
            IgnorePawnsInventoryMode
                ignoreInventory, Faction faction, WorldPath path, float nextTileCostLeft, int caravanTicksPerMove, bool
                assumeCaravanMoving)
        {
            var list = new List<Pawn>(pawns);
            list.RemoveAll(pawn => pawn.kindDef.race.defName.Contains("DRSKT"));
            pawns = list;
        }
    }
}