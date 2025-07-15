using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    public class HediffComp_SetDuration : HediffComp
    {
        public int duration = 10;

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref duration, "duration", 10, false);
            base.CompExposeData();
        }

        public HediffCompProperties_SetDuration Props => (HediffCompProperties_SetDuration)props;

        public override void CompPostMake()
        {
            duration = Props.duration;
        }

        public bool initialized;
        public bool removeNow;

        public virtual string labelCap
        {
            get
            {
                return Def.LabelCap + (" seconds remaining " + duration.ToString("#"));
            }
        }

        public virtual string label
        {
            get
            {
                return Def.label + (" seconds remaining " + duration.ToString("#"));
            }
        }


        public virtual void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {

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
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (duration > Props.maxDuration) duration = Props.maxDuration;
                duration--;
                if (duration <= 0)
                {
                    removeNow = true;
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

        //public override void CompPostMerged(Hediff other)
        //{
        //    base.CompPostMerged(other);
        //    if (other.def == this.parent.def)
        //    {
        //        HediffComp_SetDuration hdComp = other.TryGetComp<HediffComp_SetDuration>();
        //        if (hdComp != null && hdComp.duration > 0)
        //        {
        //            this.duration += hdComp.duration;
        //        }
        //        this.parent.Severity = Mathf.Max(other.Severity, this.parent.Severity);
        //    }
        //}
    }
}
