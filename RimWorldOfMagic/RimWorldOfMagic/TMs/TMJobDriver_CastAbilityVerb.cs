using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using AbilityUser;
using UnityEngine;


namespace TorannMagic
{
    public class TMJobDriver_CastAbilityVerb : JobDriver_CastAbilityVerb
    { 

        private int duration;
        public AbilityContext context => job.count == 1 ? AbilityContext.Player : AbilityContext.AI;
        public Verb_UseAbility verb; //new Verb_UseAbility(); // = this.pawn.CurJob.verbToUse as Verb_UseAbility;
        private bool wildCheck;
        private bool cooldownFlag;
        private bool energyFlag;
        private bool validCastFlag;

        public override void ExposeData()
        {
            //Scribe_Deep.Look<Verb_UseAbility>(ref verb, "verb");
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);            
            Pawn targetPawn = null;

            verb = pawn.CurJob.verbToUse as Verb_UseAbility;            
            if(verb != null)
            {
                if (TargetA.HasThing && TargetA.Thing is Pawn && (!pawn.Position.InHorDistOf(TargetA.Cell, pawn.CurJob.verbToUse.verbProps.range) || !Verb.UseAbilityProps.canCastInMelee))
                {
                    Toil toil = Toils_Combat.GotoCastPosition(TargetIndex.A);
                    yield return toil;
                }                
                if (Context == AbilityContext.Player)
                {
                    Find.Targeter.targetingSource = verb;
                }
                
                if (TargetThingA != null)
                {
                    targetPawn = TargetThingA as Pawn;
                }

                cooldownFlag = verb.Ability.CooldownTicksLeft > 0;
                
                if (verb.Ability.Def is TMAbilityDef tmAbility)
                {
                    CompAbilityUserMight compMight = pawn.GetCompAbilityUserMight();
                    CompAbilityUserMagic compMagic = pawn.GetCompAbilityUserMagic();
                    //if (compMagic != null)
                    //{
                    //    compMagic.AIAbilityJob = null;
                    //}
                    //if (compMight != null && false)
                    //{
                    //    //compMight.AIAbilityJob = null;
                    //}
                    if (tmAbility.manaCost > 0 && pawn.story != null && pawn.story.traits != null && !pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                    {
                        if (pawn.Map.gameConditionManager.ConditionIsActive(TorannMagicDefOf.TM_ManaStorm))
                        {
                            //DamageInfo dinfo2;
                            //BodyPartRecord vitalPart = null;
                            int amt = Mathf.RoundToInt(compMagic.ActualManaCost(tmAbility) * 100f);
                            //IEnumerable<BodyPartRecord> partSearch = pawn.def.race.body.AllParts;
                            //vitalPart = partSearch.FirstOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource));
                            //dinfo2 = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_Arcane, amt, 10, 0, pawn as Thing, vitalPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
                            //dinfo2.SetAllowDamagePropagation(false);
                            //pawn.TakeDamage(dinfo2);
                            if (amt > 5)
                            {
                                pawn.Map.weatherManager.eventHandler.AddEvent(new TM_WeatherEvent_MeshFlash(Map, pawn.Position, TM_MatPool.blackLightning, TMDamageDefOf.DamageDefOf.TM_Arcane, pawn, amt, Mathf.Clamp((float)amt / 5f, 1f, 5f)));
                            }
                        }
                        if (compMagic != null && compMagic.Mana != null)
                        {
                            if (compMagic.ActualManaCost(tmAbility) > compMagic.Mana.CurLevel)
                            {
                                energyFlag = true;
                            }
                        }
                        else
                        {
                            energyFlag = true;
                        }
                    }
                    if (tmAbility.staminaCost > 0)
                    {
                        if (compMight != null && compMight.Stamina != null)
                        {
                            if (compMight.ActualStaminaCost(tmAbility) > compMight.Stamina.CurLevel)
                            {
                                energyFlag = true;
                            }
                        }
                        else
                        {
                            energyFlag = true;
                        }
                    }
                }
            }
            validCastFlag = cooldownFlag || energyFlag;

            if (targetPawn != null && (verb != null || pawn.CurJob.verbToUse != null))
            {
                //yield return Toils_Combat.CastVerb(TargetIndex.A, false);
                Toil combatToil = new Toil();
                //combatToil.FailOnDestroyedOrNull(TargetIndex.A);
                //combatToil.FailOnDespawnedOrNull(TargetIndex.A);
                //combatToil.FailOnDowned(TargetIndex.A);
                //CompAbilityUserMagic comp = this.pawn.GetCompAbilityUserMagic();                
                //JobDriver curDriver = this.pawn.jobs.curDriver;

                combatToil.initAction = delegate
                {
                    if (combatToil.actor.jobs.curJob.verbToUse == null) EndJobWith(JobCondition.Errored);
                    verb = combatToil.actor.jobs.curJob.verbToUse as Verb_UseAbility;

                    if (verb != null && verb.verbProps != null)
                    {

                        try
                        {
                            duration = (int)((verb.verbProps.warmupTime * 60) * pawn.GetStatValue(StatDefOf.AimingDelayFactor, false));
                        }
                        catch
                        {
                            duration = (int)(verb.verbProps.warmupTime * 60);
                        }

                        if (pawn.RaceProps.Humanlike)
                        {
                            //if (this.pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                            //{
                            //    RemoveMimicAbility(verb);
                            //}

                            if (pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic) && !validCastFlag)
                            {
                                PsionicEnergyCost(verb);
                            }

                            if (pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight) && !validCastFlag)
                            {
                                HateCost(verb);
                            }
                            if (validCastFlag)
                            {
                                Messages.Message("TM_InvalidAbility".Translate(pawn.LabelShort, verb.Ability.Def.label), MessageTypeDefOf.RejectInput, false);
                                EndJobWith(JobCondition.Incompletable);
                            }
                        }

                        LocalTargetInfo target = combatToil.actor.jobs.curJob.GetTarget(TargetIndex.A);
                        if (target != null && !validCastFlag) 
                        {
                            verb.TryStartCastOn(target, false, true);                            
                        }
                        using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Hediff rec = enumerator.Current;
                                if (rec.def == TorannMagicDefOf.TM_PossessionHD || rec.def == TorannMagicDefOf.TM_DisguiseHD || rec.def == TorannMagicDefOf.TM_DisguiseHD_I || rec.def == TorannMagicDefOf.TM_DisguiseHD_II || rec.def == TorannMagicDefOf.TM_DisguiseHD_III)
                                {
                                    pawn.health.RemoveHediff(rec);
                                }
                            }
                        }
                    }
                    else
                    {
                        EndJobWith(JobCondition.Errored);
                    }
                };
                combatToil.tickAction = delegate
                {
                    if(pawn.Downed) EndJobWith(JobCondition.InterruptForced);
                    if (combatToil.actor.jobs.curJob.verbToUse == null) EndJobWith(JobCondition.Errored);

                    if (Find.TickManager.TicksGame % 12 == 0)
                    {
                        if (verb.Ability.Def == TorannMagicDefOf.TM_Artifact_TraitThief || verb.Ability.Def == TorannMagicDefOf.TM_Artifact_TraitInfuse)
                        {
                            float direction = Rand.Range(0, 360);
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi_Arcane, pawn.DrawPos, pawn.Map, Rand.Range(.1f, .4f), 0.2f, .02f, .1f, 0, Rand.Range(8, 10), direction, direction);
                        }
                        else
                        {
                            TM_MoteMaker.ThrowCastingMote(pawn.DrawPos, pawn.Map, Rand.Range(1.2f, 2f));
                        }
                    }
                    
                    duration--;
                    if (!wildCheck && duration <= 6)
                    {
                        wildCheck = true;
                        if (pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TorannMagicDefOf.ChaosMage) && Rand.Chance(.1f))
                        {
                            verb.Ability.PostAbilityAttempt();
                            TM_Action.DoWildSurge(pawn, pawn.GetCompAbilityUserMagic(), (MagicAbility)verb.Ability, (TMAbilityDef)verb.Ability.Def, TargetA);
                            EndJobWith(JobCondition.InterruptForced);
                        }
                    }
                };
                combatToil.AddFinishAction(delegate
                {

                    if (duration <= 5 && !pawn.DestroyedOrNull() && !pawn.Dead && !pawn.Downed)
                    {
                        //ShootLine shootLine;
                        //bool validTarg = verb.TryFindShootLineFromTo(pawn.Position, TargetLocA, out shootLine);
                        //bool inRange = (pawn.Position - TargetLocA).LengthHorizontal < verb.verbProps.range;
                        //if (inRange && validTarg)
                        //{
                        if (verb != null && verb.Ability != null && verb.Ability.Def is TMAbilityDef)
                        { 
                            TMAbilityDef tmad = (TMAbilityDef)(verb.Ability.Def);
                            if (tmad != null && tmad.relationsAdjustment != 0 && targetPawn.Faction != null && targetPawn.Faction != pawn.Faction && !targetPawn.Faction.HostileTo(pawn.Faction))
                            {
                                targetPawn.Faction.TryAffectGoodwillWith(pawn.Faction, tmad.relationsAdjustment, true, false, TorannMagicDefOf.TM_OffensiveMagic, null);
                            }
                        }
                        verb.Ability.PostAbilityAttempt();
                        pawn.ClearReservationsForJob(job);
                        //}
                    } 
                });
                //if (combatToil.actor.CurJob != this.job)
                //{
                //    curDriver.ReadyForNextToil();
                //}
                combatToil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
                pawn.ClearReservationsForJob(job);
                yield return combatToil;
                //Toil toil2 = new Toil()
                //{
                //    initAction = () =>
                //    {
                //        if (curJob.UseAbilityProps.isViolent)
                //        {
                //            JobDriver_CastAbilityVerb.CheckForAutoAttack(this.pawn);
                //        }
                //    },
                //    defaultCompleteMode = ToilCompleteMode.Instant
                //};
                //yield return toil2;
                //Toil toil1 = new Toil()
                //{
                //    initAction = () => curJob.Ability.PostAbilityAttempt(),
                //    defaultCompleteMode = ToilCompleteMode.Instant
                //};
                //yield return toil1;
            }
            else
            {

                if (verb != null && verb.verbProps != null && (pawn.Position - TargetLocA).LengthHorizontal < verb.verbProps.range)
                {
                    if (TargetLocA.IsValid && TargetLocA.InBoundsWithNullCheck(pawn.Map) && !TargetLocA.Fogged(pawn.Map))  //&& TargetLocA.Walkable(pawn.Map)
                    {
                        ShootLine shootLine;
                        bool validTarg = verb.TryFindShootLineFromTo(pawn.Position, TargetLocA, out shootLine);
                        if (validTarg)
                        {
                            //yield return Toils_Combat.CastVerb(TargetIndex.A, false);
                            //Toil toil2 = new Toil()
                            //{
                            //    initAction = () =>
                            //    {
                            //        if (curJob.UseAbilityProps.isViolent)
                            //        {
                            //            JobDriver_CastAbilityVerb.CheckForAutoAttack(this.pawn);
                            //        }

                            //    },
                            //    defaultCompleteMode = ToilCompleteMode.Instant
                            //};
                            //yield return toil2;
                            try
                            {
                                duration = (int)((verb.verbProps.warmupTime * 60) * pawn.GetStatValue(StatDefOf.AimingDelayFactor, false));
                            }
                            catch
                            {
                                duration = (int)(verb.verbProps.warmupTime * 60);
                            }
                            LocalTargetInfo target = TargetLocA;
                            Toil toil = new Toil();
                            toil.initAction = delegate
                            {

                                if (toil.actor.jobs.curJob.verbToUse == null) EndJobWith(JobCondition.Errored);
                                verb = toil.actor.jobs.curJob.verbToUse as Verb_UseAbility;
                                if (pawn.RaceProps.Humanlike)
                                {
                                    //if (this.pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                                    //{
                                    //    RemoveMimicAbility(verb);                                        
                                    //}

                                    if(pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic) && !validCastFlag)
                                    {
                                        PsionicEnergyCost(verb);
                                    }

                                    if (validCastFlag)
                                    {
                                        Messages.Message("TM_InvalidAbility".Translate(pawn.LabelShort, verb.Ability.Def.label), MessageTypeDefOf.RejectInput, false);
                                        EndJobWith(JobCondition.Incompletable);
                                    }

                                }

                                bool canFreeIntercept2 = false;
                                if (target != null && !validCastFlag)
                                {
                                    verb.TryStartCastOn(target, false, canFreeIntercept2);
                                }
                                using (IEnumerator<Hediff> enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        Hediff rec = enumerator.Current;
                                        if (rec.def == TorannMagicDefOf.TM_PossessionHD || rec.def == TorannMagicDefOf.TM_DisguiseHD || rec.def == TorannMagicDefOf.TM_DisguiseHD_I || rec.def == TorannMagicDefOf.TM_DisguiseHD_II || rec.def == TorannMagicDefOf.TM_DisguiseHD_III)
                                        {
                                            pawn.health.RemoveHediff(rec);
                                        }
                                    }
                                }
                            };
                            toil.tickAction = delegate
                            {

                                if (toil.actor.jobs.curJob.verbToUse == null) EndJobWith(JobCondition.Errored);
                                if (Find.TickManager.TicksGame % 12 == 0)
                                {
                                    TM_MoteMaker.ThrowCastingMote(pawn.DrawPos, pawn.Map, Rand.Range(1.2f, 2f));
                                }
                                duration--;
                                if (!wildCheck && duration <= 6)
                                {
                                    wildCheck = true;
                                    if (pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TorannMagicDefOf.ChaosMage) && Rand.Chance(.1f))
                                    {                                        
                                        bool completeJob = TM_Action.DoWildSurge(pawn, pawn.GetCompAbilityUserMagic(), (MagicAbility)verb.Ability, (TMAbilityDef)verb.Ability.Def, TargetA);
                                        if (!completeJob)
                                        {
                                            verb.Ability.PostAbilityAttempt();
                                            EndJobWith(JobCondition.InterruptForced);
                                        }
                                    }
                                }
                            };
                            toil.AddFinishAction(delegate
                            {                                
                                if (duration <= 5 && !pawn.DestroyedOrNull() && !pawn.Dead && !pawn.Downed)
                                {
                                    verb.Ability.PostAbilityAttempt();
                                }
                                pawn.ClearReservationsForJob(job);
                            });
                            toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
                            yield return toil;

                            //Toil toil1 = new Toil()
                            //{
                            //    initAction = () => curJob.Ability.PostAbilityAttempt(),
                            //    defaultCompleteMode = ToilCompleteMode.Instant
                            //};
                            //yield return toil1;
                        }
                        else
                        {
                            //No LoS
                            if (pawn.IsColonist)
                            {
                                Messages.Message("TM_OutOfLOS".Translate(
                                    pawn.LabelShort
                                ), MessageTypeDefOf.RejectInput);
                            }
                            pawn.ClearAllReservations(false);
                        }
                    }
                    else
                    {
                        pawn.ClearAllReservations(false);
                    }
                }
                else
                {
                    if(verb == null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Errored, false, true);
                    }
                    else if (pawn.IsColonist)
                    {
                        //out of range
                        Messages.Message("TM_OutOfRange".Translate(), MessageTypeDefOf.RejectInput);
                    }
                }
            }
        }

        private void PsionicEnergyCost(Verb_UseAbility verbCast)
        {
            if (verbCast.AbilityProjectileDef.defName == "TM_Projectile_PsionicBlast")
            {
                HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TM_PsionicHD"), -20f);
            }
            else if (verbCast.AbilityProjectileDef.defName == "Projectile_PsionicDash")
            {
                float sevReduct = 8f - pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicDash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicDash_eff").level;
                HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TM_PsionicHD"), -sevReduct);
            }
            else if(verbCast.AbilityProjectileDef.defName == "Projectile_PsionicStorm")
            {
                //float sevReduct = 65 - (5 * this.pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicStorm_eff").level);
                HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TM_PsionicHD"), -100);
            }
        }

        private void HateCost(Verb_UseAbility verbCast)
        {
            Hediff hediff = null;
            for (int h = 0; h < pawn.health.hediffSet.hediffs.Count; h++)
            {
                if (pawn.health.hediffSet.hediffs[h].def.defName.Contains("TM_HateHD"))
                {
                    hediff = pawn.health.hediffSet.hediffs[h];
                }
            }
            if (hediff != null && verbCast.AbilityProjectileDef.defName == "Projectile_Spite")
            {
                HealthUtility.AdjustSeverity(pawn, hediff.def, -20f);
            }            
        }

        private void RemoveMimicAbility(Verb_UseAbility verbCast)
        {
            CompAbilityUserMight mightComp = pawn.GetCompAbilityUserMight();
            CompAbilityUserMagic magicComp = pawn.GetCompAbilityUserMagic();
            if (mightComp.mimicAbility != null && mightComp.mimicAbility.MainVerb.verbClass == verbCast.verbProps.verbClass)
            {
                mightComp.RemovePawnAbility(mightComp.mimicAbility);
            }
            if (magicComp.mimicAbility != null && magicComp.mimicAbility.MainVerb.verbClass == verbCast.verbProps.verbClass)
            {
                magicComp.RemovePawnAbility(magicComp.mimicAbility);
            }
        }
    }
}
