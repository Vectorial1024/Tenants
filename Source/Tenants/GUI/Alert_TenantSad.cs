using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Tenants.UI
{
    public class Alert_TenantSad : Alert
    {
        private List<Pawn> SadTenants
        {
            get
            {
                var returnValue = new List<Pawn>();
                var maps = Find.Maps;
                foreach (var map in maps)
                {
                    if (!map.IsPlayerHome)
                    {
                        continue;
                    }

                    foreach (var item in map.mapPawns.FreeColonistsSpawned)
                    {
                        if (item.GetTenantComponent().IsTenant && item.needs.mood.CurInstantLevel <
                            item.mindState.mentalBreaker.BreakThresholdMinor)
                        {
                            returnValue.Add(item);
                        }
                    }
                }

                return returnValue;
            }
        }

        public override string GetLabel()
        {
            return "TenantSad".Translate(SadTenants.Count.ToStringCached());
        }

        public override TaggedString GetExplanation()
        {
            var stringBuilder = new StringBuilder();
            foreach (var sadTenant in SadTenants)
            {
                stringBuilder.AppendLine("    " + sadTenant.LabelShort.CapitalizeFirst());
            }

            return "TenantSadDesc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            if (GenDate.DaysPassed < 1)
            {
                return false;
            }

            return AlertReport.CulpritsAre(SadTenants);
        }
    }
}