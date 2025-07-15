using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Aura : HediffComp
    {

        private bool initialized;

        private int nextApplyTick;

        private HediffDef hediffDef;

        public override void CompExposeData()
        {
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
            CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
            if (spawned && comp != null && comp.IsMagicUser)
            {
                DetermineHediff();
            }
            else
            {
                Pawn.health.RemoveHediff(parent);
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

                if (Find.TickManager.TicksGame > nextApplyTick && hediffDef != null)
                {
                    Pawn pawn = TM_Calc.FindNearbyFactionPawn(Pawn, Pawn.Faction, 100);
                    if (pawn != null && pawn.health != null)
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffDef, false) || pawn.Faction != Pawn.Faction || pawn.RaceProps.Animal)
                        {
                            nextApplyTick = Find.TickManager.TicksGame + Rand.Range(80, 150);
                        }
                        else
                        {
                            HealthUtility.AdjustSeverity(pawn, hediffDef, 1f);
                            nextApplyTick = Find.TickManager.TicksGame + Rand.Range(4800, 5600);
                            FleckMaker.ThrowSmoke(pawn.DrawPos, pawn.Map, 1f);
                            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, .8f);
                            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                            {
                                CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
                                comp.MightUserXP += Rand.Range(10, 15);
                            }
                            else
                            {
                                CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
                                comp.MagicUserXP += Rand.Range(10, 15);
                            }
                            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(TorannMagicDefOf.TM_UsedMagic, Pawn.Named(HistoryEventArgsNames.Doer), Pawn.Named(HistoryEventArgsNames.Subject), Pawn.Named(HistoryEventArgsNames.AffectedFaction), Pawn.Named(HistoryEventArgsNames.Victim)), true);
                        }
                    }
                }

                if(Find.TickManager.TicksGame % 1200 == 0)
                {
                    DetermineHediff();
                }
            }
        }     

        public void DetermineHediff()
        {
            MagicPower abilityPower = null;            
            CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
            if (parent.def == TorannMagicDefOf.TM_Shadow_AuraHD && comp != null)
            {
                abilityPower = comp.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shadow);                
                if (abilityPower == null)
                {
                    abilityPower = comp.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shadow_I);                    
                    if (abilityPower == null)
                    {
                        abilityPower = comp.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shadow_II);                        
                        if (abilityPower == null)
                        {                            
                            abilityPower = comp.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shadow_III);
                        }
                    }
                }
                if (abilityPower.level == 0)
                {
                    hediffDef = TorannMagicDefOf.Shadow;
                }
                else if (abilityPower.level == 1)
                {
                    hediffDef = TorannMagicDefOf.Shadow_I;
                }
                else if (abilityPower.level == 2)
                {
                    hediffDef = TorannMagicDefOf.Shadow_II;
                }
                else
                {
                    hediffDef = TorannMagicDefOf.Shadow_III;
                }
            }
            if (parent.def == TorannMagicDefOf.TM_RayOfHope_AuraHD && comp != null)
            {
                abilityPower = comp.MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_P_RayofHope);
                if (abilityPower == null)
                {
                    abilityPower = comp.MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_P_RayofHope_I);
                    if (abilityPower == null)
                    {
                        abilityPower = comp.MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_P_RayofHope_II);
                        if (abilityPower == null)
                        {
                            abilityPower = comp.MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_P_RayofHope_III);
                        }
                    }
                }
                if (abilityPower.level == 0)
                {
                    hediffDef = TorannMagicDefOf.RayofHope;
                }
                else if (abilityPower.level == 1)
                {
                    hediffDef = TorannMagicDefOf.RayofHope_I;
                }
                else if (abilityPower.level == 2)
                {
                    hediffDef = TorannMagicDefOf.RayofHope_II;
                }
                else
                {
                    hediffDef = TorannMagicDefOf.RayofHope_III;
                }
            }
            if (parent.def == TorannMagicDefOf.TM_SoothingBreeze_AuraHD && comp != null)
            {
                abilityPower = comp.MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe);
                if (abilityPower == null)
                {
                    abilityPower = comp.MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe_I);
                    if (abilityPower == null)
                    {
                        abilityPower = comp.MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe_II);
                        if (abilityPower == null)
                        {
                            abilityPower = comp.MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe_III);
                        }
                    }
                }
                if (abilityPower.level == 0)
                {
                    hediffDef = TorannMagicDefOf.SoothingBreeze;
                }
                else if (abilityPower.level == 1)
                {
                    hediffDef = TorannMagicDefOf.SoothingBreeze_I;
                }
                else if (abilityPower.level == 2)
                {
                    hediffDef = TorannMagicDefOf.SoothingBreeze_II;
                }
                else
                {
                    hediffDef = TorannMagicDefOf.SoothingBreeze_III;
                }
            }
            if (parent.def == TorannMagicDefOf.TM_InnerFire_AuraHD && comp != null)
            {
                abilityPower = comp.MagicData.MagicPowersIF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope);
                if (abilityPower == null)
                {
                    abilityPower = comp.MagicData.MagicPowersIF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope_I);
                    if (abilityPower == null)
                    {
                        abilityPower = comp.MagicData.MagicPowersIF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope_II);
                        if (abilityPower == null)
                        {
                            abilityPower = comp.MagicData.MagicPowersIF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope_III);
                        }
                    }
                }
                if (abilityPower.level == 0)
                {
                    hediffDef = TorannMagicDefOf.InnerFireHD;
                }
                else if (abilityPower.level == 1)
                {
                    hediffDef = TorannMagicDefOf.InnerFire_IHD;
                }
                else if (abilityPower.level == 2)
                {
                    hediffDef = TorannMagicDefOf.InnerFire_IIHD;
                }
                else
                {
                    hediffDef = TorannMagicDefOf.InnerFire_IIIHD;
                }
            }
            if (abilityPower != null)
            {
                parent.Severity = .5f + abilityPower.level;
            }
            else
            {
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
