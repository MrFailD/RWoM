using RimWorld;
using Verse;
using Verse.Sound;

namespace TorannMagic.Buildings
{
    public class Building_LightningTrap : Building_ExplosiveProximityTrap
    {
        public bool ExtendedTrap;
        public bool IceTrap;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref ExtendedTrap, "extendedTrap",false, false);
            Scribe_Values.Look<bool>(ref IceTrap, "iceTrap", false, false);
        }

        protected override void Spring(Pawn p)
        {
            base.Spring(p);
            IntVec3 targetPos = Position;
            targetPos.z += 2;
            LocalTargetInfo t = targetPos;
            float speed = .8f;
            if(ExtendedTrap)
            {
                speed = .6f;
            }
            if (t.Cell != default(IntVec3))
            {
                Thing eyeThing = new Thing();
                eyeThing.def = TorannMagicDefOf.FlyingObject_LightningTrap;
                FlyingObject_LightningTrap flyingObject = (FlyingObject_LightningTrap)GenSpawn.Spawn(TorannMagicDefOf.FlyingObject_LightningTrap, Position, Map);
                flyingObject.Launch(p, Position.ToVector3Shifted(), t.Cell, eyeThing, Faction, null, speed);
            }
            if(IceTrap)
            {
                AddSnowRadial(Position, Map, 6, 1.1f);
            }
        }

        public static void AddSnowRadial(IntVec3 center, Map map, float radius, float depth)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = center + GenRadial.RadialPattern[i];
                if (intVec.InBoundsWithNullCheck(map))
                {
                    float lengthHorizontal = (center - intVec).LengthHorizontal;
                    float num2 = 1f - lengthHorizontal / radius;
                    map.snowGrid.AddDepth(intVec, num2 * depth);

                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = Map;
            base.Destroy(mode);
            InstallBlueprintUtility.CancelBlueprintsFor(this);
            if (mode == DestroyMode.Deconstruct)
            {
                SoundDef.Named("Building_Deconstructed").PlayOneShot(new TargetInfo(Position, map, false));
            }
        }
    }
}