using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Tenants;

public class IncidentWorker_RetributionForCaptured : IncidentWorker_RaidEnemy
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
            var captured = MapComponent_Tenants.GetComponent(related.Map).CapturedTenantsToAvenge[0];

            if (captured.ageTracker.AgeBiologicalYears > 25)
            {
                related.relations.AddDirectRelation(PawnRelationDefOf.Parent, captured);
            }
            else
            {
                captured.relations.AddDirectRelation(PawnRelationDefOf.Parent, related);
            }

            var str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                parms.faction.Name);
            str += "\n\n";
            str += "TenantCapturedRetribution".Translate(
                related.GetRelations(captured).FirstOrDefault()?.GetGenderSpecificLabel(captured),
                related.Named("PAWN"));
            var pawn = pawns.Find(x => x.Faction.leader == x);
            if (pawn != null)
            {
                str += "\n\n";
                str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort,
                    pawn.Named("LEADER"));
            }

            MapComponent_Tenants.GetComponent(pawns[0].Map).CapturedTenantsToAvenge.Remove(captured);
            return str;
        }
        catch (Exception)
        {
            return Utility.NewBasicRaidMessage(parms, pawns);
        }
    }
}