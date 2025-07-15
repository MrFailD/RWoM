using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using TorannMagic.Utils;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Provisioner : HediffComp
    {

        private bool initializing = true;
        private int nextTickAction;

        public int verVal;
        public int pwrVal;
        public int duration = 1;

        private bool removeNow;

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1, false);
            base.CompExposeData();
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
            if (spawned)
            {

            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
            }
            if(Find.TickManager.TicksGame >= nextTickAction)
            {
                duration--;                
                nextTickAction = Find.TickManager.TicksGame + Rand.Range(600, 700);
                if (pwrVal >= 1 && Rand.Chance(.2f + (.04f * pwrVal)))
                {
                    TickActionGear();
                }
                if (pwrVal >= 2)
                {
                    TickActionHealth();
                }
                if (duration <= 0)
                {
                    removeNow = true;
                }
            }
        }

        public void TickActionGear()
        {
            List<Apparel> gear = Pawn.apparel.WornApparel;
            for(int i = 0; i < gear.Count; i++)
            {
                if(gear[i].HitPoints < gear[i].MaxHitPoints)
                {
                    gear[i].HitPoints++;
                }
            }
            Thing weapon = Pawn.equipment.Primary;
            if (weapon != null && (weapon.def.IsRangedWeapon || weapon.def.IsMeleeWeapon))
            {
                if(weapon.HitPoints < weapon.MaxHitPoints)
                {
                    weapon.HitPoints++;
                }
            }
        }

        public void TickActionHealth()
        {
            IEnumerable<Hediff_Injury> injuries = Pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury => injury.CanHealNaturally())
                .DistinctBy(injury => injury.Part)
                .Take(2);

            foreach (Hediff_Injury injury in injuries)
            {                
                injury.Heal(Rand.Range(.2f, .5f) + .1f * pwrVal);
            }
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
