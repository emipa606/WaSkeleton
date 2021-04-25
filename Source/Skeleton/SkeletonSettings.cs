using Verse;

namespace Skeleton
{
    /// <summary>
    ///     Definition of the settings for the mod
    /// </summary>
    internal class SkeletonSettings : ModSettings
    {
        public bool AddHediffToAll;
        public bool AllowZombies;
        public bool EnemyIsHostile;
        public bool ExplodeOnDeath = true;
        public bool OnlyBuried;
        public bool OnlyColonists;
        public bool OnlyNonBuried;
        public bool OnlyNonColonists;
        public bool ReanimateCorpses;

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
        }
    }
}