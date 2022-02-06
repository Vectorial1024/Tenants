using System;
using System.Linq;
using Mlie;
using UnityEngine;
using Verse;

namespace Tenants;

public class ModMain : Mod
{
    private static Vector2 scrollPosition = Vector2.zero;
    private static string currentVersion;
    private readonly TenantsSettings settings;

    public ModMain(ModContentPack content) : base(content)
    {
        settings = GetSettings<TenantsSettings>();
        SettingsHelper.LatestVersion = settings;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.Tenants"));
    }

    public override string SettingsCategory()
    {
        return "Tenants";
    }

    public bool GetGastronomyGuestSetting()
    {
        return settings.GastronomyGuest;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        try
        {
            inRect.yMin += 20;
            inRect.yMax -= 20;
            var list = new Listing_Standard();
            var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var rect2 = new Rect(0f, 0f, inRect.width - 30f, (inRect.height * 2) + settings.RaceViewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
            list.Begin(rect2);
            if (list.ButtonText("TENN.defaultsettings".Translate()))
            {
                settings.Reset();
            }

            list.Label("TENN.mindaily".Translate(settings.MinDailyCost));
            settings.MinDailyCost = (int)Mathf.Round(list.Slider(settings.MinDailyCost, 0, settings.MaxDailyCost));
            list.Label("TENN.maxdaily".Translate(settings.MaxDailyCost));
            settings.MaxDailyCost =
                (int)Mathf.Round(list.Slider(settings.MaxDailyCost, settings.MinDailyCost, 1000));
            list.Label("TENN.mindays".Translate(settings.MinContractTime));
            settings.MinContractTime = (int)Mathf.Round(list.Slider(settings.MinContractTime, 1, 100));
            list.Label("TENN.maxdays".Translate(settings.MaxContractTime));
            settings.MaxContractTime = (int)Mathf.Round(list.Slider(settings.MaxContractTime, 1, 100));
            list.Label("TENN.chancesatisified".Translate(settings.StayChanceHappy));
            settings.StayChanceHappy =
                (int)Mathf.Round(list.Slider(settings.StayChanceHappy, settings.StayChanceNeutral, 100f));
            list.Label("TENN.chanceokay".Translate(settings.StayChanceNeutral));
            settings.StayChanceNeutral =
                (int)Mathf.Round(list.Slider(settings.StayChanceNeutral, settings.StayChanceSad, 100f));
            list.Label("TENN.chancedissatisfied".Translate(settings.StayChanceSad));
            settings.StayChanceSad = (int)Mathf.Round(list.Slider(settings.StayChanceSad, 0f, 100f));
            list.Label("TENN.harborpenalty".Translate(settings.HarborPenalty));
            settings.HarborPenalty = (int)Mathf.Round(list.Slider(settings.HarborPenalty, 1f, 100f));
            list.Label("TENN.accidentspenalty".Translate(settings.OutragePenalty));
            settings.OutragePenalty = (int)Mathf.Round(list.Slider(settings.OutragePenalty, 5f, 100f));
            list.Label("TENN.needlevel".Translate(settings.LevelOfHappinessToWork));
            settings.LevelOfHappinessToWork =
                (byte)Mathf.Round(list.Slider(settings.LevelOfHappinessToWork, 0f, 100f));
            list.Label("TENN.curiercost".Translate(settings.CourierCost));
            settings.CourierCost = (byte)Mathf.Round(list.Slider(settings.CourierCost, 10f, 100f));
            list.Gap();
            list.CheckboxLabeled("TENN.spawnnoweapons".Translate(), ref settings.Weapons,
                "TENN.spawnnoweapons.tooltip".Translate());
            list.Gap();
            list.CheckboxLabeled("TENN.spawnsimpleclothing".Translate(), ref settings.SimpleClothing,
                "TENN.spawnsimpleclothing.tooltip".Translate());
            list.Gap();
            if (settings.SimpleClothing)
            {
                list.Gap();
                list.Label("TENN.apparelmin".Translate(settings.SimpleClothingMin));
                settings.SimpleClothingMin =
                    Mathf.Round(list.Slider(settings.SimpleClothingMin, 0f, settings.SimpleClothingMax));
                list.Label("TENN.apparelmax".Translate(settings.SimpleClothingMax));
                settings.SimpleClothingMax =
                    Mathf.Round(list.Slider(settings.SimpleClothingMax, settings.SimpleClothingMin, 1000f));
            }

            list.Gap();
            list.GapLine();
            list.Label("TENN.types".Translate());
            list.CheckboxLabeled("TENN.mole".Translate(), ref settings.MoleTenants,
                "TENN.mole.tooltip".Translate());
            list.CheckboxLabeled("TENN.wanted".Translate(), ref settings.WantedTenants,
                "TENN.wanted.tooltip".Translate());
            if (ModLister.RoyaltyInstalled)
            {
                list.CheckboxLabeled("TENN.royalty".Translate(), ref settings.RoyaltyTenants,
                    "TENN.royalty.tooltip".Translate());
            }

            list.Gap();
            list.GapLine();
            if (ModLister.GetActiveModWithIdentifier("Orion.Gastronomy") != null)
            {
                list.Label("TENN.gastronomy".Translate());
                list.CheckboxLabeled("TENN.gastronomy.guests".Translate(), ref settings.GastronomyGuest,
                    "TENN.gastronomy.guests.tooltip".Translate());
                list.Gap();
                list.GapLine();
            }

            float R = settings.R, G = settings.G, B = settings.B;
            string buffer1 = R.ToString(), buffer2 = G.ToString(), buffer3 = B.ToString();
            list.Label("TENN.rgbvalue".Translate(ColorUtility.ToHtmlStringRGB(settings.Color)));
            list.TextFieldNumericLabeled("R", ref R, ref buffer1, 0, 255);
            list.TextFieldNumericLabeled("G", ref G, ref buffer2, 0, 255);
            list.TextFieldNumericLabeled("B", ref B, ref buffer3, 0, 255);
            settings.R = R;
            settings.G = G;
            settings.B = B;
            list.Gap();
            list.GapLine();
            if (settings.Races != null && settings.Races.Any())
            {
                list.Label("TENN.races".Translate());
                settings.Filter = list.TextEntryLabeled("TENN.filter".Translate(), settings.Filter);
                var list2 = list.BeginSection(settings.RaceViewHeight);
                list2.ColumnWidth = (rect2.width - 50) / 4;
                foreach (var def in settings.Races)
                {
                    if (!def.defName.ToUpper().Contains(settings.Filter.ToUpper()))
                    {
                        continue;
                    }

                    var contains = settings.AvailableRaces.Contains(def.defName);
                    list2.CheckboxLabeled(def.defName, ref contains, "");
                    if (contains == false && settings.AvailableRaces.Contains(def.defName))
                    {
                        settings.AvailableRaces.Remove(def.defName);
                    }
                    else if (contains && !settings.AvailableRaces.Contains(def.defName))
                    {
                        settings.AvailableRaces.Add(def.defName);
                    }
                }

                list.EndSection(list2);
            }

            if (currentVersion != null)
            {
                list.Gap();
                GUI.contentColor = Color.gray;
                list.Label("TENN.version".Translate(currentVersion));
                GUI.contentColor = Color.white;
            }

            list.End();
            Widgets.EndScrollView();
            settings.Write();
        }
        catch (Exception ex)
        {
            Log.Message(ex.Message);
        }
    }
}