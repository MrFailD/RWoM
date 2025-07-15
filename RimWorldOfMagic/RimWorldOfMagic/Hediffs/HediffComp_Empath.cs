using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_Empath : HediffComp
    {

        private bool initialized = false;
        private bool removeNow;

        private int eventFrequency = 1251;

        private int pwrVal;  //bonus to empathy bleed and mental break threshold
        private int verVal;  //improves resistance penetration
        private int effVal;  //event frequency and range
        private float radius = 25f;
        private float arcaneDmg = 1f;

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
            if (spawned && comp != null && comp.IsMagicUser && Pawn.needs.mood != null)
            {
                pwrVal = comp.MagicData.GetSkill_Power(TorannMagicDefOf.TM_Empathy).level;
                verVal = comp.MagicData.GetSkill_Versatility(TorannMagicDefOf.TM_Empathy).level;
                effVal = comp.MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Empathy).level;
                parent.Severity = .1f + (.17f * pwrVal);
                radius = 25f + (3f * effVal);
                arcaneDmg = comp.arcaneDmg;
                eventFrequency = 1251 - (125 * effVal);
            }
            else
            {
                removeNow = true;
            }
        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null && !removeNow;
            if (flag)
            {
                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    Initialize();
                    EmotionalInspiration(Pawn);                    

                    List<Pawn> pList = (from mp in Pawn.Map.mapPawns.AllPawnsSpawned
                                        where (mp != Pawn && !mp.Dead && (mp.Position - Pawn.Position).LengthHorizontal <= radius && mp.RaceProps != null && mp.RaceProps.IsFlesh && mp.RaceProps.Humanlike && mp.needs != null && mp.needs.mood != null)
                                        select mp).ToList();
                    if (pList != null && pList.Count > 0)
                    {
                        Pawn p = pList.RandomElement();

                        if (Rand.Chance(TM_Calc.GetSpellSuccessChance(Pawn, p, true)) && Pawn.needs.mood != null && Pawn.needs.mood.thoughts != null && Pawn.needs.mood.thoughts.memories != null && Pawn.needs.mood.thoughts.memories.Memories != null &&
                           p.needs.mood.thoughts != null && p.needs.mood.thoughts.memories != null && p.needs.mood.thoughts.memories.Memories != null)
                        {
                            if (Pawn.InMentalState)
                            {
                                p.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_NegativeEmpathyTD, Pawn);
                            }
                            else
                            {
                                List<Thought_Memory> tms = (from t in Pawn.needs.mood.thoughts.memories.Memories
                                                            where (t.def.stages != null && t.MoodOffset() != 0 && t.def != TorannMagicDefOf.TM_PositiveEmpathyTD && t.def != TorannMagicDefOf.TM_NegativeEmpathyTD)
                                                            select t).ToList();
                                if (tms != null && tms.Count > 0)
                                {
                                    Thought_Memory tm = tms.RandomElement();
                                    if (tm != null)
                                    {
                                        if (tm.MoodOffset() < 0 && Rand.Chance(1f - (.05f * pwrVal)))
                                        {
                                            p.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_NegativeEmpathyTD);
                                        }
                                        else
                                        {
                                            p.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_PositiveEmpathyTD);
                                        }
                                    }
                                }

                                tms = (from t in p.needs.mood.thoughts.memories.Memories
                                       where (t.def.stages != null && t.MoodOffset() != 0 && t.def != TorannMagicDefOf.TM_PositiveEmpathyTD && t.def != TorannMagicDefOf.TM_NegativeEmpathyTD)
                                       select t).ToList();

                                if (tms != null && tms.Count > 0)
                                {
                                    Thought_Memory tm = tms.RandomElement();
                                    if (tm != null)
                                    {
                                        if (tm.MoodOffset() < 0 && Rand.Chance(1f - (.05f * pwrVal)))
                                        {
                                            Pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_NegativeEmpathyTD);
                                        }
                                        else
                                        {
                                            Pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_PositiveEmpathyTD);
                                        }
                                    }
                                }
                            }
                        }
                    }
                } 
            }
        }

        private void EmotionalInspiration(Pawn p)
        {
            if (Rand.Chance(.02f + (.01f * verVal)))
            {
                if (p.mindState != null && p.mindState.inspirationHandler != null && !p.mindState.inspirationHandler.Inspired)
                {
                    InspirationDef ins = TM_Calc.GetRandomAvailableInspirationDef(p);
                    if (ins != null)
                    {
                        p.mindState.inspirationHandler.TryStartInspiration(ins);
                    }
                }
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return removeNow || base.CompShouldRemove;
            }
        }        
    }
}
