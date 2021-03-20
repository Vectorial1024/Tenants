using System.Collections.Generic;
using Verse.AI;

namespace Tenants
{
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
                building_MailBox.GetMailBoxComponent().EmptyMailBox();
            };
            yield return checkMailBox;
        }
    }
}