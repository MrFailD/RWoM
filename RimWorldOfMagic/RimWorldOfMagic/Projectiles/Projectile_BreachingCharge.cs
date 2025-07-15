using Verse;
using AbilityUser;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
	public class Projectile_BreachingCharge : Projectile_AbilityBase
	{

        private bool initialized;
        private int verVal;
        private float mightPwr = 1f;
        private int ticksToDetonation = 210;
        private int explosionCount = 5;
        private Pawn caster;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (!initialized)
            {
                caster = launcher as Pawn;
                CompAbilityUserMight comp = caster.GetCompAbilityUserMight();
                //verVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_ShotgunSpec, "TM_ShotgunSpec", "_ver", true); 
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_ShotgunSpec);
                explosionCount = 5;
                if(verVal >= 3)
                {
                    explosionCount++;
                }
                initialized = true;
            }

            landed = true;
            ticksToDetonation = def.projectile.explosionDelay;
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction);            
        }

        private void Explode()
        {
            if (explosionCount > 0)
            {
                ExplosionHelper.Explode(Position, Map, def.projectile.explosionRadius-explosionCount, 
                    def.projectile.damageDef, launcher as Pawn, Mathf.RoundToInt((def.projectile.GetDamageAmount(this) * (1f + (.15f * verVal))) * mightPwr), def.projectile.damageDef.defaultArmorPenetration, def.projectile.damageDef.soundExplosion, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, true);
            }
            else
            {
                Effecter exp = TorannMagicDefOf.GiantExplosion.Spawn();
                exp.Trigger(new TargetInfo(Position, Map, false), new TargetInfo(Position, Map, false));
                exp.Cleanup();
                Destroy(DestroyMode.Vanish);
            }
            explosionCount--;
        }

        public override void Tick()
        {
            base.Tick();
            ticksToDetonation--;
            if (ticksToDetonation <= 0)
            {
                Explode();
            }               
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (explosionCount <= 0)
            {
                base.Destroy(mode);
            }
        }
    }
}
