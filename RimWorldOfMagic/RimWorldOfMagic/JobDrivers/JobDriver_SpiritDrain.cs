using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;
using UnityEngine;


namespace TorannMagic
{
    internal class JobDriver_SpiritDrain : JobDriver
    {
        private const TargetIndex building = TargetIndex.A;

        private int age = -1;
        private int drainFrequency = 20;
        private int moteFrequency = 4;
        private int duration = 600;
        private float drainFrequencyReduction;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            Toil discordance = new Toil();
            Pawn target = TargetThingA as Pawn;
            Need_Spirit spiritNeed = pawn.needs.TryGetNeed(TorannMagicDefOf.TM_SpiritND) as Need_Spirit;
            int effVal = TM_Calc.GetSkillEfficiencyLevel(pawn, TorannMagicDefOf.TM_SpiritDrain);
            int pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_SpiritDrain);
            int verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_SpiritDrain);
            if(pawn.story != null && pawn.story.Adulthood != null && pawn.story.Adulthood.identifier == "tm_regret_spirit")
            {
                drainFrequencyReduction = 4;
            }
            duration =  Mathf.RoundToInt(600 * (1f - (.15f * verVal)));
            drainFrequency = Mathf.RoundToInt((20 - drainFrequencyReduction) * (1f - (.15f * verVal)));
            discordance.initAction = delegate
            {
                if (age > duration)
                {
                    EndJobWith(JobCondition.Succeeded);
                }    
                if(target.DestroyedOrNull())
                {
                    EndJobWith(JobCondition.Errored);
                }
                if (target.Map == null)
                {
                    EndJobWith(JobCondition.Errored);
                }
                if (target.Dead)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                Map map = pawn.Map;
                ticksLeftThisToil = 10;
                if(spiritNeed == null)
                {
                    EndJobWith(JobCondition.InterruptForced);
                }
            };
            discordance.tickAction = delegate
            {
                if (!target.DestroyedOrNull() && !target.Dead && target.Map != null)
                {
                    if (target != null)
                    {
                        pawn.rotationTracker.FaceTarget(target);
                    }
                    float targetDistance = (pawn.Position - target.Position).LengthHorizontal;
                    float rndSpeed = Rand.Range(5f, 10f);
                    if (Find.TickManager.TicksGame % moteFrequency == 0)
                    {
                        float angle = (Quaternion.AngleAxis(-90, Vector3.up) * TM_Calc.GetVector(pawn.Position, target.Position)).ToAngleFlat();
                        Vector3 startPos = target.DrawPos;
                        startPos.x += Rand.Range(-.3f, .3f);
                        startPos.z += Rand.Range(-.3f, .3f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PurpleSmoke, startPos, target.Map, Rand.Range(.15f, .3f), (.6f * targetDistance)/rndSpeed, .05f, (.5f * targetDistance)/rndSpeed, Rand.Range(-100, 100), rndSpeed, (angle), Rand.Range(0, 360));
                    }
                    if (Find.TickManager.TicksGame % moteFrequency == 1)
                    {
                        float angle = (Quaternion.AngleAxis(-90, Vector3.up) * TM_Calc.GetVector(pawn.Position, target.Position)).ToAngleFlat();
                        Vector3 startPos = target.DrawPos;
                        startPos.x += Rand.Range(-.2f, .2f);
                        startPos.z += Rand.Range(-.2f, .2f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Regen, startPos, target.Map, Rand.Range(.075f, .125f), (.6f * targetDistance) / rndSpeed, .05f, (.5f * targetDistance) / rndSpeed, Rand.Range(-100, 100), rndSpeed, (angle), Rand.Range(0, 360));
                    }
                    if(Find.TickManager.TicksGame % moteFrequency == 2)
                    {
                        TM_MoteMaker.ThrowCastingMote_Spirit(pawn.DrawPos, pawn.Map, .65f);
                    }
                    if (Find.TickManager.TicksGame % drainFrequency == 0)
                    {
                        float angle = (Quaternion.AngleAxis(-90, Vector3.up) * TM_Calc.GetVector(pawn.Position, target.Position)).ToAngleFlat();
                        Vector3 startPos = target.DrawPos;
                        startPos.x += Rand.Range(-.2f, .2f);
                        startPos.z += Rand.Range(-.2f, .2f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Regen, startPos, target.Map, Rand.Range(.075f, .125f), (.6f * targetDistance) / rndSpeed, .05f, (.5f * targetDistance) / rndSpeed, Rand.Range(-100, 100), rndSpeed, (angle), Rand.Range(0, 360));

                        float ch = TM_Calc.GetSpellSuccessChance(pawn, target, true);
                        float sev = .011f * ch;                        
                        spiritNeed.GainNeed(Rand.Range(.5f, 1f) * ch);
                        if (target.Faction == pawn.Faction)
                        {
                            sev *= (1f - (.15f * pwrVal));
                            HealthUtility.AdjustSeverity(target, TorannMagicDefOf.TM_SpiritDrainHD, sev);
                            Hediff hd = target.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_SpiritDrainHD);
                            if(hd != null && hd.Severity > .95f)
                            {
                                hd.Severity = .949f;
                                age = duration;
                            }
                            if (spiritNeed.CurLevel >= spiritNeed.MaxLevel)
                            {
                                age = duration;
                            }
                        }
                        else
                        {
                            sev *= (1f + (.15f * pwrVal));
                            HealthUtility.AdjustSeverity(target, TorannMagicDefOf.TM_SpiritDrainHD, sev);
                        }                                                
                        if(target.Dead)
                        {
                            spiritNeed.GainNeed(Rand.Range(8f, 12f));
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Ghost, pawn.DrawPos, pawn.Map, Rand.Range(.4f, .6f), .1f, .05f, .05f, 0, Rand.Range(1, 2), 0, 0);
                            age = duration;
                        }                        
                    }                    
                }
                else
                {
                    age = duration;
                }
                age++;               
                ticksLeftThisToil = Mathf.RoundToInt((float)(duration - age));
                if (age > duration)
                {
                    EndJobWith(JobCondition.Succeeded);
                }                              
            };
            discordance.defaultCompleteMode = ToilCompleteMode.Delay;
            discordance.defaultDuration = duration;
            discordance.WithProgressBar(TargetIndex.A, delegate
            {
                if (pawn.DestroyedOrNull() || pawn.Dead || pawn.Downed)
                {
                    return 1f;
                }
                return (float)(age/duration);

            }, false, 0f);
            discordance.AddFinishAction(delegate
            {

            });
            yield return discordance;
        }        
    }
}
