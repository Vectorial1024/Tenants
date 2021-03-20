using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Tenants
{
    public class LordToil_TenantStealCover : LordToil_DoOpportunisticTaskOrCover
    {
        protected override DutyDef DutyDef => DutyDefOf.Steal;
        public override bool ForceHighStoryDanger => false;
        public override bool AllowSelfTend => false;

        protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target,
            List<Thing> alreadyTakenTargets)
        {
            if (pawn.mindState.duty == null || pawn.mindState.duty.def != DutyDef ||
                pawn.carryTracker.CarriedThing == null)
            {
                return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 33f, out target, pawn,
                    alreadyTakenTargets);
            }

            target = pawn.carryTracker.CarriedThing;
            return true;
        }
    }
}