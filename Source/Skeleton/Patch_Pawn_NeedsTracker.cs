using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton;

[HarmonyPatch(typeof(Pawn_NeedsTracker))]
[HarmonyPatch("AddOrRemoveNeedsAsAppropriate")]
public static class Patch_Pawn_NeedsTracker
{
    public static void Postfix(Pawn_NeedsTracker __instance, ref List<Need> ___needs, ref Pawn ___pawn)
    {
        if (!___pawn.kindDef.race.defName.Contains("DRSKT"))
        {
            return;
        }

        if (__instance.TryGetNeed(NeedDefOf.Food) == null)
        {
            return;
        }

        var item = __instance.TryGetNeed(NeedDefOf.Food);
        ___needs.Remove(item);
    }
}