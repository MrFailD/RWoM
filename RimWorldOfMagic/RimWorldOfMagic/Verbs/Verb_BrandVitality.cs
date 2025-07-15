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
    public class Verb_BrandVitality : Verb_UseAbility
    {

        private int verVal = 0;
        private int pwrVal = 0;
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
            Pawn caster = CasterPawn;
            
            if(caster != null && CurrentTarget.HasThing && CurrentTarget.Thing is Pawn)
            {
                Pawn hitPawn = currentTarget.Thing as Pawn;
                CompAbilityUserMagic casterComp = caster.GetCompAbilityUserMagic();

                if (casterComp != null && hitPawn.health != null && hitPawn.health.hediffSet != null && hitPawn != caster)
                {
                    //RemoveOldBrand(hitPawn);

                    //HealthUtility.AdjustSeverity(hitPawn, TorannMagicDefOf.TM_VitalityBrandHD, .05f);
                    //if (casterComp.BrandedPawns != null)
                    //{
                    //    casterComp.BrandedPawns.Add(hitPawn);
                    //}
                    //Hediff newBrand = hitPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_VitalityBrandHD);
                    //if (newBrand != null && newBrand.TryGetComp<HediffComp_BrandingVitality>() != null)
                    //{
                    //    newBrand.TryGetComp<HediffComp_BrandingVitality>().BranderPawn = caster;
                    //}

                    TM_Action.UpdateBrand(hitPawn, caster, casterComp, TorannMagicDefOf.TM_VitalityBrandHD);

                    UpdateHediffComp(hitPawn);
                    DoBrandingEffect(hitPawn);
                }
                else
                {
                    Messages.Message("TM_InvalidTarget".Translate(CasterPawn.LabelShort, Ability.Def.label), MessageTypeDefOf.RejectInput);
                }
            }
            else
            {
                Messages.Message("TM_InvalidTarget".Translate(CasterPawn.LabelShort, Ability.Def.label), MessageTypeDefOf.RejectInput);
            }

            PostCastShot(flag, out flag);
            return flag;
        }

        private void UpdateHediffComp(Pawn hitPawn)
        {
            Hediff hd = hitPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_VitalityBrandHD);
            if (hd != null)
            {
                HediffComp_BrandingBase hdc = hd.TryGetComp<HediffComp_BrandingBase>();
                if (hdc != null)
                {
                    hdc.BranderPawn = CasterPawn;
                }
            }
        }

        private void DoBrandingEffect(Pawn hitPawn)
        {
            if (hitPawn != null && hitPawn.Map != null)
            {
                TargetInfo ti = new TargetInfo(hitPawn.Position, hitPawn.Map, false);
                TM_MoteMaker.MakeOverlay(ti, TorannMagicDefOf.TM_Mote_PsycastAreaEffect, hitPawn.Map, Vector3.zero, 1f, 0f, .1f, .4f, 1.2f, -3f);
            }
            //Effecter effect = EffecterDefOf.Skip_EntryNoDelay.Spawn();
            //effect.Trigger(new TargetInfo(this.CasterPawn), new TargetInfo(hitPawn));
            //effect.Cleanup();

            //Effecter effectExit = EffecterDefOf.Skip_ExitNoDelay.Spawn();
            //effectExit.Trigger(new TargetInfo(hitPawn), new TargetInfo(hitPawn));
            //effectExit.Cleanup();
        }

        //private void RemoveOldBrand(Pawn hitPawn)
        //{
        //    Hediff oldBrand = hitPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_VitalityBrandHD);
        //    if (oldBrand != null)
        //    {
        //        HediffComp_BrandingVitality hd_br = oldBrand.TryGetComp<HediffComp_BrandingVitality>();
        //        if (hd_br != null && hd_br.BranderPawn != null && !hd_br.BranderPawn.DestroyedOrNull() && !hd_br.BranderPawn.Dead)
        //        {
        //            CompAbilityUserMagic branderComp = hd_br.BranderPawn.GetCompAbilityUserMagic();
        //            if (branderComp != null && branderComp.BrandedPawns != null && branderComp.BrandedPawns.Contains(hitPawn))
        //            {
        //                branderComp.BrandedPawns.Remove(hitPawn);
        //            }
        //        }
        //        hitPawn.health.RemoveHediff(oldBrand);
        //    }
        //}
    }
}
