using Verse;

namespace Skeleton
{
    /// <summary>
    /// Definition of the settings for the mod
    /// </summary>
    internal class SkeletonSettings : ModSettings
    {
        public bool ExplodeOnDeath = true;
        public bool AddHediffToAll = false;
        public bool ReanimateCorpses = false;
        public bool OnlyBuried = false;
        public bool OnlyNonBuried = false;
        public bool OnlyColonists = false;
        public bool OnlyNonColonists = false;

        /// <summary>
        /// Saving and loading the values
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ExplodeOnDeath, "ExplodeOnDeath", true, false);
            Scribe_Values.Look(ref AddHediffToAll, "AddHediffToAll", false, false);
            Scribe_Values.Look(ref ReanimateCorpses, "ReanimateCorpses", false, false);
            Scribe_Values.Look(ref OnlyBuried, "OnlyBuried", false, false);
            Scribe_Values.Look(ref OnlyNonBuried, "OnlyNonBuried", false, false);
            Scribe_Values.Look(ref OnlyColonists, "OnlyColonists", false, false);
            Scribe_Values.Look(ref OnlyNonColonists, "OnlyNonColonists", false, false);
        }
    }
}