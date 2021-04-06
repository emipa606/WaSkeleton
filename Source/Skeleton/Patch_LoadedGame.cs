using HarmonyLib;
using Verse;

namespace Skeleton
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(GameComponentUtility))]
    [HarmonyPatch("LoadedGame")]
    public static class Patch_LoadedGame
    {
        // Token: 0x06000002 RID: 2 RVA: 0x00002078 File Offset: 0x00000278
        [HarmonyPrefix]
        private static void Postfix()
        {
            Skeleton.validCorpses.Clear();
            Skeleton.validZombieCorpses.Clear();
            Skeleton.ScanMapsForCorpses();
        }
    }
}