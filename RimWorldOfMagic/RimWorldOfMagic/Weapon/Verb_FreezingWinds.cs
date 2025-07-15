using Verse;
using RimWorld;
using TorannMagic.Golems;
using TorannMagic.Weapon;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace TorannMagic.Weapon
{
    public class Verb_FreezingWinds : Verb_Shoot
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

        protected override bool TryCastShot()
        {
            CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
            if(comp != null && comp.IsMagicUser)
            {
                LocalTargetInfo t = CurrentTarget;
                bool flag = t.Cell != default(IntVec3);
                if (flag)
                {
                    Thing launchedThing = new Thing()
                    {
                        def = TorannMagicDefOf.FlyingObject_FreezingWinds
                    };
                    float distanceToTarget = (CasterPawn.Position - t.Cell).LengthHorizontal;
                    List<IntVec3> cellList = GenRadial.RadialCellsAround(t.Cell, distanceToTarget * .2f, true).ToList();
                    t = cellList.RandomElement();
                    //LongEventHandler.QueueLongEvent(delegate
                    //{
                    Vector3 launchDirection = TM_Calc.GetVector(CasterPawn.Position, t.Cell);
                    Vector3 launchPosition = CasterPawn.DrawPos + (launchDirection * 1.4f);
                    FlyingObject_FreezingWinds flyingObject = (FlyingObject_FreezingWinds)GenSpawn.Spawn(launchedThing.def, CasterPawn.Position, CasterPawn.Map);
                    flyingObject.AdvancedLaunch(CasterPawn, TorannMagicDefOf.Mote_WhiteIce, Rand.RangeInclusive(3,8), Rand.RangeInclusive(0,22), true, launchPosition, t, launchedThing, (int)flyingObject.def.projectile.speed, false, 0, 0, flyingObject.def.projectile.damageDef, null, 0, true, .5f);
                    //}, "LaunchingFlyer", false, null);                    
                }
                return true;
            }           
            else
            {
                if (Rand.Chance(.1f))
                {
                    MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, "Failed", -1);
                }
                return false;
            }
            
        }
    }
}
