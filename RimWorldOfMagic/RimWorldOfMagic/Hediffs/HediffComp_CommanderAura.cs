using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_CommanderAura : HediffComp
    {

        private bool initialized;

        private int nextApplyTick;
        public int nextSpeechTick = 0;

        public int pwrVal;
        public int verVal;
        private float radius;

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
            CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
            if (spawned && comp != null && comp.IsMightUser)
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
            bool flag = Pawn != null;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }

                if(Pawn.Map != null)
                {
                    if (Find.TickManager.TicksGame > nextApplyTick)
                    {
                        nextApplyTick = Find.TickManager.TicksGame + Rand.Range(1000, 1200);
                        List<Pawn> mapPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
                        for (int i = 0; i < mapPawns.Count; i++)
                        {
                            if (mapPawns[i].RaceProps.Humanlike && mapPawns[i].Faction != null && mapPawns[i].Faction == Pawn.Faction && mapPawns[i] != Pawn)
                            {
                                if (!TM_Calc.IsUndeadNotVamp(mapPawns[i]))
                                {
                                    if (Pawn.Position.InHorDistOf(mapPawns[i].Position, radius))
                                    {
                                        ApplyHediff(mapPawns[i]);
                                    }
                                }
                            }
                        }
                        CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
                        comp.MightUserXP += Rand.Range(2, 5);                        
                    }        
                }
                else //map null
                {
                    if (Find.TickManager.TicksGame >= nextApplyTick)
                    {
                        nextApplyTick = Find.TickManager.TicksGame + Rand.Range(1000, 1200);
                        if (Pawn.ParentHolder.ToString().Contains("Caravan"))
                        {
                            foreach (Pawn current in Pawn.holdingOwner)
                            {
                                if (current != null)
                                {
                                    if (current.RaceProps.Humanlike && current.Faction != null && current.Faction == Pawn.Faction && current != Pawn)
                                    {
                                        ApplyHediff(current);
                                    }
                                }
                            }
                        }
                    }
                }

                if(Find.TickManager.TicksGame % 1200 == 0)
                {
                    DetermineHediff();
                }
            }
        }     

        private void DetermineHediff()
        {           
            CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
            if (parent.def == TorannMagicDefOf.TM_CommanderAuraHD && comp != null)
            {
                pwrVal = comp.MightData.MightPowerSkill_CommanderAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_CommanderAura_pwr").level;
                verVal = comp.MightData.MightPowerSkill_CommanderAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_CommanderAura_ver").level;
            }
            radius = 15f + (2f * verVal);
        }

        private void ApplyHediff(Pawn p)
        {
            if (p.health != null && p.health.hediffSet != null)
            {
                if (pwrVal >= 3)
                {
                    HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_CommanderHD_III, .5f + (.05f * verVal));
                }
                else if(pwrVal >= 2)
                {
                    HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_CommanderHD_II, .5f + (.05f * verVal));
                }
                else if(pwrVal >= 1)
                {
                    HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_CommanderHD_I, .5f + (.05f * verVal));
                }
                else
                {
                    HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_CommanderHD, .5f + (.05f * verVal));
                }
            }            
        }

        public void DoMotivationalSpeech(Pawn p)
        {

        }
    }
}
