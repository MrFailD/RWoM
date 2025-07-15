using System.Collections.Generic;
using Verse.AI;
using RimWorld;
using Verse;
using AbilityUser;
using System.Linq;


namespace TorannMagic
{
    internal class JobDriver_DoFor : JobDriver
    {
        private int age = -1;
        public int durationTicks = 60;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil doFor = new Toil()
            {
                initAction = () =>
                {
                    if(age > durationTicks)
                    {
                        EndJobWith(JobCondition.InterruptForced);
                    }
                },
                tickAction = () =>
                {
                    if (age > durationTicks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    age++;
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            doFor.defaultDuration = durationTicks;
            doFor.WithProgressBar(TargetIndex.A, delegate
            {
                if (pawn.DestroyedOrNull() || pawn.Dead || pawn.Downed)
                {
                    return 1f;
                }
                return 1f - (float)doFor.actor.jobs.curDriver.ticksLeftThisToil / durationTicks;

            }, false, 0f);
            yield return doFor;         
        }
    }
}