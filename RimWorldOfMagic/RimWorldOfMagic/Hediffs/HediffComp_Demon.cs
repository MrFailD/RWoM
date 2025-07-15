using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Demon : HediffComp
    {
        private int ticksUntilDestruction = 6660;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref ticksUntilDestruction, "ticksUntilDestruction", 6660, false);
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
            if (spawned)
            {

            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn.DestroyedOrNull();
            ticksUntilDestruction--;
            if (!flag)
            {
                if (Pawn.Spawned && !Pawn.Dead && !Pawn.Downed)
                {
                    if (Find.TickManager.TicksGame % 2 == 0)
                    {
                        DrawEffects();
                        if(Find.TickManager.TicksGame % 20 == 0)
                        {
                            Hediff hd = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_NightshadeToxinHD);
                            if(hd != null)
                            {
                                hd.Severity -= .04f;
                            }
                        }
                        if (Find.TickManager.TicksGame % 300 == 0)
                        {
                            IEnumerable<Hediff_Injury> injuries = Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>();
                            if(injuries.Count() > 20 && parent.Severity < 1f)
                            {
                                HealthUtility.AdjustSeverity(Pawn, Def, -10);
                                HealthUtility.AdjustSeverity(Pawn, Def, 1.5f);
                            }
                            else if(injuries.Count() > 40 && parent.Severity < 2f)
                            {
                                HealthUtility.AdjustSeverity(Pawn, Def, -10);
                                HealthUtility.AdjustSeverity(Pawn, Def, 2.5f);
                            }
                        }
                    }
                    if(ticksUntilDestruction <= 0)
                    {
                        Pawn.Map.weatherManager.eventHandler.AddEvent(new TM_WeatherEvent_MeshFlash(Pawn.Map, Pawn.Position, TM_MatPool.redLightning));
                        Pawn.Destroy();
                    }
                }
            }
        }

        public void DrawEffects()
        {
            Vector3 headOffset = Pawn.DrawPos;
            float sizeOffset = Pawn.RaceProps.lifeStageAges.FirstOrDefault().def.bodySizeFactor;
            if (Pawn.Rotation == Rot4.East)
            {
                headOffset.z += .65f * sizeOffset;
                headOffset.x += .4f * sizeOffset;
                float throwAngle = Rand.Range(-5, -20);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, throwAngle, Rand.Range(0, 360));
            }
            else if (Pawn.Rotation == Rot4.West)
            {

                headOffset.z += .65f * sizeOffset;
                headOffset.x += -.4f * sizeOffset;
                float throwAngle = Rand.Range(5, 20);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, throwAngle, Rand.Range(0, 360));
            }
            else if (Pawn.Rotation == Rot4.South)
            {
                headOffset.z += .75f * sizeOffset;
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, Rand.Range(-30, 30), Rand.Range(0, 360));
            }
            else
            {
                headOffset.z += .75f * sizeOffset;
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, Rand.Range(-30, 30), Rand.Range(0, 360));
            }
        }

    }
}
