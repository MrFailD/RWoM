using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TM_DMP : Building
    {
        private const float EffectRadius = 36f;
        private const int RotationRate = 4;
        private const float arcaneEnergyCur = 0;
        private int matRot;
        private float matMagnitude = .5f;
        private float matMagnitudeValue = .01f;
        private static List<IntVec3> portableCells = new List<IntVec3>();

        private static readonly Material dmpMat_0 = MaterialPool.MatFrom("Other/dmp0", false);
        private static readonly Material dmpMat_1 = MaterialPool.MatFrom("Other/dmp1", false);
        private static readonly Material dmpMat_2 = MaterialPool.MatFrom("Other/dmp2", false);
        private static readonly Material dmpMat_3 = MaterialPool.MatFrom("Other/dmp3", false);
        private static readonly Material dmpMat_4 = MaterialPool.MatFrom("Other/dmp4", false);
        private static readonly Material dmpMat_5 = MaterialPool.MatFrom("Other/dmp5", false);
        private static readonly Material dmpMat_6 = MaterialPool.MatFrom("Other/dmp6", false);
        private static readonly Material dmpMat_7 = MaterialPool.MatFrom("Other/dmp7", false);

        public IEnumerable<IntVec3> PortableCells => PortableCellsAround(base.InteractionCell, Map);

        public bool IsOn => this.TryGetComp<CompFlickable>().SwitchIsOn;

        public float ArcaneEnergyCur
        {
            get => this.TryGetComp<CompRefuelable>().FuelPercentOfMax;
            set => this.TryGetComp<CompRefuelable>().Refuel(value);
        }

        public float TargetArcaneEnergyPct => this.TryGetComp<CompRefuelable>().TargetFuelLevel /
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
                if (comp.IsMagicUser && IsOn)
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
            portableCells.Clear();
            if (!pos.InBoundsWithNullCheck(map))
            {
                return portableCells;
            }

            Region region = pos.GetRegion(map, RegionType.Set_All);
            if (region == null)
            {
                return portableCells;
            }

            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null,
                delegate(Region r)
                {
                    foreach (IntVec3 current in r.Cells)
                    {
                        if (current.InHorDistOf(pos, EffectRadius))
                        {
                            portableCells.Add(current);
                        }
                    }

                    return false;
                }, 54, RegionType.Set_Passable);
            return portableCells;
        }

        protected override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % RotationRate == 0)
            {
                matRot++;
                if (matRot >= 8)
                {
                    matRot = 0;
                }

                matMagnitude += matMagnitudeValue;
                if (matMagnitude >= .5f)
                {
                    matMagnitudeValue = -.005f;
                }

                if (matMagnitude <= .2f)
                {
                    matMagnitudeValue = .005f;
                }
            }

            if (Find.TickManager.TicksGame % 240 == 0 && IsOn)
            {
                List<Pawn> mapPawns = Map.mapPawns.AllPawnsSpawned.ToList();
                Pawn pawn = null;
                for (int i = 0; i < mapPawns.Count; i++)
                {
                    pawn = mapPawns[i];
                    if (!pawn.DestroyedOrNull() && pawn.Spawned && !pawn.Dead && !pawn.Downed &&
                        pawn.RaceProps != null && !pawn.AnimalOrWildMan() && pawn.RaceProps.Humanlike &&
                        pawn.Faction != null && pawn.Faction == Faction)
                    {
                        CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                        float rangeToTarget = (pawn.Position - Position).LengthHorizontal;
                        if (pawn.drafter != null && TM_Calc.IsMagicUser(pawn) &&
                            rangeToTarget <= EffectRadius && comp != null && comp.Mana != null)
                        {
                            if (pawn.Drafted && comp.Mana.CurLevelPercentage <= .9f &&
                                ArcaneEnergyCur >= .01f)
                            {
                                TransferMana(comp);
                                break;
                            }

                            if (!pawn.Drafted && comp.Mana.CurInstantLevelPercentage < .4f &&
                                ArcaneEnergyCur >= .01f)
                            {
                                TransferMana(comp);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void TransferMana(CompAbilityUserMagic comp)
        {
            ArcaneEnergyCur -= 20;
            comp.Mana.CurLevel += .16f;
            for (int i = 0; i < 4; i++)
            {
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Casting, DrawPos, Map, .4f + .5f * i,
                    0.2f, .02f + (.15f * i), .4f - (.06f * i), Rand.Range(-300, 300), 0, 0,
                    Rand.Range(0, 360));
            }

            for (int i = 0; i < 4; i++)
            {
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Casting, comp.Pawn.DrawPos, Map,
                    1.5f - (.4f * i), 0.2f, .02f + (.15f * i), .4f + (.06f * i), Rand.Range(-300, 300), 0, 0,
                    Rand.Range(0, 360));
            }

            TM_MoteMaker.ThrowManaPuff(comp.Pawn.DrawPos, comp.Pawn.Map, 1f);
        }


        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 vector = base.DrawPos;
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 s = new Vector3(matMagnitude, matMagnitude, matMagnitude);
            Matrix4x4 matrix = default(Matrix4x4);
            float angle = 0;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            switch (matRot)
            {
                case 0:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_0, 0);
                    break;
                case 1:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_1, 0);
                    break;
                case 2:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_2, 0);
                    break;
                case 3:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_3, 0);
                    break;
                case 4:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_4, 0);
                    break;
                case 5:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_5, 0);
                    break;
                case 6:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_6, 0);
                    break;
                default:
                    Graphics.DrawMesh(MeshPool.plane10, matrix, dmpMat_7, 0);
                    break;
            }
        }
    }
}