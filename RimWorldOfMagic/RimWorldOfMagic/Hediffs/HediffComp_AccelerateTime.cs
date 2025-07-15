using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;


namespace TorannMagic
{
    public class HediffComp_AccelerateTime : HediffComp
    {
        private bool initialized = true;

        public bool isBad;
        public int durationTicks = 6000;
        private int tickEffect = 300;
        private float maxAge = 100f;

        private int currentAge = 1;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref isBad, "isBad", false, false);
            Scribe_Values.Look<int>(ref durationTicks, "durationTicks", 6000, false);
            Scribe_Values.Look<int>(ref currentAge, "currentAge", 1, false);
            Scribe_Values.Look<int>(ref tickEffect, "tickEffect", 300, false);
        }

        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned && Pawn.Map != null)
            {
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
            }
            currentAge = Pawn.ageTracker.AgeBiologicalYears;
            tickEffect = Mathf.RoundToInt(durationTicks / 25);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
            }           

            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (Pawn.RaceProps != null && Pawn.RaceProps.lifeExpectancy != 0)
                {
                    maxAge = Pawn.RaceProps.lifeExpectancy;
                }
                int roundedYearAging = Mathf.RoundToInt(Pawn.ageTracker.AgeBiologicalYears / 100);
                if (isBad)
                {                    
                    if (Pawn.ageTracker.AgeBiologicalYears >= 100)
                    {
                        Pawn.ageTracker.AgeBiologicalTicks += roundedYearAging * 3600000;
                    }
                    else
                    {
                        Pawn.ageTracker.AgeBiologicalTicks = Mathf.RoundToInt(Pawn.ageTracker.AgeBiologicalTicks * (1.02f + (.002f * parent.Severity)));
                    }                    
                }
                else
                {
                    if(Pawn.ageTracker.AgeBiologicalYears >= 200)
                    {
                        Pawn.ageTracker.AgeBiologicalTicks += roundedYearAging * 3600000;
                    }
                    else
                    {
                        Pawn.ageTracker.AgeBiologicalTicks = Mathf.RoundToInt(Pawn.ageTracker.AgeBiologicalTicks * 1.00001f) + 2500;
                    }                    
                }
                if(Pawn.ageTracker.AgeBiologicalYears > currentAge)
                {
                    currentAge = Pawn.ageTracker.AgeBiologicalYears;                    
                    if (Rand.Chance(currentAge / maxAge))
                    {
                        BirthdayBiological(Pawn, currentAge);
                    }
                    if (isBad)
                    {
                        RaceAgainstTime(Pawn, currentAge);
                    }
                }

                AccelerateHediff(Pawn, 60);
                durationTicks -= 60;

                if(Find.TickManager.TicksGame % tickEffect ==0)
                {
                    AccelerateEffects(Pawn, 1);
                }
            }
        }

        private void BirthdayBiological(Pawn pawn, float age)
        {
            foreach (HediffGiver_Birthday item in AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn, Mathf.RoundToInt(age)))
            {
                if ((age > 150 && Rand.Chance(.01f * age)))
                {
                    bool hasAgelessGene = false;
                    if (pawn.genes != null)
                    {
                        
                        if (pawn.genes != null)
                        {
                            foreach (Gene g in Pawn.genes.GenesListForReading)
                            {
                                if (g.def.defName == "Ageless")
                                {
                                    hasAgelessGene = true;
                                }
                            }
                        }
                    }
                    if(!hasAgelessGene)
                    {
                        item.TryApply(pawn);
                    }
                }
            }
        }

        private void RaceAgainstTime(Pawn pawn, float age)
        {               
            if (Rand.Chance(age/maxAge))
            {
                bool hasAgelessGene = false;
                if (pawn.genes != null)
                {
                    foreach(Gene g in Pawn.genes.GenesListForReading)
                    {
                        if(g.def.defName == "Ageless")
                        {
                            hasAgelessGene = true;
                        }
                    }
                }
                if(!hasAgelessGene)
                { 
                    AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, false);
                }
            }
            if (Rand.Chance((age/maxAge)*.5f))
            {
                if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("HeartArteryBlockage")))
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDef.Named("HeartArteryBlockage"), Rand.Range(.095f, .195f));
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDef.Named("HeartArteryBlockage"), Rand.Range(.0095f * parent.Severity, .0195f * parent.Severity));
                }
            }
        }

        private void AccelerateHediff(Pawn pawn, int ticks)
        {
            float totalBleedRate = 0;
            using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Hediff rec = enumerator.Current;
                    HediffComp_Immunizable immuneComp = rec.TryGetComp<HediffComp_Immunizable>();
                    if(immuneComp != null)
                    {
                        if (immuneComp.Def.CompProps<HediffCompProperties_Immunizable>() != null)
                        {
                            float immuneSevDay = immuneComp.Def.CompProps<HediffCompProperties_Immunizable>().severityPerDayNotImmune;
                            if (immuneSevDay != 0 && !rec.FullyImmune())
                            {
                                rec.Severity += ((immuneSevDay * ticks * parent.Severity)/(24*2500));
                            }
                        }
                    }
                    HediffComp_SeverityPerDay sevDayComp = rec.TryGetComp<HediffComp_SeverityPerDay>();
                    if (sevDayComp != null)
                    {
                        if (sevDayComp.Def.CompProps<HediffCompProperties_SeverityPerDay>() != null)
                        {
                            float sevDay = sevDayComp.Def.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay;
                            if (sevDay != 0)
                            {
                                rec.Severity += ((sevDay * ticks * parent.Severity)/(24*2500));
                            }
                        }
                    }
                    HediffComp_Disappears tickComp = rec.TryGetComp<HediffComp_Disappears>();
                    if (tickComp != null)
                    {
                        int ticksToDisappear = Traverse.Create(root: tickComp).Field(name: "ticksToDisappear").GetValue<int>();
                        if (ticksToDisappear != 0)
                        {
                            Traverse.Create(root: tickComp).Field(name: "ticksToDisappear").SetValue(ticksToDisappear - (Mathf.RoundToInt(60 * parent.Severity)));                            
                        }
                    }
                    Hediff_Pregnant hdp = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Pregnant")) as Hediff_Pregnant;
                    if(hdp != null)
                    {
                        hdp.Severity += (1f / (Pawn.RaceProps.gestationPeriodDays * (2500f /parent.Severity)));
                    }
                    CompEggLayer eggComp = Pawn.TryGetComp<CompEggLayer>();
                    if (eggComp != null)
                    {
                        float eggProgress = Traverse.Create(root: eggComp).Field(name: "eggProgress").GetValue<float>();
                        bool isActive = Active(eggComp);
                        if (isActive)
                        {
                            eggProgress += (1f / (eggComp.Props.eggLayIntervalDays * (2500f / parent.Severity)));
                            Traverse.Create(root: eggComp).Field(name: "eggProgress").SetValue(eggProgress);
                        }
                    }
                    //CompHasGatherableBodyResource gatherComp = this.Pawn.TryGetComp<CompHasGatherableBodyResource>();                    
                    //if (gatherComp != null)
                    //{
                    //    float gatherProgress = gatherComp.Fullness;          
                        
                    //    int rate = Traverse.Create(root: gatherComp).Field(name: "GatherResourcesIntervalDays").GetValue<int>();
                    //    bool isActive = Active();
                    //    if (isActive)
                    //    {
                    //        gatherProgress += (1f / ((float)(rate * (2500f / this.parent.Severity))));
                    //        Traverse.Create(root: gatherComp).Field(name: "fullness").SetValue(gatherProgress);
                    //    }
                    //}
                    CompMilkable milkComp = Pawn.TryGetComp<CompMilkable>();
                    if (milkComp != null)
                    {
                        float milkProgress = milkComp.Fullness;
                        int rate = milkComp.Props.milkIntervalDays;
                        bool isActive = Active(milkComp);
                        if (isActive)
                        {
                            milkProgress += (1f / ((float)(rate * (2500f / parent.Severity))));
                            Traverse.Create(root: milkComp).Field(name: "fullness").SetValue(milkProgress);
                        }
                    }
                    if (rec.Bleeding)
                    {
                        totalBleedRate += rec.BleedRate;
                    }
                }
                if(totalBleedRate != 0)
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, (totalBleedRate * 60 * parent.Severity) / (24 * 2500));
                }
            }
        }

        public bool Active(CompMilkable comp)
        {
            if(!Active())
            {
                return false;
            }
            if(Pawn.gender != Gender.Female && comp.Props.milkFemaleOnly)
            {
                return false;
            }
            if(!Pawn.ageTracker.CurLifeStage.milkable)
            {
                return false;
            }
            return true;            
        }

        public bool Active(CompEggLayer comp)
        {
            if (!Active())
            {
                return false;
            }
            if (Pawn.gender != Gender.Female && comp.Props.eggLayFemaleOnly)
            {
                return false;
            }
            if (!Pawn.ageTracker.CurLifeStage.milkable)
            {
                return false;
            }
            return true;
        }

        public bool Active()
        {
            if (Pawn.Faction == null)
            {
                return false;
            }
            if (Pawn.Suspended)
            {
                return false;
            }
            return true;
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || durationTicks <= 0;
            }
        }

        public void AccelerateEffects(Pawn pawn, int intensity)
        {
            Effecter AccelEffect = TorannMagicDefOf.TM_TimeAccelerationEffecter.Spawn();
            AccelEffect.Trigger(new TargetInfo(pawn), new TargetInfo(pawn));
            AccelEffect.Cleanup();
        }
    }
}
