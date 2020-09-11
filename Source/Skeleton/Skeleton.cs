using System;
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
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        static Skeleton()
        {
            var harmonyInstance = new Harmony("com.rimworld.Dalrae.Skeleton");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static List<Corpse> validCorpses = new List<Corpse>();

        public static bool IsCorpseValid(Corpse corpse)
        {
            if (!corpse.IsDessicated())
            {
                return false;
            }
            if (!corpse.InnerPawn.RaceProps.Humanlike || corpse.InnerPawn.kindDef.race.defName == "DRSKT_Race")
            {
                return false;
            }
            if (!corpse.InnerPawn.health.hediffSet.HasHead)
            {
                return false;
            }
            if(LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyBuried)
            {
                if(!(corpse.ParentHolder is Building_Grave))
                {
                    return false;
                }
            }
            if(LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyColonists)
            {
                if (!corpse.InnerPawn.Faction.IsPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        public static void RessurectCorpse(Corpse corpse)
        {
            //Log.Message($"WaSkeleton: Will ressurect {corpse.InnerPawn.NameShortColored}");
            var cellToRessurectOn = corpse.TrueCenter().ToIntVec3();
            var map = corpse.Map;
            Building_Grave grave = null;
            if(map == null)
            {
                grave = corpse.ParentHolder as Building_Grave;
                map = grave.Map;
                cellToRessurectOn = grave.TrueCenter().ToIntVec3();
            }
            var pawnToRessurect = GeneratePawn(corpse);
            if(grave != null)
            {
                //Log.Message($"WaSkeleton: Updating grave");
                grave.EjectContents();
            }
            //Log.Message($"WaSkeleton: Spawning pawn");
            GenSpawn.Spawn(pawnToRessurect, cellToRessurectOn, map);
            MoteMaker.ThrowSmoke(cellToRessurectOn.ToVector3(), map, 2f);
            HediffDef hediffToAdd = (from hediff in DefDatabase<HediffDef>.AllDefsListForReading where hediff.defName == "RessurectedFromTheDead" select hediff).FirstOrDefault();
            pawnToRessurect.health.AddHediff(hediffToAdd);
            //Log.Message($"WaSkeleton: Transferring items");
            pawnToRessurect.inventory.DestroyAll();
            corpse.InnerPawn.inventory.GetDirectlyHeldThings().TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());
            //Log.Message($"WaSkeleton: Transferring clothes");
            pawnToRessurect.apparel.WornApparel.Clear();
            corpse.InnerPawn.apparel.GetDirectlyHeldThings().TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());
            //Log.Message($"WaSkeleton: Removing corpse from list");
            validCorpses.Remove(corpse);
            SendRessurectionMessage(corpse.InnerPawn);
            //Log.Message($"WaSkeleton: Removing corpse from map");
            corpse.Destroy(DestroyMode.Vanish);
        }

        public static void ScanMapsForCorpses()
        {
            if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().ReanimateCorpses)
            {
                return;
            }
            if (Prefs.DevMode)
                Log.Message($"WaSkeleton: cleared the valid corpses-list.");
            validCorpses.Clear();
            if(Current.Game == null)
            {
                return;
            }
            foreach (var map in Current.Game.Maps)
            {
                foreach (var corpse in from thing in map.listerThings.AllThings where thing != null && thing is Corpse select thing as Corpse)
                {
                    if (IsCorpseValid(corpse))
                    {
                        validCorpses.Add(corpse);
                    }
                }
                foreach (var grave in from thing in map.listerThings.AllThings where thing != null && thing is Building_Grave select thing as Building_Grave)
                {
                    if (!grave.HasCorpse)
                    {
                        continue;
                    }
                    if (IsCorpseValid(grave.Corpse))
                    {
                        validCorpses.Add(grave.Corpse);
                    }
                }
            }
            if (Prefs.DevMode && validCorpses.Count() > 0)
                Log.Message($"WaSkeleton: Added {validCorpses.Count()} corpses to the valid corpse list.");
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
            HediffDef ressurected = (from hediff in DefDatabase<HediffDef>.AllDefsListForReading where hediff.defName == "RessurectedFromTheDead" select hediff).First();
            foreach (var map in Current.Game.Maps)
            {
                foreach (var pawn in from thing in map.listerThings.AllThings where thing != null && thing.def.defName == "DRSKT_Race" && !((Pawn)thing).health.Dead select thing as Pawn)
                {
                    if (!pawn.health.hediffSet.HasHediff(ressurected))
                    {
                        Hediff hediffToAdd = HediffMaker.MakeHediff(ressurected, pawn, null);
                        pawn.health.AddHediff(hediffToAdd);
                    }
                }
            }
        }
        private static readonly PawnKindDef skeletonPawnKind = (from pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading where pawnKindDef.defName == "DRSKT_Colonist" select pawnKindDef).FirstOrDefault();

        private static readonly List<TaggedString> ressurectMessages = new List<TaggedString>
        {
            "ressurectMessage1".Translate(),
            "ressurectMessage2".Translate(),
            "ressurectMessage3".Translate(),
            "ressurectMessage4".Translate(),
            "ressurectMessage5".Translate()
        };

        private static Pawn GeneratePawn(Corpse corpse)
        {
            //Log.Message($"WaSkeleton: Generating request");
            PawnGenerationRequest request = new PawnGenerationRequest(skeletonPawnKind, corpse.InnerPawn.Faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 0, false, true, false, false, false, false, false, false, 0, null, 1, null, null, null, null, null, null, null, corpse.InnerPawn.gender, null);
            //Log.Message($"WaSkeleton: Generating pawn of kind {skeletonPawnKind}");
            Pawn newPawn = PawnGenerator.GeneratePawn(request);
            //Log.Message($"WaSkeleton: Setting properties");
            newPawn.Name = corpse.InnerPawn.Name;
            newPawn.ageTracker = corpse.InnerPawn.ageTracker;

            //Log.Message($"WaSkeleton: Copying skills");
            foreach (SkillRecord sr in newPawn.skills.skills)
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
            return newPawn;
        }

        private static void SendRessurectionMessage(Pawn pawn)
        {
            var text = $"{pawn.NameShortColored} {ressurectMessages.RandomElement()}";
            var messageType = MessageTypeDefOf.SilentInput;
            if(pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                messageType = MessageTypeDefOf.NegativeEvent;                
            }
            var message = new Message(text, messageType, new LookTargets(pawn));
            Messages.Message(message, false);
        }

        public static bool IsNightTime(Map p)
        {
            return (GenLocalDate.HourInteger(p) >= 23 || GenLocalDate.HourInteger(p) <= 5);
        }
    }

}
