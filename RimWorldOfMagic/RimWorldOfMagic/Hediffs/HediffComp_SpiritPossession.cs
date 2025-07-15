using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace TorannMagic
{
    public class HediffComp_SpiritPossession : HediffComp, IThingHolder
    {
        private bool initializing = true;
        private bool shouldRemove;
        private bool shouldUnPossess;
        private int failCheck;

        private ThingOwner innerContainer;

        //public IThingHolder ParentHolder => ((IThingHolder)SpiritPawn).ParentHolder;
        public IThingHolder ParentHolder => ((IThingHolder)Pawn);

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public Pawn SpiritPawn
        {
            get
            {
                if(innerContainer != null && innerContainer.Any)
                {
                    return innerContainer.FirstOrDefault() as Pawn;
                }
                return null;
            }
            set
            {
                if(innerContainer == null)
                {
                    innerContainer = new ThingOwner<Thing>();
                    innerContainer.Clear();
                }
                innerContainer.TryAddOrTransfer(value.SplitOff(1), false);
            }
        }

        private CompAbilityUserMagic magicComp;
        private int spiritCheckFrequency = 2500;
        public int lastSpiritCheckTick;
        private int spiritLevel = 5;
        public int SpiritLevel => spiritLevel;
        private float compatibilityRatio = -5f;
        private float effVal;
        public float MaxLevelBonus;
        public float CRatio => compatibilityRatio + (.5f * effVal);
        private int conversionAttempts;

        private void UpdateSpiritCompatibilityRatio()
        {
            int matchingCount = 0;
            if (Pawn.story != null && Pawn.story.traits != null)
            {              
                foreach(TraitDef td in SpiritPawn_Hediff.traitCompatibilityList)
                {
                    foreach(Trait t in Pawn.story.traits.allTraits)
                    {
                        if(t.def == td)
                        {
                            matchingCount++;
                        }
                    }
                }
                foreach (BackstoryDef bs in SpiritPawn_Hediff.BackstoryCompatibilityList)
                {
                    if(Pawn.story.Childhood == bs || Pawn.story.Adulthood == bs)
                    {
                        matchingCount += 2;
                    }
                }
            }
            if(Pawn.RaceProps != null && Pawn.RaceProps.Humanlike)
            {
                if(Pawn.gender == SpiritPawn.gender)
                {
                    matchingCount++;
                }
            }
            if (SpiritPawn != null && SpiritPawn.story != null && SpiritPawn.story.Adulthood != null)
            {
                if (SpiritPawn.story.Adulthood.identifier == "tm_lost_spirit")
                {
                    matchingCount += 2;
                }
                if (SpiritPawn.story.Adulthood.identifier == "tm_vengeful_spirit")
                {
                    matchingCount -= 1;
                }
            }
            //assuming 
            //maximum match count of 11
            //average match count 4
            compatibilityRatio = matchingCount;
        }

        private void UpdateSpiritLevel()
        {            
            if (magicComp == null)
            {
                magicComp = Pawn.GetCompAbilityUserMagic();
            }            
            if (magicComp != null)
            {
                MaxLevelBonus = magicComp.MagicData.GetSkill_Versatility(TorannMagicDefOf.TM_SpiritPossession).level * 15;
                effVal = magicComp.MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_SpiritPossession).level;
                lastSpiritCheckTick = Find.TickManager.TicksGame;
                spiritLevel = -4;
                TMDefs.TM_CustomClass cc = TM_ClassUtility.GetCustomClassOfTrait(TorannMagicDefOf.TM_Possessed);
                for (int i = 0; i < cc.classMageAbilities.Count; i++)
                {
                    for (int j = 0; j < magicComp.MagicData.AllMagicPowers.Count; j++)
                    {
                        MagicPower power = magicComp.MagicData.AllMagicPowers[j];
                        if (power != null && power.TMabilityDefs.Contains(cc.classMageAbilities[i]))
                        {
                            if (power.learned)
                            {
                                spiritLevel += power.learnCost;
                                spiritLevel += power.costToLevel * power.level;
                            }
                        }
                    }
                }
                foreach (TMAbilityDef ability in cc.classMageAbilities)
                {
                    MagicPowerSkill mps_e = magicComp.MagicData.GetSkill_Efficiency(ability);
                    if (mps_e != null)
                    {
                        spiritLevel += mps_e.level * mps_e.costToLevel;
                    }
                    MagicPowerSkill mps_p = magicComp.MagicData.GetSkill_Power(ability);
                    if (mps_p != null)
                    {
                        spiritLevel += mps_p.level * mps_p.costToLevel;
                    }
                    MagicPowerSkill mps_v = magicComp.MagicData.GetSkill_Versatility(ability);
                    if (mps_v != null)
                    {
                        spiritLevel += mps_v.level * mps_v.costToLevel;
                    }
                }
            }
        }

        public void UpdateSpiritEnergy()
        {
            Need nds = SpiritPawn_Need;
            Need ndp = Pawn.needs.TryGetNeed(TorannMagicDefOf.TM_SpiritND);
            if (nds != null && ndp != null)
            {
                nds.CurLevel = ndp.CurLevel;
            }
        }

        public Hediff_Possessor SpiritPawn_Hediff => SpiritPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_SpiritPossessorHD) as Hediff_Possessor;
        public Need_Spirit SpiritPawn_Need => SpiritPawn.needs.TryGetNeed(TorannMagicDefOf.TM_SpiritND) as Need_Spirit;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref compatibilityRatio, "compatibilityRatio", -5f);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look<int>(ref conversionAttempts, "conversionAttempts", 0);
        }

        public override string CompLabelInBracketsExtra => SpiritPawn != null ? SpiritPawn.LabelShort + ": " + CRatio.ToString("#.#") + base.CompLabelInBracketsExtra : base.CompLabelInBracketsExtra;

        public string labelCap
        {
            get
            {
                if (SpiritPawn != null)
                {
                    return Def.LabelCap + "(" + SpiritPawn.LabelShort + ")";
                }
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                if (SpiritPawn != null)
                {
                    return Def.label + "(" + SpiritPawn.LabelShort + ")";
                }
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned && Pawn.Map != null)
            {
                
            }
            UpdateSpiritCompatibilityRatio();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && SpiritPawn != null;
            if (flag)
            {
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
                if (Find.TickManager.TicksGame > (lastSpiritCheckTick + spiritCheckFrequency))
                {
                    lastSpiritCheckTick = Find.TickManager.TicksGame;
                    UpdateSpiritLevel();
                    if(Rand.Chance(.35f))
                    {
                        AdjustAnimalTraining();
                    }
                    if(Rand.Chance(.35f))
                    {
                        AdjustHostIdeo();
                    }
                }
                if (Find.TickManager.TicksGame % 71 == 0)
                {
                    if (SpiritPawn.DestroyedOrNull())
                    {
                        shouldUnPossess = true;
                    }
                    if (SpiritPawn.Dead)
                    {
                        shouldUnPossess = true;
                    }          
                    if(Pawn.Dead)
                    {
                        shouldUnPossess = true;
                    }

                    UpdateSpiritEnergy();
                }
                if(Find.TickManager.TicksGame % 1201 == 0)
                {
                    if(compatibilityRatio < -2f)
                    {

                    }
                    float amt = (CRatio - parent.Severity) * Rand.Range(.01f, .015f);
                    severityAdjustment = amt;
                }
                //do possession harmony sync
                failCheck = 0;
            }
            else
            {
                failCheck++;
                if (failCheck > 5)
                {
                    shouldRemove = true;
                }
            }
            if(shouldUnPossess)
            {
                shouldRemove = true;
            }
        }

        public void AdjustHostIdeo()
        {
            
            if(ModsConfig.IdeologyActive)
            {
                if (Pawn.story != null && Pawn.story.traits != null && Pawn.jobs != null)
                {
                    if (Pawn.ideo != null && Pawn.Ideo != SpiritPawn.Ideo)
                    {
                        Pawn.ideo.OffsetCertainty(Rand.Range(-.01f, -.03f));
                        if(Pawn.ideo.Certainty <= 0.2f)
                        {
                            Pawn.ideo.IdeoConversionAttempt(-.5f, SpiritPawn.Ideo);
                        }
                        if (Pawn.ideo.Certainty > .8f)
                        {
                            conversionAttempts++;
                            if (conversionAttempts >= 100)
                            {
                                SpiritPawn.ideo.IdeoConversionAttempt(-1f, Pawn.Ideo);
                            }
                        }
                    }
                }
            }            
        }
        public void AdjustAnimalTraining()
        {
            if (Pawn.kindDef != null && Pawn.kindDef.RaceProps.Animal)
            {
                bool learned = false;
                if (Pawn.training.CanAssignToTrain(TrainableDefOf.Tameness).Accepted)
                {
                    if (!Pawn.training.HasLearned(TrainableDefOf.Tameness))
                    {
                        Pawn.training.Train(TrainableDefOf.Tameness, SpiritPawn);
                        learned = true;
                    }
                }
                if (!learned && Pawn.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted)
                {
                    if (!Pawn.training.HasLearned(TrainableDefOf.Obedience))
                    {
                        Pawn.training.Train(TrainableDefOf.Obedience, SpiritPawn);
                        learned = true;
                    }
                }

                if (!learned && Pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
                {
                    if (!Pawn.training.HasLearned(TrainableDefOf.Release))
                    {
                        Pawn.training.Train(TrainableDefOf.Release, SpiritPawn);
                        learned = true;
                    }
                }

                if (!learned && Pawn.training.CanAssignToTrain(TorannMagicDefOf.Haul).Accepted)
                {
                    if (!Pawn.training.HasLearned(TorannMagicDefOf.Haul))
                    {
                        Pawn.training.Train(TorannMagicDefOf.Haul, SpiritPawn);
                        learned = true;
                    }
                }

                if (!learned && Pawn.training.CanAssignToTrain(TorannMagicDefOf.Rescue).Accepted)
                {
                    if (!Pawn.training.HasLearned(TorannMagicDefOf.Rescue))
                    {
                        Pawn.training.Train(TorannMagicDefOf.Rescue, SpiritPawn);
                        learned = true;
                    }
                }
            }
        }

        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;
        
    }
}
