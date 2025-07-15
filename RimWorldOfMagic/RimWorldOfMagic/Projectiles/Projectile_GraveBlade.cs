using Verse;
using Verse.AI;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using RimWorld;

namespace TorannMagic
{
    public class Projectile_GraveBlade : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 65;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 15;
        private int effectIndex1;
        private int effectIndex2;
        private float radius = 3;
        private bool initialized;
        private List<IntVec3> ringCellList;
        private List<IntVec3> innerCellList;
        private Pawn caster;

        private int effectDelay = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 65, false);
            Scribe_Values.Look<int>(ref strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<int>(ref effectIndex1, "effectIndex1", 0, false);
            Scribe_Values.Look<int>(ref effectIndex2, "effectIndex2", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
            Scribe_Collections.Look<IntVec3>(ref innerCellList, "innerCellList", LookMode.Value);
            Scribe_Collections.Look<IntVec3>(ref ringCellList, "ringCellList", LookMode.Value);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {            
            base.Impact(hitThing);
           
            ThingDef def = this.def;          

            if (!initialized)
            {
                caster = launcher as Pawn;               
                CompAbilityUserMight comp = caster.GetCompAbilityUserMight();
                //pwrVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_GraveBlade.FirstOrDefault((MightPowerSkill x) => x.label == "TM_GraveBlade_pwr").level;
                //verVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_GraveBlade.FirstOrDefault((MightPowerSkill x) => x.label == "TM_GraveBlade_ver").level;
                //verVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_GraveBlade, "TM_GraveBlade", "_ver", true);
                //pwrVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_GraveBlade, "TM_GraveBlade", "_pwr", true);
                //
                arcaneDmg = comp.mightPwr;
                //if (ModOptions.Settings.Instance.AIHardMode && !caster.IsColonist)
                //{
                //    pwrVal = 3;
                //    verVal = 3;
                //}
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_GraveBlade);
                pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_GraveBlade);
                radius = this.def.projectile.explosionRadius;
                duration = 10 + (int)(radius * 20);
                innerCellList = GenRadial.RadialCellsAround(Position, radius, true).ToList();
                ringCellList = GenRadial.RadialCellsAround(Position, radius+1, false).Except(innerCellList).ToList();
                effectIndex2 = ringCellList.Count / 2;
                initialized = true;
            }

            if (Map != null)
            {
                if (Find.TickManager.TicksGame % effectDelay == 0)
                {
                    Vector3 drawIndex1 = ringCellList[effectIndex1].ToVector3Shifted();
                    drawIndex1.x += Rand.Range(-.35f, .35f);
                    drawIndex1.z += Rand.Range(-.35f, .35f);
                    Vector3 drawIndex2 = ringCellList[effectIndex2].ToVector3Shifted();
                    drawIndex2.x += Rand.Range(-.35f, .35f);
                    drawIndex2.z += Rand.Range(-.35f, .35f);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_SpiritFlame, drawIndex1 , Map, Rand.Range(.4f, .8f), .1f, 0, .6f, 0, Rand.Range(.4f, 1f), Rand.Range(-20, 20), Rand.Range(-20, 20));
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_SpiritFlame, drawIndex2, Map, Rand.Range(.4f, .8f), .1f, 0, .6f, 0, Rand.Range(.4f, 1f), Rand.Range(-20, 20), Rand.Range(-20, 20));
                    effectIndex1++;
                    effectIndex2++;
                    if (effectIndex1 >= ringCellList.Count)
                    {
                        effectIndex1 = 0;
                    }
                    if (effectIndex2 >= ringCellList.Count)
                    {
                        effectIndex2 = 0;
                    }                    
                }
                if (Find.TickManager.TicksGame % strikeDelay == 0 && !caster.DestroyedOrNull())
                {
                    IntVec3 centerCell = innerCellList.RandomElement();
                    List<IntVec3> targetCells = GenRadial.RadialCellsAround(centerCell, 2f, true).ToList();
                    for (int i = 0; i < targetCells.Count; i++)
                    {
                        IntVec3 curCell = targetCells[i];
                        Pawn victim = curCell.GetFirstPawn(Map);
                        if (victim != null && !victim.Destroyed && !victim.Dead && victim != caster)
                        {
                            TM_Action.DamageEntities(victim, null, (Rand.Range(10, 16) + (2 * pwrVal)) * arcaneDmg, TMDamageDefOf.DamageDefOf.TM_Spirit, launcher);
                            if (!caster.DestroyedOrNull() && !caster.Dead && Rand.Chance(verVal))
                            {
                                TM_Action.DoAction_HealPawn(caster, caster, 1, (Rand.Range(6, 10) + (2 * verVal)) * arcaneDmg);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_SpiritRetaliation, caster.DrawPos, caster.Map, Rand.Range(1f, 1.2f), Rand.Range(.1f, .15f), 0, Rand.Range(.1f, .2f), -600, 0, 0, Rand.Range(0, 360));
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_SpiritRetaliation, caster.DrawPos, caster.Map, Rand.Range(1f, 1.2f), Rand.Range(.1f, .15f), 0, Rand.Range(.1f, .2f), 600, 0, 0, Rand.Range(0, 360));
                            }
                            if (Rand.Chance(verVal))
                            {
                                if (!victim.IsWildMan() && victim.RaceProps.Humanlike && victim.mindState != null && !victim.InMentalState)
                                {
                                    try
                                    {
                                        Job job = new Job(JobDefOf.FleeAndCower);
                                        //victim.mindState.mentalStateHandler.TryStartMentalState(TorannMagicDefOf.TM_PanicFlee);
                                    }
                                    catch (NullReferenceException ex)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_GraveBlade, centerCell.ToVector3Shifted(), Map, Rand.Range(1f, 1.6f), .15f, .1f, .2f, 0, Rand.Range(4f, 6f), 0, 0);

                }
            }
        }        
    }    
}