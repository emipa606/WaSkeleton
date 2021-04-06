using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Skeleton
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    public static class Skeleton
    {
        public static readonly List<Corpse> validCorpses = new();
        public static readonly List<Corpse> validZombieCorpses = new();

        private static readonly PawnKindDef SkeletonPawnKind;
        private static readonly PawnKindDef ZombiePawnKind;

        private static readonly List<TaggedString> ressurectMessages = new()
        {
            "ressurectMessage1".Translate(),
            "ressurectMessage2".Translate(),
            "ressurectMessage3".Translate(),
            "ressurectMessage4".Translate(),
            "ressurectMessage5".Translate()
        };

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        static Skeleton()
        {
            var harmonyInstance = new Harmony("com.rimworld.Dalrae.Skeleton");
            SkeletonPawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("DRSKT_Colonist");
            ZombiePawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("DRSKT_Colonist_Zombie");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool IsCorpseValid(Corpse corpse, out bool canBeZombie)
        {
            canBeZombie = false;
            if (!corpse.InnerPawn.RaceProps.Humanlike || corpse.InnerPawn.kindDef.race.defName.Contains("DRSKT"))
            {
                return false;
            }

            if (!corpse.InnerPawn.health.hediffSet.HasHead)
            {
                return false;
            }

            if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyBuried &&
                !(corpse.ParentHolder is Building_Grave))
            {
                return false;
            }

            if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyNonBuried &&
                corpse.ParentHolder is Building_Grave)
            {
                return false;
            }

            if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyColonists &&
                !corpse.InnerPawn.Faction.IsPlayer)
            {
                return false;
            }

            if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyNonColonists &&
                corpse.InnerPawn.Faction.IsPlayer)
            {
                return false;
            }

            if (corpse.IsDessicated())
            {
                return true;
            }

            if (corpse.GetRotStage() != RotStage.Rotting)
            {
                Log.Message(corpse.GetRotStage().ToString());
                return false;
            }

            canBeZombie = true;
            var randValue = Rand.Value;
            Log.Message(corpse.LabelShort);
            Log.Message(randValue.ToString());
            return randValue < 0.2f;
        }

        public static void RessurectCorpse(Corpse corpse, bool zombie = false)
        {
            //Log.Message($"WaSkeleton: Will ressurect {corpse.InnerPawn.NameShortColored}");
            var cellToRessurectOn = corpse.TrueCenter().ToIntVec3();
            var map = corpse.Map;
            Building_Grave grave = null;
            if (map == null)
            {
                grave = corpse.ParentHolder as Building_Grave;
                if (grave != null)
                {
                    map = grave.Map;
                    cellToRessurectOn = grave.TrueCenter().ToIntVec3();
                }
            }

            Pawn pawnToRessurect;
            try
            {
                pawnToRessurect = GeneratePawn(corpse, zombie);
            }
            catch
            {
                return;
            }

            if (grave != null && grave.GetDirectlyHeldThings().Count == 1)
            {
                //Log.Message($"WaSkeleton: Updating grave");
                grave.EjectContents();
            }

            //Log.Message($"WaSkeleton: Spawning pawn");
            GenSpawn.Spawn(pawnToRessurect, cellToRessurectOn, map);
            MoteMaker.ThrowSmoke(cellToRessurectOn.ToVector3(), map, 2f);
            var hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail("RessurectedFromTheDead");
            if (zombie)
            {
                hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail("ReanimatedFromTheDead");
            }

            pawnToRessurect.health.AddHediff(hediffToAdd);
            //Log.Message($"WaSkeleton: Transferring items");
            pawnToRessurect.inventory.DestroyAll();
            corpse.InnerPawn.inventory.GetDirectlyHeldThings()
                .TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());
            //Log.Message($"WaSkeleton: Transferring clothes");
            pawnToRessurect.apparel.WornApparel.Clear();
            corpse.InnerPawn.apparel.GetDirectlyHeldThings()
                .TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());
            //Log.Message($"WaSkeleton: Removing corpse from list");
            if (validCorpses.Contains(corpse))
            {
                validCorpses.Remove(corpse);
            }

            if (validZombieCorpses.Contains(corpse))
            {
                validZombieCorpses.Remove(corpse);
            }

            SendRessurectionMessage(pawnToRessurect);
            //Log.Message($"WaSkeleton: Removing corpse from map");
            corpse.Destroy();
        }

        public static void ScanMapsForCorpses()
        {
            if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ReanimateCorpses)
            {
                return;
            }

            if (Prefs.DevMode)
            {
                Log.Message("WaSkeleton: cleared the valid corpses-list.");
            }

            validCorpses.Clear();
            validZombieCorpses.Clear();
            if (Current.Game == null)
            {
                return;
            }

            foreach (var map in Current.Game.Maps)
            {
                foreach (var corpse in from thing in map.listerThings.AllThings
                    where thing is Corpse
                    select thing as Corpse)
                {
                    if (!IsCorpseValid(corpse, out var canBeZombie))
                    {
                        continue;
                    }

                    if (canBeZombie)
                    {
                        validZombieCorpses.Add(corpse);
                    }
                    else
                    {
                        validCorpses.Add(corpse);
                    }
                }

                foreach (var grave in from thing in map.listerThings.AllThings
                    where thing is Building_Grave
                    select thing as Building_Grave)
                {
                    if (!grave.HasCorpse)
                    {
                        continue;
                    }

                    if (!IsCorpseValid(grave.Corpse, out var canBeZombie))
                    {
                        continue;
                    }

                    if (canBeZombie)
                    {
                        validZombieCorpses.Add(grave.Corpse);
                    }
                    else
                    {
                        validCorpses.Add(grave.Corpse);
                    }
                }
            }

            if (Prefs.DevMode && (validCorpses.Any() || validZombieCorpses.Any()))
            {
                Log.Message(
                    $"WaSkeleton: Added {validCorpses.Count} corpses to the valid corpse list, {validZombieCorpses.Count} to zombies list.");
            }
        }

        public static void ScanMapsForUnaffectedSkeletons()
        {
            if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().AddHediffToAll)
            {
                return;
            }

            if (Current.Game == null)
            {
                return;
            }

            var ressurected = DefDatabase<HediffDef>.GetNamedSilentFail("RessurectedFromTheDead");
            var reanimated = DefDatabase<HediffDef>.GetNamedSilentFail("ReanimatedFromTheDead");
            foreach (var map in Current.Game.Maps)
            {
                foreach (var pawn in from thing in map.listerThings.AllThings
                    where thing != null && thing.def.defName.Contains("DRSKT") && !((Pawn) thing).health.Dead
                    select thing as Pawn)
                {
                    var hediffToAdd = HediffMaker.MakeHediff(ressurected, pawn);
                    if (pawn.def.defName == "DRSKT_Race_Zombie")
                    {
                        hediffToAdd = HediffMaker.MakeHediff(reanimated, pawn);
                    }

                    if (pawn.health.hediffSet.HasHediff(ressurected) || pawn.health.hediffSet.HasHediff(reanimated))
                    {
                        continue;
                    }

                    pawn.health.AddHediff(hediffToAdd);
                }
            }
        }

        private static Pawn GeneratePawn(Corpse corpse, bool zombie = false)
        {
            //Log.Message($"WaSkeleton: Generating request");
            var localPawnKind = SkeletonPawnKind;
            if (zombie)
            {
                localPawnKind = ZombiePawnKind;
            }

            var request = new PawnGenerationRequest(localPawnKind, corpse.InnerPawn.Faction,
                PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 0, false, true, false,
                false, false, false, false, false, 0, null, 1, null, null, null, null, null, null, null,
                corpse.InnerPawn.gender);
            //Log.Message($"WaSkeleton: Generating pawn of kind {skeletonPawnKind}");
            var newPawn = PawnGenerator.GeneratePawn(request);
            //Log.Message($"WaSkeleton: Setting properties");
            newPawn.Name = corpse.InnerPawn.Name;
            newPawn.ageTracker = corpse.InnerPawn.ageTracker;
            //Log.Message($"WaSkeleton: Copying skills");
            foreach (var sr in newPawn.skills.skills)
            {
                sr.Level = corpse.InnerPawn.skills.GetSkill(sr.def).levelInt;
            }

            //Log.Message($"WaSkeleton: Setting hostility response");
            if (corpse.InnerPawn.Faction != null && corpse.InnerPawn.Faction.IsPlayer)
            {
                newPawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            }

            newPawn.story.childhood = corpse.InnerPawn.story.childhood;
            if (newPawn.story.adulthood != null)
            {
                newPawn.story.adulthood = corpse.InnerPawn.story.adulthood;
            }

            newPawn.story.traits = corpse.InnerPawn.story.traits;
            newPawn.abilities.abilities = corpse.InnerPawn.abilities.abilities;
            newPawn.skills.skills = corpse.InnerPawn.skills.skills;
            if (zombie)
            {
                newPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt =
                    corpse.InnerPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2;
            }

            newPawn.timetable = corpse.InnerPawn.timetable;
            var directRelations = new List<DirectPawnRelation>();
            foreach (var relation in corpse.InnerPawn.relations.DirectRelations)
            {
                directRelations.Add(relation);
            }

            corpse.InnerPawn.relations.ClearAllRelations();
            foreach (var relation in directRelations)
            {
                newPawn.relations.AddDirectRelation(relation.def, relation.otherPawn);
                relation.otherPawn.needs.mood.thoughts.memories.RemoveMemoriesWhereOtherPawnIs(corpse.InnerPawn);
            }

            return newPawn;
        }

        private static void SendRessurectionMessage(Pawn pawn)
        {
            var text = $"{pawn.NameShortColored} {ressurectMessages.RandomElement()}";
            var messageType = MessageTypeDefOf.SilentInput;
            if (pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                messageType = MessageTypeDefOf.NegativeEvent;
            }

            var message = new Message(text, messageType, new LookTargets(pawn));
            Messages.Message(message, false);
        }

        public static bool IsNightTime(Map p)
        {
            return GenLocalDate.HourInteger(p) >= 23 || GenLocalDate.HourInteger(p) <= 5;
        }
    }
}