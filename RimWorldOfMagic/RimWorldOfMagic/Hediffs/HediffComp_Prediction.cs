using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TorannMagic
{
    public class HediffComp_Prediction : HediffComp
    {
        private bool initialized;

        private int pwrVal;
        public bool removeNow;

        public int blurTick = 0;
        private int predictionFrequency = 120;

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
            if (spawned && Pawn.Map != null && Pawn.story != null)
            {
                //FleckMaker.ThrowLightningGlow(base.Pawn.TrueCenter(), base.Pawn.Map, 3f);
                Pawn caster = Pawn;
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                if (comp != null)
                {
                    pwrVal = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Prediction.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Prediction_pwr").level;
                    parent.Severity = .5f + pwrVal;
                }
            }
            else
            {
                removeNow = true;
            }
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
                else if(initialized && !Pawn.Dead && !Pawn.Downed && Pawn.Spawned)
                {
                    //if(Find.TickManager.TicksGame % this.predictionFrequency == 0)
                    //{
                    //    IncidentQueue iq = Find.Storyteller.incidentQueue;

                    //    Log.Message("incidents count is  " + iq.Count + " with incident queue containing: " + iq.DebugQueueReadout);
                    //}

                    if (Find.TickManager.TicksGame % 60 == 0)
                    {
                        UpdateSeverity();
                    }
                }                
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || removeNow;
            }
        }

        public void UpdateSeverity()
        {
            float sev = parent.Severity;
            Pawn caster = Pawn;
            CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
            

            if (comp != null)
            {
                pwrVal = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Prediction.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Prediction_pwr").level;
                if (sev <= 0)
                {
                    removeNow = true;
                }
                else if(!Pawn.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                {
                    parent.Severity = 5;
                }
                else if(sev != pwrVal + .5f)
                {
                    parent.Severity = pwrVal + .5f;
                }
            }
            else
            {
                removeNow = true;
            }
        }
    }
}
