using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Undead : HediffComp
    {
        private bool necroValid = true;
        private int lichStrike;
        private bool initialized;

        public Pawn linkedPawn;
        private static readonly string[] nonStandardNeedsToAutoFulfill = new[] {
            "Mood",
            "Suppression",
            "Bladder", //Dubs Bad Hygiene
            "Hygiene", //Dubs Bad Hygiene
            "DBHThirst" //Dubs Bad Hygiene
        };

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref linkedPawn, "linkedPawn", false);
        }

        public override string CompLabelInBracketsExtra => linkedPawn != null ? linkedPawn.LabelShort + base.CompLabelInBracketsExtra : base.CompLabelInBracketsExtra;

        public string labelCap
        {
            get
            {
                if (linkedPawn != null)
                {
                    return Def.LabelCap + "(" + linkedPawn.LabelShort + ")";
                }
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                if (linkedPawn != null)
                {
                    return Def.label + "(" + linkedPawn.LabelShort + ")";
                }
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if(Pawn.IsSlave)
            {
                Pawn.guest.SetGuestStatus(null);
            }
            if (spawned)
            {
                //FleckMaker.ThrowLightningGlow(base.Pawn.TrueCenter(), base.Pawn.Map, 3f);
            }            
        }

        private void UpdateHediff()
        {
            if(linkedPawn != null)
            {
                CompAbilityUserMagic comp = linkedPawn.GetCompAbilityUserMagic();
                try
                {
                    if (comp != null)
                    {
                        //int ver = TM_Calc.GetMagicSkillLevel(linkedPawn, comp.MagicData.MagicPowerSkill_RaiseUndead, "TM_RaiseUndead", "_ver");
                        int ver = TM_Calc.GetSkillVersatilityLevel(linkedPawn, TorannMagicDefOf.TM_RaiseUndead, false);
                        if (parent.Severity != ver + .5f)
                        {
                            parent.Severity = .5f + ver;
                        }
                        if(Pawn.IsPrisoner || Pawn.Faction != linkedPawn.Faction)
                        {
                            Pawn.Kill(null, null);
                        }
                    }
                }
                catch
                {
                    Pawn.Kill(null, null);
                }

            }
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
            if(Find.TickManager.TicksGame % 16 == 0)
            {
                foreach(Hediff hd in Pawn.health.hediffSet.hediffs)
                {
                    if(hd.def.defName == "SpaceHypoxia")
                    {
                        Pawn.health.RemoveHediff(hd);
                        break;
                    }
                }
            }
            if (Find.TickManager.TicksGame % 6000 == 0 && linkedPawn != null)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(TorannMagicDefOf.TM_UsedMagic, linkedPawn.Named(HistoryEventArgsNames.Doer), linkedPawn.Named(HistoryEventArgsNames.Subject), linkedPawn.Named(HistoryEventArgsNames.AffectedFaction), linkedPawn.Named(HistoryEventArgsNames.Victim)), true);
                TM_Action.UpdateAnimalTraining(Pawn);                
            }
            bool flag4 = Find.TickManager.TicksGame % 600 == 0 && Pawn.def != TorannMagicDefOf.TM_SkeletonR && Pawn.def != TorannMagicDefOf.TM_GiantSkeletonR;
            if (flag4 && !Pawn.Dead)
            {                
                UpdateHediff();
                Pawn pawn = Pawn;
                necroValid = false;
                if (Pawn != null && !linkedPawn.DestroyedOrNull())
                {
                    necroValid = true;
                    lichStrike = 0;

                    if (ModsConfig.IdeologyActive && !Pawn.Downed && Pawn.guest != null && Pawn.IsColonist)
                    {
                        TM_Action.TryCopyIdeo(linkedPawn, Pawn);
                        if (Pawn.guest.GuestStatus != GuestStatus.Slave)
                        {
                            Pawn.guest.SetGuestStatus(linkedPawn.Faction, GuestStatus.Slave);
                        }                        
                    }                    
                }
                else
                {
                    lichStrike++;
                }   
                
                if (!necroValid && !Pawn.Dead && ((pawn.IsColonist && lichStrike > 2) || (!pawn.IsColonist && lichStrike >= 1)))
                {
                    if (Pawn.Map != null)
                    {
                        TM_MoteMaker.ThrowScreamMote(Pawn.Position.ToVector3(), Pawn.Map, .8f, 255, 255, 255);
                    }
                    Pawn.Kill(null, null);
                }
                else
                {
                    List<Need> needs = Pawn?.needs?.AllNeeds;
                    if (needs != null && needs.Count > 0)
                    { 
                        for (int i = 0; i < needs.Count; i++)
                        {
                            if (needs[i]?.def == NeedDefOf.Food || nonStandardNeedsToAutoFulfill.Contains(needs[i]?.def?.defName))
                            {
                                needs[i].CurLevel = needs[i].MaxLevel;
                            }
                        }
                    }
                    
                    //if (base.Pawn.needs.food != null)
                    //{
                    //    base.Pawn.needs.food.CurLevel = base.Pawn.needs.food.MaxLevel;
                    //}
                    //if (base.Pawn.needs.rest != null)
                    //{
                    //    base.Pawn.needs.rest.CurLevel = base.Pawn.needs.rest.MaxLevel;
                    //}

                    //if (base.Pawn.IsColonist)
                    //{
                    //    base.Pawn.needs.beauty.CurLevel = .5f;
                    //    base.Pawn.needs.comfort.CurLevel = .5f;
                    //    base.Pawn.needs.joy.CurLevel = .5f;
                    //    base.Pawn.needs.mood.CurLevel = .5f;
                    //    base.Pawn.needs.space.CurLevel = .5f;
                    //}

                    Hediff_Injury injuryToHeal = Pawn.health.hediffSet.hediffs
                        .OfType<Hediff_Injury>()
                        .FirstOrDefault();
                    injuryToHeal?.Heal(injuryToHeal.CanHealNaturally() ? 2.0f : 1.0f);


                    foreach (Hediff hediff in pawn.health.hediffSet.GetHediffsTendable())
                    {
                        if (hediff.Bleeding && hediff is Hediff_MissingPart)
                            Traverse.Create(root: hediff).Field(name: "isFreshInt").SetValue(false);
                        else
                            TM_Action.TendWithoutNotice(hediff, 1f, 1f);
                    }

                    List<Hediff> removeHDList = new List<Hediff>();
                    removeHDList.Clear();

                    using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
                    {                        
                        while (enumerator.MoveNext())
                        {
                            Hediff rec = enumerator.Current;
                            if (rec.def.makesSickThought)
                            {
                                removeHDList.Add(rec);
                            }
                            else if (!rec.IsPermanent())
                            {
                                if (rec.def.defName == "Cataract"
                                    || rec.def.defName == "HearingLoss"
                                    || rec.def.defName.Contains("ToxicBuildup")
                                    || rec.def.defName == "Blindness"
                                    || rec.def.defName.Contains("Asthma")
                                    || rec.def.defName == "Abasia" 
                                    || rec.def.defName == "BloodRot"
                                    || rec.def.defName == "Scaria"
                                    || rec.def.defName == "Cirrhosis"
                                    || rec.def.defName == "ChemicalDamageModerate"
                                    || rec.def.defName == "Frail"
                                    || rec.def.defName == "BadBack"
                                    || rec.def.defName.Contains("Carcinoma")
                                    || rec.def.defName == "ChemicalDamageSevere"
                                    || rec.def.defName.Contains("Alzheimers")
                                    || rec.def.defName == "Dementia"
                                    || rec.def.defName.Contains("HeartArteryBlockage")
                                    || rec.def.defName == "CatatonicBreakdown"
                                    || rec.def.defName.Contains("Pregnant")
                                    || rec.def.defName == "DrugOverdose"
                                    || rec.def.defName == "GeneticDrugNeed"
                                    || rec.def.defName == "HeartAttack")
                                {
                                    removeHDList.Add(rec);
                                }

                            }
                        }
                    }

                    foreach (Hediff hd in removeHDList)
                    {
                        pawn.health.RemoveHediff(hd);
                    }

                    CompHatcher cp_h = Pawn.TryGetComp<CompHatcher>();
                    if (cp_h != null)
                    {
                        Traverse.Create(root: cp_h).Field(name: "gestateProgress").SetValue(0);
                    }
                    CompMilkable cp_m = Pawn.TryGetComp<CompMilkable>();
                    if (cp_m != null)
                    {
                        Traverse.Create(root: cp_m).Field(name: "fullness").SetValue(0);
                    }

                }
                
            }
            
        }
    }
}
