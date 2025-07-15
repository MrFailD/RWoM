using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_EnchantedItem : HediffComp
    {
        public bool initialized;
        public bool removeNow;
        public Apparel enchantedItem;
        public Thing enchantedWeapon;

        public int checkActiveRate = 60;
        public int hediffActionRate = 1;

        public override void CompExposeData()
        {            
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref checkActiveRate, "checkActiveRate", 60, false);
            Scribe_Values.Look<int>(ref hediffActionRate, "hediffActionRate", 1, false);
            base.CompExposeData();
        }

        public string labelCap
        {
            get
            {
                if (Def.LabelCap != null)
                {
                    return Def.LabelCap;
                }
                else
                {
                    return "";
                }
            }
        }

        public string label
        {
            get
            {
                if (Def.label != null)
                {
                    return Def.label;
                }
                else
                {
                    return "";
                }
            }
        }


        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (enchantedItem != null && enchantedItem.def != null && enchantedItem.def.label != null)
                {
                    return enchantedItem.def.label;
                }
                else if(enchantedWeapon != null && enchantedWeapon.def != null && enchantedWeapon.def.label != null)
                {
                    return enchantedWeapon.def.label;
                }
                else
                {
                    return "";
                }
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                PostInitialize();
            }
        }

        public virtual void PostInitialize()
        {
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
            }
            if(Find.TickManager.TicksGame % checkActiveRate == 0)
            {
                if(CheckActiveApparel() && CheckActiveEquipment())
                {
                    removeNow = true;
                }                
            }
            if(hediffActionRate != 0 && Find.TickManager.TicksGame % hediffActionRate == 0)
            {
                HediffActionTick();
            }
        }
        
        public bool CheckActiveApparel()
        {
            bool remove = true;
            List<Apparel> apparel = Pawn.apparel.WornApparel;
            if (apparel != null)
            {
                if(apparel.Contains(enchantedItem))
                {
                    remove = false;
                }
            }
            return remove;
        }

        public bool CheckActiveEquipment()
        {
            bool remove = true;
            Thing primary = Pawn.equipment.Primary;
            if (primary != null && primary == enchantedWeapon)
            {                
                remove = false;                
            }
            return remove;
        }

        public override bool CompShouldRemove => base.CompShouldRemove || removeNow;

        public virtual void HediffActionTick()
        {
        }
    }
}
