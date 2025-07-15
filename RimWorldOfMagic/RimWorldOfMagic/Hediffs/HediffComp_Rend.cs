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
    public class HediffComp_Rend : HediffComp
    {

        private bool initialized;
        private int initializeDelay;
        private bool removeNow;

        private int eventFrequency = 20;

        private int rendPwr;  //increased amount blood levels affect ability power
        private int rendVer;  //increased blood per bleed rate and blood gift use
        private float arcaneDmg = 1;

        private float bleedRate = 0;

        public Pawn linkedPawn;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref linkedPawn, "linkedPawn", false);
            Scribe_Values.Look<int>(ref rendVer, "rendVer", 0, false);
            Scribe_Values.Look<int>(ref rendPwr, "rendPwr", 0, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
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
            CompAbilityUserMagic comp = linkedPawn.GetCompAbilityUserMagic();
            if (spawned && comp != null && comp.IsMagicUser)
            {
                MagicPowerSkill bpwr = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_pwr");
                rendPwr = comp.MagicData.MagicPowerSkill_Rend.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Rend_pwr").level;
                rendVer = comp.MagicData.MagicPowerSkill_Rend.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Rend_ver").level;
                arcaneDmg = comp.arcaneDmg;
                arcaneDmg *= (1 + .1f * bpwr.level);
            }
            else
            {
                removeNow = true;
            }
        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null && initializeDelay > 5;
            if (flag)
            {
                if (!initialized && linkedPawn != null)
                {
                    initialized = true;
                    Initialize();
                }

                if(Pawn.DestroyedOrNull() || Pawn.Dead || Pawn.Map == null || Pawn.RaceProps.BloodDef == null)
                {
                    removeNow = true;
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0 && !removeNow)
                {
                    DamagePawn();
                    severityAdjustment -= Rand.Range(.1f, .15f);
                }
            }
            else
            {
                initializeDelay++;
            }
        }
        
        public void DamagePawn()
        {
            if(Pawn != null && Pawn.Map != null)
            { 
                TM_Action.DamageEntities(Pawn, null, Mathf.RoundToInt(2f * (arcaneDmg + (.15f * rendPwr))), TMDamageDefOf.DamageDefOf.TM_BloodyCut, linkedPawn);

                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_CrossStrike, Pawn.DrawPos, Pawn.Map, Rand.Range(.3f, 0.45f), .45f, .05f, .20f, 0, 0, 0, Rand.Range(0, 360));
                for (int j = 0; j < (1 + rendVer); j++)
                {
                    IntVec3 rndPos = Pawn.Position;
                    rndPos.x += Mathf.RoundToInt(Rand.Range(-2f, 2f));
                    rndPos.z += Mathf.RoundToInt(Rand.Range(-2f, 2f));
                    if (Pawn.RaceProps != null && Pawn.RaceProps.BloodDef != null && Pawn.Map != null)
                    {
                        FilthMaker.TryMakeFilth(rndPos, Pawn.Map, Pawn.RaceProps.BloodDef, 1);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodSquirt, Pawn.DrawPos, Pawn.Map, Rand.Range(.6f, 1.0f), .15f, .05f, .66f, Rand.Range(-100, 100), Rand.Range(1, 2), Rand.Range(0, 360), Rand.Range(0f, 360f));
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
    }
}
