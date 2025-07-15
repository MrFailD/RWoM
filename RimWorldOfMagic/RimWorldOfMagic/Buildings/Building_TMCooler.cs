using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMCooler : Building
    {
        private static readonly Material CoolerMat1 = MaterialPool.MatFrom("Other/cooler", false);
        private static readonly Material CoolerMat2 = MaterialPool.MatFrom("Other/coolerB", false);
        private static readonly Material CoolerMat3 = MaterialPool.MatFrom("Other/coolerC", false);

        private int matRng;
        private const float MatMagnitude = 1;
        private int nextSearch;
        public bool Defensive;
        public bool BuffCool;
        public bool BuffFresh;

        private bool initialized;

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref Defensive, "defensive", false, false);
            Scribe_Values.Look<bool>(ref BuffCool, "buffCool", false, false);
            Scribe_Values.Look<bool>(ref BuffFresh, "buffFresh", false, false);
            base.ExposeData();
        }

        protected override void Tick()
        {
            int currentTick = Find.TickManager.TicksGame;

            InitializeTickState(currentTick);
            UpdateMatRng(currentTick);

            if (currentTick >= nextSearch)
            {
                nextSearch = currentTick + Rand.Range(400, 500);

                TryApplyDefensiveEffects();
                TryApplyBuffCool();
                TryApplyBuffFresh();
            }

            base.Tick();
        }

        private void InitializeTickState(int currentTick)
        {
            if (!initialized)
            {
                initialized = true;
                nextSearch = currentTick + Rand.Range(400, 500);
            }
        }

        private void UpdateMatRng(int currentTick)
        {
            if (currentTick % 8 == 0)
            {
                matRng = (matRng + 1) % 3;
            }
        }

        private void TryApplyDefensiveEffects()
        {
            if (!Defensive)
            {
                return;
            }

            List<Pawn> enemyPawns = TM_Calc.FindAllPawnsAround(Map, Position, 10, factionInt, false);
            if (enemyPawns == null || enemyPawns.Count == 0)
            {
                return;
            }

            foreach (Pawn enemyPawn in enemyPawns)
            {
                if (!enemyPawn.Faction.HostileTo(Faction))
                {
                    continue;
                }
                HealthUtility.AdjustSeverity(enemyPawn, TorannMagicDefOf.TM_FrostSlowHD, .4f);
                TM_MoteMaker.ThrowGenericMote(
                    TorannMagicDefOf.Mote_Ice,
                    enemyPawn.DrawPos,
                    Map,
                    1f, .3f, .1f, .8f,
                    Rand.Range(-100, 100),
                    .4f,
                    Rand.Range(0, 35),
                    Rand.Range(0, 360)
                );
            }
        }

        private void TryApplyBuffCool()
        {
            if (!BuffCool) return;

            List<Pawn> friendlyPawns = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
            if (friendlyPawns == null || friendlyPawns.Count == 0) return;

            foreach (Pawn pawn in friendlyPawns)
            {
                if (pawn.health?.hediffSet != null)
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_CoolHD, 0.25f);
                }
            }
        }

        private void TryApplyBuffFresh()
        {
            if (!BuffFresh) return;

            List<Pawn> friendlyPawns = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
            if (friendlyPawns == null || friendlyPawns.Count == 0) return;

            foreach (Pawn pawn in friendlyPawns)
            {
                if (pawn.health?.hediffSet != null)
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_RefreshedHD, 0.2f);
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 vector = base.DrawPos;
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 s = new Vector3(MatMagnitude, MatMagnitude, MatMagnitude);
            Matrix4x4 matrix = default(Matrix4x4);
            float angle = 0f;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            if (matRng == 0)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, CoolerMat1, 0);
            }
            else if (matRng == 1)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, CoolerMat2, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, CoolerMat3, 0);
            }
        }
    }
}