using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_Basic : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_Basic()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "TenantWorkBasic")
                {
                    pawnColumnDef.label = "Basic".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "BasicTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().MayBasic;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (value && !pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.ManualDumb))
            {
                pawn.workSettings.SetPriority(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"), 3);
                tenantComp.MayBasic = true;
            }
            else
            {
                if (value)
                {
                    Messages.Message("BasicError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }

                pawn.workSettings.Disable(
                    DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "BasicWorker"));
                tenantComp.MayBasic = false;
            }
        }
    }
}