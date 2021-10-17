using HarmonyLib;
using Verse;

namespace Skeleton
{
    [HarmonyPatch(typeof(Corpse))]
    [HarmonyPatch("RotStageChanged")]
    public static class Patch_RotStageChanged
    {
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

            if (!Skeleton.IsCorpseValid(__instance))
            {
                return;
            }

            if (Skeleton.CanBeZombie(__instance))
            {
                Skeleton.validZombieCorpses.Add(__instance);
                Skeleton.LogMessage(
                    $"Added the corpse of {__instance.InnerPawn.NameShortColored} to the valid zombie-list.");
            }
            else
            {
                if (Skeleton.validZombieCorpses.Contains(__instance))
                {
                    Skeleton.validZombieCorpses.Remove(__instance);
                }

                if (Rand.Value > LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ReanimateChance)
                {
                    return;
                }

                Skeleton.validCorpses.Add(__instance);
                Skeleton.LogMessage(
                    $"Added the corpse of {__instance.InnerPawn.NameShortColored} to the valid skeleton-list.");
            }
        }
    }
}