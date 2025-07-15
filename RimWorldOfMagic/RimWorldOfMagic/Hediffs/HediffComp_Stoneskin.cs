using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Stoneskin : HediffComp
    {
        private bool initializing = true;
        public int maxSev = 4;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref maxSev, "maxSev", 4, false);
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
            if (spawned)
            {
                maxSev = Mathf.RoundToInt(parent.Severity);
                //FleckMaker.ThrowHeatGlow(base.Pawn.DrawPos.ToIntVec3(), base.Pawn.Map, 2f);
            }            
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);            
            bool flag = Pawn != null;
            if (flag)
            {
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
            }

            if (Find.Selector.FirstSelectedObject == Pawn)
            {
                HediffStage hediffStage = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_StoneskinHD"), false).CurStage;
                hediffStage.label = parent.Severity.ToString("0") + " charges";               
            }
            
            if(Find.TickManager.TicksGame % 1800 == 0)
            {
                if(parent.Severity < maxSev)
                {
                    parent.Severity++;
                }
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < 1;
            }
        }

        public override void CompPostPostRemoved()
        {
            SoundInfo info = SoundInfo.InMap(new TargetInfo(Pawn.Position, Pawn.Map, false), MaintenanceType.None);
            info.pitchFactor = .7f;
            TorannMagicDefOf.EnergyShield_Broken.PlayOneShot(info);
            FleckMaker.ThrowLightningGlow(Pawn.DrawPos, Pawn.Map, 1.5f);
            base.CompPostPostRemoved();
        }
    }
}
