using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton
{
    [HarmonyPatch(typeof(DeathActionWorker_SmallExplosion))]
    [HarmonyPatch("PawnDied")]
    public static class Patch_SkeletonExplosion
    {
        [HarmonyPrefix]
        private static bool Prefix(ref Corpse corpse)
        {
            if (!corpse.def.defName.Contains("DRSKT"))
            {
                return true;
            }

            if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ExplodeOnDeath)
            {
                GenExplosion.DoExplosion(corpse.Position, corpse.Map, 0.9f, DamageDefOf.Flame, corpse.InnerPawn,
                    100, 1f);
            }

            return false;
        }
    }
}
