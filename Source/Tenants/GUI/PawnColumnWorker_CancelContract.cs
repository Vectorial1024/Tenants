using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_CancelContract : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_CancelContract()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "CancelContract")
                {
                    pawnColumnDef.label = "Terminate".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "TerminateTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().IsTerminated;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            pawn.GetTenantComponent().IsTerminated = value;
        }
    }
}