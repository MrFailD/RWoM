using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using AbilityUser;
using TorannMagic.Weapon;

namespace TorannMagic
{
    //[StaticConstructorOnStartup]
    public class FlyingObject_Spinning : Projectile
    {
        protected new Vector3 origin;
        protected new Vector3 destination;
        private Vector3 direction;

        public float speed = 25f;
        public int spinRate;        //spin rate > 0 makes the object rotate every spinRate Ticks
        private int rotation;
        protected new int ticksToImpact;
        //protected new Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        private bool drafted;

        public float force = 1f;

        private bool earlyImpact;
        private float impactForce;

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
            Scribe_Values.Look<bool>(ref drafted, "drafted", false, false);
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

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        { 
            bool spawned = flyingThing != null && flyingThing.Spawned;            
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
            else if(!ExactPosition.ToIntVec3().Walkable(Map))
            {
                earlyImpact = true;
                impactForce = (DestinationCell - ExactPosition.ToIntVec3()).LengthHorizontal + (speed * .2f);
                ImpactSomething();
            }
            else
            {
                Position = ExactPosition.ToIntVec3();
                if(Find.TickManager.TicksGame % 3 == 0)
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
                else if(flyingThing is Corpse)
                {
                    Vector3 arg_2B_0 = DrawPos;
                    bool flag4 = !DrawPos.ToIntVec3().IsValid;
                    if (flag4)
                    {
                        return;
                    }
                    Corpse corpse = flyingThing as Corpse;
                    corpse.InnerPawn.Rotation = flyingThing.Rotation;
                    corpse.InnerPawn.Drawer.renderer.RenderPawnAt(DrawPos);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
            }
            else
            {
                if (spinRate > 0)
                {
                    if (Find.TickManager.TicksGame % spinRate == 0)
                    {
                        rotation++;
                        if (rotation >= 4)
                        {
                            rotation = 0;
                        }
                    }
                    if (rotation == 0)
                    {
                        Rotation = Rot4.West;
                    }
                    else if (rotation == 1)
                    {
                        Rotation = Rot4.North;
                    }
                    else if (rotation == 2)
                    {
                        Rotation = Rot4.East;
                    }
                    else
                    {
                        Rotation = Rot4.South;
                    }
                }
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, (ExactRotation), def.DrawMatSingle, 0);
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

                if (flyingThing != null)
                {
                    GenSpawn.Spawn(flyingThing, Position, Map);
                    if (flyingThing is Pawn p)
                    {
                        if (p.IsColonist && drafted)
                        {
                            p.drafter.Drafted = true;
                        }
                        if (earlyImpact)
                        {
                            damageEntities(p, impactForce, DamageDefOf.Blunt);
                            damageEntities(p, impactForce, DamageDefOf.Stun);
                        }
                    }
                    else if (flyingThing.def.thingCategories != null && (flyingThing.def.thingCategories.Contains(ThingCategoryDefOf.Chunks) || flyingThing.def.thingCategories.Contains(ThingCategoryDef.Named("StoneChunks"))))
                    {
                        float radius = 4f;
                        Vector3 center = ExactPosition;
                        if (earlyImpact)
                        {
                            bool wallFlag90neg = false;
                            IntVec3 wallCheck = (center + (Quaternion.AngleAxis(-90, Vector3.up) * direction)).ToIntVec3();
                            FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                            wallFlag90neg = wallCheck.Walkable(Map);

                            wallCheck = (center + (Quaternion.AngleAxis(90, Vector3.up) * direction)).ToIntVec3();
                            FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                            bool wallFlag90 = wallCheck.Walkable(Map);

                            if ((!wallFlag90 && !wallFlag90neg) || (wallFlag90 && wallFlag90neg))
                            {
                                //fragment energy bounces in reverse direction of travel
                                center = center + ((Quaternion.AngleAxis(180, Vector3.up) * direction) * 3);
                            }
                            else if (wallFlag90)
                            {
                                center = center + ((Quaternion.AngleAxis(90, Vector3.up) * direction) * 3);
                            }
                            else if (wallFlag90neg)
                            {
                                center = center + ((Quaternion.AngleAxis(-90, Vector3.up) * direction) * 3);
                            }

                        }

                        List<IntVec3> damageRing = GenRadial.RadialCellsAround(Position, radius, true).ToList();
                        List<IntVec3> outsideRing = GenRadial.RadialCellsAround(Position, radius, false).Except(GenRadial.RadialCellsAround(Position, radius - 1, true)).ToList();
                        for (int i = 0; i < damageRing.Count; i++)
                        {
                            List<Thing> allThings = damageRing[i].GetThingList(Map);
                            for (int j = 0; j < allThings.Count; j++)
                            {
                                if (allThings[j] is Pawn)
                                {
                                    damageEntities(allThings[j], Rand.Range(14, 22), DamageDefOf.Blunt);
                                }
                                else if (allThings[j] is Building)
                                {
                                    damageEntities(allThings[j], Rand.Range(56, 88), DamageDefOf.Blunt);
                                }
                                else
                                {
                                    if (Rand.Chance(.1f))
                                    {
                                        GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_RubbleRock), damageRing[i], Map, ThingPlaceMode.Near);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < outsideRing.Count; i++)
                        {
                            IntVec3 intVec = outsideRing[i];
                            if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                            {
                                Vector3 moteDirection = TM_Calc.GetVector(ExactPosition.ToIntVec3(), intVec);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Rubble, ExactPosition, Map, Rand.Range(.3f, .6f), .2f, .02f, .05f, Rand.Range(-100, 100), Rand.Range(8f, 13f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 0);
                                TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, ExactPosition, Map, Rand.Range(.9f, 1.2f), .3f, .02f, Rand.Range(.25f, .4f), Rand.Range(-100, 100), Rand.Range(5f, 8f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 0);
                                ExplosionHelper.Explode(intVec, Map, .4f, DamageDefOf.Blunt, pawn, 0, 0, SoundDefOf.Pawn_Melee_Punch_HitBuilding_Generic, null, null, null, ThingDefOf.Filth_RubbleRock, .25f, 1, null, false, null, 0f, 1, 0, false);
                                //FleckMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                            }
                        }
                        //damageEntities(this.flyingThing, 305, DamageDefOf.Blunt);
                        flyingThing.Destroy(DestroyMode.Vanish);
                    }
                    else if ((flyingThing.def.thingCategories != null && (flyingThing.def.thingCategories.Contains(ThingCategoryDefOf.Corpses))) || flyingThing is Corpse)
                    {
                        Corpse flyingCorpse = flyingThing as Corpse;
                        float radius = 3f;
                        Vector3 center = ExactPosition;
                        if (earlyImpact)
                        {
                            bool wallFlag90neg = false;
                            IntVec3 wallCheck = (center + (Quaternion.AngleAxis(-90, Vector3.up) * direction)).ToIntVec3();
                            FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                            wallFlag90neg = wallCheck.Walkable(Map);

                            wallCheck = (center + (Quaternion.AngleAxis(90, Vector3.up) * direction)).ToIntVec3();
                            FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                            bool wallFlag90 = wallCheck.Walkable(Map);

                            if ((!wallFlag90 && !wallFlag90neg) || (wallFlag90 && wallFlag90neg))
                            {
                                //fragment energy bounces in reverse direction of travel
                                center = center + ((Quaternion.AngleAxis(180, Vector3.up) * direction) * 3);
                            }
                            else if (wallFlag90)
                            {
                                center = center + ((Quaternion.AngleAxis(90, Vector3.up) * direction) * 3);
                            }
                            else if (wallFlag90neg)
                            {
                                center = center + ((Quaternion.AngleAxis(-90, Vector3.up) * direction) * 3);
                            }

                        }

                        List<IntVec3> damageRing = GenRadial.RadialCellsAround(Position, radius, true).ToList();
                        List<IntVec3> outsideRing = GenRadial.RadialCellsAround(Position, radius, false).Except(GenRadial.RadialCellsAround(Position, radius - 1, true)).ToList();
                        Filth filth = (Filth)ThingMaker.MakeThing(flyingCorpse.InnerPawn.def.race.BloodDef);
                        for (int i = 0; i < damageRing.Count; i++)
                        {
                            List<Thing> allThings = damageRing[i].GetThingList(Map);
                            for (int j = 0; j < allThings.Count; j++)
                            {
                                if (allThings[j] is Pawn)
                                {
                                    damageEntities(allThings[j], Rand.Range(18, 28), DamageDefOf.Blunt);
                                }
                                else if (allThings[j] is Building)
                                {
                                    damageEntities(allThings[j], Rand.Range(56, 88), DamageDefOf.Blunt);
                                }
                                else
                                {
                                    if (Rand.Chance(.05f))
                                    {
                                        if (filth != null)
                                        {
                                            filth = (Filth)ThingMaker.MakeThing(flyingCorpse.InnerPawn.def.race.BloodDef);
                                            GenPlace.TryPlaceThing(filth, damageRing[i], Map, ThingPlaceMode.Near);
                                        }
                                        else
                                        {
                                            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_Blood), damageRing[i], Map, ThingPlaceMode.Near);
                                        }
                                    }
                                    if (Rand.Chance(.05f))
                                    {
                                        GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_CorpseBile), damageRing[i], Map, ThingPlaceMode.Near);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < outsideRing.Count; i++)
                        {
                            IntVec3 intVec = outsideRing[i];
                            if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                            {
                                Vector3 moteDirection = TM_Calc.GetVector(ExactPosition.ToIntVec3(), intVec);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodSquirt, ExactPosition, Map, Rand.Range(.3f, .6f), .2f, .02f, .05f, Rand.Range(-100, 100), Rand.Range(4f, 13f), (Quaternion.AngleAxis(Rand.Range(60, 120), Vector3.up) * moteDirection).ToAngleFlat(), 0);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist, ExactPosition, Map, Rand.Range(.9f, 1.2f), .3f, .02f, Rand.Range(.25f, .4f), Rand.Range(-100, 100), Rand.Range(5f, 8f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 0);
                                ExplosionHelper.Explode(intVec, Map, .4f, DamageDefOf.Blunt, pawn, 0, 0, SoundDefOf.Pawn_Melee_Punch_HitBuilding_Generic, null, null, null, filth.def, .08f, 1, null, false, null, 0f, 1, 0, false);
                                //FleckMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                            }
                        }
                        //damageEntities(this.flyingThing, 305, DamageDefOf.Blunt);
                        //this.flyingThing.Destroy(DestroyMode.Vanish);
                    }
                }
                else
                {
                    float radius = 2f;
                    Vector3 center = ExactPosition;
                    if (earlyImpact)
                    {
                        bool wallFlag90neg = false;
                        IntVec3 wallCheck = (center + (Quaternion.AngleAxis(-90, Vector3.up) * direction)).ToIntVec3();
                        FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                        wallFlag90neg = wallCheck.Walkable(Map);

                        wallCheck = (center + (Quaternion.AngleAxis(90, Vector3.up) * direction)).ToIntVec3();
                        FleckMaker.ThrowMicroSparks(wallCheck.ToVector3Shifted(), Map);
                        bool wallFlag90 = wallCheck.Walkable(Map);

                        if ((!wallFlag90 && !wallFlag90neg) || (wallFlag90 && wallFlag90neg))
                        {
                            //fragment energy bounces in reverse direction of travel
                            center = center + ((Quaternion.AngleAxis(180, Vector3.up) * direction) * 3);
                        }
                        else if (wallFlag90)
                        {
                            center = center + ((Quaternion.AngleAxis(90, Vector3.up) * direction) * 3);
                        }
                        else if (wallFlag90neg)
                        {
                            center = center + ((Quaternion.AngleAxis(-90, Vector3.up) * direction) * 3);
                        }

                    }

                    List<IntVec3> damageRing = GenRadial.RadialCellsAround(Position, radius, true).ToList();
                    List<IntVec3> outsideRing = GenRadial.RadialCellsAround(Position, radius, false).Except(GenRadial.RadialCellsAround(Position, radius - 1, true)).ToList();
                    for (int i = 0; i < damageRing.Count; i++)
                    {
                        List<Thing> allThings = damageRing[i].GetThingList(Map);
                        for (int j = 0; j < allThings.Count; j++)
                        {
                            if (allThings[j] is Pawn)
                            {
                                damageEntities(allThings[j], Rand.Range(10, 16), DamageDefOf.Blunt);
                            }
                            else if (allThings[j] is Building)
                            {
                                damageEntities(allThings[j], Rand.Range(32, 88), DamageDefOf.Blunt);
                            }
                            else
                            {
                                if (Rand.Chance(.1f))
                                {
                                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_RubbleRock), damageRing[i], Map, ThingPlaceMode.Near);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < outsideRing.Count; i++)
                    {
                        IntVec3 intVec = outsideRing[i];
                        if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                        {
                            Vector3 moteDirection = TM_Calc.GetVector(ExactPosition.ToIntVec3(), intVec);
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Rubble, ExactPosition, Map, Rand.Range(.3f, .6f), .2f, .02f, .05f, Rand.Range(-100, 100), Rand.Range(8f, 13f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 0);
                            TM_MoteMaker.ThrowGenericFleck(FleckDefOf.Smoke, ExactPosition, Map, Rand.Range(.9f, 1.2f), .3f, .02f, Rand.Range(.25f, .4f), Rand.Range(-100, 100), Rand.Range(5f, 8f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 0);
                            ExplosionHelper.Explode(intVec, Map, .4f, DamageDefOf.Blunt, pawn, 0, 0, SoundDefOf.Pawn_Melee_Punch_HitBuilding_Generic, null, null, null, null, .4f, 1, null, false, null, 0f, 1, 0, false);
                            //FleckMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                        }
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
            catch
            {
                if (flyingThing != null)
                {
                    if (!flyingThing.Spawned)
                    {
                        GenSpawn.Spawn(flyingThing, Position, Map);
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
}

        public void damageEntities(Thing e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }
}
