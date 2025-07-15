using RimWorld;
using Verse;

namespace TorannMagic
{
    public class HediffComp_Invisibility : HediffComp
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
            if (spawned && Pawn.Map != null)
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
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                if(Pawn.CurJob != null && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing is Pawn)
                {
                    severityAdjustment -= 20;
                }
                Effecter InvisEffect = TorannMagicDefOf.TM_InvisibilityEffecter.Spawn();
                InvisEffect.Trigger(new TargetInfo(Pawn.Position, Pawn.Map, false), new TargetInfo(Pawn.Position, Pawn.Map, false));
                InvisEffect.Cleanup();
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                severityAdjustment--;
            }
        }
    }
}
