using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Tenants
{
    public class TenantsSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref AvailableRaces, "AvailableRaces", LookMode.Value);
            Scribe_Values.Look(ref MinDailyCost, "MinDailyCost", minDailyCost);
            Scribe_Values.Look(ref MaxDailyCost, "MaxDailyCost", maxDailyCost);
            Scribe_Values.Look(ref MinContractTime, "MinContractTime", minContractTime);
            Scribe_Values.Look(ref MaxContractTime, "MaxContractTime", maxContractTime);
            Scribe_Values.Look(ref StayChanceHappy, "StayChanceHappy", stayChanceHappy);
            Scribe_Values.Look(ref StayChanceNeutral, "StayChanceNeutral", stayChanceNeutral);
            Scribe_Values.Look(ref StayChanceSad, "StayChanceSad", stayChanceSad);
            Scribe_Values.Look(ref HarborPenalty, "HarborPenalty", harborPenalty);
            Scribe_Values.Look(ref OutragePenalty, "OutragePenalty", outragePenalty);
            Scribe_Values.Look(ref Weapons, "Weapons", weapons);
            Scribe_Values.Look(ref SimpleClothing, "SimpleClothing", simpleClothing);
            Scribe_Values.Look(ref MoleTenants, "MoleTenants", moleTenants);
            Scribe_Values.Look(ref WantedTenants, "WantedTenants", wantedTenants);
            Scribe_Values.Look(ref RoyaltyTenants, "RoyaltyTenants", royaltyTenants);
            Scribe_Values.Look(ref SimpleClothingMin, "SimpleClothingMin", simpleClothingMin);
            Scribe_Values.Look(ref SimpleClothingMax, "SimpleClothingMax", simpleClothingMax);
            Scribe_Values.Look(ref CourierCost, "CourierCost", courierCost);
            Scribe_Values.Look(ref r, "R", r);
            Scribe_Values.Look(ref g, "G", g);
            Scribe_Values.Look(ref b, "B", b);
            Scribe_Values.Look(ref LevelOfHappinessToWork, "LevelOfHappinessToWork", levelOfHappinessToWork);
            Scribe_Values.Look(ref GastronomyGuest, "GastronomyGuest", gastronomyGuest);

            // verify selected races exists
            var correctRaces = new List<string>();
            foreach (var raceDefName in AvailableRaces)
            {
                if (DefDatabase<ThingDef>.GetNamedSilentFail(raceDefName) != null)
                {
                    correctRaces.Add(raceDefName);
                }
            }

            AvailableRaces = correctRaces;
        }

        internal void Reset()
        {
            AvailableRaces = availableRaces.Count > 0 ? availableRaces.ListFullCopy() : new List<string>();
            MinDailyCost = minDailyCost;
            MaxDailyCost = maxDailyCost;
            MinContractTime = minContractTime;
            MaxContractTime = maxContractTime;
            StayChanceHappy = stayChanceHappy;
            StayChanceNeutral = stayChanceNeutral;
            StayChanceSad = stayChanceSad;
            R = 127f;
            G = 63f;
            B = 191f;
            LevelOfHappinessToWork = levelOfHappinessToWork;
            Weapons = weapons;
            SimpleClothing = simpleClothing;
            SimpleClothingMin = simpleClothingMin;
            SimpleClothingMax = simpleClothingMax;
            GastronomyGuest = gastronomyGuest;
        }

        #region Fields

        private static readonly List<string> availableRaces = new() {"Human"};
        private static readonly int minDailyCost = 50;
        private static readonly int maxDailyCost = 100;
        private static readonly int minContractTime = 3;
        private static readonly int maxContractTime = 7;
        private static readonly float stayChanceHappy = 95F;
        private static readonly float stayChanceNeutral = 50F;
        private static readonly float stayChanceSad = 5f;
        private static readonly int harborPenalty = 5;
        private static readonly int outragePenalty = 8;
        private static readonly bool weapons = true;
        private static readonly bool simpleClothing = true;
        private static readonly bool moleTenants = true;
        private static readonly bool wantedTenants = true;
        private static readonly bool royaltyTenants = true;
        private static readonly int simpleClothingMin = 100;
        private static readonly int simpleClothingMax = 300;
        private static readonly int courierCost = 30;
        private static float r = 127f, g = 63f, b = 191f;
        private static Color color = new(r / 255f, g / 255f, b / 255f);
        private static readonly float levelOfHappinessToWork = 70f;
        private static readonly bool gastronomyGuest = false;

        #endregion Fields

        #region Properties

        public List<string> AvailableRaces =
            availableRaces.Count > 0 ? availableRaces.ListFullCopy() : new List<string>();

        public readonly IEnumerable<ThingDef> Races = DefDatabase<PawnKindDef>.AllDefsListForReading
            .Where(x => x.race != null && x.RaceProps.Humanlike && x.RaceProps.IsFlesh &&
                        x.RaceProps.ResolvedDietCategory != DietCategory.NeverEats).Select(s => s.race).Distinct();

        public readonly float RaceViewHeight = 300f;
        public int MinDailyCost = minDailyCost;
        public int MaxDailyCost = maxDailyCost;
        public int MinContractTime = minContractTime;
        public int MaxContractTime = maxContractTime;
        public float StayChanceHappy = stayChanceHappy;
        public float StayChanceNeutral = stayChanceNeutral;
        public float StayChanceSad = stayChanceSad;
        public int HarborPenalty = harborPenalty;
        public int OutragePenalty = outragePenalty;
        public bool Weapons = weapons;
        public bool SimpleClothing = simpleClothing;
        public bool MoleTenants = moleTenants;
        public bool WantedTenants = wantedTenants;
        public bool RoyaltyTenants = royaltyTenants;
        public float SimpleClothingMin = simpleClothingMin;
        public float SimpleClothingMax = simpleClothingMax;
        public float CourierCost = courierCost;
        public bool GastronomyGuest = gastronomyGuest;
        public string Filter { get; set; }

        public float R
        {
            get => r;
            set
            {
                r = value;
                color = new Color(r / 255, g / 255, b / 255);
            }
        }

        public float G
        {
            get => g;
            set
            {
                g = value;
                color = new Color(r / 255, g / 255, b / 255);
            }
        }

        public float B
        {
            get => b;
            set
            {
                b = value;
                color = new Color(r / 255, g / 255, b / 255);
            }
        }

        public Color Color => color;
        public float LevelOfHappinessToWork = levelOfHappinessToWork;

        #endregion Properties
    }
}