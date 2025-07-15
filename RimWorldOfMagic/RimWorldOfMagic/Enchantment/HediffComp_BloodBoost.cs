using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_BloodBoost : HediffComp_EnchantedItem
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
            float pawnDownCount = 0;
            float pawnKillCount = 0;
            if (Pawn.records != null)
            {
                pawnDownCount = Pawn.records.GetValue(RecordDefOf.PawnsDownedHumanlikes);
                pawnKillCount = Pawn.records.GetValue(RecordDefOf.KillsHumanlikes);
            }
            maxSeverity = Mathf.Min((2 * pawnKillCount) + pawnDownCount, 100);
            parent.Severity = maxSeverity;
        }

    }
}
