using Verse;
using RimWorld;
using AbilityUser;
using UnityEngine;

namespace TorannMagic.Weapon
{
    public class Projectile_BlazingPower : Projectile_AbilityBase
    {
        private float arcaneDmg = 1;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Pawn pawn = launcher as Pawn;
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (pawn != null)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if (comp != null && comp.IsMagicUser)
                {
                    arcaneDmg = comp.arcaneDmg;
                }
                try
                {
                    ExplosionHelper.Explode(Position, map, this.def.projectile.explosionRadius,
                        TMDamageDefOf.DamageDefOf.TM_BlazingPower, launcher,
                        Mathf.RoundToInt(this.def.projectile.GetDamageAmount(1, null) * arcaneDmg),
                        2, SoundDefOf.Crunch, def, equipmentDef, null,
                        null, 0f, 1, null,
                        false, null, 0f,
                        1, 0.0f, true);
                }
                catch
                {
                    //don't care
                }
            }
        }
    }
}
