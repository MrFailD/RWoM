using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;

namespace TorannMagic
{
    public class HediffComp_ReverseTime : HediffComp
    {
        private bool initialized = true;

        public bool isBad;
        public int durationTicks = 6000;
        private int tickEffect = 300;
        private int tickPeriod = 120;

        private long currentAge = 1;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref isBad, "isBad", false, false);
            Scribe_Values.Look<int>(ref durationTicks, "durationTicks", 6000, false);
            Scribe_Values.Look<long>(ref currentAge, "currentAge", 1, false);
            Scribe_Values.Look<int>(ref tickEffect, "tickEffect", 300, false);
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if(isBad)
                {
                    return base.CompLabelInBracketsExtra + " (warped)";
                }
                return base.CompLabelInBracketsExtra + " " + (durationTicks/60) + "s";
            }
        }
        

        public string labelCap
        {
            get
            {
                if (isBad)
                {
                    return Def.LabelCap + " (warped)";
                }
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                if(isBad)
                {
                    return Def.label + " (warped)";
                }
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
            currentAge = Pawn.ageTracker.AgeBiologicalTicks;
            tickEffect = Mathf.RoundToInt(durationTicks / 500);
        }

        public override void CompPostPostRemoved()
        {
            if(Pawn.ageTracker.AgeBiologicalTicks + (60*60000) <  currentAge)
            {
                Pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            }
            base.CompPostPostRemoved();
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

            if (Find.TickManager.TicksGame % tickPeriod == 0)
            {                

                ReverseHediff(Pawn, tickPeriod);
                durationTicks -= tickPeriod;                

                if (true)
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= Mathf.RoundToInt(15000f * Mathf.Clamp(Pawn.ageTracker.AgeBiologicalYearsFloat/10f, .5f, 20f));
                    if (Pawn.ageTracker.AgeBiologicalTicks < 0 && Pawn.ageTracker.AgeBiologicalYears > -10)
                    {
                        Messages.Message("TM_CeaseToExist".Translate(Pawn.LabelShort), MessageTypeDefOf.NeutralEvent);
                        Pawn.Destroy(DestroyMode.Vanish);
                    }
                }
            }

            if (Find.TickManager.TicksGame % tickEffect == 0)
            {
                ReverseEffects(Pawn, 1);
            }
        }

        private void ReverseHediff(Pawn pawn, int ticks)
        {
            float totalBleedRate = 0;
            int totalHDremoved = 0;
            using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {                    
                    Hediff rec = enumerator.Current;
                    if (rec != null)
                    {
                        HediffComp_Immunizable immuneComp = rec.TryGetComp<HediffComp_Immunizable>();
                        if (immuneComp != null)
                        {
                            if (immuneComp.Def.CompProps<HediffCompProperties_Immunizable>() != null)
                            {
                                float immuneSevDay = immuneComp.Def.CompProps<HediffCompProperties_Immunizable>().severityPerDayNotImmune;
                                if (immuneSevDay != 0 && !rec.FullyImmune())
                                {
                                    rec.Severity -= ((immuneSevDay * ticks * parent.Severity) / (2000));
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
                                    bool drugTolerance = false;
                                    HediffComp_DrugEffectFactor drugEffectComp = rec.TryGetComp<HediffComp_DrugEffectFactor>();
                                    if (drugEffectComp != null)
                                    {
                                        if (drugEffectComp.Def.CompProps < HediffCompProperties_DrugEffectFactor>().chemical != null)
                                        {
                                            drugTolerance = true;
                                        }
                                    }
                                    if (!drugTolerance)
                                    {
                                        rec.Severity -= ((sevDay * ticks * parent.Severity) / (800));
                                    }
                                }
                            }
                        }
                        HediffComp_Disappears tickComp = rec.TryGetComp<HediffComp_Disappears>();
                        if (tickComp != null)
                        {
                            int ticksToDisappear = Traverse.Create(root: tickComp).Field(name: "ticksToDisappear").GetValue<int>();
                            if (ticksToDisappear != 0)
                            {
                                Traverse.Create(root: tickComp).Field(name: "ticksToDisappear").SetValue(ticksToDisappear + (Mathf.RoundToInt(ticks * parent.Severity)));
                            }
                        }
                        if (rec.Bleeding)
                        {
                            totalBleedRate += rec.BleedRate;
                        }
                    }
                }
                if (totalBleedRate != 0)
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, -(totalBleedRate * ticks * parent.Severity) / (24 * 2500));
                }
            }
            List<Hediff> hediffList = pawn.health.hediffSet.hediffs.ToList();
            if (hediffList != null && hediffList.Count > 0)
            {
                for (int i = 0; i < hediffList.Count; i++)
                {
                    Hediff rec = hediffList[i];
                    if (rec != null && rec != parent)
                    {
                        if (rec.def.scenarioCanAdd || rec.def.isBad)
                        {
                            if ((rec.ageTicks - 1000) < 0)
                            {
                                if (rec.def.defName.Contains("TM_"))
                                {
                                    if (rec.def.isBad && rec.def != TorannMagicDefOf.TM_ResurrectionHD && rec.def != TorannMagicDefOf.TM_DeathReversalHD)
                                    {
                                        totalHDremoved++;
                                        Pawn.health.RemoveHediff(rec);
                                        break;
                                    }
                                }
                                else
                                {
                                    List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                                    List<BodyPartRecord> replacementList = new List<BodyPartRecord>();
                                    replacementList.Clear();
                                    for (int j = 0; j < bpList.Count; j++)
                                    {
                                        BodyPartRecord record = bpList[j];
                                        if (pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record) && (record.parent == null || pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent)) && (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)))
                                        {
                                            replacementList.Add(record);
                                        }
                                    }
                                    Hediff_MissingPart mphd = rec as Hediff_MissingPart;
                                    if (mphd != null && mphd.Part != null)
                                    {
                                        if (replacementList.Contains(mphd.Part))
                                        {
                                            if (RemoveChildParts(mphd))
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (Pawn.needs != null && Pawn.needs.mood != null && Pawn.needs.mood.thoughts != null && Pawn.needs.mood.thoughts.memories != null)
                                                {
                                                    Pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_PhantomLimb);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            goto IgnoreHediff;
                                        }
                                    }
                                    totalHDremoved +=2;
                                    Pawn.health.RemoveHediff(rec);
                                    i = hediffList.Count;
                                    break;
                                    IgnoreHediff:;                                    
                                }                                
                            }
                            else
                            {
                                rec.ageTicks -= 1000;
                            }
                        }
                    }
                }
            }
            ReduceReverseTime(totalHDremoved);
            //using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.GetHediffs<Hediff>().GetEnumerator())
            //{
            //    while (enumerator.MoveNext())
            //    {
            //        Hediff rec = enumerator.Current;
            //        if (rec != null && rec != this.parent)
            //        {
            //            if ((rec.ageTicks - 2500) < 0)
            //            {
            //                if (rec.def.defName.Contains("TM_"))
            //                {
            //                    if (rec.def.isBad)
            //                    {
            //                        this.Pawn.health.RemoveHediff(rec);
            //                    }
            //                }
            //                else
            //                {
            //                    this.Pawn.health.RemoveHediff(rec);
            //                }
            //            }
            //            else
            //            {
            //                rec.ageTicks -= 2500;
            //            }
            //        }
            //    }
            //}
        }

        public void ReduceReverseTime(int removedCount)
        {
            durationTicks -= Mathf.RoundToInt(removedCount * Rand.Range(20f, 30f) * tickPeriod);
        }

        public void ReverseEffects(Pawn pawn, int intensity)
        {
            Effecter ReverseEffect = TorannMagicDefOf.TM_TimeReverseEffecter.Spawn();
            ReverseEffect.Trigger(new TargetInfo(pawn), new TargetInfo(pawn));
            ReverseEffect.Cleanup();
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || durationTicks <= 0;
            }
        }

        public bool HasParentPart(Hediff_MissingPart mphd)
        {
            bool hasMissingParent = false;
            if (mphd.Part.parent != null)
            {
                List<Hediff_MissingPart> hediffList = Pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                for (int i = 0; i < hediffList.Count; i++)
                {
                    if(mphd.Part.parent == hediffList[i].Part)
                    {
                        
                    }
                }
            }
            return hasMissingParent;
        }

        public bool RemoveChildParts(Hediff_MissingPart mphd)
        {
            List<Hediff> hediffList = Pawn.health.hediffSet.hediffs.ToList();
            for (int i = 0; i < hediffList.Count; i++)
            {
                Hediff_MissingPart mpChild = hediffList[i] as Hediff_MissingPart;
                if (mpChild != null && mpChild != parent && mpChild != mphd && mpChild.Part != null)
                {
                    if (mphd.Part == mpChild.Part.parent)
                    {
                        if (RemoveChildParts(mpChild))
                        {
                            return true;
                        }
                        else
                        {
                            Pawn.health.RemoveHediff(hediffList[i]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
