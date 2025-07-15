using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_ShadowBolt : Projectile
    {

        private static readonly Color lightningColor = new Color(160f, 160f, 160f);
        private static readonly Material OrbMat = MaterialPool.MatFrom("Spells/shadowbolt", false);

        protected new Vector3 origin;
        protected new Vector3 destination;
        protected Vector3 direction;
        private float directionAngle;

        private int age = -1;
        private float arcaneDmg = 1;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;

        private int pwrVal;
        private int verVal;
        private float radius = 1.4f;

        private float proximityRadius = .4f;
        private int proximityFrequency = 6;

        protected float speed = 30f;
        protected new int ticksToImpact;

        protected Thing assignedTarget;
        protected Thing flyingThing;
        private Pawn pawn;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        private bool initialized = true;        

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

        public new  Vector3 ExactPosition
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
            Scribe_Values.Look<Vector3>(ref direction, "direction", default(Vector3), false);
            Scribe_Values.Look<float>(ref directionAngle, "directionAngle", 0, false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<float>(ref radius, "radius", 1.4f, false);
            Scribe_Values.Look<int>(ref proximityFrequency, "proximityFrequency", 6, false);
            Scribe_Values.Look<float>(ref proximityRadius, "proximityRadius", .4f, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 6f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            GetVector();
            flyingThing.ThingID += Rand.Range(0, 214).ToString();
            initialized = false;
        }

        public void GetVector()
        {
            Vector3 heading = (destination - ExactPosition);
            float distance = heading.magnitude;
            direction = heading / distance;
            directionAngle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
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

            

            arcaneDmg = comp.arcaneDmg;
            pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_ShadowBolt, true);
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_ShadowBolt, true);
            //MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_ShadowBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ShadowBolt_pwr");
            //MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_ShadowBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ShadowBolt_ver");
            //verVal = ver.level;
            //pwrVal = pwr.level;
            
            //if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            //{
            //    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
            //    MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
            //    pwrVal = mpwr.level;
            //    verVal = mver.level;
            //}
            //if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            //{
            //    pwrVal = 3;
            //    verVal = 3;
            //}
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.origin = origin;
            impactDamage = newDamageInfo;
            speed = def.projectile.speed;
            proximityRadius += (.4f * verVal);
            proximityFrequency -= verVal;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            float distanceAccuracyModifier = (targ.Cell.ToVector3Shifted() - pawn.Position.ToVector3Shifted()).MagnitudeHorizontal() *.1f;
            destination = targ.Cell.ToVector3Shifted();
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        protected override void Tick()
        {
            //base.Tick();
            age++;
            if (ticksToImpact >= 0)
            {
                DrawEffects(ExactPosition, Map);
            }
            ticksToImpact--;
            Position = ExactPosition.ToIntVec3();
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            if (flag)
            {
                ticksToImpact++;
                Destroy(DestroyMode.Vanish);
            }
            else
            {
                if(Find.TickManager.TicksGame % proximityFrequency ==0)
                {
                    DamageThingsAtPosition();
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

        public void DrawEffects(Vector3 effectVec, Map map)
        {
            effectVec += direction;
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);
            TM_MoteMaker.ThrowShadowMote(effectVec, map, Rand.Range(.6f, 1f));
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                Comps_PostDraw();
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
                Pawn pawn2;
                bool flag2 = (pawn2 = (Position.GetThingList(Map).FirstOrDefault((Thing x) => x == assignedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = pawn2;
                }
            }        
            if(hitThing != null)
            {
                damageEntities(hitThing, Mathf.RoundToInt(Rand.Range(def.projectile.GetDamageAmount(1,null) * .8f, def.projectile.GetDamageAmount(1,null) * 1.4f) * arcaneDmg));
            }
            TM_MoteMaker.ThrowShadowCleaveMote(ExactPosition, Map, 2f + (.4f * pwrVal), .05f, .1f, .3f, 0, (5f+ pwrVal), directionAngle);
            TorannMagicDefOf.TM_SoftExplosion.PlayOneShot(new TargetInfo(ExactPosition.ToIntVec3(), pawn.Map, false));
            int num = GenRadial.NumCellsInRadius(1+(.4f* pwrVal));

            Vector3 cleaveVector;
            IntVec3 intVec;
            for (int i = 0; i < num; i++)
            {
                cleaveVector = ExactPosition + (Quaternion.AngleAxis(-45, Vector3.up) * ((1.5f + (.5f*pwrVal)) * direction));
                intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
                //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f * this.def.projectile.GetDamageAmount(1,null), 1.1f * this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

                if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                {
                    List<Thing> hitList = new List<Thing>();
                    hitList = intVec.GetThingList(Map);
                    for (int j = 0; j < hitList.Count; j++)
                    {
                        if (hitList[j] is Pawn && hitList[j] != pawn)
                        {
                            damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(def.projectile.GetDamageAmount(1,null) * .6f, def.projectile.GetDamageAmount(1,null) * .8f) * (float)(1f + .1 * pwrVal) * arcaneDmg)));
                        }
                    }
                }
                cleaveVector = ExactPosition + (Quaternion.AngleAxis(45, Vector3.up) * ((1.5f + (.5f * pwrVal)) * direction));
                intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
                //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f * this.def.projectile.GetDamageAmount(1,null), 1.1f * this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

                if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                {
                    List<Thing> hitList = new List<Thing>();
                    hitList = intVec.GetThingList(Map);
                    for (int j = 0; j < hitList.Count; j++)
                    {
                        if (hitList[j] is Pawn && hitList[j] != pawn)
                        {
                            damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(def.projectile.GetDamageAmount(1,null) * .5f, def.projectile.GetDamageAmount(1,null) * .7f) * (float)(1f + .1 * pwrVal) * arcaneDmg)));
                        }
                    }
                }
                cleaveVector = ExactPosition + ((2 + (.3f * (float)pwrVal)) * direction);
                intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
                //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f*this.def.projectile.GetDamageAmount(1,null), 1.1f*this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

                if (intVec.IsValid && intVec.InBoundsWithNullCheck(Map))
                {
                    List<Thing> hitList = new List<Thing>();
                    hitList = intVec.GetThingList(Map);
                    for (int j = 0; j < hitList.Count; j++)
                    {
                        if (hitList[j] is Pawn && hitList[j] != pawn)
                        {
                            damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(def.projectile.GetDamageAmount(1,null) * .5f, def.projectile.GetDamageAmount(1,null) * .7f) * (float)(1f + .1 * pwrVal) * arcaneDmg)));

                        }
                    }
                }
            }
            Destroy(DestroyMode.Vanish);
            //ExplosionHelper.Explode(base.Position, base.Map, this.radius, TMDamageDefOf.DamageDefOf.TM_DeathBolt, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f*this.def.projectile.GetDamageAmount(1,null), 1.1f*this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);
        }

        public void DamageThingsAtPosition()
        {
            int num = GenRadial.NumCellsInRadius(proximityRadius);
            IntVec3 curCell;
            for (int i = 0; i < num; i++)
            {
                curCell = ExactPosition.ToIntVec3() + GenRadial.RadialPattern[i];
                List<Thing> hitList = new List<Thing>();
                hitList = curCell.GetThingList(Map);
                for (int j = 0; j < hitList.Count; j++)
                {
                    if (hitList[j] is Pawn && hitList[j] != pawn && hitList[j].Faction != pawn.Faction)
                    {
                        damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(def.projectile.GetDamageAmount(1,null) * .4f, def.projectile.GetDamageAmount(1,null) * .6f)) * arcaneDmg));
                        TM_MoteMaker.ThrowShadowCleaveMote(ExactPosition, Map, Rand.Range(.2f, .4f), .01f, .2f, .4f, 500, 0, 0);
                        TorannMagicDefOf.TM_Vibration.PlayOneShot(new TargetInfo(ExactPosition.ToIntVec3(), pawn.Map, false));
                    }
                }
            }
        }

        public void damageEntities(Thing e, int amt)
        {
            TM_Action.DamageEntities(e, null, amt, 2, TMDamageDefOf.DamageDefOf.TM_Shadow, pawn);
            //DamageInfo dinfo = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_Shadow, amt, 2, (float)(1f - this.directionAngle/360f), this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            //bool flag = e != null;
            //if (flag)
            //{
            //    e.TakeDamage(dinfo);
            //}
        }
    }
}
