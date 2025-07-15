﻿using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace TorannMagic.WorldTransport
{
    public class TM_TransportPodsArrivalAction_LandAtExactCell : TransportersArrivalAction
    {
        private MapParent mapParent;
        private IntVec3 cell;

        public bool draftFlag;

        public TM_TransportPodsArrivalAction_LandAtExactCell()
        {
        }

        public TM_TransportPodsArrivalAction_LandAtExactCell(MapParent mapParent, IntVec3 cell, bool dFlag = false)
        {
            this.mapParent = mapParent;
            this.cell = cell;
            draftFlag = dFlag;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref cell, "cell");
        }

        public override bool GeneratesMap { get; }

        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods,
            PlanetTile destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
            if (!(bool)floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (mapParent != null && mapParent.Tile != destinationTile)
            {
                return false;
            }
            return CanLandInSpecificCell(pods, mapParent);
        }

        public override void Arrived(List<ActiveTransporterInfo> pods, PlanetTile tile)
        {
            TM_TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, cell, mapParent.Map, true, draftFlag);
        }

        public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
            {
                return false;
            }
            if (mapParent.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownDaysLeft().ToString("0.#")));
            }
            return true;
        }
    }
}
