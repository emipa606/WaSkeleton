using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Skeleton;

[StaticConstructorOnStartup]
public static class Skeleton
{
    public static readonly List<Corpse> validCorpses = new List<Corpse>();
    public static readonly List<Corpse> validZombieCorpses = new List<Corpse>();

    private static readonly PawnKindDef SkeletonPawnKind;
    private static readonly PawnKindDef ZombiePawnKind;
    public static List<ThingDef> allGraves;

    private static readonly List<TaggedString> ressurectMessages = new List<TaggedString>
    {
        "ressurectMessage1".Translate(),
        "ressurectMessage2".Translate(),
        "ressurectMessage3".Translate(),
        "ressurectMessage4".Translate(),
        "ressurectMessage5".Translate()
    };

    static Skeleton()
    {
        allGraves = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.thingClass == typeof(Building_Grave) ||
                          def.thingClass.IsSubclassOf(typeof(Building_Grave))).ToList();
        Log.Message($"[WaSkeleton]: Found {allGraves.Count} possible graves");
        var harmonyInstance = new Harmony("com.rimworld.Dalrae.Skeleton");
        SkeletonPawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("DRSKT_Colonist");
        ZombiePawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("DRSKT_Colonist_Zombie");
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    }

    public static bool IsCorpseValid(Corpse corpse)
    {
        if (corpse?.InnerPawn == null)
        {
            return false;
        }

        if (!corpse.InnerPawn.RaceProps?.Humanlike == true ||
            corpse.InnerPawn.kindDef?.race?.defName?.Contains("DRSKT") == true)
        {
            return false;
        }

        if (corpse.InnerPawn.health?.hediffSet?.HasHead == false)
        {
            return false;
        }

        var settings = LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>();

        if (settings == null)
        {
            settings = new SkeletonSettings
            {
                ExplodeOnDeath = true
            };
        }

        if (!settings.OnlyNonBuried && corpse.ParentHolder is Building_Grave grave &&
            settings.SecureGraves?.Contains(grave.def.defName) == true)
        {
            return false;
        }

        if (settings.OnlyBuried && !(corpse.ParentHolder is Building_Grave))
        {
            return false;
        }

        if (settings.OnlyNonBuried && corpse.ParentHolder is Building_Grave)
        {
            return false;
        }

        if (settings.OnlyColonists && !corpse.InnerPawn.Faction?.IsPlayer == true)
        {
            return false;
        }

        if (settings.OnlyNonColonists && corpse.InnerPawn.Faction?.IsPlayer == true)
        {
            return false;
        }

        if (corpse.GetRotStage() == RotStage.Fresh)
        {
            return false;
        }

        return true;
    }

    public static bool CanBeZombie(Corpse corpse)
    {
        if (corpse.IsDessicated())
        {
            return false;
        }

        if (corpse.GetRotStage() != RotStage.Rotting)
        {
            return false;
        }

        if (validCorpses.Contains(corpse))
        {
            return false;
        }

        var randValue = Rand.Value;
        return randValue < 0.2f * LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>()
            .ReanimateChance;
    }

    public static Pawn RessurectCorpse(Corpse corpse, bool zombie = false)
    {
        LogMessage(
            $"Will ressurect {corpse.InnerPawn.NameShortColored}");
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

        Pawn pawnToRessurect = null;
        var counter = 0;
        while (pawnToRessurect == null && counter < 15)
        {
            try
            {
                pawnToRessurect = GeneratePawn(corpse, zombie);
            }
            catch
            {
                // ignored
            }

            counter++;
        }

        if (pawnToRessurect == null)
        {
            return null;
        }

        if (grave != null && grave.GetDirectlyHeldThings().Count == 1)
        {
            grave.EjectContents();
        }

        GenSpawn.Spawn(pawnToRessurect, cellToRessurectOn, map);
        FleckMaker.ThrowSmoke(cellToRessurectOn.ToVector3(), map, 2f);
        var hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail("RessurectedFromTheDead");
        if (zombie)
        {
            hediffToAdd = DefDatabase<HediffDef>.GetNamedSilentFail("ReanimatedFromTheDead");
        }

        pawnToRessurect.health.AddHediff(hediffToAdd);
        pawnToRessurect.inventory.DestroyAll();
        corpse.InnerPawn.inventory.GetDirectlyHeldThings()
            .TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());
        pawnToRessurect.apparel.WornApparel.Clear();
        corpse.InnerPawn.apparel.GetDirectlyHeldThings()
            .TryTransferAllToContainer(pawnToRessurect.apparel.GetDirectlyHeldThings());

        if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().EnemyIsHostile &&
            pawnToRessurect.Faction.HostileTo(Faction.OfPlayer))
        {
            if (zombie)
            {
                pawnToRessurect.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
            }
            else
            {
                LordMaker.MakeNewLord(pawnToRessurect.Faction, new LordJob_AssaultColony(pawnToRessurect.Faction),
                    pawnToRessurect.Map, new List<Pawn> { pawnToRessurect });
            }
        }

        if (validCorpses.Contains(corpse))
        {
            validCorpses.Remove(corpse);
        }

        if (validZombieCorpses.Contains(corpse))
        {
            validZombieCorpses.Remove(corpse);
        }

        SendRessurectionMessage(pawnToRessurect);
        corpse.Destroy();
        return pawnToRessurect;
    }

    public static void ScanMapsForCorpses()
    {
        var settings = LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>();
        if (!settings.ReanimateCorpses)
        {
            return;
        }

        LogMessage(
            "WaSkeleton: cleared the valid corpses-list.");

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
                if (!IsCorpseValid(corpse))
                {
                    continue;
                }

                if (CanBeZombie(corpse))
                {
                    validZombieCorpses.Add(corpse);
                }
                else
                {
                    if (Rand.Value <= settings.ReanimateChance)
                    {
                        validCorpses.Add(corpse);
                    }
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

                if (!IsCorpseValid(grave.Corpse))
                {
                    continue;
                }

                if (CanBeZombie(grave.Corpse))
                {
                    validZombieCorpses.Add(grave.Corpse);
                }
                else
                {
                    if (Rand.Value <= settings.ReanimateChance)
                    {
                        validCorpses.Add(grave.Corpse);
                    }
                }
            }
        }

        if (validCorpses.Any() || validZombieCorpses.Any())
        {
            LogMessage(
                $"Added {validCorpses.Count} corpses to the valid skeleton list, {validZombieCorpses.Count} to zombies list.");
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
            foreach (var pawn in from pawn in map.mapPawns.AllPawns
                     where pawn != null && pawn.def.defName.Contains("DRSKT") && pawn.health?.Dead == false
                     select pawn)
            {
                var hediffToAdd = HediffMaker.MakeHediff(ressurected, pawn);
                if (pawn.def.defName == "DRSKT_Race_Zombie")
                {
                    hediffToAdd = HediffMaker.MakeHediff(reanimated, pawn);
                }

                if (pawn.health == null)
                {
                    pawn.health = new Pawn_HealthTracker(pawn);
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
        var localPawnKind = SkeletonPawnKind;
        if (zombie)
        {
            localPawnKind = ZombiePawnKind;
        }

        var request = new PawnGenerationRequest(localPawnKind, corpse.InnerPawn.Faction,
            PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 0, false, true, false,
            false, false, false, false, false, 0, 0, null, 1, null, null, null, null, null, null, null,
            corpse.InnerPawn.gender);
        var newPawn = PawnGenerator.GeneratePawn(request);
        newPawn.Name = corpse.InnerPawn.Name;
        newPawn.ageTracker = corpse.InnerPawn.ageTracker;
        foreach (var sr in newPawn.skills.skills)
        {
            sr.Level = corpse.InnerPawn.skills.GetSkill(sr.def).levelInt;
        }

        if (corpse.InnerPawn.Faction is { IsPlayer: true })
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
        if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().AllAtOnce)
        {
            return;
        }

        var text = $"{pawn.NameShortColored} {ressurectMessages.RandomElement()}";
        var messageType = MessageTypeDefOf.SilentInput;
        if (pawn.Faction.HostileTo(Faction.OfPlayer))
        {
            messageType = MessageTypeDefOf.NegativeEvent;
        }

        var message = new Message(text, messageType, new LookTargets(pawn));
        Messages.Message(message, false);
    }

    public static bool IsNightTime(Map map)
    {
        if (LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyDuringEclipse)
        {
            return false;
        }

        return GenLocalDate.HourInteger(map) >= 23 || GenLocalDate.HourInteger(map) <= 5;
    }

    public static bool IsEclipse(Map map)
    {
        if (map == null)
        {
            return false;
        }

        if (!LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().OnlyDuringEclipse)
        {
            return false;
        }

        var tempAllGameConditionsAffectingMap = new List<GameCondition>();
        map.gameConditionManager?.GetAllGameConditionsAffectingMap(map, tempAllGameConditionsAffectingMap);
        return tempAllGameConditionsAffectingMap.Any(condition => condition?.def?.defName == "Eclipse");
    }

    public static void LogMessage(string message, bool forced = false)
    {
        if (!forced && !LoadedModManager.GetMod<SkeletonMod>().GetSettings<SkeletonSettings>().VerboseLogging)
        {
            return;
        }

        Log.Message($"[WaSkeleton]: {message}");
    }
}