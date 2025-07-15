using System;
using Verse;
using Verse.AI;
using AbilityUser;



namespace TorannMagic
{
    public class Verb_Meditate : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;

            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Job job = new Job(TorannMagicDefOf.JobDriver_TM_Meditate, CasterPawn.Position);
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            
            Ability.PostAbilityAttempt();

            burstShotsLeft = 0;
            return false;
        }
    }
}
