using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AbilityUser;
using RimWorld;
using TorannMagic.TMDefs;
using Verse.AI;
using LocalTargetInfo = Verse.LocalTargetInfo;
using Pawn = Verse.Pawn;
using Thing = Verse.Thing;
using WorkTags = Verse.WorkTags;

// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace TorannMagic
{
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    public partial class CompAbilityUserMagic
    {
        public void ResolveAutoCast()
        {
            if (!CanAutocastOnCurrentJob())
            {
                return;
            }

            var pawn = this.Pawn;
            var job = pawn.CurJob;
            bool isChaosMage = pawn.story.traits.HasTrait(TorannMagicDefOf.ChaosMage);
            bool isSummoner = pawn.story.traits.HasTrait(TorannMagicDefOf.Summoner);
            bool isChronomancer = pawn.story.traits.HasTrait(TorannMagicDefOf.Chronomancer);
            bool isArcanist = pawn.story.traits.HasTrait(TorannMagicDefOf.Arcanist);
            bool isCustomClass = this.customClass != null;

            if (pawn.drafter != null && !pawn.Drafted && this.Mana != null &&
                this.Mana.CurLevelPercentage >= ModOptions.Settings.Instance.autocastMinThreshold)
            {
                if (TryAutocastUndraftedMagic(pawn, job))
                    return;

                if ((isSummoner || isChaosMage || isCustomClass) && TryAutocastSummoner())
                    return;

                if ((isChronomancer || isCustomClass) && !this.recallSet && TryAutocastChronomancer(job))
                    return;

                if ((isArcanist || isChaosMage || isCustomClass) && TryAutocastArcanist(job, isChaosMage))
                    return;
            }
        }

        private bool CanAutocastOnCurrentJob()
        {
            var pawn = this.Pawn;
            var job = pawn?.CurJob;

            if (!ModOptions.Settings.Instance.autocastEnabled ||
                pawn?.jobs == null || job == null ||
                job.playerForced ||
                pawn.GetPosture() != PawnPosture.Standing)
            {
                return false;
            }

            bool isExcludedJob =
                job.def == TorannMagicDefOf.TMCastAbilityVerb ||
                job.def == TorannMagicDefOf.TMCastAbilitySelf ||
                job.def == JobDefOf.Ingest ||
                job.def == JobDefOf.ManTurret;
            if (isExcludedJob)
            {
                return false;
            }

            var gameConditionManager = pawn.Map.GameConditionManager;
            if (gameConditionManager.ConditionIsActive(TorannMagicDefOf.ManaDrain) ||
                gameConditionManager.ConditionIsActive(TorannMagicDefOf.TM_ManaStorm))
            {
                return false;
            }

            return true;
        }

        // Extracted helper to centralize target faction evaluation
        private bool IsValidTargetFactionType(TM_Autocast autocasting, Pawn pawn, Thing targetThing)
        {
            bool isEnemy = autocasting.targetEnemy && targetThing.Faction != null &&
                           targetThing.Faction.HostileTo(pawn.Faction);
            if (isEnemy && targetThing is Pawn enemyPawn &&
                (enemyPawn.Downed || enemyPawn.IsPrisonerInPrisonCell()))
                return false;
            bool isNeutral = autocasting.targetNeutral && targetThing.Faction != null &&
                             !targetThing.Faction.HostileTo(pawn.Faction);
            bool isNoFaction = autocasting.targetNoFaction && targetThing.Faction == null;
            bool isFriendly = autocasting.targetFriendly && targetThing.Faction == pawn.Faction;
            return isEnemy || isNeutral || isFriendly || isNoFaction;
        }

        private bool IsValidAutocastTarget(Pawn caster, MagicPower magicPower,
            PawnAbility ability, LocalTargetInfo localTarget, Func<Pawn, object, bool> extraCondition = null)
        {
            if (localTarget == null || !localTarget.IsValid)
                return false;

            var autocastDef = magicPower.autocasting;
            if (!autocastDef.ValidType(autocastDef.GetTargetType, localTarget))
                return false;

            // Check for line of sight, range, etc. Only done for targets with position.
            if ((localTarget.Thing != null || localTarget.Cell.IsValid))
            {
                var targetPos = localTarget.Thing?.Position ?? localTarget.Cell;
                float range = ability.Def.MainVerb.range;
                if (autocastDef.requiresLoS && !TM_Calc.HasLoSFromTo(caster.Position, targetPos, caster,
                        autocastDef.minRange, range))
                    return false;

                if (autocastDef.maxRange != 0f &&
                    autocastDef.maxRange < (caster.Position - targetPos).LengthHorizontal)
                    return false;
            }

            if (extraCondition != null &&
                !extraCondition(caster, localTarget.Thing ?? (object)localTarget.Cell))
                return false;

            if (!autocastDef.ValidConditions(caster, localTarget.Thing ?? (LocalTargetInfo)localTarget.Cell))
                return false;

            return true;
        }

        private void TryAutocastOnTarget(Pawn pawn, Job job, MagicPower mp, TMAbilityDef tmad,
            PawnAbility ability, out bool castSuccess)
        {
            castSuccess = false;
            if (job.targetA == null || job.targetA.Thing == null) return;

            LocalTargetInfo target = TM_Calc.GetAutocastTarget(pawn, mp.autocasting, job.targetA);
            if (!IsValidAutocastTarget(
                    pawn, mp, ability, target,
                    (p, t) => IsValidTargetFactionType(mp.autocasting, p, (Thing)t)))
                return;

            AutoCast.MagicAbility_OnTarget.TryExecute(this, tmad, ability, mp, target.Thing,
                mp.autocasting.minRange, out castSuccess);
        }

        private void TryAutocastOnSelf(Pawn pawn, Job job, MagicPower mp, TMAbilityDef tmad,
            PawnAbility ability, out bool castSuccess)
        {
            castSuccess = false;
            LocalTargetInfo target = TM_Calc.GetAutocastTarget(pawn, mp.autocasting, job.targetA);
            if (!IsValidAutocastTarget(pawn, mp, ability, target)) return;

            AutoCast.MagicAbility_OnSelf.Evaluate(this, tmad, ability, mp, out castSuccess);
        }

        private void TryAutocastOnCell(Pawn pawn, Job job, MagicPower mp, TMAbilityDef tmad,
            PawnAbility ability, out bool castSuccess)
        {
            castSuccess = false;
            if (job.targetA == null) return;

            LocalTargetInfo target = TM_Calc.GetAutocastTarget(pawn, mp.autocasting, job.targetA);
            if (!IsValidAutocastTarget(pawn, mp, ability, target)) return;

            AutoCast.MagicAbility_OnCell.TryExecute(this, tmad, ability, mp, target.Cell,
                mp.autocasting.minRange, out castSuccess);
        }

        private void TryAutocastOnNearby(Pawn pawn, Job job, MagicPower mp, TMAbilityDef tmad,
            PawnAbility ability, out bool castSuccess)
        {
            castSuccess = false;
            if (mp.autocasting.maxRange == 0f)
                mp.autocasting.maxRange = mp.abilityDef.MainVerb.range;

            LocalTargetInfo target = TM_Calc.GetAutocastTarget(pawn, mp.autocasting, job.targetA);
            if (!IsValidAutocastTarget(
                    pawn, mp, ability, target,
                    (p, t) => IsValidTargetFactionType(mp.autocasting, p, (Thing)t)))
                return;

            AutoCast.MagicAbility_OnTarget.TryExecute(this, tmad, ability, mp, target.Thing,
                mp.autocasting.minRange, out castSuccess);
        }

        private bool TryAutocastUndraftedMagic(Pawn pawn, Job job)
        {
            foreach (MagicPower magicPower in this.MagicData.MagicPowersCustomAll)
            {
                if (!magicPower.learned || !magicPower.autocast || magicPower.autocasting == null ||
                    !magicPower.autocasting.magicUser || !magicPower.autocasting.undrafted)
                {
                    continue;
                }

                TMAbilityDef abilityDef = magicPower.TMabilityDefs[magicPower.level] as TMAbilityDef;
                if (abilityDef == null)
                    continue;

                bool canUseIfViolentAbility =
                    !pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) ||
                    !abilityDef.MainVerb.isViolent;

                if (!TM_Calc.HasResourcesForAbility(this.Pawn, abilityDef))
                    continue;

                if (!canUseIfViolentAbility)
                    continue;

                PawnAbility ability =
                    this.AbilityData.Powers.FirstOrDefault(x => x.Def == abilityDef);
                bool castSuccess = false;

                switch (magicPower.autocasting.type)
                {
                    case AutocastType.OnTarget:
                        TryAutocastOnTarget(pawn, job, magicPower, abilityDef, ability, out castSuccess);
                        break;
                    case AutocastType.OnSelf:
                        TryAutocastOnSelf(pawn, job, magicPower, abilityDef, ability, out castSuccess);
                        break;
                    case AutocastType.OnCell:
                        TryAutocastOnCell(pawn, job, magicPower, abilityDef, ability, out castSuccess);
                        break;
                    case AutocastType.OnNearby:
                        TryAutocastOnNearby(pawn, job, magicPower, abilityDef, ability, out castSuccess);
                        break;
                }

                if (castSuccess)
                    return true;
            }

            return false;
        }

        private bool TryAutocastSummoner()
        {
            MagicPower magicPower =
                this.MagicData.MagicPowersS.FirstOrDefault(x =>
                    x.abilityDef == TorannMagicDefOf.TM_SummonMinion);

            if (magicPower == null || !magicPower.learned || !magicPower.autocast ||
                this.summonedMinions.Count() >= 4)
            {
                return false;
            }

            PawnAbility ability =
                this.AbilityData.Powers.FirstOrDefault(x => x.Def == TorannMagicDefOf.TM_SummonMinion);

            AutoCast.MagicAbility_OnSelfPosition.Evaluate(
                this, TorannMagicDefOf.TM_SummonMinion, ability, magicPower, out bool castSuccess);
            return castSuccess;
        }

        private bool TryAutocastChronomancer(Job job)
        {
            if (this.AbilityData.Powers.All(p => p.Def != TorannMagicDefOf.TM_TimeMark))
            {
                return false;
            }

            MagicPower magicPower =
                this.MagicData.MagicPowersStandalone.FirstOrDefault(x =>
                    x.abilityDef == TorannMagicDefOf.TM_TimeMark);

            if (magicPower != null && (magicPower.learned || spell_Recall) && magicPower.autocast &&
                !job.playerForced)
            {
                PawnAbility ability =
                    this.AbilityData.Powers.FirstOrDefault(x => x.Def == TorannMagicDefOf.TM_TimeMark);

                AutoCast.MagicAbility_OnSelfPosition.Evaluate(
                    this, TorannMagicDefOf.TM_TimeMark, ability, magicPower, out bool castSuccess);

                return castSuccess;
            }

            return false;
        }

        private bool TryAutocastArcanist(Job job, bool isChaosMage)
        {
            foreach (var tmAbilityDef in this.MagicData.MagicPowersA
                         .Where(magicPower => magicPower?.abilityDef != null).SelectMany(magicPower =>
                             magicPower.TMabilityDefs.Cast<TMAbilityDef>()
                                 .Where(tmAbilityDef => tmAbilityDef != null)))
            {
                if ((tmAbilityDef == TorannMagicDefOf.TM_Summon ||
                     tmAbilityDef == TorannMagicDefOf.TM_Summon_I ||
                     tmAbilityDef == TorannMagicDefOf.TM_Summon_II ||
                     tmAbilityDef == TorannMagicDefOf.TM_Summon_III) && !job.playerForced)
                {
                    MagicPower mpSummon =
                        this.MagicData.MagicPowersA.FirstOrDefault(x => x.abilityDef == tmAbilityDef);

                    if (mpSummon != null && mpSummon.learned && mpSummon.autocast)
                    {
                        PawnAbility ability =
                            this.AbilityData.Powers.FirstOrDefault(x => x.Def == tmAbilityDef);

                        float minDistance = ActualManaCost(tmAbilityDef) * 150;
                        AutoCast.Summon.Evaluate(
                            this, tmAbilityDef, ability, mpSummon, minDistance, out bool castSuccessSummon);

                        if (castSuccessSummon)
                            return true;
                    }
                }

                if (tmAbilityDef != TorannMagicDefOf.TM_Blink &&
                    tmAbilityDef != TorannMagicDefOf.TM_Blink_I &&
                    tmAbilityDef != TorannMagicDefOf.TM_Blink_II &&
                    tmAbilityDef != TorannMagicDefOf.TM_Blink_III)
                {
                    continue;
                }

                MagicPower mp =
                    this.MagicData.MagicPowersA.FirstOrDefault(x => x.abilityDef == tmAbilityDef);

                if (mp != null && mp.learned && mp.autocast)
                {
                    PawnAbility ability =
                        this.AbilityData.Powers.FirstOrDefault(x => x.Def == tmAbilityDef);

                    float minDistance = ActualManaCost(tmAbilityDef) * 240;
                    AutoCast.Blink.Evaluate(
                        this, tmAbilityDef, ability, mp, minDistance, out bool castSuccessLearned);

                    if (castSuccessLearned)
                        return true;
                }

                if (!isChaosMage || mp == null || !this.spell_Blink || mp.learned || !mp.autocast)
                {
                    continue;
                }

                PawnAbility abilityPower = this.AbilityData.Powers.FirstOrDefault(x => x.Def == tmAbilityDef);

                float minDistanceAbilityPower = ActualManaCost(tmAbilityDef) * 200;
                AutoCast.Blink.Evaluate(
                    this, tmAbilityDef, abilityPower, mp, minDistanceAbilityPower, out bool castSuccess);

                if (castSuccess)
                    return true;
            }

            return false;
        }
    }
}