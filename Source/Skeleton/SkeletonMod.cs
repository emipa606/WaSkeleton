using UnityEngine;
using Verse;

namespace Skeleton
{
    [StaticConstructorOnStartup]
    internal class SkeletonMod : Mod
    {
        /// <summary>
        ///     The private settings
        /// </summary>
        private SkeletonSettings settings;

        /// <summary>
        ///     Cunstructor
        /// </summary>
        /// <param name="content"></param>
        public SkeletonMod(ModContentPack content) : base(content)
        {
        }

        /// <summary>
        ///     The instance-settings for the mod
        /// </summary>
        private SkeletonSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = GetSettings<SkeletonSettings>();
                }

                return settings;
            }
            set => settings = value;
        }

        /// <summary>
        ///     The title for the mod-settings
        /// </summary>
        /// <returns></returns>
        public override string SettingsCategory()
        {
            return "Wa! Skeleton!";
        }

        /// <summary>
        ///     The settings-window
        ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
        /// </summary>
        /// <param name="rect"></param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.Gap();
            var OnlyBuriedPre = Settings.OnlyBuried;
            var OnlyNonBuriedPre = Settings.OnlyNonBuried;
            var OnlyColonistsPre = Settings.OnlyColonists;
            var OnlyNonColonistsPre = Settings.OnlyNonColonists;

            listing_Standard.CheckboxLabeled("ExplodeOnDeath_Label".Translate(), ref Settings.ExplodeOnDeath,
                "ExplodeOnDeath_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("AddHediffToAll_Label".Translate(), ref Settings.AddHediffToAll,
                "AddHediffToAll_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("ReanimateCorpses_Label".Translate(), ref Settings.ReanimateCorpses,
                "ReanimateCorpses_Tooltip".Translate());
            if (Settings.ReanimateCorpses)
            {
                listing_Standard.CheckboxLabeled("OnlyBuried_Label".Translate(), ref Settings.OnlyBuried,
                    "OnlyBuried_Tooltip".Translate());
                listing_Standard.CheckboxLabeled("OnlyNonBuried_Label".Translate(), ref Settings.OnlyNonBuried,
                    "OnlyNonBuried_Tooltip".Translate());
                listing_Standard.CheckboxLabeled("OnlyColonists_Label".Translate(), ref Settings.OnlyColonists,
                    "OnlyColonists_Tooltip".Translate());
                listing_Standard.CheckboxLabeled("OnlyNonColonists_Label".Translate(), ref Settings.OnlyNonColonists,
                    "OnlyNonColonists_Tooltip".Translate());
                listing_Standard.CheckboxLabeled("AllowZombies_Label".Translate(), ref Settings.AllowZombies,
                    "AllowZombies_Tooltip".Translate());
            }

            if (OnlyBuriedPre && Settings.OnlyNonBuried)
            {
                Settings.OnlyBuried = false;
            }

            if (OnlyNonBuriedPre && Settings.OnlyBuried)
            {
                Settings.OnlyNonBuried = false;
            }

            if (OnlyColonistsPre && Settings.OnlyNonColonists)
            {
                Settings.OnlyColonists = false;
            }

            if (OnlyNonColonistsPre && Settings.OnlyColonists)
            {
                Settings.OnlyNonColonists = false;
            }

            listing_Standard.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Skeleton.ScanMapsForCorpses();
            Skeleton.ScanMapsForUnaffectedSkeletons();
        }
    }
}