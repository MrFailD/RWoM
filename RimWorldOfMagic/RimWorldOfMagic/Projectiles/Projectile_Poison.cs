using RimWorld;
using Verse;
using AbilityUser;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace TorannMagic
{
    public class Projectile_Poison : Projectile_AbilityBase
    {
        private BodyPartRecord[] vulnerableParts = new BodyPartRecord[10];
        private bool initialized;
        private int age;
        private int lastPoison;
        private int poisonRate = 270;
        private int duration = 2700;
        private IntVec3 oldPosition;
        private Pawn hitPawn;
        private Pawn caster;

        private MagicPowerSkill pwr;
        private MagicPowerSkill ver;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", 0, false);
            Scribe_Values.Look<int>(ref duration, "duration", 2700, false);
            Scribe_Values.Look<int>(ref lastPoison, "lastPoison", 0, false);
            Scribe_Values.Look<int>(ref poisonRate, "poisonRate", 270, false);
            Scribe_Values.Look<IntVec3>(ref oldPosition, "oldPosition", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref hitPawn, "hitPawn", false);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
                        
            caster = launcher as Pawn;
            pwr = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Poison.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Poison_pwr");
            ver = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Poison.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Poison_ver");
            
            pwrVal = pwr.level;
            verVal = ver.level;
            if (caster.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                MightPowerSkill mpwr = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                MightPowerSkill mver = caster.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                pwrVal = mpwr.level;
                verVal = mver.level;
            }
            arcaneDmg = caster.GetCompAbilityUserMagic().arcaneDmg;
            if (ModOptions.Settings.Instance.AIHardMode && !caster.IsColonist)
            {
                pwrVal = 3;
                verVal = 3;
            }
            if (!initialized)
            {
                hitPawn = hitThing as Pawn;
                if (hitThing != null && hitPawn.needs.food != null)
                {
                    duration += (verVal * 180);                    
                    Initialize(hitPawn);
                    oldPosition = hitPawn.Position;
                    damageEntities(hitPawn, null, 4 , TMDamageDefOf.DamageDefOf.TM_Poison);
                    HealthUtility.AdjustSeverity(hitPawn, TorannMagicDefOf.TM_Poisoned_HD, Rand.Range(1f + verVal, 4f + (2f * verVal)));                    
                    initialized = true;
                    TM_MoteMaker.ThrowPoisonMote(hitPawn.Position.ToVector3(), map, 2.2f);
                    if (hitPawn.IsColonist && !caster.IsColonist)
                    {
                        TM_Action.SpellAffectedPlayerWarning(hitPawn);
                    }
                }
                else
                {
                    Log.Message("No target found for poison or target not susceptable to poison.");
                    age = duration + 1;
                    Destroy(DestroyMode.Vanish);
                }
            }

            if (age > (lastPoison + poisonRate))
            {
                try
                {
                    if (hitPawn != null && !hitPawn.Dead && caster != null && !caster.Dead)
                    {
                        if (hitPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Poisoned_HD, false))
                        {
                            int dmg = (int)(((hitPawn.Position - oldPosition).LengthHorizontal) * (1 + (.25f * pwrVal)));
                            int rndPart = (int)Rand.RangeInclusive(0, 4);
                            damageEntities(hitPawn, vulnerableParts[rndPart], dmg, TMDamageDefOf.DamageDefOf.TM_Poison);
                            TM_MoteMaker.ThrowPoisonMote(hitPawn.Position.ToVector3(), map, 1f);
                            lastPoison = age;
                            oldPosition = hitPawn.Position;
                        }
                        else
                        {
                            //no longer poisoned, end
                            age = duration + 1;
                            Destroy(DestroyMode.Vanish);
                        }
                    }
                    else
                    {
                        //pawn is dead, end 
                        age = duration + 1;
                        Destroy(DestroyMode.Vanish);
                    }
                }
                catch
                {
                    age = duration + 1;
                    Destroy(DestroyMode.Vanish);
                }
            }
            
        }

        public void Initialize(Pawn victim)
        {
            BodyPartRecord vitalPart = null;
            if (victim != null && !victim.Dead)
            {
                
                IEnumerable<BodyPartRecord> partSearch = victim.def.race.body.AllParts;
                vitalPart = partSearch.FirstOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource));
                if (vitalPart != null)
                {
                    vulnerableParts[0] = vitalPart;
                }
                vitalPart = null;
                vitalPart = partSearch.FirstOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.BloodFiltrationKidney));
                if (vitalPart != null)
                {
                    vulnerableParts[1] = vitalPart;
                }
                vitalPart = null;
                vitalPart = partSearch.LastOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.BloodFiltrationKidney));
                if (vitalPart != null)
                {
                    vulnerableParts[2] = vitalPart;
                }
                vitalPart = null;
                vitalPart = partSearch.FirstOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.BloodFiltrationLiver));
                if (vitalPart != null)
                {
                    vulnerableParts[3] = vitalPart;
                }
                vitalPart = null;
                vitalPart = partSearch.LastOrDefault<BodyPartRecord>((BodyPartRecord x) => x.def.tags.Contains(BodyPartTagDefOf.BloodFiltrationLiver));
                if (vitalPart != null)
                {
                    vulnerableParts[4] = vitalPart;
                }
            }
        }

        public void damageEntities(Pawn victim, BodyPartRecord hitPart, int amt, DamageDef type)
        {
            DamageInfo dinfo;
            amt = Mathf.RoundToInt((float)amt * Rand.Range(.5f, 1.2f) * arcaneDmg);
            if (caster != null && victim != null && !victim.Dead && !victim.Downed && hitPart != null)
            {
                dinfo = new DamageInfo(type, amt, 0, (float)-1, caster, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
                dinfo.SetAllowDamagePropagation(false);
                victim.TakeDamage(dinfo);
            }
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }
    }
}
