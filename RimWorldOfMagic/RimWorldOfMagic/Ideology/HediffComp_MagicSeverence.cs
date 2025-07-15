using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic.Ideology
{
    public class HediffComp_MagicSeverence : HediffComp
    {
        public bool selectableForRetaliation = true;
        public bool delayedMindburn;
        private int ticksTillDeath = 10;

        //unsaved
        private bool shouldRemove;

        public override void CompExposeData()
        {
            Scribe_Values.Look<bool>(ref selectableForRetaliation, "selectableForRetaliation", true);
            Scribe_Values.Look<bool>(ref delayedMindburn, "delayedMindburn", false);
            Scribe_Values.Look<int>(ref ticksTillDeath, "ticksTillDeath", 10);
            base.CompExposeData();
        }
        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }


        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                
            }
        }

        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;

        public override void CompPostTick(ref float severityAdjustment)
        {

            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn.DestroyedOrNull();
            if (!flag)
            {
                if (Pawn.Spawned && Pawn.needs != null)
                {
                    if(TM_Calc.IsMagicUser(Pawn))
                    {
                        shouldRemove = true;
                    }
                    if (Pawn.Dead || Pawn.Downed)
                    {
                        shouldRemove = true;
                    }
                }
                if(delayedMindburn)
                {
                    ticksTillDeath--;
                    if(ticksTillDeath <= 0)
                    {
                        TM_Action.KillPawnByMindBurn(Pawn);
                    }
                }
            }
        }
    }
}
