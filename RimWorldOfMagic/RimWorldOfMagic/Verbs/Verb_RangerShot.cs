using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using AbilityUser;


namespace TorannMagic
{
    public class Verb_RangerShot : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            if ( CasterPawn.equipment.Primary !=null && CasterPawn.equipment.Primary.def.IsRangedWeapon)
            {
                Thing wpn = CasterPawn.equipment.Primary;
                if (TM_Calc.HasLoSFromTo(CasterPawn.Position, currentTarget.Cell, CasterPawn, 0, Ability.Def.MainVerb.range))
                {
                    if (TM_Calc.IsUsingBow(CasterPawn))
                    {
                        base.TryCastShot();
                        return true;
                    }
                    else
                    {
                        if (CasterPawn.IsColonist)
                        {
                            Messages.Message("MustHaveBow".Translate(
                            CasterPawn.LabelShort,
                            wpn.LabelShort
                            ), MessageTypeDefOf.NegativeEvent);
                        }
                        return false;
                    }                    
                }
            }
            else
            {
                Messages.Message("MustHaveRangedWeapon".Translate(
                    CasterPawn.LabelCap
                ), MessageTypeDefOf.RejectInput);
                return false;
            }
            return false;
        }
    }
}
