using System;
using Verse.AI;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class JobDriver_AddEnchantingGem : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            Toil gotoThing = new Toil();
            gotoThing.initAction = delegate
            {
                pawn.pather.StartPath(TargetThingA, PathEndMode.ClosestTouch);
            };
            gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return gotoThing;
            yield return Toils_Enchant.TakeEnchantGem(TargetIndex.A, job.count);
        }
    }
}
