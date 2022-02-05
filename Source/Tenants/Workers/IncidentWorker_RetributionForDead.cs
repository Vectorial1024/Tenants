using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Tenants;

public class IncidentWorker_RetributionForDead : IncidentWorker_RaidEnemy
{
    protected override string GetLetterLabel(IncidentParms parms)
    {
        return "Retribution".Translate();
    }

    protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
    {
        try
        {
            var related = pawns[pawns.Count - 1];
            if (MapComponent_Tenants.GetComponent(related.Map).DeadTenantsToAvenge.Count > 0)
            {
                var dead = MapComponent_Tenants.GetComponent(related.Map).DeadTenantsToAvenge[0];
                if (dead.ageTracker.AgeBiologicalYears > 25)
                {
                    related.relations.AddDirectRelation(PawnRelationDefOf.Parent, dead);
                }
                else
                {
                    dead.relations.AddDirectRelation(PawnRelationDefOf.Parent, related);
                }

                var str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                    parms.faction.Name);
                str += "\n\n";
                str += "TenantDeathRetribution".Translate(
                    related.GetRelations(dead).FirstOrDefault()?.GetGenderSpecificLabel(dead),
                    related.Named("PAWN"));
                var pawn = pawns.Find(x => x.Faction.leader == x);
                if (pawn != null)
                {
                    str += "\n\n";
                    str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort,
                        pawn.Named("LEADER"));
                }

                MapComponent_Tenants.GetComponent(pawns[0].Map).DeadTenantsToAvenge.Remove(dead);
                return str;
            }

            var basic = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                parms.faction.Name);
            basic += "\n\n";
            basic += parms.raidStrategy.arrivalTextEnemy;
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
            return Utility.NewBasicRaidMessage(parms, pawns);
        }
    }
}