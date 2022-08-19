using HarmonyLib;
using Verse;

namespace Skeleton;

[HarmonyPatch(typeof(GameComponentUtility))]
[HarmonyPatch("StartedNewGame")]
public static class Patch_StartedNewGame
{
    [HarmonyPrefix]
    private static void Postfix()
    {
        Skeleton.validCorpses.Clear();
        Skeleton.validZombieCorpses.Clear();
        Skeleton.ScanMapsForCorpses();
    }
}