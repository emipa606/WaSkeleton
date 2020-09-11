using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(Map))]
	[HarmonyPatch("MapPostTick")]
	public static class Patch_MapPostTick
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002078 File Offset: 0x00000278
		[HarmonyPrefix]
		private static void Postfix(ref Map __instance)
		{
			if (Find.TickManager.TicksGame % GenTicks.TickLongInterval != 0)
            {
				return;
			}
			Skeleton.ScanMapsForUnaffectedSkeletons();
			if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ReanimateCorpses)
			{
				return;
			}
			if(!Skeleton.IsNightTime(__instance))
            {
				return;
            }
			if(Skeleton.validCorpses.Count == 0)
            {
				return;
            }
			var map = __instance;
			var corpsesForThisMap = from corpse in Skeleton.validCorpses where corpse != null && (corpse.Map == map || (corpse.ParentHolder is Building_Grave && (corpse.ParentHolder as Building_Grave).Map == map)) select corpse;
			if(corpsesForThisMap.Count() == 0)
            {
				return;
            }
			Skeleton.RessurectCorpse(corpsesForThisMap.RandomElement());
		}
	}
}
