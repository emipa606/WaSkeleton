using Verse;

namespace Skeleton
{
    /// <summary>
    ///     Definition of the settings for the mod
    /// </summary>
    internal class SkeletonSettings : ModSettings
    {
        public bool AddHediffToAll;
        public bool AllAtOnce;
        public bool AllowZombies;
        public bool EnemyIsHostile;
        public bool ExplodeOnDeath = true;
        public bool OnlyBuried;
        public bool OnlyColonists;
        public bool OnlyDuringEclipse;
        public bool OnlyNonBuried;
        public bool OnlyNonColonists;
        public float ReanimateChance = 1f;
        public bool ReanimateCorpses;
        public bool VerboseLogging;

        /// <summary>
        ///     Saving and loading the values
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ExplodeOnDeath, "ExplodeOnDeath", true);
            Scribe_Values.Look(ref AddHediffToAll, "AddHediffToAll");
            Scribe_Values.Look(ref ReanimateCorpses, "ReanimateCorpses");
            Scribe_Values.Look(ref EnemyIsHostile, "EnemyIsHostile");
            Scribe_Values.Look(ref AllowZombies, "AllowZombies");
            Scribe_Values.Look(ref OnlyBuried, "OnlyBuried");
            Scribe_Values.Look(ref OnlyNonBuried, "OnlyNonBuried");
            Scribe_Values.Look(ref OnlyColonists, "OnlyColonists");
            Scribe_Values.Look(ref OnlyNonColonists, "OnlyNonColonists");
            Scribe_Values.Look(ref OnlyDuringEclipse, "OnlyDuringEclipse");
            Scribe_Values.Look(ref AllAtOnce, "AllAtOnce");
            Scribe_Values.Look(ref ReanimateChance, "ReanimateChance", 1f);
            Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
        }
    }
}