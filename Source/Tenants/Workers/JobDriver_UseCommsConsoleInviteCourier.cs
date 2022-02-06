using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Tenants;

public class JobDriver_UseCommsConsoleInviteCourier : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var toilPawn = pawn;
        var targetA = job.targetA;
        var toilJob = job;
        return toilPawn.Reserve(targetA, toilJob, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate(Toil to)
        {
            var building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            return !building_CommsConsole.CanUseCommsNow;
        });
        var invite = new Toil();
        invite.initAction = delegate
        {
            var actor = invite.actor;
            var building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            if (building_CommsConsole.CanUseCommsNow)
            {
                Events.CourierInvite(building_CommsConsole, actor);
            }
        };
        yield return invite;
    }
}