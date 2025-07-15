using Verse;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMTotem_Lightning : Building
    {

        private int nextSearch;
        private float range = 40;
        private bool initialized;
        public int pwrVal;
        public int verVal;
        private Pawn target;

        protected override void Tick()
        {
            if(!initialized)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(120, 180);
                range = 40 + (4 * pwrVal);
                initialized = true;
            }
            else if(Find.TickManager.TicksGame >= nextSearch)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(120, 180);               

                ScanForTarget();
                if (target != null)
                {
                    if (TM_Calc.HasLoSFromTo(Position, target.Position, this, 0, range))
                    {
                        Projectile_Lightning bolt = ThingMaker.MakeThing(TorannMagicDefOf.Projectile_Lightning, null) as Projectile_Lightning;
                        bolt.pwrVal = pwrVal;
                        bolt.verVal = verVal;
                        TM_CopyAndLaunchProjectile.CopyAndLaunchProjectile(bolt, this, target, target, ProjectileHitFlags.All, null);
                    }
                }                               
            }
            base.Tick();
        }

        private void ScanForTarget()
        {            
            //Log.Message("totem has faction " + this.Faction);
            target = TM_Calc.FindNearbyEnemy(Position, Map, Faction, range, 5);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            base.ExposeData();
        }
    }
}
