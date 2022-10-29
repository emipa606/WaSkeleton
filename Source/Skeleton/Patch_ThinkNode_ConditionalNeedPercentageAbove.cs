using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton;

[HarmonyPatch(typeof(ThinkNode_ConditionalNeedPercentageAbove))]
[HarmonyPatch("Satisfied")]
public static class Patch_ThinkNode_ConditionalNeedPercentageAbove
{
    public static bool Prefix(Pawn pawn, ref bool __result, ref ThinkNode_ConditionalNeedPercentageAbove __instance,
        NeedDef ___need)
    {
        if (!pawn.kindDef.race.defName.Contains("DRSKT"))
        {
            return true;
        }

        if (___need != NeedDefOf.Food)
        {
            return true;
        }

        __result = true;
        return false;
    }
}