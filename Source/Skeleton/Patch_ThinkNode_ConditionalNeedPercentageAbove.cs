using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Skeleton
{

    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(ThinkNode_ConditionalNeedPercentageAbove))]
    [HarmonyPatch("Satisfied")]
    public static class Patch_ThinkNode_ConditionalNeedPercentageAbove
    {
        public static bool Prefix(Pawn pawn, bool __result, ThinkNode_ConditionalNeedPercentageAbove __instance)
        {
            if (pawn.kindDef.race.defName != "DRSKT_Race")
            {
                return true;
            }
            Traverse traverse = Traverse.Create(__instance);
            NeedDef need = traverse.Field("need").GetValue<NeedDef>();
            if(need != NeedDefOf.Food)
            {
                return true;
            }
            __result = true;
            return false;
        }
    }
}