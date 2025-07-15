using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_Explosion : Projectile_AbilityBase
    {
        private bool initialized;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;
        private int beamAge;
        private int age = -1;
        private int duration = 360;
        private int outerRingAngle;
        private int middleRingAngle = 120;
        private int innerRingAngle = 240;
        private int expandingTick;
        private bool phase2Flag;
        private bool phase3Flag;
        private IntVec3 strikePos = default(IntVec3);
        private List<IntVec3> outerRing = new List<IntVec3>();
        private Pawn caster;
        private IEnumerable<IntVec3> oldExplosionCells;
        private IEnumerable<IntVec3> newExplosionCells;

        private ColorInt colorInt = new ColorInt(200, 50, 0);
        private Sustainer sustainer;

        private float angle = 0;
        private float radius = 12f;
        private float damage = 1f;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BombardMat = MaterialPool.MatFrom("Other/Bombardment", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material FireCircle = MaterialPool.MatFrom("Motes/firecircle", true);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref expandingTick, "expandingTick", 0, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref phase2Flag, "phase2Flag", false, false);
            Scribe_Values.Look<bool>(ref phase3Flag, "phase3Flag", false, false);
            Scribe_Values.Look<float>(ref radius, "radius", 12f, false);
            Scribe_Values.Look<float>(ref damage, "damage", 1f, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1f, false);
            Scribe_Values.Look<int>(ref duration, "duration", 360, false);
            Scribe_Values.Look<IntVec3>(ref strikePos, "strikePos", default(IntVec3), false);
        }

        private int TicksLeft
        {
            get
            {
                return duration - age;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            ThingDef def = this.def;
            if (!initialized)
            {
                caster = launcher as Pawn;
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                //verVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_Custom, "TM_Explosion", "_ver", true);
                //pwrVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_Custom, "TM_Explosion", "_pwr", true);
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_Explosion);
                pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_Explosion);
                arcaneDmg = comp.arcaneDmg;
                CheckSpawnSustainer();
                strikePos = Position;
                duration = 360 - (verVal*3);
                damage = DamageAmount * arcaneDmg * (1f + (.02f * pwrVal));
                radius = this.def.projectile.explosionRadius + (int)(verVal/10);
                initialized = true;
                outerRing = TM_Calc.GetOuterRing(strikePos, radius - 1, radius);
                Color color = colorInt.ToColor;
                MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            }

            if (sustainer != null)
            {
                sustainer.info.volumeFactor = (age) / (duration);
                sustainer.Maintain();
                if (TicksLeft <= 0)
                {
                    sustainer.End();
                    sustainer = null;
                }
            }

            //there are 6 + 3 phases to explosion (this is no simple matter)
            //phase 1 - warmup and power collection; depicted by wind drawing into a focal point
            //phase 2 - pause (for dramatic effect)
            //phase 3 - initial explosion, ie the "shockwave"
            //phase 4 - ripping winds (the debris launched by the explosion)
            //phase 5 - burning winds (heat and flame - scorched earth)
            //phase 6 - aftershock 
            //training adds 3 phases
            //phase 3a - emp
            //phase 4a - secondary explosions
            //phase 5a - radiation
            
            //warmup 2 seconds
            if (age <= (int)(duration *.4f) && outerRing.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 startPos = outerRing.RandomElement().ToVector3Shifted();
                    Vector3 direction = TM_Calc.GetVector(startPos, strikePos.ToVector3Shifted());
                    TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, startPos, Map, .8f, .3f, .05f, .6f, 0, 12f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 0);
                }
            }
            else if(age <= (int)(duration * .6f))
            {
                //pause                
            }
            else if(age > (int)(duration * .6f) && !phase3Flag)
            {
                if (!phase2Flag)
                {
                    TargetInfo ti = new TargetInfo(strikePos, map, false);
                    TM_MoteMaker.MakeOverlay(ti, TorannMagicDefOf.TM_Mote_PsycastAreaEffect, map, Vector3.zero, 2, 0f, .1f, .4f, 0, 15f);
                    Effecter igniteED = TorannMagicDefOf.TM_ExplosionED.Spawn();
                    igniteED.Trigger(new TargetInfo(strikePos, map, false), new TargetInfo(strikePos, map, false));
                    igniteED.Cleanup();
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(strikePos, map, false), MaintenanceType.None);
                    info.pitchFactor = .75f;
                    info.volumeFactor = 2.6f;
                    TorannMagicDefOf.TM_FireWooshSD.PlayOneShot(info);
                    phase2Flag = true;
                }
                expandingTick++;
                if (expandingTick <= radius)
                {
                    IntVec3 centerCell = Position;
                    if (expandingTick <= 1 || oldExplosionCells.Count() <= 0)
                    {
                        oldExplosionCells = GenRadial.RadialCellsAround(centerCell, expandingTick - 1, true);
                    }
                    else
                    {
                        oldExplosionCells = newExplosionCells;
                    }

                    newExplosionCells = GenRadial.RadialCellsAround(centerCell, expandingTick, true);
                    IEnumerable<IntVec3> explosionCells = newExplosionCells.Except(oldExplosionCells);
                    foreach (IntVec3 cell in explosionCells)
                    {
                        if (cell.InBoundsWithNullCheck(map))
                        {
                            Vector3 heading = (cell - centerCell).ToVector3();
                            float distance = heading.magnitude;
                            Vector3 direction = TM_Calc.GetVector(centerCell, cell);
                            float angle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
                            if (expandingTick >= 6 && expandingTick < 8)
                            {
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_DirectionalDirt, cell.ToVector3Shifted(), map, .8f, .2f, .15f, .5f, 0, 7f, angle, angle);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ExpandingFlame, cell.ToVector3Shifted(), map, 1.1f, .3f, .02f, .25f, 0, 15f, angle, angle);
                            }
                            List<Thing> hitList = cell.GetThingList(map);
                            Thing burnThing = null;
                            for (int j = 0; j < hitList.Count; j++)
                            {
                                burnThing = hitList[j];
                                DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, Mathf.RoundToInt(Rand.Range(damage *.25f, damage *.35f)), 1, -1, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                                burnThing.TakeDamage(dinfo);
                            }                            
                        }
                    }
                }
                else
                {
                    TargetInfo ti = new TargetInfo(strikePos, map, false);
                    TM_MoteMaker.MakeOverlay(ti, TorannMagicDefOf.TM_Mote_PsycastAreaEffect, map, Vector3.zero, 4, 0f, .1f, .4f, .5f, 2f);
                    expandingTick = 0;
                    phase3Flag = true;                    
                }
            }
            else if(phase3Flag)
            {
                expandingTick++;
                if(expandingTick < 4)
                {
                    float energy = 80000 * arcaneDmg;
                    GenTemperature.PushHeat(strikePos, Map, energy);
                    ExplosionHelper.Explode(strikePos, Map, radius/(4-expandingTick), DamageDefOf.Bomb, launcher, Mathf.RoundToInt(Rand.Range(damage *.7f, damage*.9f)), 1, DamageDefOf.Bomb.soundExplosion, null, null, null, null, 0, 0, null, false, null, 0, 0, .4f, true);
                }
            }
            Destroy(DestroyMode.Vanish);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (initialized)
            {
                if (age <= (int)(duration * .6f))
                {
                    //DrawSmiteBeams(this.strikePos, this.beamAge);
                }
                else if(age > (int)(duration *.6f) && age <= (int)duration * .7f)
                {
                    beamAge = age - (int)(duration * .6f);
                    DrawSmiteBeams(strikePos, beamAge);
                }
                if(age <= (int)(duration * .6f))
                {
                    float sizer = 1f * (float)(radius / def.projectile.explosionRadius);
                    if(age > (int)(duration * .4f))
                    {
                        sizer = ((duration * .6f) - age) / ((duration * .6f) - (duration * .4f));
                    }
                    if (age > (int)(duration * .1f))
                    {
                        outerRingAngle+=3;
                        DrawFlameRing(strikePos, 26f*sizer, outerRingAngle);
                    }
                    if (age > (int)(duration * .18f))
                    {
                        middleRingAngle += 7;
                        DrawFlameRing(strikePos, 13f*sizer, middleRingAngle);
                    }
                    if (age > (int)(duration * .25f))
                    {
                        innerRingAngle += 13;
                        DrawFlameRing(strikePos, 7f*sizer, innerRingAngle);
                    }
                }
            }
        }

        public void DrawFlameRing(IntVec3 pos, float size, float angle)
        {
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos.ToVector3Shifted(), Quaternion.Euler(0f, angle, 0f), new Vector3(size, 1f, size));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, FireCircle, 0);
        }

        public void DrawSmiteBeams(IntVec3 pos, int wrathAge)
        {
            float lanceWidth = .1f + wrathAge;
            float lanceLength = ((float)Map.Size.z * 1.4f);
            Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);  //angle of beam
            Vector3 lanceVector = strikePos.ToVector3Shifted() + a * lanceLength * 0.5f;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(lanceVector, Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, lanceLength));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(lanceVector - (.5f * a * lanceWidth), Quaternion.Euler(0f, angle, 0f), new Vector3(lanceWidth, 1f, lanceWidth));  //drawer for beam start
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age <= duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
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
    }
}


