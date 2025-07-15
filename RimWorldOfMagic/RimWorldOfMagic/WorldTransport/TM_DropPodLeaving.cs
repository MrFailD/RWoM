using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TorannMagic.WorldTransport
{
    [StaticConstructorOnStartup]
    public class TM_DropPodLeaving : FlyShipLeaving
    {
        public IntVec3 arrivalCell = default(IntVec3);
        public bool draftFlag = false;

        private bool alreadyLeft;
        private static List<Thing> tmpActiveDropPods = new List<Thing>();
        private int maxTicks = 40;
        private Vector3 startingPos = default(Vector3);

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BombardMat = MaterialPool.MatFrom("Other/Bombardment", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        private const int beamLength = 200;

        protected override void LeaveMap()
        {
            ModOptions.Constants.SetPawnInFlight(true);
            if (alreadyLeft)
            {
                base.LeaveMap();
            }
            else if (groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + groupID);
                Destroy();
            }
            else if (destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
                Destroy();
            }
            else
            {
                Lord lord = TransporterUtility.FindLord(groupID, Map);
                if (lord != null)
                {
                    Map.lordManager.RemoveLord(lord);
                }
                TM_TravelingTransportPods travelingTransportPods = (TM_TravelingTransportPods)WorldObjectMaker.MakeWorldObject(TorannMagicDefOf.TM_TravelingTransportLightBeam);
                travelingTransportPods.Tile = Map.Tile;
                travelingTransportPods.SetFaction(Faction.OfPlayer);
                travelingTransportPods.destinationTile = destinationTile;
                travelingTransportPods.arrivalAction = arrivalAction;
                travelingTransportPods.destinationCell = arrivalCell;
                if (def == TorannMagicDefOf.TM_LightPodLeaving)
                {
                    travelingTransportPods.TravelSpeed = .025f;
                    travelingTransportPods.draftFlag = draftFlag;
                }
                Find.WorldObjects.Add(travelingTransportPods);
                tmpActiveDropPods.Clear();
                tmpActiveDropPods.AddRange(Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveTransporter));
                for (int i = 0; i < tmpActiveDropPods.Count; i++)
                {
                    TM_DropPodLeaving dropPodLeaving = tmpActiveDropPods[i] as TM_DropPodLeaving;
                    if (dropPodLeaving != null && dropPodLeaving.groupID == groupID)
                    {
                        dropPodLeaving.alreadyLeft = true;
                        travelingTransportPods.AddPod(dropPodLeaving.Contents, justLeftTheMap: true);
                        dropPodLeaving.Contents = null;
                        dropPodLeaving.Destroy();
                    }
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            startingPos = DrawPos;
            base.SpawnSetup(map, respawningAfterLoad);
        }

        protected override void Tick()
        {
            if (ticksToImpact == maxTicks)
            {
                LeaveMap();
            }
            base.Tick();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            DrawLightBeam();
        }

        public void DrawLightBeam()
        {
            float lanceWidth = 4f;
            if (ticksToImpact < (maxTicks * .5f))
            {
                lanceWidth *= (float)ticksToImpact / maxTicks;
            }
            if (ticksToImpact > (maxTicks * .5f))
            {
                lanceWidth *= (float)(maxTicks - ticksToImpact) / maxTicks;
            }
            lanceWidth *= Rand.Range(.9f, 1.1f);
            Vector3 angleVector = Vector3Utility.FromAngleFlat(angle - 90) * .5f * beamLength;
            Vector3 drawPos = startingPos + angleVector;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, beamLength));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(startingPos + Vector3Utility.FromAngleFlat(angle + 90) * .5f * lanceWidth, Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, lanceWidth));                 //drawer for beam start
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
        }
    }
}
