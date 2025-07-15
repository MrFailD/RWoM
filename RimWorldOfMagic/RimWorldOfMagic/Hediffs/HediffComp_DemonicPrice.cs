using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_DemonicPrice : HediffComp
    {
        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }


        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn.DestroyedOrNull();
            if (!flag)
            {
                if (Pawn.Spawned && !Pawn.Dead)
                {
                    if (Find.TickManager.TicksGame % 4 == 0)
                    {
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ArcaneFlame, Pawn.DrawPos, Pawn.Map, Rand.Range(.2f, .3f), .1f, .05f, .2f, 0, Rand.Range(1.5f, 2f), Rand.Range(-60, 60), 0);
                    }    
                    if(Find.TickManager.TicksGame % 60 == 0)
                    {
                        DamageEntities(Pawn, Rand.Range(4, 8), DamageDefOf.Flame);
                    }
                }
            }
        }

        public void DamageEntities(Thing e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }
}
