using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Tenants;

public class MainTabWindow_Tenant : MainTabWindow_PawnTable
{
    private static PawnTableDef pawnTableDef;
    protected override PawnTableDef PawnTableDef => pawnTableDef ??= DefDatabase<PawnTableDef>.GetNamed("Tenants");

    protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.AllPawns
        where p.GetTenantComponent() != null && p.GetTenantComponent().IsTenant && p.GetTenantComponent().Contracted
        select p;

    public override void PostOpen()
    {
        base.PostOpen();
        Find.World.renderer.wantedMode = WorldRenderMode.None;
    }
}