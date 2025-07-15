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
    public class HediffComp_BloodShield : HediffComp
    {

        private bool initialized;
        private int initializeDelay;
        private bool removeNow;
        private bool woundsHealed;

        private int eventFrequency = 180;

        //private int bfbPwr = 0;  //increased amount blood levels affect ability power
        private int bloodshieldVer;  //increased blood per bleed rate and blood gift use

        private float lastSeverity;

        public Pawn linkedPawn;
        private Vector3 directionToLinkedPawn = default(Vector3);

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref linkedPawn, "linkedPawn", false);
            Scribe_Values.Look<int>(ref bloodshieldVer, "bloodshieldVer", 0, false);
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
            lastSeverity = parent.Severity;
            CompAbilityUserMagic comp = linkedPawn.GetCompAbilityUserMagic();
            if (spawned && comp != null && comp.IsMagicUser)
            {
                bloodshieldVer = comp.MagicData.MagicPowerSkill_BloodShield.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodShield_ver").level;
                //directionToLinkedPawn = TM_Calc.GetVector(this.Pawn.DrawPos, linkedPawn.DrawPos);
            }
            else
            {
                removeNow = true;
            }
        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null && initializeDelay > 1;
            if (flag)
            {
                if (!initialized && linkedPawn != null)
                {
                    initialized = true;
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    if(!linkedPawn.DestroyedOrNull() && !linkedPawn.Dead)
                    {

                        //directionToLinkedPawn = TM_Calc.GetVector(this.Pawn.DrawPos, linkedPawn.DrawPos);
                        float severityChange = lastSeverity - parent.Severity;
                        if (severityChange > 0)
                        {
                            HealWounds((severityChange / 2) * (1 + .15f *bloodshieldVer));
                            ReverseHealLinkedPawn(severityChange);
                        }
                        severityAdjustment -= Rand.Range(2.5f, 4f);
                        lastSeverity = parent.Severity;
                    }                    
                    else
                    {
                        removeNow = true;
                    }
                }
            }
            else
            {
                initializeDelay++;
            }
        }

        public void ReverseHealLinkedPawn(float severity)
        {
            Hediff bloodHediff = linkedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_BloodHD"), false);
            if(bloodHediff != null)
            {
                if (woundsHealed)
                {
                    if (bloodHediff.Severity < 1)
                    {
                        TM_Action.DamageEntities(linkedPawn, null, severity, TMDamageDefOf.DamageDefOf.TM_BloodBurn, Pawn);
                    }
                    else
                    {
                        bloodHediff.Severity -= severity / 3;
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist, linkedPawn.DrawPos, Pawn.Map, Rand.Range(.6f, .7f), .2f, 0.05f, 1f, Rand.Range(-50, 50), Rand.Range(1.5f, 2f), (Quaternion.AngleAxis(-90, Vector3.up) * directionToLinkedPawn).ToAngleFlat(), Rand.Range(0, 360));
                    }
                }
            }
            else
            {
                removeNow = true;
            }

            if (severity > 1.25f)
            {
                Effecter BloodShieldEffect = TorannMagicDefOf.TM_BloodShieldEffecter.Spawn();
                BloodShieldEffect.Trigger(new TargetInfo(linkedPawn.Position, linkedPawn.Map, false), new TargetInfo(linkedPawn.Position, linkedPawn.Map, false));
                BloodShieldEffect.Cleanup();
            }
        }

        public void HealWounds(float healAmount)
        {
            woundsHealed = false;
            Hediff_Injury injuryToHeal = Pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .FirstOrDefault(injury => injury.CanHealNaturally());
            if (injuryToHeal == null) return;
            injuryToHeal.Heal(healAmount);
            TM_MoteMaker.ThrowGenericMote(
                TorannMagicDefOf.Mote_BloodMist, Pawn.DrawPos, Pawn.Map,
                scale: Rand.Range(.5f, .75f),
                solidTime: .2f,
                fadeIn: 0.05f,
                fadeOut: 1f,
                rotationRate: Rand.Range(-50, 50),
                velocity: Rand.Range(.5f, .7f),
                velocityAngle: Rand.Range(30, 40),
                lookAngle: Rand.Range(0, 360)
            );
            woundsHealed = true;
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
