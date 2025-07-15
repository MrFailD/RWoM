using AbilityUser;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TorannMagic 
{
    public class MightPower : IExposable
    {
        public List<AbilityDef> TMabilityDefs;
        public TMDefs.TM_Autocast autocasting;

        public int ticksUntilNextCast = -1;

        public int level;

        public bool learned;
        public bool autocast;
        public int learnCost = 2;
        private int interactionTick;
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
                if (TMabilityDefs != null && TMabilityDefs.Count > 0)
                {
                    SetMaxLevel();
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
            //
            AbilityDef result = null;
            bool flag = TMabilityDefs != null && TMabilityDefs.Count > 0;
            if (flag)
            {
                result = TMabilityDefs[0];
                bool flag2 = index > -1 && index < TMabilityDefs.Count;
                if (flag2)
                {
                    result = TMabilityDefs[index];
                }
                else
                {
                    bool flag3 = index >= TMabilityDefs.Count;
                    if (flag3)
                    {
                        result = TMabilityDefs[TMabilityDefs.Count - 1];
                    }
                }
            }
            return result;
        }

        public AbilityDef HasAbilityDef(AbilityDef defToFind)
        {
            return TMabilityDefs.FirstOrDefault((AbilityDef x) => x == defToFind);
        }

        public MightPower()
        {
        }

        public MightPower(List<AbilityDef> newAbilityDefs)
        {
            level = 0;
            TMabilityDefs = newAbilityDefs;
            maxLevel = newAbilityDefs.Count - 1;            

            if (abilityDef == TorannMagicDefOf.TM_PsionicBarrier || abilityDef == TorannMagicDefOf.TM_PsionicBarrier_Projected)
            {
                learnCost = 2;
                costToLevel = 2;
                maxLevel = 1;
            }

            if (abilityDef == TorannMagicDefOf.TM_PistolSpec || abilityDef == TorannMagicDefOf.TM_RifleSpec || abilityDef == TorannMagicDefOf.TM_ShotgunSpec)
            {
                learnCost = 0;
            }

            LoadLegacyClassAutocast();

        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref learned, "learned", true, false);
            Scribe_Values.Look<bool>(ref autocast, "autocast", false, false);
            Scribe_Values.Look<int>(ref learnCost, "learnCost", 2, false);
            Scribe_Values.Look<int>(ref costToLevel, "costToLevel", 1, false);
            Scribe_Values.Look<int>(ref level, "level", 0, false);
            Scribe_Values.Look<int>(ref maxLevel, "maxLevel", 3, false);
            Scribe_Values.Look<int>(ref ticksUntilNextCast, "ticksUntilNextCast", -1, false);
            Scribe_Collections.Look<AbilityDef>(ref TMabilityDefs, "TMabilityDefs", (LookMode)4, null);
            Scribe_Deep.Look<TMDefs.TM_Autocast>(ref autocasting, "autocasting", new object[0]);
        }

        public void LoadLegacyClassAutocast()
        {
            if (abilityDef == TorannMagicDefOf.TM_Headshot)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 36
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_ShadowStrike ||
                abilityDef == TorannMagicDefOf.TM_Spite ||
                abilityDef == TorannMagicDefOf.TM_DisablingShot)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 20
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_Nightshade)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnSelf,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = true,
                    targetEnemy = true,
                    targetNeutral = true,
                    targetFriendly = true,
                    targetNoFaction = true,
                    maxRange = 20,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_DoesNotHaveNightshadeHediff"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_GraveBlade)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
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
                    minRange = 10,
                    advancedConditionDefs = new List<string>
                    {
                        "TM_3EnemiesWithin15Cells"
                    }
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_WaveOfFear ||
                abilityDef == TorannMagicDefOf.TM_BladeSpin)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnSelf,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
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
            if (abilityDef == TorannMagicDefOf.TM_Whirlwind ||
                abilityDef == TorannMagicDefOf.TM_DragonStrike ||
                abilityDef == TorannMagicDefOf.TM_PsionicDash)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 15,
                    minRange = 3
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_PhaseStrike)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = false,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 25,
                    minRange = 3
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_TigerStrike ||
                abilityDef == TorannMagicDefOf.TM_ThunderStrike)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnTarget,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 1.4f,
                };
            }
            if (abilityDef == TorannMagicDefOf.TM_PsionicBlast)
            {
                autocasting = new TMDefs.TM_Autocast
                {
                    type = TMDefs.AutocastType.OnNearby,
                    targetType = "Pawn",
                    mightUser = true,
                    magicUser = false,
                    drafted = false,
                    undrafted = true,
                    requiresLoS = true,
                    AIUsable = true,
                    includeSelf = false,
                    targetEnemy = true,
                    targetNeutral = false,
                    targetFriendly = false,
                    targetNoFaction = false,
                    maxRange = 25,
                    minRange = 3
                };
            }
        }
    }
}
