using Verse;
using AbilityUser;
using UnityEngine;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Verb_ThunderStrike : Verb_UseAbility
    {
        private bool flag10;
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

            if (CasterPawn.equipment.Primary == null)
            {
                base.TryCastShot();
                return true;
            }
            else
            {
                Messages.Message("MustBeUnarmed".Translate(
                    CasterPawn.LabelCap,
                    verbProps.label
                ), MessageTypeDefOf.RejectInput);
                return false;
            }

            burstShotsLeft = 0;
            PostCastShot(flag10, out flag10);
            return flag10;
        }        
    }
}
