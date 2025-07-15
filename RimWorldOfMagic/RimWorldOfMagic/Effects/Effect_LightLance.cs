using Verse;
using AbilityUser;
using System.Collections.Generic;
using RimWorld;
using System.Linq;

namespace TorannMagic
{    
    public class Effect_LightLance : Verb_UseAbility
    {
        private bool validTarg;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    if (CasterIsPawn && CasterPawn.apparel != null)
                    {
                        List<Apparel> wornApparel = CasterPawn.apparel.WornApparel;
                        for (int i = 0; i < wornApparel.Count; i++)
                        {
                            if (!wornApparel[i].AllowVerbCast(this))
                            {
                                return false;
                            }
                        }
                        validTarg = true;
                    }
                    else
                    {
                        validTarg = true;
                    }                    
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

        protected override bool TryCastShot()
        {
            int shotCount = 3;
            if(!base.CasterPawn.DestroyedOrNull())
            {
                CompAbilityUserMagic comp = base.CasterPawn.GetCompAbilityUserMagic();
                if (comp != null)
                {
                    //shotCount -= TM_Calc.GetMagicSkillLevel(base.CasterPawn, comp.MagicData.MagicPowerSkill_LightLance, "TM_LightLance", "_pwr", true);
                    shotCount = TM_Calc.GetSkillPowerLevel(base.CasterPawn, Ability.Def as TMAbilityDef, true);
                }
                if (base.CasterPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_LightCapacitanceHD))
                {
                    HediffComp_LightCapacitance hd = base.CasterPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_LightCapacitanceHD).TryGetComp<HediffComp_LightCapacitance>();
                    hd.LightEnergy -= 3f;
                }
            }
            if (burstShotsLeft == verbProps.burstShotCount)
            {
                return base.TryCastShot();
            }
            else if (burstShotsLeft > (0 + shotCount))
            {
                bool outResult = true;
                PostCastShot(outResult, out outResult);
                if (!outResult)
                {
                    Ability.Notify_AbilityFailed(UseAbilityProps.refundsPointsAfterFailing);
                }
                return outResult;
            }
            else
            {
                return false;
            }
        }

        public virtual void Effect()
        {            
            LocalTargetInfo t = TargetsAoE[0];
            bool flag = t.Cell != default(IntVec3);
            if (flag)
            {
                Thing launchedThing = new Thing()
                {
                    def = TorannMagicDefOf.FlyingObject_LightLance
                };
                Pawn casterPawn = base.CasterPawn;
                FlyingObject_LightLance flyingObject = (FlyingObject_LightLance)GenSpawn.Spawn(ThingDef.Named("FlyingObject_LightLance"), CasterPawn.Position, CasterPawn.Map);
                flyingObject.Launch(CasterPawn, t.Cell, launchedThing);
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            outResult = inResult;
            Effect();            
        }

        
    }    
}
