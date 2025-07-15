using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Building_TMCooler : Building
    {

        private float arcaneEnergyCur = 0;
        private float arcaneEnergyMax = 1;

        private static readonly Material coolerMat_1 = MaterialPool.MatFrom("Other/cooler", false);
        private static readonly Material coolerMat_2 = MaterialPool.MatFrom("Other/coolerB", false);
        private static readonly Material coolerMat_3 = MaterialPool.MatFrom("Other/coolerC", false);

        private int matRng;
        private float matMagnitude = 1;
        private int nextSearch;
        public bool defensive;
        public bool buffCool;
        public bool buffFresh;

        private bool initialized;

        //public override void SpawnSetup(Map map, bool respawningAfterLoad)
        //{
        //    base.SpawnSetup(map, respawningAfterLoad);
        //    //LessonAutoActivator.TeachOpportunity(ConceptDef.Named("TM_Portals"), OpportunityType.GoodToKnow);
        //}

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref defensive, "defensive", false, false);
            Scribe_Values.Look<bool>(ref buffCool, "buffCool", false, false);
            Scribe_Values.Look<bool>(ref buffFresh, "buffFresh", false, false);
            base.ExposeData();
        }

        protected override void Tick()
        {
            if(!initialized)
            {
                initialized = true;
                nextSearch = Find.TickManager.TicksGame + Rand.Range(400, 500);
            }
            if(Find.TickManager.TicksGame % 8 == 0)
            {
                matRng++;
                if(matRng >= 3)
                {
                    matRng = 0;
                }
            }
            if (Find.TickManager.TicksGame >= nextSearch)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(400, 500);
                if (defensive)
                {
                    List<Pawn> ePawns = TM_Calc.FindAllPawnsAround(Map, Position, 10, factionInt, false);
                    if (ePawns != null && ePawns.Count > 0)
                    {
                        for (int i = 0; i < ePawns.Count; i++)
                        {
                            if (ePawns[i].Faction.HostileTo(Faction))
                            {
                                HealthUtility.AdjustSeverity(ePawns[i], TorannMagicDefOf.TM_FrostSlowHD, .4f);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Ice, ePawns[i].DrawPos, Map, 1f, .3f, .1f, .8f, Rand.Range(-100, 100), .4f, Rand.Range(0, 35), Rand.Range(0, 360));
                            }
                        }
                    }
                }
                if(buffCool)
                {
                    List<Pawn> pList = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
                    if(pList != null && pList.Count > 0)
                    {
                        for(int i =0; i < pList.Count; i++)
                        {
                            Pawn p = pList[i];
                            if (p.health != null && p.health.hediffSet != null)
                            {
                                HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_CoolHD, 0.25f);
                            }
                        }
                    }
                }
                if(buffFresh)
                {
                    List<Pawn> pList = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
                    if (pList != null && pList.Count > 0)
                    {
                        for (int i = 0; i < pList.Count; i++)
                        {
                            Pawn p = pList[i];
                            if (p.health != null && p.health.hediffSet != null)
                            {
                                HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_RefreshedHD, 0.2f);
                            }
                        }
                    }
                }
            }
            base.Tick();

        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 vector = base.DrawPos;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            Vector3 s = new Vector3(matMagnitude, matMagnitude, matMagnitude);
            Matrix4x4 matrix = default(Matrix4x4);
            float angle = 0f;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            if (matRng == 0)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, coolerMat_1, 0);
            }
            else if (matRng == 1)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, coolerMat_2, 0);
            }
            else 
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, coolerMat_3, 0);
            }            
        }
    }
}
