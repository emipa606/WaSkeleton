using HarmonyLib;
using Verse;

namespace Skeleton
{
    [HarmonyPatch(typeof(GameComponentUtility))]
    [HarmonyPatch("LoadedGame")]
    public static class Patch_LoadedGame
    {
        [HarmonyPrefix]
        private static void Postfix()
        {
            Skeleton.validCorpses.Clear();
            Skeleton.validZombieCorpses.Clear();
            Skeleton.ScanMapsForCorpses();
        }
    }
}
