using RimWorld;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_DirtDevil : Projectile
    {

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int searchDelay = 10;
        private int age = -1;
        private int duration = 12000;

        protected float speed = 9f;
        protected new int ticksToImpact;

        //protected new Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        private Pawn pawn;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        public int timesToDamage = 3;

        public int weaponDmg = 0;

        private bool initialized = true;

        protected int StartingTicksToImpact
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

        protected IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(destination);
            }
        }

        public override Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (destination - origin) * (1f - (float)ticksToImpact / (float)StartingTicksToImpact);
                return origin + b + Vector3.up * def.Altitude;
            }
        }

        public override Quaternion ExactRotation
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
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 12000, false);
            Scribe_Values.Look<Vector3>(ref origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref timesToDamage, "timesToDamage", 0, false);
            Scribe_Values.Look<int>(ref searchDelay, "searchDelay", 10, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            //Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 1f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            //flyingThing.ThingID += Rand.Range(0, 214).ToString();
            initialized = false;
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
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            if (comp != null)
            {
                if (comp.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_pwr").level >= 3)
                {
                    speed = 12;
                }
                if (comp.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_ver").level >= 4)
                {
                    duration = Mathf.RoundToInt(duration * 1.25f);
                }
            }
            duration = Mathf.RoundToInt(duration * comp.arcaneDmg);
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            this.origin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            CleanFilth();
            destination = FindNearestFilth(this.launcher.DrawPos);
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        public Vector3 FindNearestFilth(Vector3 origin)
        {
            Vector3 destination = default(Vector3);
            List<Thing> filthList = Map.listerFilthInHomeArea.FilthInHomeArea;
            Thing closestDirt = null;
            float dirtPos = 0;
            for (int i = 0; i < filthList.Count; i++)
            {
                if (closestDirt != null)
                {
                    float dirtDistance = (filthList[i].DrawPos - origin).magnitude;
                    if (dirtDistance < dirtPos)
                    {
                        dirtPos = dirtDistance;
                        closestDirt = filthList[i];
                    }
                }
                else
                {
                    closestDirt = filthList[i];
                    dirtPos = (filthList[i].DrawPos - origin).magnitude;
                }
            }

            if (closestDirt != null)
            {
                destination = closestDirt.DrawPos;
            }
            else
            {
                age = duration;
                destination = this.destination;
            }
            return destination;
        }

        public void CleanFilth()
        {
            List<Thing> allThings = new List<Thing>();
            List<Thing> allFilth = new List<Thing>();
            allThings.Clear();
            allFilth.Clear();
            List<IntVec3> cellsAround = GenRadial.RadialCellsAround(Position, 1.4f, true).ToList();
            if (cellsAround != null)
            {
                for (int i = 0; i < cellsAround.Count; i++)
                {
                    allThings = cellsAround[i].GetThingList(Map);                   
                    for (int j = 0; j < allThings.Count; j++)
                    {
                        if (allThings[j].def.category == ThingCategory.Filth || allThings[j].def.IsFilth)
                        {
                            allFilth.Add(allThings[j]);
                        }
                    }
                }
                for (int i = 0; i < allFilth.Count; i++)
                {
                    CleanGraphics(allFilth[i]);
                    allFilth[i].Destroy(DestroyMode.Vanish);
                }
            }
        }

        public void CleanGraphics(Thing filth)
        {
            TM_MoteMaker.ThrowGenericFleck(FleckDefOf.MicroSparks, ExactPosition, Map, Rand.Range(.3f, .5f), .6f, .2f, .4f, Rand.Range(-400, -100), .3f, Rand.Range(0,360), Rand.Range(0, 360));
            Vector3 angle = TM_Calc.GetVector(filth.DrawPos, ExactPosition);
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ThickDust, filth.DrawPos, Map, Rand.Range(.4f, .6f), .1f, .05f, .25f, -200, 2, (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(), Rand.Range(0,360));
        }

        protected override void Tick()
        {
            //base.Tick();

            age++;
            searchDelay--;
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
                bool flag2 = ticksToImpact <= 0;
                if (flag2)
                {
                    if (age < duration)
                    {
                        origin = ExactPosition;
                        destination = FindNearestFilth(origin);
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
                if (Find.TickManager.TicksGame % 4 == 0)
                {
                    Vector3 rndVec = ExactPosition;
                    rndVec.x += Rand.Range(-1f, 1f);
                    rndVec.z += Rand.Range(-1f, 1f);
                    Vector3 angle = TM_Calc.GetVector(rndVec, ExactPosition);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Base_Smoke, rndVec, Map, Rand.Range(.8f, 1.5f), .1f, .05f, .15f, -300, 2, (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(), Rand.Range(0, 360));
                    //TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, rndVec, base.Map, Rand.Range(.8f, 1.5f), .1f, .05f, .15f, -300, 2, (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(), Rand.Range(0, 360));
                    
                }
                if(searchDelay < 0)
                {
                    if(destination != default(Vector3))
                    {
                        searchDelay = Rand.Range(10, 20);
                        CleanFilth();
                    }
                }                
                
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
                Pawn hitPawn;
                bool flag2 = (hitPawn = (Position.GetThingList(Map).FirstOrDefault((Thing x) => x == assignedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = hitPawn;
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

            Destroy(DestroyMode.Vanish);
        }        
    }
}
