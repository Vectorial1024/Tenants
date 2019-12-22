﻿using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;


namespace Tenants {
    public class JobDriver_Courier {
        public class JobDriver_UseCommsConsoleInviteCourier : JobDriver {
            public override bool TryMakePreToilReservations(bool errorOnFailed) {
                Pawn pawn = base.pawn;
                LocalTargetInfo targetA = base.job.targetA;
                Job job = base.job;
                return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
            }

            protected override IEnumerable<Toil> MakeNewToils() {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to) {
                    Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    return !building_CommsConsole.CanUseCommsNow;
                });
                Toil invite = new Toil();
                invite.initAction = delegate {
                    Pawn actor = invite.actor;
                    Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    if (building_CommsConsole.CanUseCommsNow) {
                        UtilityCourier.CourierInvite(building_CommsConsole, actor);
                    }
                };
                yield return invite;
            }
        }
        public class JobDriver_SendLetter : JobDriver {
            public override bool TryMakePreToilReservations(bool errorOnFailed) {
                return true;
            }
            protected override IEnumerable<Toil> MakeNewToils() {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Reserve.Reserve(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false);
                Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.A);
                yield return carryToCell;
                yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, false);
                Toil checkMailBox = new Toil();
                checkMailBox.initAction = delegate {
                    Thing building_MailBox = checkMailBox.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    Letter letter = ThingCompUtility.TryGetComp<Letter>(TargetThingB);
                    letter.Skill = pawn.skills.skills.FirstOrDefault(x => x.def.defName.ToLower() == "social").levelInt;
                    building_MailBox.GetMessageBoxComponent().OutgoingLetters.Add(letter);
                    TargetThingB.Destroy();
                };
                yield return checkMailBox;
            }
        }
        public class JobDriver_CheckLetters : JobDriver {
            public override bool TryMakePreToilReservations(bool errorOnFailed) {
                pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
                return true;
            }
            protected override IEnumerable<Toil> MakeNewToils() {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
                Toil CheckLetters = new Toil();
                CheckLetters.initAction = delegate {
                    Thing building_MessageBox = CheckLetters.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    MessageBox comp = building_MessageBox.GetMessageBoxComponent();
                    UtilityCourier.EmptyMessageBox(ref comp.Items, comp.parent.Position);
                    UtilityCourier.RecieveLetters(ref comp.IncomingLetters, comp.parent.Position, comp.parent.Map);
                };
                yield return CheckLetters;
            }
        }

    }
}