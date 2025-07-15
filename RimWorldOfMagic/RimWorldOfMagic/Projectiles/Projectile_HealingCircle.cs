using Verse;
using RimWorld;
using AbilityUser;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_HealingCircle : Projectile_AbilityBase
    {
        private int pwrVal;
        private int verVal;
        private float arcaneDmg = 1;
        private int age = -1;
        private int duration = 1200;
        private float radius = 6;
        private bool initialized;
        private int healDelay = 40;
        private int waveDelay = 300;
        private int lastHeal;
        private int lastWave;
        private Pawn caster;
        private float angle;
        private float innerRing;
        private float outerRing;
        private float ringFrac;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1200, false);
            Scribe_Values.Look<int>(ref healDelay, "healDelay", 30, false);
            Scribe_Values.Look<int>(ref lastHeal, "lastHeal", 0, false);
            Scribe_Values.Look<int>(ref waveDelay, "waveDelay", 240, false);
            Scribe_Values.Look<int>(ref lastWave, "lastWave", 0, false);
            Scribe_Values.Look<float>(ref radius, "radius", 6, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
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
            base.Impact(hitThing);
            ThingDef def = this.def;
            caster = launcher as Pawn;

            if(!initialized)
            {
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_HealingCircle.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_HealingCircle_pwr");
                MagicPowerSkill ver = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_HealingCircle.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_HealingCircle_ver");
                pwrVal = pwr.level;
                verVal = ver.level;
                if (caster.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                arcaneDmg = comp.arcaneDmg;
                
                if (!caster.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                radius = this.def.projectile.explosionRadius;
                angle = Rand.Range(-12, 12);
                duration = duration + (180 * (pwrVal + verVal));
                //TM_MoteMaker.MakePowerBeamMote(base.Position, base.Map, this.radius * 8f, 1.2f, this.duration/60f);
                initialized = true;
            }

            if (age >= lastWave)
            {
                if (age >= lastHeal + healDelay)
                {
                    switch (ringFrac)
                    {
                        case 0:
                            innerRing = 0;
                            outerRing = radius * ((ringFrac + .1f) / 5f);
                            TM_MoteMaker.MakePowerBeamMote(Position, Map, radius * 6f, 1.2f, waveDelay / 60f, (healDelay * 3f) / 60f, (healDelay * 2f) / 60f);
                            break;
                        case 1:
                            innerRing = outerRing;
                            outerRing = radius * ((ringFrac) / 5f);                            
                            break;
                        case 2:
                            innerRing = outerRing;
                            outerRing = radius * ((ringFrac) / 5f);
                            break;
                        case 3:
                            innerRing = outerRing;
                            outerRing = radius * ((ringFrac) / 5f);
                            break;
                        case 4:
                            innerRing = outerRing;
                            outerRing = radius * ((ringFrac) / 5f);
                            break;
                        case 5:
                            innerRing = outerRing;
                            outerRing = radius;
                            lastWave = age + waveDelay;
                            break;
                    }                    
                    ringFrac++;
                    lastHeal = age;
                    Search(map);
                }
                if (ringFrac >= 6)
                {
                    ringFrac = 0;
                }
            }       
        }

        public void Search(Map map)
        {
            IntVec3 curCell;
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(Position, radius, true);
            IEnumerable<IntVec3> innerCircle = GenRadial.RadialCellsAround(Position, innerRing, true);
            IEnumerable<IntVec3> outerCircle = GenRadial.RadialCellsAround(Position, outerRing, true);

            for (int i = innerCircle.Count(); i < outerCircle.Count(); i++)
            {                
                Pawn pawn = null;                
                curCell = targets.ToArray<IntVec3>()[i];
                if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                {
                    pawn = curCell.GetFirstPawn(map);
                }
                if (pawn != null && (pawn.Faction == caster.Faction || pawn.IsPrisoner || pawn.Faction == null || (pawn.Faction != null && !pawn.Faction.HostileTo(caster.Faction))) && !TM_Calc.IsUndead(pawn))
                {
                    Heal(pawn);
                }
                if(pawn != null && TM_Calc.IsUndead(pawn))
                {
                    TM_Action.DamageUndead(pawn, (10.0f + (float)pwrVal * 3f) * arcaneDmg, launcher);
                }
            }
        }

        public void Heal(Pawn pawn)
        {
            if (pawn == null || pawn.Dead) return;

            IEnumerable<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury => injury.CanHealNaturally())
                .Take(1 + verVal);

            float healAmount = (10.0f + pwrVal * 2f) * arcaneDmg;
            foreach (Hediff_Injury injury in injuries)
            {
                if (!Rand.Chance(.8f)) continue;  // 80% chance to heal, 20% chance to skip

                injury.Heal(healAmount);
                TM_MoteMaker.ThrowRegenMote(pawn.Position.ToVector3Shifted(), pawn.Map, 1.2f);
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


