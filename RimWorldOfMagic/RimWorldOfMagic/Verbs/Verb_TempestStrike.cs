using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System;
using RimWorld;
using Verse.Sound;

namespace TorannMagic
{
    internal class Verb_TempestStrike : Verb_UseAbility  
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
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            if (comp != null && comp.Stamina != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float direction = Rand.Range(0, 360);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi_Yellow, pawn.DrawPos, map, Rand.Range(.1f, .3f), 0.2f, .02f, .1f, 0, Rand.Range(6, 8), direction, direction);
                }
                if (pawn.equipment != null && pawn.equipment.Primary != null)
                {
                    Thing wpn = pawn.equipment.Primary;
                    if (wpn.def.IsRangedWeapon)
                    {
                        if (wpn.def.Verbs.FirstOrDefault() != null && wpn.def.Verbs.FirstOrDefault().verbClass.ToString() != "Verb_ShootOneUse")
                        {
                            ThingDef newProjectile = wpn.def.Verbs.FirstOrDefault().defaultProjectile;
                            int shots = Mathf.Clamp(wpn.def.Verbs.FirstOrDefault().burstShotCount, 1, 5);
                            Type oldThingclass = newProjectile.thingClass;
                            newProjectile.thingClass = Projectile.thingClass;
                            bool flag = false;
                            SoundInfo info = SoundInfo.InMap(new TargetInfo(CasterPawn.Position, CasterPawn.Map, false), MaintenanceType.None);
                            SoundDef.Named(wpn.def.Verbs.FirstOrDefault().soundCast.ToString()).PlayOneShot(info);
                            bool? flag4 = TryLaunchProjectile(newProjectile, ShotTarget(pawn));                            
                            for (int i = 1; i < shots; i++)
                            {
                                TryLaunchProjectile(newProjectile, ShotTarget(pawn));
                            }
                            flag = flag4.HasValue;
                            PostCastShot(flag, out flag);
                            if (!flag)
                            {
                                Ability.Notify_AbilityFailed(UseAbilityProps.refundsPointsAfterFailing);
                            }
                            newProjectile.thingClass = oldThingclass;
                        }
                        else
                        {
                            Messages.Message("TM_IncompatibleWeapon".Translate(
                            ), MessageTypeDefOf.RejectInput);
                            return false;
                        }
                    }
                    else
                    {
                        TryLaunchProjectile(Projectile, currentTarget);
                    }
                }
                else
                {
                    TryLaunchProjectile(Projectile, currentTarget);
                }
            }
            else
            {
                MoteMaker.ThrowText(pawn.DrawPos, map, "Failed", -1);
                return false;
            }

            float spCost = (comp.ActualStaminaCost(TorannMagicDefOf.TM_TempestStrike) / 1.5f);
            if (comp.Stamina.CurLevel >= spCost)
            {
                if (pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Wayfarer))
                {
                    if (comp.MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_ver").level >= 15)
                    {
                        spCost *= .75f;
                    }
                }
                comp.Stamina.UseMightPower(spCost);
                comp.MightUserXP += (int)((spCost * 180) * comp.xpGain);
                result = true;
            }
      
            return result;
        }

        private LocalTargetInfo ShotTarget(Pawn pawn)
        {
           
            LocalTargetInfo currentTarget = this.currentTarget;
            if (!Rand.Chance(HitChance(pawn)))
            {
                IntVec3 targetVariation = this.currentTarget.Cell;
                targetVariation.x += Mathf.RoundToInt(Rand.Range(-.1f, .1f) * Vector3.Distance(pawn.DrawPos, this.currentTarget.CenterVector3) + Rand.Range(-1f, 1f));
                targetVariation.z += Mathf.RoundToInt(Rand.Range(-.1f, .1f) * Vector3.Distance(pawn.DrawPos, this.currentTarget.CenterVector3) + Rand.Range(-1f, 1f));
                currentTarget = targetVariation;
            }
            return currentTarget;
        }

        public static float HitChance(Pawn pawn)
        {
            Thing wpn = null;
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                wpn = pawn.equipment.Primary;
            }
            float chance = .95f;
            if (wpn.def.IsRangedWeapon)
            {
                chance = pawn.equipment.Primary.GetStatValue(StatDefOf.AccuracyLong, true);
            }
            else
            {
                chance = pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeHitChance, true);
            }
            return chance;
        }
    }
}


