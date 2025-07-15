using RimWorld;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Torment : HediffComp
    {

        private bool initializing = true;

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
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
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
            }

            if (Find.TickManager.TicksGame % 60 == 0)
            {

                severityAdjustment--;
                Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_TormentHD, false);
                if (hediff.Severity < 1)
                {
                    Pawn.health.RemoveHediff(hediff);
                }
            }
        }

    }
}
