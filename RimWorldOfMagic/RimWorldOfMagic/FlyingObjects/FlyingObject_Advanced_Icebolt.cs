using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_Advanced_Icebolt : FlyingObject_Advanced
    {
        public override void ImpactOverride()
        {
            Map map = Map;
            Pawn pawn = launcher as Pawn;
            ExplosionHelper.Explode(Position, map, 0.4f, TMDamageDefOf.DamageDefOf.Iceshard, launcher, Mathf.RoundToInt(def.projectile.GetDamageAmount(1, null)), 0, def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
            CellRect cellRect = CellRect.CenteredOn(Position, 2);
            cellRect.ClipInsideMap(map);
            for (int i = 0; i < 3; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                Shrapnel(1, randomCell, map, 0.4f);
            }
        }

        protected void Shrapnel(int pwr, IntVec3 pos, Map map, float radius)
        {
            ThingDef def = this.def;
            Explosion(pwr, pos, map, radius, TMDamageDefOf.DamageDefOf.Iceshard, launcher, null, def, equipmentDef, TorannMagicDefOf.Mote_Base_Smoke, 0.4f, 1, false, null, 0f, 1);

        }

        public void Explosion(int pwr, IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
        {
            System.Random rnd = new System.Random();
            int modDamAmountRand = GenMath.RoundRandom(Rand.Range(4, 5 + TMDamageDefOf.DamageDefOf.Iceshard.defaultDamage));  //4
            modDamAmountRand = Mathf.RoundToInt(modDamAmountRand);
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
