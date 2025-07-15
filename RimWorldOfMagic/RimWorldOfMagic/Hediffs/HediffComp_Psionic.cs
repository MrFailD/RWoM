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
    internal class HediffComp_Psionic : HediffComp
    {

        private bool initialized;
        private int pwrVal;
        private int effVal;
        private int verVal;

        private bool doPsionicAttack;
        private int ticksTillPsionicStrike;
        private int nextPsionicAttack;
        private Pawn threat;
        private CompAbilityUserMight comp;

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
            parent.Severity = 90f;
            FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 1f);
            DeterminePsionicHD();            
        }

        private void DeterminePsionicHD()
        {
            comp = Pawn.GetCompAbilityUserMight();
            if (comp != null && comp.MightData != null)
            {
                PwrVal = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicAugmentation.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicAugmentation_pwr").level;
                EffVal = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicAugmentation.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicAugmentation_eff").level;
                VerVal = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicAugmentation.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicAugmentation_ver").level;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn != null & parent != null && !Pawn.Dead)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
                base.CompPostTick(ref severityAdjustment);

                if (Find.TickManager.TicksGame % 60 == 0 && initialized)
                {
                    DeterminePsionicHD();
                    severityAdjustment += (Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false) * Rand.Range(.04f, .12f));
                    if (Find.Selector.FirstSelectedObject == Pawn)
                    {
                        HediffStage hediffStage = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_PsionicHD"), false).CurStage;
                        hediffStage.label = parent.Severity.ToString("0.00") + "%";
                    }

                    Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_Artifact_PsionicBoostHD);
                    float maxSev = 100;
                    if (hediff != null)
                    {
                        maxSev += hediff.Severity; 
                    }
                    parent.Severity = Mathf.Clamp(parent.Severity, 0, maxSev);

                }

                if (Pawn.Spawned && !Pawn.Downed && Pawn.Map != null && comp != null)
                {                    
                    if (doPsionicAttack)
                    {
                        ticksTillPsionicStrike--;
                        if (ticksTillPsionicStrike <= 0)
                        {
                            doPsionicAttack = false;
                            if (threat != null && !threat.Destroyed && !threat.Dead && !threat.Downed)
                            {
                                TM_MoteMaker.MakePowerBeamMotePsionic(threat.DrawPos.ToIntVec3(), threat.Map, 2f, 2f, .7f, .1f, .6f);
                                DamageInfo dinfo2 = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_PsionicInjury, Rand.Range(6, 12) * Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false) + (2 * VerVal), 0, -1, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, threat);
                                threat.TakeDamage(dinfo2);
                            }
                        }
                    }                    

                    if (comp.usePsionicAugmentationToggle && Pawn.drafter != null && Pawn.CurJob != null)
                    {
                        if (Find.TickManager.TicksGame % 600 == 0 && !Pawn.Drafted)
                        {
                            if (parent.Severity >= 95 && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null)
                            {
                                if ((Pawn.Position - Pawn.CurJob.targetA.Thing.Position).LengthHorizontal > 20 && (Pawn.Position - Pawn.CurJob.targetA.Thing.Position).LengthHorizontal < 300 && Pawn.CurJob.locomotionUrgency >= LocomotionUrgency.Jog && Pawn.CurJob.bill == null)
                                {
                                    parent.Severity -= 10f;
                                    if (EffVal == 0)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicSpeedHD"), 1f + .02f * EffVal);
                                    }
                                    else if (EffVal == 1)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicSpeedHD_I"), 1f + .02f * EffVal);
                                    }
                                    else if (EffVal == 2)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicSpeedHD_II"), 1f + .02f * EffVal);
                                    }
                                    else
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicSpeedHD_III"), 1f + .02f * EffVal);
                                    }
                                    for (int i = 0; i < 12; i++)
                                    {
                                        float direction = Rand.Range(0, 360);
                                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi, Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .4f), 0.2f, .02f, .1f, 0, Rand.Range(8, 10), direction, direction);
                                    }
                                    comp.MightUserXP += Rand.Range(10, 15);
                                }
                                if ((Pawn.Position - Pawn.CurJob.targetA.Thing.Position).LengthHorizontal < 2 && (Pawn.CurJob.bill != null || Pawn.CurJob.def.defName == "Sow" || Pawn.CurJob.def.defName == "FinishFrame" || Pawn.CurJob.def.defName == "Deconstruct" || Pawn.CurJob.def.defName == "Repair" || Pawn.CurJob.def.defName == "Clean" || Pawn.CurJob.def.defName == "Mine" || Pawn.CurJob.def.defName == "SmoothFloor" || Pawn.CurJob.def.defName == "SmoothWall" || Pawn.CurJob.def.defName == "Harvest" || Pawn.CurJob.def.defName == "HarvestDesignated" || Pawn.CurJob.def.defName == "CutPlant" || Pawn.CurJob.def.defName == "CutPlantDesignated"))
                                {
                                    parent.Severity -= 12f;
                                    if (PwrVal == 0)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicManipulationHD"), 1f + .02f * PwrVal);
                                    }
                                    else if (PwrVal == 1)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicManipulationHD_I"), 1f + .02f * PwrVal);
                                    }
                                    else if (PwrVal == 2)
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicManipulationHD_II"), 1f + .02f * PwrVal);
                                    }
                                    else
                                    {
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicManipulationHD_III"), 1f + .02f * PwrVal);
                                    }
                                    for (int i = 0; i < 12; i++)
                                    {
                                        float direction = Rand.Range(0, 360);
                                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi, Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .4f), 0.2f, .02f, .1f, 0, Rand.Range(8, 10), direction, direction);
                                    }
                                    comp.MightUserXP += Rand.Range(10, 15);
                                }                                
                            }
                        }

                        if (parent.Severity >= 20)
                        {
                            if (Find.TickManager.TicksGame % 180 == 0 && (Pawn.Drafted || !Pawn.IsColonist) && ((Pawn.equipment.Primary != null && !Pawn.equipment.Primary.def.IsRangedWeapon) || Pawn.equipment.Primary == null))
                            {
                                if (Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null && Pawn.CurJob.targetA.Thing is Pawn && Pawn.CurJobDef == JobDefOf.AttackMelee)
                                {
                                    //Log.Message("performing psionic dash - curjob " + this.Pawn.CurJob);
                                    //Log.Message("curjob def " + this.Pawn.CurJob.def.defName);
                                    //Log.Message("target " + this.Pawn.CurJob.targetA.Thing);
                                    //Log.Message("target range " + (this.Pawn.CurJob.targetA.Thing.Position - this.Pawn.Position).LengthHorizontal);
                                    Pawn targetPawn = Pawn.CurJob.targetA.Thing as Pawn;
                                    float targetDistance = (Pawn.Position - targetPawn.Position).LengthHorizontal;
                                    if (targetDistance > 3 && targetDistance < (12 + EffVal) && targetPawn.Map != null && !targetPawn.Downed)
                                    {
                                        for (int i = 0; i < 12; i++)
                                        {
                                            float direction = Rand.Range(0, 360);
                                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi, Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .4f), 0.2f, .02f, .1f, 0, Rand.Range(8, 10), direction, direction);
                                        }
                                        FlyingObject_PsionicLeap flyingObject = (FlyingObject_PsionicLeap)GenSpawn.Spawn(ThingDef.Named("FlyingObject_PsionicLeap"), Pawn.Position, Pawn.Map);
                                        flyingObject.Launch(Pawn, Pawn.CurJob.targetA.Thing, Pawn);
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicHD"), -3f);
                                        comp.Stamina.CurLevel -= .03f;
                                        comp.MightUserXP += Rand.Range(20, 30);
                                    }
                                }
                            }

                            if (nextPsionicAttack < Find.TickManager.TicksGame && Pawn.Drafted && comp.usePsionicMindAttackToggle)
                            {
                                if (Pawn.CurJob.def != TorannMagicDefOf.JobDriver_PsionicBarrier && VerVal > 0)
                                {
                                    threat = TM_Calc.FindNearbyEnemy(Pawn, 20 + (2 * verVal)); // GetNearbyTarget(20 + (2 * VerVal));
                                    if (threat != null)
                                    {
                                        //start psionic attack; ends after delay
                                        SoundInfo info = SoundInfo.InMap(new TargetInfo(Pawn.Position, Pawn.Map, false), MaintenanceType.None);
                                        TorannMagicDefOf.TM_Implosion.PlayOneShot(info);
                                        Effecter psionicAttack = TorannMagicDefOf.TM_GiantExplosion.Spawn();
                                        psionicAttack.Trigger(new TargetInfo(threat.Position, threat.Map, false), new TargetInfo(threat.Position, threat.Map, false));
                                        psionicAttack.Cleanup();
                                        for (int i = 0; i < 12; i++)
                                        {
                                            float direction = Rand.Range(0, 360);
                                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi, Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .4f), 0.2f, .02f, .1f, 0, Rand.Range(8, 10), direction, direction);
                                        }
                                        float weaponModifier = 1;
                                        if (Pawn.equipment.Primary != null)
                                        {
                                            if (Pawn.equipment.Primary.def.IsRangedWeapon)
                                            {
                                                StatModifier wpnMass = Pawn.equipment.Primary.def.statBases.FirstOrDefault((StatModifier x) => x.stat.defName == "Mass");
                                                weaponModifier = Mathf.Clamp(wpnMass.value, .8f, 6);
                                            }
                                            else //assume melee weapon
                                            {
                                                StatModifier wpnMass = Pawn.equipment.Primary.def.statBases.FirstOrDefault((StatModifier x) => x.stat.defName == "Mass");
                                                weaponModifier = Mathf.Clamp(wpnMass.value, .6f, 4);
                                            }
                                        }
                                        else //unarmed
                                        {
                                            weaponModifier = .4f;
                                        }
                                        nextPsionicAttack = Find.TickManager.TicksGame + (int)(Mathf.Clamp((600 - (60 * verVal)) * weaponModifier, 120, 900));
                                        float energyCost = Mathf.Clamp((10f - VerVal) * weaponModifier, 2f, 12f);
                                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicHD"), -energyCost);
                                        comp.MightUserXP += Rand.Range(8, 12);
                                        doPsionicAttack = true;
                                        ticksTillPsionicStrike = 24;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private float GetAngleFromTo(Vector3 from, Vector3 to)
        {
            Vector3 heading = (to - from);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            float directionAngle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
            return directionAngle;
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
