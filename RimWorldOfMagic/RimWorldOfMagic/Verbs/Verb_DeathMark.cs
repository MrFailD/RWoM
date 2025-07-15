using RimWorld;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class Verb_DeathMark : Verb_UseAbility  
    {

        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;

        private bool validTarg;

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

        protected override bool TryCastShot()
        {
            bool result = false;
            Pawn p = CasterPawn;
            CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
            verVal = TM_Calc.GetSkillVersatilityLevel(p, Ability.Def as TMAbilityDef);
            pwrVal = TM_Calc.GetSkillPowerLevel(p, Ability.Def as TMAbilityDef);
            //MagicPowerSkill pwr = comp.MagicData.MagicPowerSkill_DeathMark.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_DeathMark_pwr");
            //MagicPowerSkill ver = comp.MagicData.MagicPowerSkill_DeathMark.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_DeathMark_ver");
            //verVal = ver.level;
            //pwrVal = pwr.level;
            //this.arcaneDmg = comp.arcaneDmg;
            //if (p.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            //{
            //    MightPowerSkill mpwr = p.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
            //    MightPowerSkill mver = p.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
            //    pwrVal = mpwr.level;
            //    verVal = mver.level;
            //}

            if (currentTarget != null && base.CasterPawn != null)
            {
                
                Map map = CasterPawn.Map;
                TargetsAoE.Clear();
                //this.UpdateTargets();
                FindTargets();
                
                for(int i =0; i < TargetsAoE.Count; i++)
                {
                    if (TargetsAoE[i].Thing is Pawn)
                    {
                        Pawn victim = TargetsAoE[i].Thing as Pawn;
                        if(!victim.RaceProps.IsMechanoid)
                        {
                            if (Rand.Chance(TM_Calc.GetSpellSuccessChance(CasterPawn, victim, true)))
                            {
                                HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_DeathMarkCurse"), (Rand.Range(1f + pwrVal, 4 + 2 * pwrVal) * arcaneDmg));
                                TM_MoteMaker.ThrowSiphonMote(victim.DrawPos, victim.Map, 1.4f);
                                if (comp.Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_LichHD"), false))
                                {
                                    comp.PowerModifier += 1;
                                }

                                if (Rand.Chance(verVal * .2f))
                                {
                                    if (Rand.Chance(verVal * .1f)) //terror
                                    {
                                        HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_Terror"), Rand.Range(3f * verVal, 5f * verVal) * arcaneDmg);
                                        TM_MoteMaker.ThrowDiseaseMote(victim.DrawPos, victim.Map, 1f, .5f, .2f, .4f);
                                        MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Terror", -1);
                                    }
                                    if (Rand.Chance(verVal * .1f)) //berserk
                                    {
                                        if (victim.mindState != null && victim.RaceProps != null && victim.RaceProps.Humanlike)
                                        {
                                            victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "cursed", true, false, false, null);
                                            FleckMaker.ThrowMicroSparks(victim.DrawPos, victim.Map);
                                            MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Berserk", -1);
                                        }

                                    }
                                }
                                if (victim.IsColonist && !base.CasterPawn.IsColonist)
                                {
                                    TM_Action.SpellAffectedPlayerWarning(victim);
                                }
                            }
                            else
                            {
                                MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                            }
                        }
                    }
                }

                result = true;
            }

            burstShotsLeft = 0;
            //this.ability.TicksUntilCasting = (int)base.UseAbilityProps.SecondsToRecharge * 60;
            return result;
        }


        private void FindTargets()
        {
            bool flag = UseAbilityProps.AbilityTargetCategory == AbilityTargetCategory.TargetAoE;
            if (flag)
            {
                bool flag2 = UseAbilityProps.TargetAoEProperties == null;
                if (flag2)
                {
                    Log.Error("Tried to Cast AoE-Ability without defining a target class");
                }
                List<Thing> list = new List<Thing>();
                IntVec3 aoeStartPosition = caster.PositionHeld;
                bool flag3 = !UseAbilityProps.TargetAoEProperties.startsFromCaster;
                if (flag3)
                {
                    aoeStartPosition = currentTarget.Cell;
                }
                bool flag4 = !UseAbilityProps.TargetAoEProperties.friendlyFire;
                if (flag4)
                {
                    list = (from x in caster.Map.listerThings.AllThings
                            where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) && x.Faction != Faction.OfPlayer
                            select x).ToList<Thing>();
                }
                else
                {
                    bool flag5 = UseAbilityProps.TargetAoEProperties.targetClass == typeof(Plant) || UseAbilityProps.TargetAoEProperties.targetClass == typeof(Building);
                    if (flag5)
                    {
                        list = (from x in caster.Map.listerThings.AllThings
                                where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType())
                                select x).ToList<Thing>();
                        foreach (Thing current in list)
                        {
                            LocalTargetInfo item = new LocalTargetInfo(current);
                            TargetsAoE.Add(item);
                        }
                        return;
                    }
                    list.Clear();
                    list = (from x in caster.Map.listerThings.AllThings
                            where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) && (x.HostileTo(Faction.OfPlayer) || UseAbilityProps.TargetAoEProperties.friendlyFire)
                            select x).ToList<Thing>();
                }
                int maxTargets = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties.maxTargets;
                List<Thing> list2 = new List<Thing>(list.InRandomOrder(null));
                int num = 0;
                while (num < maxTargets && num < list2.Count<Thing>())
                {
                    TargetInfo targ = new TargetInfo(list2[num]);
                    bool flag6 = UseAbilityProps.targetParams.CanTarget(targ);
                    if (flag6)
                    {
                        TargetsAoE.Add(new LocalTargetInfo(list2[num]));
                    }
                    num++;
                }
            }
            else
            {
                TargetsAoE.Clear();
                TargetsAoE.Add(currentTarget);
            }
        }
    }
}
