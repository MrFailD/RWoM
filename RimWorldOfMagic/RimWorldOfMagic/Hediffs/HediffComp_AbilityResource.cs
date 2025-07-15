using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TorannMagic
{
    public class HediffCompProperties_AbilityResource : HediffCompProperties
    {
        public float maximumBase = 100f;
        public float maximumPerUpgrade = 0f;
        public string maximumUpgradeName;
        public float regenPerTickBase = 0f;
        public float regenPerTickPerUpgrade = 0f;
        public string regenPerTickUpgradeName;

        public TMAbilityDef linkedAbility;

        public HediffCompProperties_AbilityResource()
        {
            compClass = typeof(HediffComp_AbilityResource);
        }

    }

    [StaticConstructorOnStartup]
    public class HediffComp_AbilityResource : HediffComp
    {

        private bool initialized;
        private bool removeNow = false;

        private int eventFrequency = 300;

        private HediffCompProperties_AbilityResource Props { get => props as HediffCompProperties_AbilityResource; }

        private string maximumCachedUpgradeName;
        private float maximumCached;
        private string regenPerTickCachedUpgradeName;
        private float regenPerTickCached;

        public override string CompLabelInBracketsExtra => string.Concat(parent.Severity.ToString("0."), "/", maximumCached.ToString("0."));
        public override bool CompShouldRemove => removeNow || base.CompShouldRemove;

        private void Initialize()
        {
            UpdateCachedValues();
            initialized = true;
        }

        private void UpdateCachedValues()
        {
            CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
            if (comp != null)
            {
                int lvlMax = 0;
                MightPowerSkill mps = TM_ClassUtility.GetMightPowerSkillFromLabel(comp, Props.maximumUpgradeName);
                if (mps != null)
                {
                    lvlMax = mps.level;
                }
                maximumCached = Props.maximumBase + (Props.maximumPerUpgrade * lvlMax);
                int lvlRegen = 0;
                mps = TM_ClassUtility.GetMightPowerSkillFromLabel(comp, Props.regenPerTickUpgradeName);
                if (mps != null)
                {
                    lvlRegen = mps.level;
                }
                regenPerTickCached = Props.regenPerTickBase + (Props.regenPerTickPerUpgrade * lvlRegen);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null;
            if (flag)
            {
                if (!initialized)
                {
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 3)
                {
                    UpdateCachedValues();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 3)
                {
                    float newValue = parent.Severity + eventFrequency * regenPerTickCached;
                    parent.Severity = Mathf.Clamp(newValue, 0f, maximumCached);
                }
            }
        }
    }
}
