using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tenants
{
    public class IncidentWorker_Wanted : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var canFire = base.CanFireNowSub(parms);

            if (MapComponent_Tenants.GetComponent((Map) parms.target).WantedTenants.Count < 1)
            {
                canFire = false;
            }

            return canFire;
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return "Wanted".Translate();
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            try
            {
                if (MapComponent_Tenants.GetComponent((Map) parms.target).WantedTenants.Count > 0)
                {
                    MapComponent_Tenants.GetComponent((Map) parms.target).WantedTenants.RemoveAt(0);
                }

                var basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                    parms.faction.Name);
                basic += "\n\n";
                basic += "WantedTenant".Translate();
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

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            try
            {
                parms.faction = MapComponent_Tenants.GetComponent((Map) parms.target).WantedTenants[0]
                    .GetTenantComponent().WantedBy;
                if (FactionCanBeGroupSource(parms.faction, (Map) parms.target))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return base.TryResolveRaidFaction(parms);
            }
        }
    }
}