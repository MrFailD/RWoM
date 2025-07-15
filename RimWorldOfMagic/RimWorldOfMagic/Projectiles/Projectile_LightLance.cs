using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_LightLance : Projectile_AbilityBase
    {
        private bool initialized;
        private int verVal;
        private int pwrVal;
        public int burnTime = 200;
        private int age = -1;
        private float arcaneDmg = 1;
        private float lightPotency = .5f;
        private IntVec3 launchPosition;
        private Pawn caster;

        private ColorInt colorInt = new ColorInt(255, 255, 140);
        private Sustainer sustainer;

        private float angle;
        private float radius = 1;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        private Vector3 lanceAngle;
        private Vector3 lanceAngleInv;
        private Vector3 drawPosStart;
        private Vector3 drawPosEnd;
        private float lanceLength;
        private Vector3 lanceVector;
        private Vector3 lanceVectorInv;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref burnTime, "burntime", 200, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1, false);
            Scribe_Values.Look<float>(ref lightPotency, "lightPotency", .5f, false);
        }

        private int TicksLeft
        {
            get
            {
                return burnTime - age;
            }
        }

        private void Initialize()
        {
            caster = launcher as Pawn;
            launchPosition = caster.Position;
            CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
            //pwrVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_LightLance, "TM_LightLance", "_pwr", true);
            //verVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_LightLance, "TM_LightLance", "_ver", true);
            pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_LightLance);
            verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_LightLance);
            arcaneDmg = comp.arcaneDmg;
            if (caster.health.hediffSet.HasHediff(TorannMagicDefOf.TM_LightCapacitanceHD))
            {
                HediffComp_LightCapacitance hd = caster.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_LightCapacitanceHD).TryGetComp<HediffComp_LightCapacitance>();
                lightPotency = hd.LightPotency;
            }
            radius = Mathf.Clamp(1.8f + (.25f * verVal) * lightPotency, 1f, 3f);
            angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(caster.Position, Position)).ToAngleFlat();
            CheckSpawnSustainer();
            burnTime += (pwrVal * 22);
            lanceAngle = Vector3Utility.FromAngleFlat(angle - 90);                 //angle of beam
            lanceAngleInv = Vector3Utility.FromAngleFlat(angle + 90);              //opposite angle of beam
            drawPosStart = launchPosition.ToVector3Shifted() + lanceAngle;         //this.parent.DrawPos;
            drawPosEnd = Position.ToVector3Shifted() + lanceAngleInv;
            lanceLength = (drawPosEnd - drawPosStart).magnitude;
            lanceVector = drawPosStart + (lanceAngle * lanceLength * 0.5f);
            lanceVectorInv = drawPosEnd + (lanceAngleInv * lanceLength * .5f);          //draw for double beam
            lanceVector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);          //graphic depth
        }

        private void CheckSpawnSustainer()
        {
            if (TicksLeft >= 0)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    sustainer = SoundDef.Named("OrbitalBeam").TrySpawnSustainer(SoundInfo.InMap(selectedTarget, MaintenanceType.PerTick));
                });
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Destroy(DestroyMode.Vanish);

            if (!initialized)
            {
                Initialize();
                initialized = true;
            }

            if (sustainer != null)
            {
                sustainer.info.volumeFactor = 1;
                sustainer.Maintain();
                if (TicksLeft <= 0)
                {
                    sustainer.End();
                    sustainer = null;
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawLance(launchPosition);
        }

        public void DrawLance(IntVec3 launcherPos)
        {           
            float lanceWidth = radius;                                                              //
            if(age < (burnTime * .165f))
            {
                lanceWidth *= (float)age / 40f;
            }
            if(age > (burnTime * .835f))
            {
                lanceWidth *= (float)(burnTime - age) / 40f;
            }
            lanceWidth *= Rand.Range(.9f, 1.1f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(lanceVector, Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, lanceLength));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);

            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(drawPosStart - (.5f*lanceAngle*lanceWidth), Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, lanceWidth));                 //drawer for beam start
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
            drawPosEnd.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix4 = default(Matrix4x4);
            matrix4.SetTRS(drawPosEnd - (.5f*lanceAngleInv*lanceWidth), Quaternion.Euler(0f, angle - 180, 0f), new Vector3(lanceWidth, 1f, lanceWidth));                 //drawer for beam end
            Graphics.DrawMesh(MeshPool.plane10, matrix4, BeamEndMat, 0, null, 0, MatPropertyBlock);            
        }

        public override void Tick()
        {
            base.Tick();
            age++;
            if (age < (burnTime * .9f))
            {
                if (Find.TickManager.TicksGame % 5 == 0)
                {
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Heat, launchPosition.ToVector3Shifted(), Map, Rand.Range(.6f, 1.1f), .4f, .1f, .3f, Rand.Range(-200, 200), Rand.Range(5f, 9f), angle + Rand.Range(-15f, 15f), Rand.Range(0, 360));
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age <= burnTime;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }        
    }
}


