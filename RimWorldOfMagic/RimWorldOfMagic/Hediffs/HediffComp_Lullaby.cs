using RimWorld;
using Verse;
using System.Collections.Generic;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Lullaby : HediffComp
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

            if (Find.TickManager.TicksGame % 20 == 0 && Pawn.Map != null)
            {
                CellRect cellRect = CellRect.CenteredOn(Pawn.Position, 2);
                cellRect.ClipInsideMap(Pawn.Map);
                using (IEnumerator<IntVec3> enumerator = cellRect.Cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Pawn approachingPawn = enumerator.Current.GetFirstPawn(Pawn.Map);
                        if (approachingPawn != null && Pawn.Faction != null & approachingPawn.HostileTo(Pawn.Faction))
                        {
                            //wake up!
                            if (Pawn.CurJob.def.defName == "JobDriver_SleepNow")
                            {
                                Pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced, true);
                                Pawn.mindState.priorityWork.Clear();
                                Pawn.TryStartAttack(approachingPawn);
                                MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "Disturbed!", -1);
                            }
                        }
                    }
                }
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < .1f;
            }
        }


    }
}
