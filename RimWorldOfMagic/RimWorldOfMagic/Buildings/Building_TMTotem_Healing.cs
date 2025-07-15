using UnityEngine;
using Verse;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMTotem_Healing : Building
    {

        private int nextSearch;
        private float range = 40;
        private bool initialized;
        public int pwrVal;
        public int verVal;
        public float arcanePwr = 1f;
        private Pawn target;

        protected override void Tick()
        {
            if(!initialized)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(30, 40);
                range = 20 + pwrVal;
                initialized = true;
            }
            else if(Find.TickManager.TicksGame >= nextSearch)
            {
                nextSearch = Find.TickManager.TicksGame + Rand.Range(60, 70);

                target = TM_Calc.FindNearbyInjuredPawn(Position, Map, Faction, (int)range, 0f, true);
                if (target != null)
                {
                    TM_Action.DoAction_HealPawn(null, target, 1, Rand.Range(1f, 3f) * arcanePwr * (1f + (.04f * pwrVal)));
                    Vector3 totemPos = DrawPos;
                    totemPos.z += 1.3f;
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Healing_Small, totemPos, Map, 1.5f, .6f, .1f, 1f, 0, 0, 0, 0);
                    for (int i = 0; i < 2; i++)
                    {
                        Vector3 pos = target.DrawPos;
                        pos.x += Rand.Range(-.3f, .3f);
                        pos.z += Rand.Range(-.25f, .5f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Healing_Small, pos, Map, (.2f * (1f + (i*.5f))), Rand.Range(.05f, .15f), Rand.Range(.05f, .25f), Rand.Range(.1f, .3f), 0, 0, 0, 0);
                    }                    
                }                               
            }
            base.Tick();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<float>(ref arcanePwr, "arcanePwr", 1f, false);
            base.ExposeData();
        }
    }
}
