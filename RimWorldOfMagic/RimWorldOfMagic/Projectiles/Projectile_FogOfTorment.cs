using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using TorannMagic.Weapon;

namespace TorannMagic
{
    internal class Projectile_FogOfTorment : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 1440;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 120;
        private int lastStrike;
        private bool initialized;
        private ThingDef fog;

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

            Pawn pawn = launcher as Pawn;
            Pawn victim = null;
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            if (comp != null)
            {
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_FogOfTorment.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_FogOfTorment_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_FogOfTorment.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_FogOfTorment_ver");
                
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
            }
            else if(pawn.def == TorannMagicDefOf.TM_SkeletonLichR)
            {
                pwrVal = Rand.RangeInclusive(0,3);
                verVal = Rand.RangeInclusive(0,3);
            }

            if (!initialized)
            {
                fog = TorannMagicDefOf.Fog_Torment;
                duration = duration + (180 * verVal);
                strikeDelay = strikeDelay - (18 * verVal);

                fog.gas.expireSeconds.min = duration/60;
                fog.gas.expireSeconds.max = duration/60;
                ExplosionHelper.Explode(Position, map, this.def.projectile.explosionRadius + verVal, TMDamageDefOf.DamageDefOf.TM_Torment, launcher, 0, 0, this.def.projectile.soundExplode, def, equipmentDef, null, fog, 1f, 1, null, false, null, 0f, 0, 0.0f, false);
                
                initialized = true;
            }

            if (age > lastStrike + strikeDelay)
            {
                IntVec3 curCell;
                IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(Position, this.def.projectile.explosionRadius + verVal, true);
                for (int i = 0; i < targets.Count(); i++)
                {
                    curCell = targets.ToArray<IntVec3>()[i];

                    if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                    {
                        victim = curCell.GetFirstPawn(map);
                        if(victim != null && !victim.Dead && victim.RaceProps.FleshType.isOrganic)
                        {
                            if(TM_Calc.IsUndead(victim))
                            {
                                //heals undead
                                Hediff_Injury injuryToHeal = victim.health.hediffSet.hediffs
                                    .OfType<Hediff_Injury>()
                                    .FirstOrDefault(injury => injury.CanHealNaturally());
                                injuryToHeal?.Heal(2.0f + pwrVal);

                            }
                            else
                            {
                                //kills living
                                if (Rand.Chance(TM_Calc.GetSpellSuccessChance(pawn, victim) - .4f))
                                {
                                    float targetMassFactor = victim.def.BaseMass != 0 ? 50f/victim.def.BaseMass : 1f;                                    
                                    damageEntities(victim, Mathf.RoundToInt(Rand.Range(2f + (1f * pwrVal), 4f + (1f * pwrVal)) * arcaneDmg * targetMassFactor), TMDamageDefOf.DamageDefOf.TM_Torment);
                                }
                            }
                        }
                    }
                }
                lastStrike = age;
            }
        }            

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1440, false);
            Scribe_Values.Look<int>(ref strikeDelay, "shockDelay", 0, false);
            Scribe_Values.Look<int>(ref lastStrike, "lastStrike", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Defs.Look<ThingDef>(ref fog, "fog");
        }

        public void damageEntities(Pawn e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.5f, 1.5f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            if (launcher != null && launcher is Pawn caster)
            {
                dinfo = new DamageInfo(type, amt, 0, (float)-1, caster, null, null, DamageInfo.SourceCategory.ThingOrUnknown);                
            }
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
            
        }
    }    
}


