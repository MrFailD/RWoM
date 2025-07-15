using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace TorannMagic
{
    [Serializable]
    public class CompSkeletonController : ThingComp
	{
        private bool initialized;

        private List<Pawn> threatList = new List<Pawn>();
        private List<Pawn> closeThreats = new List<Pawn>();
        private List<Pawn> farThreats = new List<Pawn>();
        public List<Building> buildingThreats = new List<Building>();

        public int nextRangedAttack;
        public int nextAoEAttack;
        public int nextKnockbackAttack;
        public int nextChargeAttack;
        public int nextTaunt;

        private int rangedBurstShots;
        private int rangedNextBurst;
        private LocalTargetInfo rangedTarget = null;
        private Thing launchableThing;

        private int scanTick = 279;

        private int age = -1;

        //private int actionReady = 0;
        //private int actionTick = 0;

        //private LocalTargetInfo universalTarget = null;

        public override void PostDraw()
        {
            base.PostDraw();
            if (NextChargeAttack < Find.TickManager.TicksGame)
            {
                float matMagnitude = 2.5f;
                if (Pawn.def == TorannMagicDefOf.TM_GiantSkeletonR)
                {
                    Vector3 vector = ChainDrawPos;
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                    Vector3 s = new Vector3(matMagnitude, 1, matMagnitude);
                    Matrix4x4 matrix = default(Matrix4x4);
                    float angle = 0;
                    if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South)
                    {
                        angle = Rand.Range(0, 360);
                        matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                        Graphics.DrawMesh(MeshPool.plane10, matrix, TM_MatPool.circleChain, 0);
                    }
                    else
                    {
                        angle = Rand.Range(40, 60);
                        if (Pawn.Rotation == Rot4.West)
                        {
                            angle *= -1;
                        }
                        matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                        Graphics.DrawMesh(MeshPool.plane10, matrix, TM_MatPool.lineChain, 0);
                    }
                }
            }
        }

        private Vector3 ChainDrawPos
        {
            get
            {
                Vector3 drawpos = Pawn.DrawPos;
                if(Pawn.Rotation == Rot4.North)
                {
                    drawpos.x += 1.05f;
                    drawpos.z += .49f;
                }
                else if(Pawn.Rotation == Rot4.East)
                {
                    drawpos.x -= .23f;
                    drawpos.z += 1.03f;
                }
                else if(Pawn.Rotation == Rot4.West)
                {
                    drawpos.x += .23f;
                    drawpos.z += 1.03f;
                }
                else
                {
                    drawpos.x -= 1.05f;
                    drawpos.z += .49f;
                }
                return drawpos;
            }
        }

        private Vector3 MoteDrawPos
        {
            get
            {
                Vector3 drawPos = Pawn.DrawPos;
                drawPos.z -= .9f;
                drawPos.x += Rand.Range(-.5f, .5f);
                return drawPos;
            }
        }

        private int NextRangedAttack
        {
            get
            {
                if(Props.rangedCooldownTicks > 0)
                {
                    return nextRangedAttack;
                }
                else
                {
                    return Find.TickManager.TicksGame;
                }
            }
        }

        private void StartRangedAttack()
        {
            if (rangedTarget != null && (rangedTarget.Cell - Pawn.Position).LengthHorizontal <= Props.maxRangeForFarThreat)
            {
                nextRangedAttack = (int)(Props.rangedCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
                launchableThing = null;
                launchableThing = FindNearbyObject(ThingCategoryDefOf.Corpses, 1.8f);
                if (launchableThing != null)
                {
                    rangedBurstShots = Props.rangedBurstCount;
                    rangedNextBurst = Find.TickManager.TicksGame + Props.rangedTicksBetweenBursts;
                    nextChargeAttack = Find.TickManager.TicksGame + 150;
                    TM_Action.PawnActionDelay(Pawn, 120, rangedTarget, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
                }
                else if (launchableThing == null && Rand.Chance(.1f))
                {
                    rangedBurstShots = Props.rangedBurstCount;
                    rangedNextBurst = Find.TickManager.TicksGame + Props.rangedTicksBetweenBursts;
                    nextChargeAttack = Find.TickManager.TicksGame + 150;
                    TM_Action.PawnActionDelay(Pawn, 120, rangedTarget, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
                }
            }
        }

        private void DoRangedAttack(LocalTargetInfo target)
        {            
            bool flag = target.Cell != default(IntVec3);
            if (flag)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(Pawn.Position, Pawn.Map, false), MaintenanceType.None);
                info.pitchFactor = .7f;
                info.volumeFactor = 2f;
                TorannMagicDefOf.TM_AirWoosh.PlayOneShot(info);

                CellRect cellRect = CellRect.CenteredOn(target.Cell, 3);
                cellRect.ClipInsideMap(Pawn.Map);
                IntVec3 destination = cellRect.RandomCell;

                if (launchableThing != null && destination != IntVec3.Invalid)
                {
                    float launchAngle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(Pawn.Position, destination)).ToAngleFlat();
                    for (int m = 0; m < 4; m++)
                    {
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ThickDust, Pawn.Position.ToVector3Shifted(), Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), Rand.Range(-20, 20), Rand.Range(3f, 5f), launchAngle += Rand.Range(-25, 25), Rand.Range(0, 360));
                    }
                    FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), Pawn.Position, Pawn.Map);
                    flyingObject.force = 1.4f;
                    flyingObject.Launch(Pawn, destination, launchableThing.SplitOff(1), Rand.Range(45, 65));
                }
                else if (launchableThing == null && destination != IntVec3.Invalid)
                {
                    float launchAngle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(Pawn.Position, destination)).ToAngleFlat();
                    for (int m = 0; m < 4; m++)
                    {
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ThickDust, Pawn.Position.ToVector3Shifted(), Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), Rand.Range(-20, 20), Rand.Range(3f, 5f), launchAngle += Rand.Range(-25, 25), Rand.Range(0, 360));
                    }
                    FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_SpinningBone"), Pawn.Position, Pawn.Map);
                    flyingObject.force = 1.4f;
                    flyingObject.Launch(Pawn, destination, null, Rand.Range(120, 150));
                }
            }            
        }

        private int NextAoEAttack
        {
            get
            {
                if (Props.aoeCooldownTicks > 0)
                {
                    return nextAoEAttack;
                }
                else
                {
                    return Find.TickManager.TicksGame;
                }
            }
        }

        private void DoAoEAttack(IntVec3 center, bool isExplosion, float radius, DamageDef damageType, int damageAmount, ThingDef moteDef = null)
        {
            nextAoEAttack = (int)(Props.aoeCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
            List<IntVec3> targetCells = GenRadial.RadialCellsAround(center, radius, false).ToList();
            IntVec3 curCell = default(IntVec3);
            if (damageAmount > 0)
            {
                for (int i = 0; i < targetCells.Count(); i++)
                {
                    curCell = targetCells[i];
                    if (curCell.IsValid && curCell.InBoundsWithNullCheck(Pawn.Map))
                    {
                        if (isExplosion)
                        {
                            ExplosionHelper.Explode(curCell, Pawn.Map, .4f, damageType, Pawn, damageAmount, Rand.Range(0, damageAmount), TorannMagicDefOf.TM_SoftExplosion, null, null, null, null, 0f, 1, null, false, null, 0f, 0, 0.0f, false);
                        }
                        else
                        {
                            List<Thing> thingList = curCell.GetThingList(Pawn.Map);
                            for (int j = 0; j < thingList.Count(); j++)
                            {
                                TM_Action.DamageEntities(thingList[j], null, damageAmount, damageType, Pawn);
                            }
                        }
                    }
                }
            }
            if(moteDef != null)
            {
                TM_MoteMaker.ThrowGenericMote(moteDef, Pawn.Position.ToVector3Shifted(), Pawn.Map, radius + 2, .25f, .25f, 1.75f, 0, 0, 0, Rand.Range(0, 360));
            }
        }

        private int NextKnockbackAttack
        {
            get
            {
                if (Props.knockbackCooldownTicks > 0)
                {
                    return nextKnockbackAttack;
                }
                else
                {
                    return Find.TickManager.TicksGame;
                }
            }
        }

        private void DoKnockbackAttack(IntVec3 center, IntVec3 target, float radius, float force)
        {
            nextKnockbackAttack = Props.knockbackCooldownTicks + Find.TickManager.TicksGame;
            List<IntVec3> targetCells = GenRadial.RadialCellsAround(target, radius, true).ToList();
            IntVec3 curCell = default(IntVec3);
            for (int i = 0; i < targetCells.Count(); i++)
            {
                curCell = targetCells[i];
                if (curCell.IsValid && curCell.InBoundsWithNullCheck(Pawn.Map))
                {
                    Vector3 launchVector = TM_Calc.GetVector(Pawn.Position, curCell);
                    Pawn knockbackPawn = curCell.GetFirstPawn(Pawn.Map);
                    TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, Pawn.DrawPos, Pawn.Map, Rand.Range(.6f, 1f), .01f, .01f, 1f, Rand.Range(50, 100), Rand.Range(5, 7), launchVector.ToAngleFlat(), Rand.Range(0, 360));
                    TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, curCell.ToVector3Shifted(), Pawn.Map, Rand.Range(.6f, 1f), .01f, .01f, 1f, Rand.Range(50, 100), Rand.Range(5, 7), launchVector.ToAngleFlat(), Rand.Range(0, 360));
                    if (knockbackPawn != null && knockbackPawn != Pawn)
                    {
                        IntVec3 targetCell = knockbackPawn.Position + (force * force * launchVector).ToIntVec3();
                        bool flag = targetCell != IntVec3.Invalid && targetCell != default(IntVec3);
                        if (flag)
                        {
                            if (knockbackPawn.Spawned && knockbackPawn.Map != null && !knockbackPawn.Dead)
                            {
                               FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), knockbackPawn.Position, knockbackPawn.Map, WipeMode.Vanish);
                                flyingObject.speed = 15 + (2*force);
                                flyingObject.Launch(Pawn, targetCell, knockbackPawn);
                            }
                        }
                    }
                }
            }
        }

        private int NextChargeAttack
        {
            get
            {
                if (Props.chargeCooldownTicks > 0)
                {
                    return nextChargeAttack;
                }
                else
                {
                    return Find.TickManager.TicksGame;
                }
            }
        }

        private void DoChargeAttack(LocalTargetInfo t)
        {
            if(t == null)
            {
                t = rangedTarget;
            }
            nextChargeAttack = Props.chargeCooldownTicks + Find.TickManager.TicksGame;
            bool flag = t.Cell != default(IntVec3) && t.Cell.DistanceToEdge(Pawn.Map) > 6;
            float magnitude = (t.Cell - Pawn.Position).LengthHorizontal * .35f;
            if (flag && t.Thing != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 moteDirection = TM_Calc.GetVector(Pawn.Position, t.Cell);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_GrappleHook, Pawn.DrawPos, Pawn.Map, Rand.Range(1.1f, 1.4f), 0.15f, .02f + (.08f * i), .3f - (.04f * i), Rand.Range(-10, 10), magnitude + magnitude * i, (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), Rand.Chance(.5f) ? (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat() : (Quaternion.AngleAxis(-90, Vector3.up) * moteDirection).ToAngleFlat());
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_GrappleHook, t.Thing.DrawPos, Pawn.Map, Rand.Range(1.1f, 1.4f), 0.15f, .02f + (.08f * i), .3f - (.04f * i), Rand.Range(-10, 10), magnitude + magnitude * i, (Quaternion.AngleAxis(-90, Vector3.up) * moteDirection).ToAngleFlat(), Rand.Chance(.5f) ? (Quaternion.AngleAxis(-90, Vector3.up) * moteDirection).ToAngleFlat() : (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat());
                }
                PullObject(t.Thing);
                TM_Action.PawnActionDelay(Pawn, 60, rangedTarget, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
            }
        }

        private int NextTaunt
        {
            get
            {
                if (Props.tauntCooldownTicks > 0)
                {
                    return nextTaunt;
                }
                else
                {
                    return Find.TickManager.TicksGame;
                }
            }
        }

        private void DoTaunt(Map map)
        {
            nextTaunt = (int)(Props.tauntCooldownTicks*Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
            if (map != null)
            {
                List<Pawn> threatPawns = map.mapPawns.AllPawnsSpawned.ToList();
                bool anyPawnsTaunted = false;
                if (threatPawns != null && threatPawns.Count > 0)
                {
                    int count = Mathf.Min(threatPawns.Count, 10);
                    for (int i = 0; i < count; i++)
                    {
                        if (threatPawns[i].Faction != null && Pawn.Faction != null && threatPawns[i].Faction.HostileTo(Pawn.Faction) && !threatPawns[i].IsColonist)
                        {
                            if (threatPawns[i].jobs != null && threatPawns[i].CurJob != null && threatPawns[i].CurJob.targetA != null && threatPawns[i].CurJob.targetA.Thing != null && threatPawns[i].CurJob.targetA.Thing != Pawn)
                            {
                                if (Rand.Chance(Props.tauntChance) && (threatPawns[i].Position - Pawn.Position).LengthHorizontal < 60)
                                {
                                    //Log.Message("taunting " + threatPawns[i].LabelShort + " doing job " + threatPawns[i].CurJobDef.defName + " with follow radius of " + threatPawns[i].CurJob.followRadius);
                                    if(threatPawns[i].CurJobDef == JobDefOf.Follow || threatPawns[i].CurJobDef == JobDefOf.FollowClose)
                                    {
                                        Job job = new Job(JobDefOf.AttackMelee, Pawn);
                                        threatPawns[i].jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                    }
                                    else
                                    {
                                        Job job = new Job(threatPawns[i].CurJobDef, Pawn);
                                        threatPawns[i].jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                    }                                        
                                    anyPawnsTaunted = true;                                    
                                    //Log.Message("taunting " + threatPawns[i].LabelShort);
                                }
                            }
                        }
                    }
                    if (anyPawnsTaunted)
                    {
                        MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "TM_Taunting".Translate(), -1);
                        TM_Action.PawnActionDelay(Pawn, 30, rangedTarget, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
                    }
                }
            }
        }

        private Pawn Pawn
        {
            get
            {
                Pawn pawn = parent as Pawn;
                bool flag = pawn == null;
                if (flag)
                {
                    Log.Error("pawn is null");
                }
                return pawn;
            }
        }

        private List<Pawn> PawnThreatList
        {
            get
            {
                return closeThreats.Union(farThreats).ToList();
            }
        }

        public CompProperties_SkeletonController Props
        {
            get
            {
                return (CompProperties_SkeletonController)props;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void CompTick()
        {
            if (age > 0)
            {
                if (!initialized)
                {
                    //HealthUtility.AdjustSeverity(this.Pawn, TorannMagicDefOf.TM_UndeadHD, .5f);
                    initialized = true;
                }

                if (Pawn.Spawned)
                {
                    if (!Pawn.Downed)
                    {
                        if (Pawn.Faction != null && Pawn.Faction.IsPlayer && NextTaunt < Find.TickManager.TicksGame && Rand.Chance(.2f))
                        {
                            DoTaunt(Pawn.Map);
                            nextTaunt = Props.tauntCooldownTicks + Find.TickManager.TicksGame;
                        }

                        if (rangedBurstShots > 0 && rangedNextBurst < Find.TickManager.TicksGame)
                        {
                            DoRangedAttack(rangedTarget);
                            rangedBurstShots--;
                            rangedNextBurst = Find.TickManager.TicksGame + Props.rangedTicksBetweenBursts;
                        }

                        if (Find.TickManager.TicksGame % 30 == 0)
                        {
                            if (buildingThreats.Count() > 0)
                            {
                                Building randomBuildingThreat = buildingThreats.RandomElement();
                                if ((randomBuildingThreat.Position - Pawn.Position).LengthHorizontal < 80 && NextRangedAttack < Find.TickManager.TicksGame && TargetIsValid(randomBuildingThreat))
                                {
                                    rangedTarget = randomBuildingThreat;
                                    StartRangedAttack();
                                }
                            }
                            
                            if (Pawn.CurJob != null && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null && Pawn.TargetCurrentlyAimingAt == null)
                            {
                                Thing currentTargetThing = Pawn.CurJob.targetA.Thing;
                                if ((currentTargetThing.Position - Pawn.Position).LengthHorizontal > (Props.maxRangeForCloseThreat * 2))
                                {
                                    if (Rand.Chance(.6f) && NextRangedAttack < Find.TickManager.TicksGame && TargetIsValid(currentTargetThing))
                                    {
                                        rangedTarget = currentTargetThing;
                                        StartRangedAttack();
                                    }
                                    else if (NextChargeAttack < Find.TickManager.TicksGame && TargetIsValid(currentTargetThing) && TM_Calc.HasLoSFromTo(Pawn.Position, currentTargetThing, Pawn, 3, Props.maxRangeForCloseThreat * 3) && currentTargetThing is Pawn)
                                    {
                                        DoChargeAttack(currentTargetThing);
                                        goto exitTick;
                                    }
                                }
                            }

                            if (closeThreats.Count() > 1)
                            {
                                if (Rand.Chance(.2f) && NextAoEAttack < Find.TickManager.TicksGame)
                                {
                                    //DoAoEAttack(this.Pawn.Position, true, 1.4f, DamageDefOf.Stun, Rand.Range(2, 4), null);
                                    Find.CameraDriver.shaker.DoShake(4);
                                    DoAoEAttack(Pawn.Position, false, 1f, DamageDefOf.Stun, Rand.Range(2, 4), null);
                                    DoAoEAttack(Pawn.Position, false, 2f, DamageDefOf.Crush, Rand.Range(6, 12), TorannMagicDefOf.Mote_EarthCrack);
                                    DoAoEAttack(Pawn.Position, false, 2.5f, DamageDefOf.Crush, 0, TorannMagicDefOf.Mote_EarthCrack);
                                }

                                if (Rand.Chance(.1f) && farThreats.Count() > (5 * closeThreats.Count()))
                                {
                                    Pawn.CurJob.targetA = farThreats.RandomElement();
                                }
                            }

                            //    if (this.closeThreats.Count() > 1 && ((this.closeThreats.Count() * 2) > this.farThreats.Count() || Rand.Chance(.3f)))
                            //    {
                            //        if (Rand.Chance(.8f) && this.NextKnockbackAttack < Find.TickManager.TicksGame)
                            //        {
                            //            Pawn randomClosePawn = this.closeThreats.RandomElement();
                            //            if ((randomClosePawn.Position - this.Pawn.Position).LengthHorizontal < 3 && TargetIsValid(randomClosePawn))
                            //            {
                            //                DoKnockbackAttack(this.Pawn.Position, randomClosePawn.Position, 1.4f, Rand.Range(3, 5f));
                            //            }
                            //        }
                            //    }

                            if (farThreats.Count() > 2 * closeThreats.Count() && Rand.Chance(.3f))
                            {
                                Pawn randomRangedPawn = farThreats.RandomElement();
                                if (NextChargeAttack < Find.TickManager.TicksGame)
                                {
                                    Thing tempTarget = farThreats.RandomElement();
                                    if (TargetIsValid(tempTarget) && TM_Calc.HasLoSFromTo(Pawn.Position, tempTarget, Pawn, Props.maxRangeForCloseThreat, Props.maxRangeForCloseThreat * 4))
                                    {
                                        Pawn.TryStartAttack(tempTarget);
                                        DoChargeAttack(tempTarget);
                                        goto exitTick;
                                    }
                                }
                            }

                            if (farThreats.Count() > 2)
                            {
                                if (Rand.Chance(.4f) && NextRangedAttack < Find.TickManager.TicksGame)
                                {
                                    Pawn randomRangedPawn = farThreats.RandomElement();
                                    if ((randomRangedPawn.Position - Pawn.Position).LengthHorizontal < Props.maxRangeForFarThreat * 2f)
                                    {
                                        rangedTarget = randomRangedPawn;
                                        StartRangedAttack();
                                    }
                                }
                            }

                            if (Pawn.CurJob != null)
                            {
                                if (Pawn.CurJob.targetA == null  || Pawn.CurJob.targetA == Pawn)
                                {
                                    if (closeThreats.Count() > 0)
                                    {
                                        Thing tempTarget = closeThreats.RandomElement();
                                        if (TargetIsValid(tempTarget))
                                        {
                                            Pawn.CurJob.targetA = tempTarget;
                                            Pawn.TryStartAttack(Pawn.CurJob.targetA);
                                        }
                                    }
                                    else if (farThreats.Count() > 0)
                                    {
                                        Thing tempTarget = farThreats.RandomElement();
                                        if (TargetIsValid(tempTarget))
                                        {
                                            Pawn.CurJob.targetA = tempTarget;
                                            Pawn.TryStartAttack(Pawn.CurJob.targetA);
                                        }
                                    }
                                    else if (buildingThreats.Count() > 0)
                                    {
                                        Thing tempTarget = buildingThreats.RandomElement();
                                        if (TargetIsValid(tempTarget))
                                        {
                                            Pawn.CurJob.targetA = tempTarget;
                                            Pawn.TryStartAttack(Pawn.CurJob.targetA);
                                        }
                                    }
                                }
                            }

                        }

                        if (Find.TickManager.TicksGame >= scanTick)
                        {
                            scanTick = Rand.Range(250, 320) + Find.TickManager.TicksGame;
                            DetermineThreats();
                        }
                    }

                    if (Pawn.Downed)
                    {
                        Pawn.Kill(null);
                    }

                    if (Pawn.def == TorannMagicDefOf.TM_GiantSkeletonR && Find.TickManager.TicksGame % 9 == 0)
                    {
                        ThingDef rndMote = TorannMagicDefOf.Mote_BoneDust;
                        TM_MoteMaker.ThrowGenericMote(rndMote, MoteDrawPos, Pawn.Map, .6f, .1f, 0f, .6f, Rand.Range(-200, 200), 0f, 0, Rand.Range(0, 360));
                        TM_MoteMaker.ThrowGenericMote(rndMote, MoteDrawPos, Pawn.Map, .6f, .1f, 0f, .6f, Rand.Range(-200, 200), 0f, 0, Rand.Range(0, 360));
                    }
                }
            }
            exitTick:;
            age++;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Instigator is Building instigatorThing)
            {
                if (instigatorThing.Faction != null && instigatorThing.Faction != Pawn.Faction)
                {
                    buildingThreats.AddDistinct(instigatorThing);
                }                
            }
        }

        private void DetermineThreats()
        {
            closeThreats.Clear();
            farThreats.Clear();
            List<Pawn> allPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
            for (int i = 0; i < allPawns.Count; i++)
            {
                if (!allPawns[i].DestroyedOrNull() && allPawns[i] != Pawn)
                {
                    if (!allPawns[i].Dead && !allPawns[i].Downed)
                    {
                        if (allPawns[i].Faction != null && (allPawns[i].Faction.HostileTo(Pawn.Faction)) && !allPawns[i].IsPrisoner)
                        {
                            if ((allPawns[i].Position - Pawn.Position).LengthHorizontal <= Props.maxRangeForCloseThreat)
                            {
                                closeThreats.Add(allPawns[i]);
                            }
                            else if ((allPawns[i].Position - Pawn.Position).LengthHorizontal <= Props.maxRangeForFarThreat)
                            {
                                farThreats.Add(allPawns[i]);
                            }
                        }
                        if (allPawns[i].Faction == null && allPawns[i].InMentalState)
                        {
                            if ((allPawns[i].Position - Pawn.Position).LengthHorizontal <= Props.maxRangeForCloseThreat)
                            {
                                closeThreats.Add(allPawns[i]);
                            }
                            else if ((allPawns[i].Position - Pawn.Position).LengthHorizontal <= Props.maxRangeForFarThreat)
                            {
                                farThreats.Add(allPawns[i]);
                            }
                        }
                    }
                }
            }
            if (closeThreats.Count() < 1 && farThreats.Count() < 1)
            {
                Pawn randomMapPawn = allPawns.RandomElement();
                if (TargetIsValid(randomMapPawn) && randomMapPawn.RaceProps.Humanlike)
                {
                    if (randomMapPawn.Faction != null && randomMapPawn.Faction.HostileTo(Pawn.Faction))
                    {
                        farThreats.Add(randomMapPawn);
                    }
                }
            }
            for (int i = 0; i < buildingThreats.Count(); i++)
            {
                if (buildingThreats[i].DestroyedOrNull())
                {
                    buildingThreats.Remove(buildingThreats[i]);
                }
            }
        }

        public bool TargetIsValid(Thing target)
        {
            if(target.DestroyedOrNull())
            {
                return false;
            }
            if(!target.Spawned)
            {
                return false;
            }
            if(target is Pawn targetPawn)
            {
                return !targetPawn.Downed;
            }
            if(target.Position.DistanceToEdge(Pawn.Map) < 8)
            {
                return false;
            }
            if(target.Faction != null)
            {
                return target.Faction != Pawn.Faction && target.Faction.HostileTo(Pawn.Faction);
            }
            return true;
        }

        public Thing FindNearbyObject(ThingCategoryDef tcd, float radius)
        {
            List<IntVec3> searchCells = GenRadial.RadialCellsAround(Pawn.Position, radius, true).ToList();
            List<Thing> returnThings = new List<Thing>();
            returnThings.Clear();
            for (int i = 0; i < searchCells.Count(); i++)
            {
                if (searchCells[i].IsValid && searchCells[i].InBoundsWithNullCheck(Pawn.Map))
                {
                    List<Thing> cellList = searchCells[i].GetThingList(Pawn.Map);
                    for (int j = 0; j<cellList.Count(); j++)
                    {
                        try
                        {
                            if (cellList[j].def.thingCategories != null)
                            {
                                if (cellList[j].def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks) || cellList[j].def.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks) || cellList[j].def.thingCategories.Contains(tcd))
                                {
                                    returnThings.Add(cellList[j]);
                                }
                                if(cellList[j] is Corpse)
                                {
                                    returnThings.Add(cellList[j]);
                                }
                            }
                            if(cellList[j].def == TorannMagicDefOf.TM_SkeletonR && TM_Calc.HasLoSFromTo(Pawn.Position, rangedTarget, Pawn, 4, Props.maxRangeForFarThreat))
                            {
                                returnThings.Add(cellList[j]);
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            //Log.Message("threw exception " + ex);
                        }
                    }
                }
            }
            if (returnThings != null && returnThings.Count > 0)
            {
                return returnThings.RandomElement();
            }
            return null;
        }

        public void PullObject(Thing t)
        {
            Thing summonableThing = t;
            Pawn victim = null;
            if (summonableThing != null)
            {
                victim = t as Pawn;
                if (victim != null)
                {
                    DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Stun, 10, 10, -1, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, victim);
                    if (!victim.RaceProps.Humanlike || victim.Faction == Pawn.Faction)
                    {
                        FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), victim.Position, Pawn.Map, WipeMode.Vanish);
                        flyingObject.speed = 25;
                        flyingObject.Launch(victim, Pawn.Position, victim);
                    }
                    else if (victim.RaceProps.Humanlike && victim.Faction != Pawn.Faction && Rand.Chance(TM_Calc.GetSpellSuccessChance(Pawn, victim, true)))
                    {
                        FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), victim.Position, Pawn.Map, WipeMode.Vanish);
                        flyingObject.speed = Rand.Range(23f, 27f);
                        flyingObject.Launch(victim, Pawn.Position, victim);
                    }
                    else
                    {
                        MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                    }
                }
            }
        }
    }
}