using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_RangerBond : HediffComp
    {
        private bool initializing = true;
        public Pawn bonderPawn;

        public override string CompLabelInBracketsExtra => bonderPawn != null ? bonderPawn.LabelShort + base.CompLabelInBracketsExtra : base.CompLabelInBracketsExtra;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref bonderPawn, "bonderPawn", false);
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
            if (bonderPawn == null)
            {
                if (spawned)
                {
                    FleckMaker.ThrowHeatGlow(Pawn.DrawPos.ToIntVec3(), Pawn.Map, 2f);
                }
                List<Pawn> mapPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
                for (int i = 0; i < mapPawns.Count(); i++)
                {
                    if (!mapPawns[i].DestroyedOrNull() && mapPawns[i].Spawned && !mapPawns[i].Downed && mapPawns[i].RaceProps.Humanlike)
                    {
                        CompAbilityUserMight comp = mapPawns[i].GetCompAbilityUserMight();
                        if (comp.IsMightUser && comp.bondedPet != null)
                        {
                            if (comp.bondedPet == Pawn)
                            {
                                bonderPawn = comp.Pawn;
                                break;
                            }
                        }
                        CompAbilityUserMagic compMagic = mapPawns[i].GetCompAbilityUserMagic();
                        if(compMagic.IsMagicUser && compMagic.bondedSpirit != null)
                        {
                            if(compMagic.bondedSpirit == Pawn)
                            {
                                bonderPawn = compMagic.Pawn;
                                break;
                            }
                        }
                    }
                }
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

            if (Find.TickManager.TicksGame % 600 == 0)
            {

                Hediff_Injury injuryToHeal = Pawn?.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>()
                    .FirstOrDefault();
                injuryToHeal?.Heal(injuryToHeal.CanHealNaturally() ? 1.0f + parent.Severity / 3f : .2f);

                if (bonderPawn != null && !bonderPawn.Destroyed && !bonderPawn.Dead)
                {
                    RefreshBond();
                    UpdateBond();                    
                }
                else
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(parent.def));
                    if(Pawn.def.thingClass == typeof(TMPawnSummoned))
                    {
                        if (Pawn.Map != null)
                        {
                            FleckMaker.ThrowSmoke(Pawn.DrawPos, Pawn.Map, 1f);
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Ghost, Pawn.DrawPos, Pawn.Map, 1.3f, .25f, .1f, .45f, 0, Rand.Range(1f, 2f), 0, 0);
                        }
                        Pawn.Destroy(DestroyMode.Vanish);                        
                    }
                    //this.Pawn.SetFactionDirect(null);
                }
            }
        }

        private void UpdateBond()
        {
            int verVal = 0;
            CompAbilityUserMight comp = bonderPawn.GetCompAbilityUserMight();
            if (comp != null && comp.IsMightUser)
            {
                verVal = comp.MightData.MightPowerSkill_AnimalFriend.FirstOrDefault((MightPowerSkill x) => x.label == "TM_AnimalFriend_ver").level;
            }
            CompAbilityUserMagic compMagic = bonderPawn.GetCompAbilityUserMagic();
            if(compMagic != null && compMagic.IsMagicUser)
            {
                verVal = compMagic.MagicData.MagicPowerSkill_GuardianSpirit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_GuardianSpirit_ver").level;
            }
            parent.Severity = .5f + verVal;
        }

        public void RefreshBond()
        {
            TM_Action.UpdateAnimalTraining(Pawn);
        }
    }
}
