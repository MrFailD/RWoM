using Verse;

namespace TorannMagic
{
    public class MightPowerSkill : IExposable
    {
        public string label;
        public string desc;
        public int level;
        public int levelMax;
        public int costToLevel = 1;

        public MightPowerSkill()
        {
        }

        public MightPowerSkill(string newLabel, string newDesc)
        {
            label = newLabel;
            desc = newDesc;
            level = 0;

            if (label.Contains("TM_BladeFocus") || label.Contains("TM_BladeArt") || label.Contains("TM_RangerTraining" ) || label.Contains("TM_BowTraining") || 
                label.Contains("TM_PsionicAugmentation") || label.Contains("TM_SniperFocus_pwr"))
            {
                costToLevel = 2;
            }

            if (newLabel == "TM_global_endurance_pwr")
            {
                levelMax = 50;
            }
            else if (newLabel == "TM_FieldTraining_pwr" || newLabel == "TM_FieldTraining_eff" || newLabel == "TM_FieldTraining_ver" || newLabel == "TM_PistolSpec_pwr" || newLabel == "TM_RifleSpec_pwr" || newLabel == "TM_ShotgunSpec_pwr")
            {
                levelMax = 15;
            }
            else if (newLabel == "TM_WayfarerCraft_pwr" || newLabel == "TM_WayfarerCraft_eff" || newLabel == "TM_WayfarerCraft_ver")
            {
                levelMax = 30;
            }
            else if (newLabel == "TM_global_refresh_pwr" || newLabel == "TM_global_seff_pwr" || newLabel == "TM_global_strength_pwr" || 
                newLabel == "TM_Shroud_pwr" || newLabel == "TM_Shroud_ver" || newLabel == "TM_Shroud_eff" ||
                label.Contains("TM_ShadowStrike") || label.Contains("TM_Nightshade") || label.Contains("TM_VeilOfShadows") ||
                label.Contains("TM_ShadowSlayer"))
            {
                levelMax = 5;
            }
            else if (newLabel.StartsWith("TM_Herbalist"))
            {
                levelMax = 10;
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
