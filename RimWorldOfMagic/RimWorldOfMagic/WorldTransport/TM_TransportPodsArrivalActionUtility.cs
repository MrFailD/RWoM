﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TorannMagic.WorldTransport
{
    public class TM_TransportPodsArrivalActionUtility
    {
        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter,
            Func<T> arrivalActionGetter, string label, CompLaunchable representative, int destinationTile, Action<Action> uiConfirmationCallback = null) where T : TransportersArrivalAction
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
            if (floatMenuAcceptanceReport.Accepted || !floatMenuAcceptanceReport.FailReason.NullOrEmpty() || !floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
            {
                if (!floatMenuAcceptanceReport.FailReason.NullOrEmpty())
                {
                    yield return new FloatMenuOption(label + " (" + floatMenuAcceptanceReport.FailReason + ")", null);
                }
                else
                {
                    yield return new FloatMenuOption(label, delegate
                    {
                        FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
                        if (floatMenuAcceptanceReport2.Accepted)
                        {
                            if (uiConfirmationCallback == null)
                            {
                                representative.TryLaunch(destinationTile, arrivalActionGetter());
                            }
                            else
                            {
                                uiConfirmationCallback(delegate
                                {
                                    representative.TryLaunch(destinationTile, arrivalActionGetter());
                                });
                            }
                        }
                        else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
                        {
                            Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(destinationTile), MessageTypeDefOf.RejectInput, historical: false);
                        }
                    });
                }
            }
        }

        public static bool AnyNonDownedColonist(IEnumerable<IThingHolder> pods)
        {
            foreach (IThingHolder pod in pods)
            {
                ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    Pawn pawn = directlyHeldThings[i] as Pawn;
                    if (pawn != null && pawn.IsColonist && !pawn.Downed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool AnyPotentialCaravanOwner(IEnumerable<IThingHolder> pods, Faction faction)
        {
            foreach (IThingHolder pod in pods)
            {
                ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    Pawn pawn = directlyHeldThings[i] as Pawn;
                    if (pawn != null && CaravanUtility.IsOwner(pawn, faction))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Thing GetLookTarget(List<ActiveTransporterInfo> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner directlyHeldThings = pods[i].GetDirectlyHeldThings();
                for (int j = 0; j < directlyHeldThings.Count; j++)
                {
                    Pawn pawn = directlyHeldThings[j] as Pawn;
                    if (pawn != null && pawn.IsColonist)
                    {
                        return pawn;
                    }
                }
            }
            for (int k = 0; k < pods.Count; k++)
            {
                Thing thing = pods[k].GetDirectlyHeldThings().FirstOrDefault();
                if (thing != null)
                {
                    return thing;
                }
            }
            return null;
        }

        public static void DropTravelingTransportPods(List<ActiveTransporterInfo> dropPods, IntVec3 near, Map map, bool exactCell = false, bool draftFlag = false)
        {
            RemovePawnsFromWorldPawns(dropPods);
            for (int i = 0; i < dropPods.Count; i++)
            {
                IntVec3 result = default(IntVec3);
                if (!exactCell || !(near.InBoundsWithNullCheck(map) && near.Walkable(map) && !near.Roofed(map)))
                {                    
                    DropCellFinder.TryFindDropSpotNear(near, map, out result, allowFogged: false, canRoofPunch: true);
                }
                else
                {
                    result = near;
                }
                WorldTransport.TM_DropPodUtility.MakeDropPodAt(result, map, dropPods[i], TorannMagicDefOf.TM_ActiveLightPod, TorannMagicDefOf.TM_LightPodIncoming, draftFlag);
            }
        }

        public static void RemovePawnsFromWorldPawns(List<ActiveTransporterInfo> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner innerContainer = pods[i].innerContainer;
                for (int j = 0; j < innerContainer.Count; j++)
                {
                    Pawn pawn = innerContainer[j] as Pawn;
                    if (pawn != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(pawn);
                    }
                }
            }
        }
    }
}
