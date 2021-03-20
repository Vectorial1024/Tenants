using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Tenants
{
    public class ModMain : Mod
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private readonly TenantsSettings settings;

        public ModMain(ModContentPack content) : base(content)
        {
            settings = GetSettings<TenantsSettings>();
            SettingsHelper.LatestVersion = settings;
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
                if (list.ButtonText("Default Settings"))
                {
                    settings.Reset();
                }

                list.Label($"Minimum daily tenant contract payment ({settings.MinDailyCost}).");
                settings.MinDailyCost = (int) Mathf.Round(list.Slider(settings.MinDailyCost, 0, 100));
                list.Label($"Maximum daily tenant contract payment ({settings.MaxDailyCost}).");
                settings.MaxDailyCost =
                    (int) Mathf.Round(list.Slider(settings.MaxDailyCost, settings.MinDailyCost, 1000));
                list.Label($"Minimum contracted days ({settings.MinContractTime}).");
                settings.MinContractTime = (int) Mathf.Round(list.Slider(settings.MinContractTime, 1, 100));
                list.Label($"Maximum contracted days ({settings.MaxContractTime}).");
                settings.MaxContractTime = (int) Mathf.Round(list.Slider(settings.MaxContractTime, 1, 100));
                list.Label($"({settings.StayChanceHappy}) Chance of contract extension when the tenant is satisfied.");
                settings.StayChanceHappy =
                    (int) Mathf.Round(list.Slider(settings.StayChanceHappy, settings.StayChanceNeutral, 100f));
                list.Label($"({settings.StayChanceNeutral}) Chance of contract extension when the tenant is okay.");
                settings.StayChanceNeutral =
                    (int) Mathf.Round(list.Slider(settings.StayChanceNeutral, settings.StayChanceSad, 100f));
                list.Label($"({settings.StayChanceSad}) Chance of contract extension when the tenant is dissatisfied.");
                settings.StayChanceSad = (int) Mathf.Round(list.Slider(settings.StayChanceSad, 0f, 100f));
                list.Label($"Faction penalty to relations for harboring fugitives. ({settings.HarborPenalty})");
                settings.HarborPenalty = (int) Mathf.Round(list.Slider(settings.HarborPenalty, 1f, 100f));
                list.Label($"Faction penalty to relations for tenancy accidents. ({settings.OutragePenalty})");
                settings.OutragePenalty = (int) Mathf.Round(list.Slider(settings.OutragePenalty, 5f, 100f));
                list.Label($"Needed level of tenancy happiness to do labor: ({settings.LevelOfHappinessToWork}).");
                settings.LevelOfHappinessToWork =
                    (byte) Mathf.Round(list.Slider(settings.LevelOfHappinessToWork, 0f, 100f));
                list.Label($"Basic courier cost: ({settings.CourierCost}).");
                settings.CourierCost = (byte) Mathf.Round(list.Slider(settings.CourierCost, 10f, 100f));
                list.Gap();
                list.CheckboxLabeled("Should tenants spawn without weapons?", ref settings.Weapons,
                    "Keep in mind that this removes any weapon when a tenant spawns. Have you given a weapon to a tenant once before, it'll be removed should they leave the map and spawn again somewhere.");
                list.Gap();
                list.CheckboxLabeled("Should tenants spawn simpler clothing?", ref settings.SimpleClothing,
                    "Upon tenant creation, tenants will spawn with simpler clothing within the selected value range.");
                list.Gap();
                if (settings.SimpleClothing)
                {
                    list.Gap();
                    list.Label($"Min total apparel value ({settings.SimpleClothingMin}).");
                    settings.SimpleClothingMin = Mathf.Round(list.Slider(settings.SimpleClothingMin, 0f, 500f));
                    list.Label($"Max total apparel value ({settings.SimpleClothingMax}).");
                    settings.SimpleClothingMax = Mathf.Round(list.Slider(settings.SimpleClothingMax, 0f, 1000f));
                }

                list.Gap();
                list.GapLine();
                list.Label("Tenant types allowed");
                list.CheckboxLabeled("Mole tenants", ref settings.MoleTenants,
                    "Some tenants can be moles from hostile factions and will send info for an attack.");
                list.CheckboxLabeled("Wanted tenants", ref settings.WantedTenants,
                    "Some tenants can be wanted by other factions that will not like the helping of a fugitive.");
                if (ModLister.GetActiveModWithIdentifier("Ludeon.RimWorld.Royalty") != null)
                {
                    list.CheckboxLabeled("Royalty tenants", ref settings.RoyaltyTenants,
                        "Allow tenants with royal titles that needs to be satisfied.");
                }

                list.Gap();
                list.GapLine();
                if (ModLister.GetActiveModWithIdentifier("Orion.Gastronomy") != null)
                {
                    list.Label("Gastronomy settings");
                    list.CheckboxLabeled("Should Tennants count as guests?", ref settings.GastronomyGuest,
                        "If checked Tennants count as guests, if unchecked they count as colonists");
                    list.Gap();
                    list.GapLine();
                }

                float R = settings.R, G = settings.G, B = settings.B;
                string buffer1 = R.ToString(), buffer2 = G.ToString(), buffer3 = B.ToString();
                list.Label("RGB value for tenants name: <color=#" + ColorUtility.ToHtmlStringRGB(settings.Color) + ">" +
                           "Color" + "</color>");
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
                    list.Label("Available races");
                    settings.Filter = list.TextEntryLabeled("Filter:", settings.Filter);
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
}