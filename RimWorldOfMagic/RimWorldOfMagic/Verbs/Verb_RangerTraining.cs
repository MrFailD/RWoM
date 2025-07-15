using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TorannMagic
{
    public class Verb_RangerTraining : Verb_UseAbility
    {

        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            MightPowerSkill pwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_RangerTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_RangerTraining_pwr");

            List<Trait> traits = CasterPawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].def.defName == "Ranger")
                {
                    
                    if ( traits[i].Degree < pwr.level)
                    {
                        traits.Remove(traits[i]);
                        CasterPawn.story.traits.GainTrait(new Trait(TraitDef.Named("Ranger"), pwr.level, false));
                        FleckMaker.ThrowHeatGlow(base.CasterPawn.Position, map, 2);
                    }
                }
            }
            
            burstShotsLeft = 0;
            return false;
        }
    }
}
