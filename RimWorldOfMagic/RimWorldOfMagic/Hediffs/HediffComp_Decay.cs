using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Decay : HediffComp
    {
        private int tickAction = 21;
        private bool shouldRemove;
        private Pawn hediffInstigator;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref hediffInstigator, "hediffInstigator");
        }

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
            bool flag = Pawn.DestroyedOrNull();
            if (!flag)
            {
                if (Pawn.Spawned)
                {
                    if (Find.TickManager.TicksGame % tickAction == 0 && !Pawn.Dead && parent.Part != null && Pawn.health.hediffSet.GetPartHealth(parent.Part) > 0)
                    {
                        severityAdjustment -= Rand.Range(.05f, .1f);
                        if(Pawn.RaceProps != null)
                        {
                            bool damageArmor = false;
                            if(Pawn.RaceProps.Humanlike && Pawn.apparel != null && Pawn.apparel.WornApparel != null && Pawn.apparel.WornApparel.Count > 0)
                            {
                                for(int i = 0; i < Pawn.apparel.WornApparel.Count; i++)
                                {
                                    Apparel p = Pawn.apparel.WornApparel[i] as Apparel;
                                    if(p != null)
                                    {                                        
                                        if(p.def.apparel.CoversBodyPart(parent.Part) && p.HitPoints > 0)
                                        {
                                            p.HitPoints = Mathf.Clamp(p.HitPoints - Rand.Range(30, 40), 0, p.MaxHitPoints);
                                            damageArmor = true;
                                            FleckMaker.ThrowSmoke(Pawn.DrawPos, Pawn.Map, .6f);
                                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BlackSmoke, Pawn.DrawPos, Pawn.Map, Rand.Range(.4f, .6f), 1f, .1f, 2f, Rand.Range(-20, 20), Rand.Range(.25f, .5f), 0, Rand.Range(0, 360));
                                            break;
                                        }
                                    }
                                }
                            }
                            if(!damageArmor)
                            {
                                TM_Action.DamageEntities(Pawn, parent.Part, 10f, TMDamageDefOf.DamageDefOf.TM_DecayDD, hediffInstigator);
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BlackSmoke, Pawn.DrawPos, Pawn.Map, Rand.Range(.2f, .5f), 1f, .1f, 2f, Rand.Range(-20, 20), Rand.Range(.25f, .5f), 0, Rand.Range(0, 360));
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Disease, Pawn.DrawPos, Pawn.Map, Rand.Range(.2f, .5f), 1f, .1f, 2f, Rand.Range(-20, 20), Rand.Range(.25f, .5f), Rand.Range(0,360), Rand.Range(0, 360));
                            }                           
                        }
                    }

                    if (Pawn.health.hediffSet.GetPartHealth(parent.Part) <= 0)
                    {
                        shouldRemove = true;
                    }
                } 
            }
            base.CompPostTick(ref severityAdjustment);
        }

        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;
    }
}
