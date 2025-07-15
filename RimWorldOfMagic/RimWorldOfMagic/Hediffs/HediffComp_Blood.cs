using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Blood : HediffComp
    {

        private bool initialized;
        private bool removeNow;

        private int eventFrequency = 60;

        private int bloodPwr;  //increased amount blood levels affect ability power
        private int bloodVer;  //increased blood per bleed rate and blood gift use
        private int bloodEff;  //reduces ability blood costs
        private float arcaneDmg = 1f;

        public override void CompExposeData()
        {
            base.CompExposeData();
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
            CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
            if (spawned && comp != null && comp.IsMagicUser)
            {
                //bloodPwr = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_pwr").level;
                //bloodVer = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_ver").level;
                //bloodEff = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_eff").level;
                bloodPwr = TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_BloodGift, false);
                bloodVer = TM_Calc.GetSkillVersatilityLevel(Pawn, TorannMagicDefOf.TM_BloodGift, false);
                bloodEff = TM_Calc.GetSkillEfficiencyLevel(Pawn, TorannMagicDefOf.TM_BloodGift, false);
                arcaneDmg = comp.arcaneDmg;
            }
            else
            {
                removeNow = true;
            }
        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    if(Pawn.health.hediffSet.BleedRateTotal != 0)
                    {
                        //.06 bleed rate per 1 dmg "cut"
                        //.1 bleed rate per 1 dmg sacrificial cut
                        //Log.Message("current bleed rate is " + this.Pawn.health.hediffSet.BleedRateTotal);
                        severityAdjustment += (Pawn.health.hediffSet.BleedRateTotal * (1.25f + (.125f *bloodVer))) * arcaneDmg;
                    }
                    else if(!Pawn.IsColonist)
                    {
                        severityAdjustment += 5;
                    }
                    else
                    {
                        severityAdjustment -= Rand.Range(.04f, .1f);
                    }

                    Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_Artifact_BloodBoostHD);
                    float maxSev = 100;
                    if(hediff != null)
                    {
                        maxSev += hediff.Severity;
                    }
                    parent.Severity = Mathf.Clamp(parent.Severity, 0, maxSev);
                }

               
                
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return removeNow || base.CompShouldRemove;
            }
        }        
    }
}
