using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_Hauling : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_Hauling()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "TenantWorkHauling")
                {
                    pawnColumnDef.label = "Hauling".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "HaulingTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().MayHaul;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (value &&
                !(pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.ManualDumb) ||
                  pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.Hauling)))
            {
                pawn.workSettings.SetPriority(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"), 3);
                tenantComp.MayHaul = true;
            }
            else
            {
                if (value)
                {
                    Messages.Message("HaulingError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }

                pawn.workSettings.Disable(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Hauling"));
                tenantComp.MayHaul = false;
            }
        }
    }
}