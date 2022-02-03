using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants
{
    public class LordToil_CourierDeliver : LordToil
    {
        private readonly Thing MailBox;

        public LordToil_CourierDeliver(Thing mailBox)
        {
            MailBox = mailBox;
        }

        public override bool ForceHighStoryDanger => false;
        public override bool AllowSelfTend => true;

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait);
            }

            if (MailBox == null || MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail.Count <= 0)
            {
                return;
            }

            int cost = 0, taken = 0;
            // C# is such that doing it like this will modify the original list
            var courierCostList = MapComponent_Tenants.GetComponent(MailBox.Map).CourierCost;
            if (courierCostList.Count > 0)
            {
                foreach (var thing in courierCostList)
                {
                    cost += thing.stackCount;
                }
            }

            var mailBoxComp = MailBox.GetMailBoxComponent();
            foreach (var thing in MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail)
            {
                if (cost > 0)
                {
                    if (thing.stackCount > cost)
                    {
                        thing.stackCount -= cost;
                        taken += cost;
                        cost = 0;
                    }
                    else
                    {
                        cost -= thing.stackCount;
                        taken += thing.stackCount;
                        thing.stackCount = 0;
                    }
                }

                if (thing.stackCount > 0)
                {
                    mailBoxComp.Items.Add(thing);
                }
            }

            // carefully resolve the courier cost list to reset courier delivery cost
            // each silver taken as cost must match each remaining courier silver cost
            // at this point, "taken" = how many silver taken as cost, and "cost" = how many cost remaining
            int remainingResolve = taken;
            int costTakenInThisDelivery = taken;
            while (remainingResolve > 0 && courierCostList.Count > 0)
            {
                // we inspect this cost silver stack. how many to remove?
                if (courierCostList[0] == null)
                {
                    // invalid cost!
                    courierCostList.RemoveAt(0);
                    continue;
                }
                // we still have something remaining to match
                if (remainingResolve > courierCostList[0].stackCount)
                {
                    // current cost stack not enough to match remaining outstanding costs
                    remainingResolve -= courierCostList[0].stackCount;
                    courierCostList.RemoveAt(0);
                }
                else
                {
                    // current cost stack is enough to match remaining outstanding costs
                    courierCostList[0].stackCount -= remainingResolve;
                    remainingResolve = 0;
                }
            }

            int remainingCost = 0;
            foreach (var costStack in courierCostList)
            {
                // we make no assumptions. calculate the remaining cost from the cost list.
                remainingCost += costStack.stackCount;
            }
            Log.Message("Courier costs: taken " + taken + ", resolve " + remainingResolve + "=0, remaining1 " + cost +  " = remaining2 " + remainingCost);
            MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail.Clear();
            var stringBuilder = new StringBuilder("");
            stringBuilder.Append("MailDelivered".Translate());
            if (taken > 0)
            {
                stringBuilder.Append("CourierCost".Translate(taken));
            }
            if (remainingCost > 0)
            {
                stringBuilder.AppendInNewLine("CourierRemainingCost".Translate(remainingCost));
            }

            Messages.Message(stringBuilder.ToString(), MailBox, MessageTypeDefOf.NeutralEvent);
        }
    }
}