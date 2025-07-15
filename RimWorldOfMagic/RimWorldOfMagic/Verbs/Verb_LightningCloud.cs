using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using UnityEngine;


namespace TorannMagic
{
    public class Verb_LightningCloud : Verb_UseAbility
    {
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            TargetAoEProperties targetAoEProperties = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties;
            if (targetAoEProperties == null || !targetAoEProperties.showRangeOnSelect)
            {
                CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
                float adjustedRadius = verbProps.defaultProjectile?.projectile?.explosionRadius - 2f ?? 1f;
                if (comp != null && comp.MagicData != null)
                {
                    int verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, Ability.Def as TMAbilityDef);
                    adjustedRadius += verVal;
                }
                return adjustedRadius;
            }
            return (float)targetAoEProperties.range;
        }
    }
}
