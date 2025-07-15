using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_PsiStorm : Projectile
    {

        protected Vector3 orbPosition;
        protected Vector3 directionToOrb;
        protected IntVec3 centerLoc;

        private int[] ticksTillHeavy = new int[200];

        private List<IntVec3> boltOrigin = new List<IntVec3>();
        private List<IntVec3> boltDestination = new List<IntVec3>();
        private List<Vector3> boltPosition = new List<Vector3>();
        private List<Vector3> boltVector = new List<Vector3>();
        private List<int> boltTick = new List<int>();
        private List<float> boltMagnitude = new List<float>();

        private List<IntVec3> strikeCells;

        private int effectsTick = 0;
        private int boltDelayTicks = 10;
        private int nextStrikeGenTick;
        private float magnitudeAdjuster = 1f;
        private float initialOffsetMagnitude = 10f;

        private float directionAngle;

        private int age = -1;
        private int duration = 240;
        private float arcaneDmg = 1;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;

        private int pwrVal;
        private int verVal;
        private int effVal = 0;
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

        public override Vector3 DrawPos
        {
            get
            {
                return orbPosition;
            }
        }

        public new Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(orbPosition - centerLoc.ToVector3Shifted());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
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
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.ExplosionFlash, 12f);
                FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));
            }
            boltOrigin = new List<IntVec3>();
            boltPosition = new List<Vector3>();
            boltDestination = new List<IntVec3>();
            boltVector = new List<Vector3>();
            strikeCells = new List<IntVec3>();
            boltTick = new List<int>();
            boltMagnitude = new List<float>();
            strikeCells.Clear();
            strikeCells = GenRadial.RadialCellsAround(centerLoc, 7, true).ToList();
            for(int i =0; i < strikeCells.Count(); i++)
            {
                if(!strikeCells[i].InBoundsWithNullCheck(pawn.Map) || !strikeCells[i].IsValid)
                {
                    strikeCells.Remove(strikeCells[i]);
                }
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

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            
            arcaneDmg = comp.mightPwr;
            //MightPowerSkill pwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicStorm_pwr");
            //MightPowerSkill ver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicStorm_ver");
            //verVal = ver.level;
            //pwrVal = pwr.level;
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_PsionicStorm, false);
            pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_PsionicStorm, false);
            if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            {
                pwrVal = 3;
                verVal = 3;
            }
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            //determine true center, calculate strike radius
            //determine pawn position relative to true center, if pawn is to the right set orb top right of strike radius (multiple of a vector shift ~30deg offset of north and in the direction of the pawn)
            //this position is the orb position, set orb exact position to this (make sure to check if out of bounds, if so, put the orb on the other side, if too high, put orb below)
            //tick checks for pawn status (exists, alive, not downed, performing job)
            //maintains a 'strike interval' that slowly decreases as the spell remains active (to a minimum point mathf.max(minimum interval, strike interval (which changes)
            //if strike time - pick two spots within the strike radius, one is origin, one is destination, get a vector from origin to dest
            //with a small strike delay (1-3 ticks), continue to generate a new lighting strike from the origin to the dest until the position is the same as the destination
            //might need to set an array to handle the lightning bolts, if you want more than 1 at a time
            //if the caster runs out of stamina or psi to sustain the ability, then it ends the job and the psi storm should fade
            //psi storm job does nothing but stand there and subtract energy
            //might use tm_thunderstrike sound to reduce volume of the strike, play thunder offmap each iteration, generate a "cloud" of motes around the orb
            //might need to pull the mesh maker functionality from wizardry

            centerLoc = targ.Cell;
            impactDamage = newDamageInfo;
            speed = 0f;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            Initialize();
            GetOrbOffset();
        }

        private void GetOrbOffset()
        {
            Vector3 offsetVec = default(Vector3);
            if (centerLoc.x < pawn.Position.x)
            {
                offsetVec.x = .4f;                
            }
            else
            {
                offsetVec.x = -.4f;
            }
            offsetVec.z = .866f;
            orbPosition = (centerLoc.ToVector3Shifted() + (initialOffsetMagnitude * offsetVec));
            if(!orbPosition.InBoundsWithNullCheck(pawn.Map) || !orbPosition.ToIntVec3().IsValid)
            {
                offsetVec.x *= -1f;
                orbPosition = (centerLoc.ToVector3Shifted() + (initialOffsetMagnitude * offsetVec));
                if (!orbPosition.InBoundsWithNullCheck(pawn.Map) || !orbPosition.ToIntVec3().IsValid)
                {
                    offsetVec.z *= -1f;
                    orbPosition = (centerLoc.ToVector3Shifted() + (initialOffsetMagnitude * offsetVec));
                    if (!orbPosition.InBoundsWithNullCheck(pawn.Map) || !orbPosition.ToIntVec3().IsValid)
                    {
                        offsetVec.x *= -1f;
                        orbPosition = (centerLoc.ToVector3Shifted() + (initialOffsetMagnitude * offsetVec));
                        if (!orbPosition.InBoundsWithNullCheck(pawn.Map) || !orbPosition.ToIntVec3().IsValid)
                        {
                            Log.Message("No valid cell found to begin psionic storm.");
                            Destroy(DestroyMode.Vanish);
                        }
                    }
                }
            }
            //this.orbPosition.z = (int)AltitudeLayer.MoteOverhead;
        }

        protected override void Tick()
        {
            //base.Tick();
            age++;
            if (!pawn.DestroyedOrNull() && !pawn.Dead && !pawn.Downed && age > 0)
            { 
                //if job def is on pawn...
                if (Find.TickManager.TicksGame % 3 == 0)
                {
                    DrawEffects(orbPosition, pawn.Map);
                }

                if(nextStrikeGenTick < Find.TickManager.TicksGame)
                {
                    nextStrikeGenTick = Find.TickManager.TicksGame + 360;
                    GenerateNewBolt();                    
                }

                DrawBoltMeshes();
            }
            else if(age > duration)
            {
                Destroy(DestroyMode.Vanish);
            }
            else
            {
                //fade out
            }
        }

        private void DrawBoltMeshes()
        {
            for (int i = 0; i < boltPosition.Count(); i++)
            {
                if(boltTick[i] < 0)
                {
                    boltTick[i] = boltDelayTicks;
                    if(boltPosition[i].ToIntVec3() == boltDestination[i] || ((boltDestination[i] - boltOrigin[i]).LengthHorizontal <= (boltPosition[i].ToIntVec3() - boltOrigin[i]).LengthHorizontal))
                    {
                        //clears this instance of a bolt
                        boltOrigin.Remove(boltOrigin[i]);
                        boltDestination.Remove(boltDestination[i]);
                        boltPosition.Remove(boltPosition[i]);
                        boltTick.Remove(boltTick[i]);
                        boltVector.Remove(boltVector[i]);
                        boltMagnitude.Remove(boltMagnitude[i]);
                    }
                    else
                    {
                        //int rnd = Rand.RangeInclusive(0, 2);
                        //if (rnd == 0)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.doubleForkLightning, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 1f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        //else if (rnd == 1)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.singleForkLightning, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 1f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        //else if (rnd == 2)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.multiForkLightning, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 1f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        FleckMaker.ThrowHeatGlow(boltPosition[i].ToIntVec3(), pawn.Map, 1f);
                        //else if (rnd == 3)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.doubleForkLightning, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 2f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        //else if (rnd == 4)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.standardLightning, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 2f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        //else if (rnd == 5)
                        //{
                        //    this.pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(this.pawn.Map, TM_MatPool.psiMote, this.orbPosition.ToIntVec3(), this.boltPosition[i].ToIntVec3(), 2f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        //}
                        pawn.Map.weatherManager.eventHandler.AddEvent(new TM_MapMesh(pawn.Map, TM_MatPool.standardLightning, orbPosition.ToIntVec3(), boltPosition[i].ToIntVec3(), 2f, AltitudeLayer.MoteOverhead, 6, 25, 10));
                        MoveBoltPos(i);
                    }
                }
                else
                {
                    boltTick[i]--;
                }
            }
        }

        private void MoveBoltPos(int i)
        {
            boltPosition[i] = boltOrigin[i].ToVector3Shifted() + (boltVector[i] * boltMagnitude[i]);
            boltMagnitude[i] += magnitudeAdjuster;
        }

        private void GenerateNewBolt()
        {
            IntVec3 origin = strikeCells.RandomElement();
            IntVec3 destination = strikeCells.RandomElement();
            boltOrigin.Add(origin);
            boltDestination.Add(destination);
            boltVector.Add(TM_Calc.GetVector(origin, destination));
            boltPosition.Add(origin.ToVector3Shifted());
            boltTick.Add(0);
            boltMagnitude.Add(magnitudeAdjuster);
        }

        public void DrawEffects(Vector3 effectVec, Map map)
        {
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);
            FleckMaker.ThrowLightningGlow(effectVec, map, Rand.Range(.6f, .9f));
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

        //private void ImpactSomething()
        //{
        //    bool flag = this.assignedTarget != null;
        //    if (flag)
        //    {
        //        Pawn pawn = this.assignedTarget as Pawn;
        //        bool flag2 = pawn != null && pawn.GetPosture() != PawnPosture.Standing && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f;
        //        if (flag2)
        //        {
        //            this.Impact(null);
        //        }
        //        else
        //        {
        //            this.Impact(this.assignedTarget);
        //        }
        //    }
        //    else
        //    {
        //        this.Impact(null);
        //    }
        //}

        //protected virtual void Impact(Thing hitThing)
        //{
        //    bool flag = hitThing == null;
        //    if (flag)
        //    {
        //        Pawn pawn;
        //        bool flag2 = (pawn = (base.Position.GetThingList(base.Map).FirstOrDefault((Thing x) => x == this.assignedTarget) as Pawn)) != null;
        //        if (flag2)
        //        {
        //            hitThing = pawn;
        //        }
        //    }
        //    if (hitThing != null)
        //    {
        //        damageEntities(hitThing, Mathf.RoundToInt(Rand.Range(this.def.projectile.GetDamageAmount(1, null) * .75f, this.def.projectile.GetDamageAmount(1, null) * 1.25f)));
        //    }
        //    TM_MoteMaker.ThrowShadowCleaveMote(this.ExactPosition, this.Map, 2f + (.4f * pwrVal), .05f, .1f, .3f, 0, (5f + pwrVal), this.directionAngle);
        //    TorannMagicDefOf.TM_SoftExplosion.PlayOneShot(new TargetInfo(this.ExactPosition.ToIntVec3(), this.pawn.Map, false));
        //    int num = GenRadial.NumCellsInRadius(1 + (.4f * pwrVal));

        //    Vector3 cleaveVector;
        //    IntVec3 intVec;
        //    for (int i = 0; i < num; i++)
        //    {
        //        cleaveVector = this.ExactPosition + (Quaternion.AngleAxis(-45, Vector3.up) * ((1.5f + (.5f * pwrVal)) * this.direction));
        //        intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
        //        //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f * this.def.projectile.GetDamageAmount(1,null), 1.1f * this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

        //        if (intVec.IsValid && intVec.InBoundsWithNullCheck(this.Map))
        //        {
        //            List<Thing> hitList = new List<Thing>();
        //            hitList = intVec.GetThingList(base.Map);
        //            for (int j = 0; j < hitList.Count; j++)
        //            {
        //                if (hitList[j] is Pawn && hitList[j] != this.pawn)
        //                {
        //                    damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(this.def.projectile.GetDamageAmount(1, null) * .6f, this.def.projectile.GetDamageAmount(1, null) * .8f) * (float)(1f + .1 * pwrVal) * this.arcaneDmg)));
        //                }
        //            }
        //        }
        //        cleaveVector = this.ExactPosition + (Quaternion.AngleAxis(45, Vector3.up) * ((1.5f + (.5f * pwrVal)) * this.direction));
        //        intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
        //        //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f * this.def.projectile.GetDamageAmount(1,null), 1.1f * this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

        //        if (intVec.IsValid && intVec.InBoundsWithNullCheck(this.Map))
        //        {
        //            List<Thing> hitList = new List<Thing>();
        //            hitList = intVec.GetThingList(base.Map);
        //            for (int j = 0; j < hitList.Count; j++)
        //            {
        //                if (hitList[j] is Pawn && hitList[j] != this.pawn)
        //                {
        //                    damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(this.def.projectile.GetDamageAmount(1, null) * .5f, this.def.projectile.GetDamageAmount(1, null) * .7f) * (float)(1f + .1 * pwrVal) * this.arcaneDmg)));
        //                }
        //            }
        //        }
        //        cleaveVector = this.ExactPosition + ((2 + (.3f * (float)pwrVal)) * this.direction);
        //        intVec = cleaveVector.ToIntVec3() + GenRadial.RadialPattern[i];
        //        //ExplosionHelper.Explode(intVec, base.Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f*this.def.projectile.GetDamageAmount(1,null), 1.1f*this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);

        //        if (intVec.IsValid && intVec.InBoundsWithNullCheck(this.Map))
        //        {
        //            List<Thing> hitList = new List<Thing>();
        //            hitList = intVec.GetThingList(base.Map);
        //            for (int j = 0; j < hitList.Count; j++)
        //            {
        //                if (hitList[j] is Pawn && hitList[j] != this.pawn)
        //                {
        //                    damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(this.def.projectile.GetDamageAmount(1, null) * .5f, this.def.projectile.GetDamageAmount(1, null) * .7f) * (float)(1f + .1 * pwrVal) * this.arcaneDmg)));
        //                }
        //            }
        //        }
        //    }
        //    this.Destroy(DestroyMode.Vanish);
        //    //ExplosionHelper.Explode(base.Position, base.Map, this.radius, TMDamageDefOf.DamageDefOf.TM_DeathBolt, this.launcher as Pawn, Mathf.RoundToInt((Rand.Range(.6f*this.def.projectile.GetDamageAmount(1,null), 1.1f*this.def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * this.arcaneDmg), this.def.projectile.soundExplode, def, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);
        //}

        //public void DamageThingsAtPosition()
        //{
        //    int num = GenRadial.NumCellsInRadius(this.proximityRadius);
        //    IntVec3 curCell;
        //    for (int i = 0; i < num; i++)
        //    {
        //        curCell = this.ExactPosition.ToIntVec3() + GenRadial.RadialPattern[i];
        //        List<Thing> hitList = new List<Thing>();
        //        hitList = curCell.GetThingList(base.Map);
        //        for (int j = 0; j < hitList.Count; j++)
        //        {
        //            if (hitList[j] is Pawn && hitList[j] != this.pawn)
        //            {
        //                damageEntities(hitList[j], Mathf.RoundToInt((Rand.Range(this.def.projectile.GetDamageAmount(1, null) * .2f, this.def.projectile.GetDamageAmount(1, null) * .3f)) * this.arcaneDmg));
        //                TM_MoteMaker.ThrowShadowCleaveMote(this.ExactPosition, this.Map, Rand.Range(.2f, .4f), .01f, .2f, .4f, 500, 0, 0);
        //                TorannMagicDefOf.TM_Vibration.PlayOneShot(new TargetInfo(this.ExactPosition.ToIntVec3(), pawn.Map, false));
        //            }
        //        }
        //    }
        //}

        public void damageEntities(Thing e, int amt)
        {
            DamageInfo dinfo = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_Shadow, amt, 2, (float)(1f - directionAngle / 360f), null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }
}
