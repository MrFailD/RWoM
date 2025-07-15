using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_SpiritOfLight : Projectile
    {

        protected new Vector3 origin;
        protected new Vector3 destination;

        private CompProperties_Glower gProps = new CompProperties_Glower();
        private CompGlower glower = new CompGlower();
        public IntVec3 glowCenter = default(IntVec3);

        private static readonly Material sol_up = MaterialPool.MatFrom("PawnKind/sol_up", ShaderDatabase.Transparent, Color.white);
        private static readonly Material sol_side = MaterialPool.MatFrom("PawnKind/sol_side", ShaderDatabase.Transparent, Color.white);
        private static readonly Material sol_down = MaterialPool.MatFrom("PawnKind/sol_down", ShaderDatabase.Transparent, Color.white);
        private static readonly Material sol_blade = MaterialPool.MatFrom("Motes/LightShield", ShaderDatabase.Transparent, Color.white);
        private static readonly Material sol_barrier = MaterialPool.MatFrom("Other/LightBarrier", ShaderDatabase.Transparent, Color.white);

        private int age = -1;
        private int nextMoteTick;

        private int drawEnum;

        protected float speed = 9f;
        protected float speed_hover = .25f;
        protected float speed_dash = 18f;
        protected float speed_jog = 9f;
        protected float speed_walk = 5f;
        protected new int ticksToImpact;

        //protected new Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        private Pawn pawn;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion;
        public int timesToDamage = 3;
        public float arcaneDmg = 1f;
        public int pwrVal;
        public int verVal;
        public int effVal;
        private int actionDelay = 4;
        private int chargeFrequency = 66;
        public int delayCount;
        private int doublesidedVariance;
        private int destinationCurvePoint;
        public bool spinning = false;
        public float curveVariance; // 0 = no curve
        private List<Vector3> curvePoints = new List<Vector3>();

        private float lightEnergy = 1f;
        private bool initialized;
        public bool shouldDismiss = false;
        public bool shouldGlow;
        private bool glowing;

        public SoLAction solAction = SoLAction.Pending;
        public SoLAction queuedAction = SoLAction.Null;

        public bool IsGlowing
        {
            get
            {
                return glowing;
            }
        }

        public float AttackTarget_MaxRange
        {
            get
            {
                float val = 40 + pwrVal;
                return val;
            }
        }

        public float AttackTarget_ShortRange
        {
            get
            {
                float val = 10;
                return val;
            }
        }

        public int ActionDelay
        {
            get
            {
                int dec = 0;
                if(pwrVal > 7)
                {
                    dec--;
                    if(pwrVal > 14)
                    {
                        dec--;
                    }
                }
                return (actionDelay - dec);
            }
        }

        public float ActualLightCost(float baseCost)
        {
            float cost = (baseCost * (1f - (.01f * effVal)));
            LightEnergy -= cost;
            return cost;
        }

        public float LightEnergyMax
        {
            get
            {
                float val = 100f + (4f * verVal);
                return val;
            }
        }

        public float LightPotency
        {
            get
            {
                float val = 1f * (1f + (.03f * pwrVal)) * arcaneDmg;
                return val;
            }
        }

        public float LightEnergy
        {
            get
            {
                return lightEnergy;
            }
            set
            {
                lightEnergy = Mathf.Clamp(value, 0f, LightEnergyMax);
            }
        }

        public float ChargeAmount
        {
            get
            {
                float val = .012f * (1f + (.03f*verVal));
                if (pawn.Map != null)
                {
                    if(pawn.Map.weatherManager?.curWeather?.defName != "Clear")
                    {
                        val *= .8f;
                    }
                    if(pawn.Map.GameConditionManager?.ActiveConditions?.Count > 0)
                    {
                        List<GameCondition> gcList = pawn.Map.GameConditionManager.ActiveConditions;
                        for(int i =0; i < gcList.Count;i++)
                        {
                            if(gcList[i].def == GameConditionDefOf.Aurora)
                            {
                                val += .004f;
                            }
                            if(gcList[i].def == GameConditionDefOf.VolcanicWinter)
                            {
                                val *= .8f;
                            }
                            if(gcList[i].def == GameConditionDefOf.ToxicFallout)
                            {
                                val *= .85f;
                            }
                            if(gcList[i].def == TorannMagicDefOf.SolarFlare)
                            {
                                val *= 1.5f;
                            }
                            if(gcList[i].def == GameConditionDefOf.Eclipse)
                            {
                                val *= .7f;
                            }
                            if(gcList[i].def == TorannMagicDefOf.DarkClouds)
                            {
                                val *= .8f;
                            }
                        }
                    }
                    if (pawn.Position.Roofed(pawn.Map))
                    {
                        val -= .006f;
                    }
                    int mapTime = GenLocalDate.HourOfDay(pawn.Map);
                    if (mapTime < 20 && mapTime > 5)
                    {
                        float amt = 0;
                        if (mapTime >= 13)
                        {
                            amt =((float)Mathf.Abs(24f - mapTime) * val);
                        }
                        else if (mapTime <= 11)
                        {
                            amt = ((float)Mathf.Abs(mapTime) * val);
                        }
                        else
                        {
                            amt = (val * 12f);
                        }
                        return (amt / 1.3f);
                    }
                    return ((val * 2f) - .05f);
                }
                return (val * 2f);
            }
        }

        public bool EnergyChance
        {
            get
            {
                return Rand.Chance(LightEnergy / 100f);
            }
        }

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
        
        public Vector3 GetCasterOffset_Exact
        {
            get
            {
                Vector3 casterPos = pawn.DrawPos;
                if(solAction == SoLAction.Sleeping)
                {
                    return casterPos;
                }
                casterPos.x += -.55f;
                casterPos.z += .55f;
                return casterPos;
            }
        }

        public Vector3 GetCasterOffset_Rand
        {
            get
            {
                Vector3 casterPosOffset = GetCasterOffset_Exact;
                if(solAction == SoLAction.Sleeping)
                {
                    casterPosOffset.x += Rand.Range(-.2f, .2f);
                    casterPosOffset.z += Rand.Range(-.2f, .2f);
                    return casterPosOffset;
                }
                casterPosOffset.x += Rand.Range(-.2f, .1f);
                casterPosOffset.z += Rand.Range(-.15f, .25f);
                return casterPosOffset;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<Vector3>(ref origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref timesToDamage, "timesToDamage", 0, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            //Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            //Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
            Scribe_Values.Look<SoLAction>(ref solAction, "solAction", SoLAction.Pending, false);
            Scribe_Values.Look<SoLAction>(ref queuedAction, "queuedAction", SoLAction.Null, false);
            Scribe_Values.Look<float>(ref lightEnergy, "lightEnergy", 0f, false);
            Scribe_Values.Look<IntVec3>(ref glowCenter, "glowCenter", default(IntVec3), false);
            Scribe_Values.Look<bool>(ref glowing, "glowing", false, false);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag && solAction != SoLAction.Sleeping && solAction != SoLAction.Limbo)
            {
                Material mat = sol_down;
                if (drawEnum == 1)
                {
                    drawEnum = -1;
                    mat = sol_up;
                }
                else if (drawEnum == 0)
                {
                    drawEnum = 1;
                    mat = sol_side;
                }
                else
                {
                    drawEnum = 0;
                }

                Vector3 vector = DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                Vector3 s = new Vector3(.4f, pawn.DrawPos.y, .4f);
                Matrix4x4 matrix = default(Matrix4x4);
                Quaternion q = Quaternion.AngleAxis(0f, Vector3.up);
                if (solAction == SoLAction.Attacking || solAction == SoLAction.Returning || solAction == SoLAction.ChargeAttacking || solAction == SoLAction.Goto || solAction == SoLAction.Circling)
                {
                    q = (Vector3Utility.ToAngleFlat(DrawPos - destination) - 90).ToQuat();
                }
                if(solAction == SoLAction.ChargeAttacking)
                {
                    Matrix4x4 matrix2 = default(Matrix4x4);
                    float bScale = Mathf.Clamp(.6f + (5f / (float)ticksToImpact), .6f, 2.5f);
                    Vector3 s2 = new Vector3(bScale, pawn.DrawPos.y, bScale);
                    matrix2.SetTRS(vector, q, s2);
                    Graphics.DrawMesh(MeshPool.plane10, matrix2, sol_blade, 0);
                }
                if(solAction == SoLAction.Guarding && assignedTarget != null)
                {
                    Vector3 barrierPos = assignedTarget.DrawPos;                    
                    Quaternion barrierRot = Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up);
                    Matrix4x4 matrix2 = default(Matrix4x4);
                    float bScale = 1.4f;
                    Vector3 s2 = new Vector3(bScale, pawn.DrawPos.y, bScale);
                    matrix2.SetTRS(barrierPos, barrierRot, s2);
                    Graphics.DrawMesh(MeshPool.plane10, matrix2, sol_barrier, 0);

                }
                matrix.SetTRS(vector, q, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
            
            Comps_PostDraw();
        }

        public void DrawMotes()
        {
            nextMoteTick = age + Rand.Range((int)(55 - Mathf.Clamp((LightEnergy / 2), 0, 55)), (int)(65 - Mathf.Clamp((LightEnergy / 2),0, 60)));
            Vector3 rndVec = ExactPosition;
            rndVec.x += Rand.Range(-.1f, .1f);
            rndVec.z += Rand.Range(-.1f, .1f);
            //Vector3 angle = TM_Calc.GetVector(rndVec, this.ExactPosition);
            ThingDef mote = TorannMagicDefOf.Mote_Twinkle;
            int directionAngle = 0;
            float moteSpeed = Rand.Range(.25f, .6f);
            if (solAction == SoLAction.Sleeping)
            {
                directionAngle = Rand.Range(170, 190);
            }
            if(solAction == SoLAction.ChargeAttacking)
            {
                directionAngle = (int)Vector3Utility.ToAngleFlat(DrawPos - destination) - 90;
                TM_MoteMaker.ThrowGenericMote(mote, rndVec, Map, Rand.Range(.4f, .6f), Rand.Range(.2f, .4f), Rand.Range(0f, .5f), Rand.Range(.3f, .5f), Rand.Range(-50, 50), moteSpeed, directionAngle, Rand.Range(0, 360));
            }
            if(solAction == SoLAction.Guarding)
            {
                Vector3 barrierPos = assignedTarget.DrawPos;
                barrierPos.x += Rand.Range(-.25f, .25f);
                barrierPos.z += Rand.Range(-.25f, .25f);
                TM_MoteMaker.ThrowEnchantingMote(barrierPos, assignedTarget.Map, .25f);
            }
            if(solAction == SoLAction.Circling)
            {
                directionAngle = (int)Vector3Utility.ToAngleFlat(DrawPos - assignedTarget.DrawPos) - 90;
                TM_MoteMaker.ThrowGenericMote(mote, rndVec, Map, Rand.Range(.4f, .6f), Rand.Range(.2f, .4f), Rand.Range(0f, .5f), Rand.Range(.3f, .5f), Rand.Range(-50, 50), Rand.Range(1f,2f), directionAngle, Rand.Range(0, 360));
            }
            TM_MoteMaker.ThrowGenericMote(mote, rndVec, Map, Rand.Range(.2f, .3f), Rand.Range(.2f, .4f), Rand.Range(0f, .5f), Rand.Range(.3f, .5f), Rand.Range(-50, 50), moteSpeed, directionAngle, Rand.Range(0, 360));
        }

        public void StopGlow()
        {
            if (glower != null && glower.parent != null && Map != null)
            {
                if (glowing)
                {
                    //this.Map.mapDrawer.MapMeshDirty(glowCenter, MapMeshFlag.Things);
                    Map.glowGrid.DeRegisterGlower(glower);
                    glowing = false;
                    glowCenter = default(IntVec3);
                }
            }
        }

        public void DoGlow()
        {
            if (glower != null && glower.parent != null && Map != null)
            {
                //Log.Message("glower not null, glow center " + glowCenter);
                if (glowCenter != default(IntVec3) && pawn.Spawned)
                {
                    Map.mapDrawer.MapMeshDirty(glowCenter, MapMeshFlagDefOf.Things);
                    GlowGrid gg = Map.glowGrid;
                    HashSet<CompGlower> litGlowers = Traverse.Create(root: gg).Field(name: "litGlowers").GetValue<HashSet<CompGlower>>();
                    litGlowers.Add(glower);
                    Traverse.Create(root: gg).Field(name: "litGlowers").SetValue(litGlowers);
                    gg.DirtyCell(glowCenter);
                    if (Current.ProgramState != ProgramState.Playing)
                    {
                        List<IntVec3> locs = Traverse.Create(root: gg).Field(name: "initialGlowerLocs").GetValue<List<IntVec3>>();
                        locs.Add(glowCenter);
                        Traverse.Create(root: gg).Field(name: "initialGlowerLocs").SetValue(locs);
                    }
                    //this.Map.glowGrid.RegisterGlower(glower);
                    glowing = true;
                }                
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!pawn.DestroyedOrNull())
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                comp.SoL = null;
            }
            base.Destroy(mode);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                FleckMaker.Static(origin, Map, FleckDefOf.ExplosionFlash, 1f);
                SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
                FleckMaker.ThrowDustPuff(origin, Map, Rand.Range(1.2f, 1.8f));
                UpdateSoLPower();
                glower = new CompGlower();
                gProps.glowColor = new ColorInt(255, 255, 180, 1);
                gProps.glowRadius = 14f;
                gProps.overlightRadius = 12f;
                glower.parent = this;
                glower.Initialize(gProps);
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                Map.glowGrid.DeRegisterGlower(glower);
                glowing = false;
                //this.shouldGlow = false;
                initialized = true;
                //Log.Message("initializing sol, glowing: " + glowing + " glow center: " + glowCenter);
            }            
        }

        private void UpdateSoLPower()
        {
            if(!pawn.DestroyedOrNull() && pawn.Spawned)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if(comp != null && comp.IsMagicUser)
                {
                    //pwrVal = TM_Calc.GetMagicSkillLevel(pawn, comp.MagicData.MagicPowerSkill_SpiritOfLight, "TM_SpiritOfLight", "_pwr");
                    //effVal = TM_Calc.GetMagicSkillLevel(pawn, comp.MagicData.MagicPowerSkill_SpiritOfLight, "TM_SpiritOfLight", "_eff");
                    //verVal = TM_Calc.GetMagicSkillLevel(pawn, comp.MagicData.MagicPowerSkill_SpiritOfLight, "TM_SpiritOfLight", "_ver");
                    pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_SpiritOfLight, false);
                    verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_SpiritOfLight, false);
                    effVal = TM_Calc.GetSkillEfficiencyLevel(pawn, TorannMagicDefOf.TM_SpiritOfLight, false);
                    arcaneDmg = comp.arcaneDmg;
                }
            }
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
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            Initialize();
            this.origin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            flyingThing.ThingID += Rand.Range(0, 2147).ToString();
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            UpdateAction();
            ticksToImpact = StartingTicksToImpact;            
        }

        protected override void Tick()
        {
            age++;
            Vector3 exactPosition = ExactPosition;
            ticksToImpact--;

            if(!initialized)
            {
                Initialize();
                if(glowing && glowCenter != default(IntVec3))
                {
                    shouldGlow = true;
                }
            }

            if(Find.TickManager.TicksGame % chargeFrequency == 0)
            {
                LightEnergy += ChargeAmount;
            }

            bool dFlag = false;
            if(pawn.DestroyedOrNull())
            {
                dFlag = true;
                Destroy(DestroyMode.Vanish);
            }
            else if (!pawn.Spawned || pawn.Map != Map || pawn.Map == null)
            {
                solAction = SoLAction.Limbo;
                dFlag = true;
                delayCount++;
                if (delayCount >= 300)
                {
                    StopGlow();
                    shouldGlow = false;
                    Destroy(DestroyMode.Vanish);
                }            
                if(delayCount == 10)
                {
                    FleckMaker.ThrowLightningGlow(ExactPosition, Map, 1f);
                    FleckMaker.ThrowLightningGlow(ExactPosition, Map, .7f);
                    FleckMaker.ThrowLightningGlow(ExactPosition, Map, .4f);
                }
            }
            else if(shouldDismiss)
            {
                if(glowing)
                {
                    glowCenter = default(IntVec3);
                    StopGlow();
                    Destroy(DestroyMode.Vanish);
                }
            }

            if (!dFlag)
            {                
                bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
                if (flag)
                {
                    if (!pawn.DestroyedOrNull() && pawn.Spawned)
                    {
                        Position = pawn.Position;
                        destination = GetCasterOffset_Exact;
                        speed = speed_dash;
                        ticksToImpact = StartingTicksToImpact;
                    }
                    ticksToImpact++;
                    Position = ExactPosition.ToIntVec3();

                }
                else if(solAction == SoLAction.Limbo)
                {
                    UpdateAction(solAction);
                }
                else
                {
                    Position = ExactPosition.ToIntVec3();
                    if(Find.TickManager.TicksGame % ActionDelay == 0)
                    {
                        DoAction();
                    }
                    bool flag2 = ticksToImpact <= 0;
                    if (flag2)
                    {
                        if (shouldDismiss)
                        {
                            bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                            if (flag3)
                            {
                                Position = DestinationCell;
                            }
                            ImpactSomething();
                        }
                        else
                        {
                            UpdateSoLPower();
                            if (curveVariance > 0)
                            {
                                if ((curvePoints.Count() - 1) > destinationCurvePoint)
                                {
                                    origin = curvePoints[destinationCurvePoint];
                                    destinationCurvePoint++;
                                    destination = curvePoints[destinationCurvePoint];
                                    ticksToImpact = StartingTicksToImpact;
                                }
                                else
                                {
                                    bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                                    if (flag3)
                                    {
                                        Position = DestinationCell;
                                    }
                                    origin = ExactPosition;
                                    solAction = SoLAction.Pending;                                           
                                    UpdateAction(solAction);
                                }
                            }
                            else
                            {
                                bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                                if (flag3)
                                {
                                    Position = DestinationCell;
                                }
                                if (solAction == SoLAction.ChargeAttacking)
                                {
                                    ActualLightCost(5f);
                                    int moteAngle = Mathf.RoundToInt(Vector3Utility.ToAngleFlat(origin - destination) - 90f);
                                    TM_Action.DamageEntities(assignedTarget, null, Rand.Range(10f, 15f) * LightPotency, TMDamageDefOf.DamageDefOf.TM_BurningLight, pawn);
                                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_LightShield, ExactPosition, Map, 1.5f, .1f, 0f, .2f, 0, 2f, moteAngle, moteAngle);
                                }

                                origin = destination;
                                solAction = SoLAction.Pending;
                                UpdateAction(solAction);
                            }                            
                        }
                    }

                    if (nextMoteTick <= age && solAction != SoLAction.Limbo)
                    {
                        DrawMotes();                        
                    }
                }
            }
        }

        public void DoAction()
        {
            if(solAction == SoLAction.Attacking && (curvePoints.Count - 5) <= destinationCurvePoint)
            {
                if(assignedTarget != null && TM_Calc.HasLoSFromTo(Position, assignedTarget, this, 0, AttackTarget_MaxRange) && EnergyChance && LightEnergy > 3f)
                {
                    ActualLightCost(3f);
                    TM_CopyAndLaunchProjectile.CopyAndLaunchThing(TorannMagicDefOf.Projectile_LightLaser, this, assignedTarget, assignedTarget, ProjectileHitFlags.All, null);
                }
            }
        }

        public void UpdateAction(SoLAction withAction = SoLAction.Pending)
        {
            SoLAction inAction = solAction;
            Vector3 destTarget = ExactPosition;
            curveVariance = 0;
            //Log.Message("action for " + solAction.ToString() + " queued action: " + queuedAction.ToString());
            if(queuedAction != SoLAction.Null)
            {
                solAction = queuedAction;
                queuedAction = SoLAction.Null;
            }
            if (solAction == SoLAction.Pending || solAction == SoLAction.Goto)
            {
                if (pawn.GetPosture() == PawnPosture.LayingInBed || pawn.CurJobDef == JobDefOf.LayDown)
                {
                    if (!pawn.Awake() && LightEnergy > 5f)
                    {
                        Action_Sleeping(out destTarget);
                    }
                    //pawn injured?
                }                
                else if (pawn.Drafted)
                {
                    delayCount++;
                    if (LightEnergy > 20 && EnergyChance && delayCount > 2)
                    {
                        Pawn p = TM_Calc.FindNearbyEnemy(pawn, (int)AttackTarget_ShortRange);
                        if (p != null && TM_Calc.HasLoSFromTo(pawn.Position, p, pawn, 0, AttackTarget_ShortRange))
                        {
                            Action_AttackCharge(p, out destTarget);
                            queuedAction = SoLAction.Limbo;
                        }
                        else
                        {
                            if (LightEnergy > 30 && delayCount > 6)
                            {
                                p = TM_Calc.FindNearbyEnemy(pawn.Position, pawn.Map, pawn.Faction, AttackTarget_MaxRange, AttackTarget_ShortRange);
                                if (p != null && TM_Calc.HasLoSFromTo(pawn.Position, p, pawn, 0, AttackTarget_MaxRange))
                                {
                                    Action_Attack(p, out destTarget);
                                    queuedAction = SoLAction.Returning;
                                }
                                else
                                {
                                    Action_Hover(out destTarget);
                                }
                            }
                            else
                            {
                                Action_Hover(out destTarget);
                            }
                        }
                    }
                    else
                    {
                        Action_Hover(out destTarget);
                    }
                }                
                else if (pawn.Downed && pawn.health.hediffSet.GetHediffsTendable().Count() > 0 && LightEnergy > 10)
                {
                    Action_GotoTarget(pawn.DrawPos, speed_jog, out destTarget);
                    assignedTarget = pawn;
                    queuedAction = SoLAction.Guarding;
                }
                else if (shouldGlow)
                {
                    shouldGlow = false;
                    Action_GotoTarget(glowCenter.ToVector3Shifted(), speed_jog, out destTarget);
                    queuedAction = SoLAction.Glow;
                }
                else if(LightEnergy > 20 && EnergyChance)
                {
                    Pawn p = TM_Calc.FindNearbyInjuredPawnOther(pawn, 10, 10f);
                    delayCount++;
                    if (delayCount > 1 && p != null && p.Downed && TM_Calc.HasLoSFromTo(pawn.Position, p, pawn, 0, 10) && p.health.hediffSet.GetHediffsTendable().Count() > 0)
                    {
                        Action_GotoTarget(p.DrawPos, speed_dash, out destTarget);
                        queuedAction = SoLAction.Guarding;
                        assignedTarget = p;
                    }
                    else if (LightEnergy > 60 && EnergyChance)
                    {                        
                        if (delayCount > 2)
                        {
                            p = TM_Calc.FindNearbyPawn(pawn, 10);
                            if (p != null && p.needs != null && TM_Calc.HasLoSFromTo(pawn.Position, p, pawn, 0, 10))
                            {
                                if ((pawn.CurJobDef.joyKind != null || pawn.CurJobDef == JobDefOf.Wait_Wander || pawn.CurJobDef == JobDefOf.GotoWander) && pawn.needs.joy.CurLevelPercentage < .5f)
                                {
                                    delayCount = 0;
                                    Action_JoyBurst(pawn.DrawPos, out destTarget);
                                }
                                else if (p.needs.joy != null && (p.CurJobDef.joyKind != null || p.CurJobDef == JobDefOf.Wait_Wander || p.CurJobDef == JobDefOf.GotoWander) && p.needs.joy.CurLevelPercentage < .5f)
                                {
                                    delayCount = 0;
                                    Action_JoyBurst(p.DrawPos, out destTarget);
                                }
                                else if (delayCount > 3 && p.needs.mood != null && p.needs.mood.thoughts != null && p.needs.mood.thoughts.memories != null && p.needs.mood.thoughts.memories.GetFirstMemoryOfDef(TorannMagicDefOf.TM_BrightDayTD) == null)
                                {
                                    delayCount = 0;
                                    Action_GotoTarget(p.DrawPos, speed_jog, out destTarget);
                                    assignedTarget = p;
                                    queuedAction = SoLAction.BrightenDay;
                                    //action circle target
                                }
                                else
                                {
                                    Action_Hover(out destTarget);
                                }
                            }
                            else
                            {
                                Action_Hover(out destTarget);
                            }
                        }
                        else
                        {
                            Action_Hover(out destTarget);
                        }
                    }
                    else
                    {
                        Action_Hover(out destTarget);
                    }
                }
                //else if(LightEnergy > 90 && (EnergyChance && EnergyChance && EnergyChance))
                //{
                //    Action_Clean(out destTarget);
                //}                
                else
                {
                    delayCount = 0;
                    Action_Hover(out destTarget);
                }
            }
            else if(solAction == SoLAction.BrightenDay)
            {
                if(assignedTarget != null && assignedTarget is Pawn p)
                {
                    ActualLightCost(6f);
                    p.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_BrightDayTD);
                    Action_CircleTarget(assignedTarget, out destTarget);
                    queuedAction = SoLAction.Returning;
                }
            }
            else if(solAction == SoLAction.Glow)
            {
                if (glowing)
                {
                    StopGlow();
                }
                else
                {
                    DoGlow();
                }
                Action_Return();
            }
            else if(solAction == SoLAction.Returning)
            {
                Action_Return();
            }
            else if(solAction == SoLAction.Guarding)
            {
                if(LightEnergy > 10 && assignedTarget != null && assignedTarget is Pawn)
                {
                    Pawn p = assignedTarget as Pawn;
                    if (p.health.hediffSet.GetHediffsTendable().Count() > 0)
                    {
                        ticksToImpact = 300;
                        ActualLightCost(2f);
                        CauterizeWounds(p);
                        queuedAction = SoLAction.Guarding;
                    }
                    else
                    {
                        Action_Hover(out destTarget);
                    }
                }
                else
                {
                    Action_Hover(out destTarget);
                }
            }
            else if(solAction == SoLAction.Limbo)
            {
                Action_FromLimbo(out destTarget);
            }

            if (solAction != SoLAction.Attacking && solAction != SoLAction.Returning && solAction != SoLAction.Guarding && solAction != SoLAction.Circling)
            {
                destination = destTarget;
                ticksToImpact = StartingTicksToImpact;
            }
            //Log.Message("ending update action, new action: " + solAction.ToString() + " speed: " + this.speed + " delay action: " + this.delayCount + " light energy: "+ LightEnergy);

            if(solAction != inAction && solAction != SoLAction.Hovering && solAction != SoLAction.Sleeping)
            {
                delayCount = 0;
            }
        }

        public void UpdateDestination()
        {
            origin = ExactPosition;
            Vector3 destTarget = destination;
            if (solAction == SoLAction.Hovering)
            {
                Action_Hover(out destTarget);
                destination = destTarget;
                ticksToImpact = StartingTicksToImpact;
            }
        }

        public void Action_Guard(Pawn target, out Vector3 destTarget)
        {
            speed = speed_walk;
            solAction = SoLAction.Guarding;
            assignedTarget = target;
            destTarget = target.DrawPos;           
        }

        public void Action_Attack(Pawn target, out Vector3 destTarget)
        {
            speed = speed_jog;
            solAction = SoLAction.Attacking;
            assignedTarget = target;
            destTarget = TM_Calc.GetVectorBetween(origin, target.DrawPos);
            CalculateCurvePoints(origin, destTarget, 40, 10);
            destinationCurvePoint++;
            destination = curvePoints[destinationCurvePoint];
            ticksToImpact = StartingTicksToImpact;
            delayCount = 0;
        }

        public void Action_AttackCharge(Pawn target, out Vector3 destTarget)
        {
            speed = speed_dash;
            solAction = SoLAction.ChargeAttacking;
            assignedTarget = target;
            destTarget = target.DrawPos;
        }

        public void Action_Return()
        {
            speed = speed_jog;
            solAction = SoLAction.Returning;
            CalculateCurvePoints(origin, GetCasterOffset_Exact, 20, 10);
            destinationCurvePoint++;
            destination = curvePoints[destinationCurvePoint];
            ticksToImpact = StartingTicksToImpact;
        }

        public void Action_Sleeping(out Vector3 destTarget)
        {
            speed = speed_hover;
            solAction = SoLAction.Sleeping;
            delayCount++;
            if(delayCount > 10 && EnergyChance && pawn.needs.mood != null && pawn.needs.mood.thoughts != null)
            {
                delayCount = 0;
                if (pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(TorannMagicDefOf.TM_PleasantDreamsTD) == null)
                {
                    ActualLightCost(5f);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.TM_PleasantDreamsTD);
                }
                if(pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDefOf.SleepDisturbed) != null)
                {
                    ActualLightCost(3f);
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleepDisturbed);
                }
            }
            destTarget = GetCasterOffset_Rand;
        }

        public void Action_FromLimbo(out Vector3 destTarget)
        {
            delayCount = 0;
            Position = pawn.Position;
            origin = GetCasterOffset_Exact;
            Action_Hover(out destTarget);
            FleckMaker.ThrowLightningGlow(GetCasterOffset_Exact, pawn.Map, .7f);
            FleckMaker.ThrowLightningGlow(ExactPosition, Map, 1f);
            FleckMaker.ThrowLightningGlow(ExactPosition, Map, .4f);
        }

        public void Action_Hover(out Vector3 destTarget)
        {            
            speed = speed_hover;
            solAction = SoLAction.Hovering;
            destTarget = GetCasterOffset_Rand;
            if ((origin - destTarget).magnitude > .5f)
            {
                speed = speed_walk;
                if((origin - destTarget).magnitude > 25f)
                {
                    Action_FromLimbo(out destTarget);
                }
            }
        }

        public void Action_JoyBurst(Vector3 center, out Vector3 destTarget)
        {
            ActualLightCost(8f);
            for(int i = 0; i < 15; i++)
            {
                Vector3 motePos = center;
                motePos.x += Rand.Range(-5f, 5f);
                motePos.z += Rand.Range(-5f, 5f);
                if (motePos.InBoundsWithNullCheck(Map))
                {
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Twinkle, motePos, Map, Rand.Range(.3f, .4f), Rand.Range(.3f, .6f), Rand.Range(0f, .3f), Rand.Range(.4f, .6f), Rand.Range(-50, 50), Rand.Range(2f, 4f), 0, Rand.Range(0, 360));
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(center.ToIntVec3(), Map, false), MaintenanceType.None);
            info.pitchFactor = 1.5f;
            info.volumeFactor = .8f;
            TorannMagicDefOf.TM_Gong.PlayOneShot(info);
            List<Pawn> affectedPawns = TM_Calc.FindAllPawnsAround(Map, center.ToIntVec3(), 5f, pawn.Faction, true);
            if(affectedPawns != null && affectedPawns.Count > 0)
            {
                for(int i = 0; i < affectedPawns.Count; i++)
                {
                    Pawn p = affectedPawns[i];
                    if(p.needs != null && p.needs.joy != null)
                    {
                        p.needs.joy.CurLevel += Rand.Range(.2f, .4f) * LightPotency;
                    }
                }
            }            
            Action_Hover(out destTarget);
        }

        public void Action_CircleTarget(Thing target, out Vector3 destTarget)
        {
            Vector3 drawCenter = target.DrawPos;
            Vector3 rndDir = Vector3Utility.FromAngleFlat(Rand.Range(0, 360));
            Vector3 startPos = drawCenter + (rndDir * 3f);
            Vector3 endPos = drawCenter + (rndDir * -3f);
            speed = speed_jog;
            solAction = SoLAction.Circling;
            destTarget = startPos;
            doublesidedVariance = 1;
            if(Rand.Chance(.5f))
            {
                doublesidedVariance = -1;
            }
            List<Vector3> totalCircle = new List<Vector3>();
            totalCircle.Clear();
            CalculateCurvePoints(startPos, endPos, 90, 10);
            totalCircle.AddRange(curvePoints);
            CalculateCurvePoints(endPos, startPos, 90, 10);
            totalCircle.AddRange(curvePoints);
            curvePoints = totalCircle;
            destinationCurvePoint++;
            destination = curvePoints[destinationCurvePoint];
            ticksToImpact = StartingTicksToImpact;
            delayCount = 0;
            doublesidedVariance = 0;
        }

        public void Action_GotoTarget(Vector3 target, float speed, out Vector3 destTarget)
        {
            destTarget = target;
            this.speed = speed;
            solAction = SoLAction.Goto;
        }

        public void Action_Clean(out Vector3 destTarget)
        {
            List<Thing> filthList = Map.listerFilthInHomeArea.FilthInHomeArea;
            Thing closestDirt = null;
            float dirtPos = 0;
            for (int i = 0; i < filthList.Count; i++)
            {
                if (closestDirt != null)
                {
                    float dirtDistance = (filthList[i].DrawPos - origin).magnitude;
                    if (dirtDistance < dirtPos)
                    {
                        dirtPos = dirtDistance;
                        closestDirt = filthList[i];
                    }
                }
                else
                {
                    closestDirt = filthList[i];
                    dirtPos = (filthList[i].DrawPos - origin).magnitude;
                }
            }
            if(closestDirt != null)
            {
                solAction = SoLAction.Cleaning;
                destTarget = closestDirt.DrawPos;
            }
            else
            {
                Action_Hover(out destTarget);
            }
        }

        private void ImpactSomething()
        {
            Impact(null);            
        }

        protected new void Impact(Thing hitThing)
        {
            Destroy(DestroyMode.Vanish);
        }


        public void CalculateCurvePoints(Vector3 start, Vector3 end, float variance, int variancePoints)
        {
            curvePoints.Clear();
            curveVariance = variance;
            destinationCurvePoint = 0;
            Vector3 initialVector = GetVector(start, end);
            initialVector.y = 0;
            float initialAngle = (initialVector).ToAngleFlat(); //Quaternion.AngleAxis(90, Vector3.up) *
            float curveAngle = variance;
            if (doublesidedVariance == 0 && Rand.Chance(.5f))
            {
                curveAngle = (-1) * variance;
            }
            else
            {
                curveAngle = (doublesidedVariance * variance);
            }

            //calculate extra distance bolt travels around the ellipse
            float a = .5f * Vector3.Distance(start, end);
            float b = a * Mathf.Sin(.5f * Mathf.Deg2Rad * variance);
            float p = .5f * Mathf.PI * (3 * (a + b) - (Mathf.Sqrt((3 * a + b) * (a + 3 * b))));

            float incrementalDistance = p / variancePoints;
            float incrementalAngle = (curveAngle / variancePoints) * 2f;
            curvePoints.Add(start);
            for (int i = 1; i <= (variancePoints + 1); i++)
            {
                curvePoints.Add(curvePoints[i - 1] + ((Quaternion.AngleAxis(curveAngle, Vector3.up) * initialVector) * incrementalDistance)); //(Quaternion.AngleAxis(curveAngle, Vector3.up) *
                curveAngle -= incrementalAngle;
            }
            
        }

        public Vector3 GetVector(Vector3 center, Vector3 objectPos)
        {
            Vector3 heading = (objectPos - center);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public void CauterizeWounds(Pawn p)
        {
            int iCount = 1;
            for (int i = 0; i < iCount; i++)
            {
                Hediff hi = p.health.hediffSet.GetHediffsTendable().RandomElement();
                if (hi != null)
                {
                    float tendQuality = Rand.Range(.3f, .6f) * LightPotency;
                    hi.Tended(tendQuality, 1f);
                }
                else
                {
                    i = iCount;
                }
            }
        }
    }

    public enum SoLAction
    {
        Limbo,
        Goto,
        Glow,
        Returning,
        Flirting,
        Hovering,
        Sleeping,
        Circling,
        Attacking,
        ChargeAttacking,
        Guarding,
        Cleaning,
        Assisting,
        BrightenDay,
        Pending,
        Null
    }
}
