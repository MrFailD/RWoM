using Verse;
using AbilityUser;
using System.Collections.Generic;
using RimWorld;

namespace TorannMagic
{    
    public class Effect_SpiritWolves : Verb_UseAbility
    {
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

        public virtual void Effect()
        {
            LocalTargetInfo t = TargetsAoE[0];
            bool flag = t.Cell != default(IntVec3);
            if (flag)
            {
                Thing launchedThing = new Thing()
                {
                    def = TorannMagicDefOf.FlyingObject_SpiritWolves
                };
                Pawn casterPawn = base.CasterPawn;
                FlyingObject_SpiritWolves flyingObject = (FlyingObject_SpiritWolves)GenSpawn.Spawn(ThingDef.Named("FlyingObject_SpiritWolves"), CasterPawn.Position, CasterPawn.Map);
                flyingObject.Launch(CasterPawn, t.Cell, launchedThing);
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
