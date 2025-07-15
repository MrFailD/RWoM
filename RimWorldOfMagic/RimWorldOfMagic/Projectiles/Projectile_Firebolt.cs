using Verse;
using AbilityUser;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    internal class Projectile_Firebolt : Projectile_AbilityBase
    {

        private int pwrVal;
        private float arcaneDmg = 1f;
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            

            Pawn pawn = launcher as Pawn;
            Pawn victim = hitThing as Pawn;
            if (pawn != null)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Firebolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Firebolt_pwr");
                pwrVal = pwr.level;
                arcaneDmg = comp.arcaneDmg;
                if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    pwrVal = mpwr.level;
                }
            }
            
            ExplosionHelper.Explode(Position, map, 0.4f, TMDamageDefOf.DamageDefOf.Firebolt, launcher, Mathf.RoundToInt(this.def.projectile.GetDamageAmount(1, null) * arcaneDmg), 0, this.def.projectile.soundExplode, def, equipmentDef, intendedTarget.Thing, null, 0f, 1, null, false, null, 0f, 1, 0.6f, false);
            CellRect cellRect = CellRect.CenteredOn(Position, 3);
            cellRect.ClipInsideMap(map);

            victim = Position.GetFirstPawn(map);
            if (victim != null)
            {                
                int dmg = Mathf.RoundToInt(((this.def.projectile.GetDamageAmount(1,null) / 3) * pwrVal)* arcaneDmg);  //projectile = 16
                if (ModOptions.Settings.Instance.AIHardMode && launcher is Pawn && !pawn.IsColonist)
                {
                    dmg += 10;
                }
                damageEntities(victim, dmg, TMDamageDefOf.DamageDefOf.Firebolt);
            }
        }

        public void damageEntities(Pawn e, int amt, DamageDef type)
        {
            DamageInfo dinfo = new DamageInfo(type, Mathf.RoundToInt(Rand.Range((float)(.5f * amt), (float)(1.25*amt))), 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }    
}


