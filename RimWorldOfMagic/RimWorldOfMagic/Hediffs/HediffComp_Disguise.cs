using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Disguise : HediffComp
    {

        private bool initialized;
        private bool hasDisguise;
        private bool hasPossess;
        private bool disguiseFlag;
        private bool possessFlag;
        private int age;

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
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 1f);
                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_PossessionHD) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_PossessionHD_I) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_PossessionHD_II) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_PossessionHD_III))
                {
                    hasPossess = true;
                }
                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_I) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_II) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_III))
                {
                    hasDisguise = true;
                }
                if (parent.def.defName == "TM_DisguiseHD" || parent.def.defName == "TM_DisguiseHD_I" || parent.def.defName == "TM_DisguiseHD_II" || parent.def.defName == "TM_DisguiseHD_III")
                {
                    disguiseFlag = true;
                }
                if (parent.def.defName == "TM_PossessionHD" || parent.def.defName == "TM_PossessionHD_I" || parent.def.defName == "TM_PossessionHD_II" || parent.def.defName == "TM_PossessionHD_III")
                {
                    possessFlag = true;
                }
            }
            age = 60;
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
                Vector3 drawOverhead = Pawn.DrawPos;
                drawOverhead.z += .9f;
                drawOverhead.x += .2f;
                if(hasPossess)
                {
                    if(possessFlag)
                    {
                        TM_MoteMaker.ThrowTextMote(drawOverhead, Pawn.Map, Mathf.RoundToInt(parent.Severity).ToString(), Color.white, 1f / 66f, -1f);
                    }
                }
                else
                {
                    TM_MoteMaker.ThrowTextMote(drawOverhead, Pawn.Map, Mathf.RoundToInt(parent.Severity).ToString(), Color.white, 1f / 66f, -1f);
                }                

                if (age <=0)
                {
                    severityAdjustment--;
                    age = 60;                    
                }
                age--;                
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < .1f;
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref disguiseFlag, "disguiseFlag", false, false);
            Scribe_Values.Look<bool>(ref possessFlag, "possessFlag", false, false);
            Scribe_Values.Look<bool>(ref hasPossess, "hasPossess", false, false);
            Scribe_Values.Look<bool>(ref hasDisguise, "hasDisguise", false, false);
        }
    }
}
