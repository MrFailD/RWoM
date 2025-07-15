using System.Linq;
using Verse;
using AbilityUser;
using UnityEngine;
using TorannMagic.Weapon;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class Projectile_ArcaneBolt : Projectile_AbilityBase
    {
        private int rotationOffset;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            Pawn pawn = launcher as Pawn;
            Pawn victim = hitThing as Pawn;
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            ExplosionHelper.Explode(Position, map, this.def.projectile.explosionRadius, TMDamageDefOf.DamageDefOf.TM_Arcane, launcher,  Mathf.RoundToInt(Rand.Range(5,this.def.projectile.GetDamageAmount(1, null))* comp.arcaneDmg), 1, this.def.projectile.soundExplode, def, equipmentDef, intendedTarget.Thing, null, 0f, 1, null, false, null, 0f, 1, 0.0f, false);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            rotationOffset += Rand.Range(20, 36);
            if(rotationOffset > 360)
            {
                rotationOffset = 0;
            }
            Mesh mesh = MeshPool.GridPlane(def.graphicData.drawSize);
            Graphics.DrawMesh(mesh, DrawPos, (Quaternion.AngleAxis(rotationOffset, Vector3.up) * ExactRotation), def.DrawMatSingle, 0);
            Comps_PostDraw();
        }

    }    
}


