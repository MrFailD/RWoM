using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using AbilityUser;


namespace TorannMagic
{
    public class Verb_SniperShot : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            if (TM_Calc.IsUsingRanged(CasterPawn))
            {
                Thing wpn = CasterPawn.equipment.Primary;
                ThingDef newProjectile = wpn.def.Verbs.FirstOrDefault().defaultProjectile;
                Type oldThingclass = newProjectile.thingClass;
                newProjectile.thingClass = Projectile.thingClass;
                bool flag = false;
                SoundInfo info = SoundInfo.InMap(new TargetInfo(CasterPawn.Position, CasterPawn.Map, false), MaintenanceType.None);
                SoundDef.Named(wpn.def.Verbs.FirstOrDefault().soundCast.ToString()).PlayOneShot(info);
                bool? flag4 = TryLaunchProjectile(newProjectile, currentTarget);
                bool hasValue = flag4.HasValue;
                if (hasValue)
                {
                    bool flag5 = flag4 == true;
                    if (flag5)
                    {
                        flag = true;
                    }
                    bool flag6 = flag4 == false;
                    if (flag6)
                    {
                        flag = false;
                    }
                }
                PostCastShot(flag, out flag);
                bool flag7 = !flag;
                if (flag7)
                {
                    Ability.Notify_AbilityFailed(UseAbilityProps.refundsPointsAfterFailing);
                }
                newProjectile.thingClass = oldThingclass;
                burstShotsLeft = 0;
                return flag;  
            }
            else
            {
                Messages.Message("MustHaveRangedWeapon".Translate(
                    CasterPawn.LabelCap
                ), MessageTypeDefOf.RejectInput);
                return false;
            }
        }
    }
}
