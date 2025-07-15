using RimWorld;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_EyeOfTheStorm : Projectile
    {

        private static readonly Color lightningColor = new Color(160f, 160f, 160f);
        private static readonly Material lightningMat = MaterialPool.MatFrom("Spells/LightningBolt_w", false);
        private static readonly Material OrbMat = MaterialPool.MatFrom("Spells/eyeofthestorm", false);

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int searchDelay = 210;
        private int maxStrikeDelay = 90;
        private int maxStrikeDelayBldg = 50;
        private int lastStrike;
        private int lastStrikeBldg;
        private int age = -1;
        private float arcaneDmg = 1;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        private IntVec3[] from = new IntVec3[10];
        private Vector3[] to = new Vector3[10];
        private int[] fadeTimer = new int[10];

        private int pwrVal;
        private int verVal;

        protected float speed = 5.5f;
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
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
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
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_EyeOfTheStorm, false);
            pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_EyeOfTheStorm, false);
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
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
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
                    SearchForTargets(Position, 8f + (1 * verVal));
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
            float xOffset = verVal * Rand.Range(-0.6f, 0.6f);
            float zOffset = verVal * Rand.Range(-0.6f, 0.6f);
            orbVec.x += xOffset;
            orbVec.z += zOffset;
            FleckMaker.ThrowLightningGlow(orbVec, map, 0.5f + (0.15f * (float)verVal));
            float num = Mathf.Lerp(1.2f, 1.55f, 5f);            
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float angle = (float)Rand.Range(0, 360);
            Vector3 s = new Vector3(0.8f + (.1f* ((float)pwrVal)), 0.8f + (.1f * ((float)pwrVal)), 0.8f + (.1f * ((float)pwrVal)));
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, OrbMat, 0);  
        }

        public void SearchForTargets(IntVec3 center, float radius)
        {            
            Pawn curPawnTarg = null;
            Building curBldgTarg = null;
            //IntVec3 curCell;
            //IEnumerable <IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            //for (int i = 0; i < targets.Count(); i++)
            //{
            //    curCell = targets.ToArray<IntVec3>()[i];
            //    if ( curCell.InBoundsWithNullCheck(base.Map) && curCell.IsValid)
            //    {
            //        curPawnTarg = curCell.GetFirstPawn(base.Map);
            //        curBldgTarg = curCell.GetFirstBuilding(base.Map);
            //    }               
            List<Pawn> targets = new List<Pawn>();
            targets.Clear();
            if (age > lastStrike)
            {
                targets = TM_Calc.FindAllPawnsAround(Map, center, radius);
                if (targets != null && targets.Count > 0)
                {
                    curPawnTarg = targets.RandomElement();
                    if (curPawnTarg != launcher)
                    {
                        CellRect cellRect = CellRect.CenteredOn(curPawnTarg.Position, 2);
                        cellRect.ClipInsideMap(Map);
                        DrawStrike(center, curPawnTarg.Position.ToVector3());
                        for (int k = 0; k < Rand.Range(1, 8); k++)
                        {
                            IntVec3 randomCell = cellRect.RandomCell;
                            ExplosionHelper.Explode(randomCell, Map, Rand.Range(.4f, .8f), TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(5 + pwrVal, 9 + 3 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OnMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                        }
                        ExplosionHelper.Explode(curPawnTarg.Position, Map, 1f, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(5 + pwrVal, 9 + 3 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                        lastStrike = age + (maxStrikeDelay - (int)Rand.Range(0 + (pwrVal * 20), 50 + (pwrVal * 10)));
                    }
                    else
                    {
                        lastStrike += 10;
                    }
                }
                else
                {
                    lastStrike += 10;
                }
            }                

            if (age > lastStrikeBldg)
            {
                List<Building> list = new List<Building>();
                list.Clear();
                list = (from x in Map.listerThings.AllThings
                        where x.Position.InHorDistOf(center, radius) && !x.DestroyedOrNull() && x is Building
                        select x as Building).ToList<Building>();
                if (list.Count > 0)
                {
                    curBldgTarg = list.RandomElement();
                    CellRect cellRect = CellRect.CenteredOn(curBldgTarg.Position, 1);
                    cellRect.ClipInsideMap(Map);
                    DrawStrike(center, curBldgTarg.Position.ToVector3());
                    for (int k = 0; k < Rand.Range(1, 8); k++)
                    {
                        IntVec3 randomCell = cellRect.RandomCell;
                        ExplosionHelper.Explode(randomCell, Map, Rand.Range(.2f, .6f), TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(3 + 3 * pwrVal, 7 + 5 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);

                    }
                    ExplosionHelper.Explode(curBldgTarg.Position, Map, 1f, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(10 + 3 * pwrVal, 25 + 5 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                    lastStrikeBldg = age + (maxStrikeDelayBldg - (int)Rand.Range(0 + (pwrVal * 10), (pwrVal * 15)));
                }
                else
                {
                    lastStrikeBldg += 10;
                }
            }

            //}
            DrawStrikeFading();
            age++;
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
                    ExplosionHelper.Explode(Position, Map, 0.9f, DamageDefOf.Stun, this, -1, 0, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                }
            }

            List<IntVec3> dissipationList = GenRadial.RadialCellsAround(ExactPosition.ToIntVec3(), 12, false).ToList();
            for (int i = 0; i < (15 + 4*pwrVal); i++)
            {
                IntVec3 strikeCell = dissipationList.RandomElement();
                if (strikeCell.InBoundsWithNullCheck(Map) && strikeCell.IsValid && !strikeCell.Fogged(Map))
                {
                    DrawStrike(ExactPosition.ToIntVec3(), strikeCell.ToVector3Shifted());
                    for (int k = 0; k < Rand.Range(1, 8); k++)
                    {
                        CellRect cellRect = CellRect.CenteredOn(strikeCell, 1);
                        cellRect.ClipInsideMap(Map);
                        IntVec3 randomCell = cellRect.RandomCell;
                        ExplosionHelper.Explode(randomCell, Map, Rand.Range(.2f, .6f), TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(3 + 3 * pwrVal, 7 + 5 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);
                    }
                }
            }
            ExplosionHelper.Explode(ExactPosition.ToIntVec3(), Map, 3f + verVal, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, Mathf.RoundToInt(Rand.Range(5 + 5 * pwrVal, 10 + 10 * pwrVal) * arcaneDmg), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.1f, true);


            Destroy(DestroyMode.Vanish);
        }        
    }
}
