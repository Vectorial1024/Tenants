using RimWorld;
using Verse;

namespace Tenants;

public class PawnColumnWorker_FireFight : PawnColumnWorker_Checkbox
{
    public PawnColumnWorker_FireFight()
    {
        foreach (var pawnColumnDef in DefDatabase<PawnColumnDef>.AllDefs)
        {
            if (pawnColumnDef.defName == "TenantWorkFireFighting")
            {
                pawnColumnDef.label = "FireFighting".Translate();
            }
        }
    }

    protected override string GetTip(Pawn pawn)
    {
        return "FireFightingTip".Translate();
    }

    protected override bool GetValue(Pawn pawn)
    {
        return pawn.GetTenantComponent().MayFirefight;
    }

    protected override void SetValue(Pawn pawn, bool value, PawnTable table)
    {
        var tenantComp = pawn.GetTenantComponent();
        if (value &&
            !pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.Firefighting))
        {
            pawn.workSettings.SetPriority(
                DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"), 3);
            tenantComp.MayFirefight = true;
        }
        else
        {
            if (value)
            {
                Messages.Message("FireFightingError".Translate(pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            }

            pawn.workSettings.Disable(
                DefDatabase<WorkTypeDef>.AllDefs.FirstOrFallback(x => x.defName == "Firefighter"));
            tenantComp.MayFirefight = false;
        }
    }
}