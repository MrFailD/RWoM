using Verse;
using AbilityUser;
using UnityEngine;
using RimWorld;
using System.Linq;
using System.Collections.Generic;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Verb_BladeSpin : Verb_UseAbility
    {
        private bool flag10;
        private int verVal;
        private int pwrVal;

        private static readonly Color bladeColor = new Color(160f, 160f, 160f);
        private static readonly Material bladeMat = MaterialPool.MatFrom("Spells/cleave", false);

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

        public static int GetWeaponDmg(Pawn pawn)
        {
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            MightPowerSkill str = comp.MightData.MightPowerSkill_global_strength.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_strength_pwr");
            int dmgNum = 0;
            ThingWithComps weaponComp = pawn.equipment.Primary;
            float weaponDPS = weaponComp.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, false) * .7f;
            float dmgMultiplier = weaponComp.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, false);
            float pawnDPS = pawn.GetStatValue(StatDefOf.MeleeDPS, false);
            float skillMultiplier = (.6f ) * comp.mightPwr;
            return dmgNum = Mathf.RoundToInt(skillMultiplier * dmgMultiplier * (pawnDPS + weaponDPS));
        }

        protected override bool TryCastShot()
        {            
            if (CasterPawn.equipment.Primary != null && !CasterPawn.equipment.Primary.def.IsRangedWeapon)
            {
                CompAbilityUserMight comp = CasterPawn.GetCompAbilityUserMight();
                //MightPowerSkill ver = comp.MightData.MightPowerSkill_SeismicSlash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_SeismicSlash_ver");
                //MightPowerSkill pwr = comp.MightData.MightPowerSkill_SeismicSlash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_SeismicSlash_pwr");
                //verVal = TM_Calc.GetMightSkillLevel(this.CasterPawn, comp.MightData.MightPowerSkill_BladeSpin, "TM_BladeSpin", "_ver", true);
                //pwrVal = TM_Calc.GetMightSkillLevel(this.CasterPawn, comp.MightData.MightPowerSkill_BladeSpin, "TM_BladeSpin", "_pwr", true);
                verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, Ability.Def as TMAbilityDef);
                pwrVal = TM_Calc.GetSkillPowerLevel(CasterPawn, Ability.Def as TMAbilityDef);
                //verVal = ver.level;
                //pwrVal = pwr.level;
                //if (base.CasterPawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                //{
                //    MightPowerSkill mver = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                //    MightPowerSkill mpwr = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                //    verVal = mver.level;
                //    pwrVal = mpwr.level;
                //}
                CellRect cellRect = CellRect.CenteredOn(base.CasterPawn.Position, 1);
                Map map = base.CasterPawn.Map;
                cellRect.ClipInsideMap(map);

                IntVec3 centerCell = cellRect.CenterCell;                
                TMAbilityDef ad = (TMAbilityDef)Ability.Def;
                int dmgNum = Mathf.RoundToInt(comp.weaponDamage * ad.weaponDamageFactor);
                
                if (!CasterPawn.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                {
                    dmgNum += 10;
                }

                SearchForTargets(base.CasterPawn.Position, (2f + (float)(.5f * verVal)), map);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BladeSweep, CasterPawn.DrawPos, CasterPawn.Map, 1.6f + .4f * verVal, .04f, 0f, .18f, 1000, 0, 0, Rand.Range(0, 360));
            }
            else
            {
                Messages.Message("MustHaveMeleeWeapon".Translate(
                    CasterPawn.LabelCap
                ), MessageTypeDefOf.RejectInput);
                return false;
            }

            burstShotsLeft = 0;
            PostCastShot(flag10, out flag10);
            return flag10;

        }

        public void SearchForTargets(IntVec3 center, float radius, Map map)
        {
            Pawn victim = null;
            IntVec3 curCell;            
            DrawBlade(base.CasterPawn.Position.ToVector3Shifted(), 4f + (float)(verVal));
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            for (int i = 0; i < targets.Count(); i++)
            {
                curCell = targets.ToArray<IntVec3>()[i];
                if (curCell.InBoundsWithNullCheck(base.CasterPawn.Map) && curCell.IsValid)
                {
                    victim = curCell.GetFirstPawn(map);
                }

                if (victim != null && victim != base.CasterPawn && victim.Faction != base.CasterPawn.Faction)
                {
                    for (int j = 0; j < 2+pwrVal; j++)
                    {
                        bool newTarg = false;
                        if (Rand.Chance(.5f + .04f*(pwrVal+verVal)))
                        {
                            newTarg = true;
                        }
                        if (newTarg)
                        {
                            DrawStrike(center, victim.Position.ToVector3(), map);
                            damageEntities(victim, null, GetWeaponDmg(CasterPawn), DamageDefOf.Cut);
                        }
                    }                    
                }
                targets.GetEnumerator().MoveNext();
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
            amt = (int)((float)amt * Rand.Range(.7f, 1.3f));
            dinfo = new DamageInfo(type, amt, 0, (float)-1, CasterPawn, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);            
            dinfo.SetAllowDamagePropagation(false);
            victim.TakeDamage(dinfo);
        }
    }
}
