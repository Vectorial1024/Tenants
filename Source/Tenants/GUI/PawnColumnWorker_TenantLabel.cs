﻿using RimWorld;
using UnityEngine;
using Verse;

namespace Tenants;

public class PawnColumnWorker_TenantLabel : PawnColumnWorker_Text
{
    protected override string GetTextFor(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        if (tenantComp != null && !tenantComp.IsTenant)
        {
            return string.Empty;
        }

        return pawn.Name.ToStringFull;
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        base.DoCell(rect, pawn, table);
        var rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
        if (Widgets.ButtonInvisible(rect2))
        {
            CameraJumper.TryJumpAndSelect(pawn);
            if (Current.ProgramState == ProgramState.Playing && Event.current.button == 0)
            {
                Find.MainTabsRoot.EscapeCurrentTab(false);
            }
        }
        else
        {
            var tooltip = pawn.GetTooltip();
            tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
            TooltipHandler.TipRegion(rect2, tooltip);
        }
    }
}