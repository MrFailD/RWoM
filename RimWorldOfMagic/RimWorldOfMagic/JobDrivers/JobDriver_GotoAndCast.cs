using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI;
using UnityEngine;
using AbilityUser;

namespace TorannMagic
{
    internal class JobDriver_GotoAndCast : JobDriver
    {
        private const TargetIndex caster = TargetIndex.B;

        private int age = -1;
        private int lastEffect = 0;
        private int ticksTillEffects = 20;
        public int duration = 5;
        private Vector3 positionBetween = Vector3.zero;
        public PawnAbility ability = null;
        private Thing targetThing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(TargetA, job, 1, 1, null, errorOnFailed))
            {
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (TargetA.Thing != null)
            {
                targetThing = TargetA.Thing;
            }
            Toil gotoThing = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return gotoThing;
            Toil doSpell = new Toil();
            doSpell.initAction = delegate
            {
                if(ability != null)
                {
                    duration = (int)(ability.Def.MainVerb.warmupTime * 60 * pawn.GetStatValue(StatDefOf.AimingDelayFactor, false));
                }
                if (age > duration)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                if (targetThing != null && (targetThing.DestroyedOrNull() || targetThing.Map == null))
                {
                    EndJobWith(JobCondition.Incompletable);
                }

                if (targetThing != null)
                {
                    pawn.rotationTracker.FaceTarget(targetThing);
                }
            };
            doSpell.tickAction = delegate
            {
                if (targetThing != null && (targetThing.DestroyedOrNull() || targetThing.Map == null))
                {
                    EndJobWith(JobCondition.Incompletable);
                }                
                age++;
                ticksLeftThisToil = duration - age;
                if (Find.TickManager.TicksGame % 12 == 0)
                {
                    TM_MoteMaker.ThrowCastingMote(pawn.DrawPos, pawn.Map, Rand.Range(1.2f, 2f));                    
                }

                if (age > duration)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            doSpell.defaultCompleteMode = ToilCompleteMode.Never;
            doSpell.defaultDuration = duration;
            doSpell.AddFinishAction(delegate
            {
                if (ability != null)
                {
                    if(ability.Def == TorannMagicDefOf.TM_Transmutate && targetThing != null)
                    {
                        bool flagRawResource = false;
                        bool flagStuffItem = false;
                        bool flagNoStuffItem = false;
                        bool flagNutrition = false;
                        bool flagCorpse = false;

                        TM_Calc.GetTransmutableThingFromCell(targetThing.Position, pawn, out flagRawResource, out flagStuffItem, out flagNoStuffItem, out flagNutrition, out flagCorpse);
                        TM_Action.DoTransmutate(pawn, targetThing, flagNoStuffItem, flagRawResource, flagStuffItem, flagNutrition, flagCorpse);                      
                    }
                    else if(ability.Def == TorannMagicDefOf.TM_RegrowLimb)
                    {
                        SpawnThings tempThing = new SpawnThings();
                        tempThing.def = ThingDef.Named("SeedofRegrowth");
                        Verb_RegrowLimb.SingleSpawnLoop(tempThing, TargetA.Cell, pawn.Map);
                    }
                    ability.PostAbilityAttempt();
                }
                //AssignXP();
            });
            yield return doSpell;
        }

        private void AssignXP()
        {
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();

            if (comp != null)
            {
                try
                {

                    int xpBase = Rand.Range(50, 75);
                    int xpGain = Mathf.RoundToInt(xpBase * comp.xpGain);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "XP +" + xpGain, -1f);
                    comp.MightUserXP += xpGain;
                    if (pawn.needs.joy != null)
                    {
                        pawn.needs.joy.GainJoy(.4f, TorannMagicDefOf.Social);
                    }
                    if (pawn.skills != null)
                    {
                        pawn.skills.Learn(SkillDefOf.Social, Rand.Range(200f, 500f));
                    }
                }
                catch (NullReferenceException ex)
                {
                    //failed
                }
            }
        }        
    }
}