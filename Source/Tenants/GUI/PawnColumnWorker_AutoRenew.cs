using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_AutoRenew : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_AutoRenew()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "AutoRenew")
                {
                    pawnColumnDef.label = "AutoRenew".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "AutoRenewTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().AutoRenew;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            pawn.GetTenantComponent().AutoRenew = value;
        }
    }
}