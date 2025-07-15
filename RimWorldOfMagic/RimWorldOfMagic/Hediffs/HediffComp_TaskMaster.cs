using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_TaskMaster : HediffComp
    {

        private bool initializing = true;
        private int nextTickAction;

        public int duration = 1;

        private bool removeNow;

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref duration, "duration", 1, false);
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
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
            }
            if(Find.TickManager.TicksGame >= nextTickAction)
            {
                duration--;                
                nextTickAction = Find.TickManager.TicksGame + Rand.Range(600, 700);
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
    }
}
