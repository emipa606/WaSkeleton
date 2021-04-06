using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(DeathActionWorker_SmallExplosion))]
    [HarmonyPatch("PawnDied")]
    public static class Patch_SkeletonExplosion
    {
        // Token: 0x06000002 RID: 2 RVA: 0x00002078 File Offset: 0x00000278
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