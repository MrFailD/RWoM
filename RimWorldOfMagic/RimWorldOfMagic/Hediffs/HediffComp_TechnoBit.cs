using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_TechnoBit : HediffComp
    {

        private bool initialized;
        private int pwrVal;
        private int effVal;
        private int verVal;

        private int ticksBitWorking;
        private int nextBitEffect;
        private int nextBitGrenade;
        private int nextBitShock = 0;
        private int bitGrenadeCount;
        private Vector3 moteLoc = Vector3.zero;

        public int PwrVal
        {
            get
            {
                return pwrVal;
            }
            set
            {
                pwrVal = value;
            }
        }

        public int EffVal
        {
            get
            {
                return effVal;
            }
            set
            {
                effVal = value;
            }
        }

        public int VerVal
        {
            get
            {
                return verVal;
            }
            set
            {
                verVal = value;
            }
        }

        public string LabelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string Label
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
                parent.Severity = 90f;
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 1f);
                DetermineHDRank();
            }
        }

        private void DetermineHDRank()
        {
            CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
            PwrVal = comp.MagicData.MagicPowerSkill_TechnoBit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoBit_pwr").level;
            EffVal = comp.MagicData.MagicPowerSkill_TechnoBit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoBit_eff").level;
            VerVal = comp.MagicData.MagicPowerSkill_TechnoBit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoBit_ver").level;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.Spawned && !Pawn.Dead && !Pawn.Downed)
            {
                base.CompPostTick(ref severityAdjustment);
                if (Pawn != null & parent != null)
                {
                    if (!initialized)
                    {
                        initialized = true;
                        Initialize();
                    }
                }

                CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();

                if (ticksBitWorking > 0 && nextBitEffect < Find.TickManager.TicksGame && moteLoc != Vector3.zero)
                {
                    Vector3 rndVec = moteLoc;
                    rndVec.x += Rand.Range(-.15f, .15f);
                    rndVec.z += Rand.Range(-.15f, .15f);
                    TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.SparkFlash, rndVec, Pawn.Map, Rand.Range(.9f, 1.5f), .05f, 0f, .1f, 0, 0f, 0f, 0f);
                    rndVec = moteLoc;
                    rndVec.x += Rand.Range(-.15f, .15f);
                    rndVec.z += Rand.Range(-.15f, .15f);
                    TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.SparkFlash, rndVec, Pawn.Map, Rand.Range(.6f, 1.1f), .05f, 0f, .1f, 0, 0f, 0f, 0f);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Enchanting, comp.bitPosition, Pawn.Map, Rand.Range(0.35f, 0.43f), .2f, .05f, Rand.Range(.4f, .6f), Rand.Range(-200, 200), 0, 0, 0);
                    ticksBitWorking--;
                    nextBitEffect = Find.TickManager.TicksGame + Rand.Range(6, 10);
                    if(ticksBitWorking == 0)
                    {
                        moteLoc = Vector3.zero;
                    }
                }

                if (comp.useTechnoBitToggle)
                {
                    if(Find.TickManager.TicksGame % 60 == 0)
                    {
                        DetermineHDRank();
                    }
                    if (Find.TickManager.TicksGame % 600 == 0 && !Pawn.Drafted)
                    {
                        if (comp.Mana.CurLevelPercentage >= .9f && comp.Mana.CurLevel >= (.06f - (.001f * VerVal)) && Pawn.CurJob.targetA.Thing != null)
                        {                                                       
                            if (Pawn.CurJob.targetA.Thing != null)
                            {
                                if((Pawn.Position - Pawn.CurJob.targetA.Thing.Position).LengthHorizontal < 2 && (Pawn.CurJob.bill != null || Pawn.CurJob.def.defName == "FinishFrame" || Pawn.CurJob.def.defName == "Deconstruct" || Pawn.CurJob.def.defName == "Repair" || Pawn.CurJob.def.defName == "Mine" || Pawn.CurJob.def.defName == "SmoothFloor" || Pawn.CurJob.def.defName == "SmoothWall"))
                                {
                                    comp.Mana.CurLevel -= (.05f - (.001f * VerVal));
                                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_BitAssistHD"), .5f + 1f * VerVal);                                                                      
                                    comp.MagicUserXP += Rand.Range(6, 8);
                                    ticksBitWorking = 8;
                                    moteLoc = Pawn.CurJob.targetA.Thing.DrawPos;
                                }
                            }
                        }
                    }

                    if(comp.useTechnoBitRepairToggle && Find.TickManager.TicksGame % (160 - 3 * EffVal) == 0 && Pawn.Drafted && comp.Mana.CurLevel >= (.03f - .0006f * EffVal))
                    {
                        Thing damagedThing = TM_Calc.FindNearbyDamagedThing(Pawn, Mathf.RoundToInt(5 + .33f * EffVal));
                        if (damagedThing != null)
                        {
                            Building repairBuilding = damagedThing as Building;
                            if (repairBuilding != null)
                            {
                                repairBuilding.HitPoints = Mathf.Min(Mathf.RoundToInt(repairBuilding.HitPoints + (Rand.Range(8, 12) + (.5f * EffVal))), repairBuilding.MaxHitPoints);
                                comp.Mana.CurLevel -= (.03f - .0006f * EffVal);
                                comp.MagicUserXP += Rand.Range(4, 5);
                                ticksBitWorking = 8;
                                moteLoc = repairBuilding.DrawPos;
                            }
                            Pawn damagedRobot = damagedThing as Pawn;
                            if (damagedRobot != null)
                            {
                                TM_Action.DoAction_HealPawn(Pawn, damagedRobot, 1, (4 + .4f * EffVal));
                                comp.Mana.CurLevel -= (.03f - .0006f * EffVal);
                                comp.MagicUserXP += Rand.Range(4, 6);
                                ticksBitWorking = 5;
                                moteLoc = damagedRobot.DrawPos;
                            }
                        }
                    }

                    if (comp.useTechnoBitRepairToggle && Find.TickManager.TicksGame % (600 - 6 * EffVal) == 0 && !Pawn.Drafted && comp.Mana.CurLevel >= .05f)
                    {
                        Thing damagedThing = TM_Calc.FindNearbyDamagedThing(Pawn, Mathf.RoundToInt(10 + .5f * EffVal));
                        Building repairBuilding = damagedThing as Building;
                        if (repairBuilding != null)
                        {
                            repairBuilding.HitPoints = Mathf.Min(repairBuilding.HitPoints + (25 + (2*EffVal)), repairBuilding.MaxHitPoints);
                            comp.Mana.CurLevel -= (.05f - .0008f * EffVal);
                            comp.MagicUserXP += Rand.Range(9, 11);
                            ticksBitWorking = 8;
                            moteLoc = repairBuilding.DrawPos;
                        }
                        Pawn damagedRobot = damagedThing as Pawn;
                        if (damagedRobot != null)
                        {
                            TM_Action.DoAction_HealPawn(Pawn, damagedRobot, 2, (8+.4f * EffVal));
                            comp.Mana.CurLevel -= (.05f - .0008f * EffVal);
                            comp.MagicUserXP += Rand.Range(9, 11);
                            ticksBitWorking = 5;
                            moteLoc = damagedRobot.DrawPos;
                        }
                    }

                    if (comp.Mana.CurLevel >= .1f && (Pawn.Drafted || !Pawn.IsColonist))
                    {
                        if (Pawn.TargetCurrentlyAimingAt != null && (Pawn.CurJob.def.defName == "AttackStatic" || Pawn.CurJob.def.defName == "Wait_Combat") && nextBitGrenade < Find.TickManager.TicksGame) 
                        {
                            float maxRange = 25 + PwrVal;
                            Thing targetThing = Pawn.TargetCurrentlyAimingAt.Thing;
                            float targetDistance = (Pawn.Position - targetThing.Position).LengthHorizontal;
                            float acc = 15f + (PwrVal / 3f);
                            if (TM_Calc.HasLoSFromTo(Pawn.Position, Pawn.TargetCurrentlyAimingAt.Thing, Pawn as Thing, 2f, maxRange) && targetThing.Map != null && bitGrenadeCount > 0)
                            {                              
                                IntVec3 rndTargetCell = targetThing.Position;
                                rndTargetCell.x += Mathf.RoundToInt(Rand.Range(-targetDistance / acc, targetDistance / acc)); //grenades were 8
                                rndTargetCell.z += Mathf.RoundToInt(Rand.Range(-targetDistance / acc, targetDistance / acc));
                                LocalTargetInfo ltiTarget = rndTargetCell;
                                //if (this.bitGrenadeCount == 2)
                                //{
                                //    //launch emp grenade
                                //    Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Projectile_TMEMPGrenade"), this.Pawn.Position, this.Pawn.Map, WipeMode.Vanish);
                                //    float launchAngle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(this.Pawn.Position, ltiTarget.Cell)).ToAngleFlat();
                                //    for (int m = 0; m < 4; m++)
                                //    {
                                //        TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, comp.bitPosition, this.Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), Rand.Range(-20, 20), Rand.Range(3f, 5f), launchAngle += Rand.Range(-25, 25), Rand.Range(0, 360));
                                //    }
                                //    SoundInfo info = SoundInfo.InMap(new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), MaintenanceType.None);
                                //    info.pitchFactor = 2f;
                                //    info.volumeFactor = .6f;
                                //    SoundDef.Named("Mortar_LaunchA").PlayOneShot(info);
                                //    projectile.def.projectile.speed = 20 + PwrVal;
                                //    projectile.def.projectile.explosionDelay = Rand.Range(80, 120) - (4 * PwrVal);                                    
                                //    projectile.Launch(this.Pawn, comp.bitPosition, ltiTarget, targetThing, ProjectileHitFlags.All, null, null);
                                //}
                                //else
                                //{
                                //    //fire he grenade
                                //    Projectile projectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Projectile_TMFragGrenade"), this.Pawn.Position, this.Pawn.Map, WipeMode.Vanish);
                                //    float launchAngle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(this.Pawn.Position, ltiTarget.Cell)).ToAngleFlat();
                                //    for (int m = 0; m < 4; m++)
                                //    {
                                //        TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, comp.bitPosition, this.Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), Rand.Range(-20, 20), Rand.Range(3f, 5f), launchAngle += Rand.Range(-25, 25), Rand.Range(0, 360));
                                //    }
                                //    SoundInfo info = SoundInfo.InMap(new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), MaintenanceType.None);
                                //    info.pitchFactor = 1.4f;
                                //    info.volumeFactor = .5f;
                                //    SoundDef.Named("Mortar_LaunchA").PlayOneShot(info);
                                //    projectile.def.projectile.speed = 20 + PwrVal;
                                //    projectile.def.projectile.explosionDelay = Rand.Range(80, 120) - (4 * PwrVal);
                                //    projectile.Launch(this.Pawn, comp.bitPosition, ltiTarget, targetThing, ProjectileHitFlags.All, null, null);
                                //}
                                Projectile p = (Projectile)(GenSpawn.Spawn(ThingDef.Named("Projectile_TM_BitTechLaser"), Pawn.Position, Pawn.Map, WipeMode.Vanish));
                                //float launchAngle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(this.Pawn.Position, ltiTarget.Cell)).ToAngleFlat();
                                
                                SoundInfo info = SoundInfo.InMap(new TargetInfo(Pawn.Position, Pawn.Map, false), MaintenanceType.None);
                                info.pitchFactor = 1.5f;
                                info.volumeFactor = .9f;
                                SoundDef.Named("Shot_ChargeBlaster").PlayOneShot(info);
                                
                                if (rndTargetCell == targetThing.Position)
                                {
                                    p.Launch(Pawn, comp.bitPosition, targetThing, targetThing, ProjectileHitFlags.IntendedTarget, false, null, null);
                                }
                                else
                                {
                                    p.Launch(Pawn, comp.bitPosition, ltiTarget, targetThing, ProjectileHitFlags.All, false, null, null);
                                }
                                nextBitGrenade = 3 + Find.TickManager.TicksGame;
                                bitGrenadeCount--;
                                if (bitGrenadeCount == 0)
                                {
                                    bitGrenadeCount = 3 + (int)((PwrVal) / 5);
                                    nextBitGrenade = Find.TickManager.TicksGame + (180 - 3*PwrVal);
                                    comp.Mana.CurLevel -= (.06f - (.001f * PwrVal));
                                    comp.MagicUserXP += Rand.Range(8, 12);
                                }
                            }
                            else if (nextBitGrenade < Find.TickManager.TicksGame && bitGrenadeCount <= 0)
                            {
                                bitGrenadeCount = 3 + (int)((PwrVal) / 5);
                                nextBitGrenade = Find.TickManager.TicksGame + (180 - 3 * PwrVal);
                            }
                        }                        
                    }
                }
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref effVal, "effVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
        }       
    }
}
