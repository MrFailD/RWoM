using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class Verb_PhaseStrike : Verb_UseAbility  
    {
        private bool arg_41_0;
        private bool arg_42_0;
        private MightPowerSkill pwr;
        private MightPowerSkill ver;
        private MightPowerSkill str;

        private int verVal;
        private int pwrVal;
        private int dmgNum;

        private bool validTarg;
        private IntVec3 arg_29_0;

        private static readonly Color bladeColor = new Color(160f, 160f, 160f);
        private static readonly Material bladeMat = MaterialPool.MatFrom("Spells/cleave", false);

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    //out of range
                    validTarg = true;
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

        public static int GetWeaponDmg(Pawn pawn)
        {
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            MightPowerSkill str = comp.MightData.MightPowerSkill_global_strength.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_strength_pwr");
            ThingWithComps weaponComp = pawn.equipment.Primary;
            float weaponDPS = weaponComp.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, false) * .7f;
            float dmgMultiplier = weaponComp.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, false);
            float pawnDPS = pawn.GetStatValue(StatDefOf.MeleeDPS, false);
            float skillMultiplier = (.6f) * comp.mightPwr;
            return Mathf.RoundToInt(skillMultiplier * dmgMultiplier * (pawnDPS + weaponDPS));
        }

        protected override bool TryCastShot()
        {
            bool result = false;
            bool arg_40_0;

            CompAbilityUserMight comp = CasterPawn.GetCompAbilityUserMight();
            verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, Ability.Def as TMAbilityDef);
            pwrVal = TM_Calc.GetSkillPowerLevel(CasterPawn, Ability.Def as TMAbilityDef);
            //pwr = comp.MightData.MightPowerSkill_PhaseStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PhaseStrike_pwr");
            //ver = comp.MightData.MightPowerSkill_PhaseStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PhaseStrike_ver");
            //verVal = ver.level;
            //pwrVal = pwr.level;
            //if (base.CasterPawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            //{
            //    MightPowerSkill mver = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
            //    MightPowerSkill mpwr = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
            //    verVal = mver.level;
            //    pwrVal = mpwr.level;
            //}
            if (CasterPawn.equipment.Primary != null && !CasterPawn.equipment.Primary.def.IsRangedWeapon)
            {
                TMAbilityDef ad = (TMAbilityDef)Ability.Def;
                dmgNum = Mathf.RoundToInt(comp.weaponDamage * ad.weaponDamageFactor);
                
                if (!CasterPawn.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                {
                    dmgNum += 10;
                }

                if (currentTarget != null && base.CasterPawn != null)
                {
                    arg_29_0 = currentTarget.Cell;
                    Vector3 vector = currentTarget.CenterVector3;
                    arg_40_0 = currentTarget.Cell.IsValid;
                    arg_41_0 = vector.InBoundsWithNullCheck(base.CasterPawn.Map);
                    arg_42_0 = true; // vector.ToIntVec3().Standable(base.CasterPawn.Map);
                }
                else
                {
                    arg_40_0 = false;
                }
                bool flag = arg_40_0;
                bool flag2 = arg_41_0;
                bool flag3 = arg_42_0;
                if (flag)
                {
                    if (flag2 & flag3)
                    {
                        Pawn p = CasterPawn;
                        Map map = CasterPawn.Map;
                        IntVec3 pLoc = CasterPawn.Position;
                        bool drafted = p.Drafted;
                        if (p.IsColonist)
                        {
                            try
                            {
                                ThingSelectionUtility.SelectNextColonist();
                                CasterPawn.DeSpawn();
                                //p.SetPositionDirect(this.currentTarget.Cell);
                                SearchForTargets(arg_29_0, (2f + (float)(.5f * verVal)), map, p);
                                GenSpawn.Spawn(p, currentTarget.Cell, map);
                                DrawBlade(p.Position.ToVector3Shifted(), 4f + (float)(verVal));
                                p.drafter.Drafted = drafted;
                                if (ModOptions.Settings.Instance.cameraSnap)
                                {
                                    CameraJumper.TryJumpAndSelect(p);
                                }
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BladeSweep, CasterPawn.DrawPos, CasterPawn.Map, 1.4f + .4f * ver.level, .04f, 0f, .18f, 1000, 0, 0, Rand.Range(0, 360));
                            }
                            catch
                            {
                                if(!CasterPawn.Spawned)
                                {
                                    GenSpawn.Spawn(p, pLoc, map);
                                    p.drafter.Drafted = true;
                                    Log.Message("Phase strike threw exception - despawned pawn has been recovered at casting location");
                                }
                            }

                        }
                        else
                        {
                            CasterPawn.DeSpawn();
                            SearchForTargets(arg_29_0, (2f + (float)(.5f * verVal)), map, p);
                            GenSpawn.Spawn(p, currentTarget.Cell, map);
                            DrawBlade(p.Position.ToVector3Shifted(), 4f + (float)(verVal));
                        }
                    
                        //this.CasterPawn.SetPositionDirect(this.currentTarget.Cell);
                        //base.CasterPawn.SetPositionDirect(this.currentTarget.Cell);
                        //this.CasterPawn.pather.ResetToCurrentPosition();
                        result = true;
                    }
                    else
                    {
                    
                        Messages.Message("InvalidTargetLocation".Translate(), MessageTypeDefOf.RejectInput);
                    }
                }
            }
            else
            {
                Messages.Message("MustHaveMeleeWeapon".Translate(
                    CasterPawn.LabelCap
                ), MessageTypeDefOf.RejectInput);
                return false;
            }

            burstShotsLeft = 0;
            //this.ability.TicksUntilCasting = (int)base.UseAbilityProps.SecondsToRecharge * 60;
            return result;
        }

        public void SearchForTargets(IntVec3 center, float radius, Map map, Pawn pawn)
        {       
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            foreach (IntVec3 curCell in targets)
            {
                FleckMaker.ThrowDustPuff(curCell, map, .2f);
                if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                {
                    Pawn victim = curCell.GetFirstPawn(map);
                    if (victim != null && victim.Faction != pawn.Faction)
                    {
                        if (Rand.Chance(.1f + .15f * pwrVal))
                        {
                            dmgNum *= 3;
                            MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Critical Hit", -1f);
                        }
                        DrawStrike(center, victim.Position.ToVector3(), map);
                        damageEntities(victim, null, dmgNum, TMDamageDefOf.DamageDefOf.TM_PhaseStrike);                        
                    }
                }
            }
        }

        public void DrawStrike(IntVec3 center, Vector3 strikePos, Map map)
        {
            TM_MoteMaker.ThrowCrossStrike(strikePos, map, 1f);
            TM_MoteMaker.ThrowBloodSquirt(strikePos, map, 1.5f);
        }

        private void DrawBlade(Vector3 center, float magnitude)
        {

            Vector3 vector = center;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            Vector3 s = new Vector3(magnitude, magnitude, (1.5f * magnitude));
            Matrix4x4 matrix = default(Matrix4x4);

            for (int i = 0; i < 6; i++)
            {
                float angle = Rand.Range(0, 360);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, bladeMat, 0);
            }
        }

        public void damageEntities(Pawn victim, BodyPartRecord hitPart, int amt, DamageDef type)
        {
            DamageInfo dinfo;
            amt = (int)((float)amt * Rand.Range(.5f, 1.5f));
            dinfo = new DamageInfo(type, amt, 0, (float)-1, CasterPawn, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
            dinfo.SetAllowDamagePropagation(false);
            victim.TakeDamage(dinfo);
        }
    }
}
