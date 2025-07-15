using Verse;
using RimWorld;
using System.Collections.Generic;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Building_TMHeater : Building_WorkTable
    {

        private int nextSearch;
        private bool initialized;
        public bool defensive;
        public bool buffWarm;
        public bool boostJoy;

                
        protected override void Tick()
        {
            if(!initialized)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(280, 320);
                initialized = true;
            }
            if(Find.TickManager.TicksGame >= nextSearch)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(280, 320);
                if (defensive)
                {
                    Pawn e = TM_Calc.FindNearbyEnemy(Position, Map, Faction, 20, 0);
                    if (e != null && TM_Calc.HasLoSFromTo(Position, e, this, 0, 20))
                    {
                        Projectile firebolt = ThingMaker.MakeThing(ThingDef.Named("Projectile_Firebolt"), null) as Projectile;
                        TM_CopyAndLaunchProjectile.CopyAndLaunchProjectile(firebolt, this, e, e, ProjectileHitFlags.All, null);
                    }
                }
                if (buffWarm)
                {
                    List<Pawn> pList = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
                    if (pList != null && pList.Count > 0)
                    {
                        for (int i = 0; i < pList.Count; i++)
                        {
                            Pawn p = pList[i];
                            if (p.health != null && p.health.hediffSet != null)
                            {
                                HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_WarmHD, 0.18f);
                            }
                        }
                    }
                }
                if (boostJoy)
                {
                    List<Pawn> pList = TM_Calc.FindAllPawnsAround(Map, Position, 7, Faction, true);
                    if (pList != null && pList.Count > 0)
                    {
                        for (int i = 0; i < pList.Count; i++)
                        {
                            Pawn p = pList[i];
                            if (p.needs != null && p.needs.joy != null)
                            {
                                Need joy = p.needs.TryGetNeed(TorannMagicDefOf.Joy);
                                if(joy != null)
                                {
                                    joy.CurLevel += Rand.Range(.01f, .02f);
                                }
                            }
                        }
                    }
                }
            }
            base.Tick();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref defensive, "defensive", false, false);
            Scribe_Values.Look<bool>(ref boostJoy, "boostJoy", false, false);
            Scribe_Values.Look<bool>(ref buffWarm, "buffWarm", false, false);
            base.ExposeData();
        }
    }
}
