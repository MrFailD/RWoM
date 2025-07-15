using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_HateBoost : HediffComp_EnchantedItem
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
            float damageTaken = 0;
            float damageDealt = 0;
            if (Pawn.records != null)
            {
                damageTaken = Pawn.records.GetValue(RecordDefOf.DamageTaken);
                damageDealt = Pawn.records.GetValue(RecordDefOf.DamageDealt);
            }
            maxSeverity = Mathf.Min((damageDealt / 100) + (damageTaken / 10), 100);
            parent.Severity = maxSeverity;
        }

    }
}
