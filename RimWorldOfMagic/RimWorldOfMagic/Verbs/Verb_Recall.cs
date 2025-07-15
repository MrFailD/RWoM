using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TorannMagic
{
    internal class Verb_Recall : Verb_UseAbility
    {
        private int pwrVal = 0;
        private CompAbilityUserMagic comp;
        private Map map;

        protected override bool TryCastShot()
        {
            bool result = false;
            map = CasterPawn.Map;
            comp = CasterPawn.GetCompAbilityUserMagic();

            if (CasterPawn != null && !CasterPawn.Downed && comp != null && comp.recallSet)
            {
                TM_Action.DoRecall(CasterPawn, comp, false);
            }
            else
            {
                Log.Warning("failed to TryCastShot");
            }

            burstShotsLeft = 0;
            return result;
        }

       
    }
}
