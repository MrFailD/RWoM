using RimWorld;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_LivingWall : Projectile
    {
        protected new Vector3 origin;        
        protected new Vector3 destination;
        protected Vector3 trueOrigin;
        protected Vector3 trueDestination;

        public float speed = 30f;
        protected new int ticksToImpact;
        protected Thing assignedTarget;
        protected Thing flyingThing;

        public ThingDef moteDef;
        public int moteFrequency;

        public bool spinning;
        public float curveVariance; // 0 = no curve
        private List<Vector3> curvePoints = new List<Vector3>();
        public float force = 1f;
        private int destinationCurvePoint;
        private float impactRadius;
        private int explosionDamage;
        private bool isExplosive;
        private DamageDef impactDamageType;
        private bool fliesOverhead;

        private bool earlyImpact = false;
        private float impactForce = 0;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int weaponDmg = 0;
        private int doublesidedVariance;

        private int searchEnemySpeed = 200;
        private float searchEnemyRange = 3f;
        private float enemyDamage = 12;

        private int idleFor;
        private Thing targetWall;
        private bool shouldDestroy;

        private Pawn pawn;
        public Pawn CasterPawn
        {
            get
            {
                if(!pawn.DestroyedOrNull() && !pawn.Dead)
                {
                    return pawn;
                }
                else
                {
                    shouldDestroy = true;
                }
                return null;
            }
        }

        //Magic related
        private CompAbilityUserMagic comp;
        private TMPawnSummoned newPawn = new TMPawnSummoned();

        public Building OccupiedWall
        {
            get
            {
                List<Thing> tmpList = Position.GetThingList(Map);
                foreach(Thing t in tmpList)
                {
                    if(TM_Calc.IsWall(t))
                    {
                        return t as Building;
                    }
                }
                return null;
            }
        }

        public Building DestinationWall
        {
            get
            {
                Building fromWall = null;
                if (curvePoints.Count > 0)
                {
                    List<Thing> destList = curvePoints[curvePoints.Count - 1].ToIntVec3().GetThingList(Map);

                    foreach (Thing w in destList)
                    {
                        if (TM_Calc.IsWall(w))
                        {
                            fromWall = w as Building;
                        }
                    }
                }
                if (fromWall == null)
                {
                    fromWall = OccupiedWall;
                }
                return fromWall;
            }
        }

        protected new int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((origin - destination).magnitude / (speed / 100f));
                bool flag = num < 1;
                if (flag)
                {
                    num = 1;
                }
                return num;
            }
        }

        protected new IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(destination);
            }
        }

        public new Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (destination - origin) * (1f - (float)ticksToImpact / (float)StartingTicksToImpact);
                return origin + b + Vector3.up * def.Altitude;
            }
        }

        public new Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(destination - origin);
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return ExactPosition;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            Scribe_Values.Look<float>(ref curveVariance, "curveVariance", 1f, false);
            Scribe_Values.Look<float>(ref speed, "speed", 15f, false);
            Scribe_Collections.Look<Building>(ref connectedWalls, "connectedWalls", LookMode.Reference);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            flyingThing.ThingID += Rand.Range(0, 214).ToString();
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void ExactLaunch(ThingDef effectMote, int moteFrequencyTicks, bool shouldSpin, List<Vector3> travelPath, Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, int flyingSpeed, float _impactRadius)
        {
            moteFrequency = moteFrequencyTicks;
            moteDef = effectMote;
            impactRadius = _impactRadius;
            spinning = shouldSpin;
            speed = flyingSpeed;
            curvePoints = travelPath;
            curveVariance = 1;
            Launch(launcher, origin, targ, flyingThing, null);
        }

        public void AdvancedLaunch(Thing launcher, ThingDef effectMote, int moteFrequencyTicks, float curveAmount, bool shouldSpin, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, int flyingSpeed, bool isExplosion, int _impactDamage, float _impactRadius, DamageDef damageType, DamageInfo? newDamageInfo = null, int doubleVariance = 0, bool flyOverhead = false)
        {
            fliesOverhead = flyOverhead;
            explosionDamage = _impactDamage;
            isExplosive = isExplosion;
            impactRadius = _impactRadius;
            impactDamageType = damageType;
            moteFrequency = moteFrequencyTicks;
            moteDef = effectMote;
            curveVariance = curveAmount;
            spinning = shouldSpin;
            speed = flyingSpeed;
            doublesidedVariance = doubleVariance;
            curvePoints = new List<Vector3>();
            curvePoints.Clear();
            Launch(launcher, origin, targ, flyingThing, newDamageInfo);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;            
            pawn = launcher as Pawn;
            if (spawned)
            {               
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            trueOrigin = origin;
            trueDestination = targ.Cell.ToVector3();
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            speed = speed * force;
            this.origin = origin;
            if(curveVariance > 0)
            {
                CalculateCurvePoints(trueOrigin, trueDestination, curveVariance);
                destinationCurvePoint++;
                destination = curvePoints[destinationCurvePoint];
            }
            else
            {
                destination = trueDestination;
            }            
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }        

        public void CalculateCurvePoints(Vector3 start, Vector3 end, float variance)
        {
            int variancePoints = 20;
            Vector3 initialVector = GetVector(start, end);
            initialVector.y = 0;
            float initialAngle = (initialVector).ToAngleFlat(); 
            float curveAngle = variance;
            if(doublesidedVariance == 0 && Rand.Chance(.5f))
            { 
                curveAngle = (-1) * variance;
            }
            else
            {
                curveAngle = (doublesidedVariance * variance);
            }

            //calculate extra distance bolt travels around the ellipse
            float a = .5f * Vector3.Distance(start, end);
            float b = a * Mathf.Sin(.5f * Mathf.Deg2Rad * variance);
            float p = .5f * Mathf.PI * (3 * (a + b) - (Mathf.Sqrt((3 * a + b) * (a + 3 * b))));
                    
            float incrementalDistance = p / variancePoints; 
            float incrementalAngle = (curveAngle / variancePoints) * 2f;
            curvePoints.Add(trueOrigin);
            for(int i = 1; i <= (variancePoints + 1); i++)
            {
                curvePoints.Add(curvePoints[i - 1] + ((Quaternion.AngleAxis(curveAngle, Vector3.up) * initialVector) * incrementalDistance));
                curveAngle -= incrementalAngle;
            }
        }

        public Vector3 GetVector(Vector3 center, Vector3 objectPos)
        {
            Vector3 heading = (objectPos - center);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public static Pawn closestThreat;
        public void FindClosestThreat()
        {            
            closestThreat = null;
            float closest = 999f;
            foreach(Pawn p in Map.mapPawns.AllPawnsSpawned)
            {
                float pDistance = (p.Position - Position).LengthHorizontal;
                if (!p.Dead && !p.Downed && CasterPawn != null && p.HostileTo(CasterPawn) &&  pDistance < closest)
                {
                    closest = pDistance;
                    closestThreat = p;
                }
            }
        }

        private List<Building> connectedWalls = new List<Building>();        
        public void FindClosestWallToTarget()
        {            
            if(connectedWalls == null)
            {
                connectedWalls = new List<Building>();
            }
            if(closestThreat != null && connectedWalls != null && connectedWalls.Count > 0)
            {
                float closest = (Position - closestThreat.Position).LengthHorizontal;
                Thing closestWall = null;
                foreach(Thing t in connectedWalls)
                {
                    float distance = (t.Position - closestThreat.Position).LengthHorizontal;
                    if (distance <= closest)
                    {
                        closest = distance;
                        closestWall = t;
                    }
                }
                targetWall = closestWall;
            }
            
        }

        public Thing FindClosestWallFromTarget()
        {
            Thing tmp = null;
            float closest = 999;
            IEnumerable<Building> allThings = from def in Map.listerThings.AllThings
                                              where (def is Building && TM_Calc.IsWall(def))
                                              select def as Building;
            foreach(Building b in allThings)
            {
                float dist = (b.Position - closestThreat.Position).LengthHorizontal;
                if (dist < closest)
                {
                    closest = dist;
                    tmp = b;
                }
            }
            return tmp;
        }

        public bool MoveToClosestWall()
        {
            if (curvePoints.Count > 1 && destinationCurvePoint > 0)
            {
                origin = curvePoints[destinationCurvePoint];
                destination = curvePoints[destinationCurvePoint - 1];
                ticksToImpact = StartingTicksToImpact;
                curvePoints.Clear();
                updatedPath.Clear();
                targetWall = null;
                return false;
            }
            else
            {
                if (OccupiedWall == null && CasterPawn != null)
                {
                    Building nearestWall = TM_Calc.FindNearestWall(Map, ExactPosition.ToIntVec3(), CasterPawn.Faction);
                    if(nearestWall != null)
                    {
                        Position = nearestWall.Position;
                        origin = nearestWall.DrawPos;
                        destination = nearestWall.DrawPos;
                        ticksToImpact = StartingTicksToImpact;
                        curvePoints.Clear();
                        updatedPath.Clear();
                        targetWall = null;
                        return false;
                    }
                }
            }
            //Log.Message("no nearby wall found - destroying");
            return true;
        }

        private List<Vector3> updatedPath = new List<Vector3>();
        public void CreatePath()
        {
            List<IntVec3> tmpList = new List<IntVec3>();
            if (DestinationWall != null)
            {
                tmpList = TM_Calc.FindTPath(DestinationWall, targetWall, OccupiedWall.Faction);
            }
            updatedPath = TM_Calc.IntVec3List_To_Vector3List(tmpList);            
            targetWall = null;
        }

        private int nextWallUpdate;
        private int nextWallHealthUpdate = 0;
        private int nextThreatUpdate;
        private int nextWallSelectUpdate;
        private bool pathLocked;
        private bool canAddNewPath;

        public void DoPathUpdate()
        {            
            CreatePath();
            pathLocked = false;
        }

        //never gets used, just repairs occupied wall
        private List<Building> damagedWallList = new List<Building>();
        public void DoWallHealthUpdate()
        {
            List<Building> rmvList = new List<Building>();
            rmvList.Clear();
            if(damagedWallList == null)
            {
                damagedWallList = new List<Building>();
                damagedWallList.Clear();
            }
            foreach(Building b in connectedWalls)
            {
                if(b.DestroyedOrNull())
                {
                    rmvList.Add(b);
                }
                else if(b.HitPoints < b.MaxHitPoints)
                {
                    damagedWallList.Add(b);
                }
            }
            foreach(Building b in rmvList)
            {
                connectedWalls.Remove(b);
            }
        }

        private bool wallUpdateLock = false;
        public void DoConnectedWallUpdate()
        {
            //incrementally finds connected walls and queues them for next move;
            //this should occur while the nurikabe moves, to appear seamless
            connectedWalls = TM_Calc.FindConnectedWalls(DestinationWall, 1.4f, 20f, true);
        }

        public void DoThreadedActions()
        {
            if (Find.TickManager.TicksGame > nextWallUpdate && OccupiedWall != null)
            {
                nextWallUpdate = Find.TickManager.TicksGame + Rand.Range(10, 20);
                DoConnectedWallUpdate();
            }

            if (Find.TickManager.TicksGame > nextThreatUpdate)
            {
                if (closestThreat != null)
                {
                    nextThreatUpdate = Find.TickManager.TicksGame + Rand.Range(4, 6);
                }
                else
                {
                    nextThreatUpdate = Find.TickManager.TicksGame + Rand.Range(120, 300);
                }
                FindClosestThreat();
            }
            if (Find.TickManager.TicksGame >= nextWallSelectUpdate && closestThreat != null)
            {
                nextWallSelectUpdate = Find.TickManager.TicksGame + Rand.Range(10, 20);
                FindClosestWallToTarget();
            }
            if (targetWall != null && targetWall.Position != Position && !pathLocked && updatedPath.Count <= 0)
            {
                pathLocked = true;
                DoPathUpdate();
            }
            threadLocked = false;
        }

        private IntVec3 lastGoodPosition = default(IntVec3);
        public void DoDirectActions()
        {
            if (OccupiedWall == null)
            {
                shouldDestroy = MoveToClosestWall();
            }
        }

        public void ChangePath()
        {            
            curvePoints.Clear();
            //Log.Message("adding new path");
            foreach (Vector3 v in updatedPath)
            {
                curvePoints.Add(v);
            }
            updatedPath.Clear();
            destinationCurvePoint = 0;
            pathLocked = false;            
        }

        public void UpdateStatus()
        {
            if (!CasterPawn.DestroyedOrNull() && !CasterPawn.Dead)
            {
                CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
                //int verVal = TM_Calc.GetMagicSkillLevel(CasterPawn, comp.MagicData.MagicPowerSkill_LivingWall, "TM_LivingWall", "_ver", true);
                //int pwrVal = TM_Calc.GetMagicSkillLevel(CasterPawn, comp.MagicData.MagicPowerSkill_LivingWall, "TM_LivingWall", "_pwr", true);
                int verVal = TM_Calc.GetSkillVersatilityLevel(CasterPawn, TorannMagicDefOf.TM_LivingWall, true);
                int pwrVal = TM_Calc.GetSkillPowerLevel(CasterPawn, TorannMagicDefOf.TM_LivingWall, true);
                speed = 15 + (3 * verVal);
                searchEnemySpeed = 200 - (20 * verVal);
                enemyDamage = 12 + pwrVal;
                searchEnemyRange = 2.5f + (.2f * pwrVal);
            }
            else
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        private bool threadLocked;
        private int threadLockTick;
        private int searchEnemyTick;
        private static List<Thread> activeThreads = new List<Thread>();
        protected override void Tick()
        {
            Vector3 exactPosition = ExactPosition;
            if (shouldDestroy)
            {
                Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (ticksToImpact >= 0 && moteDef != null && Find.TickManager.TicksGame % moteFrequency == 0)
                {
                    DrawEffects(exactPosition);
                }
                if (Find.TickManager.TicksGame > searchEnemyTick)
                {
                    searchEnemyTick = Mathf.RoundToInt(Rand.Range(.4f, .6f) * searchEnemySpeed) + Find.TickManager.TicksGame;
                    AttackNearby();
                    RepairOccupiedWall();
                    if(pawn.DestroyedOrNull() || pawn.Dead || pawn.Map != Map)
                    {
                        shouldDestroy = true;
                    }
                }
                if (idleFor > 0)
                {
                    idleFor--;
                    if (closestThreat != null && !threadLocked)
                    {
                        threadLocked = true;
                        threadLockTick = Find.TickManager.TicksGame + 100;
                        Thread tryDirectPath = new Thread(DirectPath);
                        tryDirectPath.Start();
                    }
                }
                else
                {
                    ticksToImpact--;
                    //updates list of connected wall segments
                    if (!threadLocked)
                    {
                        threadLockTick = Find.TickManager.TicksGame + 100;
                        threadLocked = true;
                        Thread threadedActions = new Thread(DoThreadedActions);
                        activeThreads.Add(threadedActions);
                        threadedActions.Start();
                    }
                    if (Find.TickManager.TicksGame > threadLockTick)
                    {
                        for (int i = 0; i < activeThreads.Count; i++)
                        {
                            activeThreads[i].Abort();
                        }
                        activeThreads.Clear();
                        threadLocked = false;
                    }
                    DoDirectActions();
                    if (!shouldDestroy)
                    {
                        bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
                        if (flag)
                        {
                            ticksToImpact++;
                            Position = ExactPosition.ToIntVec3();
                            Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            Position = ExactPosition.ToIntVec3();
                            if (Find.TickManager.TicksGame % 3 == 0 && destination.ToIntVec3() != Position)
                            {
                                FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.4f, .6f));
                                float angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(origin, destination)).ToAngleFlat();
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_DirectionalDirtOverhead, DrawPos, Map, 1.2f, .05f, .15f, .38f, 0, 3f, angle, angle);
                            }

                            bool flag2 = ticksToImpact <= 0;
                            if (flag2)
                            {
                                if (curveVariance > 0)
                                {
                                    if ((curvePoints.Count() - 1) > destinationCurvePoint)
                                    {
                                        origin = curvePoints[destinationCurvePoint];
                                        destinationCurvePoint++;
                                        destination = curvePoints[destinationCurvePoint];
                                        ticksToImpact = StartingTicksToImpact;
                                        canAddNewPath = false;
                                    }
                                    else
                                    {
                                        bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                                        if (flag3)
                                        {
                                            Position = DestinationCell;
                                        }
                                        canAddNewPath = true;
                                        IdleFlight();
                                    }
                                }
                                else
                                {
                                    bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                                    if (flag3)
                                    {
                                        Position = DestinationCell;
                                    }
                                    ImpactSomething();
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void IdleFlight()
        {
            if (updatedPath != null && updatedPath.Count > 0 && !pathLocked)
            {
                idleFor = 5;
                pathLocked = true;
                ChangePath();
            }
            else
            {
                idleFor = 60;
                origin = ExactPosition;
                destination = ExactPosition;
                ticksToImpact = StartingTicksToImpact;
            }
        }

        public void DirectPath()
        {
            Thing cwftWall = FindClosestWallFromTarget();
            if (cwftWall != null && (cwftWall.Position - closestThreat.Position).LengthHorizontal < (Position - closestThreat.Position).LengthHorizontal)
            {
                List<IntVec3> tmpList = TM_Calc.FindTPath(OccupiedWall, cwftWall, OccupiedWall.Faction);
                if (tmpList.Count > 0)
                {
                    updatedPath = TM_Calc.IntVec3List_To_Vector3List(tmpList);
                    ChangePath();
                }
            }
            
            targetWall = null;
            idleFor = 0;
            threadLocked = false;
        }

        public void AttackNearby()
        {
            List<Pawn> hitList = new List<Pawn>();
            hitList.Clear();
            List<Pawn> atkPawns = Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => (x.Position - Position).LengthHorizontal <= searchEnemyRange).ToList();
            foreach(Pawn p in atkPawns)
            {
                if(p.HostileTo(CasterPawn) && !p.Dead && !p.Downed)
                {                    
                    float angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(DrawPos, p.DrawPos)).ToAngleFlat();
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_WallSpike, TM_Calc.GetVectorBetween(DrawPos, p.DrawPos), Map, Rand.Range(1f, 1.2f), Rand.Range(.3f, .4f), 0f, Rand.Range(.1f, .2f), 0, Rand.Range(.1f, .3f), angle, angle); 
                    for(int i = 0; i < 5; i++)
                    {
                        TM_MoteMaker.ThrowGenericFleck(FleckDefOf.DustPuff, DrawPos, Map, Rand.Range(.8f, 1.2f), .3f, .1f, .2f, Rand.Range(-200, 200), Rand.Range(1f, 3f), angle + Rand.Range(-15, 15), Rand.Range(0, 360));
                    }
                    FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.4f, .6f));
                    TM_Action.DamageEntities(p, null, enemyDamage, 0f, DamageDefOf.Stab, CasterPawn);
                }
            }
        }

        public void RepairOccupiedWall()
        {
            if(OccupiedWall != null && OccupiedWall.HitPoints < OccupiedWall.MaxHitPoints)
            {
                OccupiedWall.HitPoints = Mathf.Clamp(OccupiedWall.HitPoints + 10, 0, OccupiedWall.MaxHitPoints);
                FleckMaker.ThrowDustPuff(OccupiedWall.Position, Map, Rand.Range(.6f, 1f));
            }
        }


        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {
                bool flag2 = flyingThing is Pawn;
                if (flag2)
                {
                    Vector3 arg_2B_0 = DrawPos;
                    bool flag4 = !DrawPos.ToIntVec3().IsValid;
                    if (flag4)
                    {
                        return;
                    }
                    Pawn pawn = flyingThing as Pawn;
                    pawn.Drawer.renderer.RenderPawnAt(DrawPos);                      
                }
                else
                {
                    Vector3 drawP = DrawPos;
                    drawP.y = 8.1f;
                    Graphics.DrawMesh(MeshPool.plane10, drawP, Quaternion.identity, flyingThing.def.DrawMatSingle, 0);
                }
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, Quaternion.identity, flyingThing.def.DrawMatSingle, 0);
            }
            Comps_PostDraw();
        }

        private void DrawEffects(Vector3 effectVec)
        {
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);
            TM_MoteMaker.ThrowGenericMote(moteDef, effectVec, Map, Rand.Range(.4f, .6f), Rand.Range(.05f, .1f), .03f, Rand.Range(.2f, .3f), Rand.Range(-200, 200), Rand.Range(.5f, 2f), Rand.Range(0, 360), Rand.Range(0, 360));
        }

        private void ImpactSomething()
        {
            bool flag = assignedTarget != null;
            if (flag)
            {
                Pawn pawn = assignedTarget as Pawn;
                bool flag2 = pawn != null && pawn.GetPosture() != PawnPosture.Standing && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f;
                if (flag2)
                {
                    Impact(null);
                }
                else
                {
                    Impact(assignedTarget);
                }
            }
            else
            {
                Impact(null);
            }
        }

        protected new void Impact(Thing hitThing)
        {           
            Destroy(DestroyMode.Vanish);            
        }
    }
}
