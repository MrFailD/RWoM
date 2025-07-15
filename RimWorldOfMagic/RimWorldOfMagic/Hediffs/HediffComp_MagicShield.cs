using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_MagicShield : HediffComp
    {
        private static readonly Color shieldColor = new Color(160f, 160f, 160f);

        private int shieldFade;
        public int ShieldFade
        {
            get
            {
                return shieldFade;
            }
            set
            {
                shieldFade = value;
            }
        }

        private float sevChange;
        public float SevChange
        {
            get
            {
                return sevChange;
            }
            set
            {
                sevChange = value;
            }
        }

        private float lastSev = 0;

        private float energy;

        private bool initializing = true;

        private bool broken;

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

        private float EnergyLossPerTick  
        {
            get
            {
                return 1f;
            }
        }

        public float Energy
        {
            get
            {
                return energy;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
            }
            energy = 2700; //45s
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
                ResolveSeverityChange();
                if (SevChange > 0.005f)
                {
                    TM_Action.DisplayShield(Pawn, SevChange);
                }
                energy -= EnergyLossPerTick;
                bool flag5 = energy <= 0;
                if (flag5)
                {
                    severityAdjustment = -10f;
                    Break();
                }

            }
            Pawn.SetPositionDirect(Pawn.Position);
        }

        private void ResolveSeverityChange()
        {
            SevChange = lastSev - parent.Severity; 
        }

        private void Break()
        {
            if (!broken)
            {
                TorannMagicDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                FleckMaker.Static(Pawn.TrueCenter(), Pawn.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    Vector3 loc = Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                    FleckMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(0.8f, 1.2f));
                }
                energy = 0f;
                broken = true;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref energy, "energy", 0f, false);
        }
    }
}
