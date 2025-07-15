using Verse;
using Verse.Sound;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using HarmonyLib;
using TorannMagic.Weapon;


namespace TorannMagic
{
    public struct SabotageThing
    {
        public Thing thing;
        public int duration;
        public float effectRadius;
        public int effectFrequency;

        public SabotageThing(Thing sThing, int sDuration, float sEffectRadius, int sEffectFrequency)
        {
            thing = sThing;
            duration = sDuration;
            effectRadius = sEffectRadius;
            effectFrequency = sEffectFrequency;
        }
    }

    internal class Projectile_Sabotage : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 1800;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 120;
        private int lastStrike;
        private int targetThingCount = 0;
        private bool initialized;

        private List<IntVec3> targetCells;
        private List<SabotageThing> targetThings;

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
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;

            Pawn caster = launcher as Pawn;
            if (!initialized)
            {
                
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();

                pwrVal = comp.MagicData.MagicPowerSkill_Sabotage.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Sabotage_pwr").level;
                verVal = comp.MagicData.MagicPowerSkill_Sabotage.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Sabotage_ver").level;
                if (caster.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                arcaneDmg = comp.arcaneDmg;
                if (ModOptions.Settings.Instance.AIHardMode && !caster.IsColonist)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                info.pitchFactor = .5f;
                info.volumeFactor = .8f;
                SoundDefOf.PsychicPulseGlobal.PlayOneShot(info);
                Effecter SabotageEffect = TorannMagicDefOf.TM_SabotageExplosion.Spawn();
                SabotageEffect.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                SabotageEffect.Cleanup();
                targetCells = new List<IntVec3>();
                targetCells.Clear();
                targetCells = GenRadial.RadialCellsAround(Position, this.def.projectile.explosionRadius, true).ToList();
                targetThings = new List<SabotageThing>();
                targetThings.Clear();
                initialized = true;

                Pawn targetPawn = null;
                Building targetBuilding = null;

                for (int i = 0; i < targetCells.Count; i++)
                {
                    if (Rand.Chance((.5f + (.1f * verVal)) * arcaneDmg))
                    {
                        float rnd = Rand.Range(0, 1f);
                        targetPawn = targetCells[i].GetFirstPawn(Map);
                        if (targetPawn != null)
                        {
                            if (TM_Calc.IsRobotPawn(targetPawn))
                            {
                                TM_Action.DoAction_SabotagePawn(targetPawn, caster, rnd, pwrVal, arcaneDmg, launcher);
                                age = duration;
                            }
                            else
                            {
                                targetPawn = null;
                                //Log.Message("pawn not a robot, mechanoid, or android");
                            }
                        }

                        targetBuilding = targetCells[i].GetFirstBuilding(Map);
                        if (targetPawn == null && targetBuilding != null)
                        {
                            CompPower compP = targetBuilding.GetComp<CompPower>();
                            CompPowerTrader cpt = targetBuilding.GetComp<CompPowerTrader>();
                            if (compP != null && compP.Props.PowerConsumption != 0 && cpt != null && cpt.powerOutputInt != 0)
                            {
                                ExplosionHelper.Explode(targetBuilding.Position, Map, 2 + pwrVal + Mathf.RoundToInt(cpt.powerOutputInt / 400), DamageDefOf.Stun, null);
                                ExplosionHelper.Explode(targetBuilding.Position, Map, 1 + pwrVal + Mathf.RoundToInt(cpt.powerOutputInt / 600), TMDamageDefOf.DamageDefOf.TM_ElectricalBurn, null);
                            }

                            Building_Battery targetBattery = targetBuilding as Building_Battery;
                            if (targetBattery != null && targetBattery.def.thingClass.ToString() == "RimWorld.Building_Battery")
                            {
                                CompPowerBattery compB = targetBattery.GetComp<CompPowerBattery>();
                                if (rnd <= .5f)
                                {
                                    Traverse.Create(root: targetBattery).Field(name: "ticksToExplode").SetValue(Rand.Range(40, 130) - (5*pwrVal));
                                    compB.SetStoredEnergyPct(.81f);
                                }
                                else
                                {
                                    ExplosionHelper.Explode(targetBattery.Position, Map, 2 + pwrVal + Mathf.RoundToInt(compB.StoredEnergy / 200), DamageDefOf.EMP, null);
                                    compB.DrawPower(compB.StoredEnergy);
                                }

                            }

                            Building_TurretGun targetTurret = targetBuilding as Building_TurretGun;
                            if (targetTurret != null && targetTurret.gun != null)
                            {
                                if (rnd <= .5f)
                                {
                                    targetTurret.SetFaction(Faction.OfAncientsHostile, null);
                                }
                                else
                                {
                                    ExplosionHelper.Explode(targetTurret.Position, Map, 2 + pwrVal, TMDamageDefOf.DamageDefOf.TM_ElectricalBurn, null); //20 default damage
                                }
                            }
                        }
                        else
                        {
                            //Log.Message("no thing to sabotage");
                        }
                        targetPawn = null;
                        targetBuilding = null;
                    }
                }
            }
            else if(targetThings.Count > 0)
            {
                age = duration;
            }
            else
            {
                age = duration;
            }

            
        }            

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1800, false);
            Scribe_Values.Look<int>(ref strikeDelay, "shockDelay", 0, false);
            Scribe_Values.Look<int>(ref lastStrike, "lastStrike", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
        }
    }    
}


