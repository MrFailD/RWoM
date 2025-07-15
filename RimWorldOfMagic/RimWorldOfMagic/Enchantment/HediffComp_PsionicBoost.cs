using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_PsionicBoost : HediffComp_EnchantedItem
    {

        public float maxSeverity;

        public override void CompExposeData()
        {
            Scribe_Values.Look<float>(ref maxSeverity, "maxSeverity", 0, false);
            base.CompExposeData();            
        }

        public override void PostInitialize()
        {
            hediffActionRate = 300;            
        }

        public override void HediffActionTick()
        {
            float sensitivity = Pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            maxSeverity = Mathf.Clamp((sensitivity - 1) * 100, 0, 100);
            parent.Severity = maxSeverity;
        }

    }
}
