using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;
using System;
using UnityEngine;


namespace TorannMagic
{
    internal class JobDriver_Entertain : JobDriver
    {
        private const TargetIndex entertaineeTI = TargetIndex.A;
        private CompAbilityUserMagic comp;

        private int age = -1;
        private int duration = 120;

        protected Pawn entertaineePawn
        {
            get
            {
                return (Pawn)job.targetA.Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            comp = pawn.GetCompAbilityUserMagic();
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnMentalState(TargetIndex.A);
            Toil entertain = new Toil()
            {
                initAction = () =>
                {
                    if (!entertaineePawn.Spawned && !entertaineePawn.Awake())
                    {
                        return;
                    }
                },
                tickAction = () =>
                {
                    if (age > duration)
                    {
                        pawn.interactions.TryInteractWith(entertaineePawn, TorannMagicDefOf.TM_EntertainID);
                        FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
                        EndJobWith(JobCondition.Succeeded);
                        comp.nextEntertainTick = Find.TickManager.TicksGame + 2000;
                        age = 0;
                    }
                    age++;
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };            
            yield return entertain;
        }
    }
}