using RimWorld;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_ValiantCharge : Projectile
    {

        private static readonly Color wingColor = new Color(160f, 160f, 160f);
        private static readonly Material wingsNS = MaterialPool.MatFrom("Other/angelwings_up", ShaderDatabase.Transparent, wingColor);
        private static readonly Material wingsE = MaterialPool.MatFrom("Other/angelwings_up_east", ShaderDatabase.Transparent, wingColor);
        private static readonly Material wingsW = MaterialPool.MatFrom("Other/angelwings_up_west", ShaderDatabase.Transparent, wingColor);

        protected new Vector3 origin;

        protected new Vector3 destination;

        protected float speed = 55f;

        protected new int ticksToImpact;

        protected Thing assignedTarget;

        protected Thing flyingThing;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        public int timesToDamage = 3;

        public int weaponDmg = 0;

        private bool initialize = true;
        private int wingDelay;
        private bool wingDisplay = true;

        private IntVec3 expCell1;
        private IntVec3 expCell2;
        private float hyp;
        private float angleRad;
        private float angleDeg;
        private float xProb;

        private bool xflag = false;
        private bool zflag = false;

        private Pawn pawn;
        private CompAbilityUserMagic comp;
        private MagicPowerSkill pwr;
        private MagicPowerSkill ver;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;

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
            Scribe_Values.Look<int>(ref timesToDamage, "timesToDamage", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            //Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 12f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
                expCell1 = DestinationCell;
                expCell2 = DestinationCell;
                XProb(DestinationCell, origin);
            }
            //flyingThing.ThingID += Rand.Range(0, 2147).ToString();
            initialize = false;
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
            Hediff invul = new Hediff();
            invul.def = TorannMagicDefOf.TM_HediffInvulnerable;
            invul.Severity = .2f;
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            pawn.health.AddHediff(invul, null, null);
            comp = pawn.GetCompAbilityUserMagic();
            pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_ValiantCharge);
            verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_ValiantCharge);
            //pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_ValiantCharge.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ValiantCharge_pwr");
            //ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_ValiantCharge.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ValiantCharge_ver");
            //
            //pwrVal = pwr.level;
            //verVal = ver.level;
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
            //
            ModOptions.Constants.SetPawnInFlight(true);
            //
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
                    if (wingDisplay)
                    {
                        if (wingDelay < 5)
                        {
                            DrawWings(DrawPos, pawn, 10);
                            wingDelay++;
                        }
                        else
                        {
                            wingDelay = 0;
                            wingDisplay = false;

                        }                        
                    }
                    else
                    {
                        if (wingDelay < 3)
                        {
                            wingDelay++;
                        }
                        else
                        {
                            wingDelay = 0;
                            wingDisplay = true;
                        }
                    }
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
                Comps_PostDraw();
            }
        }

        private void DrawWings(Vector3 pawnVec, Pawn flyingPawn, int magnitude)
        {
            bool flag = !pawn.Dead && !pawn.Downed;
            if (flag)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, magnitude);
                Vector3 vector = pawnVec;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(3f, 3f, 3f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                if (flyingPawn.Rotation == Rot4.South || flyingPawn.Rotation == Rot4.North)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, wingsNS, 0);
                }
                if (flyingPawn.Rotation == Rot4.East)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, wingsE, 0);
                }
                if (flyingPawn.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, wingsW, 0);
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
            try
            {
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FireExplosion(pwrVal, verVal, Position, Map, (1.2f + (float)(verVal * .8f)));
                if (!pawn.IsColonist)
                {
                    FireExplosion(3, 3, Position, Map, (1.2f + (float)(3 * .8f)));
                }
                FleckMaker.ThrowSmoke(pawn.Position.ToVector3(), Map, (0.8f + (float)(verVal * .8f)));

                for (int i = 0; i < (2 + verVal); i++)
                {
                    expCell1 = GetNewPos(expCell1, origin.x <= DestinationCell.x, origin.z <= DestinationCell.z, false, 0, 0, xProb, 1 - xProb);
                    FleckMaker.ThrowSmoke(expCell1.ToVector3(), Map, 1.6f);
                    expCell2 = GetNewPos(expCell2, origin.x <= DestinationCell.x, origin.z <= DestinationCell.z, false, 0, 0, 1 - xProb, xProb);
                    FleckMaker.ThrowSmoke(expCell2.ToVector3(), Map, 1.6f);
                }
                for (int i = 0; i < (4 + (3 * verVal)); i++)
                {
                    CellRect cellRect = CellRect.CenteredOn(expCell1, (1 + verVal));
                    cellRect.ClipInsideMap(Map);
                    IntVec3 randomCell = cellRect.RandomCell;
                    FireExplosion(pwrVal, verVal, randomCell, Map, .4f);
                    cellRect = CellRect.CenteredOn(expCell2, (1 + verVal));
                    randomCell = cellRect.RandomCell;
                    FireExplosion(pwrVal, verVal, randomCell, Map, .4f);
                }

                GenSpawn.Spawn(flyingThing, Position, Map);

                ModOptions.Constants.SetPawnInFlight(false);
                Pawn p = flyingThing as Pawn;
                TM_Action.SearchAndTaunt(p, 3f, 5, .7f);
                HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_HediffShield, .25f);
                RemoveInvul(p);
                if (p.IsColonist)
                {
                    p.drafter.Drafted = true;
                    if (ModOptions.Settings.Instance.cameraSnap)
                    {
                        CameraJumper.TryJumpAndSelect(p);
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
            catch
            {
                GenSpawn.Spawn(flyingThing, Position, Map);
                ModOptions.Constants.SetPawnInFlight(false);
                Pawn p = flyingThing as Pawn;
                RemoveInvul(p);
                if (p.IsColonist)
                {
                    p.drafter.Drafted = true;
                }
                Destroy(DestroyMode.Vanish);
            }
        }

        protected void FireExplosion(int pwr, int ver, IntVec3 pos, Map map, float radius)
        {
            ThingDef def = this.def;

            Explosion(pwr, pos, map, radius, TMDamageDefOf.DamageDefOf.TM_Holy, pawn, null, def, ThingDefOf.Explosion, null, 0.3f, 1, false, null, 0f, 1);

            if (ver >= 2)
            {
                int stunProb = Rand.Range(1, 10);
                if (stunProb > (4 + ver))
                {
                    Explosion(pwr, pos, map, radius, DamageDefOf.Stun, pawn, null, def, ThingDefOf.Explosion, null, 0.3f, 1, false, null, 0f, 1);
                }
            }
            
        }

        public void Explosion(int pwr, IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
        {
            if (center.InBounds(map))
            {
                System.Random rnd = new System.Random();
                int modDamAmountRand = GenMath.RoundRandom(rnd.Next(8 + pwr, 15 + (4 * pwr)));
                modDamAmountRand = Mathf.RoundToInt(modDamAmountRand * arcaneDmg);
                if (map == null)
                {
                    Log.Warning("Tried to do explosion in a null map.");
                    return;
                }
                Explosion explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
                explosion.damageFalloff = false;
                explosion.chanceToStartFire = 0.0f;
                explosion.Position = center;
                explosion.radius = radius;
                explosion.damType = damType;
                explosion.instigator = instigator;
                explosion.damAmount = ((projectile == null) ? GenMath.RoundRandom((float)damType.defaultDamage) : modDamAmountRand);
                explosion.weapon = source;
                explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
                explosion.preExplosionSpawnChance = preExplosionSpawnChance;
                explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
                explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
                explosion.postExplosionSpawnChance = postExplosionSpawnChance;
                explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
                explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
                explosion.StartExplosion(explosionSound, null);
            }
        }

        private void XProb(IntVec3 target, Vector3 origin)
        {
            hyp = Mathf.Sqrt((Mathf.Pow(origin.x - target.x, 2)) + (Mathf.Pow(origin.z - target.z, 2)));
            angleRad = Mathf.Asin(Mathf.Abs(origin.x - target.x) / hyp);
            angleDeg = Mathf.Rad2Deg * angleRad;
            xProb = angleDeg / 90;
        }

        private IntVec3 GetNewPos(IntVec3 curPos, bool xdir, bool zdir, bool halfway, float zvar, float xvar, float xguide, float zguide)
        {
            float rand = (float)Rand.Range(0, 100);
            bool flagx = rand <= ((xguide + Mathf.Abs(xvar)) * 100) && !xflag;
            bool flagz = rand <= ((zguide + Mathf.Abs(zvar)) * 100) && !zflag;

            if (halfway)
            {
                xvar = (-1 * xvar);
                zvar = (-1 * zvar);
            }

            if (xdir && zdir)
            {
                //top right
                if (flagx)
                {
                    if (xguide + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagz)
                {
                    if (zguide + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (xdir && !zdir)
            {
                //bottom right
                if (flagx)
                {
                    if (xguide + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagz)
                {
                    if ((-1 * zguide) + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (!xdir && zdir)
            {
                //top left
                if (flagx)
                {
                    if ((-1 * xguide) + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagz)
                {
                    if (zguide + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (!xdir && !zdir)
            {
                //bottom left
                if (flagx)
                {
                    if ((-1 * xguide) + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagz)
                {
                    if ((-1 * zguide) + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            else
            {
                //no direction identified
            }
            return curPos;
            //return curPos;
        }

        private void RemoveInvul(Pawn abilityUser)
        {
            List<Hediff> list = new List<Hediff>();
            List<Hediff> arg_32_0 = list;
            IEnumerable<Hediff> arg_32_1;
            if (abilityUser == null)
            {
                arg_32_1 = null;
            }
            else
            {
                Pawn_HealthTracker expr_1A = abilityUser.health;
                if (expr_1A == null)
                {
                    arg_32_1 = null;
                }
                else
                {
                    HediffSet expr_26 = expr_1A.hediffSet;
                    arg_32_1 = ((expr_26 != null) ? expr_26.hediffs : null);
                }
            }
            arg_32_0.AddRange(arg_32_1);
            Pawn expr_3E = abilityUser;
            int? arg_84_0;
            if (expr_3E == null)
            {
                arg_84_0 = null;
            }
            else
            {
                Pawn_HealthTracker expr_52 = expr_3E.health;
                if (expr_52 == null)
                {
                    arg_84_0 = null;
                }
                else
                {
                    HediffSet expr_66 = expr_52.hediffSet;
                    arg_84_0 = ((expr_66 != null) ? new int?(expr_66.hediffs.Count<Hediff>()) : null);
                }
            }
            bool flag = (arg_84_0 ?? 0) > 0;
            if (flag)
            {
                foreach (Hediff current in list)
                {
                    if (current.def.defName == "Burn")
                    {
                        current.Severity = -5;
                    }
                    if (current.def.defName == "TM_HediffInvulnerable")
                    {
                        current.Severity = -5;
                        return;
                    }
                }
            }
            list.Clear();
            list = null;
        }
    }
}
