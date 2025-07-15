using RimWorld;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_Advanced : Projectile
    {
        protected new Vector3 origin;        
        protected new Vector3 destination;
        protected Vector3 trueOrigin;
        protected Vector3 trueDestination;

        public float speed = 30f;
        public Vector3 travelVector = default(Vector3);
        protected new int ticksToImpact;
        //protected new Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;

        public ThingDef moteDef;
        public int moteFrequency;
        public float moteSize = 1f;

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

        private bool earlyImpact;
        private float impactForce;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int weaponDmg = 0;
        private int doublesidedVariance;

        private Pawn pawn;

        //Magic related
        private CompAbilityUserMagic comp;
        private TMPawnSummoned newPawn = new TMPawnSummoned();

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
            //Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
        }

        public virtual void PreInitialize()
        {

        }

        private void Initialize()
        {
            PreInitialize();
            if (pawn != null)
            {
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            else
            {
                flyingThing.ThingID += Rand.Range(0, 214).ToString();
            }            
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void AdvancedLaunch(Thing launcher, ThingDef effectMote, int moteFrequencyTicks, float curveAmount, bool shouldSpin, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, int flyingSpeed, bool isExplosion, int _impactDamage, float _impactRadius, DamageDef damageType, DamageInfo? newDamageInfo = null, int doubleVariance = 0, bool flyOverhead = false, float moteEffectSize = 1f)
        {
            fliesOverhead = flyOverhead;
            explosionDamage = _impactDamage;
            isExplosive = isExplosion;
            impactRadius = _impactRadius;
            impactDamageType = damageType;
            moteFrequency = moteFrequencyTicks;
            moteDef = effectMote;
            moteSize = moteEffectSize;
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
            travelVector = initialVector;
            float initialAngle = (initialVector).ToAngleFlat(); //Quaternion.AngleAxis(90, Vector3.up) *
            float curveAngle = variance;
            if(doublesidedVariance == 0)
            {
                if (Rand.Chance(.5f))
                {
                    curveAngle = (-1) * variance;
                }
            }
            else
            {
                curveAngle = (doublesidedVariance * variance);
            }

            //calculate extra distance bolt travels around the ellipse
            float a = .47f * Vector3.Distance(start, end);
            float b = a * Mathf.Sin(.5f * Mathf.Deg2Rad * variance);
            float p = .5f * Mathf.PI * (3 * (a + b) - (Mathf.Sqrt((3 * a + b) * (a + 3 * b))));
                    
            float incrementalDistance = p / variancePoints; 
            float incrementalAngle = (curveAngle / (float)variancePoints) * 2f;
            curvePoints.Add(trueOrigin);
            for(int i = 1; i <= (variancePoints + 1); i++)
            {
                curvePoints.Add(curvePoints[i - 1] + ((Quaternion.AngleAxis(curveAngle, Vector3.up) * initialVector) * incrementalDistance)); //(Quaternion.AngleAxis(curveAngle, Vector3.up) *
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

        public virtual void PreTick()
        {

        }

        protected override void Tick()
        {
            PreTick();
            Vector3 exactPosition = ExactPosition;
            if (ticksToImpact >= 0 && moteDef != null && Find.TickManager.TicksGame % moteFrequency == 0)
            {
                DrawEffects(exactPosition);
            }
            ticksToImpact--;
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            if (flag)
            {
                ticksToImpact++;
                Position = ExactPosition.ToIntVec3();
                Destroy(DestroyMode.Vanish);
            }
            else if(!ExactPosition.ToIntVec3().Walkable(Map) && !fliesOverhead)
            {
                earlyImpact = true;
                impactForce = (DestinationCell - ExactPosition.ToIntVec3()).LengthHorizontal + (speed * .2f);
                ImpactSomething();
            }
            else
            {
                Position = ExactPosition.ToIntVec3();
                if(moteDef == null && Find.TickManager.TicksGame % 3 == 0)
                {
                    FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.6f, .8f));
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
            PostTick();
        }

        public virtual void PostTick()
        {

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
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetTRS(DrawPos, ExactRotation, new Vector3(Graphic.drawSize.x, 13f, Graphic.drawSize.y));
                    Graphics.DrawMesh(MeshPool.plane10,matrix, flyingThing.def.DrawMatSingle, 0);
                }
            }
            //else
            //{
            //    Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.DrawMatSingle, 0);
            //}
            Comps_PostDraw();
        }

        public virtual void DrawEffects(Vector3 effectVec)
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
                hitThing.TakeDamage(impactDamage.Value);
            }
            ImpactOverride();
            if (flyingThing is Pawn p)
            {
                try
                {
                    SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);

                    GenSpawn.Spawn(flyingThing, Position, Map);                   
                    if (earlyImpact)
                    {
                        damageEntities(p, impactForce, DamageDefOf.Blunt);
                        damageEntities(p, 2 * impactForce, DamageDefOf.Stun);
                    }
                    Destroy(DestroyMode.Vanish);
                }
                catch
                {
                    GenSpawn.Spawn(flyingThing, Position, Map);
                    Destroy(DestroyMode.Vanish);
                }
            }
            else
            {
                if(impactRadius > 0)
                {
                    if(isExplosive)
                    {
                        ExplosionHelper.Explode(ExactPosition.ToIntVec3(), Map, impactRadius, impactDamageType, launcher as Pawn, explosionDamage, -1, impactDamageType.soundExplosion, def, null, null, null, 0f, 1, null, false, null, 0f, 0, 0.0f, true);
                    }
                    else
                    {
                        int num = GenRadial.NumCellsInRadius(impactRadius);
                        IntVec3 curCell;
                        for (int i = 0; i < num; i++)
                        {
                            curCell = ExactPosition.ToIntVec3() + GenRadial.RadialPattern[i];
                            List<Thing> hitList = new List<Thing>();
                            hitList = curCell.GetThingList(Map);
                            for (int j = 0; j < hitList.Count; j++)
                            {
                                if (hitList[j] is Pawn && hitList[j] != pawn)
                                {
                                    damageEntities(hitList[j], explosionDamage, impactDamageType);
                                }
                            }
                        }
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
        }

        public virtual void ImpactOverride()
        {

        }

        public void damageEntities(Thing e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }
}
