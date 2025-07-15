using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_BrandingEmotion : HediffComp_BrandingBase
    {
        public override void DoSigilAction(bool surging = false, bool draining = false)
        {
            if(parent.Severity >= .1f)
            {
                float sev = surging ? 2 * parent.Severity : parent.Severity;

                if(Pawn.InMentalState && Rand.Chance(sev * .05f))
                {
                    Pawn.MentalState.RecoverFromState();
                }

                if(Pawn.needs != null && Pawn.needs.mood != null)
                {
                    Pawn.needs.mood.CurLevel += sev * .025f;
                }
            }
        }
    }
}