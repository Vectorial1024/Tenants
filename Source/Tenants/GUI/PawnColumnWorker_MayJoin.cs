using RimWorld;
using Verse;

namespace Tenants
{
    public class PawnColumnWorker_MayJoin : PawnColumnWorker_Checkbox
    {
        public PawnColumnWorker_MayJoin()
        {
            foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
            {
                if (pawnColumnDef.defName == "MayJoin")
                {
                    pawnColumnDef.label = "MayJoin".Translate();
                }
            }
        }

        protected override string GetTip(Pawn pawn)
        {
            return "MayJoinTip".Translate();
        }

        protected override bool GetValue(Pawn pawn)
        {
            return pawn.GetTenantComponent().MayJoin;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            pawn.GetTenantComponent().MayJoin = value;
        }
    }
}