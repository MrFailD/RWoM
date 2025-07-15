using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_Mind : HediffComp_EnchantedItem
    {

        public override void CompExposeData()
        {            
            base.CompExposeData();
        }

        public override void PostInitialize()
        {
            hediffActionRate = 1800;            
        }

        public override void HediffActionTick()
        {
            if (Pawn.mindState.mentalStateHandler.InMentalState && Rand.Chance(.08f))
            {
                Messages.Message("TM_BrokenOutOfMentalState".Translate(Pawn.LabelShort, Pawn.mindState.mentalStateHandler.CurState.def.label), Pawn, MessageTypeDefOf.PositiveEvent);
                Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();                
            }
        }

    }
}
