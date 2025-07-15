using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using AbilityUser;
using TorannMagic.Weapon;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_PsionicStorm : Projectile
    {
        private int verVal;

        private Verb verb;

        protected new Vector3 origin;
        protected new Vector3 destination;
        protected Vector3 trueOrigin;
        protected Vector3 targetCenter;
        private Vector3 nearApex;
        private Vector3 farApex;
        private Vector3 direction;
        private List<IntVec3> targetCells;

        public float curveVariance = 0; // 0 = no curve
        private List<Vector3> curvePoints = new List<Vector3>();
        private int destinationCurvePoint;
        private int stage;
        private float curveAngle;

        protected float speed = 40f;

        protected new int ticksToImpact = 60;
        private int nextAttackTick;

        protected Thing assignedTarget;
        protected Thing flyingThing;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int timesToDamage = 3;
        public int weaponDmg;

        private Pawn pawn;

        //local variables
        private float targetCellRadius = 4;
        private float circleFlightSpeed = 10;
        private float circleRadius = 10;
        private int attackFrequencyLow = 10;
        private int attackFrequencyHigh = 40;

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
            Scribe_Values.Look<Vector3>(ref trueOrigin, "trueOrigin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref nearApex, "nearApex", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref farApex, "farApex", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref direction, "direction", default(Vector3), false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref weaponDmg, "weaponDmg", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref destinationCurvePoint, "destinationCurvePoint", 0, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
            Scribe_Collections.Look<IntVec3>(ref targetCells, "targetCells", LookMode.Value);
            Scribe_Collections.Look<Vector3>(ref curvePoints, "curvePoints", LookMode.Value);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 12f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
                curvePoints = new List<Vector3>();
                curvePoints.Clear();
                targetCells = new List<IntVec3>();
                targetCells.Clear();
                targetCells = GenRadial.RadialCellsAround(targetCenter.ToIntVec3(), 4, true).ToList();
                verVal = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicStorm_ver").level;
            }
            //flyingThing.ThingID += Rand.Range(0, 2147).ToString();
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, Verb verb)
        {
            this.verb = verb;
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            //
            ModOptions.Constants.SetPawnInFlight(true);
            //
            this.origin = origin;
            trueOrigin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            targetCenter = targ.Cell.ToVector3Shifted();
            direction = GetVector(trueOrigin, targetCenter);
            nearApex = targetCenter + ((-circleRadius) * direction);
            farApex = targetCenter + (circleRadius * direction);
            destination = nearApex; //set initial destination to be outside of storm circle
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        public void CalculateCurvePoints(Vector3 start, Vector3 end, float variance)
        {
            destinationCurvePoint = 0;
            curvePoints.Clear();
            int variancePoints = 20;
            Vector3 initialVector = GetVector(start, end);
            initialVector.y = 0;
            float initialAngle = (initialVector).ToAngleFlat(); //Quaternion.AngleAxis(90, Vector3.up) *
            if (curveAngle == 0)
            {
                if (Rand.Chance(.5f))
                {
                    curveAngle = variance;
                }
                else
                {
                    variance = (-1) * variance;
                    curveAngle = variance;
                }
            }
            else
            {
                variance = curveAngle;
            }
            //calculate extra distance bolt travels around the ellipse
            float a = .5f * Vector3.Distance(start, end);
            float b = a * Mathf.Sin(.5f * Mathf.Deg2Rad * Mathf.Abs(curveAngle));
            float p = .5f * Mathf.PI * (3 * (a + b) - (Mathf.Sqrt((3 * a + b) * (a + 3 * b))));

            float incrementalDistance = p / variancePoints;
            float incrementalAngle = (variance / variancePoints) * 2;
            curvePoints.Add(start);
            for (int i = 1; i < variancePoints; i++)
            {
                curvePoints.Add(curvePoints[i - 1] + ((Quaternion.AngleAxis(variance, Vector3.up) * initialVector) * incrementalDistance)); //(Quaternion.AngleAxis(curveAngle, Vector3.up) *
                variance -= incrementalAngle;
            }
        }

        protected override void Tick()
        {
            //base.Tick();
            ticksToImpact--;
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map) || Position.DistanceToEdge(Map) <= 1;
            if (stage > 0 && stage < 4 && nextAttackTick < Find.TickManager.TicksGame)
            {
                IntVec3 targetVariation = targetCells.RandomElement();
                float angle = (Quaternion.AngleAxis(90, Vector3.up) * GetVector(ExactPosition, targetVariation.ToVector3Shifted())).ToAngleFlat();
                Vector3 drawPos = ExactPosition + (GetVector(ExactPosition, targetVariation.ToVector3Shifted()) * .5f);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiBlastStart, drawPos, Map, Rand.Range(.4f, .6f), Rand.Range(.0f, .05f), 0f, .1f, 0, 0, 0, angle); //throw psi blast start
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiBlastEnd, drawPos, Map, Rand.Range(.4f, .8f), Rand.Range(.0f, .1f), .2f, .3f, 0, Rand.Range(1f, 1.5f), angle, angle); //throw psi blast end 
                TryLaunchProjectile(ThingDef.Named("TM_Projectile_PsionicBlast"), targetVariation);
                nextAttackTick = Find.TickManager.TicksGame + Mathf.RoundToInt(Rand.Range(attackFrequencyLow, attackFrequencyHigh) * (1 - .1f * verVal));
            }
            if (flag)
            {
                ticksToImpact++;
                Position = ExactPosition.ToIntVec3();
                GenPlace.TryPlaceThing(flyingThing, Position, Map, ThingPlaceMode.Near);
                //GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
                ModOptions.Constants.SetPawnInFlight(false);
                Pawn p = flyingThing as Pawn;
                if (p.IsColonist)
                {
                    p.drafter.Drafted = true;
                    if (ModOptions.Settings.Instance.cameraSnap)
                    {
                        CameraJumper.TryJumpAndSelect(p);
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
            else
            {
                Position = ExactPosition.ToIntVec3();
                FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.8f, 1.2f));
                bool flag2 = ticksToImpact <= 0;
                if (flag2)
                {
                    if (stage == 0)
                    {
                        CalculateCurvePoints(nearApex, farApex, 90);
                        origin = curvePoints[destinationCurvePoint];
                        destinationCurvePoint++;
                        destination = curvePoints[destinationCurvePoint];
                        speed = circleFlightSpeed;
                        ticksToImpact = StartingTicksToImpact;
                        nextAttackTick = Find.TickManager.TicksGame + Mathf.RoundToInt(Rand.Range(attackFrequencyLow, attackFrequencyHigh) * (1 - .1f * verVal));
                        stage = 1;                        
                    }
                    else if(stage == 1)
                    {
                        if ((curvePoints.Count() - 1) > destinationCurvePoint)
                        {
                            origin = curvePoints[destinationCurvePoint];
                            destinationCurvePoint++;
                            destination = curvePoints[destinationCurvePoint];
                            ticksToImpact = StartingTicksToImpact;
                        }
                        else
                        {
                            origin = curvePoints[destinationCurvePoint];
                            CalculateCurvePoints(origin, nearApex, 90);
                            destinationCurvePoint++;
                            destination = curvePoints[destinationCurvePoint];
                            ticksToImpact = StartingTicksToImpact;
                            stage = 2;
                        }
                    }
                    else if(stage == 2)
                    {
                        if ((curvePoints.Count() - 1) > destinationCurvePoint)
                        {
                            origin = curvePoints[destinationCurvePoint];
                            destinationCurvePoint++;
                            destination = curvePoints[destinationCurvePoint];
                            ticksToImpact = StartingTicksToImpact;
                        }
                        else
                        {
                            origin = curvePoints[destinationCurvePoint];
                            destination = nearApex;
                            ticksToImpact = StartingTicksToImpact;
                            //this.speed = 15;
                            stage = 3;
                        }
                    }
                    else if (stage == 3)
                    {
                        speed = 25f;
                        origin = nearApex;
                        destination = trueOrigin;
                        ticksToImpact = StartingTicksToImpact;
                        stage = 4;                        
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {
                float angleToCenter = GetVector(ExactPosition, targetCenter).ToAngleFlat();
                if (angleToCenter > -45 && angleToCenter < 45)
                {
                    flyingThing.Rotation = Rot4.East;
                }
                else if (angleToCenter > 45 && angleToCenter < 135)
                {
                    flyingThing.Rotation = Rot4.South;
                }
                else if (angleToCenter > 135 || angleToCenter < -135)
                {
                    flyingThing.Rotation = Rot4.West;
                }
                else
                {
                    flyingThing.Rotation = Rot4.North;
                }

                bool flag2 = flyingThing is Pawn;
                if (flag2)
                {
                    Vector3 arg_2B_0 = DrawPos;
                    bool flag3 = false;
                    if (flag3)
                    {
                        return;
                    }
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
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
                Comps_PostDraw();
            }
        }

        private void TryLaunchProjectile(ThingDef projectileDef, LocalTargetInfo launchTarget)
        {
                Vector3 drawPos = ExactPosition; 
                Projectile_AbilityBase projectile_AbilityBase = (Projectile_AbilityBase)GenSpawn.Spawn(projectileDef, ExactPosition.ToIntVec3(), Map);
                //ShotReport shotReport = ShotReport.HitReportFor(this.pawn, this.verb, launchTarget);
                SoundDef expr_C8 = TorannMagicDefOf.TM_AirWoosh;
                if (expr_C8 != null)
                {
                    SoundStarter.PlayOneShot(expr_C8, new TargetInfo(ExactPosition.ToIntVec3(), Map, false));
                }
                projectile_AbilityBase.Launch(pawn, TorannMagicDefOf.TM_PsionicBlast, drawPos, launchTarget, ProjectileHitFlags.All, false, null, null, null, null);
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
            bool flag = hitThing == null;
            if (flag)
            {
                Pawn pawn;
                bool flag2 = (pawn = (Position.GetThingList(Map).FirstOrDefault((Thing x) => x == assignedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = pawn;
                }
            }
            bool hasValue = impactDamage.HasValue;
            if (hasValue)
            {
                for (int i = 0; i < timesToDamage; i++)
                {
                    bool flag3 = damageLaunched;
                    if (flag3)
                    {
                        flyingThing.TakeDamage(impactDamage.Value);
                    }
                    else
                    {
                        hitThing.TakeDamage(impactDamage.Value);
                    }
                }
                bool flag4 = explosion;
                if (flag4)
                {
                    ExplosionHelper.Explode(Position, Map, 0.9f, DamageDefOf.Stun, this, -1, 0, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                }
            }
            GenSpawn.Spawn(flyingThing, Position, Map);
            ModOptions.Constants.SetPawnInFlight(false);
            Pawn p = flyingThing as Pawn;
            if(p.IsColonist)
            {
                p.drafter.Drafted = true;
                if (ModOptions.Settings.Instance.cameraSnap)
                {
                    CameraJumper.TryJumpAndSelect(p);
                }
            }
            Destroy(DestroyMode.Vanish);
        }

        public Vector3 GetVector(Vector3 center, Vector3 objectPos)
        {
            Vector3 heading = (objectPos - center);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }
    }
}
