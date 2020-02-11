﻿using RimWorld;
using Tenants.Comps;
using UnityEngine;
using Verse;

namespace Tenants.UI {
    public class PawnColumnWorker_MayJoin : PawnColumnWorker_Checkbox {

        public PawnColumnWorker_MayJoin() {
            foreach (PawnColumnDef def in DefDatabase<PawnColumnDef>.AllDefs) {
                if (def.defName == "MayJoin") {
                    def.label = "MayJoin".Translate();
                }
            }
        }
        protected override string GetTip(Pawn pawn) {
            return "MayJoinTip".Translate();
        }
        protected override bool GetValue(Pawn pawn) {

            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (comp != null) 
                return ThingCompUtility.TryGetComp<TenantComp>(pawn).MayJoin;
            else
                return false;
        }
        protected override void SetValue(Pawn pawn, bool value) {
            TenantComp comp = ThingCompUtility.TryGetComp<TenantComp>(pawn);
            if (comp != null) comp.MayJoin = value;
        }
    }
}
