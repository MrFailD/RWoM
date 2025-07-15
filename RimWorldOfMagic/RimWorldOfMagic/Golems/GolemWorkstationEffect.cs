using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using TorannMagic.TMDefs;

namespace TorannMagic.Golems
{
    public class GolemWorkstationEffect : IExposable
    {
        public int ticksTillNextEffect80 = 100;
        public int effectDuration;
        public SoundDef effectSound;
        public float effectFlashScale;
        public bool doEffectEachBurst = false;
        public float energyCost;
        public bool alwaysDraw;
        public bool requiresTarget;
        public float effectLevelModifier;
        public Vector3 drawOffset = new Vector3(0, 0, 0);
        public List<TM_GolemItemRecipeDef> recipes = new List<TM_GolemItemRecipeDef>();

        public LocalTargetInfo target = null;
        public Building_TMGolemBase parent;
        public TM_GolemUpgrade parentUpgrade;
        public int startTick;
        public int nextEffectTick;
        public float currentLevel;
        public int effectFrequency = 0;
        public int chargesRequired = 0;

        public bool EffectActive => (startTick + effectDuration) > Find.TickManager.TicksGame;

        public virtual float LevelModifier => (1f + (effectLevelModifier * currentLevel));

        public virtual void ExposeData()
        {
            Scribe_Values.Look<int>(ref startTick, "startTick");
            Scribe_Values.Look<int>(ref nextEffectTick, "nextEffectTick");
            Scribe_Values.Look<float>(ref currentLevel, "currentLevel");

            Scribe_Values.Look<int>(ref ticksTillNextEffect80, "ticksTillNextEffect80");
            Scribe_Values.Look<int>(ref effectDuration, "effectDuration");
            Scribe_Values.Look<float>(ref effectFlashScale, "effectFlashScale");
            Scribe_Values.Look<float>(ref energyCost, "energyCost");
            Scribe_Values.Look<float>(ref effectLevelModifier, "effectLevelModifier");
            Scribe_Values.Look<bool>(ref alwaysDraw, "alwaysDraw", false);
            Scribe_Values.Look<bool>(ref requiresTarget, "requiresTarget");
            Scribe_Defs.Look<SoundDef>(ref effectSound, "effectSound");
        }

        public virtual void StartEffect(Building_TMGolemBase golem_building, TM_GolemUpgrade upgrade, float effectLevel = 1)
        {
            parent = golem_building;
            parentUpgrade = upgrade;
            currentLevel = effectLevel;
            if(effectFlashScale > 0.01f)
            {
                FleckMaker.Static(golem_building.Position, golem_building.Map, FleckDefOf.ShotFlash, effectFlashScale);
            }
            startTick = Find.TickManager.TicksGame;
            nextEffectTick = Find.TickManager.TicksGame + Mathf.RoundToInt(Rand.Range(.8f, 1.2f) * ticksTillNextEffect80 * golem_building.GolemComp.ProcessingModifier);
            effectSound?.PlayOneShot(new TargetInfo(golem_building.Position, golem_building.Map));
            CompGolemEnergyHandler cgeh = golem_building.TryGetComp<CompGolemEnergyHandler>();
            if (energyCost > 0 && cgeh != null)
            {
                cgeh.AddEnergy(-energyCost);
            }            
        }

        public virtual void ContinueEffect(Building_TMGolemBase golem_building)
        {
            if (doEffectEachBurst && effectFlashScale > 0.01f)
            {
                FleckMaker.Static(golem_building.Position, golem_building.Map, FleckDefOf.ShotFlash, effectFlashScale);
            }
        }

        public virtual bool CanDoEffect(Building_TMGolemBase golem_building)
        {
            CompGolemEnergyHandler cgeh = golem_building.TryGetComp<CompGolemEnergyHandler>();
            if(energyCost > 0 && cgeh != null)
            {
                if(cgeh.StoredEnergy < golem_building.ActualEnergyCost(energyCost))
                {
                    return false;
                }
            }
            return nextEffectTick <= Find.TickManager.TicksGame;
        }
    }
}
