using System;
using Verse;
using Verse.AI;
using AbilityUser;



namespace TorannMagic
{
    public class Verb_PsionicBarrier : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            if(verbProps.targetParams.canTargetLocations)
            {
                Job job = new Job(TorannMagicDefOf.JobDriver_PsionicBarrier, currentTarget);
                CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
            else
            {
                Job job = new Job(TorannMagicDefOf.JobDriver_PsionicBarrier, CasterPawn.Position);
                CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }                       
            Ability.PostAbilityAttempt();

            burstShotsLeft = 0;
            return false;
        }
    }
}
