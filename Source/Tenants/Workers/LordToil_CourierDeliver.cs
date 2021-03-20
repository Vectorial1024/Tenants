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
            if (MapComponent_Tenants.GetComponent(MailBox.Map).CourierCost.Count > 0)
            {
                foreach (var thing in MapComponent_Tenants.GetComponent(MailBox.Map).CourierCost)
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

            MapComponent_Tenants.GetComponent(MailBox.Map).IncomingMail.Clear();
            var stringBuilder = new StringBuilder("");
            stringBuilder.Append("MailDelivered".Translate());
            if (taken > 0)
            {
                stringBuilder.Append("CourierCost".Translate(taken));
            }

            Messages.Message(stringBuilder.ToString(), MailBox, MessageTypeDefOf.NeutralEvent);
        }
    }
}