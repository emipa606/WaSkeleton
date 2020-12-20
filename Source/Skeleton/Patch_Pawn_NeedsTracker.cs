using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
    [HarmonyPatch("AddOrRemoveNeedsAsAppropriate")]
    public static class Patch_Pawn_NeedsTracker
    {
        public static void Postfix(Pawn_NeedsTracker __instance)
        {
            var traverse = Traverse.Create(__instance);
            Pawn pawn = traverse.Field("pawn").GetValue<Pawn>();
            if (pawn.kindDef.race.defName != "DRSKT_Race")
            {
                return;
            }

            if (__instance.TryGetNeed(NeedDefOf.Food) != null)
            {
                Need item = __instance.TryGetNeed(NeedDefOf.Food);
                List<Need> needlist = traverse.Field("needs").GetValue<List<Need>>();
                needlist.Remove(item);
            }
        }
    }
}