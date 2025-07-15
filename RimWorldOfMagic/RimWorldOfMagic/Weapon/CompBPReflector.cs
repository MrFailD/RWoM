using HarmonyLib;
using Verse;
using UnityEngine;

namespace TorannMagic.Weapon
{
    public class CompBPReflector : CompDeflector.CompDeflector
    {
        public override Verb ReflectionHandler(Verb newVerb)
        {
            CompAbilityUserMagic holder = GetPawn.GetCompAbilityUserMagic();
            bool canReflect = Props.canReflect && holder.IsMagicUser;
            Verb result;
            if (canReflect)
            {
                lastAccuracyRoll = ReflectionAccuracy();
                VerbProperties verbProperties = new VerbProperties
                {
                    hasStandardCommand = newVerb.verbProps.hasStandardCommand,
                    defaultProjectile = newVerb.verbProps.defaultProjectile,
                    range = newVerb.verbProps.range,
                    muzzleFlashScale = newVerb.verbProps.muzzleFlashScale,
                    warmupTime = 0f,
                    defaultCooldownTime = 0f,
                    soundCast = Props.deflectSound
                };
                switch (lastAccuracyRoll)
                {
                    case AccuracyRoll.CritialFailure:
                        {
                            verbProperties.accuracyLong = 999f;
                            verbProperties.accuracyMedium = 999f;
                            verbProperties.accuracyShort = 999f;
                            lastShotReflected = true;
                            break;
                        }
                    case AccuracyRoll.Failure:
                        verbProperties.accuracyLong = 0f;
                        verbProperties.accuracyMedium = 0f;
                        verbProperties.accuracyShort = 0f;
                        lastShotReflected = false;
                        break;
                    case AccuracyRoll.Success:
                        verbProperties.accuracyLong = 999f;
                        verbProperties.accuracyMedium = 999f;
                        verbProperties.accuracyShort = 999f;
                        lastShotReflected = true;
                        break;
                    case AccuracyRoll.CriticalSuccess:
                        {
                            verbProperties.accuracyLong = 999f;
                            verbProperties.accuracyMedium = 999f;
                            verbProperties.accuracyShort = 999f;
                            lastShotReflected = true;
                            break;
                        }
                }
                newVerb.verbProps = verbProperties;
                result = newVerb;
            }
            else
            {
                result = newVerb;
            }
            return result;
            //return base.ReflectionHandler(newVerb);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            bool flag = dinfo.Weapon != null;
            if (flag)
            {
                bool flag2 = !dinfo.Weapon.IsMeleeWeapon && dinfo.WeaponBodyPartGroup == null && GetPawn != null;
                if (flag2)
                {
                    bool hasCompActivatableEffect = HasCompActivatableEffect;
                    if (hasCompActivatableEffect)
                    {
                        bool? flag3 = new bool?((bool)AccessTools.Method(GetActivatableEffect.GetType(), "IsActive", null, null).Invoke(GetActivatableEffect, null));
                        bool flag4 = flag3 == false;
                        if (flag4)
                        {
                            absorbed = false;
                            return;
                        }
                    }
                    float deflectionChance = DeflectionChance;
                    float meleeSkill = GetPawn.skills != null ? GetPawn.skills.GetSkill(Props.deflectSkill).Level : 0;
                    CompAbilityUserMagic holder = GetPawn.GetCompAbilityUserMagic();
                    deflectionChance += (meleeSkill * Props.deflectRatePerSkillPoint);
                    if (holder != null && !holder.IsMagicUser && (parent.def.defName == "TM_DefenderStaff" || parent.def.defName == "TM_BlazingPowerStaff"))
                    {
                        deflectionChance = 0;
                    }
                    int num = (int)(deflectionChance * 100f);
                    bool flag5 = Rand.Range(1, 100) > num;
                    if (flag5)
                    {
                        absorbed = false;
                        lastShotReflected = false;
                        return;
                    }
                    //splicing in TM handling of reflection
                    Thing instigator = dinfo.Instigator;
                    if (instigator != null && instigator.Map != null && GetPawn.Map != null)
                    {
                        Vector3 drawPos = GetPawn.DrawPos;
                        drawPos.x += ((instigator.DrawPos.x - drawPos.x) / 20f) + Rand.Range(-.2f, .2f);
                        drawPos.z += ((instigator.DrawPos.z - drawPos.z) / 20f) + Rand.Range(-.2f, .2f);
                        TM_MoteMaker.ThrowSparkFlashMote(drawPos, GetPawn.Map, 2f);
                        Thing thing = new Thing();
                        thing.def = dinfo.Weapon;
                        if (instigator is Pawn shooterPawn)
                        {
                            if (!dinfo.Weapon.IsMeleeWeapon && dinfo.WeaponBodyPartGroup == null)
                            {
                                TM_CopyAndLaunchProjectile.CopyAndLaunchThing(shooterPawn.equipment.PrimaryEq.PrimaryVerb.verbProps.defaultProjectile, GetPawn, instigator, shooterPawn, ProjectileHitFlags.IntendedTarget, null);
                            }
                        }
                    }
                    //no longer using comp deflector handling
                    //this.ResolveDeflectVerb();
                    //this.GiveDeflectJob(dinfo);
                    dinfo.SetAmount(0);
                    absorbed = true; // true; 
                    return;
                }
            }
            absorbed = false;
        }
    }
}
