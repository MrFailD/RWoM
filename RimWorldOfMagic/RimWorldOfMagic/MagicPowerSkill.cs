using System;
using Verse;

namespace TorannMagic
{
    public class MagicPowerSkill : IExposable
    {
        public string label;
        public string desc;
        public int level;
        public int levelMax;
        public int costToLevel = 1;

        public MagicPowerSkill()
        {
        }

        public MagicPowerSkill(string newLabel, string newDesc)
        {
            label = newLabel;
            desc = newDesc;
            level = 0;

            if(label == "TM_HolyWrath_ver" || label == "TM_HolyWrath_pwr" || label.Contains("TM_BardTraining") || label == "TM_Sentinel_pwr" || label == "TM_EnchanterStone_ver" || 
                label == "TM_Polymorph_ver" || label.Contains("TM_Shapeshift") || label == "TM_AlterFate_pwr" || label == "TM_LightSkip_pwr" || label.Contains("TM_ChaosTradition") ||
                label == "TM_RuneCarving_pwr")
            {
                costToLevel = 2;
            }

            if (newLabel == "TM_Firebolt_pwr")
            {
                levelMax = 6;
            }
            else if (newLabel == "TM_global_regen_pwr" || newLabel == "TM_global_eff_pwr" || newLabel == "TM_EarthSprites_pwr" || newLabel == "TM_Prediction_pwr" || newLabel == "TM_GuardianSpirit_pwr" ||
                newLabel == "TM_Golemancy_pwr" || newLabel == "TM_Golemancy_eff" || newLabel == "TM_Golemancy_ver")
            {
                levelMax = 5;
            }
            else if (newLabel == "TM_Blink_eff" || newLabel == "TM_Summon_eff" || newLabel == "TM_AdvancedHeal_pwr" || newLabel == "TM_AdvancedHeal_ver" || newLabel == "TM_HealingCircle_pwr")
            {
                levelMax = 4;
            }
            else if (newLabel == "TM_global_spirit_pwr")
            {
                levelMax = 50;
            }
            else if (newLabel == "TM_TechnoBit_pwr" || newLabel == "TM_TechnoBit_ver" || newLabel == "TM_TechnoBit_eff" || newLabel == "TM_TechnoTurret_pwr" || newLabel == "TM_TechnoTurret_ver" || newLabel == "TM_TechnoTurret_eff" || newLabel == "TM_TechnoWeapon_pwr" || newLabel == "TM_TechnoWeapon_ver" || newLabel == "TM_TechnoWeapon_eff" ||
                 newLabel == "TM_Cantrips_pwr" || newLabel == "TM_Cantrips_eff" || newLabel == "TM_Cantrips_ver" || newLabel == "TM_Totems_pwr" || newLabel == "TM_Totems_eff" || newLabel == "TM_Totems_ver" ||
                 newLabel == "TM_SpiritOfLight_pwr" || newLabel == "TM_SpiritOfLight_eff" || newLabel == "TM_SpiritOfLight_ver" || newLabel == "TM_Cantrips_pwr" || newLabel == "TM_Cantrips_eff" || newLabel == "TM_Cantrips_ver")
            {
                levelMax = 15;
            }
            else if (newLabel == "TM_WandererCraft_pwr" || newLabel == "TM_WandererCraft_eff" || newLabel == "TM_WandererCraft_ver")
            {
                levelMax = 30;
            }
            else if (newLabel == "TM_Sentinel_pwr" || newLabel == "TM_LightSkip_pwr")
            {
                levelMax = 2;
            }
            else
            {
                levelMax = 3;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref label, "label", "default", false);
            Scribe_Values.Look<string>(ref desc, "desc", "default", false);
            Scribe_Values.Look<int>(ref level, "level", 0, false);
            Scribe_Values.Look<int>(ref costToLevel, "costToLevel", 1, false);
            Scribe_Values.Look<int>(ref levelMax, "levelMax", 0, false);
        }

    }
}
