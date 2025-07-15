using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMArcaneCapacitor : Building
    {
        private const float EffectRadius = 12f;
        private const float arcaneEnergyCur = 0;
        private static List<IntVec3> _portableCells = new List<IntVec3>();

        public bool CapacitorIsOn => this.TryGetComp<CompFlickable>().SwitchIsOn;

        public float ArcaneEnergyCur
        {
            get => this.TryGetComp<CompRefuelable>().FuelPercentOfMax;
            set => this.TryGetComp<CompRefuelable>().Refuel(value);
        }

        public float TargetArcaneEnergyPct =>
            this.TryGetComp<CompRefuelable>().TargetFuelLevel /
            this.TryGetComp<CompRefuelable>().Props.fuelCapacity;

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            CompAbilityUserMagic comp = myPawn.GetCompAbilityUserMagic();
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, false,
                    TraverseMode.ByPawn))
            {
                list.Add(new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default,
                    null, null, 0f, null, null));
            }
            else
            {
                if (comp.IsMagicUser && CapacitorIsOn)
                {
                    list.Add(new FloatMenuOption("TM_ChargeManaStorage".Translate(
                        Mathf.RoundToInt(arcaneEnergyCur * 100)
                    ), delegate
                    {
                        Job job = new Job(TorannMagicDefOf.ChargePortal, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
            }

            return list;
        }

        public static List<IntVec3> PortableCellsAround(IntVec3 pos, Map map)
        {
            _portableCells.Clear();
            if (!pos.InBoundsWithNullCheck(map))
            {
                return _portableCells;
            }

            Region region = pos.GetRegion(map, RegionType.Set_All);
            if (region == null)
            {
                return _portableCells;
            }

            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null,
                delegate(Region r)
                {
                    foreach (IntVec3 current in r.Cells)
                    {
                        if (current.InHorDistOf(pos, EffectRadius))
                        {
                            _portableCells.Add(current);
                        }
                    }

                    return false;
                }, 13, RegionType.Set_Passable);
            return _portableCells;
        }

        protected override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 120 != 0 || !CapacitorIsOn)
                return;

            IReadOnlyList<Pawn> allPawns = Map.mapPawns.AllPawnsSpawned;
            foreach (Pawn pawn in allPawns)
            {
                TryTransferManaToPawn(pawn);
            }
        }

        private void TryTransferManaToPawn(Pawn pawn)
        {
            if (pawn.DestroyedOrNull() || pawn.Dead || pawn.Downed || !pawn.RaceProps.Humanlike ||
                pawn.Faction == null || pawn.Faction != Faction)
                return;

            var comp = pawn.GetCompAbilityUserMagic();
            if (comp == null || comp.Mana == null)
                return;

            float rangeToTarget = (pawn.Position - Position).LengthHorizontal;
            if (pawn.drafter == null || !TM_Calc.IsMagicUser(pawn) || rangeToTarget > EffectRadius)
                return;

            float manaPct = comp.Mana.CurLevelPercentage;
            if (pawn.Drafted && manaPct <= .9f && ArcaneEnergyCur > .05f)
            {
                TransferMana(comp, rangeToTarget * .4f);
            }
            else if (!pawn.Drafted && (2.5f * manaPct < ArcaneEnergyCur))
            {
                TransferMana(comp, rangeToTarget * .4f);
            }
        }

        private void TransferMana(CompAbilityUserMagic comp, float magnitude)
        {
            ArcaneEnergyCur -= 7;
            comp.Mana.CurLevel += .05f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 moteDirection = TM_Calc.GetVector(Position, comp.Pawn.Position);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ArcaneStream, DrawPos, Map,
                    Rand.Range(.4f, .8f), 0.15f, .02f + (.08f * i), .3f - (.04f * i), Rand.Range(-10, 10),
                    magnitude + magnitude * i,
                    (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(),
                    Rand.Chance(.5f)
                        ? (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat()
                        : (Quaternion.AngleAxis(-90, Vector3.up) * moteDirection).ToAngleFlat());
            }

            for (int i = 0; i < 10; i++)
            {
                float direction = Rand.Range(0, 360);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi_Arcane, DrawPos, Map,
                    Rand.Range(.3f, .5f), 0.1f, .02f, .1f, 0, Rand.Range(5, 7), direction, direction);
            }

            TM_MoteMaker.ThrowManaPuff(comp.Pawn.DrawPos, comp.Pawn.Map, 1f);
        }
    }
}