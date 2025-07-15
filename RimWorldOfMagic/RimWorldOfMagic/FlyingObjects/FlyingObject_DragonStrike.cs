using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_DragonStrike : Projectile
    {
        protected new Vector3 origin;

        protected new Vector3 destination;

        protected float speed = 40f;
        private bool drafted;
        private int verVal;

        protected new int ticksToImpact;

        protected Thing assignedTarget;
        protected Thing flyingThing;

        private float distanceToTarget;
        private float damageMultiplier = 1;
        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        public int weaponDmg = 0;

        private Pawn pawn;
        private CompAbilityUserMight comp;

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
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<float>(ref distanceToTarget, "distanceToTarget", 0, false);
            Scribe_Values.Look<float>(ref damageMultiplier, "damageMultiplier", 1, false);
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
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 12f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            if(distanceToTarget > 12)
            {
                damageMultiplier = .5f;
            }
            else if(distanceToTarget > 8)
            {
                damageMultiplier = .8f;
            }
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

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;            
            pawn = launcher as Pawn;
            drafted = pawn.Drafted;
            comp = pawn.GetCompAbilityUserMight();
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_DragonStrike, true);
            //verVal = TM_Calc.GetMightSkillLevel(pawn, comp.MightData.MightPowerSkill_DragonStrike, "TM_DragonStrike", "_ver", true);
            //this.verVal = comp.MightData.MightPowerSkill_DragonStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_DragonStrike_ver").level;
            //if (comp.Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            //{
            //    MightPowerSkill mver = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
            //    verVal = mver.level;
            //}
            if (spawned)
            {               
                flyingThing.DeSpawn();
            }
            //
            ModOptions.Constants.SetPawnInFlight(true);
            //
            this.origin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            distanceToTarget = (targ.Cell - origin.ToIntVec3()).LengthHorizontal;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;                
            }
            destination = targ.Cell.ToVector3();
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
                    FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.8f, 1.2f));
                }               
         
                //if(Find.TickManager.TicksGame % 10 == 0 && this.assignedTarget != null)
                //{
                //    this.origin = this.ExactPosition;
                //    this.destination = this.assignedTarget.DrawPos;
                //    this.ticksToImpact = this.StartingTicksToImpact;
                //}

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
                    //pawn.Drawer.DrawAt(this.DrawPos);  
                    
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
                Pawn targetPawn = assignedTarget as Pawn;
                bool flag2 = targetPawn != null && targetPawn.GetPosture() != PawnPosture.Standing && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.1f;
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
            bool hasValue = impactDamage.HasValue && hitThing != null && hitThing is Pawn;
            if (hasValue)
            {
                Pawn hitPawn = hitThing as Pawn;
                impactDamage.Value.SetAmount(impactDamage.Value.Amount * damageMultiplier);
                try
                {
                    hitPawn.TakeDamage(impactDamage.Value);
                    if (distanceToTarget <= 6f && hitThing is Pawn)
                    {
                        if (!hitPawn.DestroyedOrNull() && !hitPawn.Downed && !hitPawn.Dead)
                        {
                            Vector3 launchVector = TM_Calc.GetVector(origin, hitThing.Position.ToVector3());
                            IntVec3 projectedPosition = hitThing.Position + ((10f - distanceToTarget) * (1 + (.15f * verVal)) * launchVector).ToIntVec3();
                            if (projectedPosition.IsValid && projectedPosition.InBoundsWithNullCheck(hitThing.Map))
                            {
                                LaunchFlyingObect(projectedPosition, hitPawn, Mathf.RoundToInt(10f - distanceToTarget));
                            }
                        }
                    }
                }
                catch (NullReferenceException ex)
                {

                }
            }
            try
            {
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);                

                GenSpawn.Spawn(flyingThing, Position, Map);
                ModOptions.Constants.SetPawnInFlight(false);
                Pawn p = flyingThing as Pawn;
                if (p.IsColonist)
                {
                    if (ModOptions.Settings.Instance.cameraSnap)
                    {
                        CameraJumper.TryJumpAndSelect(p);
                    }
                    p.drafter.Drafted = drafted;
                }
                Destroy(DestroyMode.Vanish);
            }
            catch
            {
                GenSpawn.Spawn(flyingThing, Position, Map);
                ModOptions.Constants.SetPawnInFlight(false);
                Pawn p = flyingThing as Pawn;
                if (p.IsColonist)
                {
                    p.drafter.Drafted =drafted;
                }
                Destroy(DestroyMode.Vanish);
            }
        }

        public void LaunchFlyingObect(IntVec3 targetCell, Pawn pawn, int force)
        {
            bool flag = targetCell != IntVec3.Invalid && targetCell != default(IntVec3);
            if (flag)
            {
                if (pawn != null && pawn.Position.IsValid && pawn.Spawned && pawn.Map != null && !pawn.Downed && !pawn.Dead)
                {
                    FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), pawn.Position, pawn.Map);
                    flyingObject.speed = 25 + force;
                    flyingObject.Launch(pawn, targetCell, pawn);
                }
            }
        }
    }
}
