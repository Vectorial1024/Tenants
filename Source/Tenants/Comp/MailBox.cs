using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Tenants
{
    public class MailBox : ThingComp
    {
        public List<Thing> Items = new();

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref Items, "Items", LookMode.Reference);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            var list = new List<FloatMenuOption>();
            if (Items.Count <= 0)
            {
                return list.AsEnumerable();
            }

            void CheckInventory()
            {
                var job = new Job(JobDefOf.JobCheckMailBox, parent);
                pawn.jobs.TryTakeOrderedJob(job);
            }

            var checkMailBox = new FloatMenuOption("CheckMailBox".Translate(), CheckInventory, MenuOptionPriority.High);
            list.Add(checkMailBox);
            //void addItem() {
            //    Items.Add(ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver));
            //}
            //FloatMenuOption add = new FloatMenuOption("Add".Translate(), addItem, MenuOptionPriority.High);
            //list.Add(add);
            return list.AsEnumerable();
        }

        public void SelfCheck()
        {
            // attempts to recover from known inconsistency errors; basically patches itself

            // known consistency error: Items list has many elements but the items are null
            // this error results in users being able to check mail many times with no obvious feedback
            // we will fix by removing all null elements
            // lost silver payments will not be reimbursed!
            if (Items.Count > 0)
            {
                // remove the errored elements by index
                Stack<int> erroredIndices = new Stack<int>();
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null)
                    {
                        erroredIndices.Push(i);
                    }
                }
                while (erroredIndices.Count > 0)
                {
                    Items.RemoveAt(erroredIndices.Pop());
                }
            }

            // no more known consistency errors
            return;
        }

        public void EmptyMailBox()
        {
            if (Items.Count <= 0)
            {
                return;
            }

            foreach (var thing in Items)
            {
                DebugThingPlaceHelper.DebugSpawn(thing.def, parent.Position, thing.stackCount);
            }

            Items.Clear();
        }
    }
}