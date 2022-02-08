using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Tenants;

public static class Events
{
    public static void ContractConclusion(Pawn pawn, bool terminated, float stealChance = 0.5f)
    {
        var tenantComp = pawn.GetTenantComponent();
        string letterLabel, letterText;
        LetterDef def;

        if (!tenantComp.IsTenant && !pawn.IsColonist)
        {
            return;
        }

        if (terminated)
        {
            if (Rand.Value < stealChance)
            {
                letterLabel = "ContractEnd".Translate();
                letterText =
                    "ContractDoneTerminated".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000,
                        pawn.Named("PAWN"));
                def = LetterDefOf.NeutralEvent;
                TenantLeave(pawn);
            }
            else
            {
                letterLabel = "ContractBreach".Translate();
                letterText = "ContractDoneTheft".Translate(pawn.Named("PAWN"));
                def = LetterDefOf.NegativeEvent;
                TenantTheft(pawn);
            }
        }
        else
        {
            var mood = Utility.CalculateMood(tenantComp);
            if (mood == 1)
            {
                letterLabel = "ContractEnd".Translate();
                letterText = "ContractDoneHappy".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000,
                    pawn.Named("PAWN"));
                def = LetterDefOf.PositiveEvent;
                ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceHappy / 100f);
            }
            else if (mood == -1)
            {
                letterLabel = "ContractEnd".Translate();
                letterText = "ContractDoneSad".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000,
                    pawn.Named("PAWN"));
                def = LetterDefOf.NeutralEvent;
                ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceSad / 100f);
            }
            else
            {
                letterLabel = "ContractEnd".Translate();
                letterText = "ContractDone".Translate(tenantComp.Payment * tenantComp.ContractLength / 60000,
                    pawn.Named("PAWN"));
                def = LetterDefOf.NeutralEvent;
                ContractProlong(pawn, SettingsHelper.LatestVersion.StayChanceNeutral / 100f);
            }
        }

        Find.LetterStack.ReceiveLetter(letterLabel, letterText, def);
    }

    private static void ContractProlong(Pawn pawn, float chance)
    {
        if (Rand.Value < chance)
        {
            var tenantComp = pawn.TryGetComp<Tenant>();
            if (tenantComp.AutoRenew)
            {
                tenantComp.ContractDate = Find.TickManager.TicksGame;
                tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                tenantComp.ResetMood();
                Utility.MakePayment(pawn);

                string letterLabel = "ContractNew".Translate();
                string letterText = "ContractRenewedMessage".Translate(pawn.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
                return;
            }

            var text = Utility.ProlongContractMessage(pawn);
            var diaNode = new DiaNode(text);
            //Accepted offer.
            var diaOption = new DiaOption("ContractAgree".Translate())
            {
                action = delegate
                {
                    Utility.MakePayment(pawn);
                    tenantComp.ContractDate = Find.TickManager.TicksGame;
                    tenantComp.ContractEndDate = Find.TickManager.TicksAbs + tenantComp.ContractLength + 60000;
                    tenantComp.ResetMood();
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            //Denied offer
            string text2 =
                "RequestForTenancyContinuedRejected".Translate(pawn.LabelShort, pawn, pawn.Named("PAWN"));
            var diaNode2 = new DiaNode(text2);
            var diaOption2 = new DiaOption("OK".Translate())
            {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            var diaOption3 = new DiaOption("ContractReject".Translate())
            {
                action = delegate { TenantLeave(pawn); },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            string title = "RequestForTenancyTitle".Translate(pawn.Map.Parent.Label);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
        }
        else
        {
            TenantLeave(pawn);
        }
    }

    public static bool ContractTenancy(Map map)
    {
        if (!Utility.TryFindSpawnSpot(map, out var spawnSpot))
        {
            return false;
        }

        var pawn = Utility.FindRandomTenant();
        if (pawn == null)
        {
            return false;
        }

        pawn.relations.everSeenByPlayer = true;
        var tenantComp = pawn.TryGetComp<Tenant>();
        Utility.GenerateBasicTenancyContract(tenantComp);
        var stringBuilder = new StringBuilder("");
        //Check if pawn is special
        //Wanted
        if (SettingsHelper.LatestVersion.WantedTenants && Rand.Value < 0.2f)
        {
            Utility.GenerateWanted(pawn);
        }

        //Mole
        if (SettingsHelper.LatestVersion.MoleTenants && Rand.Value < 0.33f &&
            tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer))
        {
            tenantComp.Mole = true;
        }

        string title;
        string text;
        if (pawn.GetTenantComponent().Wanted)
        {
            stringBuilder.Append("RequestForTenancyHiding".Translate(tenantComp.WantedBy, pawn.Named("PAWN")));
            title = "RequestForTenancyHidingTitle".Translate(map.Parent.Label);
            tenantComp.Payment *= 2;
            text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);
            return Utility.GenerateContractDialogue(title, text, pawn, map, spawnSpot);
        }

        //Broadcasted
        if (MapComponent_Tenants.GetComponent(map).Broadcast)
        {
            stringBuilder.Append("RequestForTenancyOpportunity".Translate(pawn.Named("PAWN")));
            title = "RequestForTenancyTitle".Translate(map.Parent.Label);
            MapComponent_Tenants.GetComponent(map).Broadcast = false;
            text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);
            return Utility.GenerateContractDialogue(title, text, pawn, map, spawnSpot);
        }

        //Normal
        stringBuilder.Append("RequestForTenancyInitial".Translate(pawn.Named("PAWN")));
        title = "RequestForTenancyTitle".Translate(map.Parent.Label);
        text = Utility.AppendContractDetails(stringBuilder.ToString(), pawn);
        return Utility.GenerateContractDialogue(title, text, pawn, map, spawnSpot);
    }

    public static bool Courier(Map map, Building box)
    {
        try
        {
            if (!Utility.TryFindSpawnSpot(map, out var spawnSpot))
            {
                return false;
            }

            if (MapComponent_Tenants.GetComponent(map).BroadcastCourier)
            {
                MapComponent_Tenants.GetComponent(map).BroadcastCourier = false;
            }

            if (MapComponent_Tenants.GetComponent(map).KilledCourier > 0)
            {
                MapComponent_Tenants.GetComponent(map).KilledCourier--;
                string courierDeniedLabel = "CourierDeniedTitle".Translate(map.Parent.Label);
                string courierDeniedText = "CourierDeniedMessage".Translate();
                Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
                return true;
            }

            var pawn = Utility.FindRandomCourier();
            if (pawn == null)
            {
                return false;
            }

            GenSpawn.Spawn(pawn, spawnSpot, map);
            //pawn.SetFaction(Faction.OfAncients);
            pawn.relations.everSeenByPlayer = true;
            Utility.CourierDress(pawn, map);
            Utility.CourierInventory(pawn, map);
            string letterLabel = "CourierArrivedTitle".Translate(map.Parent.Label);
            string letterText = "CourierArrivedMessage".Translate(pawn.Named("PAWN"));
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
            LordMaker.MakeNewLord(pawn.Faction,
                new LordJob_CourierDeliver(box), pawn.Map, new List<Pawn> { pawn });
            return true;
        }
        catch (Exception ex)
        {
            Log.Message(ex.Message);
            return false;
        }
    }

    public static void TenantLeave(Pawn pawn)
    {
        Utility.MakePayment(pawn);
        pawn.jobs.ClearQueuedJobs();
        pawn.SetFaction(null);
        pawn.GetTenantComponent().CleanTenancy();
        LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
        if (MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn))
        {
            MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Remove(pawn);
        }
    }

    public static void TenantCancelContract(Pawn pawn)
    {
        Messages.Message("ContractDonePlayerTerminated".Translate(pawn.Named("PAWN")),
            MessageTypeDefOf.NeutralEvent);
        pawn.jobs.ClearQueuedJobs();
        pawn.SetFaction(null);
        pawn.GetTenantComponent().CleanTenancy();
        LordMaker.MakeNewLord(pawn.Faction, new LordJob_ExitMapBest(), pawn.Map, new List<Pawn> { pawn });
    }

    private static void TenantTheft(Pawn pawn)
    {
        pawn.jobs.ClearQueuedJobs();
        pawn.SetFaction(null);
        pawn.GetTenantComponent().IsTenant = false;
        LordMaker.MakeNewLord(pawn.Faction, new LordJob_TenantTheft(), pawn.Map, new List<Pawn> { pawn });
        
        // make the thief guilty
        pawn.guilt?.Notify_Guilty();
    }

    public static void TenantDeath(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        tenantComp.IsTenant = false;
        string text = "TenantDeath".Translate(pawn.Named("PAWN"));
        text = text.AdjustedFor(pawn);
        string label = "Death".Translate() + ": " + pawn.LabelShortCap;
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, pawn);
        pawn.SetFaction(tenantComp.HiddenFaction);
        if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer))
        {
            if (!(Rand.Value < 0.5f))
            {
                return;
            }

            if (tenantComp.HiddenFaction.def == FactionDefOf.Ancients)
            {
                return;
            }

            var relation = tenantComp.HiddenFaction.RelationWith(Find.FactionManager.OfPlayer);
            relation.baseGoodwill -= SettingsHelper.LatestVersion.OutragePenalty * 2;
            Messages.Message(
                "TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty,
                    pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
            MapComponent_Tenants.GetComponent(pawn.Map).DeadTenantsToAvenge.Add(pawn);
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            parms.raidStrategy = RaidStrategyDefOf.Retribution;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForDead,
                Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
        }
        else
        {
            if (!(Rand.Value < 0.66f))
            {
                return;
            }

            if (tenantComp.HiddenFaction.def == FactionDefOf.Ancients)
            {
                return;
            }

            var relation = tenantComp.HiddenFaction.RelationWith(Find.FactionManager.OfPlayer);
            relation.baseGoodwill -= SettingsHelper.LatestVersion.OutragePenalty * 2;
            Messages.Message(
                "TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty * 2,
                    pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
        }
    }

    public static void TenantCaptured(Pawn pawn, Pawn byPawn)
    {
        if (pawn.HostileTo(Find.FactionManager.OfPlayer))
        {
            return;
        }

        string text = "TenantCaptured".Translate(pawn.Named("PAWN"));
        text = text.AdjustedFor(pawn);
        string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);
        var tenantComp = pawn.GetTenantComponent();
        tenantComp.IsTenant = false;
        pawn.GetTenantComponent().CapturedTenant = true;
        pawn.SetFaction(tenantComp.HiddenFaction);

        if (pawn.Faction.HostileTo(Find.FactionManager.OfPlayer))
        {
            if (!(Rand.Value < 0.25f) && !tenantComp.Wanted)
            {
                return;
            }

            MapComponent_Tenants.GetComponent(byPawn.Map).CapturedTenantsToAvenge.Add(pawn);
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, byPawn.Map);
            parms.raidStrategy = RaidStrategyDefOf.Retribution;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RetributionForCaptured,
                Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
        }
        else
        {
            if (!(Rand.Value < 0.66f) && !tenantComp.Wanted)
            {
                return;
            }

            if (tenantComp.HiddenFaction.def == FactionDefOf.Ancients)
            {
                return;
            }

            var relation = pawn.Faction.RelationWith(Find.FactionManager.OfPlayer);
            relation.baseGoodwill -= SettingsHelper.LatestVersion.OutragePenalty;
            Messages.Message(
                "TenantFactionOutrage".Translate(pawn.Faction, SettingsHelper.LatestVersion.OutragePenalty,
                    pawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
        }
    }

    public static void TenantMoleCaptured(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        tenantComp.IsTenant = false;
        pawn.SetFaction(tenantComp.HiddenFaction);
        string text = "MoleCaptured".Translate(pawn.Named("PAWN"));
        text = text.AdjustedFor(pawn);
        string label = "Captured".Translate() + ": " + pawn.LabelShortCap;
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);

        // make the mole guilty
        pawn.guilt?.Notify_Guilty();
    }

    public static void TenantWantToJoin(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        if (tenantComp.MayJoin && Rand.Value < 0.02f && tenantComp.HappyMoodCount > 7)
        {
            string text = "RequestTenantWantToJoin".Translate(pawn.Named("PAWN"));

            var diaNode = new DiaNode(text);
            var diaOption = new DiaOption("ContractAgree".Translate())
            {
                action = delegate
                {
                    Utility.MakePayment(pawn);
                    tenantComp.IsTenant = false;
                    Messages.Message(
                        "ContractDone".Translate(pawn.Name.ToStringFull,
                            tenantComp.Payment * tenantComp.ContractLength / 60000, pawn.Named("PAWN")),
                        MessageTypeDefOf.PositiveEvent);
                    Find.ColonistBar.MarkColonistsDirty();
                },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);
            //Denied offer
            string text2 = "RequestTenantWantToJoinRejected".Translate(pawn.Named("PAWN"));
            var diaNode2 = new DiaNode(text2);
            var diaOption2 = new DiaOption("OK".Translate())
            {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            var diaOption3 = new DiaOption("ContractReject".Translate())
            {
                action = delegate { },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            string title = "RequestTenantWantToJoinTitle".Translate(pawn.Map.Parent.Label);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title));
        }

        if (tenantComp.HiddenFaction != null && tenantComp.HiddenFaction.HostileTo(Find.FactionManager.OfPlayer))
        {
            tenantComp.HiddenFaction = null;
        }
    }

    public static void TenantMole(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        tenantComp.MoleActivated = true;
        tenantComp.MoleMessage = true;
        MapComponent_Tenants.GetComponent(pawn.Map).Moles.Add(pawn);
        var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
        parms.raidStrategy = RaidStrategyDefOf.MoleRaid;
        parms.forced = true;
        Find.Storyteller.incidentQueue.Add(IncidentDefOf.MoleRaid,
            Find.TickManager.TicksGame + Rand.Range(5000, 30000), parms, 90000);
    }

    public static void TenantWanted(Pawn pawn)
    {
        var tenantComp = pawn.GetTenantComponent();
        if (Rand.Value < 0.66 && tenantComp.WantedBy.HostileTo(Find.FactionManager.OfPlayer) &&
            !MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Contains(pawn))
        {
            MapComponent_Tenants.GetComponent(pawn.Map).WantedTenants.Add(pawn);
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            parms.raidStrategy = RaidStrategyDefOf.WantedRaid;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.WantedRaid,
                Find.TickManager.TicksGame + Rand.Range(100000, 300000), parms, 60000);
        }
        else if (Rand.Value < 0.5)
        {
            tenantComp.WantedBy.RelationWith(Find.FactionManager.OfPlayer).baseGoodwill -=
                SettingsHelper.LatestVersion.HarborPenalty;
            Find.FactionManager.OfPlayer.RelationWith(tenantComp.WantedBy).baseGoodwill -=
                SettingsHelper.LatestVersion.HarborPenalty;

            Messages.Message(
                "HarboringWantedTenant".Translate(pawn.GetTenantComponent().WantedBy,
                    SettingsHelper.LatestVersion.HarborPenalty, pawn.Named("PAWN")),
                MessageTypeDefOf.NegativeEvent);
        }
    }

    public static void TenantInvite(Building_CommsConsole comms, Pawn pawn)
    {
        Messages.Message("InviteTenantMessage".Translate(), MessageTypeDefOf.NeutralEvent);
        MapComponent_Tenants.GetComponent(pawn.Map).Broadcast = true;
        if (Rand.Value < 0.20f)
        {
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            parms.raidStrategy = RaidStrategyDefOf.Retribution;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.Opportunists,
                Find.TickManager.TicksGame + Rand.Range(25000, 150000), parms, 240000);
        }
        else
        {
            var parms = new IncidentParms { target = pawn.Map, forced = true };
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RequestForTenancy,
                Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            while (Rand.Value < 0.10)
            {
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RequestForTenancy,
                    Find.TickManager.TicksGame + Rand.Range(15000, 120000), parms, 240000);
            }
        }
    }

    public static void CourierInvite(Building_CommsConsole comms, Pawn pawn)
    {
        if (MapComponent_Tenants.GetComponent(pawn.Map).KilledCourier > 0)
        {
            string courierDeniedLabel = "CourierDeniedTitle".Translate(pawn.Map.Parent.Label);
            string courierDeniedText = "CourierDeniedRadioMessage".Translate();
            Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
        }
        else
        {
            Messages.Message("CourierInvited".Translate(SettingsHelper.LatestVersion.CourierCost),
                MessageTypeDefOf.NeutralEvent);
            MapComponent_Tenants.GetComponent(pawn.Map).BroadcastCourier = true;
            var parms = new IncidentParms { target = pawn.Map, forced = true };
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.TenantCourier,
                Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
            var silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
            silver.stackCount = (int)SettingsHelper.LatestVersion.CourierCost;
            MapComponent_Tenants.GetComponent(pawn.Map).CourierCost.Add(silver);
        }
    }
}
