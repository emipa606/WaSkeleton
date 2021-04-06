using HarmonyLib;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(Corpse))]
    [HarmonyPatch("RotStageChanged")]
    public static class Patch_RotStageChanged
    {
        // Token: 0x06000002 RID: 2 RVA: 0x00002078 File Offset: 0x00000278
        [HarmonyPrefix]
        private static void Postfix(ref Corpse __instance)
        {
            if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ReanimateCorpses)
            {
                return;
            }

            if (__instance == null)
            {
                return;
            }

            if (!Skeleton.IsCorpseValid(__instance, out var canBeZombie))
            {
                return;
            }

            if (canBeZombie)
            {
                Skeleton.validZombieCorpses.Add(__instance);
            }
            else
            {
                Skeleton.validCorpses.Add(__instance);
            }

            if (Prefs.DevMode)
            {
                Log.Message(
                    $"WaSkeleton: Added the corpse of {__instance.InnerPawn.NameShortColored} to the valid corpse list, can be zombie: {canBeZombie}.");
            }
        }
    }
}