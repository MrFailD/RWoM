using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    public class HediffComp_ShadowSlayerCloak : HediffComp
    {
        private bool initialized = false;

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

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % 21 == 0)
            {
                bool firingAtTarget = Pawn.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt.Thing != null;
                bool hasTargetedJob = Pawn.CurJob != null && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing is Pawn;
                if (firingAtTarget || hasTargetedJob)
                {
                    HediffComp_Disappears hdComp = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_ShadowSlayerCloakHD).TryGetComp<HediffComp_Disappears>();
                    if (hdComp != null)
                    {
                        hdComp.ticksToDisappear -= Rand.Range(40, 60);
                        if (hdComp.ticksToDisappear <= 0)
                        {
                            Effecter InvisEffect = TorannMagicDefOf.TM_InvisibilityEffecter.Spawn();
                            InvisEffect.Trigger(new TargetInfo(Pawn.Position, Pawn.Map, false), new TargetInfo(Pawn.Position, Pawn.Map, false));
                            InvisEffect.Cleanup();
                        }
                    }
                }                
            }
        }
    }
}
