using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using TorannMagic.Ideology;

namespace TorannMagic
{
    public class MagicAbility : PawnAbility
    {
       
        public CompAbilityUserMagic MagicUser
        {
            get
            {
                return MagicUserUtility.GetMagicUser(Pawn);
            }
        }

        public TMAbilityDef magicDef
        {
            get
            {
                return Def as TMAbilityDef;
            }
        }

        public float ActualBloodCost
        {
            get
            {
                float num = 1;
                if(magicDef != null)
                {
                    num *= 1f - (magicDef.efficiencyReductionPercent * MagicUser.MagicData.GetSkill_Efficiency(magicDef).level);
                    num *= (1f - (TorannMagicDefOf.TM_BloodGift.efficiencyReductionPercent /2f) * MagicUser.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_eff").level);
                }
                return magicDef.bloodCost * num;
            }
        }

        public static float ActualNeedCost (TMAbilityDef magicDef, CompAbilityUserMagic magicUser)
        {
            float num = 1f;
            if (magicDef != null && magicUser.MagicData.GetSkill_Efficiency(magicDef) != null)
            {
                num = 1f - (magicDef.efficiencyReductionPercent * magicUser.MagicData.GetSkill_Efficiency(magicDef).level);
            }
            return magicDef.needCost * num;            
        }

        public static float ActualHediffCost(TMAbilityDef magicDef, CompAbilityUserMagic magicUser)
        {
            float num = 1f;
            if (magicDef != null && magicUser.MagicData.GetSkill_Efficiency(magicDef) != null)
            {
                num = 1f - (magicDef.efficiencyReductionPercent * magicUser.MagicData.GetSkill_Efficiency(magicDef).level);
            }
            return magicDef.hediffCost * num;            
        }

        private float ActualManaCost
        {
            get
            {
                if (magicDef != null)
                {
                    return MagicUser.ActualManaCost(magicDef);
                }
                return magicDef.manaCost;         
            }
        }

        public MagicAbility()
        {
        }

        public MagicAbility(CompAbilityUser abilityUser) : base(abilityUser)
		{
            AbilityUser = (abilityUser as CompAbilityUserMagic);
        }

        public MagicAbility(Pawn user, AbilityUser.AbilityDef pdef) : base(user, pdef)
		{

        }

        public override void PostAbilityAttempt()  //commented out in CompAbilityUserMagic
        {
            //base.PostAbilityAttempt();
            
            
            if (!Pawn.IsColonist && ModOptions.Settings.Instance.AIAggressiveCasting)// for AI
            {
                CooldownTicksLeft = Mathf.RoundToInt(MaxCastingTicks/2f);
            }
            else
            {
                CooldownTicksLeft = Mathf.RoundToInt(MaxCastingTicks * MagicUser.coolDown);
            }
            if(Rand.Chance(MagicUser.arcalleumCooldown))
            {
                CooldownTicksLeft = 4;
            }
            if (magicDef != null)
            {
                if (Pawn.IsColonist)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(TorannMagicDefOf.TM_UsedMagic, Pawn.Named(HistoryEventArgsNames.Doer), Pawn.Named(HistoryEventArgsNames.Subject), Pawn.Named(HistoryEventArgsNames.AffectedFaction), Pawn.Named(HistoryEventArgsNames.Victim)), true);
                }
                if (MagicUser.Mana != null)
                {
                    if(!Pawn.IsColonist && ModOptions.Settings.Instance.AIAggressiveCasting)// for AI
                    {
                        MagicUser.Mana.UseMagicPower(MagicUser.ActualManaCost(magicDef)/2f);
                    }
                    else
                    {                       
                        MagicUser.Mana.UseMagicPower(MagicUser.ActualManaCost(magicDef));
                    }
                                       
                    if(magicDef != TorannMagicDefOf.TM_TransferMana && magicDef.abilityHediff == null)
                    {                        
                        MagicUser.MagicUserXP += Mathf.Clamp((int)((magicDef.manaCost * 300) * MagicUser.xpGain * ModOptions.Settings.Instance.xpMultiplier), 0, 9999);
                    }

                    TM_EventRecords er = new TM_EventRecords();
                    er.eventPower = magicDef.manaCost;
                    er.eventTick = Find.TickManager.TicksGame;
                    MagicUser.MagicUsed.Add(er);      
                    
                    if(magicDef == TorannMagicDefOf.TM_TechnoWeapon && (Pawn.Downed || Pawn.Dead) && Pawn.Map != null)
                    {
                        foreach(Thing t in Pawn.Position.GetThingList(Pawn.Map))
                        {
                            if(t.def.defName.StartsWith("TM_TechnoWeapon"))
                            {
                                t.Destroy(DestroyMode.Vanish);
                                break;
                            }
                        }
                    }
                }
                else if (MagicUser.Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    CompAbilityUserMight mightComp = MagicUser.Pawn.GetCompAbilityUserMight();
                    mightComp.Stamina.UseMightPower(magicDef.manaCost);
                    mightComp.MightUserXP += Mathf.Clamp((int)((magicDef.manaCost * 180) * mightComp.xpGain * ModOptions.Settings.Instance.xpMultiplier),0,9999);
                }
                if (magicDef.staminaCost != 0)
                {
                    CompAbilityUserMight mightComp = Pawn.GetCompAbilityUserMight();
                    if (mightComp != null && mightComp.Stamina != null)
                    {
                        mightComp.Stamina.UseMightPower(magicDef.staminaCost);
                        //this.MagicUser.Mana.UseMagicPower(this.MagicUser.ActualManaCost(magicDef)
                    }
                }
                if (magicDef.bloodCost != 0)
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_BloodHD"), -100 * ActualBloodCost);
                }
                if (magicDef.requiredHediff != null)
                {
                    Hediff reqHediff = TM_Calc.GetLinkedHediff(Pawn, magicDef.requiredHediff);
                    if (reqHediff != null)
                    {
                        reqHediff.Severity -= ActualHediffCost(magicDef, MagicUser);
                        MagicUser.MagicUserXP += Mathf.Clamp((int)((magicDef.hediffXPFactor * MagicUser.xpGain * ModOptions.Settings.Instance.xpMultiplier) * magicDef.hediffCost),0,9999);
                    }
                    else
                    {
                        Log.Warning("" + Pawn.LabelShort + " attempted to use an ability requiring the hediff " + magicDef.requiredHediff.label + " but does not have the hediff; should never happen since we required the hediff to use the ability.");
                    }
                }
                if (magicDef.requiredNeed != null)
                {
                    if (Pawn.needs != null && Pawn.needs.AllNeeds != null && Pawn.needs.TryGetNeed(magicDef.requiredNeed) != null)
                    {
                        Need nd = Pawn.needs.TryGetNeed(magicDef.requiredNeed);
                        nd.CurLevel -= ActualNeedCost(magicDef, MagicUser);
                        MagicUser.MagicUserXP += Mathf.Clamp((int)((magicDef.needXPFactor * MagicUser.xpGain * ModOptions.Settings.Instance.xpMultiplier) * magicDef.needCost),0,9999);
                    }
                    else
                    {
                        Log.Warning("" + Pawn.LabelShort + " attempted to use an ability requiring the need " + magicDef.requiredNeed.label + " but does not have the need; should never happen since we required the need to use the ability.");
                    }
                }
                if((magicDef.requiredInspiration != null || magicDef.requiresAnyInspiration) && magicDef.consumesInspiration)
                {
                    if (Pawn.mindState != null && Pawn.mindState.inspirationHandler != null && Pawn.Inspiration != null)
                    {
                        Pawn.mindState.inspirationHandler.EndInspiration(Pawn.Inspiration);
                    }
                }
                if(magicDef.chainedAbility != null)
                {
                    if (magicDef.chainedAbilityTraitRequirements != null && magicDef.chainedAbilityTraitRequirements.Count > 0)
                    {
                        foreach (TraitDef reqTrait in magicDef.chainedAbilityTraitRequirements)
                        {
                            if (Pawn.story.traits.allTraits.Any(t => t.def == reqTrait))
                            {
                                AddChainedAbility(magicDef);
                                break;
                            }
                        }
                    }
                    else
                    {
                        AddChainedAbility(magicDef);
                    }                    
                }
                if(magicDef.removeAbilityAfterUse)
                {
                    MagicUser.RemovePawnAbility(magicDef);
                }
                if(magicDef.abilitiesRemovedWhenUsed != null && magicDef.abilitiesRemovedWhenUsed.Count > 0)
                {
                    foreach(TMAbilityDef rem in magicDef.abilitiesRemovedWhenUsed)
                    {
                        MagicUser.RemovePawnAbility(rem);
                    }
                }
            }                       
        }

        private void AddChainedAbility(TMAbilityDef magicDef)
        {
            MagicUser.TryAddPawnAbility(magicDef.chainedAbility);
            bool expires = false;
            int expireTicks = -1;
            if (magicDef.chainedAbilityExpiresAfterTicks >= 0)
            {
                expires = true;
                expireTicks = magicDef.chainedAbilityExpiresAfterTicks;
            }
            else if (magicDef.chainedAbilityExpiresAfterCooldown)
            {
                expires = true;
                expireTicks = CooldownTicksLeft;
            }
            if (expires)
            {
                CompAbilityUserMagic.ChainedMagicAbility cab = new CompAbilityUserMagic.ChainedMagicAbility(magicDef.chainedAbility, expireTicks, expires);
                MagicUser.chainedAbilitiesList.Add(cab);
            }
        }

        public override string PostAbilityVerbCompDesc(VerbProperties_Ability verbDef)
        {
            string result = "";
            StringBuilder stringBuilder = new StringBuilder();
            TMAbilityDef magicAbilityDef = (TMAbilityDef)verbDef.abilityDef;
            bool flag = magicAbilityDef != null;
            if (flag)
            {
                string text = "";
                string text2 = "";
                string text3 = "";
                float num = 0;
                float num2 = 0;
                
               
                if (magicAbilityDef == TorannMagicDefOf.TM_Teleport)
                {
                    num = MagicUser.ActualManaCost(magicDef)*100;
                    MagicPowerSkill mps2 = MagicUser.MagicData.MagicPowerSkill_Teleport.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Teleport_ver");
                    MagicPowerSkill mps1 = MagicUser.MagicData.MagicPowerSkill_Teleport.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Teleport_pwr");
                    num2 = 80 + (mps1.level * 20) + (mps2.level * 20);
                    text2 = "TM_AbilityDescPortalTime".Translate(
                        num2.ToString()
                    );
                }
                else if (magicAbilityDef == TorannMagicDefOf.TM_SummonMinion)
                {
                    num = MagicUser.ActualManaCost(magicDef)*100;
                    MagicPowerSkill mps1 = MagicUser.MagicData.MagicPowerSkill_SummonMinion.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SummonMinion_ver");
                    num2 = 1200 + (600 * mps1.level);
                    text2 = "TM_AbilityDescSummonDuration".Translate(
                        num2.ToString()
                    );
                }
                else if (magicAbilityDef == TorannMagicDefOf.TM_SummonPylon)
                {
                    num = MagicUser.ActualManaCost(magicDef)*100;
                    MagicPowerSkill mps1 = MagicUser.MagicData.MagicPowerSkill_SummonPylon.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SummonPylon_ver");
                    num2 = 240 + (120 * mps1.level);
                    text2 = "TM_AbilityDescSummonDuration".Translate(
                        num2.ToString()
                    );
                }
                else if (magicAbilityDef == TorannMagicDefOf.TM_SummonExplosive)
                {
                    num = MagicUser.ActualManaCost(magicDef) * 100;
                    MagicPowerSkill mps1 = MagicUser.MagicData.MagicPowerSkill_SummonExplosive.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SummonExplosive_ver");
                    num2 = 240 + (120 * mps1.level);
                    text2 = "TM_AbilityDescSummonDuration".Translate(
                        num2.ToString()
                    );
                }
                else if (magicAbilityDef == TorannMagicDefOf.TM_SummonElemental)
                {
                    num = MagicUser.ActualManaCost(magicDef) * 100;
                    MagicPowerSkill mps1 = MagicUser.MagicData.MagicPowerSkill_SummonElemental.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SummonElemental_ver");
                    num2 = 30 + (15 * mps1.level);
                    text2 = "TM_AbilityDescSummonDuration".Translate(
                        num2.ToString()
                    );
                }
                else if (magicAbilityDef == TorannMagicDefOf.TM_PsychicShock)
                {
                    num = MagicUser.ActualManaCost(magicDef) * 100;
                    num2 = MagicUser.Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false);
                    text3 = "TM_PsychicSensitivity".Translate(
                        num2.ToString()
                    );
                }
                else
                {
                    num = MagicUser.ActualManaCost(magicDef) * 100;
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {                    
                    text = "TM_AbilityDescBaseStaminaCost".Translate(
                        (magicAbilityDef.manaCost * 100).ToString("n1")
                    ) + "\n" + "TM_AbilityDescAdjustedStaminaCost".Translate(
                        (magicDef.manaCost * 100).ToString("n1")
                    );
                }
                else if (magicAbilityDef.requiredHediff != null)
                {
                    text = "TM_AbilityDescBaseResourceCost".Translate(magicAbilityDef.requiredHediff.label,
                        ((magicAbilityDef.hediffCost).ToString("n2"))
                    ) + "\n" + "TM_AbilityDescAdjustedResourceCost".Translate(magicAbilityDef.requiredHediff.label,
                        (ActualHediffCost(magicAbilityDef, MagicUser).ToString("n2"))
                    );
                }
                else if (magicAbilityDef.requiredNeed != null)
                {
                    text = "TM_AbilityDescBaseResourceCost".Translate(magicAbilityDef.requiredNeed.label,
                        (magicAbilityDef.needCost).ToString("n2")
                    ) + "\n" + "TM_AbilityDescAdjustedResourceCost".Translate(magicAbilityDef.requiredNeed.label,
                        (ActualNeedCost(magicAbilityDef, MagicUser).ToString("n2"))
                    );
                }
                else
                {
                    text = "TM_AbilityDescBaseManaCost".Translate(
                        (magicAbilityDef.manaCost * 100).ToString("n1")
                    ) + "\n" + "TM_AbilityDescAdjustedManaCost".Translate(
                        num.ToString("n1")
                    );
                }

                if(magicAbilityDef == TorannMagicDefOf.TM_IgniteBlood || magicAbilityDef == TorannMagicDefOf.TM_BloodShield || magicAbilityDef == TorannMagicDefOf.TM_BloodForBlood || 
                    magicAbilityDef == TorannMagicDefOf.TM_Rend || magicAbilityDef == TorannMagicDefOf.TM_Rend_I || magicAbilityDef == TorannMagicDefOf.TM_Rend_II || magicAbilityDef == TorannMagicDefOf.TM_Rend_III ||
                    magicAbilityDef == TorannMagicDefOf.TM_BloodMoon || magicAbilityDef == TorannMagicDefOf.TM_BloodMoon_I || magicAbilityDef == TorannMagicDefOf.TM_BloodMoon_II || magicAbilityDef == TorannMagicDefOf.TM_BloodMoon_III)
                {
                    num = ActualBloodCost * 100;
                    text = "TM_AbilityDescBaseBloodCost".Translate(
                    (magicAbilityDef.bloodCost * 100).ToString("n1")
                    ) + "\n" + "TM_AbilityDescAdjustedBloodCost".Translate(
                        num.ToString("n1")
                    );
                }

                if(MagicUser.coolDown != 1f)
                {
                    text3 = "TM_AdjustedCooldown".Translate(
                        ((MaxCastingTicks * MagicUser.coolDown)/60).ToString("0.00")
                    );
                }

                if(magicAbilityDef == TorannMagicDefOf.TM_Firebolt)
                {
                    text2 = "TM_BonusDamage".Translate(
                        Mathf.RoundToInt((float)magicAbilityDef.MainVerb.defaultProjectile.projectile.GetDamageAmount(1, null) / 3f * (float)MagicUser.MagicData.MagicPowerSkill_Firebolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Firebolt_pwr").level)
                    );
                }

                bool flag2 = text != "";
                if (flag2)
                {
                    stringBuilder.AppendLine(text);
                }
                bool flag3 = text2 != "";
                if (flag3)
                {
                    stringBuilder.AppendLine(text2);
                }
                bool flag4 = text3 != "";
                if(flag4)
                {
                    stringBuilder.AppendLine(text3);
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        public override bool CanCastPowerCheck(AbilityContext context, out string reason)
        {
            bool flag = base.CanCastPowerCheck(context, out reason);
            bool result;
            if (flag)
            {
                reason = "";
                TMAbilityDef tmAbilityDef;
                bool flag1 = Def != null && (tmAbilityDef = (Def as TMAbilityDef)) != null;
                if (flag1)
                {
                    bool flag4 = MagicUser.Mana != null;
                    if (flag4)
                    {
                        bool flag5 = magicDef.manaCost > 0f && ActualManaCost > MagicUser.Mana.CurLevel;
                        if (flag5)
                        {
                            reason = "TM_NotEnoughMana".Translate(
                                Pawn.LabelShort
                            );
                            result = false;
                            return result;
                        }
                        if (magicDef.staminaCost > 0f)
                        {
                            CompAbilityUserMight compMight = Pawn.GetCompAbilityUserMight();
                            if(compMight != null && compMight.Stamina != null)
                            {
                                if(magicDef.staminaCost > compMight.Stamina.CurLevel)
                                {
                                    reason = "TM_NotEnoughStamina".Translate(
                                    Pawn.LabelShort
                                    );
                                    result = false;
                                    return result;
                                }
                            }
                        }
                        if (magicDef.bloodCost > 0f)
                        {
                            bool flag6 = MagicUser.Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_BloodHD"), false) ? (ActualBloodCost * 100) > MagicUser.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_BloodHD"), false).Severity : true;
                            if (flag6)
                            {
                                reason = "TM_NotEnoughBlood".Translate(
                                    Pawn.LabelShort
                                );
                                result = false;
                                return result;
                            }
                        }
                        bool flagMute = MagicUser.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MuteHD);
                        if(flagMute)
                        {
                            reason = "TM_CasterMute".Translate(
                                Pawn.LabelShort
                            );
                            result = false;
                            return result;
                        }
                        bool flagNeed = magicDef.requiredNeed != null; ;
                        if(flagNeed)
                        {
                            if (MagicUser.Pawn.needs.TryGetNeed(magicDef.requiredNeed) != null)
                            {
                                if(MagicUser.Pawn.needs.TryGetNeed(magicDef.requiredNeed).CurLevel < ActualNeedCost(magicDef, MagicUser))
                                {
                                    reason = "TM_NotEnoughEnergy".Translate(
                                        Pawn.LabelShort,
                                        magicDef.requiredNeed.label
                                    );
                                    result = false;
                                    return result;
                                }
                                //passes need requirements
                            }
                            else
                            {
                                reason = "TM_NoRequiredNeed".Translate(
                                    Pawn.LabelShort,
                                    magicDef.requiredNeed.label
                                );
                                result = false;
                                return result;
                            }
                        }

                        bool flagHediff = magicDef.requiredHediff != null;
                        if (flagHediff)
                        {
                            Hediff reqHediff = TM_Calc.GetLinkedHediff(Pawn, magicDef.requiredHediff);
                            if (reqHediff != null)
                            {
                                if (reqHediff.Severity < ActualHediffCost(magicDef, MagicUser))
                                {
                                    reason = "TM_NotEnoughEnergy".Translate(
                                        Pawn.LabelShort,
                                        magicDef.requiredHediff.label
                                    );
                                    result = false;
                                    return result;
                                }
                                //passes hediff requirements
                            }
                            else
                            {
                                reason = "TM_NoRequiredHediff".Translate(
                                    Pawn.LabelShort,
                                    magicDef.requiredHediff.label
                                );
                                result = false;
                                return result;
                            }                            
                        }

                        bool flagInspiration = magicDef.requiredInspiration != null;
                        if (flagInspiration)
                        {
                            if (Pawn.mindState != null && Pawn.mindState.inspirationHandler != null && Pawn.InspirationDef != null && Pawn.mindState.inspirationHandler.CurStateDef == magicDef.requiredInspiration)
                            {
                                //passes hediff requirements
                            }
                            else
                            {
                                reason = "TM_NoRequiredInspiration".Translate(
                                        Pawn.LabelShort,
                                        magicDef.requiredInspiration.label
                                    );
                                result = false;
                                return result;
                            }
                        }

                        if (magicDef.requiresAnyInspiration)
                        {
                            if (!Pawn.Inspired)
                            {
                                reason = "TM_NotInspired".Translate(
                                        Pawn.LabelShort
                                    );
                                result = false;
                                return result;
                            }
                        }
                    }
                    else if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                    {
                        CompAbilityUserMight mightComp = Pawn.GetCompAbilityUserMight();
                        bool flag7 = mightComp != null && mightComp.Stamina != null && magicDef.manaCost > 0f && magicDef.manaCost > mightComp.Stamina.CurLevel;
                        if (flag7)
                        {
                            reason = "TM_NotEnoughStamina".Translate(
                            Pawn.LabelShort
                            );
                            result = false;
                            return result;
                        }
                    }
                    TMAbilityDef tmad = magicDef;
                    if (tmad != null && tmad.requiredWeaponsOrCategories != null && tmad.IsRestrictedByEquipment(Pawn))
                    {
                        reason = "TM_IncompatibleWeaponType".Translate(
                            Pawn.LabelShort,
                            tmad.label);
                        return false;
                    }
                    //if (magicDef == TorannMagicDefOf.TM_HarvestPassion && !Pawn.Inspired)
                    //{
                    //    reason = "TM_MustHaveInspiration".Translate(
                    //            base.Pawn.LabelShort,
                    //            magicDef.label
                    //        );
                    //    result = false;
                    //    return result;
                    //}
                }
                List<Apparel> wornApparel = Pawn.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    if (!wornApparel[i].AllowVerbCast(Verb) &&
                        (magicDef.defName == "TM_LightningCloud" || magicDef.defName == "Laser_LightningBolt" || magicDef.defName == "TM_LightningStorm" || magicDef.defName == "TM_EyeOfTheStorm" ||
                        magicDef.defName.Contains("Laser_FrostRay") || magicDef.defName == "TM_Blizzard" || magicDef.defName == "TM_Snowball" || magicDef.defName == "TM_Icebolt" ||
                        magicDef.defName == "TM_Firestorm" || magicDef.defName == "TM_Fireball" || magicDef.defName == "TM_Fireclaw" || magicDef.defName == "TM_Firebolt" ||
                        magicDef.defName.Contains("TM_MagicMissile") ||
                        magicDef.defName.Contains("TM_DeathBolt") ||
                        magicDef.defName.Contains("TM_ShadowBolt") ||
                        magicDef.defName == "TM_BloodForBlood" || magicDef.defName == "TM_IgniteBlood" ||
                        magicDef.defName == "TM_Poison" ||
                        magicDef == TorannMagicDefOf.TM_ChainLightning ||
                        magicDef == TorannMagicDefOf.TM_ArcaneBolt) )
                    {
                        reason = "TM_ShieldBlockingPowers".Translate(
                            Pawn.Label,
                            wornApparel[i].Label
                        );
                        return false;
                    }
                }
                result = true;
                
            }
            else
            {
                result = false;
            }
            return result;

        }

        public new Command_PawnAbility GetGizmo()
        {
            Command_PawnAbility command_PawnAbility = new Command_PawnAbility(AbilityUser, this, CooldownTicksLeft)
            {
                verb = Verb,
                defaultLabel = magicDef.LabelCap,
                Order = 9999
            };
            command_PawnAbility.curTicks = CooldownTicksLeft;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(magicDef.GetDescription());
            stringBuilder.AppendLine(PostAbilityVerbCompDesc(Verb.UseAbilityProps));
            command_PawnAbility.defaultDesc = stringBuilder.ToString();
            stringBuilder = null;
            command_PawnAbility.targetingParams = magicDef.MainVerb.targetParams;
            command_PawnAbility.icon = magicDef.uiIcon;
            command_PawnAbility.action = delegate (LocalTargetInfo target)
            {
                LocalTargetInfo target2 = GenCollection.FirstOrFallback<LocalTargetInfo>(GenUI.TargetsAt(UI.MouseMapPosition(), Verb.verbProps.targetParams, false, null), target);
                TryCastAbility(AbilityContext.Player, target2);
            };
            string reason = "";
            if (!CanCastPowerCheck(AbilityContext.Player, out reason))
            {
                command_PawnAbility.Disable(reason);
            }
            return command_PawnAbility;
        }
    }
}
