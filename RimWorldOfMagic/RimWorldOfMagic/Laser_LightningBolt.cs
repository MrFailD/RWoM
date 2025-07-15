using RimWorld;
using Verse;
using Verse.Sound;
using AbilityUser;
using System.Linq;
using UnityEngine;

namespace TorannMagic
{
    internal class Laser_LightningBolt : Projectile_AbilityLaser 
    {
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;

        public override void Impact_Override(Thing hitThing)
        {
            Map map = Map;
            base.Impact_Override(hitThing);

            Pawn pawn = launcher as Pawn;
                       
            
            
            if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                pwrVal = mpwr.level;
                verVal = mver.level;
                arcaneDmg = pawn.GetCompAbilityUserMight().mightPwr;
            }
            else
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_LightningBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_LightningBolt_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_LightningBolt.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_LightningBolt_ver");
                pwrVal = pwr.level;
                verVal = ver.level;
                arcaneDmg = comp.arcaneDmg;
            }
            
            if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            {
                pwrVal = 3;
                verVal = 3;
            }
            bool flag = hitThing != null;
            if (flag)
            {
                int DamageAmount = Mathf.RoundToInt(def.projectile.GetDamageAmount(1,null) + (pwrVal * 6)* arcaneDmg);
                DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, 1, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo);
                if(Rand.Chance(.6f))
                {
                    DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Stun, DamageAmount/4, 1, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
                    hitThing.TakeDamage(dinfo2);
                }

                bool flag2 = canStartFire && Rand.Range(0f, 1f) > startFireChance;
                if (flag2)
                {
                    hitThing.TryAttachFire(0.05f, null);
                }
                Pawn hitTarget;
                bool flag3 = (hitTarget = (hitThing as Pawn)) != null;
                if (flag3)
                {
                    PostImpactEffects(launcher as Pawn, hitTarget);
                    FleckMaker.ThrowMicroSparks(destination, Map);
                    FleckMaker.Static(destination, Map, FleckDefOf.ShotHit_Dirt, 1f);
                }
            }
            else
            {
                FleckMaker.Static(ExactPosition, Map, FleckDefOf.ShotHit_Dirt, 1f);
                FleckMaker.ThrowMicroSparks(ExactPosition, Map);
            }
            for (int i = 0; i <= verVal; i++)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            }
            CellRect cellRect = CellRect.CenteredOn(hitThing.Position, 2);
            cellRect.ClipInsideMap(map);
            for (int i = 0; i < Rand.Range(verVal, verVal * 4); i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                StaticExplosion(randomCell, map, 0.4f);
            }
        }

        protected void StaticExplosion(IntVec3 pos, Map map, float radius)
        {
            ThingDef def = this.def;
            Explosion(pos, map, radius, TMDamageDefOf.DamageDefOf.TM_Lightning, launcher, null, def, equipmentDef, null, 0.4f, 1, false, null, 0f, 1);
            Explosion(pos, map, radius, DamageDefOf.Stun, launcher, null, def, equipmentDef, null, 0.4f, 1, false, null, 0f, 1);

        }

        public void Explosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = true, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
        {
            System.Random rnd = new System.Random();
            int modDamAmountRand = GenMath.RoundRandom(Rand.Range(2, TMDamageDefOf.DamageDefOf.TM_Lightning.defaultDamage));
            modDamAmountRand *= Mathf.RoundToInt(arcaneDmg);
            if (map == null)
            {
                Log.Warning("Tried to do explosion in a null map.");
                return;
            }
            Explosion explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
            explosion.damageFalloff = false;
            explosion.chanceToStartFire = 0.0f;
            explosion.Position = center;
            explosion.radius = radius;
            explosion.damType = damType;
            explosion.instigator = instigator;
            explosion.damAmount = ((projectile == null) ? GenMath.RoundRandom((float)damType.defaultDamage) : modDamAmountRand);
            explosion.weapon = source;
            explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
            explosion.preExplosionSpawnChance = preExplosionSpawnChance;
            explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
            explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
            explosion.postExplosionSpawnChance = postExplosionSpawnChance;
            explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
            explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            explosion.StartExplosion(explosionSound, null);
        }
    }
}
