﻿using System;
using System.Collections.Generic;
using Mlie;
using UnityEngine;
using Verse;

namespace Skeleton;

[StaticConstructorOnStartup]
internal class SkeletonMod : Mod
{
    private static string currentVersion;
    private static Vector2 scrollPosition;

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
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.WaSkeleton"));
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
        var scrollContentRect = rect;
        scrollContentRect.height = (rect.height * 0.8f) + (Skeleton.allGraves.Count * 25f);
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var listing_Standard = new Listing_Standard();
        Widgets.BeginScrollView(rect, ref scrollPosition, scrollContentRect);
        listing_Standard.Begin(scrollContentRect);
        listing_Standard.Gap();
        var OnlyBuriedPre = Settings.OnlyBuried;
        var OnlyNonBuriedPre = Settings.OnlyNonBuried;
        var OnlyColonistsPre = Settings.OnlyColonists;
        var OnlyNonColonistsPre = Settings.OnlyNonColonists;

        listing_Standard.CheckboxLabeled("ExplodeOnDeath_Label".Translate(), ref Settings.ExplodeOnDeath,
            "ExplodeOnDeath_Tooltip".Translate());
        listing_Standard.CheckboxLabeled("AddHediffToAll_Label".Translate(), ref Settings.AddHediffToAll,
            "AddHediffToAll_Tooltip".Translate());
        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("ReanimateCorpses_Label".Translate(), ref Settings.ReanimateCorpses,
            "ReanimateCorpses_Tooltip".Translate());
        if (Settings.ReanimateCorpses)
        {
            listing_Standard.CheckboxLabeled("EnemyIsHostile_Label".Translate(), ref Settings.EnemyIsHostile,
                "EnemyIsHostile_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("AllowZombies_Label".Translate(), ref Settings.AllowZombies,
                "AllowZombies_Tooltip".Translate());
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("OnlyBuried_Label".Translate(), ref Settings.OnlyBuried,
                "OnlyBuried_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("OnlyNonBuried_Label".Translate(), ref Settings.OnlyNonBuried,
                "OnlyNonBuried_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("OnlyColonists_Label".Translate(), ref Settings.OnlyColonists,
                "OnlyColonists_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("OnlyNonColonists_Label".Translate(), ref Settings.OnlyNonColonists,
                "OnlyNonColonists_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("OnlyDuringEclipse_Label".Translate(), ref Settings.OnlyDuringEclipse,
                "OnlyDuringEclipse_Tooltip".Translate());
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("AllAtOnce_Label".Translate(), ref Settings.AllAtOnce,
                "AllAtOnce_Tooltip".Translate());
            listing_Standard.Gap();
            listing_Standard.Label("ReanimateChance_Label".Translate(Math.Round(Settings.ReanimateChance * 100)),
                -1f,
                "ReanimateChance_Description".Translate());
            Settings.ReanimateChance = listing_Standard.Slider(Settings.ReanimateChance, 0, 1f);

            if (!Settings.OnlyNonBuried)
            {
                listing_Standard.Label("SecureGraves_Label".Translate(), -1, "SecureGraves_Tooltip".Translate());
                if (Settings.SecureGraves == null)
                {
                    Settings.SecureGraves = new List<string>();
                }

                foreach (var grave in Skeleton.allGraves)
                {
                    var tempValue = Settings.SecureGraves.Contains(grave.defName);
                    listing_Standard.CheckboxLabeled($"{grave.LabelCap} ({grave.defName})", ref tempValue,
                        grave.description);
                    if (tempValue && !Settings.SecureGraves.Contains(grave.defName))
                    {
                        Settings.SecureGraves.Add(grave.defName);
                    }

                    if (!tempValue && Settings.SecureGraves.Contains(grave.defName))
                    {
                        Settings.SecureGraves.Remove(grave.defName);
                    }
                }
            }
        }

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("VerboseLogging_Label".Translate(), ref Settings.VerboseLogging,
            "VerboseLogging_Tooltip".Translate());

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CurrentModVersion_Label".Translate(currentVersion));
            GUI.contentColor = Color.white;
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
        Widgets.EndScrollView();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        Skeleton.ScanMapsForCorpses();
        Skeleton.ScanMapsForUnaffectedSkeletons();
    }
}