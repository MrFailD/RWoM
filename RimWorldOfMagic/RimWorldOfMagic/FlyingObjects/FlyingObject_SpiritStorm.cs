using RimWorld;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_SpiritStorm : Projectile
    {

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int searchDelay = 10;
        private int age = -1;
        private int duration = 1000;

        protected float speed = 6f;
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

        public float radius = 4;
        public float spellDamage = 5;
        public int destinationTick;
        public int frenzyBonus;

        public Vector3 ManualDestination = default(Vector3);
        public bool PlayerTargetSet;

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
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 600, false);
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
            Scribe_Values.Look<float>(ref radius, "radius", 4);
            Scribe_Values.Look<float>(ref spellDamage, "spellDamage", 5);
            Scribe_Values.Look<int>(ref frenzyBonus, "frenzyBonus", 0);
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
            if(pawn != null && pawn.story != null && pawn.story.Adulthood != null && pawn.story.Adulthood.identifier == "tm_vengeful_spirit")
            {
                frenzyBonus = 8;
            }
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            duration = Mathf.RoundToInt(duration * comp.arcaneDmg);
            radius = def.projectile.explosionRadius + comp.MagicData.GetSkill_Versatility(TorannMagicDefOf.TM_SpiritStorm).level;
            spellDamage = def.projectile.GetDamageAmount(this) * (1f + (.12f * comp.MagicData.GetSkill_Power(TorannMagicDefOf.TM_SpiritStorm).level)) * comp.arcaneDmg;
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
            StormEffects();
            destination = FindNearestTarget(this.launcher.DrawPos);
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        public Vector3 FindNearestTarget(Vector3 origin)
        {
            Vector3 destination = default(Vector3);
            if (PlayerTargetSet)
            {
                if (ExactPosition.ToIntVec3() == ManualDestination.ToIntVec3())
                {
                    PlayerTargetSet = false;
                }
                destination = ManualDestination;                
            }
            else
            {
                Pawn nearestEnemey = TM_Calc.FindNearestEnemy(Map, ExactPosition.ToIntVec3(), launcher.Faction, false, false);
                if(nearestEnemey != null)
                {
                    destination = nearestEnemey.DrawPos;
                }
                else
                {
                    Vector3 rndPos = Position.ToVector3Shifted();
                    rndPos.x += Rand.Range(-5f, 5f);
                    rndPos.z += Rand.Range(-5f, 5f);
                    destination = rndPos;
                }
            }
            return destination;
        }

        public void StormEffects()
        {
            List<Pawn> allPawns = TM_Calc.FindAllPawnsAround(Map, ExactPosition.ToIntVec3(), radius);
            if (allPawns != null && allPawns.Count > 0)
            {
                for (int i = 0; i < allPawns.Count; i++)
                {
                    Pawn p = allPawns[i];
                    if (p != null && !p.Dead)
                    {
                        TM_Action.DamageEntities(p, null, spellDamage, TMDamageDefOf.DamageDefOf.TM_Spirit, launcher);
                    }
                    StormGraphics(p);
                }
            }
        }

        public void StormGraphics(Thing filth)
        {
            float angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(filth.DrawPos, ExactPosition)).ToAngleFlat();
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Regen, filth.DrawPos, Map, Rand.Range(.2f, .4f), .8f, .2f, .4f, Rand.Range(-400, -100), 1.9f, angle, Rand.Range(0, 360));
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
                if (ticksToImpact <= 0)
                {
                    if (age < duration)
                    {
                        origin = ExactPosition;
                        destination = FindNearestTarget(origin);
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
                if(destinationTick < Find.TickManager.TicksGame && age < duration)
                {
                    destinationTick = Find.TickManager.TicksGame + 150;
                    origin = ExactPosition;
                    destination = FindNearestTarget(origin);
                    ticksToImpact = StartingTicksToImpact;
                }
                if (Find.TickManager.TicksGame % 6 == 0)
                {
                    Vector3 rndVec = ExactPosition;
                    rndVec.x += Rand.Range(-3f, 3f);
                    rndVec.z += Rand.Range(-3f, 3f);
                    Vector3 angle = TM_Calc.GetVector(rndVec, ExactPosition);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PurpleSmoke, rndVec, Map, Rand.Range(.8f, 1.5f), .3f, .1f, .25f, -300, 2, (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(), Rand.Range(0, 360));
                    Effecter effecter = TorannMagicDefOf.TM_SpiritStormED.Spawn();
                    effecter.scale *= (radius / 4f);
                    effecter.Trigger(new TargetInfo(ExactPosition.ToIntVec3(), Map, false), new TargetInfo(ExactPosition.ToIntVec3(), Map, false));
                    effecter.Cleanup();
                }
                if(searchDelay < 0)
                {
                    if(destination != default(Vector3))
                    {
                        searchDelay = Rand.Range(25, 35) - frenzyBonus;
                        StormEffects();
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
