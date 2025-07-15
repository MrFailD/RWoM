using System.Linq;
using Verse;
using AbilityUser;
using UnityEngine;
using RimWorld;
using TorannMagic.Weapon;

namespace TorannMagic
{
    internal class Projectile_ES_Fire : Projectile_AbilityBase
    {

        private int verVal;
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;

            Pawn pawn = launcher as Pawn;
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_TechnoWeapon.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_TechnoWeapon_ver");
            verVal = ver.level;

            ExplosionHelper.Explode(Position, map, 1f + (verVal * .05f), DamageDefOf.Burn, launcher, Mathf.RoundToInt(this.def.projectile.GetDamageAmount(1,null) * comp.arcaneDmg * (1f + .02f * verVal)), 0, this.def.projectile.soundExplode, def, equipmentDef, intendedTarget.Thing, null, 0f, 1, null, false, null, 0f, 1, 0.0f, false);

        }
    }    
}


