using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tenants
{
    public class IncidentWorker_Opportunists : IncidentWorker_RaidEnemy
    {
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return "Opportunists".Translate();
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            try
            {
                MapComponent_Tenants.GetComponent((Map) parms.target).Broadcast = false;
                var basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                    parms.faction.Name);
                basic += "\n\n";
                basic += "TenantOpportunists".Translate();
                var leader = pawns.Find(x => x.Faction.leader == x);
                if (leader == null)
                {
                    return basic;
                }

                basic += "\n\n";
                basic += "EnemyRaidLeaderPresent".Translate(leader.Faction.def.pawnsPlural, leader.LabelShort,
                    leader.Named("LEADER"));
                return basic;
            }
            catch (Exception)
            {
                return base.GetLetterText(parms, pawns);
            }
        }
    }
}