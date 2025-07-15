using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;
using System;
using AbilityUser;

namespace TorannMagic
{
    internal class JobDriver_TM_GotoAndWait : JobDriver
    {
        private int age = -1;
        public int durationTicks = 60;
        private Pawn waitForPawn;
        public JobDef targetJobDef;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            //this.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            
            Toil waitFor = new Toil()
            {
                initAction = () =>
                {
                    if (TargetB != null)
                    {
                        waitForPawn = TargetB.Thing as Pawn;
                        if(waitForPawn != null && waitForPawn.jobs != null && waitForPawn.jobs.curJob != null && waitForPawn.CurJobDef != null)
                        {
                            targetJobDef = waitForPawn.CurJobDef;
                        }
                    }
                    durationTicks = job.expiryInterval;
                    if(age > durationTicks)
                    {
                        EndJobWith(JobCondition.InterruptForced);
                    }
                },
                tickAction = () =>
                {
                    if(waitForPawn != null)
                    {
                        if (targetJobDef != null && waitForPawn.jobs != null && waitForPawn.jobs.curJob != null && waitForPawn.CurJobDef != targetJobDef)
                        {
                            EndJobWith(JobCondition.InterruptForced);
                        }
                    }
                    if (age > durationTicks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    age++;
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            waitFor.defaultDuration = durationTicks;
            waitFor.WithProgressBar(TargetIndex.A, delegate
            {
                if (pawn.DestroyedOrNull() || pawn.Dead || pawn.Downed)
                {
                    return 1f;
                }
                return 1f - (float)waitFor.actor.jobs.curDriver.ticksLeftThisToil / durationTicks;

            }, false, 0f);
            yield return waitFor;         
        }
    }
}