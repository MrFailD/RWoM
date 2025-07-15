using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_TimeDelay : Projectile
    {
        protected new Vector3 origin;
        protected new Vector3 destination;
        private Vector3 direction;
        private Vector3 variationDestination;
        private Vector3 drawPosition;

        public float speed = 10f;
        public int spinRate;        //spin rate > 0 makes the object rotate every spinRate Ticks
        public float xVariation;    //x variation makes the object move side to side by +- variation
        public float zVariation;    //z variation makes the object move up and down by +- variation
        private int rotation;
        protected new int ticksToImpact;
        //protected new Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        private bool drafted;
        public float destroyPctAtEnd;

        public int moteFrequency;
        public ThingDef moteDef;
        public float fadeInTime = .25f;
        public float fadeOutTime = .25f;
        public float solidTime = .5f;
        public float moteScale = 1f;

        public float force = 1f;
        public int duration = 600;

        private bool earlyImpact = false;
        private float impactForce = 0;
        private int variationShiftTick = 100;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        public int weaponDmg = 0;

        private Pawn pawn;
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
                //Vector3 b = (this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact);
                //return this.origin + b + Vector3.up * this.def.Altitude;
                return origin + Vector3.up * def.Altitude;
            }
        }

        public new Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(origin - destination);
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
            Scribe_Values.Look<bool>(ref drafted, "drafted", false, false);
            Scribe_Values.Look<float>(ref xVariation, "xVariation", 0, false);
            Scribe_Values.Look<float>(ref zVariation, "zVariation", 0, false);
            Scribe_Values.Look<float>(ref solidTime, "solidTime", .5f, false);
            Scribe_Values.Look<float>(ref fadeInTime, "fadeInTime", .25f, false);
            Scribe_Values.Look<float>(ref fadeOutTime, "fadeOutTime", .25f, false);
            Scribe_Defs.Look<ThingDef>(ref moteDef, "moteDef");
            Scribe_Values.Look<float>(ref moteScale, "moteScale", 1f, false);
            Scribe_Values.Look<int>(ref moteFrequency, "moteFrequency", 0, false);
            Scribe_Values.Look<float>(ref destroyPctAtEnd, "destroyPctAtEnd", 0f, false);
            Scribe_Values.Look<int>(ref duration, "duration", 600, false);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }

            direction = TM_Calc.GetVector(origin.ToIntVec3(), destination.ToIntVec3());
            //flyingThing.ThingID += Rand.Range(0, 2147).ToString();
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, int _spinRate)
        {
            spinRate = _spinRate;
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void LaunchVaryPosition(Thing launcher, LocalTargetInfo targ, Thing flyingThing, int _spinRate, float _xVariation, float _zVariation, ThingDef mote = null, int moteFreq = 0, float destroy = 0f)
        {
            destroyPctAtEnd = destroy;
            moteDef = mote;
            moteFrequency = moteFreq; 
            xVariation = _xVariation;
            zVariation = _zVariation;
            spinRate = _spinRate;
            Launch(launcher, flyingThing.DrawPos, flyingThing.Position, flyingThing, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {            
            bool spawned = flyingThing.Spawned;            
            pawn = launcher as Pawn;
            if (pawn != null && pawn.Drafted)
            {
                drafted = true;
            }
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            speed = speed * force;
            this.launcher = launcher;
            this.origin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;

            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            destination = targ.Cell.ToVector3Shifted();
            ticksToImpact = StartingTicksToImpact;
            variationDestination = Position.ToVector3Shifted(); //this.DrawPos //not initialized?
            drawPosition = Position.ToVector3Shifted(); //this.DrawPos; 
            Initialize();
        }        

        protected override void Tick()
        {
            duration--;
            Position = origin.ToIntVec3();
            bool flag2 = duration <= 0;
            if(moteDef != null && Map != null && Find.TickManager.TicksGame % moteFrequency == 0)
            {
                TM_MoteMaker.ThrowGenericMote(moteDef, ExactPosition, Map, Rand.Range(moteScale * .75f, moteScale * 1.25f), solidTime, fadeInTime, fadeOutTime, Rand.Range(200, 400), 0, 0, Rand.Range(0, 360));
            }
            if (flag2)
            {
                ImpactSomething();
            }

        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {
                if (spinRate > 0)
                {
                    if(Find.TickManager.TicksGame % spinRate ==0)
                    {
                        rotation++;
                        if(rotation >= 4)
                        {
                            rotation = 0;
                        }
                    }
                    if (rotation == 0)
                    {
                        flyingThing.Rotation = Rot4.West;
                    }
                    else if (rotation == 1)
                    {
                        flyingThing.Rotation = Rot4.North;
                    }
                    else if (rotation == 2)
                    {
                        flyingThing.Rotation = Rot4.East;
                    }
                    else
                    {
                        flyingThing.Rotation = Rot4.South;
                    }
                }

                bool flag2 = flyingThing is Pawn;
                if (flag2 && zVariation == 0 && xVariation == 0)
                {
                    Vector3 arg_2B_0 = DrawPos;
                    bool flag4 = !DrawPos.ToIntVec3().IsValid;
                    if (flag4)
                    {
                        return;
                    }
                    Pawn pawn = flyingThing as Pawn;
                    pawn.Drawer.renderer.RenderPawnAt(DrawPos);
                    Material bubble = TM_MatPool.TimeBubble;
                    Vector3 vec3 = DrawPos;
                    vec3.y++;
                    Vector3 s = new Vector3(2f, 1f, 2f);
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(vec3, Quaternion.AngleAxis(0, Vector3.up), s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, bubble, 0, null);
                    //Graphics.DrawMesh(MeshPool.plane10, vec3, this.ExactRotation, bubble, 0);
                }
                else if(zVariation != 0 || xVariation != 0)
                {
                    drawPosition = VariationPosition(drawPosition);
                    //bool flag4 = !this.DrawPos.ToIntVec3().IsValid;
                    //if (flag4)
                    //{
                    //    return;
                    //}
                    //Pawn pawn = this.flyingThing as Pawn;
                    //pawn.Drawer.DrawAt(this.DrawPos);
                    //this.flyingThing.DrawAt(this.drawPosition);
                    flyingThing.DrawNowAt(drawPosition);
                    //this.flyingThing.DrawAt(drawPosition);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
            }
            Comps_PostDraw();
        }

        private Vector3 VariationPosition(Vector3 currentDrawPos)
        {
            Vector3 startPos = currentDrawPos;
            startPos.y = 10f;
            float variance = (xVariation / 100f);
            if ((startPos.x - variationDestination.x) < -variance)
            {
                startPos.x += variance;
            }
            else if((startPos.x - variationDestination.x) > variance)
            {
                startPos.x += -variance;
            }
            else if (xVariation != 0)
            {
                variationDestination.x = DrawPos.x + Rand.Range(-xVariation, xVariation);
            }
            variance = (zVariation / 100f);
            if ((startPos.z - variationDestination.z) < -variance)
            {
                startPos.z += variance;
            }
            else if ((startPos.z - variationDestination.z) > variance)
            {
                startPos.z += -variance;
            }
            else if (zVariation != 0)
            {
                variationDestination.z = DrawPos.z + Rand.Range(-zVariation, zVariation);
            }

            return startPos;
        }


        private void ImpactSomething()
        {
            Impact(null);            
        }

        protected new void Impact(Thing hitThing)
        {
            if (Map != null)
            {
                GenPlace.TryPlaceThing(flyingThing, Position, Map, ThingPlaceMode.Direct);
                if (flyingThing is Pawn p)
                {
                    if (p.IsColonist && drafted && p.drafter != null)
                    {
                        p.drafter.Drafted = true;
                    }
                }

                if (destroyPctAtEnd != 0)
                {
                    int rangeMax = 10;
                    for (int i = 0; i < rangeMax; i++)
                    {
                        float direction = Rand.Range(0, 360);
                        Vector3 rndPos = flyingThing.DrawPos;
                        rndPos.x += Rand.Range(-.3f, .3f);
                        rndPos.z += Rand.Range(-.3f, .3f);
                        ThingDef mote = TorannMagicDefOf.Mote_Shadow;
                        TM_MoteMaker.ThrowGenericMote(mote, rndPos, Map, Rand.Range(.5f, 1f), 0.4f, Rand.Range(.1f, .4f), Rand.Range(1.2f, 2f), Rand.Range(-200, 200), Rand.Range(1.2f, 2f), direction, direction);

                    }
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                    info.pitchFactor = .8f;
                    info.volumeFactor = 1.2f;
                    TorannMagicDefOf.TM_Vibration.PlayOneShot(info);
                }

                if (destroyPctAtEnd >= 1f)
                {
                    flyingThing.Destroy(DestroyMode.Vanish);
                }
                else if (destroyPctAtEnd != 0)
                {
                    flyingThing.SplitOff(Mathf.RoundToInt(flyingThing.stackCount * destroyPctAtEnd)).Destroy(DestroyMode.Vanish);
                }
            }

            Destroy(DestroyMode.Vanish);
        }
    }
}
