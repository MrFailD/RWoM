using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace TorannMagic
{
    internal class Verb_Mimic : Verb_UseAbility  
    {
        private bool validTarg;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {            
            if ( targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    if(targ.Thing is Pawn targetPawn)
                    {
                        if(targetPawn.RaceProps.Humanlike)
                        {
                            ShootLine shootLine;
                            validTarg = TryFindShootLineFromTo(root, targ, out shootLine);
                        }
                        else
                        {
                            validTarg = false;
                            //Log.Message("Target was not humanlike or did not have traits");
                        }
                    }
                    else
                    {
                        //target is not a pawn
                        validTarg = false;
                        //Log.Message("Target was not a valid pawn to mimic.");
                    }
                    
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
            bool result = false;

            if (currentTarget != null && base.CasterPawn != null && currentTarget.Thing is Pawn targetPawn)
            {
                if (targetPawn.RaceProps.Humanlike)
                {
                    CompAbilityUserMagic magicPawn = targetPawn.GetCompAbilityUserMagic();
                    CompAbilityUserMight mightPawn = targetPawn.GetCompAbilityUserMight();
                    bool copyMagic = false;
                    bool copyMight = false;
                    if(magicPawn != null && magicPawn.IsMagicUser)
                    {
                        copyMagic = true;
                    }
                    if(mightPawn != null && mightPawn.IsMightUser)
                    {
                        copyMight = true;
                    }

                    if(copyMight && copyMagic && Rand.Chance(.5f))
                    {
                        copyMagic = false;
                    }
                    TMAbilityDef tempAbility = null;
                    CompAbilityUserMight mightComp = CasterPawn.GetCompAbilityUserMight();
                    CompAbilityUserMagic magicComp = CasterPawn.GetCompAbilityUserMagic();

                    if (copyMagic)
                    {
                        tempAbility = TM_Calc.GetCopiedMagicAbility(targetPawn, base.CasterPawn);

                        if (tempAbility != null)
                        {
                            if (mightComp.mimicAbility != null)
                            {
                                if (mightComp.mimicAbility.manaCost > 0)
                                {
                                    MagicPower mp = magicComp.MagicData.AllMagicPowers.FirstOrDefault((MagicPower x) => x.abilityDef == mightComp.mimicAbility);
                                    if (mp != null)
                                    {
                                        mp.autocast = false;
                                    }
                                }
                                else if (mightComp.mimicAbility.staminaCost > 0)
                                {
                                    MightPower mp = mightComp.MightData.AllMightPowers.FirstOrDefault((MightPower x) => x.abilityDef == mightComp.mimicAbility);
                                    if (mp != null)
                                    {
                                        mp.autocast = false;
                                    }
                                }
                                mightComp.RemovePawnAbility(mightComp.mimicAbility);
                            }
                            TM_Action.ClearSustainedMagicHediffs(magicComp);
                            mightComp.mimicAbility = tempAbility;
                            mightComp.AddPawnAbility(tempAbility);
                        }
                        else
                        {
                            //invalid target
                            Messages.Message("TM_MimicFailed".Translate(
                                    CasterPawn.LabelShort
                                ), MessageTypeDefOf.RejectInput);

                        }
                    }
                    else if (copyMight)
                    {
                        tempAbility = TM_Calc.GetCopiedMightAbility(targetPawn, base.CasterPawn);

                        if (tempAbility != null)
                        {
                            if (mightComp.mimicAbility != null)
                            {
                                if (mightComp.mimicAbility.manaCost > 0)
                                {
                                    MagicPower mp = magicComp.MagicData.AllMagicPowers.FirstOrDefault((MagicPower x) => x.abilityDef == mightComp.mimicAbility);
                                    if (mp != null)
                                    {
                                        mp.autocast = false;
                                    }
                                }
                                else if (mightComp.mimicAbility.staminaCost > 0)
                                {
                                    MightPower mp = mightComp.MightData.AllMightPowers.FirstOrDefault((MightPower x) => x.abilityDef == mightComp.mimicAbility);
                                    if (mp != null)
                                    {
                                        mp.autocast = false;
                                    }
                                }
                                mightComp.RemovePawnAbility(mightComp.mimicAbility);
                            }
                            if(magicComp.mimicAbility != null)
                            {                                
                                magicComp.RemovePawnAbility(magicComp.mimicAbility);
                            }
                            TM_Action.ClearSustainedMagicHediffs(magicComp);
                            mightComp.mimicAbility = tempAbility;
                            mightComp.AddPawnAbility(tempAbility);
                        }
                        else
                        {
                            //invalid target
                            Messages.Message("TM_MimicFailed".Translate(
                                    CasterPawn.LabelShort
                                ), MessageTypeDefOf.RejectInput);
                        }
                    }
                }
            }
            else
            {
                Log.Warning("failed to TryCastShot");
            }
            burstShotsLeft = 0;
            return result;
        }
    }
}
