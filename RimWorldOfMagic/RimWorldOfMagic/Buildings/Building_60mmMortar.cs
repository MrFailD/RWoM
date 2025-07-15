using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
// ReSharper disable RedundantArgumentDefaultValue

namespace TorannMagic.Buildings
{   
    [StaticConstructorOnStartup]
    public class Building_60mmMortar : Building
    {
        private int mortarMaxRange = 80;
        private const int MortarMinRange = 20;
        private int mortarTicksToFire = 60;
        private int mortarCount = 3;
        private float mortarAccuracy = 4f;
        private readonly ThingDef projectileDef = TorannMagicDefOf.FlyingObject_60mmMortar;

        private int verVal;
        private int pwrVal;
        private int effVal;

        private LocalTargetInfo setTarget = null;
        private readonly TargetingParameters targetingParameters = new TargetingParameters();

        private CompMannable mannableComp;

        private bool MannedByColonist => mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;
        private bool MannedByNonColonist => mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;
        private bool Manned => MannedByColonist || MannedByNonColonist;
        private bool initialized;

        private CompAbilityUserMight comp;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref verVal, "verVal", 0, false);
            Scribe_Values.Look(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look(ref effVal, "effVal", 0, false);
            Scribe_Values.Look(ref mortarMaxRange, "mortarMaxRange", 80, false);
            Scribe_Values.Look(ref mortarTicksToFire, "mortarTicksToFire", 50, false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            mannableComp = GetComp<CompMannable>();
        }

        protected override void Tick()
        {
            if (Manned)
            {
                if (!initialized)
                {
                    InitializeMortar();
                }

                var manningPawn = mannableComp.ManningPawn;
                if (!manningPawn.DestroyedOrNull() && !manningPawn.Dead && !manningPawn.Downed)
                {
                    TryFireMortar();
                }

                if (mortarCount <= 0)
                {
                    CleanUpAndDestroy();
                }
            }
            else
            {
                Destroy(DestroyMode.Vanish);
            }

        }

        private void InitializeMortar()
        {
            comp = mannableComp.ManningPawn.GetCompAbilityUserMight();
            verVal = GetMightSkillLevel("TM_60mmMortar_ver");
            pwrVal = GetMightSkillLevel("TM_60mmMortar_pwr");
            effVal = GetMightSkillLevel("TM_60mmMortar_eff");

            mortarTicksToFire = Find.TickManager.TicksGame + 300;
            mortarMaxRange += verVal * 10;
            if (verVal >= 3)
            {
                mortarCount++;
            }
            mortarAccuracy -= 0.7f * effVal;
            setTarget = null;
            SetTargetingParameters();
            initialized = true;
        }

        private int GetMightSkillLevel(string label)
        {
            return comp.MightData.MightPowerSkill_60mmMortar
                .FirstOrDefault(x => x.label == label)?.level ?? 0;
        }
        
        private void SetTargetingParameters()
        {
            targetingParameters.canTargetBuildings = true;
            targetingParameters.canTargetPawns = true;
            targetingParameters.canTargetLocations = true;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(Position, MortarMinRange, Color.red);
            GenDraw.DrawFieldEdges(PortableCellsAround(Position, Map, mortarMaxRange));
        }

        private void TryFireMortar()
        {
            if (mortarTicksToFire < Find.TickManager.TicksGame && mortarCount > 0)
            {
                mortarTicksToFire = Find.TickManager.TicksGame + (60 - (6 * verVal));
                LocalTargetInfo target = setTarget != null ? setTarget : TM_Calc.FindNearbyEnemy(Position, Map, Faction, mortarMaxRange, MortarMinRange);
                if (target != null && target.Cell.IsValid && target.Cell.DistanceToEdge(Map) > 5)
                {
                    bool isTargetValid = target.Cell != default(IntVec3);
                    if (isTargetValid)
                    {
                        LaunchMortarAtTarget(target);
                        PlayMortarLaunchSound();
                        mortarCount--;
                    }
                }
            }
        }
        private void LaunchMortarAtTarget(LocalTargetInfo target)
        {
            IntVec3 rndTarget = target.Cell;
            rndTarget.x += Mathf.RoundToInt(Rand.Range(-mortarAccuracy, mortarAccuracy));
            rndTarget.z += Mathf.RoundToInt(Rand.Range(-mortarAccuracy, mortarAccuracy));
            Thing launchedThing = new Thing()
            {
                def = projectileDef
            };
            int arc = target.Cell.x >= Position.x ? -1 : 1;
            var flyingObject = (FlyingObject_Advanced)GenSpawn.Spawn(projectileDef, Position, Map);
            flyingObject.AdvancedLaunch(
                this, null, 0,
                Mathf.Clamp(Rand.Range(50, 60), 0, Position.DistanceToEdge(Map)),
                false, DrawPos, rndTarget, launchedThing,
                Rand.Range(35, 40), true,
                Rand.Range(14 + pwrVal, 20 + (2 * pwrVal)),
                (3f + (0.35f * pwrVal)),
                DamageDefOf.Bomb, null, arc, true
            );
        }

        private void PlayMortarLaunchSound()
        {
            SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
            info.pitchFactor = 1.6f;
            info.volumeFactor = .7f;
            SoundDef.Named("Mortar_LaunchA").PlayOneShot(info);
        }

        private void CleanUpAndDestroy()
        {
            mannableComp.ManningPawn.jobs.EndCurrentJob(JobCondition.Succeeded);
            Destroy(DestroyMode.Vanish);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (true)
            {
                TM_Command_Target command_Target = new TM_Command_Target
                {
                    defaultLabel = "CommandSetForceAttackTarget".Translate(),
                    defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
                    targetingParams = targetingParameters,
                    hotKey = KeyBindingDefOf.Misc4,
                    icon = TexCommand.Attack,
                    action = delegate (LocalTargetInfo target)
                    {
                        float distance = (Position - target.Cell).LengthHorizontal;
                        if (distance < MortarMinRange)
                        {
                            Messages.Message("TooClose".Translate(), MessageTypeDefOf.RejectInput);
                        }
                        else if (distance > mortarMaxRange)
                        {
                            Messages.Message("OutOfRange".Translate(), MessageTypeDefOf.RejectInput);                        
                        }
                        else
                        {
                            setTarget = target;
                        }
                    }
                };
                yield return command_Target;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 rndPos = DrawPos;
                rndPos.x += Rand.Range(-.5f, .5f);
                rndPos.z += Rand.Range(-.5f, .5f);
                TM_MoteMaker.ThrowGenericFleck(FleckDefOf.ExplosionFlash, rndPos, Map, Rand.Range(.6f, .8f), .1f, .05f, .05f, 0, 0, 0, Rand.Range(0, 360));
                FleckMaker.ThrowSmoke(rndPos, Map, Rand.Range(.8f, 1.2f));
                rndPos = DrawPos;
                rndPos.x += Rand.Range(-.5f, .5f);
                rndPos.z += Rand.Range(-.5f, .5f);
                TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.ElectricalSpark, rndPos, Map, Rand.Range(.4f, .7f), .2f, .05f, .1f, 0, 0, 0, Rand.Range(0, 360));
            }
            base.Destroy(mode);
        }

        private static List<IntVec3> PortableCellsAround(IntVec3 pos, Map map, float cellRadius)
        {
            List<IntVec3> cellRange = new List<IntVec3>();
            cellRange.Clear();
            if (!pos.InBoundsWithNullCheck(map))
            {
                return null;
            }
            Region region = pos.GetRegion(map, RegionType.Set_All);
            if (region == null)
            {
                return null;
            }
            int drawRad = (int)(cellRadius * 4);
            RegionTraverser.BreadthFirstTraverse(region, (from, r) => r.door == null, delegate (Region r)
            {
                foreach (IntVec3 current in r.Cells)
                {
                    if (current.InHorDistOf(pos, cellRadius))
                    {
                        cellRange.Add(current);
                    }
                }
                return false;
            }, drawRad, RegionType.Set_All);
            return cellRange;
        }
    }
}
