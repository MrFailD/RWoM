using RimWorld;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_Leap : Projectile
    {
        protected new Vector3 origin;

        protected new Vector3 destination;

        protected float speed = 28f;
        private bool drafted = false;

        protected new int ticksToImpact;

        protected Thing assignedTarget;

        protected Thing flyingThing;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        public int weaponDmg = 0;

        private bool initialize = true;

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
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                //MoteMaker.MakeStaticMote(pawn.TrueCenter(), pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
                //SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            //flyingThing.ThingID += Rand.Range(0, 2147).ToString();
            initialize = false;
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
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
            Initialize();
        }        

        protected override void Tick()
        {
            //base.Tick();
            Vector3 exactPosition = ExactPosition;
            ticksToImpact--;
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
                if(Find.TickManager.TicksGame % 2 == 0)
                {
                    FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.6f, .8f));
                }               
                
                bool flag2 = ticksToImpact <= 0;
                if (flag2)
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
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
            }
            Comps_PostDraw();
        }

        private void DrawEffects(Vector3 pawnVec, Pawn flyingPawn, int magnitude)
        {
            bool flag = !pawn.Dead && !pawn.Downed;
            if (flag)
            {

            }
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
            try
            {
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);                

                //GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
                GenPlace.TryPlaceThing(flyingThing, Position, Map, ThingPlaceMode.Near);
                Pawn p = flyingThing as Pawn;
                Destroy(DestroyMode.Vanish);
            }
            catch
            {
                Destroy(DestroyMode.Vanish);
            }
        }        
    }
}
