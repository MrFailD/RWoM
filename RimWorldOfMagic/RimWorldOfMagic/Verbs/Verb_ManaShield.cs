using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Collections.Generic;

namespace TorannMagic
{
    internal class Verb_ManaShield : Verb_UseAbility  
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
            bool result = false;
            Pawn pawn = CasterPawn;
            Map map = CasterPawn.Map;

            if (pawn != null && !pawn.Downed)
            {
                if(pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_ManaShieldHD, false))
                {
                    Hediff hd = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_ManaShieldHD);
                    pawn.health.RemoveHediff(hd);

                    TM_MoteMaker.ThrowManaPuff(pawn.DrawPos, pawn.Map, .75f);
                    TM_MoteMaker.ThrowManaPuff(pawn.DrawPos, pawn.Map, .75f);
                    TM_MoteMaker.ThrowSiphonMote(pawn.DrawPos, pawn.Map, 1f);
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_ManaShieldHD, 1f);
                    TM_MoteMaker.ThrowManaPuff(pawn.DrawPos, pawn.Map, .75f);
                    TM_MoteMaker.ThrowManaPuff(pawn.DrawPos, pawn.Map, 1);
                    TM_MoteMaker.ThrowManaPuff(pawn.DrawPos, pawn.Map, .75f);
                }
            }
            else
            {
                Log.Warning("failed to TryCastShot");
            }

            burstShotsLeft = 0;

            return result;
        }
    }
}
