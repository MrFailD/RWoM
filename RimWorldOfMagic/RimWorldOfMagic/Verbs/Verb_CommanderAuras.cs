using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using UnityEngine;


namespace TorannMagic
{
    public class Verb_CommanderAuras : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            bool removedAura = RemoveAura();
            if(!removedAura)
            {
                ApplyAura();
            }
            ToggleAbilityAutocast();
            return true;
        }

        private bool RemoveAura()
        {
            bool auraRemoved = false;
            Hediff hediff = null;
            for (int h = 0; h < CasterPawn.health.hediffSet.hediffs.Count; h++)
            {
                if (Ability.Def == TorannMagicDefOf.TM_ProvisionerAura && CasterPawn.health.hediffSet.hediffs[h].def == TorannMagicDefOf.TM_ProvisionerAuraHD)
                {
                    hediff = CasterPawn.health.hediffSet.hediffs[h];
                    CasterPawn.health.RemoveHediff(hediff);
                    auraRemoved = true;
                    break;
                }
                if (Ability.Def == TorannMagicDefOf.TM_TaskMasterAura && CasterPawn.health.hediffSet.hediffs[h].def == TorannMagicDefOf.TM_TaskMasterAuraHD)
                {
                    hediff = CasterPawn.health.hediffSet.hediffs[h];
                    CasterPawn.health.RemoveHediff(hediff);
                    auraRemoved = true;
                    break;
                }
                if (Ability.Def == TorannMagicDefOf.TM_CommanderAura && CasterPawn.health.hediffSet.hediffs[h].def == TorannMagicDefOf.TM_CommanderAuraHD)
                {
                    hediff = CasterPawn.health.hediffSet.hediffs[h];
                    CasterPawn.health.RemoveHediff(hediff);
                    auraRemoved = true;
                    break;
                }
            }
            return auraRemoved;
        }

        private void ApplyAura()
        {
            if (Ability.Def == TorannMagicDefOf.TM_ProvisionerAura)
            {
                HealthUtility.AdjustSeverity(CasterPawn, TorannMagicDefOf.TM_ProvisionerAuraHD, .5f);
            }
            else if(Ability.Def == TorannMagicDefOf.TM_TaskMasterAura)
            {
                HealthUtility.AdjustSeverity(CasterPawn, TorannMagicDefOf.TM_TaskMasterAuraHD, .5f);
            }
            else if (Ability.Def == TorannMagicDefOf.TM_CommanderAura)
            {
                HealthUtility.AdjustSeverity(CasterPawn, TorannMagicDefOf.TM_CommanderAuraHD, .5f);
            }            
        }

        private void ToggleAbilityAutocast()
        {
            MightPower mightPower = null;
            if (Ability.Def == TorannMagicDefOf.TM_ProvisionerAura)
            {
               mightPower = CasterPawn.GetCompAbilityUserMight().MightData.MightPowersC.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ProvisionerAura);
            }
            else if (Ability.Def == TorannMagicDefOf.TM_TaskMasterAura)
            {
                mightPower = CasterPawn.GetCompAbilityUserMight().MightData.MightPowersC.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TaskMasterAura);
            }
            else if (Ability.Def == TorannMagicDefOf.TM_CommanderAura)
            {
                mightPower = CasterPawn.GetCompAbilityUserMight().MightData.MightPowersC.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_CommanderAura);
            }            

            if (mightPower != null)
            {
                mightPower.autocast = !mightPower.autocast;
            }
        }
    }
}
