using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using AbilityUser;
using UnityEngine;
using Verse.AI.Group;

namespace TorannMagic
{
    public class Verb_BloodShield : Verb_UseAbility  
    {
        private int pwrVal;
        private CompAbilityUserMagic comp;
        private float arcaneDmg = 1;

        private bool validTarg;
        //Used for non-unique abilities that can be used with shieldbelt
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
            Pawn caster = base.CasterPawn;
            Pawn pawn = currentTarget.Thing as Pawn;

            comp = caster.GetCompAbilityUserMagic();
            MagicPowerSkill bpwr = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_pwr");
            MagicPowerSkill pwr = comp.MagicData.MagicPowerSkill_BloodShield.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodShield_pwr");
            pwrVal = pwr.level;
            
            if (caster.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                pwrVal = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr").level;
            }
            if (ModOptions.Settings.Instance.AIHardMode && !caster.IsColonist)
            {
                pwrVal = 3;
            }
            arcaneDmg = comp.arcaneDmg;
            arcaneDmg *= (1f + .1f * bpwr.level);

            if (pawn != null && pawn != CasterPawn)
            {
                ApplyBloodShield(pawn);
            }
            else
            {
                Messages.Message("TM_InvalidTarget".Translate(CasterPawn.LabelShort, TorannMagicDefOf.TM_BloodShield.label), MessageTypeDefOf.RejectInput);
            }
            
            return true;
        }

        public void ApplyBloodShield(Pawn pawn)
        {
            HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TM_BloodShieldHD"), (100f + (10f * pwrVal))*arcaneDmg);
            HediffComp_BloodShield comp = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_BloodShieldHD"), false).TryGetComp<HediffComp_BloodShield>();
            comp.linkedPawn = CasterPawn;
            SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map, false), MaintenanceType.None);
            info.pitchFactor = .75f;
            SoundDefOf.EnergyShield_Reset.PlayOneShot(info);
            Effecter bloodshieldEffecter = TorannMagicDefOf.TM_BloodShieldEffecter.Spawn();
            bloodshieldEffecter.Trigger(new TargetInfo(pawn.Position, pawn.Map, false), new TargetInfo(pawn.Position, pawn.Map, false));
            bloodshieldEffecter.Cleanup();
            
        }    
    }
}
