using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Nightshade : HediffComp
    {

        private bool initialized;
        private bool removeNow;

        private int eventFrequency = 240;

        private int pwrVal;  //increased amount blood levels affect ability power
        private int verVal;  //increased blood per bleed rate and blood gift use

        public float GetApplicationSeverity
        {
            get
            {
                return 1f + (.2f * pwrVal);
            }
        }

        public int GetDoseCount
        {
            get
            {
                return (int)(parent.Severity / GetApplicationSeverity);
            }
        }

        public override string CompLabelInBracketsExtra => "" + GetDoseCount + " doses";

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
            CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
            if (spawned && comp != null && comp.IsMightUser)
            {
                pwrVal = comp.MightData.MightPowerSkill_Nightshade.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Nightshade_pwr").level;
                verVal = comp.MightData.MightPowerSkill_Nightshade.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Nightshade_ver").level;
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
                    Initialize();
                    severityAdjustment += Rand.Range(.2f, .3f);                    

                    float maxSev = 10 + (2*verVal);
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
