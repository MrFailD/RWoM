using Verse;
using AbilityUser;
using System.Collections.Generic;
using RimWorld;

namespace TorannMagic
{    
    public class Effect_SpiritStorm : Verb_UseAbility
    {
        private bool validTarg;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    ShootLine shootLine;
                    validTarg = TryFindShootLineFromTo(root, targ, out shootLine);
                }
                else
                {
                    //out of range
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            TargetAoEProperties targetAoEProperties = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties;
            if (targetAoEProperties == null || !targetAoEProperties.showRangeOnSelect)
            {
                CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
                float adjustedRadius = verbProps.defaultProjectile?.projectile?.explosionRadius ?? 1f;
                if (comp != null && comp.MagicData != null)
                {
                    int verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, Ability.Def as TMAbilityDef);
                    adjustedRadius += verVal;
                }
                return adjustedRadius;
            }
            return (float)targetAoEProperties.range;        
        }

        public virtual void Effect()
        {
            LocalTargetInfo t = TargetsAoE[0];
            bool flag = t.Cell != default(IntVec3);
            if (flag)
            {                
                base.CasterPawn.rotationTracker.Face(t.CenterVector3);
                Thing ss = new Thing();
                ss.def = TorannMagicDefOf.FlyingObject_SpiritStorm;                
                FlyingObject_SpiritStorm flyingObject = (FlyingObject_SpiritStorm)GenSpawn.Spawn(ThingDef.Named("FlyingObject_SpiritStorm"), t.Cell, CasterPawn.Map);
                flyingObject.Launch(CasterPawn, t.Cell, ss);
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult)
            {
                Effect();
                outResult = true;
            }
            outResult = inResult;
        }
    }    
}
