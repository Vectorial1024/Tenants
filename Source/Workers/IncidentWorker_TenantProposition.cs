﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Tenants {
    public class IncidentWorker_TenantProposition : IncidentWorker {
        private const float RelationWithColonistWeight = 20f;
        protected override bool CanFireNowSub(IncidentParms parms) {
            if (!base.CanFireNowSub(parms)) {
                return false;
            }
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false && !x.Dead);
                    if (pawn != null)
                        return Utility.TryFindSpawnSpot(map, out IntVec3 spawnSpot);
                }
            }
            return false;
        }


        protected override bool TryExecuteWorker(IncidentParms parms) {
            //Map and spot finder.
            if (parms.target != null) {
                Map map = (Map)parms.target;
                if (map != null) {
                    Pawn pawn = map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false && !x.Dead);
                    if (pawn != null)
                        return Utility.ContractGenerateNew((Map)parms.target);

                }
            }
            return false;
        }
    }
}