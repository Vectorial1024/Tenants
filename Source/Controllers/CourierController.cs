﻿using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenants.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Tenants.Controllers {
    public static class CourierController {

        public static bool Courier(Map map, Building box) {
            try {
                if (!Utilities.MapUtilities.TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                    return false;
                }
                if (TenantsMapComp.GetComponent(map).BroadcastCourier == true) {
                    TenantsMapComp.GetComponent(map).BroadcastCourier = false;
                }
                if (TenantsMapComp.GetComponent(map).KilledCourier > 0) {
                    TenantsMapComp.GetComponent(map).KilledCourier--;
                    string courierDeniedLabel = "CourierDeniedTitle".Translate(map.Parent.Label);
                    string courierDeniedText = "CourierDeniedMessage".Translate();
                    Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
                    return true;
                }
                Pawn pawn = FindRandomCourier();
                if (pawn == null)
                    return false;
                GenSpawn.Spawn(pawn, spawnSpot, map);
                CourierDress(pawn, map);
                CourierInventory(pawn, map);
                pawn.relations.everSeenByPlayer = true;
                pawn.SetFaction(Faction.OfAncients);
                string letterLabel = "CourierArrivedTitle".Translate(map.Parent.Label);
                string letterText = "CourierArrivedMessage".Translate(pawn.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
                LordMaker.MakeNewLord(pawn.Faction, new LordJobs.LordJob_CourierDeliver(map.listerThings.ThingsOfDef(Defs.ThingDefOf.Tenant_MailBox).RandomElement()), pawn.Map, new List<Pawn> { pawn });
                return true;
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
                return false;
            }
        }
        public static void CourierInvite(Building_CommsConsole comms, Pawn pawn) {
            if (TenantsMapComp.GetComponent(pawn.Map).KilledCourier > 0) {
                string courierDeniedLabel = "CourierDeniedTitle".Translate(pawn.Map.Parent.Label);
                string courierDeniedText = "CourierDeniedRadioMessage".Translate();
                Find.LetterStack.ReceiveLetter(courierDeniedLabel, courierDeniedText, LetterDefOf.NegativeEvent);
            }
            else {
                Messages.Message("CourierInvited".Translate(Settings.Settings.CourierCost), MessageTypeDefOf.NeutralEvent);
                TenantsMapComp.GetComponent(pawn.Map).BroadcastCourier = true;
                IncidentParms parms = new IncidentParms() { target = pawn.Map, forced = true };
                Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.TenantCourier, Find.TickManager.TicksGame + Rand.Range(15000, 90000), parms, 240000);
                Thing silver = ThingMaker.MakeThing(RimWorld.ThingDefOf.Silver);
                silver.stackCount = (int)Settings.Settings.CourierCost;
                TenantsMapComp.GetComponent(pawn.Map).CourierCost.Add(silver);

            }
        }
        public static Pawn FindRandomCourier() {
            IEnumerable<Pawn> pawns = Find.WorldPawns.AllPawnsAlive.Where(x => ThingCompUtility.TryGetComp<CourierComp>(x) != null && !x.Dead && !x.Spawned && !x.Discarded);
            if (pawns.Count() < 20)
                for (int i = 0; i < 3; i++)
                    pawns.Add(GenerateNewCourier());
            if (pawns.Count() == 0)
                return null;
            return pawns.RandomElement();
        }
        public static Pawn GenerateNewCourier() {
            bool generation = true;
            Pawn newCourier = null;
            while (generation) {
                newCourier = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                if (newCourier != null && !newCourier.Dead && !newCourier.IsDessicated() && !newCourier.AnimalOrWildMan() && !newCourier.story.WorkTagIsDisabled(WorkTags.Violent)) {
                    {
                        CourierComp courierComp = new CourierComp();
                        newCourier.AllComps.Add(courierComp);
                        newCourier.DestroyOrPassToWorld();
                        generation = false;
                    }
                }
            }
            return newCourier;
        }
        public static void CourierDress(Pawn pawn, Map map) {
            pawn.apparel.DestroyAll();
            ThingDef hatDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_cowboyhat");
            Thing hat = ThingMaker.MakeThing(hatDef, GenStuff.RandomStuffByCommonalityFor(hatDef));
            pawn.apparel.Wear((Apparel)hat);
            ThingDef pantsDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_flakpants");
            Thing pants = ThingMaker.MakeThing(pantsDef, GenStuff.RandomStuffByCommonalityFor(pantsDef));
            pawn.apparel.Wear((Apparel)pants);
            ThingDef shirtDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_basicshirt");
            Thing shirt = ThingMaker.MakeThing(shirtDef, GenStuff.RandomStuffByCommonalityFor(shirtDef));
            pawn.apparel.Wear((Apparel)shirt);
            if (map.mapTemperature.OutdoorTemp < 0) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_parka");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else if (map.mapTemperature.OutdoorTemp < 15) {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_jacket");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }
            else {
                ThingDef topDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.ToLower() == "apparel_duster");
                Thing top = ThingMaker.MakeThing(topDef, GenStuff.RandomStuffByCommonalityFor(topDef));
                pawn.apparel.Wear((Apparel)top);
            }

            ThingDef scrollCaseDef = Defs.ThingDefOf.Tenant_ScrollCase;
            Thing scrollCase = ThingMaker.MakeThing(scrollCaseDef, GenStuff.RandomStuffByCommonalityFor(scrollCaseDef));
            pawn.apparel.Wear((Apparel)scrollCase);
        }
        public static void CourierInventory(Pawn pawn, Map map) {
            ThingDef bowDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(x => x.defName.ToLower() == "bow_recurve".ToLower());
            Thing bow = ThingMaker.MakeThing(bowDef, GenStuff.RandomStuffByCommonalityFor(bowDef));
            pawn.equipment.AddEquipment((ThingWithComps)bow);
        }
        public static void EmptyMailBox(ref List<Thing> content, IntVec3 pos) {
            if (content.Count > 0) {
                foreach (Thing thing in content) {
                    DebugThingPlaceHelper.DebugSpawn(thing.def, pos, thing.stackCount);
                }
                content.Clear();
            }
        }
        public static void RecieveLetters(ref List<Thing> content, IntVec3 pos, Map map) {
            if (content.Count > 0) {
                foreach (Thing letter in content) {
                    Comps.ScrollComp letterComp = ThingCompUtility.TryGetComp<Comps.ScrollComp>(letter);
                    switch ((ScrollType)letterComp.TypeValue) {
                        case ScrollType.Diplomatic: {
                                if (Rand.Value < 0.2f + ((letterComp.Skill * 3.5f) / 100f)) {
                                    if (Rand.Value < letterComp.Skill / 100f) {
                                        int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                        val += Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                        StringBuilder builder = new StringBuilder();
                                        builder.Append("LetterDiplomaticPositive".Translate(letterComp.Faction) + "\n" + "LetterRelationIncrease".Translate(val));

                                        List<Thing> gifts = Defs.ThingSetMakerDefOf.Gift_Diplomatic.root.Generate();
                                        foreach (Thing gift in gifts) {
                                            builder.AppendLine(gift.stackCount + " " + gift.Label);
                                            DebugThingPlaceHelper.DebugSpawn(gift.def, pos, gift.stackCount);
                                        }
                                        Find.LetterStack.ReceiveLetter("LetterDiplomaticTitle".Translate(), builder.ToString(), LetterDefOf.PositiveEvent);
                                    }
                                    else {
                                        int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                        Find.LetterStack.ReceiveLetter("LetterDiplomaticTitle".Translate(), "LetterDiplomaticResponse".Translate(letterComp.Faction) + "\n" + "LetterRelationIncrease".Translate(val), LetterDefOf.PositiveEvent);
                                    }
                                }
                                else {
                                    int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction, true);
                                    Find.LetterStack.ReceiveLetter("LetterDiplomaticTitle".Translate(), "LetterDiplomaticNegative".Translate(letterComp.Faction) + "\n" + "LetterRelationPenalty".Translate(val), LetterDefOf.NegativeEvent);
                                }
                                break;
                            }
                        case ScrollType.Angry: {
                                if (letter.Faction.RelationKindWith(Find.FactionManager.OfPlayer) == FactionRelationKind.Ally) {
                                    int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                    val += Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                    Find.LetterStack.ReceiveLetter("LetterAngryTitle".Translate(), "LetterAngrySad".Translate(letterComp.Faction) + "\n" + "LetterRelationPenalty".Translate(val), LetterDefOf.NegativeEvent);
                                }
                                if (Rand.Value < 0.6f + ((letterComp.Skill * 1.5f) / 100f)) {
                                    if (Rand.Value < letterComp.Skill / 100f) {
                                        int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction, true);
                                        val += Utilities.FactionUtilities.ChangeRelations(letterComp.Faction, true);
                                        Find.LetterStack.ReceiveLetter("LetterAngryTitle".Translate(), "LetterAngryNegative".Translate(letterComp.Faction) + "\n" + "LetterRelationPenalty".Translate(val), LetterDefOf.NegativeEvent);
                                        IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                                        parms.raidStrategy = RimWorld.RaidStrategyDefOf.ImmediateAttack;
                                        parms.forced = true;
                                        Find.Storyteller.incidentQueue.Add(RimWorld.IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + Rand.Range(5000, 30000), parms, 90000);
                                    }
                                    else {
                                        int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction, true);
                                        Find.LetterStack.ReceiveLetter("LetterAngryTitle".Translate(), "LetterAngryResponse".Translate(letterComp.Faction) + "\n" + "LetterRelationPenalty".Translate(val), LetterDefOf.NegativeEvent);
                                    }
                                }
                                else {
                                    int val = Utilities.FactionUtilities.ChangeRelations(letterComp.Faction);
                                    Find.LetterStack.ReceiveLetter("LetterAngryTitle".Translate(), "LetterAngryPositive".Translate(letterComp.Faction) + "\n" + "LetterRelationIncrease".Translate(val), LetterDefOf.PositiveEvent);
                                }
                                break;
                            }
                        case ScrollType.Invite: {
                                if (Rand.Value < 0.4f + ((letterComp.Skill * 2.5f) / 100f)) {
                                    Find.LetterStack.ReceiveLetter("LetterInviteTitle".Translate(), "LetterInvitePositive".Translate(letterComp.Faction), LetterDefOf.PositiveEvent);
                                    IncidentParms parms = new IncidentParms() { target = map, forced = true, faction = letterComp.Faction };
                                    Find.Storyteller.incidentQueue.Add(Defs.IncidentDefOf.TenantProposition, Find.TickManager.TicksGame + Rand.Range(15000, 30000), parms, 240000);
                                }
                                else {
                                    Find.LetterStack.ReceiveLetter("LetterInviteTitle".Translate(), "LetterInviteResponse".Translate(letterComp.Faction), LetterDefOf.NeutralEvent);
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
                content.Clear();
            }
            //DO STUFF
        }
    }
}
