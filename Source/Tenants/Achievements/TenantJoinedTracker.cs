using AchievementsExpanded;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Tenants.Achievements
{
    public class TenantJoinedTracker : TrackerBase
    {
        public override string Key => "TenantJoinedTracker";

        public override Func<bool> AttachToLongTick => () => Trigger();

        protected override string[] DebugText => new string[] { $"Count: {count}" };

        public TenantJoinedTracker()
        {
        }

        public TenantJoinedTracker(TenantJoinedTracker reference) : base(reference)
        {
            count = reference.count;
        }

        public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref count, "count", 1);
        }
        public override bool Trigger()
        {
            base.Trigger();
            var factionPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
            if (factionPawns is null)
            {
                return false;
            }
            var tenants = (from tenant in factionPawns where tenant.GetTenantComponent() != null && !tenant.GetTenantComponent().IsTenant && tenant.GetTenantComponent().HiddenFaction != null select tenant);
            triggeredCount = tenants.Count();

            return triggeredCount >= count;
        }


        public override bool UnlockOnStartup => Trigger();

        public int count = 1;
        [Unsaved]
        protected int triggeredCount = 0; //Only for display
    }
}
