using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton
{
    [HarmonyPatch(typeof(ThinkNode_ConditionalNeedPercentageAbove))]
    [HarmonyPatch("Satisfied")]
    public static class Patch_ThinkNode_ConditionalNeedPercentageAbove
    {
        public static bool Prefix(Pawn pawn, ref bool __result, ref ThinkNode_ConditionalNeedPercentageAbove __instance)
        {
            if (!pawn.kindDef.race.defName.Contains("DRSKT"))
            {
                return true;
            }

            var traverse = Traverse.Create(__instance);
            var need = traverse.Field("need").GetValue<NeedDef>();
            if (need != NeedDefOf.Food)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
