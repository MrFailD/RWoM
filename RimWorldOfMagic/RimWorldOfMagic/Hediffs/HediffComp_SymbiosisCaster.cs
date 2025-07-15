using RimWorld;
using Verse;

namespace TorannMagic
{
    public class HediffComp_SymbiosisCaster : HediffComp
    {
        private bool initializing = true;
        private bool shouldRemove;
        public Pawn symbiosisHost;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref symbiosisHost, "symbiosisHost");
        }

        public override string CompLabelInBracketsExtra => symbiosisHost != null ? symbiosisHost.LabelShort + "[-]" + base.CompLabelInBracketsExtra : base.CompLabelInBracketsExtra;

        public string labelCap
        {
            get
            {
                if (symbiosisHost != null)
                {
                    return Def.LabelCap + "(" + symbiosisHost.LabelShort + ")";
                }
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                if (symbiosisHost != null)
                {
                    return Def.label + "(" + symbiosisHost.LabelShort + ")";
                }
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned && Pawn.Map != null)
            {
                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 2f);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
                if(Find.TickManager.TicksGame % 200 == 0)
                {
                    if(symbiosisHost.DestroyedOrNull())
                    {
                        shouldRemove = true;
                    }
                    else if(symbiosisHost.Dead)
                    {
                        DamageInfo dinfo = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_SymbiosisDD, 1f);
                        Pawn.Kill(dinfo, null);
                        shouldRemove = true;
                    }

                    if (Pawn.needs != null && Pawn.needs.mood != null)
                    {
                        if (Pawn.needs.mood.CurLevel < .01f)
                        {
                            shouldRemove = true;
                            RemoveHostHediff();
                        }
                        Pawn.needs.mood.CurLevel -= .002f;
                    }
                    else
                    {
                        RemoveHostHediff();
                        shouldRemove = true;
                    }
                }                
            }
        }

        public void RemoveHostHediff()
        {
            if(symbiosisHost != null && symbiosisHost.health != null && symbiosisHost.health.hediffSet != null)
            {
                Hediff hd = symbiosisHost.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_SymbiosisHD);
                if(hd != null)
                {
                    symbiosisHost.health.RemoveHediff(hd);
                }
            }
        }

        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;
    }
}
