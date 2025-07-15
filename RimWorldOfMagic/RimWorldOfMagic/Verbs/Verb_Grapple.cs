using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;

namespace TorannMagic
{
    public class Verb_Grapple : Verb_UseAbility_TrueBurst
    {
        private Vector3 pVect;

        private DamageInfo dinfo;

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

            bool flag10 = false;
            TargetsAoE.Clear();
            UpdateTargets();
            int shotsPerBurst = ShotsPerBurst;
            bool flag2 = UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1;
            if (flag2)
            {
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            for (int i = 0; i < TargetsAoE.Count; i++)
            {
                bool? flag3 = TryLaunchProjectile(verbProps.defaultProjectile, TargetsAoE[i]);
                bool hasValue = flag3.HasValue;
                if (hasValue)
                {
                    bool flag4 = flag3 == true;
                    if (flag4)
                    {
                        flag10 = true;
                    }
                    bool flag5 = flag3 == false;
                    if (flag5)
                    {
                        flag10 = false;
                    }
                }
            }

            CellRect cellRect = CellRect.CenteredOn(currentTarget.Cell, 1);
            Map map = caster.Map;
            cellRect.ClipInsideMap(map);

            IntVec3 centerCell = cellRect.CenterCell;
            Thing summonableThing = new Thing();
            FlyingObject summonablePawn = new FlyingObject();
            Pawn victim = null;
            //dinfo.SetAmount(10);            
            //dinfo.SetWeaponHediff(TorannMagicDefOf.TM_GrapplingHook);

            bool pflag = true;

            summonableThing = centerCell.GetFirstPawn(map);
            if (summonableThing == null)
            {
                pflag = false;
                //miss
            }
            else
            {
                pVect = summonableThing.TrueCenter();
                pVect.x = caster.TrueCenter().x;
                pVect.z = caster.TrueCenter().z;
                pVect.y = 0f;
                victim = summonableThing as Pawn;
                if (victim != null)
                {
                    if (!victim.IsColonist && !victim.IsPrisoner && !victim.Faction.HostileTo(CasterPawn.Faction) && victim.Faction != null && victim.RaceProps.Humanlike)
                    {
                        //Faction faction = victim.Faction;
                        //faction.TrySetRelationKind(this.CasterPawn.Faction, FactionRelationKind.Ally, false, null);
                        //FactionRelation fr = new FactionRelation();
                        //fr.baseGoodwill = -100;
                        //fr.kind = FactionRelationKind.Hostile;
                        //faction.SetRelation(fr);
                    }
                }                
                
            }
            bool result;
            bool arg_40_0;
            if (currentTarget != null && caster != null)
            {
                IntVec3 arg_29_0 = currentTarget.Cell;
                arg_40_0 = caster.Position.IsValid;
            }
            else
            {
                arg_40_0 = false;
            }
            bool flag = arg_40_0;
            if (flag)
            {
                if (summonableThing != null)
                {
                    if (pflag)
                    {
                        DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Stun, 10, 10, -1, CasterPawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, victim);
                        if (!victim.RaceProps.Humanlike || victim.Faction == CasterPawn.Faction)
                        {
                            if (ModCheck.Validate.GiddyUp.Core_IsInitialized())
                            {
                                ModCheck.GiddyUp.ForceDismount(victim);
                            }
                            victim.Position = base.Caster.Position;
                            victim.Notify_Teleported();
                            victim.TakeDamage(dinfo2);
                            //summonablePawn = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("TM_SummonedPawn"), summonableThing.Position, summonableThing.Map);
                            //summonablePawn.impactDamage = dinfo2;
                            //summonablePawn.Launch(base.caster, new LocalTargetInfo(pVect.ToIntVec3()), summonableThing);
                        }
                        else if (victim.RaceProps.Humanlike && victim.Faction != CasterPawn.Faction && Rand.Chance(TM_Calc.GetSpellSuccessChance(CasterPawn, victim, true)))
                        {
                            if(ModCheck.Validate.GiddyUp.Core_IsInitialized())
                            {
                                ModCheck.GiddyUp.ForceDismount(victim);
                            }
                            victim.Position = base.Caster.Position;
                            victim.Notify_Teleported();
                            victim.TakeDamage(dinfo2);
                            //summonablePawn = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("TM_SummonedPawn"), summonableThing.Position, summonableThing.Map);
                            //summonablePawn.impactDamage = dinfo2;
                            //summonablePawn.Launch(base.caster, new LocalTargetInfo(pVect.ToIntVec3()), summonableThing);
                        }
                        else
                        {
                            MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                        }
                    }
                    else
                    {
                        //miss
                    }
                    result = true;
                }
            }
            else
            {
                Log.Warning("failed to TryCastShot");
            }
            //this.burstShotsLeft = 0;
            //this.ability.TicksUntilCasting = (int)base.UseAbilityProps.SecondsToRecharge * 60;
            PostCastShot(flag10, out flag10);
            return flag;
        }

    }
}
