using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Tenants;

public class JobDriver_CheckMailBox : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
        var checkMailBox = new Toil();
        checkMailBox.initAction = delegate
        {
            var building_MailBox = checkMailBox.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            var mailbox = building_MailBox.GetMailBoxComponent();
            if (mailbox == null)
            {
                // no mailbox!
                Log.Error(
                    "Cannot check mail: no Mailbox component in Mailbox building. Please try rebuilding the Mailbox building.");
                return;
            }

            // has mailbox
            mailbox.SelfCheck();
            mailbox.EmptyMailBox();
        };
        yield return checkMailBox;
    }
}