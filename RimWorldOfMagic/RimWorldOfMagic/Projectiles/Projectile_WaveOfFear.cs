using Verse;
using RimWorld;
using AbilityUser;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;
using Verse.Sound;
using Verse.AI;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_WaveOfFear : Projectile_AbilityBase
    {
        private int pwrVal;
        private int verVal;
        private int effVal;
        private float arcaneDmg = 1;
        private Hediff hediff;
        private int age = -1;
        private int duration = 1800;
        private float radius = 4;
        private bool initialized;
        private int waveDelay = 10;
        private int waveRange = 1;
        private Pawn caster;
        private List<Pawn> affectedPawns;

        //if (!victim.IsWildMan() && victim.RaceProps.Humanlike && victim.mindState != null && !victim.InMentalState)
        //                        {
        //                            try
        //                            {
        //                                victim.mindState.mentalStateHandler.TryStartMentalState(TorannMagicDefOf.TM_PanicFlee);
        //                            }
        //                            catch (NullReferenceException ex)
        //                            {

        //                            }
        //                        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 40, false);
            Scribe_Values.Look<int>(ref waveRange, "waveRange", 1, false);
            Scribe_Values.Look<float>(ref radius, "radius", 4, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
            Scribe_Values.Look<int>(ref waveDelay, "waveDelay", 10, false);
        }

        private int TicksLeft
        {
            get
            {
                return duration - age;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            caster = launcher as Pawn;

            if(!initialized)
            {
                CompAbilityUserMight comp = caster.GetCompAbilityUserMight();
                //pwrVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_WaveOfFear.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WaveOfFear_pwr").level;
                //verVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_WaveOfFear.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WaveOfFear_ver").level;
                //verVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_WaveOfFear, "TM_WaveOfFear", "_ver", true);
                //pwrVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_WaveOfFear, "TM_WaveOfFear", "_pwr", true);
                //effVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_WaveOfFear.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WaveOfFear_eff").level;
                //if (caster.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                //{
                //    MightPowerSkill mpwr = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                //    MightPowerSkill mver = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                //    pwrVal = mpwr.level;
                //    verVal = mver.level;
                //}
                pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_WaveOfFear);
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_WaveOfFear);
                effVal = TM_Calc.GetSkillEfficiencyLevel(caster, TorannMagicDefOf.TM_WaveOfFear);
                arcaneDmg = comp.mightPwr;
                
                //if (!caster.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                //{
                //    pwrVal = 3;
                //    verVal = 3;
                //}                
                for (int h = 0; h < caster.health.hediffSet.hediffs.Count; h++)
                {
                    if (caster.health.hediffSet.hediffs[h].def.defName.Contains("TM_HateHD"))
                    {
                        hediff = caster.health.hediffSet.hediffs[h];
                    }
                }
                if (hediff != null)
                {
                    radius = 4 + (.8f * verVal) + (.07f * hediff.Severity);
                    HealthUtility.AdjustSeverity(caster, hediff.def, -(25f * (1 - .1f * effVal)));
                }
                else
                {
                    radius = 4 + (.8f * verVal);
                }
                duration = Mathf.RoundToInt(radius * 10);
                affectedPawns = new List<Pawn>();
                affectedPawns.Clear();
                if (Map != null)
                {
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                    TorannMagicDefOf.TM_GaspingAir.PlayOneShot(info);
                    Effecter FearWave = TorannMagicDefOf.TM_FearWave.Spawn();
                    FearWave.Trigger(new TargetInfo(caster.Position, caster.Map, false), new TargetInfo(Position, Map, false));
                    FearWave.Cleanup();
                    SearchAndFear();
                }
                initialized = true;
            }  

            if(Find.TickManager.TicksGame % waveDelay == 0 && !caster.DeadOrDowned && caster.Map != null)
            {
                SearchAndFear();
                waveRange++;
            }
        }

        public void SearchAndFear()
        {
            List<Pawn> mapPawns = caster.Map.mapPawns.AllPawnsSpawned.ToList();
            if (mapPawns != null && mapPawns.Count > 0)
            {
                for (int i = 0; i < mapPawns.Count; i++)
                {
                    Pawn victim = mapPawns[i];
                    if (!victim.DestroyedOrNull() && !victim.Dead && victim.Map != null && victim.health != null && victim.health.hediffSet != null && !victim.Downed && victim.mindState != null && !victim.InMentalState && !affectedPawns.Contains(victim))
                    {
                        if (victim.Faction != null && victim.Faction != caster.Faction && (victim.Position - caster.Position).LengthHorizontal < waveRange)
                        {
                            if (Rand.Chance(TM_Calc.GetSpellSuccessChance(caster, victim, true)))
                            {
                                LocalTargetInfo t = new LocalTargetInfo(victim.Position + (6 * arcaneDmg * TM_Calc.GetVector(caster.DrawPos, victim.DrawPos)).ToIntVec3());
                                if (victim.jobs != null)
                                {
                                    Job job = new Job(JobDefOf.FleeAndCower, t);
                                    victim.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }
                                HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_WaveOfFearHD"), .5f + pwrVal);
                                affectedPawns.Add(victim);
                            }
                            else
                            {
                                MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                            }
                        }
                        else if(victim.Faction == null && (victim.Position - caster.Position).LengthHorizontal < waveRange)
                        {
                            if (Rand.Chance(TM_Calc.GetSpellSuccessChance(caster, victim, true)))
                            {
                                LocalTargetInfo t = new LocalTargetInfo(victim.Position + (6 * arcaneDmg * TM_Calc.GetVector(caster.DrawPos, victim.DrawPos)).ToIntVec3());
                                if (victim.jobs != null)
                                {
                                    Job job = new Job(JobDefOf.FleeAndCower, t);
                                    victim.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }
                                HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_WaveOfFearHD"), .5f + pwrVal);
                                affectedPawns.Add(victim);
                            }
                            else
                            {
                                MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                            }
                        }
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age <= duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }
    }
}


