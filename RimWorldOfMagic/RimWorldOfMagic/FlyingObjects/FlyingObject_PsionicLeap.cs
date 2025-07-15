using RimWorld;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_PsionicLeap : Projectile
    {

        private int effVal;

        protected new Vector3 origin;
        private Vector3 trueOrigin;
        protected new Vector3 destination;
        private Vector3 trueDestination;
        private Vector3 direction;
        private float trueAngle;

        private bool isSelected;
        private bool earlyImpact;

        protected float speed = 75f;

        protected new int ticksToImpact = 60;

        protected Thing assignedTarget;
        protected Thing flyingThing;
        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int weaponDmg = 0;

        private Pawn pawn;
        private Thing oldjobTarget;

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
            Scribe_Values.Look<int>(ref effVal, "effVal", 0, false);
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
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.3f, .5f), .1f, 0f, .1f, 0, 4f, trueAngle, trueAngle);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.5f, .6f), .1f, .04f, .1f, 0, 7f, trueAngle, trueAngle);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.7f, .8f), .1f, .08f, .1f, 0, 10f, trueAngle, trueAngle);
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
            if (Find.Selector.FirstSelectedObject == launcher)
            {
                isSelected = true;
            }
                
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            oldjobTarget = pawn.CurJob.targetA.Thing;
            //Log.Message("pre leap target is " + this.oldjobTarget.LabelShort);
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            effVal = TM_Calc.GetSkillEfficiencyLevel(pawn, TorannMagicDefOf.TM_PsionicAugmentation, false); //comp.MightData.MightPowerSkill_PsionicAugmentation.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicAugmentation_eff").level;
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
            trueDestination = targ.Cell.ToVector3Shifted();            
            direction = GetVector(trueOrigin.ToIntVec3(), targ.Cell);
            trueAngle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
            destination = targ.Cell.ToVector3Shifted();         
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        protected override void Tick()
        {
            //base.Tick();
            ticksToImpact--;
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            if (flag)
            {
                ticksToImpact++;
                Position = ExactPosition.ToIntVec3();
                Destroy(DestroyMode.Vanish);
            }
            else if (!ExactPosition.ToIntVec3().Walkable(Map))
            {
                earlyImpact = true;
                ImpactSomething();
            }
            else
            {
                Position = ExactPosition.ToIntVec3();
                FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.8f, 1.2f));
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
                bool flag3 = damageLaunched;
                if (flag3)
                {
                    hitThing.TakeDamage(impactDamage.Value);
                }
                
                bool flag4 = explosion;
                if (flag4)
                {
                    ExplosionHelper.Explode(Position, Map, 0.9f, DamageDefOf.Stun, this, -1, 0, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                }
            }
            TM_MoteMaker.MakePowerBeamMotePsionic(ExactPosition.ToIntVec3(), Map, 10f, 2f, .7f, .1f, .6f);
            //ExplosionHelper.Explode(this.ExactPosition.ToIntVec3(), this.Map, 1.7f, TMDamageDefOf.DamageDefOf.TM_PsionicInjury, this.pawn, Rand.Range(8, 12) + 2*this.effVal, 0, this.def.projectile.soundExplode, def, null, null, null, 0f, 1, false, null, 0f, 1, 0.0f, false);
            SearchForTargets(Position, 1.7f, Map);
            GenSpawn.Spawn(flyingThing, Position, Map);
            ModOptions.Constants.SetPawnInFlight(false);
            Pawn p = flyingThing as Pawn;
            if (p.IsColonist)
            {
                p.drafter.Drafted = true;
                if (isSelected)
                {
                    if (ModOptions.Settings.Instance.cameraSnap)
                    {
                        CameraJumper.TryJumpAndSelect(p);
                    }
                }
                if (oldjobTarget != null && !oldjobTarget.Destroyed)
                {
                    Job job = new Job(JobDefOf.AttackMelee, oldjobTarget);
                    p.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
                }
            }
            Destroy(DestroyMode.Vanish);
        }

        public void SearchForTargets(IntVec3 center, float radius, Map map)
        {
            Pawn victim = null;
            IntVec3 curCell;
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            for (int i = 0; i < targets.Count(); i++)
            {
                victim = null;
                curCell = targets.ToArray<IntVec3>()[i];
                if (curCell.InBoundsWithNullCheck(Map) && curCell.IsValid)
                {
                    victim = curCell.GetFirstPawn(map);
                }

                if (victim != null && victim != pawn && victim.Faction != pawn.Faction)
                {
                    DamageInfo dinfo = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_PsionicInjury, Rand.Range(8,12) + (2 * effVal), 0, (float)-1, pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                    victim.TakeDamage(dinfo);                    
                }
                targets.GetEnumerator().MoveNext();
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
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 25);
                }
                Comps_PostDraw();
            }
        }

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }
    }
}
