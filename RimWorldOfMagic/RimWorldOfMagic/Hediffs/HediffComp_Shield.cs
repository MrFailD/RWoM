using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Shield : HediffComp
    {
        private static readonly Color shieldColor = new Color(160f, 160f, 160f);
        private static readonly Material shieldNS = MaterialPool.MatFrom("Other/angelwings3", ShaderDatabase.Transparent, shieldColor);
        private static readonly Material shieldE = MaterialPool.MatFrom("Other/angelwings_east", ShaderDatabase.Transparent, shieldColor);
        private static readonly Material shieldW = MaterialPool.MatFrom("Other/angelwings_west", ShaderDatabase.Transparent, shieldColor);

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

        private float lastSev;

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
                return 0.000166667f;
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
            energy = 0.5f; //lasts for x * 600 ticks; 3000ticks = 50s
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
                if (ShieldFade > 0)
                {
                    DrawShieldFade(Pawn, ShieldFade);
                    ShieldFade--;
                }
                ResolveSeverityChange();
                if (SevChange > 0.005f)
                {
                    ShieldFade += (int)(1000 * SevChange);
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
            List<Hediff> list = new List<Hediff>();
            List<Hediff> arg_32_0 = list;
            Pawn pawn = Pawn;
            IEnumerable<Hediff> arg_32_1;
            if (pawn == null)
            {
                arg_32_1 = null;
            }
            else
            {
                Pawn_HealthTracker expr_1A = pawn.health;
                if (expr_1A == null)
                {
                    arg_32_1 = null;
                }
                else
                {
                    HediffSet expr_26 = expr_1A.hediffSet;
                    arg_32_1 = ((expr_26 != null) ? expr_26.hediffs : null);
                }
            }
            arg_32_0.AddRange(arg_32_1);
            Pawn expr_3E = Pawn;
            int? arg_84_0;
            if (expr_3E == null)
            {
                arg_84_0 = null;
            }
            else
            {
                Pawn_HealthTracker expr_52 = expr_3E.health;
                if (expr_52 == null)
                {
                    arg_84_0 = null;
                }
                else
                {
                    HediffSet expr_66 = expr_52.hediffSet;
                    arg_84_0 = ((expr_66 != null) ? new int?(expr_66.hediffs.Count<Hediff>()) : null);
                }
            }
            bool flag = (arg_84_0 ?? 0) > 0;
            if (flag)
            {
                foreach (Hediff current in list)
                {
                    if (current.def.defName == "TM_HediffShield")
                    {
                        SevChange = lastSev - current.Severity;
                        lastSev = current.Severity;
                    }
                }
            }
            list.Clear();
            list = null;
        }


        private void DrawShieldFade(Pawn shieldedPawn, int magnitude)
        {
            bool flag = !shieldedPawn.Dead && !shieldedPawn.Downed;
            if (flag)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, magnitude);
                Vector3 vector = shieldedPawn.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(3f, 3f, 3f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                if (shieldedPawn.Rotation == Rot4.South || shieldedPawn.Rotation == Rot4.North )
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, shieldNS, 0);
                }
                if (shieldedPawn.Rotation == Rot4.East)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, shieldE, 0);
                }
                if (shieldedPawn.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, shieldW, 0);
                }
            }
        }

        private void Break()
        {
            if (!broken && Pawn.Map != null)
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
