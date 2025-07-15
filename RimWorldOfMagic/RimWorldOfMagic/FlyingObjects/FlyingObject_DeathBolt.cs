using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_DeathBolt : Projectile
    {

        private static readonly Color lightningColor = new Color(160f, 160f, 160f);
        private static readonly Material OrbMat = MaterialPool.MatFrom("Spells/deathbolt", false);

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int age = -1;
        private float arcaneDmg = 1;
        private bool powered;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        private bool reverseDirection;

        private int pwrVal;
        private int verVal;
        private float radius = 1.4f;

        protected float speed = 30f;
        protected new int ticksToImpact;
        private bool impacted;
        protected int ticksFollowingImpact;

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
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<float>(ref radius, "radius", 1.4f, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref impacted, "impacted", false, false);
            Scribe_Values.Look<bool>(ref powered, "powered", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            //Scribe_References.Look<Thing>(ref this.flyingThing, "flyingThing", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 12f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if (comp != null && comp.PowerModifier > 0)
                {
                    arcaneDmg += .2f;
                    comp.PowerModifier--;
                    powered = true;
                }
            }            
            flyingThing.ThingID += Rand.Range(0, 214).ToString();
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
                foreach (MagicPower current in comp.MagicData.MagicPowersN)
                {
                    if ((current.abilityDef == TorannMagicDefOf.TM_DeathBolt || current.abilityDef == TorannMagicDefOf.TM_DeathBolt_I || current.abilityDef == TorannMagicDefOf.TM_DeathBolt_II || current.abilityDef == TorannMagicDefOf.TM_DeathBolt_III))
                    {
                        if (current.level == 0)
                        {
                            radius = 1.4f;
                        }
                        else if (current.level == 1)
                        {
                            radius = 2f;
                        }
                        else if (current.level == 2)
                        {
                            radius = 2f;
                        }
                        else
                        {
                            radius = 2.4f;
                        }
                    }
                }
                arcaneDmg = comp.arcaneDmg;
                pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_DeathBolt, true);
                verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_DeathBolt, true);
                //MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_DeathBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_DeathBolt_pwr");
                //MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_DeathBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_DeathBolt_ver");
                //verVal = ver.level;
                //pwrVal = pwr.level;
                //
                //if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
                //{
                //    pwrVal = 1;
                //    verVal = 1;
                //}                
            }      
            else if (pawn.def == TorannMagicDefOf.TM_SkeletonLichR)
            {
                pwrVal = Rand.RangeInclusive(0, 3);
                verVal = Rand.RangeInclusive(0, 3);
                radius = 2f;
            }

            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.origin = origin;
            impactDamage = newDamageInfo;
            speed = def.projectile.speed;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            float distanceAccuracyModifier = (targ.Cell.ToVector3Shifted() - pawn.Position.ToVector3Shifted()).MagnitudeHorizontal() *.1f;
            destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-distanceAccuracyModifier, distanceAccuracyModifier), 0f, Rand.Range(-distanceAccuracyModifier, distanceAccuracyModifier));
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
            if (reverseDirection)
            {
                ticksToImpact++;
            }
            else
            {
                ticksToImpact--;
            }
            ticksFollowingImpact--;
            Position = ExactPosition.ToIntVec3();
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            if (flag)
            {
                ticksToImpact++;
                Destroy(DestroyMode.Vanish);
            }
            else if (!ExactPosition.ToIntVec3().Walkable(Map) && !ExactPosition.ToIntVec3().CanBeSeenOverFast(Map))
            {
                if (reverseDirection)
                {
                    Destroy(DestroyMode.Vanish);
                }
                else
                {
                    reverseDirection = true;
                    ImpactSomething();
                }
            }
            else
            {                                           
                bool flag2 = ticksToImpact <= 0 && !impacted;
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

            if(impacted)
            {
                if (ticksFollowingImpact > 0 && Find.TickManager.TicksGame % 5 == 0)
                {
                    CellRect cellRect = CellRect.CenteredOn(Position, 2);
                    cellRect.ClipInsideMap(Map);
                    IntVec3 spreadingDarknessCell;
                    if(!(cellRect.CenterCell.GetTerrain(Map).passability == Traversability.Impassable) && !cellRect.CenterCell.IsValid || !cellRect.CenterCell.InBoundsWithNullCheck(Map))
                    {
                        ticksFollowingImpact = -1;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        spreadingDarknessCell = cellRect.RandomCell;
                        if (spreadingDarknessCell.InBoundsWithNullCheck(Map) && spreadingDarknessCell.IsValid)
                        {
                            ExplosionHelper.Explode(spreadingDarknessCell, Map, .4f, TMDamageDefOf.DamageDefOf.TM_DeathBolt, pawn, Mathf.RoundToInt((Rand.Range(.4f * def.projectile.GetDamageAmount(1, null), .8f * def.projectile.GetDamageAmount(1, null)) + (3f * pwrVal)) * arcaneDmg), 2, def.projectile.soundExplode, def, null, null, null, 0f, 1, null, false, null, 0f, 0, 0.0f, true);
                            TM_MoteMaker.ThrowDiseaseMote(Position.ToVector3Shifted(), Map, .6f);
                            if (powered)
                            {
                                TM_MoteMaker.ThrowBoltMote(Position.ToVector3Shifted(), Map, 0.3f);
                            }
                        }
                    }
                }
                
                if(ticksFollowingImpact < 0)
                {
                    Destroy(DestroyMode.Vanish);
                }
            }
        }

        public void DrawEffects(Vector3 effectVec, Map map)
        {
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);
            TM_MoteMaker.ThrowDiseaseMote(effectVec, map, 0.4f, 0.1f, .01f, 0.35f);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null && !impacted;
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
                Pawn pawn;
                bool flag2 = (pawn = (Position.GetThingList(Map).FirstOrDefault((Thing x) => x == assignedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = pawn;
                }
            }        

            ExplosionHelper.Explode(Position, Map, radius, TMDamageDefOf.DamageDefOf.TM_DeathBolt, this.pawn, Mathf.RoundToInt((Rand.Range(.6f*def.projectile.GetDamageAmount(1,null), 1.1f*def.projectile.GetDamageAmount(1,null)) + (5f * pwrVal)) * arcaneDmg), 4, def.projectile.soundExplode, def, null, null, null, 0f, 1, null, false, null, 0f, 0, 0.0f, true);

            ticksFollowingImpact = verVal * 15;
            impacted = true;
        }        
    }
}
