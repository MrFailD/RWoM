using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Dominate : HediffComp
    {
        private bool initialized;
        private int age;
        private int infectionRate = 120;
        private int lastInfection = 240;
        private int hediffPwr;
        private int infectionRadius = 3;
        private int effVal;
        private int verVal;
        private float minimumSev = .3f;

        public int EffVal
        {
            get
            {
                return effVal;
            }
            set
            {
                effVal = value;
            }
        }

        public int VerVal
        {
            get
            {
                return verVal;
            }
            set
            {
                verVal = value;
            }
        }

        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            minimumSev = .3f - (.03f * effVal);
            infectionRadius = 3 + verVal;

            if (spawned)
            {
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 1f);
                if (Def.defName == "TM_SDDominateHD_III" || Def.defName == "TM_WDDominateHD_III")
                {
                    hediffPwr = 3;
                }
                else if (Def.defName == "TM_SDDominateHD_II" || Def.defName == "TM_WDDominateHD_II")
                {
                    hediffPwr = 2;
                }
                else if (Def.defName == "TM_SDDominateHD_I" || Def.defName == "TM_WDDominateHD_I")
                {
                    hediffPwr = 1;
                }
                else
                {
                    hediffPwr = 0;
                }
                infectionRate -= hediffPwr * 40;
                for (int i = 0; i < 4; i++)
                {
                    TM_MoteMaker.ThrowShadowMote(Pawn.Position.ToVector3(), Pawn.Map, Rand.Range(.6f, 1f));
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn != null & parent != null)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
            }
            age++;

            if (Find.TickManager.TicksGame % 60 == 0)
            {
                HealthUtility.AdjustSeverity(Pawn, Def, -0.1f);
            }

            if (age > (lastInfection + infectionRate) && parent.Severity > minimumSev)
            {
                bool infectionFlag = false;
                HealthUtility.AdjustSeverity(Pawn, Def, -1 * (minimumSev));
                lastInfection = age;
                Pawn pawn = Pawn as Pawn;
                Map map = pawn.Map;
                if (!pawn.DestroyedOrNull() && pawn.Map != null)
                {
                    IntVec3 curCell;
                    Pawn victim = null;
                    IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(pawn.Position, infectionRadius, true);
                    for (int i = 0; i < targets.Count(); i++)
                    {
                        curCell = targets.ToArray<IntVec3>()[i];
                        if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                        {
                            victim = curCell.GetFirstPawn(map);
                        }

                        if (victim != null && victim.Faction == pawn.Faction && infectionFlag == false && !victim.health.hediffSet.HasHediff(Def))
                        {
                            //bool hediffFlag = (victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SDDominateHD) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SDDominateHD_I) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SDDominateHD_II) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SDDominateHD_III) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_WDDominateHD) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_WDDominateHD) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_WDDominateHD) ||
                            //    victim.health.hediffSet.HasHediff(TorannMagicDefOf.TM_WDDominateHD));
                            infectionFlag = true;
                            float angle = GetAngleFromTo(pawn.Position.ToVector3(), victim.Position.ToVector3());
                            HealthUtility.AdjustSeverity(victim, Def, parent.Severity);
                            for (int j = 0; j < 3; j++)
                            {
                                TM_MoteMaker.ThrowShadowMote(pawn.DrawPos, map, Rand.Range(.6f, 1f), Rand.Range(50, 80), Rand.Range(1f, 2f), angle + Rand.Range(-20, 20));
                            }
                        }
                    }

                }

            }
        }

        private float GetAngleFromTo(Vector3 from, Vector3 to)
        {
            Vector3 heading = (to - from);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            float directionAngle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
            return directionAngle;
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < .1f;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", 0, false);
            Scribe_Values.Look<int>(ref infectionRate, "infectionRate", 240, false);
            Scribe_Values.Look<int>(ref lastInfection, "lastInfection", 240, false);
            Scribe_Values.Look<int>(ref hediffPwr, "hediffPwr", 0, false);
            Scribe_Values.Look<int>(ref infectionRadius, "infectionRadius", 3, false);
            Scribe_Values.Look<int>(ref effVal, "effVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
        }
        
    }
}
