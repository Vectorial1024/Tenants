using System.Linq;
using RimWorld;
using Verse;

namespace Tenants;

public class IncidentWorker_TenantCourier : IncidentWorker
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
        if (map != null && maps.Contains(map))
        {
            return Utility.TryFindSpawnSpot(map, out _);
        }

        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;

        var building =
            map?.listerBuildings.allBuildingsColonist.Where(x => x.def == ThingDefOf.Tenants_MailBox);
        return building != null && Events.Courier((Map)parms.target, building.RandomElement());
    }
}