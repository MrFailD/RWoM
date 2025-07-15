using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TorannMagic
{
    internal class Projectile_Attraction : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 1200;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 6;
        private float radius = 5;
        private bool initialized;
        private List<IntVec3> cellList;
        private List<IntVec3> hediffCellList;
        private Pawn pawn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1800, false);
            Scribe_Values.Look<int>(ref strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Collections.Look<IntVec3>(ref cellList, "cellList", LookMode.Value);
            Scribe_Collections.Look<IntVec3>(ref hediffCellList, "hediffCellList", LookMode.Value);
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
            age++;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {            
            base.Impact(hitThing);
           
            ThingDef def = this.def;
            Pawn victim = null;

            if (!initialized)
            {
                pawn = launcher as Pawn;
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Attraction.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Attraction_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Attraction.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Attraction_ver");
                
                pwrVal = pwr.level;
                verVal = ver.level;
                if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                arcaneDmg = comp.arcaneDmg;
                if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                duration = duration + (120 * verVal);
                strikeDelay = strikeDelay - verVal;
                radius = this.def.projectile.explosionRadius + (1.5f * pwrVal);
                //ExplosionHelper.Explode(base.Position, this.Map, this.radius, TMDamageDefOf.DamageDefOf.TM_Shadow, this.pawn, (int)((this.def.projectile.GetDamageAmount(1, null) * (1 + .15 * pwrVal)) * this.arcaneDmg * Rand.Range(.75f, 1.25f)), 0, TorannMagicDefOf.TM_SoftExplosion, def, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);

                initialized = true;
                IEnumerable<IntVec3> hediffCells = GenRadial.RadialCellsAround(Position, 2, true);
                hediffCellList = hediffCells.ToList<IntVec3>();
                IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(Position, radius, false).Except(hediffCells);
                cellList = targets.ToList<IntVec3>();
                for(int i = 0; i < cellList.Count(); i ++)
                {
                    if (cellList[i].IsValid && cellList[i].InBoundsWithNullCheck(pawn.Map))
                    {
                        victim = cellList[i].GetFirstPawn(pawn.Map);
                        if (victim != null && !victim.Dead && !victim.Downed)
                        {
                            HealthUtility.AdjustSeverity(victim, TorannMagicDefOf.TM_GravitySlowHD, .5f);
                        }
                    }
                }
            }
            IntVec3 curCell = cellList.RandomElement();
            //Vector3 angle = GetVector(base.Position, curCell);
            //TM_MoteMaker.ThrowArcaneWaveMote(curCell.ToVector3(), base.Map, .4f * (curCell - base.Position).LengthHorizontal, .1f, .05f, .5f, 0, Rand.Range(1, 2), (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(), (Quaternion.AngleAxis(-90, Vector3.up) * angle).ToAngleFlat());

            if (Find.TickManager.TicksGame % strikeDelay == 0 && Map != null)
            {
                if (pwrVal == 0)
                {
                    Effecter AttractionEffect = TorannMagicDefOf.TM_AttractionEffecter.Spawn();
                    AttractionEffect.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                    AttractionEffect.Cleanup();
                }
                else if(pwrVal ==1)
                {
                    Effecter AttractionEffect = TorannMagicDefOf.TM_AttractionEffecter_I.Spawn();
                    AttractionEffect.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                    AttractionEffect.Cleanup();
                }
                else if(pwrVal == 2)
                {
                    Effecter AttractionEffect = TorannMagicDefOf.TM_AttractionEffecter_II.Spawn();
                    AttractionEffect.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                    AttractionEffect.Cleanup();
                }
                else
                {
                    Effecter AttractionEffect = TorannMagicDefOf.TM_AttractionEffecter_III.Spawn();
                    AttractionEffect.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                    AttractionEffect.Cleanup();
                }
                for (int i = 0; i < 3; i++)
                {
                    curCell = cellList.RandomElement();
                    if (curCell.IsValid && curCell.InBoundsWithNullCheck(Map))
                    {
                        victim = curCell.GetFirstPawn(Map);
                        if (victim != null && !victim.Dead && victim.RaceProps.IsFlesh && victim != pawn)
                        {                            
                            if (Rand.Chance(TM_Calc.GetSpellSuccessChance(pawn, victim) - .4f))
                            {
                                Vector3 launchVector = GetVector(Position, victim.Position);
                                HealthUtility.AdjustSeverity(victim, TorannMagicDefOf.TM_GravitySlowHD, (.4f + (.1f * verVal)));
                                LaunchFlyingObect(victim.Position + (2f * (1 + (.4f * pwrVal)) * launchVector).ToIntVec3(), victim);
                            }
                        }
                        //else if (victim != null && !victim.Dead && !victim.RaceProps.IsFlesh)
                        //{
                        //    HealthUtility.AdjustSeverity(victim, TorannMagicDefOf.TM_GravitySlowHD, .4f + (.1f * verVal));
                        //}
                    }
                }
                for(int i =0; i < hediffCellList.Count(); i++)
                {
                    curCell = hediffCellList[i];
                    if (curCell.IsValid && curCell.InBoundsWithNullCheck(Map))
                    {
                        victim = curCell.GetFirstPawn(Map);
                        if (victim != null && !victim.Dead && victim != pawn)
                        {
                            if (Rand.Chance(TM_Calc.GetSpellSuccessChance(pawn, victim) - .4f))
                            {
                                HealthUtility.AdjustSeverity(victim, TorannMagicDefOf.TM_GravitySlowHD, .3f + (.1f * verVal));
                            }
                        }
                    }
                }   
            }
        }

        public void LaunchFlyingObect(IntVec3 targetCell, Pawn pawn)
        {
            bool flag = targetCell != IntVec3.Invalid && targetCell != default(IntVec3);
            if (flag)
            {
                if (pawn != null && pawn.Position.IsValid && pawn.Spawned && pawn.Map != null && !pawn.Downed && !pawn.Dead)
                {
                    if (ModCheck.Validate.GiddyUp.Core_IsInitialized())
                    {
                        ModCheck.GiddyUp.ForceDismount(pawn);
                    }
                    FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(TorannMagicDefOf.FlyingObject_Spinning, pawn.Position, pawn.Map);
                    flyingObject.Launch(pawn, targetCell, pawn);
                }
            }
        }

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (center - objectPos).ToVector3();
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