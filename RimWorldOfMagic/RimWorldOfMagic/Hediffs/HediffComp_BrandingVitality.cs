using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_BrandingVitality : HediffComp_BrandingBase
    {
        public override int AverageUpdateTick => 600;

        public override void DoSigilAction(bool surging = false, bool draining = false)
        {
            if(parent.Severity >= .1f)
            {
                float healAmt = .8f * parent.Severity;
                int healCount = surging ? 2 : 1;

                TM_Action.DoAction_HealPawn(Pawn, Pawn, healCount, healAmt);

                float restAmt = parent.Severity * .01f * healCount;
                if(Pawn.needs != null && Pawn.needs.rest != null)
                {
                    Pawn.needs.rest.CurLevel += restAmt;
                }
            }
        }
    }
}
