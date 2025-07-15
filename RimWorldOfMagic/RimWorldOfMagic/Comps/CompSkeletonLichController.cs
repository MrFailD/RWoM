using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Sound;
using AbilityUser;

namespace TorannMagic
{
    [Serializable]
    public class CompSkeletonLichController : ThingComp
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
        public int castingCompleteTick;

        private int rangedBurstShots;
        private int rangedNextBurst;
        private LocalTargetInfo rangedTarget = null;
        private LocalTargetInfo flightTarget = null;
        private bool shouldDoAOEAttack;
        private bool shouldDoKnockBackAttack;
        private bool shouldDoTaunt;
        private LocalTargetInfo attackTarget = null;
        public LocalTargetInfo tauntTarget = null;

        private int age = -1;

        public float geChance = .0023f;
        public float leChance = .011f;
        public float raiseRadius = 4f;
        public bool shouldAssault;

        //private int actionReady = 0;
        //private int actionTick = 0;

        //private LocalTargetInfo universalTarget = null;

        //This comp controller is unique to the undead lich and interfaces with the wandering lich event
        //Unique abilities:
        //Ranged attack - launches several death bolts, similar to a player lich master spell
        //AoE attack - creates fields of "fog of torment" that will heal other undead
        //Knockback attack - stuns (short) and curses nearby pawns
        //Charge attack - flight, used primarily to escape from damage or when too many enemies are nearby
        //Taunt - raises a host of undead skeletons to fight for the lich

        private Vector3 MoteDrawPos
        {
            get
            {
                Vector3 drawPos = Pawn.DrawPos;
                drawPos.z -= .6f;
                drawPos.x += Rand.Range(-.5f, .5f);
                return drawPos;
            }
        }

        public bool IsCasting
        {
            get
            {
                return castingCompleteTick >= Find.TickManager.TicksGame;
            }
        }

        public Pawn ParentPawn
        {
            get
            {
                return Pawn;
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
            if (rangedTarget != null && TM_Calc.HasLoSFromTo(Pawn.Position, rangedTarget, Pawn, 4, Props.maxRangeForFarThreat))
            {
                nextRangedAttack = (int)(Props.rangedCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
                rangedBurstShots = Props.rangedBurstCount;
                rangedNextBurst = Find.TickManager.TicksGame + Props.rangedTicksBetweenBursts;
                castingCompleteTick = Find.TickManager.TicksGame + Props.rangedAttackDelay;
                
                //this.Pawn.CurJob.SetTarget(this.Pawn.jobs.curDriver.rotateToFace, rangedTarget);
                TM_Action.PawnActionDelay(Pawn, Props.rangedAttackDelay, rangedTarget, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
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

                CellRect cellRect = CellRect.CenteredOn(target.Cell, 4);
                cellRect.ClipInsideMap(Pawn.Map);
                IntVec3 destination = cellRect.RandomCell;

                if (destination != IntVec3.Invalid)
                {
                    Thing launchedThing = new Thing()
                    {
                        def = TorannMagicDefOf.FlyingObject_DeathBolt
                    };
                    Pawn casterPawn = Pawn;
                    //LongEventHandler.QueueLongEvent(delegate
                    //{
                        FlyingObject_DeathBolt flyingObject = (FlyingObject_DeathBolt)GenSpawn.Spawn(TorannMagicDefOf.FlyingObject_DeathBolt, Pawn.Position, Pawn.Map);
                        flyingObject.Launch(Pawn, destination, launchedThing);
                    //}, "LaunchingFlyer", false, null);
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

        private void StartAoEAttack(IntVec3 center, LocalTargetInfo target)
        {
            if (target.Thing != null && target.Thing.Map == Pawn.Map)
            {
                nextAoEAttack = (int)(Props.aoeCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
                castingCompleteTick = Props.aoeAttackDelay + Find.TickManager.TicksGame;
                TM_Action.PawnActionDelay(Pawn, Props.aoeAttackDelay, target, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
                shouldDoAOEAttack = true;
            }
        }

        private void DoAoEAttack(IntVec3 center, LocalTargetInfo target)
        {
            TM_CopyAndLaunchProjectile.CopyAndLaunchThing(TorannMagicDefOf.Projectile_FogOfTorment, Pawn, center, target, ProjectileHitFlags.All, null);
            shouldDoAOEAttack = false;
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

        private void StartKnockbackAttack(IntVec3 target, float radius)
        {
            nextKnockbackAttack = (int)(Props.knockbackCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
            castingCompleteTick = Props.knockbackAttackDelay + Find.TickManager.TicksGame;
            TM_Action.PawnActionDelay(Pawn, Props.knockbackAttackDelay, target, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
            shouldDoKnockBackAttack = true;
        }

        private void DoKnockbackAttack(IntVec3 target, float radius)
        {
            int pwrVal = 3;
            int verVal = 3;
            List<Pawn> TargetsAoE = TM_Calc.FindPawnsNearTarget(Pawn, 5, target, true);
            if (TargetsAoE != null && TargetsAoE.Count > 0)
            {
                for (int i = 0; i < TargetsAoE.Count; i++)
                {
                    Pawn victim = TargetsAoE[i];
                    if (!victim.RaceProps.IsMechanoid)
                    {
                        if (Rand.Chance(TM_Calc.GetSpellSuccessChance(Pawn, victim, true)))
                        {
                            HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_DeathMarkCurse"), (Rand.Range(1f + pwrVal, 4 + 2 * pwrVal)));
                            TM_MoteMaker.ThrowSiphonMote(victim.DrawPos, victim.Map, 1f);

                            if (Rand.Chance(verVal * .2f))
                            {
                                if (Rand.Chance(verVal * .1f)) //terror
                                {
                                    HealthUtility.AdjustSeverity(victim, HediffDef.Named("TM_Terror"), Rand.Range(3f * verVal, 5f * verVal));
                                    TM_MoteMaker.ThrowDiseaseMote(victim.DrawPos, victim.Map, 1f, .5f, .2f, .4f);
                                    MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Terror", -1);
                                }
                                if (Rand.Chance(verVal * .1f)) //berserk
                                {
                                    if (victim.mindState != null && victim.RaceProps != null && victim.RaceProps.Humanlike)
                                    {
                                        victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "cursed", true, false, false, null);
                                        FleckMaker.ThrowMicroSparks(victim.DrawPos, victim.Map);
                                        MoteMaker.ThrowText(victim.DrawPos, victim.Map, "Berserk", -1);
                                    }

                                }
                            }
                        }
                        else
                        {
                            MoteMaker.ThrowText(victim.DrawPos, victim.Map, "TM_ResistedSpell".Translate(), -1);
                        }
                    }
                }
            }
            shouldDoKnockBackAttack = false;
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

        private void StartChargeAttack(IntVec3 t)
        {
            nextChargeAttack = Props.chargeCooldownTicks + Find.TickManager.TicksGame;
            bool flag = t != IntVec3.Invalid && t.DistanceToEdge(Pawn.Map) > 6;
            if (flag && t.InBoundsWithNullCheck(Pawn.Map) && t.IsValid && t.Walkable(Pawn.Map) && Pawn.Position.DistanceTo(t) <= 60)
            {
                castingCompleteTick = Find.TickManager.TicksGame + Props.chargeAttackDelay;
                flightTarget = t;
                TM_Action.PawnActionDelay(Pawn, Props.chargeAttackDelay, t, Pawn.meleeVerbs.TryGetMeleeVerb(rangedTarget.Thing));
            }
            else
            {
                flightTarget = null;
            }
        }

        private void DoChargeAttack(LocalTargetInfo t)
        {
            if (t != null && t.Cell.DistanceToEdge(Pawn.Map) > 6)
            {
                Pawn.rotationTracker.Face(t.CenterVector3);
               // Log.Message("flying to " + t.Cell);
                LongEventHandler.QueueLongEvent(delegate
                {
                    FlyingObject_Flight flyingObject = (FlyingObject_Flight)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Flight"), Pawn.Position, Pawn.Map);
                    flyingObject.Launch(Pawn, t.Cell, Pawn);
                }, "LaunchingFlyer", false, null);
                flightTarget = null;
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

        public void GotoRaiseLocation(Map map, LocalTargetInfo target)
        {
            nextTaunt = (int)(Props.tauntCooldownTicks * Rand.Range(.9f, 1.1f)) + Find.TickManager.TicksGame;
            tauntTarget = target;
            Job job = new Job(JobDefOf.Goto, target);
            job.locomotionUrgency = LocomotionUrgency.Amble;
            Pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, false, false);
        }

        private void StartTaunt(Map map, LocalTargetInfo target)
        {
            castingCompleteTick = Find.TickManager.TicksGame + Props.tauntAttackDelay;
            TM_Action.PawnActionDelay(Pawn, Props.tauntAttackDelay, target, Pawn.meleeVerbs.TryGetMeleeVerb(Pawn));
            shouldDoTaunt = true;
        }

        private void DoTaunt(Map map, LocalTargetInfo target)
        {
            shouldDoTaunt = false;
            tauntTarget = null;
            if (map != null)
            {
                SpawnSkeletonMinions(Pawn.Position, raiseRadius, Pawn.Faction);
                shouldAssault = true;
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

        public CompProperties_SkeletonLichController Props
        {
            get
            {
                return (CompProperties_SkeletonLichController)props;
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
                    HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_LichHD, .5f);
                    initialized = true;
                }                

                if (Pawn.Spawned)
                {                    
                    if (!Pawn.Downed)
                    {
                        if (!Pawn.stances.curStance.StanceBusy)
                        {
                            if (tauntTarget != null && Pawn.Faction != null && !Pawn.Faction.IsPlayer && NextTaunt < Find.TickManager.TicksGame && Pawn.CurJob.def != JobDefOf.Goto)
                            {
                                if((tauntTarget.Cell - Pawn.Position).LengthHorizontal > 5)
                                {
                                    GotoRaiseLocation(Pawn.Map, tauntTarget);
                                }
                                else
                                {
                                    StartTaunt(Pawn.Map, tauntTarget);
                                }
                            }

                            if(shouldDoTaunt)
                            {
                                DoTaunt(Pawn.Map, tauntTarget.Cell);
                            }

                            if (flightTarget != null)
                            {
                                DoChargeAttack(flightTarget.Cell);
                                goto exitTick;
                            }                            

                            if (rangedBurstShots > 0 && rangedNextBurst < Find.TickManager.TicksGame)
                            {
                                DoRangedAttack(rangedTarget);
                                rangedBurstShots--;
                                rangedNextBurst = Find.TickManager.TicksGame + Props.rangedTicksBetweenBursts;
                            }

                            if(shouldDoAOEAttack)
                            {
                                if (attackTarget != null)
                                {
                                    DoAoEAttack(attackTarget.Cell, attackTarget);
                                }
                            }

                            if(shouldDoKnockBackAttack)
                            {
                                if (attackTarget != null)
                                {
                                    DoKnockbackAttack(attackTarget.Cell, 5);
                                }
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
                                    }
                                }
                                else if (Pawn.TargetCurrentlyAimingAt != null && closeThreats.Count() > 3)
                                {
                                    if (Rand.Chance(.4f) && NextAoEAttack < Find.TickManager.TicksGame && TM_Calc.HasLoSFromTo(Pawn.Position, attackTarget, Pawn, 0, 60))
                                    {
                                        attackTarget = Pawn.TargetCurrentlyAimingAt;
                                        StartAoEAttack(attackTarget.Cell, attackTarget);                                        
                                    }

                                    if (Rand.Chance(.8f) && NextAoEAttack < Find.TickManager.TicksGame && farThreats.Count() > (4 * closeThreats.Count()) && TM_Calc.HasLoSFromTo(Pawn.Position, attackTarget, Pawn, 0, 60))
                                    {
                                        Pawn p = farThreats.RandomElement();
                                        if (TM_Calc.FindAllPawnsAround(Pawn.Map, p.Position, 5, p.Faction, false).Count > 3)
                                        {
                                            attackTarget = p;
                                            StartAoEAttack(p.Position, p);
                                        }
                                    }
                                }

                                if(closeThreats != null && closeThreats.Count > 5)
                                {
                                    if (Rand.Chance(.2f) && NextKnockbackAttack < Find.TickManager.TicksGame)
                                    {
                                        Pawn p = closeThreats.RandomElement();
                                        if (p.health != null && p.health.hediffSet != null && !p.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DeathMarkHD))
                                        {
                                            attackTarget = p;
                                            StartKnockbackAttack(p.Position, 5);
                                        }
                                    }
                                    else if(Rand.Chance(.4f) && NextChargeAttack < Find.TickManager.TicksGame)
                                    {
                                        flightTarget = TM_Calc.TryFindSafeCell(Pawn, Pawn.Position, 40, 3, 2);
                                        StartChargeAttack(flightTarget.Cell);
                                    }
                                }

                                if (farThreats.Count() < 2 * closeThreats.Count() && Rand.Chance(.3f))
                                {
                                    if (NextChargeAttack < Find.TickManager.TicksGame && farThreats.Count >= 1)
                                    {
                                        Pawn tempTarget = farThreats.RandomElement();
                                        if (TargetIsValid(tempTarget) && (tempTarget.Position - Pawn.Position).LengthHorizontal > (Props.maxRangeForCloseThreat * 3) && (tempTarget.Position - Pawn.Position).LengthHorizontal < (Props.maxRangeForCloseThreat * 6))
                                        {
                                            flightTarget = tempTarget;
                                            StartChargeAttack(flightTarget.Cell);
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

                                    if(Rand.Chance(.2f) && NextAoEAttack < Find.TickManager.TicksGame)
                                    {
                                        Pawn p = farThreats.RandomElement();
                                        if ((p.Position - Pawn.Position).LengthHorizontal < Props.maxRangeForFarThreat * 2f && TM_Calc.HasLoSFromTo(Pawn.Position, attackTarget, Pawn, 0, 60))
                                        {
                                            List<Pawn> threatPawns = TM_Calc.FindAllPawnsAround(Pawn.Map, p.Position, 5, p.Faction, true);
                                            if (threatPawns != null && threatPawns.Count > 3)
                                            {
                                                attackTarget = p;
                                                StartAoEAttack(attackTarget.Cell, attackTarget);
                                            }
                                        }
                                    }
                                }

                                if (Pawn.CurJob != null)
                                {
                                    if (Pawn.CurJob.targetA == null || Pawn.CurJob.targetA == Pawn)
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
                        }
                        else if(IsCasting)
                        {
                            if (Find.TickManager.TicksGame % 12 == 0)
                            {
                                TM_MoteMaker.ThrowCastingMote_Anti(Pawn.DrawPos, Pawn.Map, 2f);
                            }
                        }                        

                        if (Find.TickManager.TicksGame % 279 == 0)
                        {
                            DetermineThreats();
                        }
                    }

                    if (Find.TickManager.TicksGame % 4 == 0)
                    {
                        FleckDef rndMote = FleckDefOf.Smoke;
                        TM_MoteMaker.ThrowGenericFleck(rndMote, MoteDrawPos, Pawn.Map, Rand.Range(.4f, .5f), .1f, 0f, Rand.Range(.5f, .6f), Rand.Range(-40, 40), Rand.Range(.2f, .3f), Rand.Range(-95, -110), Rand.Range(0, 360));
                        TM_MoteMaker.ThrowGenericFleck(rndMote, MoteDrawPos, Pawn.Map, Rand.Range(.4f, .5f), .1f, 0f, Rand.Range(.5f, .6f), Rand.Range(-40, 40), Rand.Range(.2f, .3f), Rand.Range(90, 110), Rand.Range(0, 360));
                    }

                    if (Pawn.Downed)
                    {
                        Pawn.Kill(null);
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
            //Log.Message("taking damage");
            if (dinfo.Instigator is Building instigatorThing)
            {
                if (instigatorThing.Faction != null && instigatorThing.Faction != Pawn.Faction)
                {
                        //Log.Message("adding building threat");
                        buildingThreats.AddDistinct(instigatorThing);
                    
                }
            }
        }

        private void DetermineThreats()
        {
            //Log.Message("checking threats - lich");
            closeThreats.Clear();
            farThreats.Clear();
            List<Pawn> allPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
            for (int i = 0; i < allPawns.Count; i++)
            {
                if (!allPawns[i].DestroyedOrNull() && allPawns[i] != Pawn)
                {
                    if (!allPawns[i].Dead && !allPawns[i].Downed)
                    {
                        if (allPawns[i].Faction != null && allPawns[i].Faction != Pawn.Faction)
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
                        if(allPawns[i].Faction == null && allPawns[i].InMentalState)
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
                    if (randomMapPawn.Faction != null && randomMapPawn.Faction != Pawn.Faction)
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
            //LearnAndShareBuildingThreats();

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
            if (target.Map != Pawn.Map)
            {
                return false;
            }
            if(target.Faction != null)
            {
                return target.Faction != Pawn.Faction && target.Faction.HostileTo(Pawn.Faction);
            }
            return true;
        }

        public void SpawnSkeletonMinions(IntVec3 center, float radius, Faction faction)
        {
            IntVec3 curCell;
            Map map = Pawn.Map;
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            for (int j = 0; j < targets.Count(); j++)
            {
                curCell = targets.ToArray<IntVec3>()[j];
                List<Thing> cellList = curCell.GetThingList(Pawn.Map);
                float corpseMult = 0f;
                for(int i =0; i < cellList.Count; i++)
                {
                    if(cellList[i] is Corpse)
                    {
                        corpseMult = .7f;
                        Corpse c = cellList[i] as Corpse;
                        c.Strip();
                        c.Destroy(DestroyMode.Vanish);
                    }
                }
                if (curCell.InBoundsWithNullCheck(map) && curCell.Walkable(map))
                {
                    SpawnThings skeleton = new SpawnThings();
                    if (Rand.Chance(geChance + corpseMult))
                    {
                        skeleton.def = TorannMagicDefOf.TM_GiantSkeletonR;
                        skeleton.kindDef = PawnKindDef.Named("TM_GiantSkeleton");
                    }
                    else if (Rand.Chance(leChance))
                    {
                        skeleton.def = TorannMagicDefOf.TM_SkeletonR;
                        skeleton.kindDef = PawnKindDef.Named("TM_Skeleton");
                    }
                    else
                    {
                        skeleton = null;
                    }
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Disease, curCell.ToVector3Shifted(), Pawn.Map, Rand.Range(1.5f, 2.4f), 1f, Rand.Range(.05f, .3f), Rand.Range(.8f, 2f), Rand.Range(0, 50), Rand.Range(.5f, 1f), 30, Rand.Range(0, 360));

                    if (skeleton != null)
                    {
                        TM_Action.SingleSpawnLoop(null, skeleton, curCell, map, 0, false, false, faction);
                    }
                }
            }
        }

        public void LearnAndShareBuildingThreats()
        {
            //Log.Message("sharing threats");
            List<Pawn> allPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
            for(int i =0; i < allPawns.Count; i++)
            {
                Pawn p = allPawns[i];
                if(p != Pawn && p.Faction == Pawn.Faction)
                {
                    if(p.def == TorannMagicDefOf.TM_SkeletonLichR)
                    {
                        CompSkeletonLichController comp = p.GetComp<CompSkeletonLichController>();
                        if (comp != null && comp.buildingThreats != null && comp.buildingThreats.Count > 0)
                        {
                            for (int j = 0; j < comp.buildingThreats.Count; j++)
                            {
                                buildingThreats.AddDistinct(comp.buildingThreats[j]);
                            }
                            for (int j = 0; j < buildingThreats.Count; j++)
                            {
                                comp.buildingThreats.AddDistinct(buildingThreats[j]);
                            }
                        }
                    }
                    else if(p.def == TorannMagicDefOf.TM_GiantSkeletonR)
                    {
                        CompSkeletonController comp = p.GetComp<CompSkeletonController>();
                        if (comp != null && comp.buildingThreats != null && comp.buildingThreats.Count > 0)
                        {
                            for (int j = 0; j < comp.buildingThreats.Count; j++)
                            {
                                buildingThreats.AddDistinct(comp.buildingThreats[j]);
                            }
                            for (int j = 0; j < buildingThreats.Count; j++)
                            {
                                comp.buildingThreats.AddDistinct(buildingThreats[j]);
                            }
                        }
                    }
                }
            }
            //Log.Message("ending threat share");
        }
    }
}