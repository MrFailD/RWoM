using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace TorannMagic.Thoughts
{
    public class TM_Inspiration : Inspiration
    {
        public override void PostStart(bool sendLetter = true)
        {
            base.PostStart(sendLetter);
            if(def == TorannMagicDefOf.ID_Enlightened && pawn != null && pawn.health != null && pawn.health.hediffSet != null)
            {
                HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_EnlightenedHD, 1f);
            }
        }

        public override void PostEnd()
        {
            base.PostEnd();
            if (def == TorannMagicDefOf.ID_Enlightened && pawn != null && pawn.health != null && pawn.health.hediffSet != null)
            {
                Hediff hd = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_EnlightenedHD, false);
                if (hd != null)
                {
                    pawn.health.RemoveHediff(hd);
                }
            }
        }
    }
}
