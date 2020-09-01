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
			if(!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ExplodeOnDeath)
            {
				return true;
            }
			bool flag = corpse.def.defName == "Corpse_DRSKT_Race";
			if (flag)
			{
				GenExplosion.DoExplosion(corpse.Position, corpse.Map, 0.9f, DamageDefOf.Flame, corpse.InnerPawn, 100, 1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
				ExplosionDef.SkeletonEXPLOSION = false;
			}
			else
			{
				ExplosionDef.SkeletonEXPLOSION = true;
			}
			return ExplosionDef.SkeletonEXPLOSION;
		}
	}
}
