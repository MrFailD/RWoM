using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_LowFlight : HediffComp
    {
        private bool initialized;
        public List<Graphic> _nakedGraphicCycle = new List<Graphic>();
        public Graphic _nakedGraphicDefault;
        public Graphic _nakedGraphicActive;
        private int cycleIndex;
        public int cycleFrequency = 12;
        public float minDistanceToFly = 10;

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

        public Graphic GetActiveGraphic
        {
            get
            {
                if (parent.Severity > 1f)
                {
                    return _nakedGraphicActive;
                }
                return _nakedGraphicDefault;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn != null & parent != null && !Pawn.Dead && !Pawn.Downed && Pawn.Map != null)
            {
                if (!initialized)
                {
                    if(Pawn.def == TorannMagicDefOf.TM_SpiritCrowR)
                    {
                        _nakedGraphicCycle.Clear();
                        _nakedGraphicCycle.Add(Pawn.kindDef.lifeStages[0].bodyGraphicData.Graphic);
                        _nakedGraphicCycle.Add(Pawn.kindDef.lifeStages[1].bodyGraphicData.Graphic);
                        _nakedGraphicDefault = Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
                        //_nakedGraphicCycle.Add(GraphicDatabase.Get<Graphic_Multi>("PawnKind/HPL_Crow_FlyingDown", ShaderDatabase.Cutout, this.Pawn.Graphic.drawSize, Color.white));
                        _nakedGraphicActive = _nakedGraphicCycle[0];
                    }
                    initialized = true;
                }

                if (_nakedGraphicActive != null && _nakedGraphicCycle != null && _nakedGraphicCycle.Count > 0)
                {
                    if (Find.TickManager.TicksGame % 31 == 0)
                    {
                        if (Pawn.jobs != null && Pawn.CurJobDef != JobDefOf.Wait_Wander && Pawn.CurJobDef != JobDefOf.Wait && Pawn.CurJobDef != JobDefOf.GotoWander)
                        {
                            Thing carriedThing = Pawn.carryTracker.CarriedThing;
                            LocalTargetInfo target = Pawn.CurJob.targetA;
                            if (carriedThing != null)
                            {
                                target = Pawn.CurJob.targetB;
                            }
                            if (target != null && (target.Cell - Pawn.Position).LengthHorizontal > minDistanceToFly)
                            {
                                parent.Severity = 1.5f;
                            }
                            else
                            {
                                parent.Severity = .5f;
                            }
                        }
                        else
                        {
                            parent.Severity = .5f;
                        }
                    }

                    if (parent.Severity >= 1f && Find.TickManager.TicksGame % cycleFrequency == 0)
                    {
                        cycleIndex++;
                        if (cycleIndex >= _nakedGraphicCycle.Count)
                        {
                            cycleIndex = 0;
                        }
                        _nakedGraphicActive = _nakedGraphicCycle[cycleIndex];
                    }
                }
            }
        }
    }
}
