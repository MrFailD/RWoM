using Verse;
using AbilityUser;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_Mk203GL : Projectile_AbilityBase
    {
        private bool initialized;
        private int verVal;
        private float mightPwr = 1f;
        private IntVec3 strikePos = default(IntVec3);
        private Pawn caster;
        private float radius = 4;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (!initialized)
            {
                caster = launcher as Pawn;
                strikePos = Position;
                CompAbilityUserMight comp = caster.GetCompAbilityUserMight();
                //verVal = TM_Calc.GetMightSkillLevel(caster, comp.MightData.MightPowerSkill_RifleSpec, "TM_RifleSpec", "_ver", true);
                verVal = TM_Calc.GetSkillVersatilityLevel(caster, TorannMagicDefOf.TM_RifleSpec, true);
                radius = this.def.projectile.explosionRadius;
                initialized = true;
            }

            ExplosionHelper.Explode(strikePos, map, this.def.projectile.explosionRadius, this.def.projectile.damageDef,
                launcher as Pawn, Mathf.RoundToInt((this.def.projectile.GetDamageAmount(this) + (1.5f * verVal)) * mightPwr), 3, this.def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, true);
            strikePos.x += Mathf.RoundToInt(Rand.Range(-radius, radius));
            strikePos.z += Mathf.RoundToInt(Rand.Range(-radius, radius));
            int damage = Mathf.RoundToInt(((this.def.projectile.GetDamageAmount(this) / 2f) + (1f * verVal)) * mightPwr);
            ExplosionHelper.Explode(strikePos, map, this.def.projectile.explosionRadius/2f,
                this.def.projectile.damageDef, launcher as Pawn, damage, 0, this.def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, true);
            strikePos = Position;
            strikePos.x += Mathf.RoundToInt(Rand.Range(-radius, radius));
            strikePos.z += Mathf.RoundToInt(Rand.Range(-radius, radius));
            ExplosionHelper.Explode(strikePos, map, this.def.projectile.explosionRadius/2f, this.def.projectile.damageDef,
                launcher as Pawn, damage, 0, this.def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, true);
        }

    }
}


