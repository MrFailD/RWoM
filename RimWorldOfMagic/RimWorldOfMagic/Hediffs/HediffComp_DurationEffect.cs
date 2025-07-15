using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_DurationEffect : HediffComp
    {

        private bool initialized;
        private int effectFrequency;
        private int ticksTillEffect;
        private float severityReduction;

        private float scaleAvg = 1f;
        private float fadeIn = .5f;
        private float fadeOut = .5f;
        private float solidTime;
        private float velocity = 1f;
        private float velocityAngle;
        private float lookAngle;
        private int rotationRate;


        private ThingDef moteDef;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref ticksTillEffect, "ticksTillEffect", 0, false);
            Scribe_Values.Look<float>(ref severityReduction, "severityReduction", .1f, false);
            Scribe_Defs.Look<ThingDef>(ref moteDef, "moteDef");
            Scribe_Values.Look<float>(ref scaleAvg, "scaleAvg", 1f, false);
            Scribe_Values.Look<float>(ref fadeIn, "fadeIn", .5f, false);
            Scribe_Values.Look<float>(ref fadeOut, "fadeOut", .5f, false);
            Scribe_Values.Look<float>(ref solidTime, "solidTime", 0f, false);
            Scribe_Values.Look<float>(ref velocity, "velocity", 0f, false);
            Scribe_Values.Look<float>(ref velocityAngle, "velocityAngle", 0f, false);
            Scribe_Values.Look<float>(ref lookAngle, "lookAngle", 0f, false);
            Scribe_Values.Look<int>(ref rotationRate, "rotationRate", 0, false);
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
                if(parent.def == TorannMagicDefOf.TM_GravitySlowHD)
                {
                    effectFrequency = 120;
                    severityReduction = .2f;
                    moteDef = TorannMagicDefOf.Mote_ArcaneWaves;
                    scaleAvg = .25f;
                    solidTime = 1f;
                    fadeIn = .1f;
                    fadeOut = .75f;
                    rotationRate = 500;
                    velocity = 0;
                    velocityAngle = 0;
                    lookAngle = Rand.Range(0, 360);
                }
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
                if(ticksTillEffect <=0)
                {
                    severityAdjustment -= severityReduction;
                    ticksTillEffect = effectFrequency;
                    TM_MoteMaker.ThrowGenericMote(moteDef, Pawn.DrawPos, Pawn.Map, Rand.Range(.75f*scaleAvg, 1.25f*scaleAvg), solidTime, fadeIn, fadeOut, rotationRate, velocity, velocityAngle, lookAngle);
                }
                ticksTillEffect--;
            }
            else
            {
                severityAdjustment = 0;
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < .01f;
            }
        }
    }
}
