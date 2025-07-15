using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic.Ideology
{
    public class HediffComp_BestowMagicClass : HediffComp
    {
        public bool selectableForInspiration = true;
        public bool delayedInspiration;
        public bool botchedRitual;
        private int ticksTillInspiration = 10;

        //unsaved
        private bool shouldRemove;

        public override void CompExposeData()
        {
            Scribe_Values.Look<bool>(ref selectableForInspiration, "selectableForInspiration", true);
            Scribe_Values.Look<bool>(ref delayedInspiration, "delayedInspiration", false);
            Scribe_Values.Look<bool>(ref botchedRitual, "botchedRitual", false);
            Scribe_Values.Look<int>(ref ticksTillInspiration, "ticksTillInspiration", 10);
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
                if(delayedInspiration)
                {
                    ticksTillInspiration--;
                    if(ticksTillInspiration <= 0)
                    {
                        delayedInspiration = false;
                        Pawn.mindState.inspirationHandler.TryStartInspiration(TorannMagicDefOf.ID_ArcanePathways);
                    }
                }
                if(botchedRitual)
                {
                    CompUseEffect_LearnMagic.FixTrait(Pawn, Pawn.story.traits.allTraits);
                    shouldRemove = true;
                }
            }
        }
    }
}
