using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
    [HarmonyPatch("AddOrRemoveNeedsAsAppropriate")]
    public static class Patch_Pawn_NeedsTracker
    {
        public static void Postfix(Pawn_NeedsTracker __instance)
        {
            var traverse = Traverse.Create(__instance);
            var pawn = traverse.Field("pawn").GetValue<Pawn>();
            if (!pawn.kindDef.race.defName.Contains("DRSKT"))
            {
                return;
            }

            if (__instance.TryGetNeed(NeedDefOf.Food) == null)
            {
                return;
            }

            var item = __instance.TryGetNeed(NeedDefOf.Food);
            var needlist = traverse.Field("needs").GetValue<List<Need>>();
            needlist.Remove(item);
        }
    }
}
