using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using AbilityUser;
using Verse.Sound;
using UnityEngine;

namespace TorannMagic.Weapon
{
    internal class Projectile_LightningWand : Projectile_AbilityLaser
    {
        private float arcaneDmg = 1;

        public override void Impact_Override(Thing hitThing)
        {
            Map map = Map;
            base.Impact_Override(hitThing);
            Pawn pawn = launcher as Pawn;
            if (pawn != null)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if (comp.IsMagicUser)
                {
                    arcaneDmg = comp.arcaneDmg;
                }
            }

            bool flag = hitThing != null;
            if (flag)
            {
                int DamageAmount = Mathf.RoundToInt(def.projectile.GetDamageAmount(1,null) * arcaneDmg);
                DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, 0, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo);

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
                }
            }
            else
            {
                FleckMaker.ThrowMicroSparks(ExactPosition, Map);
            }
            for (int i = 0; i <= 1; i++)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map, false), MaintenanceType.None);
                //SoundDef.Named("TM_Thunder_OnMap");
            }
            CellRect cellRect = CellRect.CenteredOn(hitThing.Position, 2);
            cellRect.ClipInsideMap(map);
            for (int i = 0; i < Rand.Range(1, 3); i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                StaticExplosion(randomCell, map, 0.4f);
            }
        }

        protected void StaticExplosion(IntVec3 pos, Map map, float radius)
        {
            ThingDef def = this.def;
            Explosion(pos, map, radius, DamageDefOf.Stun, launcher, null, def, equipmentDef, ThingDefOf.Spark, 0.4f, 1, false, null, 0f, 1);

        }

        public static void Explosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = true, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
        {
            System.Random rnd = new System.Random();
            int modDamAmountRand = GenMath.RoundRandom(Rand.Range(2, projectile.projectile.GetDamageAmount(1,null) / 2));
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
            explosion.armorPenetration = .8f;
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
