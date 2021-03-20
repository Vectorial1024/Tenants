using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.ColonistBar;

namespace Tenants
{
    [StaticConstructorOnStartup]
    internal static class HarmonyTenants
    {
        static HarmonyTenants()
        {
            var harmonyInstance = new Harmony("rimworld.limetreesnake.tenants");

            #region Ticks

            //Tenant Tick
            //harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "Tick"), null, new HarmonyMethod(typeof(HarmonyTenants).GetMethod("Tick_PostFix")));
            //Tenant TickRare
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "TickRare"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("TickRare_PostFix")));

            #endregion Ticks

            #region Functionality

            //Removes ability to control tenant
            harmonyInstance.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "CanTakeOrder"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CanTakeOrder_PreFix")));
            //What happens when you capture a tenant 
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "CapturedBy"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CapturedBy_PreFix")));
            //Tenant dies
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "Kill"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("Kill_PreFix")));
            //Tenant Inspiration
            harmonyInstance.Patch(AccessTools.Method(typeof(InspirationWorker), "InspirationCanOccur"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("InspirationCanOccur_PreFix")));

            #endregion Functionality

            #region GUI

            //Removes tenant gizmo
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetGizmos_PostFix")));
            //Tenant can work
            harmonyInstance.Patch(AccessTools.Method(typeof(JobGiver_Work), "PawnCanUseWorkGiver"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnCanUseWorkGiver_PreFix")));
            //Remove tenants from caravan list.
            harmonyInstance.Patch(AccessTools.Method(typeof(CaravanFormingUtility), "AllSendablePawns"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("AllSendablePawns_PostFix")));
            //Removes tenants from from pawn table 
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnTable_PlayerPawns), "RecachePawns"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("RecachePawns_PostFix")));
            //Removes tenants from from colonist bar 
            harmonyInstance.Patch(
                typeof(ColonistBarDrawLocsFinder).GetMethods().FirstOrDefault(x =>
                    x.Name == "CalculateDrawLocs" && x.GetParameters().Length == 2),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("CalculateDrawLocs_PreFix")));
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetReport"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetReport_PostFix")));
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetExplanation"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetExplanation_PreFix")));
            //Removes check for idle tenants
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ColonistsIdle), "GetLabel"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetLabel_PreFix")));
            //Pawn name color patch
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnNameColorUtility), "PawnNameColorOf"),
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("PawnNameColorOf_PreFix")));
            //Comms Console Float Menu Option
            harmonyInstance.Patch(AccessTools.Method(typeof(Building_CommsConsole), "GetFloatMenuOptions"), null,
                new HarmonyMethod(typeof(HarmonyTenants).GetMethod("GetFloatMenuOptions_PostFix")));

            #endregion GUI

            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race == null)
                {
                    continue;
                }

                def.comps.Add(new CompProps_Tenant());
                def.comps.Add(new CompProps_Courier());
            }
        }

        #region Ticks

        //public static void Tick_PostFix(Pawn __instance) {
        //    Tenant tenantComp = __instance.GetTenantComponent();
        //    if (tenantComp != null && tenantComp.IsTenant && __instance.IsColonist) {

        //    }
        //}
        public static void TickRare_PostFix(Pawn __instance)
        {
            if (!__instance.IsColonist)
            {
                return;
            }

            var tenantComp = __instance.GetTenantComponent();
            if (tenantComp == null || !tenantComp.IsTenant)
            {
                return;
            }

            //If a tenant has joined but has no contract.
            if (!tenantComp.Contracted)
            {
                tenantComp.IsTenant = false;
            }

            //Tenant alone with no colonist
            var colonist =
                __instance.Map.mapPawns.FreeColonists.FirstOrDefault(x => x.GetTenantComponent().IsTenant == false);
            if (colonist == null)
            {
                Events.ContractConclusion(__instance, true, 1f);
                return;
            }

            //Tenant contract is out
            if (Find.TickManager.TicksGame >= tenantComp.ContractEndTick)
            {
                Events.ContractConclusion(__instance, false);
                return;
            }

            //If tenancy is to be terminated.
            if (tenantComp.IsTerminated)
            {
                if (__instance.health.Downed)
                {
                    Messages.Message("ContractTerminateFail".Translate(), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    Events.TenantCancelContract(__instance);
                    return;
                }

                tenantComp.IsTerminated = false;
            }

            ////Operations queue for tenancy rejected.
            if (__instance.BillStack.Count > 0)
            {
                if (__instance.BillStack.Bills.Any(x => x.recipe.isViolation))
                {
                    __instance.BillStack.Clear();
                    tenantComp.SurgeryQueue++;
                    if (tenantComp.SurgeryQueue < 2)
                    {
                        Messages.Message("TenantSurgeryWarning".Translate(__instance.Named("PAWN")),
                            MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        Messages.Message("TenantSurgeryLeave".Translate(__instance.Named("PAWN")),
                            MessageTypeDefOf.NegativeEvent);
                        Events.TenantLeave(__instance);
                    }
                }
            }

            //Tenancy tick per day
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                if (tenantComp.Wanted)
                {
                    if (!MapComponent_Tenants.GetComponent(__instance.Map).WantedTenants.Contains(__instance))
                    {
                        Events.TenantWanted(__instance);
                    }
                }
            }

            //Tenancy tick 1/10 per day
            if (Find.TickManager.TicksGame % 6000 != 0)
            {
                return;
            }

            {
                if (tenantComp.MoleMessage)
                {
                    tenantComp.MoleMessage = false;
                    Messages.Message("TenantMoleMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                }

                if (tenantComp.Mole && !tenantComp.MoleActivated)
                {
                    if (Utility.CalculateMood(tenantComp) < 1 && tenantComp.NeutralMoodCount > 2)
                    {
                        var building = __instance.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(x =>
                            x.def.defName.Contains("commsconsole") || x.def.defName.Contains("CommsConsole"));
                        if (building != null)
                        {
                            var job = new Job(JobDefOf.JobUseCommsConsoleMole, building);
                            __instance.jobs.TryTakeOrderedJob(job);
                        }
                    }
                }

                if (__instance.needs.mood.CurInstantLevel > 0.8f)
                {
                    Events.TenantWantToJoin(__instance);
                }

                //Calculate mood
                if (__instance.needs.mood.CurInstantLevel > 0.66f)
                {
                    tenantComp.HappyMoodCount++;
                    tenantComp.RecentBadMoodsCount = 0;
                }
                else if (__instance.needs.mood.CurInstantLevel < __instance.mindState.mentalBreaker.BreakThresholdMinor)
                {
                    tenantComp.SadMoodCount++;
                    tenantComp.RecentBadMoodsCount++;
                    if (tenantComp.RecentBadMoodsCount > 5)
                    {
                        Events.ContractConclusion(__instance, true);
                    }
                }
                else
                {
                    tenantComp.NeutralMoodCount++;
                    tenantComp.RecentBadMoodsCount = 0;
                }
            }
        }

        #endregion Ticks

        #region Functionality

        public static bool CanTakeOrder_PreFix(Pawn pawn)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (tenantComp != null && tenantComp.IsTenant)
            {
                return false;
            }

            return true;
        }

        public static void CapturedBy_PreFix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            var tenantComp = pawn.GetTenantComponent();
            if (tenantComp == null || !tenantComp.IsTenant)
            {
                return;
            }

            if (tenantComp.MoleActivated)
            {
                Events.TenantMoleCaptured(pawn);
            }
            else
            {
                Events.TenantCaptured(pawn, byPawn);
            }
        }

        public static void Kill_PreFix(Pawn __instance, DamageInfo? dinfo)
        {
            var tenantComp = __instance.GetTenantComponent();
            if (tenantComp == null)
            {
                return;
            }

            if ((tenantComp.Contracted || tenantComp.CapturedTenant && !__instance.guest.Released) &&
                __instance.Spawned)
            {
                Events.TenantDeath(__instance);
            }
        }

        public static bool InspirationCanOccur_PreFix(Pawn pawn)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (tenantComp == null)
            {
                return true;
            }

            if (tenantComp.IsTenant)
            {
                return false;
            }

            return true;
        }

        #endregion Functionality

        #region GUI

        public static void GetGizmos_PostFix(ref IEnumerable<Gizmo> __result, ref Pawn __instance)
        {
            var log = 0;
            try
            {
                log++;
                if (__instance == null)
                {
                    return;
                }

                log++;
                var tenantComp = __instance.GetTenantComponent();
                if (tenantComp == null || !tenantComp.IsTenant || __result == null)
                {
                    return;
                }

                log++;
                if (__result == null || !__result.Any())
                {
                    return;
                }

                log++;
                var gizmos = __result.ToList();

                log++;
                foreach (var giz in gizmos.ToList())
                {
                    log++;
                    if (giz.GetType() == typeof(Command_Toggle) && (giz as Command)?.defaultLabel == "Draft")
                    {
                        gizmos.Remove(giz);
                    }
                }

                log++;
                __result = gizmos.AsEnumerable();
                log++;
            }
            catch (Exception ex)
            {
                Log.Message(log + ex.Message);
            }
        }

        public static bool PawnCanUseWorkGiver_PreFix(Pawn pawn, WorkGiver giver)
        {
            var tenantComp = pawn.GetTenantComponent();
            if (tenantComp == null || !tenantComp.IsTenant || !pawn.IsColonist)
            {
                return true;
            }

            if (pawn.needs.mood.CurLevel > SettingsHelper.LatestVersion.LevelOfHappinessToWork / 100f ||
                Utility.UpdateEmergencyWork(giver))
            {
                return true;
            }

            return false;
        }

        public static void AllSendablePawns_PostFix(ref List<Pawn> __result)
        {
            Utility.RemoveTenantsFromList(__result);
        }

        public static void RecachePawns_PostFix(PawnTable __instance)
        {
            if (__instance is PawnTable_Tenants)
            {
                return;
            }

            var pawns = Traverse.Create(__instance).Field("cachedPawns").GetValue<List<Pawn>>();
            if (pawns != null && pawns.Count > 0)
            {
                Utility.RemoveTenantsFromList(pawns);
            }
        }

        public static void CalculateDrawLocs_PreFix(List<Vector2> outDrawLocs, out float scale)
        {
            scale = 1f;
            var entries = Traverse.Create(Find.ColonistBar).Field("cachedEntries").GetValue<List<Entry>>();
            if (entries == null || entries.Count <= 0)
            {
                return;
            }

            var newentries = new List<Entry>();
            foreach (var entry in entries)
            {
                var tenantComp = entry.pawn?.GetTenantComponent();
                if (tenantComp != null && tenantComp.IsTenant)
                {
                    newentries.Add(entry);
                }
            }

            foreach (var entry in newentries)
            {
                entries.Remove(entry);
            }
        }

        public static void GetReport_PostFix(ref AlertReport __result, Alert_ColonistsIdle __instance)
        {
            if (__result.culpritsPawns == null)
            {
                return;
            }

            __result.culpritsPawns = Utility.RemoveTenantsFromList(__result.culpritsPawns);
            __result.active = __result.AnyCulpritValid;
        }

        public static bool GetExplanation_PreFix(ref TaggedString __result, Alert_ColonistsIdle __instance)
        {
            var stringBuilder = new StringBuilder();
            var IdleColonists = Traverse.Create(__instance).Property("IdleColonists").GetValue<IEnumerable<Pawn>>();

            foreach (var idleColonist in IdleColonists)
            {
                var tenantComp = idleColonist.GetTenantComponent();
                if (tenantComp != null && !tenantComp.IsTenant)
                {
                    stringBuilder.AppendLine("    " + idleColonist.LabelShort.CapitalizeFirst());
                }
            }

            __result = "ColonistsIdleDesc".Translate(stringBuilder.ToString());

            return false;
        }

        public static bool GetLabel_PreFix(ref string __result, Alert_ColonistsIdle __instance)
        {
            var IdleColonists = Traverse.Create(__instance).Property("IdleColonists").GetValue<IEnumerable<Pawn>>();
            var x = 0;
            foreach (var idleColonist in IdleColonists)
            {
                var tenantComp = idleColonist.GetTenantComponent();
                if (tenantComp != null && !tenantComp.IsTenant)
                {
                    x++;
                }
            }

            __result = "ColonistsIdle".Translate(x.ToStringCached());
            return false;
        }

        public static bool PawnNameColorOf_PreFix(ref Color __result, Pawn pawn)
        {
            if (!pawn.IsColonist)
            {
                return true;
            }

            var tenantComp = pawn.GetTenantComponent();
            if (tenantComp == null || !tenantComp.IsTenant)
            {
                return true;
            }

            __result = SettingsHelper.LatestVersion.Color;
            return false;
        }

        public static void GetFloatMenuOptions_PostFix(Building_CommsConsole __instance,
            ref IEnumerable<FloatMenuOption> __result, Pawn myPawn)
        {
            var list = __result.ToList();
            if (!MapComponent_Tenants.GetComponent(myPawn.Map).Broadcast)
            {
                void inviteTenant()
                {
                    var job = new Job(JobDefOf.JobUseCommsConsoleTenants, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }

                var inviteTenants = new FloatMenuOption("InviteTenant".Translate(), inviteTenant,
                    MenuOptionPriority.InitiateSocial);
                list.Add(inviteTenants);
            }

            if (!MapComponent_Tenants.GetComponent(myPawn.Map).BroadcastCourier)
            {
                void inviteCourier()
                {
                    var job = new Job(JobDefOf.JobUseCommsConsoleInviteCourier, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }

                var inviteCouriers =
                    new FloatMenuOption("CourierInvite".Translate(SettingsHelper.LatestVersion.CourierCost),
                        inviteCourier, MenuOptionPriority.InitiateSocial);
                list.Add(inviteCouriers);
            }

            __result = list.AsEnumerable();
        }

        #endregion GUI
    }
}