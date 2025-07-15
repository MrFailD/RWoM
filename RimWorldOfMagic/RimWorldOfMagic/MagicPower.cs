using AbilityUser;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TorannMagic 
{
    public class MagicPower : IExposable
    {
        public List<AbilityDef> TMabilityDefs;
        public TMDefs.TM_Autocast autocasting;

        public int ticksUntilNextCast = -1;

        public int level;
        public bool learned;
        public bool autocast;
        public int learnCost = 2;
        private int interactionTick;
        public bool requiresScroll;
        public int maxLevel = 3;
        public int costToLevel = 1;
        
        public bool AutoCast
        {
            get
            {
                return autocast;
            }
            set
            {
                if (interactionTick < Find.TickManager.TicksGame)
                {
                    autocast = value;
                    interactionTick = Find.TickManager.TicksGame + 5;
                }
            }

        }

        private void SetMaxLevel()
        {
            maxLevel = TMabilityDefs.Count - 1;
        }

        public AbilityDef abilityDescDef
        {
            get
            {
                return abilityDef;                
            }
        }

        public AbilityDef nextLevelAbilityDescDef
        {
            get
            {
                return nextLevelAbilityDef;                
            }
        }

        public AbilityDef abilityDef
        {
            get
            {
                SetMaxLevel();
                if (TMabilityDefs != null && TMabilityDefs.Count > 0)
                {
                    if (level <= 0)
                    {
                        return TMabilityDefs[0];
                    }
                    else if (level >= maxLevel)
                    {
                        return TMabilityDefs[maxLevel];
                    }
                    return TMabilityDefs[level];
                }
                return null;
            }
        }

        public AbilityDef nextLevelAbilityDef
        {
            get
            {
                SetMaxLevel();
                if ((level + 1) >= maxLevel)
                {
                    return TMabilityDefs[maxLevel];
                }
                else
                {
                    return TMabilityDefs[level + 1];
                }               
            }
        }

        public Texture2D Icon
        {
            get
            {
                return abilityDef.uiIcon;
            }
        }

        public AbilityDef GetAbilityDef(int index)
        {
            try
            {
                return TMabilityDefs[index];
            }
            catch
            {
                return TMabilityDefs[0];
            }            
        }

        public AbilityDef HasAbilityDef(AbilityDef defToFind)
        {
            return TMabilityDefs.FirstOrDefault((AbilityDef x) => x == defToFind);
        }

        public MagicPower()
        {
        }

        public MagicPower(List<AbilityDef> newAbilityDefs, bool requireScrollToLearn = false)
        {
            level = 0;
            requiresScroll = requireScrollToLearn;
            TMabilityDefs = newAbilityDefs;
            maxLevel = newAbilityDefs.Count - 1;

            if (abilityDef.defName == "TM_TechnoBit" || abilityDef.defName == "TM_TechnoTurret" || abilityDef.defName == "TM_TechnoWeapon")
            {
                learnCost = 0;
            }

            if (abilityDef.defName == "TM_TechnoShield" || abilityDef.defName == "TM_Sabotage" || abilityDef.defName == "TM_Overdrive")
            {
                learnCost = 99;
            }

            if (abilityDef.defName == "TM_Firebolt" || abilityDef.defName == "TM_Icebolt" || abilityDef.defName == "TM_Rainmaker" || abilityDef.defName == "TM_LightningBolt" ||
                abilityDef.defName == "TM_Blink" || abilityDef.defName == "TM_Summon" || abilityDef.defName == "TM_Heal" || abilityDef.defName == "TM_SummonExplosive" ||
                abilityDef.defName == "TM_SummonPylon" || abilityDef.defName == "TM_Poison" || abilityDef.defName == "TM_FogOfTorment" || abilityDef.defName == "TM_AdvancedHeal" ||
                abilityDef.defName == "TM_CorpseExplosion" || abilityDef.defName == "TM_Entertain" || abilityDef.defName == "TM_Encase" || abilityDef.defName == "TM_EarthernHammer")
            {
                learnCost = 1;
            }

            if(abilityDef.defName == "TM_Fireball" || abilityDef.defName == "TM_LightningStorm" || abilityDef.defName == "TM_SummonElemental" || abilityDef == TorannMagicDefOf.TM_DeathBolt ||
                abilityDef == TorannMagicDefOf.TM_Sunfire || abilityDef == TorannMagicDefOf.TM_Refraction || abilityDef == TorannMagicDefOf.TM_ChainLightning)
            {
                learnCost = 3;
            }

            LoadLegacyClassAutocast();
        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref learned, "learned", true, false);
            Scribe_Values.Look<bool>(ref autocast, "autocast", false, false);
            Scribe_Values.Look<bool>(ref requiresScroll, "requiresScroll", false, false);
            Scribe_Values.Look<int>(ref learnCost, "learnCost", 2, false);
            Scribe_Values.Look<int>(ref costToLevel, "costToLevel", 1, false);
            Scribe_Values.Look<int>(ref level, "level", 0, false);
            Scribe_Values.Look<int>(ref maxLevel, "maxLevel", 3, false);
            Scribe_Values.Look<int>(ref ticksUntilNextCast, "ticksUntilNextCast", -1, false);
            Scribe_Collections.Look<AbilityDef>(ref TMabilityDefs, "TMabilityDefs", LookMode.Def, null);
            Scribe_Deep.Look<TMDefs.TM_Autocast>(ref autocasting, "autocasting", new object[0]);
        }

        public void LoadLegacyClassAutocast()
        {
            if (abilityDef == TorannMagicDefOf.TM_RaiseUndead)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Corpse",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = true,
                    targetFriendly = true,
                    targetNoFaction = true,
                    hostileCasterOnly = true,
                    maxRange = 20
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_DeathMark)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 30
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_FogOfTorment || 
                abilityDef == TorannMagicDefOf.TM_LightningCloud || 
                abilityDef == TorannMagicDefOf.TM_IgniteBlood ||
                abilityDef == TorannMagicDefOf.TM_Attraction ||
                abilityDef == TorannMagicDefOf.TM_Repulsion ||
                abilityDef == TorannMagicDefOf.TM_HolyWrath)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 40,
                    minRange = 20,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_3EnemiesWithin15Cells"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Fireball ||
                abilityDef == TorannMagicDefOf.TM_Snowball ||
                abilityDef == TorannMagicDefOf.TM_Blizzard ||
                abilityDef == TorannMagicDefOf.TM_Firestorm ||
                abilityDef == TorannMagicDefOf.TM_ChainLightning)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 55,
                    minRange = 20,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_3EnemiesWithin15Cells"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_LightningBolt ||
               abilityDef == TorannMagicDefOf.TM_ShadowBolt ||
               abilityDef == TorannMagicDefOf.TM_Firebolt ||
               abilityDef == TorannMagicDefOf.TM_Icebolt)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 35,
                    minRange = 5
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_MagicMissile ||
                abilityDef == TorannMagicDefOf.TM_FrostRay)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 22
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Dominate)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 45
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Scorn)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 290,
                    minRange = 30
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_ValiantCharge ||
                abilityDef == TorannMagicDefOf.TM_SummonTotemEarth)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 30,
                    minRange = 3
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Overwhelm)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnSelf,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = true,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = true,
                    targetNoFaction = false,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_3EnemiesWithin15Cells"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_SummonTotemLightning)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnSelf,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = true,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = true,
                    targetNoFaction = false,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_1EnemyWithin30Cells"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Enrage ||
                abilityDef == TorannMagicDefOf.TM_AMP)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = false,
                    magicUser = true,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = true,
                    targetEnemy = false,
                    targetNeutral = false,
                    targetFriendly = true,
                    targetNoFaction = false,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_1EnemyWithin15Cells"
                    }
                };
            }
        }
    }
}
