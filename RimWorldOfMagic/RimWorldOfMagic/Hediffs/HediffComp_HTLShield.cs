using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_HTLShield : HediffComp
    {

        private bool initialized;
        private int initializeDelay;
        private bool removeNow = false;

        private int eventFrequency = 180;

        private float lastSeverity = 0;

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

        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null && initializeDelay > 1;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    severityAdjustment -= Rand.Range(2.5f, 4f);                  
                }
            }
            else
            {
                initializeDelay++;
            }
        }        

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || removeNow || parent.Severity <= .001f;
            }
        }        
    }
}
