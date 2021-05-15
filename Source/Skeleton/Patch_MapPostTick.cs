using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
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

            if (!Skeleton.IsNightTime(__instance) && !Skeleton.IsEclipse(__instance))
            {
                return;
            }

            if (Skeleton.validCorpses.Count == 0 && Skeleton.validZombieCorpses.Count == 0)
            {
                Skeleton.ScanMapsForCorpses();
                if (Skeleton.validCorpses.Count == 0 && Skeleton.validZombieCorpses.Count == 0)
                {
                    return;
                }
            }

            var ressurectedPawns = new List<Pawn>();
            var map = __instance;
            var corpsesForThisMap = from corpse in Skeleton.validCorpses
                where corpse != null &&
                      (corpse.Map == map || corpse.ParentHolder is Building_Grave grave && grave.Map == map) &&
                      corpse.GetRotStage() == RotStage.Dessicated
                select corpse;
            if (corpsesForThisMap.Any())
            {
                if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().AllAtOnce)
                {
                    foreach (var corpse in corpsesForThisMap.ToList())
                    {
                        var pawn = Skeleton.RessurectCorpse(corpse);
                        if (pawn != null)
                        {
                            ressurectedPawns.Add(pawn);
                        }
                    }
                }
                else
                {
                    if (Rand.Value < 0.4)
                    {
                        Skeleton.RessurectCorpse(corpsesForThisMap.RandomElement());
                        return;
                    }
                }
            }

            if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().AllowZombies)
            {
                return;
            }

            corpsesForThisMap = from corpse in Skeleton.validZombieCorpses
                where corpse != null &&
                      (corpse.Map == map || corpse.ParentHolder is Building_Grave grave && grave.Map == map)
                      && corpse.GetRotStage() == RotStage.Rotting
                select corpse;
            if (corpsesForThisMap.Any())
            {
                if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().AllAtOnce)
                {
                    foreach (var corpse in corpsesForThisMap.ToList())
                    {
                        var pawn = Skeleton.RessurectCorpse(corpse, true);
                        if (pawn != null)
                        {
                            ressurectedPawns.Add(pawn);
                        }
                    }
                }
                else
                {
                    if (Rand.Value < 0.4)
                    {
                        Skeleton.RessurectCorpse(corpsesForThisMap.RandomElement(), true);
                    }
                }
            }

            if (ressurectedPawns.Count <= 0)
            {
                return;
            }

            var messageType = MessageTypeDefOf.NeutralEvent;
            if (ressurectedPawns.Any(pawn => pawn.Faction.HostileTo(Faction.OfPlayer)))
            {
                messageType = MessageTypeDefOf.NegativeEvent;
            }

            var message = new Message("ressurectMessageAll".Translate(ressurectedPawns.Count), messageType,
                new LookTargets(ressurectedPawns));
            Messages.Message(message);
        }
    }
}