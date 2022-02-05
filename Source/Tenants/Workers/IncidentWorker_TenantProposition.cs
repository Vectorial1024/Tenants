using System.Linq;
using RimWorld;
using Verse;

namespace Tenants;

public class IncidentWorker_TenantProposition : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms))
        {
            return false;
        }

        if (parms.target == null)
        {
            return false;
        }

        var map = (Map)parms.target;
        var maps = Find.Maps.Where(x => x.IsPlayerHome).ToList();
        if (map == null || !maps.Contains(map))
        {
            return false;
        }

        var pawn = map.mapPawns.FreeColonists.FirstOrDefault(x =>
            x.GetTenantComponent().IsTenant == false && !x.Dead);
        if (pawn != null)
        {
            return Utility.TryFindSpawnSpot(map, out _);
        }

        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        if (map == null)
        {
            return false;
        }

        var pawn = map.mapPawns.FreeColonists.FirstOrDefault(x =>
            x.GetTenantComponent().IsTenant == false && !x.Dead);
        var building =
            map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def == ThingDefOf.Tenants_MailBox);
        if (pawn != null && building != null)
        {
            return Events.ContractTenancy((Map)parms.target);
        }

        return false;
    }
}