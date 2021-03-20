using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_Cleaning : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_Cleaning()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "TenantWorkCleaning")
                {
                    pawnColumnDef.label = "Cleaning".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "CleaningTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().MayClean;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (value &&
                !(pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.ManualDumb) ||
                  pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.Cleaning)))
            {
                pawn.workSettings.SetPriority(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"), 3);
                tenantComp.MayClean = true;
            }
            else
            {
                if (value)
                {
                    Messages.Message("CleaningError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }

                pawn.workSettings.Disable(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Cleaning"));
                tenantComp.MayClean = false;
            }
        }
    }
}