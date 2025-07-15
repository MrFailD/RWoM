using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using Verse.AI;


namespace TorannMagic
{
    public class Verb_Rend : Verb_UseAbility
    {

        private int verVal;
        private float arcaneDmg = 1f;

        private bool validTarg;
        //can be used with shieldbelt
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.Thing != null && targ.Thing == caster)
            {
                return verbProps.targetParams.canTargetSelf;
            }
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    ShootLine shootLine;
                    validTarg = TryFindShootLineFromTo(root, targ, out shootLine);
                }
                else
                {
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
            //this.UpdateTargets();
            FindTargets();
            MagicPowerSkill ver = base.CasterPawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Rend.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Rend_ver");
            arcaneDmg = base.CasterPawn.GetCompAbilityUserMagic().arcaneDmg;
            verVal = ver.level;
            bool flag2 = UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1;
            if (flag2)
            {
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            for (int i = 0; i < TargetsAoE.Count; i++)
            {
                Pawn newPawn = TargetsAoE[i].Thing as Pawn;
                if(newPawn.RaceProps.IsFlesh)
                {
                    if (Rand.Chance(TM_Calc.GetSpellSuccessChance(CasterPawn, newPawn, true)))
                    {
                        HealthUtility.AdjustSeverity(newPawn, HediffDef.Named("TM_RendHD"), (3f + (.6f * ver.level)) * arcaneDmg);
                        if (newPawn.Faction != null && !newPawn.Faction.HostileTo(base.CasterPawn.Faction))
                        {
                            newPawn.Faction.TryAffectGoodwillWith(base.CasterPawn.Faction, -20, true, false, null, null);
                        }
                        else if (newPawn.kindDef != null && newPawn.kindDef.RaceProps.Animal && newPawn.Faction == null)
                        {
                            newPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, true, false, false, null);
                        }
                        if (newPawn.IsColonist && !base.CasterPawn.IsColonist)
                        {
                            TM_Action.SpellAffectedPlayerWarning(newPawn);
                        }
                    }
                    else
                    {
                        MoteMaker.ThrowText(newPawn.DrawPos, newPawn.Map, "TM_ResistedSpell".Translate(), -1);
                    }
                }
                else
                {
                    TM_Action.DamageEntities(newPawn, null, (4f), DamageDefOf.Cut, CasterPawn);
                }
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
