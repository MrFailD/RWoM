using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_LightningTrap : Projectile
    {

        private static readonly Color lightningColor = new Color(160f, 160f, 160f);
        private static readonly Material lightningMat = MaterialPool.MatFrom("Spells/LightningBolt_w", false);
        private static readonly Material OrbMat = MaterialPool.MatFrom("Spells/eyeofthestorm", false);

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int searchDelay = 10;
        private int maxStrikeDelay = 100;
        private int maxStrikeDelayBldg = 60;
        private int lastStrike;
        private int lastStrikeBldg = 0;
        private int age = -1;
        private float arcaneDmg = 1;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        private IntVec3[] from = new IntVec3[10];
        private Vector3[] to = new Vector3[10];
        private int[] fadeTimer = new int[10];

        //private int pwrVal = 0;
        //private int verVal = 0;

        public float speed = .8f;
        protected new int ticksToImpact;

        protected Faction faction;
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
            Scribe_Values.Look<int>(ref timesToDamage, "timesToDamage", 0, false);
            Scribe_Values.Look<int>(ref searchDelay, "searchDelay", 210, false);
            //Scribe_Values.Look<int>(ref this.pwrVal, "pwrVal", 0, false);
            //Scribe_Values.Look<int>(ref this.verVal, "verVal", 0, false);
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
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 12f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
            }
            flyingThing.ThingID += Rand.Range(0, 214).ToString();
            initialized = false;
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, Faction faction, DamageInfo? newDamageInfo = null, float _speed = .8f)
        {
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            speed = _speed;
            //CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            //this.arcaneDmg = comp.arcaneDmg;
            //MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_EyeOfTheStorm.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EyeOfTheStorm_pwr");
            //MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_EyeOfTheStorm.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EyeOfTheStorm_ver");
            //verVal = ver.level;
            //pwrVal = pwr.level;
            //
            //if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            //{
            //    pwrVal = 1;
            //    verVal = 1;
            //}
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            this.origin = origin;
            this.faction = faction;
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
                DrawOrb(exactPosition, Map);
                if(searchDelay < 0)
                {
                    searchDelay = Rand.Range(20, 35);
                    SearchForTargets(origin.ToIntVec3(), 6f);
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

        public void DrawOrb(Vector3 orbVec, Map map)
        {
            Vector3 vector = orbVec;
            float xOffset = Rand.Range(-0.6f, 0.6f);
            float zOffset = Rand.Range(-0.6f, 0.6f);
            orbVec.x += xOffset;
            orbVec.z += zOffset;
            FleckMaker.ThrowLightningGlow(orbVec, map, 0.4f);
            float num = Mathf.Lerp(1.2f, 1.55f, 5f);            
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float angle = (float)Rand.Range(0, 360);
            Vector3 s = new Vector3(0.4f, 0.4f, 0.4f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, OrbMat, 0);  
        }

        public void SearchForTargets(IntVec3 center, float radius)
        {
            Pawn target = null;
            if (faction == null)
            {
                faction = Faction.OfPlayer;
            }
            target = TM_Calc.FindNearbyEnemy(center, Map, faction, radius, 0f);
            if (target != null)
            {
                CellRect cellRect = CellRect.CenteredOn(target.Position, 2);
                cellRect.ClipInsideMap(Map);
                DrawStrike(center, target.Position.ToVector3());
                for (int k = 0; k < Rand.Range(1, 5); k++)
                {
                    IntVec3 randomCell = cellRect.RandomCell;
                    ExplosionHelper.Explode(randomCell, Map, Rand.Range(.4f, .8f), TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(4, 6)), 0, SoundDefOf.Thunder_OnMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                }
                ExplosionHelper.Explode(target.Position, Map, 1f, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(5, 9) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                lastStrike = age;
            }            
            DrawStrikeFading();
        }

        public void DrawStrike(IntVec3 center, Vector3 dest)
        {
            TM_MeshBolt meshBolt = new TM_MeshBolt(center, dest, lightningMat);
            meshBolt.CreateBolt();
            for (int i = 0; i < 10; i++)
            {
                if (fadeTimer[i] <= 0)
                {
                    from[i] = center;
                    to[i] = dest;
                    fadeTimer[i] = 30;
                    i = 10;
                }
            }
        }

        public void DrawStrikeFading()
        {
            for(int i = 0; i < 10; i ++)
            {
                if (fadeTimer[i] > 0)
                {
                    TM_MeshBolt meshBolt = new TM_MeshBolt(from[i], to[i], lightningMat);
                    meshBolt.CreateFadedBolt(fadeTimer[i]/30);
                    fadeTimer[i]--;
                    if (fadeTimer[i] == 0)
                    {
                        from[i] = default(IntVec3);
                        to[i] = default(Vector3);
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
                    ExplosionHelper.Explode(origin.ToIntVec3(), Map, 0.9f, DamageDefOf.Stun, this, -1, 0, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                }
            }

            List<IntVec3> dissipationList = GenRadial.RadialCellsAround(origin.ToIntVec3(), 5, false).ToList();
            for (int i = 0; i < 4; i++)
            {
                IntVec3 strikeCell = dissipationList.RandomElement();
                if (strikeCell.InBoundsWithNullCheck(Map) && strikeCell.IsValid && !strikeCell.Fogged(Map))
                {
                    DrawStrike(ExactPosition.ToIntVec3(), strikeCell.ToVector3Shifted());
                    for (int k = 0; k < Rand.Range(1, 8); k++)
                    {
                        CellRect cellRect = CellRect.CenteredOn(strikeCell, 2);
                        cellRect.ClipInsideMap(Map);
                        IntVec3 randomCell = cellRect.RandomCell;
                        ExplosionHelper.Explode(randomCell, Map, Rand.Range(.2f, .6f), TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(2, 6)), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                    }
                }
            }
            ExplosionHelper.Explode(origin.ToIntVec3(), Map, 1f, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(4, 8)), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);


            Destroy(DestroyMode.Vanish);
        }        
    }
}
