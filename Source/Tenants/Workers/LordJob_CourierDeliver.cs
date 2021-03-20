using Verse;
using Verse.AI.Group;

namespace Tenants
{
    public class LordJob_CourierDeliver : LordJob
    {
        private Thing Mailbox;

        public LordJob_CourierDeliver()
        {
        }

        public LordJob_CourierDeliver(Thing mailbox)
        {
            Mailbox = mailbox;
        }

        public override StateGraph CreateGraph()
        {
            var StateGraph = new StateGraph();
            Mailbox ??= Map.listerThings.ThingsOfDef(ThingDefOf.Tenants_MailBox).RandomElement();

            LordToil toilTravel = new LordToil_Travel(Mailbox.Position)
            {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilTravel);
            LordToil toilDeliver = new LordToil_CourierDeliver(Mailbox);
            StateGraph.AddToil(toilDeliver);
            LordToil toilLeave = new LordToil_ExitMap
            {
                useAvoidGrid = true
            };
            StateGraph.AddToil(toilLeave);

            var transitionWait = new Transition(toilTravel, toilDeliver);
            transitionWait.AddTrigger(new Trigger_Memo("TravelArrived"));
            StateGraph.AddTransition(transitionWait);

            var transitionLeave = new Transition(toilDeliver, toilLeave);
            transitionLeave.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(3000));
            StateGraph.AddTransition(transitionLeave);
            return StateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref Mailbox, "Mailbox");
        }
    }
}