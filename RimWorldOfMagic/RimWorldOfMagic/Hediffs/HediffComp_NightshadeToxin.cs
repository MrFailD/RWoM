using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_NightshadeToxin : HediffComp
    {

        private bool removeNow;

        private int eventFrequency = 60;       

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null;
            if (flag)
            {
                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    if (!Pawn.Dead)
                    {
                        if (Pawn.RaceProps != null && !Pawn.RaceProps.IsMechanoid)
                        {
                            float sev = parent.Severity;
                            if (sev > 1f)
                            {
                                List<BodyPartRecord> insideParts = new List<BodyPartRecord>();
                                for (int i = 0; i < Pawn.RaceProps.body.AllParts.Count; i++)
                                {
                                    BodyPartRecord part = Pawn.RaceProps.body.AllParts[i];
                                    if (part.depth == BodyPartDepth.Inside)
                                    {
                                        insideParts.AddDistinct(part);
                                    }
                                }
                                if (insideParts.Count > 0 && parent.Severity >= 3)
                                {
                                    TM_Action.DamageEntities(Pawn, insideParts.RandomElement(), parent.Severity / 2f, TMDamageDefOf.DamageDefOf.TM_Toxin, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        removeNow = true;
                    }
                } 
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return removeNow || base.CompShouldRemove;
            }
        }        
    }
}
