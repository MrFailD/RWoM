using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;

namespace TorannMagic
{
    public class Verb_LightBurst : Verb_UseAbility  
    {
        private int verVal;
        private int pwrVal;
        private int burstCount = 2;
        private bool initialized;
        private float arcaneDmg = 1f;
        private float lightPotency = .5f;

        private void Initialize(Pawn pawn)
        {
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            //pwrVal = TM_Calc.GetMagicSkillLevel(pawn, comp.MagicData.MagicPowerSkill_LightBurst, "TM_LightBurst", "_pwr", TorannMagicDefOf.TM_LightBurst.canCopy);
            //verVal = TM_Calc.GetMagicSkillLevel(pawn, comp.MagicData.MagicPowerSkill_LightBurst, "TM_LightBurst", "_ver", TorannMagicDefOf.TM_LightBurst.canCopy);
            pwrVal = TM_Calc.GetSkillPowerLevel(pawn, Ability.Def as TMAbilityDef);
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, Ability.Def as TMAbilityDef);
            burstCount = 2;
            if (verVal >= 1)
            {
                burstCount++;
                if (verVal >= 3)
                {
                    burstCount++;
                }
            }
            arcaneDmg = comp.arcaneDmg;
            if (pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_LightCapacitanceHD))
            {
                HediffComp_LightCapacitance hd = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_LightCapacitanceHD).TryGetComp<HediffComp_LightCapacitance>();
                lightPotency = hd.LightPotency;
                hd.LightEnergy -= (2.5f * burstCount);
            }
        }

        protected override bool TryCastShot()
        {
            bool result = false;
            Pawn pawn = CasterPawn;
            if (!initialized)
            {
                initialized = true;
                Initialize(pawn);
            }

            Map map = CasterPawn.Map;
            IntVec3 targetVariation = currentTarget.Cell;
            float radius = (Ability.Def.MainVerb.TargetAoEProperties.range / 2f) + (.3f*pwrVal);
            targetVariation.x += Mathf.RoundToInt(Rand.Range(-radius, radius));
            targetVariation.z += Mathf.RoundToInt(Rand.Range(-radius, radius));
            CreateLightBurst(targetVariation, map, radius);
            ApplyEffects(targetVariation, map, radius);
            burstCount--;
            result = burstCount > 0;           
            return result;
        }

        public void CreateLightBurst(IntVec3 center, Map map, float radius)
        {
            GenClamor.DoClamor(CasterPawn, 2*radius, ClamorDefOf.Ability);
            Effecter flashED = TorannMagicDefOf.TM_LightBurstED.Spawn();
            flashED.Trigger(new TargetInfo(center, map, false), new TargetInfo(center, map, false));
            flashED.Cleanup();
            TargetInfo ti = new TargetInfo(center, map, false);
            TM_MoteMaker.MakeOverlay(ti, TorannMagicDefOf.TM_Mote_PsycastAreaEffect, map, Vector3.zero, .1f, 0f, .05f, .1f, .1f, 4.3f);
        }

        public void ApplyEffects(IntVec3 center, Map map, float radius)
        {
            List<Pawn> targets = TM_Calc.FindAllPawnsAround(map, center, radius);
            float baseDamage = 2f * lightPotency * arcaneDmg;
            if(targets != null && targets.Count > 0)
            {
                for(int i =0; i < targets.Count; i++)
                {
                    Pawn p = targets[i];                    
                    bool flag = false;                    
                    flag = p.RaceProps.IsFlesh || (p.RaceProps.IsMechanoid && pwrVal >= 2);
                    if(flag)
                    {
                        List<BodyPartRecord> bpr = p.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.SightSource).ToList();
                        float distanceToCenter = (p.Position - center).LengthHorizontal;
                        float distanceMultiplier = radius - distanceToCenter;
                        if(p.Faction != null && p.Faction == CasterPawn.Faction)
                        {
                            distanceMultiplier *= .5f;
                        }
                        if (bpr != null && bpr.Count >= 0)
                        {
                            for (int j = 0; j < bpr.Count; j++)
                            {
                                TM_Action.DamageEntities(p, bpr[j], distanceMultiplier * (baseDamage + (.3f * pwrVal)), TMDamageDefOf.DamageDefOf.TM_BurningLight, CasterPawn);                                
                                HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_LightBurstHD, distanceMultiplier * lightPotency * (.1f + (.015f * pwrVal)));
                            }
                        }
                    }
                }
            }
        }
    }
}
