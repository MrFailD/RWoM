using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using UnityEngine;
using TorannMagic.Golems;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_DoomField : HediffComp
    {
        private int tickAction = 901;
        private bool shouldRemove = false;
        private float energyCost = 8f;

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
                    if ((Find.TickManager.TicksGame % tickAction == 0) && !Pawn.Dead && Pawn.Map != null)
                    {
                        TMHollowGolem hg = Pawn as TMHollowGolem;
                        if (hg != null && hg.doomFieldHediffEnabled)
                        {
                            TryDoomCurse(hg);
                        }
                    }
                } 
            }
            base.CompPostTick(ref severityAdjustment);
        }

        public void TryDoomCurse(TMHollowGolem caster)
        {
            foreach (Pawn p in caster.Map.mapPawns.AllPawnsSpawned)
            {
                if(p != null && p.HostileTo(caster) && p.health != null && p.health.hediffSet != null && !p.health.hediffSet.HasHediff(TorannMagicDefOf.TM_GravitySlowHD) && (p.Position - caster.Position).LengthHorizontal <= 36f)
                {
                    if (caster.Golem.Energy != null && caster.Golem.Energy.CurLevelPercentage >= caster.Golem.minEnergyPctForAbilities && caster.Golem.Energy.CurLevel >= energyCost)
                    {
                        caster.Golem.Energy.SubtractEnergy(energyCost);
                        Effecter AttractionEffect = TorannMagicDefOf.TM_AttractionEffecterSmall.Spawn();
                        AttractionEffect.Trigger(new TargetInfo(p.Position, p.Map, false), new TargetInfo(p.Position, p.Map, false));
                        AttractionEffect.Cleanup();
                        HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_GravitySlowHD, 1.5f);
                    }
                }
            }
        }

        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;
    }
}
