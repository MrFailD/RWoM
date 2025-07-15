using System;
using Verse;
using RimWorld;
using AbilityUser;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TorannMagic.Enchantment
{
    public class CompProperties_EnchantedItem : CompProperties, IExposable
    {
        public List<TMAbilityDef> MagicAbilities = new List<TMAbilityDef>();

        public Type AbilityUserClass;

        public bool hasEnchantment;
        public bool hasAbility = false;

        public EnchantmentTier maxMPTier;
        public EnchantmentTier mpRegenRateTier;
        public EnchantmentTier coolDownTier;
        public EnchantmentTier mpCostTier;
        public EnchantmentTier xpGainTier;
        public EnchantmentTier arcaneResTier;
        public EnchantmentTier arcaneDmgTier;

        //Magic Stats (%)
        public float maxMP;
        public float mpRegenRate;
        public float coolDown;
        public float mpCost;
        public float xpGain;

        public float arcaneRes;
        public float arcaneDmg;

        public float arcalleumCooldown;

        //Might Stats (%)

        //Common Stats (%)        

        public float healthRegenRate = 0;

        //Special Abilities
        public EnchantmentTier skillTier = EnchantmentTier.Skill;
        public bool arcaneSpectre;
        public bool phantomShift;

        public EnchantmentAction enchantedAction = new EnchantmentAction();

        //Hediffs
        public HediffDef hediff = null;
        public float hediffSeverity = 0f;
        public bool hediffStacks = false;
        public bool usesStackingHediff = true;

        //Thoughts
        public ThoughtDef enchantmentThought = null;

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref maxMP, "maxMP", 0, false);
            Scribe_Values.Look<float>(ref mpRegenRate, "mpRegenRateP", 0, false);
            Scribe_Values.Look<float>(ref coolDown, "coolDown", 0, false);
            Scribe_Values.Look<float>(ref mpCost, "mpCost", 0, false);
            Scribe_Values.Look<float>(ref xpGain, "xpGain", 0, false);
            Scribe_Values.Look<float>(ref arcaneRes, "arcaneRes", 0, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 0, false);
            Scribe_Values.Look<bool>(ref arcaneSpectre, "arcaneSpectre", false, false);
            Scribe_Values.Look<bool>(ref phantomShift, "phantomShift", false, false);
            Scribe_Values.Look<float>(ref arcalleumCooldown, "arcalleumCooldown", 0f, false);
            Scribe_Values.Look<EnchantmentTier>(ref maxMPTier, "maxMPTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref mpRegenRateTier, "mpRegenRateTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref coolDownTier, "coolDownTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref mpCostTier, "mpCostTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref xpGainTier, "xpGainTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref arcaneResTier, "arcaneResTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<EnchantmentTier>(ref arcaneDmgTier, "arcaneDmgTier", (EnchantmentTier)0, false);
            Scribe_Values.Look<bool>(ref hasEnchantment, "hasEnchantment", false, false);
        }

        public CompProperties_EnchantedItem()
        {
            compClass = typeof(CompEnchantedItem);
            AbilityUserClass = typeof(GenericCompAbilityUser);
        }
    }
}
