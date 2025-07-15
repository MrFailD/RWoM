using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

using Verse.Sound;
using TorannMagic.TMDefs;

namespace TorannMagic.Golems
{
    public class GolemAbilityUpgrade : IExposable
    {
        public float damageModifier;
        public float cooldownModifier;
        public float energyCostModifier;
        public float durationModifier;
        public float healingModifier;
        public float processingModifier;

        public virtual void ExposeData()
        {
            Scribe_Values.Look<float>(ref energyCostModifier, "energyCostModifier", 0f);
            Scribe_Values.Look<float>(ref cooldownModifier, "cooldownModifier", 0f);
            Scribe_Values.Look<float>(ref damageModifier, "damageModifier", 0f);
            Scribe_Values.Look<float>(ref durationModifier, "durationModifier", 0f);
            Scribe_Values.Look<float>(ref healingModifier, "healingModifier", 0f);
            Scribe_Values.Look<float>(ref processingModifier, "processingModifier", 0f);
        }        
    }
}
