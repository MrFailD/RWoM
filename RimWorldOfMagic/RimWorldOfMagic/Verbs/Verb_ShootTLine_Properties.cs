using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;


namespace TorannMagic
{
    public class Verb_ShootTLine_Properties : VerbProperties_Ability
    {
        public int distBetweenShots = 1;
        public Verb_ShootTLine_Properties() : base()
        {
            verbClass = verbClass ?? typeof(Verb_ShootTLine);
        }
    }

    public class Verb_ShootTLine : Verb_SB
    {
        private int ShotsSoFar;
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
            Verb_ShootTLine_Properties Properties = verbProps as Verb_ShootTLine_Properties;
            IntVec3 angleVec = (currentTarget.Cell - CasterPawn.Position).RotatedBy(Rot4.FromAngleFlat(90));
            double distanceToTarget = Math.Pow(Math.Pow((currentTarget.Cell.x - CasterPawn.Position.x), 2) + (Math.Pow((currentTarget.Cell.z - CasterPawn.Position.z), 2)), 0.5);
            float directionX = angleVec.x / (float)distanceToTarget;
            float directionZ = angleVec.z / (float)distanceToTarget;
            float cellOffset = ((ShotsPerBurst - 1) / 2 - ShotsSoFar) * Properties.distBetweenShots;
            IntVec3 offsetTarget = currentTarget.Cell;
            offsetTarget.x += (int)(directionX * cellOffset);
            offsetTarget.z += (int)(directionZ * cellOffset);
            TryLaunchProjectile(Projectile, offsetTarget);
            ShotsSoFar++;
            return true;
        }

        //private float CalculateAngles(IntVec3 originPos, IntVec3 destPos)
        //{
        //    float hyp = Mathf.Sqrt((Mathf.Pow(originPos.x - destPos.x, 2)) + (Mathf.Pow(originPos.z - destPos.z, 2)));
        //    float angleRad = Mathf.Asin(Mathf.Abs(originPos.x - destPos.x) / hyp);
        //    float angleDeg = Mathf.Rad2Deg * angleRad;
        //    return angleDeg / 90;
        //}

        public override void WarmupComplete()
        {
            ShotsSoFar = 0;
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            TryCastNextBurstShot();
        }
    }
}