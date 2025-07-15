using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_PsionicDash : Projectile
    {
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;

        protected new Vector3 origin;
        private Vector3 trueOrigin;
        protected new Vector3 destination;
        private Vector3 trueDestination;
        private Vector3 direction;
        private float trueAngle;

        private int dashStep;
        private bool earlyImpact;
        private int drawTicks = 300;
        private bool shouldDrawPawn = true;

        protected float speed = 15f;

        protected new int ticksToImpact = 60;

        protected Thing assignedTarget;
        protected Thing flyingThing;
        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int weaponDmg;
        private bool drafted;
        private Pawn pawn;

        //step variables
        private float sideForwardMagnitude = 2f;
        private float sideMagnitude = 3f;
        private float explosiveMagnitude = 1f;

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
            Scribe_Values.Look<Vector3>(ref trueOrigin, "trueOrigin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref trueDestination, "trueDestination", default(Vector3), false);            
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref weaponDmg, "weaponDmg", 0, false);
            Scribe_Values.Look<int>(ref dashStep, "dashStep", 0, false);
            Scribe_Values.Look<float>(ref trueAngle, "trueAngle", 0, false);
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
                CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
                verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_PsionicDash, false);
                pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_PsionicDash, false);
                //verVal = TM_Calc.GetMightSkillLevel(pawn, comp.MightData.MightPowerSkill_PsionicDash, "TM_PsionicDash", "_ver", true);
                //pwrVal = TM_Calc.GetMightSkillLevel(pawn, comp.MightData.MightPowerSkill_PsionicDash, "TM_PsionicDash", "_pwr", true);
                //this.pwrVal = comp.MightData.MightPowerSkill_PsionicDash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicDash_pwr").level;
                //this.verVal = comp.MightData.MightPowerSkill_PsionicDash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicDash_ver").level;
                arcaneDmg = comp.mightPwr;
                //if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                //{
                //    this.pwrVal = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr").level;
                //    this.verVal = comp.MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver").level;
                //}                
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
            if (pawn.Drafted)
            {
                drafted = true;
            }
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
            speed = 15;
            trueDestination = targ.Cell.ToVector3Shifted();            
            direction = GetVector(trueOrigin.ToIntVec3(), targ.Cell);
            trueAngle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
            destination = this.origin + (direction * 3f);         
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        protected override void Tick()
        {
            //base.Tick();
            drawTicks--;
            if(drawTicks <= 0)
            {
                shouldDrawPawn = false;
            }
            Vector3 exactPosition = ExactPosition;
            ticksToImpact--;
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            //if (flag)
            //{
            //    this.ticksToImpact++;
            //    base.Position = this.ExactPosition.ToIntVec3();
            //    this.Destroy(DestroyMode.Vanish);
            //}
            if (flag || !ExactPosition.ToIntVec3().Walkable(Map) || ExactPosition.ToIntVec3().DistanceToEdge(Map) <= 1)
            {
                earlyImpact = true;
                ImpactSomething();
            }
            else
            {
                Position = ExactPosition.ToIntVec3();
                FleckMaker.ThrowDustPuff(Position, Map, Rand.Range(0.8f, 1.2f));
                if (Find.TickManager.TicksGame % 4 == 0)
                {
                    ApplyDashDamage();
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

        private void ImpactSomething()
        {
            if (dashStep == 0 && !earlyImpact) //1
            {
                shouldDrawPawn = false;
                speed = 30;
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.3f, .5f), .1f, 0f, .1f, 0, 4f, trueAngle, trueAngle);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.5f, .6f), .1f, .04f, .1f, 0, 7f, trueAngle, trueAngle);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.7f, .8f), .1f, .08f, .1f, 0, 10f, trueAngle, trueAngle);
                origin = ExactPosition;
                destination = origin + (direction * 2f);
                ticksToImpact = StartingTicksToImpact;
                dashStep = 1;
            }
            else if (dashStep == 1 && !earlyImpact) //2
            {
                ExplosiveStep(explosiveMagnitude);
                dashStep = 2;
            }
            else if (dashStep == 2 && !earlyImpact) //3
            {
                SideStep(90, sideMagnitude/2, sideForwardMagnitude);
                dashStep = 3;

            }
            else if (dashStep == 3 && !earlyImpact) //4
            {
                ExplosiveStep(explosiveMagnitude);
                dashStep = 4;
            }
            else if (dashStep == 4 && !earlyImpact) //5
            {
                SideStep(-90, sideMagnitude, sideForwardMagnitude);
                dashStep = 5;
            }
            else if (dashStep == 5 && !earlyImpact) //6 - check for skill upgrades
            {
                ExplosiveStep(explosiveMagnitude); 
                if (verVal > 0)
                {
                    dashStep = 6;
                }
                else
                {
                    dashStep = 20;
                }
            }
            else if(dashStep == 6 && !earlyImpact) //skill step 1
            {
                SideStep(90, sideMagnitude, sideForwardMagnitude);
                dashStep = 7;

            }
            else if (dashStep == 7 && !earlyImpact)
            {
                ExplosiveStep(explosiveMagnitude);
                if (verVal > 1)
                {
                    dashStep = 8;
                }
                else
                {
                    dashStep = 21;
                }
            }
            else if (dashStep == 8 && !earlyImpact) //skill step 2
            {
                SideStep(-90, sideMagnitude, sideForwardMagnitude);
                dashStep = 9;

            }
            else if (dashStep == 9 && !earlyImpact)
            {
                ExplosiveStep(explosiveMagnitude);
                if (verVal > 2)
                {
                    dashStep = 10;
                }
                else
                {
                    dashStep = 20;
                }
            }
            else if (dashStep == 10 && !earlyImpact) //skill step 3
            {
                SideStep(90, sideMagnitude, sideForwardMagnitude);
                dashStep = 11;

            }
            else if (dashStep == 11 && !earlyImpact)
            {
                ExplosiveStep(explosiveMagnitude);
                dashStep = 21;
            }
            else if (dashStep == 20 && !earlyImpact) //Recenter
            {
                SideStep(90, sideMagnitude / 2, sideForwardMagnitude);
                dashStep = 22;
            }
            else if(dashStep == 21 && !earlyImpact)
            {
                SideStep(-90, sideMagnitude / 2, sideForwardMagnitude);
                dashStep = 22;
            }
            else if (dashStep == 22 && !earlyImpact) //End
            {
                ExplosiveStepFinal(explosiveMagnitude/2f);
                dashStep = 50;
            }
            else
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
        }

        private void SideStep(float angle, float sideMagnitude, float forwardMagnitude)
        {
            shouldDrawPawn = false;
            speed = 60;
            origin = ExactPosition + ((Quaternion.AngleAxis(angle, Vector3.up) * direction) * sideMagnitude);
            destination = origin + (direction * forwardMagnitude);
            ticksToImpact = StartingTicksToImpact;            
        }

        private void ExplosiveStep(float forwardMagnitude)
        {
            drawTicks = 120;
            speed = 40;
            TM_MoteMaker.MakePowerBeamMotePsionic(Position, Map, 10f, 2f, .7f, .1f, .6f);
            ExplosionHelper.Explode(Position, Map, 1.7f, TMDamageDefOf.DamageDefOf.TM_PsionicInjury, pawn, Mathf.RoundToInt(Rand.Range(8, 14) * (1+ .1f * pwrVal) * arcaneDmg), 0, def.projectile.soundExplode, def, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.0f, false);
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.3f, .5f), .1f, 0f, .1f, 0, 4f, trueAngle, trueAngle);
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.5f, .6f), .1f, .04f, .1f, 0, 7f, trueAngle, trueAngle);
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_PsiCurrent, ExactPosition, Map, Rand.Range(.7f, .8f), .1f, .08f, .1f, 0, 10f, trueAngle, trueAngle);
            origin = ExactPosition;
            destination = origin + (direction * forwardMagnitude);
            ticksToImpact = StartingTicksToImpact;
        }

        private void ExplosiveStepFinal(float forwardMagnitude)
        {
            shouldDrawPawn = true;
            drawTicks = 60;
            speed = 20;
            TM_MoteMaker.MakePowerBeamMotePsionic(Position, Map, 10f, 2f, .7f, .1f, .6f);
            ExplosionHelper.Explode(Position, Map, 1.7f, TMDamageDefOf.DamageDefOf.TM_PsionicInjury, pawn, Mathf.RoundToInt(Rand.Range(10, 16) * (1 + .1f * pwrVal) * arcaneDmg), 0, def.projectile.soundExplode, def, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.0f, false);
            origin = ExactPosition;
            destination = origin + (direction * forwardMagnitude);
            ticksToImpact = StartingTicksToImpact;
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
            GenSpawn.Spawn(flyingThing, Position, Map);
            ModOptions.Constants.SetPawnInFlight(false);
            Pawn p = flyingThing as Pawn;
            if (p.IsColonist && drafted)
            {
                p.drafter.Drafted = true;
                if (ModOptions.Settings.Instance.cameraSnap)
                {
                    CameraJumper.TryJumpAndSelect(p);
                }
            }
            Destroy(DestroyMode.Vanish);
        }

        public void ApplyDashDamage()
        {
            DamageInfo dinfo = new DamageInfo(TMDamageDefOf.DamageDefOf.TM_PsionicInjury, Rand.Range(6,10) * (1 + .1f * pwrVal) * arcaneDmg, 0, (float)-1, pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);

            bool flag3 = Position != default(IntVec3);
            if (flag3)
            {
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 intVec = Position + GenAdj.AdjacentCells[i];
                    Pawn cleaveVictim = new Pawn();
                    cleaveVictim = intVec.GetFirstPawn(Map);
                    if (cleaveVictim != null && cleaveVictim.Faction != pawn.Faction)
                    {
                        cleaveVictim.TakeDamage(dinfo);
                        FleckMaker.ThrowMicroSparks(cleaveVictim.Position.ToVector3(), Map);
      
                        //System.Random random = new System.Random();
                        //int rnd = GenMath.RoundRandom(random.Next(0, 100));
                        //if (rnd < (pwrVal * 5))
                        //{
                        //    cleaveVictim.TakeDamage(dinfo2);
                        //    FleckMaker.ThrowMicroSparks(cleaveVictim.Position.ToVector3(), base.Map);
                        //}
                    }
                }
            }

        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {
                if (shouldDrawPawn)
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
