using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Chi : HediffComp
    {

        private bool initialized;
        private bool removeNow;

        private int eventFrequency = 120;
        private int chiFrequency = 4;
        private int lastChiTick = 0;
        private float lastChi = 0;

        public float maxSev = 100f;

        private int pwrVal;
        private int verVal;
        private int effVal;

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
            CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();

            if (comp != null && comp.IsMightUser)
            {
                pwrVal = comp.MightData.MightPowerSkill_Chi.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Chi_pwr").level;
                verVal = comp.MightData.MightPowerSkill_Chi.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Chi_ver").level;
                effVal = comp.MightData.MightPowerSkill_Chi.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Chi_eff").level;
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
            }
            if(initialized && Find.TickManager.TicksGame % eventFrequency == 0)
            {
                if (Pawn.IsColonist)
                {
                    severityAdjustment -= (Rand.Range(.03f, .05f) - (.008f * verVal));
                }
                else if(Pawn.IsPrisoner)
                {
                    severityAdjustment -= (Rand.Range(.25f, .5f) - (.00375f * verVal));
                }
                else
                {
                    severityAdjustment += 2f;
                }
            }
            parent.Severity = Mathf.Clamp(parent.Severity, 0, maxSev);
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || removeNow;
            }
        }        
    }
}
