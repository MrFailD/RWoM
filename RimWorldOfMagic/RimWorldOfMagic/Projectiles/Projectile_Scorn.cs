using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using TorannMagic.Weapon;

namespace TorannMagic
{
    internal class Projectile_Scorn : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 20;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 4;
        private int strikeNum = 1;
        private float radius = 5;
        private bool initialized;
        private float angle;
        private List<IntVec3> cellList;
        private Pawn pawn;
        private IEnumerable<IntVec3> targets;
        private Skyfaller skyfaller2;
        private Skyfaller skyfaller;
        private Map map;
        private IntVec3 safePos = default(IntVec3);

        private bool launchedFlag;
        private bool pivotFlag;
        private bool landedFlag;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref launchedFlag, "launchedFlag", false, false);
            Scribe_Values.Look<bool>(ref landedFlag, "landedFlag", false, false);
            Scribe_Values.Look<bool>(ref pivotFlag, "pivotFlag", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1800, false);
            Scribe_Values.Look<int>(ref strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<int>(ref strikeNum, "strikeNum", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<IntVec3>(ref safePos, "safePos", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Collections.Look<IntVec3>(ref cellList, "cellList", LookMode.Value);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            //this.age++;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {            
            base.Impact(hitThing);
           
            ThingDef def = this.def;          

            if (!initialized)
            {
                pawn = launcher as Pawn;
                map = pawn.Map;                
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Scorn.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Scorn_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Scorn.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Scorn_ver");
                
                pwrVal = pwr.level;
                verVal = ver.level;
                arcaneDmg = comp.arcaneDmg;
                if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
                {
                    pwrVal = 1;
                    verVal = 1;
                }
                radius = this.def.projectile.explosionRadius + verVal;
                //this.duration = Mathf.RoundToInt(this.radius * this.strikeDelay);
                initialized = true;
            }

            if (!launchedFlag)
            {
                ModOptions.Constants.SetPawnInFlight(true);
                if (ModCheck.Validate.GiddyUp.Core_IsInitialized())
                {
                    ModCheck.GiddyUp.ForceDismount(pawn);
                }
                skyfaller = SkyfallerMaker.SpawnSkyfaller(ThingDef.Named("TM_ScornLeaving"), pawn.Position, map);
                if(Position.x < pawn.Position.x)
                {
                    angle = Rand.Range(20, 40);
                }
                else
                {
                    angle = Rand.Range(-40, -20);
                }
                skyfaller.angle = angle;
                launchedFlag = true;
                pawn.DeSpawn();
            }
            if (skyfaller.DestroyedOrNull() && !pivotFlag)
            {
                safePos = Position;
                if (safePos.x > map.Size.x - 5)
                {
                    safePos.x = map.Size.x - 5;
                }
                else if (safePos.x < 5)
                {
                    safePos.x = 5;
                }

                if (safePos.z > map.Size.z - 5)
                {
                    safePos.z = map.Size.z - 5;
                }
                else if (safePos.z < 5)
                {
                    safePos.z = 5;
                }
                skyfaller2 = SkyfallerMaker.SpawnSkyfaller(ThingDef.Named("TM_ScornIncoming"), safePos, map);
                skyfaller2.angle = angle;
                pivotFlag = true;

            }

            if (skyfaller2.DestroyedOrNull() && pivotFlag && launchedFlag && !landedFlag)
            {
                landedFlag = true;
                GenSpawn.Spawn(pawn, safePos, map);
                if (pawn.drafter != null)
                {
                    pawn.drafter.Drafted = true;
                }
                ModOptions.Constants.SetPawnInFlight(false);
                if(verVal == 0)
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_DemonScornHD, 60f + (pwrVal * 15));
                }
                else if(verVal == 1)
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_DemonScornHD_I, 60f + (pwrVal * 15));
                }
                else if(verVal == 2)
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_DemonScornHD_II, 60f + (pwrVal * 15));
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_DemonScornHD_III, 60f + (pwrVal * 15));
                }
            }
            if(landedFlag)
            { 
                if (Find.TickManager.TicksGame % strikeDelay == 0)
                {
                    if (safePos.DistanceToEdge(map) > strikeNum)
                    {
                        List<IntVec3> targets;
                        if (strikeNum == 1)
                        {
                            targets = GenRadial.RadialCellsAround(safePos, strikeNum, false).ToList();
                        }
                        else
                        {
                            IEnumerable<IntVec3> oldTargets = GenRadial.RadialCellsAround(Position, strikeNum - 1, false);
                            targets = GenRadial.RadialCellsAround(safePos, strikeNum, false).Except(oldTargets).ToList();
                        }
                        for (int j = 0; j < targets.Count(); j++)
                        {
                            IntVec3 curCell = targets[j];
                            if (map != null && curCell.IsValid && curCell.InBoundsWithNullCheck(map))
                            {
                                ExplosionHelper.Explode(curCell, Map, .4f, TMDamageDefOf.DamageDefOf.TM_Shadow, pawn, (int)((this.def.projectile.GetDamageAmount(1, null) * (1 + .15 * pwrVal)) * arcaneDmg * Rand.Range(.75f, 1.25f)), 0, TorannMagicDefOf.TM_SoftExplosion, def, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                            }
                        }
                        strikeNum++;
                    }
                    else
                    {
                        strikeNum = (int)radius + 1;
                    }
                }               
            }
            if (strikeNum > radius)
            {
                age = duration;
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

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public void damageEntities(Pawn e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.5f, 1.5f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }    
}