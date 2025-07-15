using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_EnchantedWeapon : HediffComp
    {

        private bool initialized;
        private bool removeNow;

        private int eventFrequency = 60;

        public Pawn enchanterPawn;
        public Thing enchantedWeapon;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref enchanterPawn, "enchanterPawn", false);
            Scribe_References.Look<Thing>(ref enchantedWeapon, "enchantedWeapon", false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
        }

        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            CompAbilityUserMagic comp = enchanterPawn.GetCompAbilityUserMagic();
            if (!spawned || enchanterPawn == null)
            {
                removeNow = true;
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
                    initialized = true;
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    if(!enchanterPawn.DestroyedOrNull() && !enchanterPawn.Dead)
                    {
                        CompAbilityUserMagic comp = enchanterPawn.GetCompAbilityUserMagic();
                        if(comp != null && comp.weaponEnchants != null && comp.weaponEnchants.Count >0)
                        {
                            bool isRegistered = false;
                            for(int i =0; i < comp.weaponEnchants.Count; i++)
                            {
                                if(comp.weaponEnchants[i] == Pawn)
                                {
                                    isRegistered = true;
                                }
                            }
                            if(!isRegistered)
                            {
                                removeNow = true;
                            }
                        }
                    }
                    else
                    {
                        removeNow = true;
                    }
                    
                    if(!enchantedWeapon.DestroyedOrNull() && Pawn.equipment.Primary != null && Pawn.equipment.Primary == enchantedWeapon)
                    {

                    }
                    else
                    {
                        removeNow = true;
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            if(enchanterPawn != null)
            {
                CompAbilityUserMagic comp = enchanterPawn.GetCompAbilityUserMagic();
                if(comp != null && comp.weaponEnchants != null && comp.weaponEnchants.Count > 0)
                {
                    if(comp.weaponEnchants.Contains(Pawn))
                    {
                        comp.weaponEnchants.Remove(Pawn);
                    }
                }
            }
            base.CompPostPostRemoved();
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || removeNow;
            }
        }        
    }
}
