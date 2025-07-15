﻿using AbilityUser;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using TorannMagic.Weapon;


namespace TorannMagic
{
	public class Projectile_PoisonFlask : Projectile_AbilityBase
	{
        private int age = -1;
        private int duration = 360;
        private bool initialized;
        private float radius = 4;
        private int strikeDelay = 40;
        private int lastStrike;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref age, "age", 0);
            Scribe_Values.Look<float>(ref radius, "radius", 4);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            age++;            
            if (age < duration)
            {
                Pawn caster = launcher as Pawn;
                if (caster != null && !initialized)
                {
                    initialized = true;
                    CompAbilityUserMight comp = caster.GetCompAbilityUserMight();
                    int verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_PoisonFlask, false);
                    int pwrVal = TM_Calc.GetSkillPowerLevel(caster, TorannMagicDefOf.TM_PoisonFlask, false);
                    Map map = Map;
                    radius = def.projectile.explosionRadius + (.8f * verVal);
                    duration = 360 + (60 * pwrVal);
                    ThingDef fog = TorannMagicDefOf.Fog_Poison;
                    fog.gas.expireSeconds.min = duration / 60;
                    fog.gas.expireSeconds.max = duration / 60;
                    ExplosionHelper.Explode(Position, map, radius, TMDamageDefOf.DamageDefOf.TM_Poison, this, 0, 0, SoundDef.Named("TinyBell"), def, null, null, fog, 1f, 1, null, false, null, 0f, 0, 0.0f, false);
                }

                if (age >= lastStrike + strikeDelay)
                {
                    try
                    {
                        List<Pawn> pList = (from pawn in Map.mapPawns.AllPawnsSpawned
                                            where (!pawn.Dead && (pawn.Position - Position).LengthHorizontal <= radius && pawn.RaceProps != null && pawn.RaceProps.IsFlesh)
                                            select pawn).ToList();

                        for (int i = 0; i < pList.Count(); i++)
                        {
                            Pawn victim = pList[i];
                            List<BodyPartRecord> bprList = new List<BodyPartRecord>();
                            bprList.Clear();
                            BodyPartRecord bpr = null;
                            foreach (BodyPartRecord record in victim.def.race.body.AllParts)
                            {
                                if (record.def.tags.Contains(BodyPartTagDefOf.BreathingSource) || record.def.tags.Contains(BodyPartTagDefOf.BreathingPathway))
                                {
                                    if (victim.health != null && victim.health.hediffSet != null && !victim.health.hediffSet.PartIsMissing(record))
                                    {
                                        bprList.Add(record);
                                    }
                                }
                            }
                            if (bprList != null && bprList.Count > 0 && caster != null)
                            {
                                TM_Action.DamageEntities(victim, bprList.RandomElement(), Rand.Range(1f, 2f), 2f, TMDamageDefOf.DamageDefOf.TM_Poison, caster);
                            }
                        }
                    }
                    catch
                    {
                        Log.Message("Debug: poison trap failed to process triggered event - terminating poison trap");
                        age = duration;
                    }
                    lastStrike = age;
                }
            }
            base.Impact(hitThing);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (age >= duration)
            {
                base.Destroy(mode);
            }
        }
    }	
}


