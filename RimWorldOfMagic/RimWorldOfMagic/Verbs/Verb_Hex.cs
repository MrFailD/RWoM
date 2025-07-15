using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using Verse.AI;


namespace TorannMagic
{
    public class Verb_Hex : Verb_UseAbility
    {

        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;
        private bool validTarg;
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    validTarg = true;
                }
                else
                {
                    //out of range
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        protected override bool TryCastShot()
        {
            bool flag = false;
            TargetsAoE.Clear();
            FindTargets();
            CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
            if (comp != null && comp.MagicData != null)
            {
                pwrVal = TM_Calc.GetSkillPowerLevel(CasterPawn, Ability.Def as TMAbilityDef);
                verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, Ability.Def as TMAbilityDef);
                //pwrVal = TM_Calc.GetMagicSkillLevel(CasterPawn, comp.MagicData.MagicPowerSkill_Hex, "TM_Hex", "_pwr", true);
                //verVal = TM_Calc.GetMagicSkillLevel(CasterPawn, comp.MagicData.MagicPowerSkill_Hex, "TM_Hex", "_ver", true);
                arcaneDmg = comp.arcaneDmg;
            }
            bool flag2 = UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1;
            if (flag2)
            {
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            bool addAbilities = false;
            bool shouldAddAbilities = comp.HexedPawns.Count <= 0;
            for (int i = 0; i < TargetsAoE.Count; i++)
            {
                Pawn newPawn = TargetsAoE[i].Thing as Pawn;                
                if(newPawn.RaceProps.IsFlesh && !TM_Calc.IsUndead(newPawn))
                {
                    if (Rand.Chance(.4f + (.1f * pwrVal) * TM_Calc.GetSpellSuccessChance(CasterPawn, newPawn, true)))
                    {
                        HealthUtility.AdjustSeverity(newPawn, TorannMagicDefOf.TM_HexHD, 1f);
                        if(!comp.HexedPawns.Contains(newPawn))
                        {
                            comp.HexedPawns.Add(newPawn);                            
                        }
                        addAbilities = true;
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Hex, newPawn.DrawPos, newPawn.Map, .6f, .1f, .2f, .2f, 0, 0, 0, 0);
                    }
                    else
                    {
                        MoteMaker.ThrowText(newPawn.DrawPos, newPawn.Map, "TM_ResistedSpell".Translate(), -1);
                    }                    
                }
            }
            if(shouldAddAbilities && addAbilities)
            {
                comp.AddPawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                comp.AddPawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                comp.AddPawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
            }
            PostCastShot(flag, out flag);
            return flag;
        }


        private void FindTargets()
        {
            bool flag = UseAbilityProps.AbilityTargetCategory == AbilityTargetCategory.TargetAoE;
            if (flag)
            {
                bool flag2 = UseAbilityProps.TargetAoEProperties == null;
                if (flag2)
                {
                    Log.Error("Tried to Cast AoE-Ability without defining a target class");
                }
                List<Thing> list = new List<Thing>();
                IntVec3 aoeStartPosition = caster.PositionHeld;
                bool flag3 = !UseAbilityProps.TargetAoEProperties.startsFromCaster;
                if (flag3)
                {
                    aoeStartPosition = currentTarget.Cell;
                }
                bool flag4 = !UseAbilityProps.TargetAoEProperties.friendlyFire;
                if (flag4)
                {
                    list = (from x in caster.Map.listerThings.AllThings
                            where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) && x.Faction != Faction.OfPlayer
                            select x).ToList<Thing>();
                }
                else
                {
                    bool flag5 = UseAbilityProps.TargetAoEProperties.targetClass == typeof(Plant) || UseAbilityProps.TargetAoEProperties.targetClass == typeof(Building);
                    if (flag5)
                    {
                        list = (from x in caster.Map.listerThings.AllThings
                                where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType())
                                select x).ToList<Thing>();
                        foreach (Thing current in list)
                        {
                            LocalTargetInfo item = new LocalTargetInfo(current);
                            TargetsAoE.Add(item);
                        }
                        return;
                    }
                    list.Clear();
                    list = (from x in caster.Map.listerThings.AllThings
                            where x.Position.InHorDistOf(aoeStartPosition, (float)UseAbilityProps.TargetAoEProperties.range) && UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) && (x.HostileTo(Faction.OfPlayer) || UseAbilityProps.TargetAoEProperties.friendlyFire)
                            select x).ToList<Thing>();
                }
                int maxTargets = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties.maxTargets;
                List<Thing> list2 = new List<Thing>(list.InRandomOrder(null));
                int num = 0;
                while (num < maxTargets && num < list2.Count<Thing>())
                {
                    TargetInfo targ = new TargetInfo(list2[num]);
                    bool flag6 = UseAbilityProps.targetParams.CanTarget(targ);
                    if (flag6)
                    {
                        TargetsAoE.Add(new LocalTargetInfo(list2[num]));
                    }
                    num++;
                }
            }
            else
            {
                TargetsAoE.Clear();
                TargetsAoE.Add(currentTarget);
            }
        }

    }
}
