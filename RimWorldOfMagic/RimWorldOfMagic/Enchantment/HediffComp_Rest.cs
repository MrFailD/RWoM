using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_Rest : HediffComp_EnchantedItem
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
            Need rest = Pawn.needs.rest;
            if (rest != null)
            {
                rest.CurLevel += .0065f;               
            }
        }

    }
}
