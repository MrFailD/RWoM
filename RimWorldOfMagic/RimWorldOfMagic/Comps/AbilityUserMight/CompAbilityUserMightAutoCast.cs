using AbilityUser;
using RimWorld;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    public partial class CompAbilityUserMight
    {
        public void ResolveAutoCast()
        {
            
            if (ModOptions.Settings.Instance.autocastEnabled && Pawn.jobs != null && Pawn.CurJob != null && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilityVerb && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilitySelf && 
                Pawn.CurJob.def != JobDefOf.Ingest && Pawn.CurJob.def != JobDefOf.ManTurret && Pawn.GetPosture() == PawnPosture.Standing && !Pawn.CurJob.playerForced)
            {
                //Log.Message("pawn " + this.Pawn.LabelShort + " current job is " + this.Pawn.CurJob.def.defName);
                //non-combat (undrafted) spells    
                bool castSuccess = false;
                bool isFaceless = (mimicAbility != null);
                bool isCustom = customIndex >= 0;
                if (Pawn.drafter != null && !Pawn.Drafted && Stamina != null && Stamina.CurLevelPercentage >= ModOptions.Settings.Instance.autocastMinThreshold)
                {
                    foreach (MightPower mp in MightData.MightPowersCustomAll)
                    {
                        //Log.Message("checking custom power " + mp.abilityDef.defName);
                        if (mp.learned && mp.autocast && mp.autocasting != null && mp.autocasting.mightUser && mp.autocasting.undrafted)
                        {
                            TMAbilityDef tmad = mp.TMabilityDefs[mp.level] as TMAbilityDef; // issues with index?
                            //Log.Message("checking autocast for ability " + tmad.defName);
                            bool canUseWithEquippedWeapon = true;
                            bool canUseIfViolentAbility = Pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) ? !tmad.MainVerb.isViolent : true;
                            if (!TM_Calc.HasResourcesForAbility(Pawn, tmad))
                            {
                                continue;
                            }
                            if (canUseWithEquippedWeapon && canUseIfViolentAbility)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                if (mp.autocasting.type == TMDefs.AutocastType.OnTarget && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.CurJob.targetA);                                        
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if(mp.autocasting.type == TMDefs.AutocastType.OnSelf)
                                {                                        
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.CurJob.targetA);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Pawn targetThing = localTarget.Pawn;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnSelf.Evaluate(this, tmad, ability, mp, out castSuccess);
                                    }
                                }
                                if(mp.autocasting.type == TMDefs.AutocastType.OnCell && Pawn.CurJob.targetA != null)
                                {                                        
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.CurJob.targetA);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        IntVec3 targetThing = localTarget.Cell;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnCell.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if(mp.autocasting.type == TMDefs.AutocastType.OnNearby)
                                {
                                    //Log.Message("nearby autocast for " + tmad.defName);
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.CurJob.targetA);
                                    if(localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {                                            
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {                                            
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {                                            
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (castSuccess) goto AutoCastExit;
                            }
                        }
                    }
                    //Hunting only
                    if (Pawn.CurJob.def == JobDefOf.Hunt && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null && Pawn.CurJob.targetA.Thing is Pawn)
                    {                        
                        if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger) || isFaceless || isCustom) && !Pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                        {
                            PawnAbility ability = null;
                            foreach (MightPower current in MightData.MightPowersR)
                            {
                                if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger)))
                                {
                                    foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                    {
                                        if ((tmad == TorannMagicDefOf.TM_ArrowStorm || tmad == TorannMagicDefOf.TM_ArrowStorm_I || tmad == TorannMagicDefOf.TM_ArrowStorm_II || tmad == TorannMagicDefOf.TM_ArrowStorm_III))
                                        {
                                            if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                            {
                                                Thing wpn = Pawn.equipment.Primary;

                                                if (TM_Calc.IsUsingBow(Pawn))
                                                {
                                                    MightPower mightPower = MightData.MightPowersR.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                                    if (mightPower != null && mightPower.learned && mightPower.autocast)
                                                    {
                                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                                        AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, tmad, ability, mightPower, Pawn.CurJob.targetA, 4, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }                                                    
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            PawnAbility ability = null;

                            foreach (MightPower current in MightData.MightPowersSS)
                            {
                                if (current.abilityDef != null)
                                {
                                    if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                    {
                                        if (specWpnRegNum != -1)
                                        {
                                            if (current.abilityDef == TorannMagicDefOf.TM_PistolWhip)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PistolWhip);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_PistolWhip);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null && Pawn.TargetCurrentlyAimingAt != Pawn)
                                                    {
                                                        AutoCast.MeleeCombat_OnTarget.TryExecute(this, TorannMagicDefOf.TM_PistolWhip, ability, mightPower, targetPawn, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                            if (current.abilityDef == TorannMagicDefOf.TM_SuppressingFire)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_SuppressingFire);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_SuppressingFire);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null)
                                                    {
                                                        AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_SuppressingFire, ability, mightPower, targetPawn, 1, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                            if (current.abilityDef == TorannMagicDefOf.TM_Buckshot)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Buckshot);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_Buckshot);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null)
                                                    {
                                                        AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_Buckshot, ability, mightPower, targetPawn, 1, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }                            
                        }
                        if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            PawnAbility ability = null;
                            foreach (MightPower current in MightData.MightPowersS)
                            {
                                if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper)))
                                {
                                    if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                    {
                                        if (current.abilityDef == TorannMagicDefOf.TM_Headshot)
                                        {
                                            MightPower mightPower = MightData.MightPowersS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Headshot);
                                            if (mightPower != null && mightPower.autocast)
                                            {
                                                ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_Headshot);
                                                AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_Headshot, ability, mightPower, Pawn.CurJob.targetA, 4, out castSuccess);
                                                if (castSuccess) goto AutoCastExit;
                                            }
                                        }                                        
                                    }
                                }
                            }
                        }
                        if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight) || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            PawnAbility ability = null;
                            foreach (MightPower current in MightData.MightPowersDK)
                            {
                                if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || Pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight)))
                                {
                                    foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                    {
                                        if ((tmad == TorannMagicDefOf.TM_Spite || tmad == TorannMagicDefOf.TM_Spite_I || tmad == TorannMagicDefOf.TM_Spite_II || tmad == TorannMagicDefOf.TM_Spite_III))
                                        {
                                            if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                            {
                                                MightPower mightPower = MightData.MightPowersDK.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                                if (mightPower != null && mightPower.learned && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                                    AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, tmad, ability, mightPower, Pawn.CurJob.targetA, 4, out castSuccess);
                                                    if (castSuccess) goto AutoCastExit;
                                                }                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (skill_ThrowingKnife && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ThrowingKnife);
                            if (mightPower != null && mightPower.autocast)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_ThrowingKnife);
                                AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_ThrowingKnife, ability, mightPower, Pawn.CurJob.targetA, 4, out castSuccess);
                                if (castSuccess) goto AutoCastExit;                                
                            }
                        }
                        if (skill_TempestStrike && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TempestStrike);
                            if (mightPower != null && mightPower.autocast)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_TempestStrike);
                                AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_TempestStrike, ability, mightPower, Pawn.CurJob.targetA, 4, out castSuccess);
                                if (castSuccess) goto AutoCastExit;
                            }
                        }
                    }                    

                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersB)
                        {                      
                            if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || (Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer) || isCustom))) //current.abilityDef == this.mimicAbility ||
                            {
                                foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                {
                                    if (tmad == TorannMagicDefOf.TM_PhaseStrike || tmad == TorannMagicDefOf.TM_PhaseStrike_I || tmad == TorannMagicDefOf.TM_PhaseStrike_II || tmad == TorannMagicDefOf.TM_PhaseStrike_III)
                                    {
                                        MightPower mightPower = MightData.MightPowersB.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                        if (mightPower != null && mightPower.autocast && Pawn.equipment.Primary != null && !Pawn.equipment.Primary.def.IsRangedWeapon)
                                        {
                                            ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                            float minDistance = ActualStaminaCost(tmad) * 100;
                                            AutoCast.Phase.Evaluate(this, tmad, ability, mightPower, minDistance, out castSuccess);
                                            if (castSuccess) goto AutoCastExit;
                                        }                                        
                                    }
                                }
                            }
                        }
                    }

                    if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier) || isFaceless || isCustom)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersSS)
                        {
                            if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier))) 
                            {
                                if (current.abilityDef == TorannMagicDefOf.TM_FirstAid && TM_Calc.IsPawnInjured(Pawn, .2f))
                                {
                                    MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_FirstAid);
                                    if (mightPower != null && mightPower.autocast && !Pawn.CurJob.playerForced)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_FirstAid);
                                        AutoCast.HealSelf.Evaluate(this, TorannMagicDefOf.TM_FirstAid, ability, mightPower, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }
                            }
                        }
                    }

                    if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Apothecary) || isCustom)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersApothecary)
                        {
                            if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Apothecary)))
                            {
                                Pawn injuredPawn = TM_Calc.FindNearbyInjuredPawn(Pawn, 12, 2, false);
                                if (current.abilityDef == TorannMagicDefOf.TM_Elixir && injuredPawn != null && injuredPawn.health != null && injuredPawn.health.hediffSet != null && !injuredPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_HerbalElixirHD))
                                {
                                    MightPower mightPower = MightData.MightPowersApothecary.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Elixir);
                                    if (mightPower != null && mightPower.autocast && !Pawn.CurJob.playerForced)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_Elixir);
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, TorannMagicDefOf.TM_Elixir, ability, current, injuredPawn, 0, out castSuccess);
                                        //AutoCast.HealSelf.Evaluate(this, TorannMagicDefOf.TM_FirstAid, ability, mightPower, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }
                            }
                        }
                    }

                    if (MightUserLevel >= 20)
                    {
                        MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TeachMight);
                        if (mightPower.autocast && !Pawn.CurJob.playerForced && mightPower.learned)
                        {
                            if (Pawn.CurJobDef.joyKind != null || Pawn.CurJobDef == JobDefOf.Wait_Wander || Pawn.CurJobDef == JobDefOf.GotoWander)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_TeachMight);
                                AutoCast.TeachMight.Evaluate(this, TorannMagicDefOf.TM_TeachMight, ability, mightPower, out castSuccess);
                                if (castSuccess) goto AutoCastExit;
                            }
                        }
                    }
                }

                //combat (drafted) spells
                if (Pawn.drafter != null && Pawn.Drafted && Pawn.drafter.FireAtWill && Stamina != null && (Stamina.CurLevelPercentage >= ModOptions.Settings.Instance.autocastCombatMinThreshold || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk)) && Pawn.CurJob.def != JobDefOf.Goto && Pawn.CurJob.def != JobDefOf.AttackMelee)
                {
                    foreach (MightPower mp in MightData.MightPowersCustom)
                    {
                        if (mp.learned && mp.autocast && mp.autocasting != null && mp.autocasting.mightUser && mp.autocasting.drafted)
                        {
                            //try
                            //{ 
                            TMAbilityDef tmad = mp.TMabilityDefs[mp.level] as TMAbilityDef; // issues with index?
                            bool canUseWithEquippedWeapon = true;
                            bool canUseIfViolentAbility = Pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) ? !tmad.MainVerb.isViolent : true;
                            if (!TM_Calc.HasResourcesForAbility(Pawn, tmad))
                            {
                                continue;
                            }
                            if (canUseWithEquippedWeapon && canUseIfViolentAbility)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                if (mp.autocasting.type == TMDefs.AutocastType.OnTarget && Pawn.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt.Thing != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnSelf)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Pawn targetThing = localTarget.Pawn;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnSelf.Evaluate(this, tmad, ability, mp, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnCell && Pawn.TargetCurrentlyAimingAt != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        IntVec3 targetThing = localTarget.Cell;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnCell.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnNearby)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (castSuccess) goto AutoCastExit;
                            }
                            //}
                            //catch
                            //{
                            //    Log.Message("no index found at " + mp.level + " for " + mp.abilityDef.defName);
                            //}
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersB)
                        {
                            if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer)))
                            {
                                if (current.abilityDef == TorannMagicDefOf.TM_BladeSpin)
                                {
                                    MightPower mightPower = MightData.MightPowersB.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_BladeSpin);
                                    if (mightPower != null && mightPower.autocast && Pawn.equipment.Primary != null && !Pawn.equipment.Primary.def.IsRangedWeapon)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_BladeSpin);
                                        MightPowerSkill ver = MightData.MightPowerSkill_BladeSpin.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BladeSpin_ver");
                                        AutoCast.AoECombat.Evaluate(this, TorannMagicDefOf.TM_BladeSpin, ability, mightPower, 2, Mathf.RoundToInt(2+(.5f*ver.level)), Pawn.Position, true, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }
                            }
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk) || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersM)
                        {
                            if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk)))
                            {
                                if (current.abilityDef == TorannMagicDefOf.TM_TigerStrike)
                                {
                                    MightPower mightPower = MightData.MightPowersM.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TigerStrike);
                                    if (mightPower != null && mightPower.autocast && Pawn.equipment.Primary == null)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_TigerStrike);
                                        AutoCast.MonkCombatAbility.EvaluateMinRange(this, TorannMagicDefOf.TM_TigerStrike, ability, mightPower, Pawn.TargetCurrentlyAimingAt, 1.48f, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }
                                if (current.abilityDef == TorannMagicDefOf.TM_ThunderStrike)
                                {
                                    MightPower mightPower = MightData.MightPowersM.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ThunderStrike);
                                    if (mightPower != null && mightPower.autocast)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_ThunderStrike);
                                        AutoCast.MonkCombatAbility.EvaluateMinRange(this, TorannMagicDefOf.TM_ThunderStrike, ability, mightPower, Pawn.TargetCurrentlyAimingAt, 6f, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }
                            }
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Gladiator) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersG)
                        {
                            if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || Pawn.story.traits.HasTrait(TorannMagicDefOf.Gladiator)))
                            {
                                foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                {
                                    if ((tmad == TorannMagicDefOf.TM_Grapple || tmad == TorannMagicDefOf.TM_Grapple_I || tmad == TorannMagicDefOf.TM_Grapple_II || tmad == TorannMagicDefOf.TM_Grapple_III))
                                    {
                                        MightPower mightPower = MightData.MightPowersG.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                        if (mightPower != null && mightPower.learned && mightPower.autocast)
                                        {
                                            ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                            AutoCast.CombatAbility.Evaluate(this, tmad, ability, mightPower, out castSuccess);
                                            if (castSuccess) goto AutoCastExit;
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersR)
                        {
                            if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger)))
                            {
                                foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                {
                                    if ((tmad == TorannMagicDefOf.TM_ArrowStorm || tmad == TorannMagicDefOf.TM_ArrowStorm_I || tmad == TorannMagicDefOf.TM_ArrowStorm_II || tmad == TorannMagicDefOf.TM_ArrowStorm_III))
                                    {
                                        if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                        {
                                            Thing wpn = Pawn.equipment.Primary;

                                            if (TM_Calc.IsUsingBow(Pawn))
                                            {
                                                MightPower mightPower = MightData.MightPowersR.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                                if (mightPower != null && mightPower.learned && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                                    AutoCast.CombatAbility.Evaluate(this, tmad, ability, mightPower, out castSuccess);
                                                    if (castSuccess) goto AutoCastExit;
                                                }                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight) || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersDK)
                        {
                            if (current.abilityDef != null)
                            {
                                foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                {
                                    if ((tmad == TorannMagicDefOf.TM_Spite || tmad == TorannMagicDefOf.TM_Spite_I || tmad == TorannMagicDefOf.TM_Spite_II || tmad == TorannMagicDefOf.TM_Spite_III))
                                    {
                                        MightPower mightPower = MightData.MightPowersDK.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                        if (mightPower != null && mightPower.learned && mightPower.autocast)
                                        {
                                            ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                            AutoCast.CombatAbility.EvaluateMinRange(this, tmad, ability, mightPower, 4, out castSuccess);
                                            if (castSuccess) goto AutoCastExit;
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                    if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper) || isFaceless || isCustom) && Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        PawnAbility ability = null;
                        foreach (MightPower current in MightData.MightPowersS)
                        {
                            if (current.abilityDef != null && ((mimicAbility != null && mimicAbility.defName.Contains(current.abilityDef.defName)) || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper))) 
                            {
                                if(TM_Calc.IsUsingRanged(Pawn))
                                {
                                    foreach (TMAbilityDef tmad in current.TMabilityDefs)
                                    {
                                        if (tmad == TorannMagicDefOf.TM_AntiArmor)
                                        {
                                            MightPower mightPower = MightData.MightPowersS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_AntiArmor);
                                            if (mightPower != null && mightPower.autocast)
                                            {
                                                ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_AntiArmor);
                                                AutoCast.CombatAbility.Evaluate(this, TorannMagicDefOf.TM_AntiArmor, ability, mightPower, out castSuccess);
                                                if (castSuccess) goto AutoCastExit;
                                            }
                                        }
                                        if (tmad == TorannMagicDefOf.TM_Headshot)
                                        {
                                            MightPower mightPower = MightData.MightPowersS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Headshot);
                                            if (mightPower != null && mightPower.autocast)
                                            {
                                                ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_Headshot);
                                                AutoCast.CombatAbility.Evaluate(this, TorannMagicDefOf.TM_Headshot, ability, mightPower, out castSuccess);
                                                if (castSuccess) goto AutoCastExit;
                                            }
                                        }
                                        if ((tmad == TorannMagicDefOf.TM_DisablingShot || tmad == TorannMagicDefOf.TM_DisablingShot_I || tmad == TorannMagicDefOf.TM_DisablingShot_II || tmad == TorannMagicDefOf.TM_DisablingShot_III))
                                        {
                                            MightPower mightPower = MightData.MightPowersS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == tmad);
                                            if (mightPower != null && mightPower.learned && mightPower.autocast)
                                            {
                                                ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                                AutoCast.CombatAbility.Evaluate(this, tmad, ability, mightPower, out castSuccess);
                                                if (castSuccess) goto AutoCastExit;
                                            }                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier) || isFaceless || isCustom)
                    {
                        PawnAbility ability = null;

                        foreach (MightPower current in MightData.MightPowersSS)
                        {
                            if (current.abilityDef != null && (current.abilityDef == mimicAbility || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier)))
                            {
                                if (current.abilityDef == TorannMagicDefOf.TM_FirstAid && TM_Calc.IsPawnInjured(Pawn, .2f))
                                {
                                    MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_FirstAid);
                                    if (mightPower != null && mightPower.autocast)
                                    {
                                        ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_FirstAid);
                                        AutoCast.HealSelf.Evaluate(this, TorannMagicDefOf.TM_FirstAid, ability, mightPower, out castSuccess);
                                        if (castSuccess) goto AutoCastExit;
                                    }
                                }                                
                            }
                        }
                        if (Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                        {
                            foreach (MightPower current in MightData.MightPowersSS)
                            {
                                if (current.abilityDef != null && Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier))
                                {
                                    if (Pawn.equipment.Primary != null && Pawn.equipment.Primary.def.IsRangedWeapon)
                                    {
                                        if (specWpnRegNum != -1)
                                        {
                                            if (current.abilityDef == TorannMagicDefOf.TM_PistolWhip)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PistolWhip);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_PistolWhip);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null)
                                                    {
                                                        AutoCast.MeleeCombat_OnTarget.TryExecute(this, TorannMagicDefOf.TM_PistolWhip, ability, mightPower, targetPawn, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                            if (current.abilityDef == TorannMagicDefOf.TM_SuppressingFire)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_SuppressingFire);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_SuppressingFire);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null)
                                                    {
                                                        AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_SuppressingFire, ability, mightPower, targetPawn, 6, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                            if (current.abilityDef == TorannMagicDefOf.TM_Buckshot)
                                            {
                                                MightPower mightPower = MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Buckshot);
                                                if (mightPower != null && mightPower.autocast)
                                                {
                                                    ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_Buckshot);
                                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                                    if (targetPawn != null)
                                                    {
                                                        AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_Buckshot, ability, mightPower, targetPawn, 2, out castSuccess);
                                                        if (castSuccess) goto AutoCastExit;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ThrowingKnife);
                        if (mightPower != null && mightPower.learned && mightPower.autocast)
                        {
                            PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_ThrowingKnife);
                            Pawn targetPawn = TM_Calc.FindNearbyEnemy(Pawn, Mathf.RoundToInt(ability.Def.MainVerb.range * .9f));
                            AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_ThrowingKnife, ability, mightPower, targetPawn, 1, out castSuccess);
                            if (castSuccess) goto AutoCastExit;
                        }
                    }
                    if (Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TempestStrike);
                        if (mightPower != null && mightPower.learned && mightPower.autocast)
                        {
                            PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_TempestStrike);
                            Pawn targetPawn = TM_Calc.FindNearbyEnemy(Pawn, Mathf.RoundToInt(ability.Def.MainVerb.range * .9f));
                            AutoCast.CombatAbility_OnTarget_LoS.TryExecute(this, TorannMagicDefOf.TM_TempestStrike, ability, mightPower, targetPawn, 4, out castSuccess);
                            if (castSuccess) goto AutoCastExit;
                        }
                    }
                    if (Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                    {
                        MightPower mightPower = MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PommelStrike);                        
                        if (mightPower != null && mightPower.learned && mightPower.autocast && Pawn.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt != Pawn)
                        {
                            PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == TorannMagicDefOf.TM_PommelStrike);
                            Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                            float minPain = .3f;
                            if(MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_pwr").level >= 2)
                            {
                                minPain = .2f;
                            }
                            if (targetPawn != null && targetPawn.health?.hediffSet?.PainTotal >= minPain)
                            {
                                AutoCast.MeleeCombat_OnTarget.TryExecute(this, TorannMagicDefOf.TM_PommelStrike, ability, mightPower, targetPawn, out castSuccess);
                                if (castSuccess) goto AutoCastExit;
                            }
                        }
                    }
                }
                AutoCastExit:;
            }
        }
    }
}