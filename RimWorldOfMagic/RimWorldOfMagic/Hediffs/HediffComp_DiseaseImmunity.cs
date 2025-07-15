using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TorannMagic
{
    public class HediffComp_DiseaseImmunity : HediffComp
    {
        private bool initialized;
        public int verVal;

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

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {

                if(!Pawn.Dead && Pawn.Spawned)
                {
                    if (Find.TickManager.TicksGame % 2500 == 0)
                    {
                        if(verVal >= 3)
                        {
                            IEnumerable<Hediff> hdEnum = Pawn.health.hediffSet.hediffs;
                            foreach (Hediff hd in hdEnum)
                            {
                                if (hd.def.defName == "BloodRot")
                                {
                                    int pwrDef = 2;
                                    if (parent.def == TorannMagicDefOf.TM_DiseaseImmunity2HD)
                                    {
                                        pwrDef = 3;
                                    }
                                    hd.Severity -= (.005f * pwrDef);
                                    break;
                                }
                            }
                        }
                    }
                }                
            }
        }
    }
}
