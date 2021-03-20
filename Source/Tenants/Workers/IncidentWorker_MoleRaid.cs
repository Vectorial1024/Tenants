using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tenants
{
    public class IncidentWorker_MoleRaid : IncidentWorker_RaidEnemy
    {
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return "Mole".Translate();
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            try
            {
                var mole = MapComponent_Tenants.GetComponent((Map) parms.target).Moles[0];
                var tenantComp = mole.GetTenantComponent();
                if (Rand.Value < 0.66f)
                {
                    mole.SetFaction(mole.GetTenantComponent().HiddenFaction);
                    tenantComp.IsTenant = false;
                }

                var str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural,
                    parms.faction.Name);
                str += "\n\n";
                str += "TenantMoles".Translate();
                var pawn = pawns.Find(x => x.Faction.leader == x);
                if (pawn != null)
                {
                    str += "\n\n";
                    str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort,
                        pawn.Named("LEADER"));
                }

                MapComponent_Tenants.GetComponent((Map) parms.target).CapturedTenantsToAvenge.Remove(mole);
                return str;
            }
            catch (Exception)
            {
                return Utility.NewBasicRaidMessage(parms, pawns);
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            try
            {
                base.ResolveRaidStrategy(parms, groupKind);
                var mole = MapComponent_Tenants.GetComponent((Map) parms.target).Moles[0];
                if (mole.GetTenantComponent().HiddenFaction.def.techLevel >= TechLevel.Spacer && Rand.Value < 0.5f)
                {
                    parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
                }
            }
            catch (Exception)
            {
                base.ResolveRaidStrategy(parms, groupKind);
            }
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            try
            {
                parms.faction = MapComponent_Tenants.GetComponent((Map) parms.target).Moles[0].GetTenantComponent()
                    .HiddenFaction;

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