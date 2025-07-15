using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using AbilityUser;
using UnityEngine;
using Verse.AI.Group;

namespace TorannMagic
{
    public class Verb_ShieldOther : Verb_UseAbility  
    {
        private int pwrVal;
        private int verVal;
        private CompAbilityUserMagic comp;

        private bool validTarg;
        //Used for non-unique abilities that can be used with shieldbelt
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
            Pawn caster = base.CasterPawn;
            Pawn pawn = currentTarget.Thing as Pawn;

            comp = caster.GetCompAbilityUserMagic();


            if (pawn != null)
            {
                ApplyShield(pawn);                
            }            
            return true;
        }

        public void ApplyShield(Pawn pawn)
        {
            ApplyHediffs(pawn);
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.5f);
            TM_Action.DisplayShield(pawn, 5f);
        }

        private void ApplyHediffs(Pawn target)
        {
            HealthUtility.AdjustSeverity(target, TorannMagicDefOf.TM_MagicShieldHD, ((.32f + (.002f * comp.MagicUserLevel)) * comp.arcaneDmg));
        }
    }
}
