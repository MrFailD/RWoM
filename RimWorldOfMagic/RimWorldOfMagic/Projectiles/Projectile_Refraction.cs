using Verse;
using RimWorld;
using AbilityUser;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_Refraction : Projectile_AbilityBase
    {
        private bool initialized;
        private bool wallActive;
        private float wallLength;
        private int age = -1;
        private int duration = 300;
        private Vector3 wallStart = default(Vector3);
        private Vector3 wallDir = default(Vector3);
        private Vector3 wallEnd = default(Vector3);
        private List<IntVec3> wallPositions = new List<IntVec3>();
        private LocalTargetInfo secondTarget = null;
        private Pawn caster;
        public static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        private int pwrVal;
        private int verVal;
        private float wallEnergy = 1500f;

        //unsaved variables
        private int wallLengthMax = 20;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref wallActive, "wallActive", false, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 300, false);
            Scribe_Values.Look<float>(ref wallLength, "wallLength", 0, false);
            Scribe_Values.Look<float>(ref wallEnergy, "wallEnergy", 1500f, false);
            Scribe_Values.Look<Vector3>(ref wallStart, "wallStart", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref wallDir, "wallDir", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref wallEnd, "wallEnd", default(Vector3), false);
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
            Scribe_Collections.Look<IntVec3>(ref wallPositions, "wallPositions", LookMode.Value);
        }

        public void BeginTargetingWithVerb(TMAbilityDef verbToAdd, TargetingParameters targetParams, Action<LocalTargetInfo> action, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
        {
            AccessTools.Field(typeof(Targeter), "action").SetValue(Find.Targeter, action);
            AccessTools.Field(typeof(Targeter), "targetParams").SetValue(Find.Targeter, targetParams);
            AccessTools.Field(typeof(Targeter), "caster").SetValue(Find.Targeter, caster);
            AccessTools.Field(typeof(Targeter), "actionWhenFinished").SetValue(Find.Targeter, actionWhenFinished);
            AccessTools.Field(typeof(Targeter), "mouseAttachment").SetValue(Find.Targeter, mouseAttachment);
        }

        private void GetSecondTarget()
        {
            Find.Targeter.StopTargeting();
            BeginTargetingWithVerb(TorannMagicDefOf.TM_CompVerb, TorannMagicDefOf.TM_CompVerb.MainVerb.targetParams, delegate (LocalTargetInfo info)
            {
                secondTarget = info;
            }, caster, null, null);
        }

        private void GetWallCells()
        {
            wallPositions = new List<IntVec3>();
            wallPositions.Clear();
            wallLength = Mathf.RoundToInt(Mathf.Clamp(wallLength, 1, wallLengthMax));
            IntVec3 cell = default(IntVec3);
            for(int i = 0; i < wallLength; i++)
            {
                cell = (wallStart + (wallDir * i)).ToIntVec3();
                if (!wallPositions.Contains(cell))
                {
                    wallPositions.Add(cell);
                }
            }
        }

        private void Initialize()
        {
            if (caster != null)
            {
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                //pwrVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_Refraction, "TM_Refraction", "_pwr", TorannMagicDefOf.TM_Refraction.canCopy);
                //verVal = TM_Calc.GetMagicSkillLevel(caster, comp.MagicData.MagicPowerSkill_Refraction, "TM_Refraction", "_ver", TorannMagicDefOf.TM_Refraction.canCopy);
                pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_Refraction);
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_Refraction);
                if (caster.health.hediffSet.HasHediff(TorannMagicDefOf.TM_LightCapacitanceHD))
                {
                    HediffComp_LightCapacitance hd = caster.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_LightCapacitanceHD).TryGetComp<HediffComp_LightCapacitance>();
                    wallEnergy *= hd.LightPotency;
                    hd.LightEnergy -= 20f;
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (!initialized)
            {
                caster = launcher as Pawn;
                Initialize();
                GetSecondTarget();
                initialized = true;
            }

            if (caster != null && !caster.DestroyedOrNull() && !caster.Dead)
            {
                if (!wallActive && secondTarget != null)
                {
                    age = 0;
                    duration = 1500 + (verVal * 150);
                    wallActive = true;
                    wallStart = Position.ToVector3Shifted();
                    wallEnd = secondTarget.Cell.ToVector3Shifted();
                    wallDir = TM_Calc.GetVector(wallStart, wallEnd);
                    wallLength = (wallEnd - wallStart).magnitude;
                    secondTarget = null;
                    GetWallCells();
                }

                if (!wallActive)
                {
                    if (Find.TickManager.TicksGame % 6 == 0)
                    {
                        FleckMaker.ThrowDustPuff(Position, caster.Map, Rand.Range(.6f, .9f));
                    }
                }
                else
                {
                    if (Map != null)
                    {
                        if (Find.TickManager.TicksGame % 2 == 0)
                        {
                            RefractProjectiles();
                        }
                    }
                }
            }
            else
            {
                age = duration;
            }
        }

        public void RefractProjectiles()
        {
            float eMissVar = .1f + (.015f * pwrVal);
            float fMissVar = .05f - (.01f * pwrVal);
            for (int k = 0; k < wallPositions.Count; k++)
            {
                List<Thing> cellList = wallPositions[k].GetThingList(Map);
                for (int i = 0; i < cellList.Count; i++)
                {
                    Thing t = cellList[i];
                    if (t is Projectile proj && t.def.defName != "Projectile_Refraction")
                    {
                        IntVec3 projOrigin = Traverse.Create(root: proj).Field(name: "origin").GetValue<Vector3>().ToIntVec3();

                        if (proj.Launcher != null && (proj.Launcher.Faction != null || proj.def.defName == "Projectile_LightLaser") && !proj.def.projectile.flyOverhead && !wallPositions.Contains(projOrigin))
                        {
                            if (proj.Launcher.Faction != caster.Faction)
                            {
                                Vector3 displayEffect = wallPositions[k].ToVector3Shifted();
                                displayEffect.x += Rand.Range(-.2f, .2f);
                                displayEffect.z += Rand.Range(-.2f, .2f);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_LightBarrier, displayEffect, Map, .4f, .2f, .05f, .2f, 0, 0, 0, 0);
                                IntVec3 targetVec = Traverse.Create(root: proj).Field(name: "destination").GetValue<Vector3>().ToIntVec3();
                                float targetRange = (Position - targetVec).LengthHorizontal;
                                LocalTargetInfo initialTarget = proj.intendedTarget;
                                targetVec.x += Mathf.RoundToInt(Rand.Range(-eMissVar, eMissVar) * targetRange);
                                targetVec.z += Mathf.RoundToInt(Rand.Range(-eMissVar, eMissVar) * targetRange);
                                TM_CopyAndLaunchProjectile.CopyAndLaunchThingFromPosition(proj.def, proj.Launcher, wallPositions[k], Map, targetVec, intendedTarget, ProjectileHitFlags.All, null);
                                proj.Destroy(DestroyMode.Vanish);
                                wallEnergy -= proj.def.projectile.GetDamageAmount(this);
                            }
                            else
                            {
                                Vector3 displayEffect = wallPositions[k].ToVector3Shifted();
                                displayEffect.x += Rand.Range(-.2f, .2f);
                                displayEffect.z += Rand.Range(-.2f, .2f);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_LightBarrier, displayEffect, Map, .4f, .2f, .05f, .2f, 0, 0, 0, 0);
                                IntVec3 targetCell = proj.intendedTarget.Cell;
                                float targetRange = (Position - targetCell).LengthHorizontal;
                                LocalTargetInfo initialTarget = proj.intendedTarget;
                                targetCell.x += Mathf.RoundToInt(Rand.Range(-fMissVar, fMissVar) * targetRange);
                                targetCell.z += Mathf.RoundToInt(Rand.Range(-fMissVar, fMissVar) * targetRange);
                                TM_CopyAndLaunchProjectile.CopyAndLaunchThingFromPosition(proj.def, proj.Launcher, wallPositions[k], Map, targetCell, initialTarget, ProjectileHitFlags.All);
                                wallEnergy -= proj.def.projectile.GetDamageAmount(this);
                            }
                        }
                    }
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (wallActive)
            {
                float altitude = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);          //graphic depth
                float wallWidth = Mathf.Clamp(1f * (wallEnergy/1000f), .3f, 2f);
                Vector3 pos = wallStart + (wallDir * wallLength * .5f);
                pos.y = altitude;
                float angle = Vector3Utility.AngleFlat(wallDir);
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(wallWidth, altitude, wallLength));   //drawer for wall shell
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_MatPool.LightBarrier, 0);
                if (Rand.Chance(.5f))
                {
                    angle += 180;
                }
                Matrix4x4 matrix2 = new Matrix4x4();
                Vector3 pulsePos = pos;
                pulsePos.x += Rand.Range(-.025f, .025f);
                pulsePos.z += Rand.Range(0.025f, .025f);
                matrix2.SetTRS(pulsePos, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(wallWidth * 1.3f, altitude, wallLength * .95f));   //drawer for internal pulse
                Graphics.DrawMesh(MeshPool.plane10, matrix2, TM_MatPool.chiLightning, 0, null, 0, MatPropertyBlock);         
            }
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
    }
}


