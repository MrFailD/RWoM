using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_TaskMasterAura : HediffComp
    {

        private bool initialized;

        private int nextApplyTick;

        private int pwrVal;
        private int verVal;
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

                if (Find.TickManager.TicksGame > nextApplyTick)
                {
                    nextApplyTick = Find.TickManager.TicksGame + (Rand.Range(1000, 1200) - (50 * verVal));
                    if (Pawn.Map != null)
                    {
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
                    }
                    else //map null
                    {
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
                    CompAbilityUserMight comp = Pawn.GetCompAbilityUserMight();
                    comp.MightUserXP += Rand.Range(2, 5);                    
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
            if (parent.def == TorannMagicDefOf.TM_TaskMasterAuraHD && comp != null)
            {
                pwrVal = comp.MightData.MightPowerSkill_TaskMasterAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_TaskMasterAura_pwr").level;
                verVal = comp.MightData.MightPowerSkill_TaskMasterAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_TaskMasterAura_ver").level;
            }
            radius = 15f + (2f * verVal);
        }

        private void ApplyHediff(Pawn p)
        {
            if (p.health != null && p.health.hediffSet != null)
            {
                Hediff hd = p.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_TaskMasterHD);
                if (hd != null)
                {
                    if (hd.Severity < (.5f + pwrVal))
                    {
                        hd.Severity = .5f + pwrVal;
                    }
                }
                else
                {
                    HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_TaskMasterHD, .5f + pwrVal);
                }
                HediffComp_TaskMaster hdComp = p.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_TaskMasterHD, false).TryGetComp<HediffComp_TaskMaster>();
                if(hdComp != null)
                {
                    hdComp.duration += 2;
                }
            }            
        }
    }
}
