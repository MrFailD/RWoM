using System;
using Verse;
using Verse.AI;
using AbilityUser;



namespace TorannMagic
{
    public class Verb_LightningTrap : Verb_UseAbility
    {
        private bool validTarg;
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.Thing != null && targ.Thing == caster)
            {
                return verbProps.targetParams.canTargetSelf;
            }
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    ShootLine shootLine;
                    validTarg = TryFindShootLineFromTo(root, targ, out shootLine);
                }
                else
                {
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;

            CellRect cellRect = CellRect.CenteredOn(currentTarget.Cell, 1);
            cellRect.ClipInsideMap(map);
            IntVec3 centerCell = cellRect.CenterCell;

            if ((centerCell.IsValid && centerCell.Standable(map)))
            {
                Job job = new Job(TorannMagicDefOf.JobDriver_PlaceLightningTrap, currentTarget);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
            Ability.PostAbilityAttempt();

            burstShotsLeft = 0;
            return false;
        }
    }
}
