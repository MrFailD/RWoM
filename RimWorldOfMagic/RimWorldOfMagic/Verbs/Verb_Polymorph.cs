using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using Verse.AI;
using UnityEngine;


namespace TorannMagic
{
    public class Verb_Polymorph : Verb_UseAbility
    {

        private int verVal;
        private int pwrVal;
        private bool validTarg;
        private float arcaneDmg = 1;

        private int duration = 1800;
        private int min = 20;
        private int max = 100;

        private bool drafted;

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
            UpdateTargets();
            MagicPowerSkill pwr = base.CasterPawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Polymorph.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Polymorph_pwr");
            MagicPowerSkill ver = base.CasterPawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Polymorph.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Polymorph_ver");
            verVal = ver.level;
            pwrVal = pwr.level;
            CompAbilityUserMagic comp = base.CasterPawn.GetCompAbilityUserMagic();
            arcaneDmg = base.CasterPawn.GetCompAbilityUserMagic().arcaneDmg;
            duration += Mathf.RoundToInt(600 * verVal * arcaneDmg); 

            if (base.CasterPawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                MightPowerSkill mpwr = base.CasterPawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                MightPowerSkill mver = base.CasterPawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                pwrVal = mpwr.level;
                verVal = mver.level;
            }
            
            if (ModOptions.Settings.Instance.AIHardMode && !CasterPawn.IsColonist)
            {
                verVal = 2;
                pwrVal = 3;
            }
            bool flag2 = UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1;
            if (flag2)
            {
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            for (int i = 0; i < TargetsAoE.Count; i++)
            {
                Pawn newPawn = TargetsAoE[i].Thing as Pawn;
                if (newPawn != CasterPawn)
                {
                    CompPolymorph compPoly = newPawn.GetComp<CompPolymorph>();
                    if (compPoly != null && compPoly.Original != null && compPoly.TicksLeft > 0)
                    {
                        compPoly.Temporary = true;
                        compPoly.TicksLeft = 0;
                    }
                    else
                    {
                        float enchantChance = .5f;
                        if (!TM_Calc.IsRobotPawn(newPawn))
                        {
                            enchantChance = (.5f + (.1f * pwrVal) * TM_Calc.GetSpellSuccessChance(CasterPawn, newPawn));
                        }
                        else
                        {
                            enchantChance = (.0f + (.2f * pwrVal) * TM_Calc.GetSpellSuccessChance(CasterPawn, newPawn));
                        }
                        if (Rand.Chance(enchantChance) && newPawn.GetComp<CompPolymorph>() != null)
                        { 
                            if(newPawn.drafter != null)
                            {
                                drafted = newPawn.Drafted;
                            }
                            FactionDef fDef = null;
                            if (newPawn.Faction != null)
                            {
                                fDef = newPawn.Faction.def;
                            }
                            SpawnThings spawnThing = new SpawnThings();
                            spawnThing.factionDef = fDef;
                            spawnThing.spawnCount = 1;
                            spawnThing.temporary = false;

                            GetPolyMinMax(newPawn);

                            spawnThing = TM_Action.AssignRandomCreatureDef(spawnThing, min, max);
                            if (spawnThing.def == null || spawnThing.kindDef == null)
                            {
                                spawnThing.def = ThingDef.Named("Rat");
                                spawnThing.kindDef = PawnKindDef.Named("Rat");
                                Log.Message("random creature was null");
                            }

                            Pawn polymorphedPawn = TM_Action.PolymorphPawn(CasterPawn, newPawn, newPawn, spawnThing, newPawn.Position, true, duration, newPawn.Faction);

                            if (polymorphedPawn.Faction != CasterPawn.Faction && polymorphedPawn.mindState != null && Rand.Chance(Mathf.Clamp((.2f * pwrVal), 0f, .5f)))
                            {
                                polymorphedPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "wild beast!", true, false, false, null, true);
                            }

                            if (verVal >= 3)
                            {
                                polymorphedPawn.GetComp<CompPolymorph>().Temporary = false;
                            }

                            if(polymorphedPawn.IsColonist && base.CasterPawn.IsColonist && polymorphedPawn.drafter != null)
                            {
                                polymorphedPawn.drafter.Drafted = drafted;
                            }

                            FleckMaker.ThrowSmoke(newPawn.DrawPos, newPawn.Map, 2);
                            FleckMaker.ThrowMicroSparks(newPawn.DrawPos, newPawn.Map);
                            FleckMaker.ThrowHeatGlow(newPawn.Position, newPawn.Map, 2);
                            
                            newPawn.DeSpawn();
                            if (polymorphedPawn.IsColonist && !base.CasterPawn.IsColonist)
                            {
                                TM_Action.SpellAffectedPlayerWarning(polymorphedPawn);
                            }
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
        
        private void GetPolyMinMax(Pawn pawn)
        {
            if (verVal >= 3)
            {
                if (pawn.Faction != CasterPawn.Faction)
                {
                    min = 20;
                    max = 60;
                }
                else
                {
                    min = 80;
                    max = 200;
                }
            }
            else if (verVal >= 2)
            {
                if (pawn.Faction != CasterPawn.Faction)
                {
                    min = 20;
                    max = 60;
                }
                else
                {
                    min = 60;
                    max = 160;
                }
            }
            else if (verVal >= 1)
            {
                if (pawn.Faction != CasterPawn.Faction)
                {
                    min = 40;
                    max = 80;
                }
                else
                {
                    min = 50;
                    max = 100;
                }
            }
            else
            {
                if (pawn.Faction != CasterPawn.Faction)
                {
                    min = 60;
                    max = 100;
                }
                else
                {
                    min = 20;
                    max = 60;
                }
            }
        }        
    }
}
