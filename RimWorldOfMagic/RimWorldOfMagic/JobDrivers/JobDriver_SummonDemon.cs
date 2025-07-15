using System.Collections.Generic;
using Verse.AI;
using RimWorld;
using Verse;
using AbilityUser;
using System.Linq;


namespace TorannMagic
{
    internal class JobDriver_SummonDemon : JobDriver
    {
        private int age = -1;
        public int durationTicks = 1200;

        private CompAbilityUserMagic comp;
        private Pawn markedPawn;

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
                    comp = pawn.GetCompAbilityUserMagic();
                    markedPawn = comp.soulBondPawn;
                },
                tickAction = () =>
                {
                    if(!markedPawn.Spawned)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    if (age > durationTicks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    if (Find.TickManager.TicksGame % 12 == 0)
                    {
                        TM_MoteMaker.ThrowCastingMote(pawn.DrawPos, pawn.Map, Rand.Range(1.2f, 2f));
                        TM_MoteMaker.ThrowShadowMote(markedPawn.DrawPos, markedPawn.Map, Rand.Range(.8f, 1.2f), Rand.Range(-200, 200), Rand.Range(1, 2), Rand.Range(1.5f, 2f));
                    }
                    if(Find.TickManager.TicksGame % 6 ==0)
                    {
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Siphon, markedPawn.DrawPos, markedPawn.Map, Rand.Range(.15f, .3f), Rand.Range(.2f, .4f), Rand.Range(.1f, .2f), Rand.Range(.3f, .5f), Rand.Range(-300, 300), Rand.Range(.5f, 3f), Rand.Range(-90, 90), 0);
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