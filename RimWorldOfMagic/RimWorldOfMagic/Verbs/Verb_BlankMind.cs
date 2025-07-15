using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using UnityEngine;
using Verse;
using Verse.AI;


namespace TorannMagic
{
    public class Verb_BlankMind : Verb_UseAbility
    {
        private bool validTarg;
        //Used specifically for non-unique verbs that ignore LOS (can be used with shield belt)
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
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
            //this.UpdateTargets();
            FindTargets();

            bool friendlyTarget = currentTarget != null && currentTarget.Thing != null && currentTarget.Thing.Faction != null && currentTarget.Thing.Faction == CasterPawn.Faction && currentTarget.Thing != CasterPawn;
            bool flag2 = (UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1) || friendlyTarget;
            if (flag2)
            {
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            if (friendlyTarget)
            {
                Pawn pawn = currentTarget.Thing as Pawn;
                if (pawn != null && pawn.RaceProps.Humanlike && pawn.needs != null && pawn.needs.mood.thoughts != null)
                {
                    if (Rand.Chance(TM_Calc.GetSpellSuccessChance(CasterPawn, pawn, true)))
                    {
                        List<Thought_Memory> thoughts = pawn.needs.mood.thoughts.memories.Memories;
                        pawn.mindState.mentalStateHandler.TryStartMentalState(TorannMagicDefOf.WanderConfused, null, false, false, false, null, false);
                        for(int i =0; i< thoughts.Count; i++)
                        {
                            pawn.needs.mood.thoughts.memories.RemoveMemory(thoughts[i]);                            
                            i--;
                        }
                        pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_MemoryWipe, null);
                        Effects(pawn.Position);
                    }
                    else
                    {
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TM_ResistedSpell".Translate(), -1);
                    }                    
                }
            }
            else
            {
                for (int i = 0; i < TargetsAoE.Count; i++)
                {
                    Pawn newPawn = TargetsAoE[i].Thing as Pawn;
                    if (newPawn.RaceProps.IsFlesh && newPawn.RaceProps.Humanlike && newPawn.Faction != CasterPawn.Faction)
                    {
                        if (Rand.Chance(TM_Calc.GetSpellSuccessChance(CasterPawn, newPawn, true)))
                        {
                            TM_Action.DamageEntities(newPawn, null, 30, DamageDefOf.Stun, CasterPawn);
                            Effects(newPawn.Position);
                        }
                        else
                        {
                            MoteMaker.ThrowText(newPawn.DrawPos, newPawn.Map, "TM_ResistedSpell".Translate(), -1);
                        }
                    }
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

        public void Effects(IntVec3 position)
        {
            Vector3 rndPos = position.ToVector3Shifted();
            for (int i = 0; i < 3; i++)
            {
                rndPos.x += Rand.Range(-.5f, .5f);
                rndPos.z += Rand.Range(-.5f, .5f);
                rndPos.y += Rand.Range(.3f, 1.3f);
                FleckMaker.ThrowLightningGlow(position.ToVector3Shifted(), CasterPawn.Map, 1.4f);
            }
        }

    }
}
