using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Regeneration : HediffComp
    {

        private bool initialized;
        private int age;
        private int regenRate = 300;
        private int lastRegen;
        private int hediffPwr;

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
                FleckMaker.ThrowLightningGlow(Pawn.DrawPos, Pawn.Map, 1f);
                if (Def == TorannMagicDefOf.TM_Regeneration_III)
                {
                    hediffPwr = 3;
                }
                else if (Def == TorannMagicDefOf.TM_Regeneration_II)
                {
                    hediffPwr = 2;
                }
                else if (Def == TorannMagicDefOf.TM_Regeneration_I)
                {
                    hediffPwr = 1;
                }
                else
                {
                    hediffPwr = 0;
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn != null & parent != null)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
            }
            age++;
            if(!Pawn.DestroyedOrNull() && !Pawn.Dead)
            {
                if (age > lastRegen + regenRate)
                {
                    HealthUtility.AdjustSeverity(Pawn, Def, -0.3f);
                    lastRegen = age;
                    Pawn pawn = Pawn;

                    TM_MoteMaker.ThrowRegenMote(pawn.DrawPos, pawn.Map, 1f);

                    if (!TM_Calc.IsUndead(pawn))
                    {
                        
                        int injuriesToHeal = ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist ? 2 : 1;
                        IEnumerable<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
                            .OfType<Hediff_Injury>()
                            .Where(injury => injury.CanHealNaturally())
                            .Take(injuriesToHeal);

                        float amountToHeal = pawn.IsColonist ? 4f + .5f * hediffPwr : 10f + 1.5f * hediffPwr;
                        foreach (Hediff_Injury injury in injuries)
                        {
                            injury.Heal(amountToHeal);
                        }                        
                    }
                    else
                    {
                        TM_Action.DamageUndead(pawn, (2f + 1f * hediffPwr), null);
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", 0, false);
            Scribe_Values.Look<int>(ref regenRate, "regenRate", 300, false);
            Scribe_Values.Look<int>(ref lastRegen, "lastRegen", 0, false);
            Scribe_Values.Look<int>(ref hediffPwr, "hediffPwr", 0, false);
        }

    }
}
