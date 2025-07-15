using System.Reflection;
using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic.Buildings
{   
    [StaticConstructorOnStartup]
    public class Building_TechnoTurret : Building_TurretGun
    {
        private int mortarMaxRange = 180;
        private const int MortarMinRange = 40;
        private int mortarTicksToFire = 900;
        private float mortarManaCost = .08f;

        private const int RocketMinRange = 5;
        private int rocketTicksToFire = 600;
        private int rocketCount = 1;
        private int nextRocketFireTick;
        private float rocketManaCost = .04f;

        private int verVal;
        private int pwrVal;
        private int effVal;

        private int age;
        private int duration = 3600;

        private bool MannedByColonist => mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;
        private bool MannedByNonColonist => mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;
        private bool PlayerControlled => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;
        private bool WarmingUp => burstWarmupTicksLeft > 0;
        protected override bool CanSetForcedTarget => mannableComp != null && PlayerControlled;
        private bool CanToggleHoldFire => PlayerControlled;
        private bool initialized;
        private bool burstActivated;

        public IntVec3 Cell;
        public override IntVec3 InteractionCell => Cell;

        private CompAbilityUserMagic comp;
        public Pawn ManPawn;

        readonly FieldInfo holdFireField = typeof(Building_TurretGun).GetField("holdFire", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool TT_Active
        {
            get
            {
                if ((powerComp == null || powerComp.PowerOn) && (dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated))
                {
                    if (interactableComp != null)
                    {
                        return burstActivated;
                    }
                    return true;
                }
                return false;
            }
        }

        public override AcceptanceReport ClaimableBy(Faction by)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref mortarMaxRange, "mortarMaxRange", 80, false);
            Scribe_Values.Look<int>(ref mortarTicksToFire, "mortarTicksToFire", 900, false);
            Scribe_Values.Look<int>(ref rocketCount, "rocketCount", 1, false);
            Scribe_Values.Look<int>(ref rocketTicksToFire, "rocketTicksToFire", 600, false);
            Scribe_Values.Look<float>(ref rocketManaCost, "rocketManaCost", 0.05f, false);
            Scribe_Values.Look<float>(ref mortarManaCost, "mortarManaCost", 0.1f, false);
            Scribe_Values.Look(ref burstActivated, "burstActivated", false);
            Scribe_Values.Look<Pawn>(ref ManPawn, "manPawn");
            Scribe_Values.Look<IntVec3>(ref Cell, "iCell");
            Scribe_Values.Look<int>(ref age, "age", 0);
            Scribe_Values.Look<int>(ref duration, "duration", 3600);
        }

        protected override void Tick()
        {
            base.Tick();
            age++;
            //if (!manPawn.DestroyedOrNull() && !manPawn.Dead && !manPawn.Downed && manPawn.Position == this.InteractionCell)
            if (age <= duration)
            {
                if (!initialized)
                {
                    comp = ManPawn.GetCompAbilityUserMagic();
                    verVal = comp.MagicData.MagicPowerSkill_TechnoTurret.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoTurret_ver").level;
                    pwrVal = comp.MagicData.MagicPowerSkill_TechnoTurret.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoTurret_pwr").level;
                    effVal = comp.MagicData.MagicPowerSkill_TechnoTurret.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoTurret_eff").level;
                    duration = 3600 + (300 * effVal);
                    if (verVal >= 5)
                    {
                        rocketTicksToFire = 600 - ((verVal - 5) * 20);
                        rocketCount = verVal / 5;
                        rocketManaCost = .04f - (.001f * effVal);
                    }
                    if (verVal >= 10)
                    {
                        mortarTicksToFire = 900 - ((verVal - 10) * 40);
                        mortarMaxRange += ((verVal - 10) * 5);
                        mortarManaCost = .08f - (.002f * effVal);
                    }
                    initialized = true;
                }

                if (!ManPawn.DestroyedOrNull() && !ManPawn.Dead && !ManPawn.Downed && comp != null && comp.Mana != null)
                {
                    if (verVal >= 5 && nextRocketFireTick < Find.TickManager.TicksGame && TargetCurrentlyAimingAt != null && comp.Mana.CurLevel >= rocketManaCost)
                    {
                        if (TargetCurrentlyAimingAt.Cell.IsValid && TargetCurrentlyAimingAt.Cell.DistanceToEdge(Map) > 5 && (TargetCurrentlyAimingAt.Cell - Position).LengthHorizontal >= RocketMinRange)
                        {
                            bool flag = TargetCurrentlyAimingAt.Cell != default(IntVec3);
                            if (flag)
                            {
                                Thing launchedThing = new Thing()
                                {
                                    def = ThingDef.Named("FlyingObject_RocketSmall")
                                };
                                FlyingObject_Advanced flyingObject = (FlyingObject_Advanced)GenSpawn.Spawn(ThingDef.Named("FlyingObject_RocketSmall"), Position, Map);
                                flyingObject.AdvancedLaunch(this, TorannMagicDefOf.Mote_Base_Smoke, 1, Rand.Range(5, 25), false, DrawPos, TargetCurrentlyAimingAt.Cell, launchedThing, Rand.Range(32, 38), true, Mathf.RoundToInt(Rand.Range(22f, 30f) * comp.arcaneDmg), 2, TMDamageDefOf.DamageDefOf.TM_PersonnelBombDD, null);
                                rocketCount--;
                                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                                info.pitchFactor = 1.3f;
                                info.volumeFactor = 1.5f;
                                TorannMagicDefOf.TM_AirWoosh.PlayOneShot(info);
                            }
                            if (rocketCount <= 0)
                            {
                                rocketCount = verVal / 5;
                                nextRocketFireTick = Find.TickManager.TicksGame + (600 - ((verVal - 5) * 20));
                                comp.Mana.CurLevel -= rocketManaCost;
                                comp.MagicUserXP += Rand.Range(9, 12);
                            }
                            else
                            {
                                nextRocketFireTick = Find.TickManager.TicksGame + 20;
                            }
                        }
                    }

                    if (verVal >= 10 && mortarTicksToFire < Find.TickManager.TicksGame && comp.Mana.CurLevel >= mortarManaCost)
                    {
                        mortarTicksToFire = Find.TickManager.TicksGame + (900 - ((verVal - 10) * 40));
                        Pawn target = TM_Calc.FindNearbyEnemy(Position, Map, Faction, mortarMaxRange, MortarMinRange);
                        if (target != null && target.Position.DistanceToEdge(Map) > 8)
                        {
                            bool flag = target.Position != default(IntVec3);
                            if (flag)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    IntVec3 rndTarget = target.Position;
                                    rndTarget.x += Rand.RangeInclusive(-6, 6);
                                    rndTarget.z += Rand.RangeInclusive(-6, 6);
                                    Projectile newProjectile = (Projectile)GenSpawn.Spawn(ThingDef.Named("Bullet_Shell_TechnoTurretExplosive"), Position, Map, WipeMode.Vanish);
                                    newProjectile.Launch(this, rndTarget, target, ProjectileHitFlags.All, false, null);
                                }
                            }
                            SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                            info.pitchFactor = 1.3f;
                            info.volumeFactor = .8f;
                            SoundDef.Named("Mortar_LaunchA").PlayOneShot(info);
                            comp.Mana.CurLevel -= mortarManaCost;
                            comp.MagicUserXP += Rand.Range(12, 15);
                        }
                    }

                    if (CanExtractShell)
                    {
                        CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
                        if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                        {
                            ExtractShell();
                        }
                    }
                    if (forcedTarget.IsValid && !CanSetForcedTarget)
                    {
                        ResetForcedTarget();
                    }
                    if (!CanToggleHoldFire)
                    {
                        holdFireField.SetValue(this, true);
                    }
                    if (forcedTarget.ThingDestroyed)
                    {
                        ResetForcedTarget();
                    }
                    if (TT_Active && !IsStunned && Spawned) //&& (mannableComp == null || mannableComp.MannedNow)
                    {
                        GunCompEq.verbTracker.VerbsTick();
                        if (AttackVerb.state != VerbState.Bursting)
                        {
                            burstActivated = false;
                            if (WarmingUp)
                            {
                                burstWarmupTicksLeft--;
                                if (burstWarmupTicksLeft == 0)
                                {
                                    BeginBurst();
                                }
                            }
                            else
                            {
                                if (burstCooldownTicksLeft > 0)
                                {
                                    burstCooldownTicksLeft--;
                                }
                                if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                                {
                                    TryStartShootSomething(canBeginBurstImmediately: true);
                                }
                            }
                            top.TurretTopTick();
                        }
                    }
                    else
                    {
                        ResetCurrentTarget();
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 rndPos = DrawPos;
                    rndPos.x += Rand.Range(-.5f, .5f);
                    rndPos.z += Rand.Range(-.5f, .5f);
                    TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.SparkFlash, rndPos, Map, Rand.Range(.6f, .8f), .1f, .05f, .05f, 0, 0, 0, Rand.Range(0, 360));
                    FleckMaker.ThrowSmoke(rndPos, Map, Rand.Range(.8f, 1.2f));
                    rndPos = DrawPos;
                    rndPos.x += Rand.Range(-.5f, .5f);
                    rndPos.z += Rand.Range(-.5f, .5f);
                    TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.ElectricalSpark, rndPos, Map, Rand.Range(.4f, .7f), .2f, .05f, .1f, 0, 0, 0, Rand.Range(0, 360));
                }
                ExplosionHelper.Explode(Position, Map, 1f, DamageDefOf.EMP, this, 0, 0, SoundDefOf.Crunch, null, null, this, null, 0, 0, null, false, null, 0, 0, 0, false);
                Destroy(DestroyMode.Vanish);
            }
        }

        public new void TryActivateBurst()
        {
            burstActivated = true;
            TryStartShootSomething(canBeginBurstImmediately: true);
        }

        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), Position, Map, ThingPlaceMode.Near);
        }

        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(canBeginBurstImmediately: false);
            }
        }

        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        private bool CanExtractShell
        {
            get
            {
                if (!PlayerControlled)
                {
                    return false;
                }
                return gun.TryGetComp<CompChangeableProjectile>()?.Loaded ?? false;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (HitPoints < 1 && ManPawn != null && !ManPawn.Dead)
            {
                int rnd = Mathf.RoundToInt(Rand.Range(3, 5) - (.2f * effVal));
                for (int i = 0; i < rnd; i++)
                {
                    TM_Action.DamageEntities(ManPawn, null, Rand.Range(4f, 8f), DamageDefOf.Burn, this);
                }
            }
            base.Destroy(mode);            
        }
    }
}
