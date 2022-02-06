using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tenants;

public class PawnTable_Tenants : PawnTable
{
    public PawnTable_Tenants(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
        : base(def, pawnsGetter, uiWidth, uiHeight)
    {
    }
}