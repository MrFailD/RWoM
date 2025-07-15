using AbilityUser;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TorannMagic.TMDefs;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class MightData : IExposable
    {
        private Pawn mightPawn;        
        private int mightUserLevel;
        private int mightAbilityPoints;
        private int mightUserXP = 1;
        private int ticksToLearnMightXP = -1;
        public bool initialized;
        private Faction affiliation;
        private int ticksAffiliation;
        //public ThingOwner<ThingWithComps> equipmentContainer = new ThingOwner<ThingWithComps>();


        public bool customPowersInitialized;
        public List<MightPower> mightPowerCustom;
        public List<MightPower> MightPowersCustom  //supports customs abilities
        {
            get
            {
                bool flag = mightPowerCustom == null; // || !this.customPowersInitialized;
                if (flag)
                {
                    customPowersInitialized = true;
                    IEnumerable<TM_CustomPowerDef> enumerable = TM_Data.CustomFighterPowerDefs();

                    if (mightPowerCustom == null || mightPowerCustom.Count <= 0)
                    {
                        mightPowerCustom = new List<MightPower>();
                        mightPowerCustom.Clear();
                    }                    
                    foreach (TM_CustomPowerDef current in enumerable)
                    {
                        bool newPower = false;
                        List<AbilityUser.AbilityDef> abilityList = current.customPower.abilityDefs;
                        MightPower mp = new MightPower(abilityList);
                        mp.maxLevel = current.customPower.maxLevel;
                        mp.learnCost = current.customPower.learnCost;
                        mp.costToLevel = current.customPower.costToLevel;
                        mp.autocasting = current.customPower.autocasting;
                        if (!mightPowerCustom.Any(a => a.GetAbilityDef(0) == mp.GetAbilityDef(0)))
                        {
                            newPower = true;
                        }                         
                        bool hasSkills = false;
                        if (current.customPower.skills != null)
                        {
                            foreach (TM_CustomSkill skill in current.customPower.skills)
                            {
                                MightPowerSkill mps = new MightPowerSkill(skill.label, skill.description);
                                mps.levelMax = skill.levelMax;
                                mps.costToLevel = skill.costToLevel;
                                if (!AllMightPowerSkills.Any(b => b.label == mps.label) && !MightPowerSkill_Custom.Any(b => b.label == mps.label))
                                {
                                    MightPowerSkill_Custom.Add(mps);
                                }
                                hasSkills = true;
                            }
                        }
                        if (newPower)
                        {
                            if (hasSkills)
                            {
                                mightPowerCustom.Add(mp);
                            }
                            else if(!MightPowersCustomStandalone.Any((a => a.GetAbilityDef(0) == mp.GetAbilityDef(0))))
                            {
                                mightPowerCustomStandalone.Add(mp);
                            }
                        }
                    }
                    allMightPowerSkills = null; //force rediscovery and caching to include custom defs
                }
                return mightPowerCustom;
            }
        }

        public List<MightPower> mightPowerCustomStandalone;
        public List<MightPower> MightPowersCustomStandalone  //supports customs abilities
        {
            get
            {
                bool flag = mightPowerCustomStandalone == null;
                if (flag)
                {
                    mightPowerCustomStandalone = new List<MightPower>();
                    mightPowerCustomStandalone.Clear();
                }
                return mightPowerCustomStandalone;
            }
        }

        private List<MightPower> mightPowerCustomAll;
        public List<MightPower> MightPowersCustomAll
        {
            get
            {
                if(mightPowerCustomAll == null)
                {
                    mightPowerCustomAll = new List<MightPower>();
                    mightPowerCustomAll.Clear();
                    mightPowerCustomAll.AddRange(MightPowersCustom);
                    mightPowerCustomAll.AddRange(MightPowersCustomStandalone);                    
                }
                return mightPowerCustomAll;
            }
        }

        public List<MightPowerSkill> mightPowerSkill_Custom;
        public List<MightPowerSkill> MightPowerSkill_Custom
        {
            get
            {
                bool flag = mightPowerSkill_Custom == null;
                if (flag)
                {
                    mightPowerSkill_Custom = new List<MightPowerSkill>();
                    mightPowerSkill_Custom.Clear();
                }
                return mightPowerSkill_Custom;
            }
        }

        public List<MightPower> mightPowerStandalone;
        public List<MightPower> MightPowersStandalone  //skills needed for mightcpower reference during autocast
        {
            get
            {
                bool flag = mightPowerStandalone == null;
                if (flag)
                {
                    mightPowerStandalone = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_TeachMight
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_GearRepair
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_InnerHealing
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_HeavyBlow
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_StrongBack
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ThickSkin
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_FightersFocus
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ThrowingKnife
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BurningFury
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PommelStrike
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Legion
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_TempestStrike
                        }),                        
                    };
                }
                return mightPowerStandalone;
            }
        }

        public List<MightPower> mightPowerApothecary;
        public List<MightPowerSkill> mightPowerSkill_Herbalist;
        public List<MightPowerSkill> mightPowerSkill_PoisonFlask;
        public List<MightPowerSkill> mightPowerSkill_Elixir;
        public List<MightPowerSkill> mightPowerSkill_SoothingBalm;

        public List<MightPower> MightPowersApothecary
        {
            get
            {
                bool flag = mightPowerApothecary == null;
                if (flag)
                {
                    mightPowerApothecary = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Herbalist
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PoisonFlask
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Elixir
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_SoothingBalm
                        }),
                    };
                }
                return mightPowerApothecary;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Herbalist
        {
            get
            {
                bool flag = mightPowerSkill_Herbalist == null;
                if (flag)
                {
                    mightPowerSkill_Herbalist = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Herbalist_pwr", "TM_Herbalist_pwr_desc"), // increases maximum herb count by 10 per lvl
                        new MightPowerSkill("TM_Herbalist_eff", "TM_Herbalist_eff_desc"), // ??
                        new MightPowerSkill("TM_Herbalist_ver", "TM_Herbalist_ver_desc")  // increases amount of herbs gained per harvest by 5%
                    };
                }
                return mightPowerSkill_Herbalist;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PoisonFlask
        {
            get
            {
                bool flag = mightPowerSkill_PoisonFlask == null;
                if (flag)
                {
                    mightPowerSkill_PoisonFlask = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PoisonFlask_pwr", "TM_PoisonFlask_pwr_desc"), // duration of cloud
                        new MightPowerSkill("TM_PoisonFlask_eff", "TM_PoisonFlask_eff_desc"), // herb efficiency
                        new MightPowerSkill("TM_PoisonFlask_ver", "TM_PoisonFlask_ver_desc")  // radius of poison cloud
                    };
                }
                return mightPowerSkill_PoisonFlask;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Elixir
        {
            get
            {
                bool flag = mightPowerSkill_Elixir == null;
                if (flag)
                {
                    mightPowerSkill_Elixir = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Elixir_pwr", "TM_Elixir_pwr_desc"), // severity applied 
                        new MightPowerSkill("TM_Elixir_eff", "TM_Elixir_eff_desc"), // reduces herb consumption
                        new MightPowerSkill("TM_Elixir_ver", "TM_Elixir_ver_desc")  // variations of healing
                    };
                }
                return mightPowerSkill_Elixir;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_SoothingBalm
        {
            get
            {
                bool flag = mightPowerSkill_SoothingBalm == null;
                if (flag)
                {
                    mightPowerSkill_SoothingBalm = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_SoothingBalm_pwr", "TM_SoothingBalm_pwr_desc"), // severity applied
                        new MightPowerSkill("TM_SoothingBalm_eff", "TM_SoothingBalm_eff_desc"), // herb efficiency
                        new MightPowerSkill("TM_SoothingBalm_ver", "TM_SoothingBalm_ver_desc")  // ?
                    };
                }
                return mightPowerSkill_SoothingBalm;
            }
        }

        public List<MightPower> mightPowerShadow;
        public List<MightPowerSkill> mightPowerSkill_ShadowStrike;
        public List<MightPowerSkill> mightPowerSkill_Nightshade;
        public List<MightPowerSkill> mightPowerSkill_VeilOfShadows;

        public List<MightPower> MightPowersShadow
        {
            get
            {
                bool flag = mightPowerShadow == null;
                if (flag)
                {
                    mightPowerShadow = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ShadowStrike
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Nightshade
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_VeilOfShadows
                        }),                        
                    };
                }
                return mightPowerShadow;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ShadowStrike
        {
            get
            {
                bool flag = mightPowerSkill_ShadowStrike == null;
                if (flag)
                {
                    mightPowerSkill_ShadowStrike = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ShadowStrike_pwr", "TM_ShadowStrike_pwr_desc"), // increases strike damage
                        new MightPowerSkill("TM_ShadowStrike_eff", "TM_ShadowStrike_eff_desc"), // reduces stamina cost
                        new MightPowerSkill("TM_ShadowStrike_ver", "TM_ShadowStrike_ver_desc")  // invisibility and toxin cloud
                    };
                }
                return mightPowerSkill_ShadowStrike;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Nightshade
        {
            get
            {
                bool flag = mightPowerSkill_Nightshade == null;
                if (flag)
                {
                    mightPowerSkill_Nightshade = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Nightshade_pwr", "TM_Nightshade_pwr_desc"), // amount of severity to apply each strike
                        new MightPowerSkill("TM_Nightshade_eff", "TM_Nightshade_eff_desc"), // less stamina and mana to maintain
                        new MightPowerSkill("TM_Nightshade_ver", "TM_Nightshade_ver_desc")  // max severity
                    };
                }
                return mightPowerSkill_Nightshade;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_VeilOfShadows
        {
            get
            {
                bool flag = mightPowerSkill_VeilOfShadows == null;
                if (flag)
                {
                    mightPowerSkill_VeilOfShadows = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_VeilOfShadows_pwr", "TM_VeilOfShadows_pwr_desc"), // duration of invisibility
                        new MightPowerSkill("TM_VeilOfShadows_eff", "TM_VeilOfShadows_eff_desc"), // less stamina cost
                        new MightPowerSkill("TM_VeilOfShadows_ver", "TM_VeilOfShadows_ver_desc")  // manipulation and movement speed buff potency
                    };
                }
                return mightPowerSkill_VeilOfShadows;
            }
        }

        public List<MightPower> mightPowerSS;
        public List<MightPowerSkill> mightPowerSkill_PistolSpec;
        public List<MightPowerSkill> mightPowerSkill_RifleSpec;
        public List<MightPowerSkill> mightPowerSkill_ShotgunSpec;
        public List<MightPowerSkill> mightPowerSkill_CQC;
        public List<MightPowerSkill> mightPowerSkill_FirstAid;
        public List<MightPowerSkill> mightPowerSkill_60mmMortar;

        public List<MightPower> MightPowersSS
        {
            get
            {
                bool flag = mightPowerSS == null;
                if (flag)
                {
                    mightPowerSS = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PistolSpec
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_RifleSpec
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ShotgunSpec
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_CQC
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_FirstAid
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_60mmMortar
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PistolWhip
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_SuppressingFire
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Mk203GL
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Buckshot
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BreachingCharge
                        }),
                    };
                }
                return mightPowerSS;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PistolSpec
        {
            get
            {
                bool flag = mightPowerSkill_PistolSpec == null;
                if (flag)
                {
                    mightPowerSkill_PistolSpec = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PistolSpec_pwr", "TM_PistolSpec_pwr_desc"), //
                        new MightPowerSkill("TM_PistolSpec_eff", "TM_PistolSpec_eff_desc"), //
                        new MightPowerSkill("TM_PistolSpec_ver", "TM_PistolSpec_ver_desc")  //
                    };
                }
                return mightPowerSkill_PistolSpec;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_RifleSpec
        {
            get
            {
                bool flag = mightPowerSkill_RifleSpec == null;
                if (flag)
                {
                    mightPowerSkill_RifleSpec = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_RifleSpec_pwr", "TM_RifleSpec_pwr_desc"), //
                        new MightPowerSkill("TM_RifleSpec_eff", "TM_RifleSpec_eff_desc"), //
                        new MightPowerSkill("TM_RifleSpec_ver", "TM_RifleSpec_ver_desc")  //
                    };
                }
                return mightPowerSkill_RifleSpec;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ShotgunSpec
        {
            get
            {
                bool flag = mightPowerSkill_ShotgunSpec == null;
                if (flag)
                {
                    mightPowerSkill_ShotgunSpec = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ShotgunSpec_pwr", "TM_ShotgunSpec_pwr_desc"), //
                        new MightPowerSkill("TM_ShotgunSpec_eff", "TM_ShotgunSpec_eff_desc"), //
                        new MightPowerSkill("TM_ShotgunSpec_ver", "TM_ShotgunSpec_ver_desc")  //
                    };
                }
                return mightPowerSkill_ShotgunSpec;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_CQC
        {
            get
            {
                bool flag = mightPowerSkill_CQC == null;
                if (flag)
                {
                    mightPowerSkill_CQC = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_CQC_pwr", "TM_CQC_pwr_desc"), //
                        new MightPowerSkill("TM_CQC_eff", "TM_CQC_eff_desc"), //
                        new MightPowerSkill("TM_CQC_ver", "TM_CQC_ver_desc")  //
                    };
                }
                return mightPowerSkill_CQC;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_FirstAid
        {
            get
            {
                bool flag = mightPowerSkill_FirstAid == null;
                if (flag)
                {
                    mightPowerSkill_FirstAid = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_FirstAid_pwr", "TM_FirstAid_pwr_desc"), //
                        new MightPowerSkill("TM_FirstAid_eff", "TM_FirstAid_eff_desc"), //
                        new MightPowerSkill("TM_FirstAid_ver", "TM_FirstAid_ver_desc")  //
                    };
                }
                return mightPowerSkill_FirstAid;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_60mmMortar
        {
            get
            {
                bool flag = mightPowerSkill_60mmMortar == null;
                if (flag)
                {
                    mightPowerSkill_60mmMortar = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_60mmMortar_pwr", "TM_60mmMortar_pwr_desc"), //
                        new MightPowerSkill("TM_60mmMortar_eff", "TM_60mmMortar_eff_desc"), //
                        new MightPowerSkill("TM_60mmMortar_ver", "TM_60mmMortar_ver_desc")  //
                    };
                }
                return mightPowerSkill_60mmMortar;
            }
        }


        public List<MightPower> mightPowerC;
        public List<MightPowerSkill> mightPowerSkill_ProvisionerAura;
        public List<MightPowerSkill> mightPowerSkill_TaskMasterAura;
        public List<MightPowerSkill> mightPowerSkill_CommanderAura;
        public List<MightPowerSkill> mightPowerSkill_StayAlert;
        public List<MightPowerSkill> mightPowerSkill_MoveOut;
        public List<MightPowerSkill> mightPowerSkill_HoldTheLine;

        public List<MightPower> MightPowersC
        {
            get
            {
                bool flag = mightPowerC == null;
                if (flag)
                {
                    mightPowerC = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ProvisionerAura
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_TaskMasterAura
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_CommanderAura
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_StayAlert,
                            TorannMagicDefOf.TM_StayAlert_I,
                            TorannMagicDefOf.TM_StayAlert_II,
                            TorannMagicDefOf.TM_StayAlert_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_MoveOut,
                            TorannMagicDefOf.TM_MoveOut_I,
                            TorannMagicDefOf.TM_MoveOut_II,
                            TorannMagicDefOf.TM_MoveOut_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_HoldTheLine,
                            TorannMagicDefOf.TM_HoldTheLine_I,
                            TorannMagicDefOf.TM_HoldTheLine_II,
                            TorannMagicDefOf.TM_HoldTheLine_III
                        }),
                    };
                }
                return mightPowerC;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ProvisionerAura
        {
            get
            {
                bool flag = mightPowerSkill_ProvisionerAura == null;
                if (flag)
                {
                    mightPowerSkill_ProvisionerAura = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ProvisionerAura_pwr", "TM_ProvisionerAura_pwr_desc"), //
                        new MightPowerSkill("TM_ProvisionerAura_eff", "TM_ProvisionerAura_eff_desc"), //
                        new MightPowerSkill("TM_ProvisionerAura_ver", "TM_ProvisionerAura_ver_desc")  //
                    };
                }
                return mightPowerSkill_ProvisionerAura;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_TaskMasterAura
        {
            get
            {
                bool flag = mightPowerSkill_TaskMasterAura == null;
                if (flag)
                {
                    mightPowerSkill_TaskMasterAura = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_TaskMasterAura_pwr", "TM_TaskMasterAura_pwr_desc"), //
                        new MightPowerSkill("TM_TaskMasterAura_eff", "TM_TaskMasterAura_eff_desc"), //
                        new MightPowerSkill("TM_TaskMasterAura_ver", "TM_TaskMasterAura_ver_desc")  //
                    };
                }
                return mightPowerSkill_TaskMasterAura;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_CommanderAura
        {
            get
            {
                bool flag = mightPowerSkill_CommanderAura == null;
                if (flag)
                {
                    mightPowerSkill_CommanderAura = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_CommanderAura_pwr", "TM_CommanderAura_pwr_desc"), //
                        new MightPowerSkill("TM_CommanderAura_eff", "TM_CommanderAura_eff_desc"), //
                        new MightPowerSkill("TM_CommanderAura_ver", "TM_CommanderAura_ver_desc")  //
                    };
                }
                return mightPowerSkill_CommanderAura;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_StayAlert
        {
            get
            {
                bool flag = mightPowerSkill_StayAlert == null;
                if (flag)
                {
                    mightPowerSkill_StayAlert = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_StayAlert_pwr", "TM_StayAlert_pwr_desc"), //
                        new MightPowerSkill("TM_StayAlert_eff", "TM_StayAlert_eff_desc") //
                    };
                }
                return mightPowerSkill_StayAlert;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_MoveOut
        {
            get
            {
                bool flag = mightPowerSkill_MoveOut == null;
                if (flag)
                {
                    mightPowerSkill_MoveOut = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_MoveOut_pwr", "TM_MoveOut_pwr_desc"), //
                        new MightPowerSkill("TM_MoveOut_eff", "TM_MoveOut_eff_desc") //
                    };
                }
                return mightPowerSkill_MoveOut;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_HoldTheLine
        {
            get
            {
                bool flag = mightPowerSkill_HoldTheLine == null;
                if (flag)
                {
                    mightPowerSkill_HoldTheLine = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_HoldTheLine_pwr", "TM_HoldTheLine_pwr_desc"), //
                        new MightPowerSkill("TM_HoldTheLine_eff", "TM_HoldTheLine_eff_desc")
                    };
                }
                return mightPowerSkill_HoldTheLine;
            }
        }

        public List<MightPower> mightPowerW;
        public List<MightPowerSkill> mightPowerSkill_WayfarerCraft;
        public List<MightPowerSkill> mightPowerSkill_FieldTraining;

        public List<MightPower> MightPowersW
        {
            get
            {
                bool flag = mightPowerW == null;
                if (flag)
                {
                    mightPowerW = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_WayfarerCraft
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_FieldTraining
                        }),
                    };
                }
                return mightPowerW;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_WayfarerCraft
        {
            get
            {
                bool flag = mightPowerSkill_WayfarerCraft == null;
                if (flag)
                {
                    mightPowerSkill_WayfarerCraft = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_WayfarerCraft_pwr", "TM_WayfarerCraft_pwr_desc"), //
                        new MightPowerSkill("TM_WayfarerCraft_eff", "TM_WayfarerCraft_eff_desc"), //
                        new MightPowerSkill("TM_WayfarerCraft_ver", "TM_WayfarerCraft_ver_desc")  //
                    };
                }
                return mightPowerSkill_WayfarerCraft;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_FieldTraining
        {
            get
            {
                bool flag = mightPowerSkill_FieldTraining == null;
                if (flag)
                {
                    mightPowerSkill_FieldTraining = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_FieldTraining_pwr", "TM_FieldTraining_pwr_desc"), //
                        new MightPowerSkill("TM_FieldTraining_eff", "TM_FieldTraining_eff_desc"), //
                        new MightPowerSkill("TM_FieldTraining_ver", "TM_FieldTraining_ver_desc")
                    };
                }
                return mightPowerSkill_FieldTraining;
            }
        }

        public List<MightPower> mightPowerM;
        public List<MightPowerSkill> mightPowerSkill_Chi;
        public List<MightPowerSkill> mightPowerSkill_MindOverBody;
        public List<MightPowerSkill> mightPowerSkill_Meditate;
        public List<MightPowerSkill> mightPowerSkill_TigerStrike;
        public List<MightPowerSkill> mightPowerSkill_DragonStrike;
        public List<MightPowerSkill> mightPowerSkill_ThunderStrike;

        public List<MightPower> MightPowersM
        {
            get
            {
                bool flag = mightPowerM == null;
                if (flag)
                {
                    mightPowerM = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Chi
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_MindOverBody
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Meditate
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_TigerStrike
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_DragonStrike
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ThunderStrike
                        }),
                    };
                }
                return mightPowerM;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Chi
        {
            get
            {
                bool flag = mightPowerSkill_Chi== null;
                if (flag)
                {
                    mightPowerSkill_Chi = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Chi_pwr", "TM_Chi_pwr_desc"), //
                        new MightPowerSkill("TM_Chi_eff", "TM_Chi_eff_desc"), //
                        new MightPowerSkill("TM_Chi_ver", "TM_Chi_ver_desc")  //
                    };
                }
                return mightPowerSkill_Chi;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_MindOverBody
        {
            get
            {
                bool flag = mightPowerSkill_MindOverBody == null;
                if (flag)
                {
                    mightPowerSkill_MindOverBody = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_MindOverBody_pwr", "TM_MindOverBody_pwr_desc"), //
                        new MightPowerSkill("TM_MindOverBody_ver", "TM_MindOverBody_ver_desc")
                    };
                }
                return mightPowerSkill_MindOverBody;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Meditate
        {
            get
            {
                bool flag = mightPowerSkill_Meditate == null;
                if (flag)
                {
                    mightPowerSkill_Meditate = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Meditate_pwr", "TM_Meditate_pwr_desc"),
                        new MightPowerSkill("TM_Meditate_eff", "TM_Meditate_eff_desc"),
                        new MightPowerSkill("TM_Meditate_ver", "TM_Meditate_ver_desc")
                    };
                }
                return mightPowerSkill_Meditate;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_TigerStrike
        {
            get
            {
                bool flag = mightPowerSkill_TigerStrike == null;
                if (flag)
                {
                    mightPowerSkill_TigerStrike = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_TigerStrike_pwr", "TM_TigerStrike_pwr_desc"),
                        new MightPowerSkill("TM_TigerStrike_eff", "TM_TigerStrike_eff_desc"),
                        new MightPowerSkill("TM_TigerStrike_ver", "TM_TigerStrike_ver_desc")
                    };
                }
                return mightPowerSkill_TigerStrike;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_DragonStrike
        {
            get
            {
                bool flag = mightPowerSkill_DragonStrike == null;
                if (flag)
                {
                    mightPowerSkill_DragonStrike = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_DragonStrike_pwr", "TM_DragonStrike_pwr_desc"), //
                        new MightPowerSkill("TM_DragonStrike_eff", "TM_DragonStrike_eff_desc"),
                        new MightPowerSkill("TM_DragonStrike_ver", "TM_DragonStrike_ver_desc") //
                    };
                }
                return mightPowerSkill_DragonStrike;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ThunderStrike
        {
            get
            {
                bool flag = mightPowerSkill_ThunderStrike == null;
                if (flag)
                {
                    mightPowerSkill_ThunderStrike = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ThunderStrike_pwr", "TM_ThunderStrike_pwr_desc"), //
                        new MightPowerSkill("TM_ThunderStrike_eff", "TM_ThunderStrike_eff_desc"),
                        new MightPowerSkill("TM_ThunderStrike_ver", "TM_ThunderStrike_ver_desc") //
                    };
                }
                return mightPowerSkill_ThunderStrike;
            }
        }

        public List<MightPower> mightPowerDK;
        public List<MightPowerSkill> mightPowerSkill_Shroud;
        public List<MightPowerSkill> mightPowerSkill_WaveOfFear;
        public List<MightPowerSkill> mightPowerSkill_Spite;
        public List<MightPowerSkill> mightPowerSkill_LifeSteal;
        public List<MightPowerSkill> mightPowerSkill_GraveBlade;

        public List<MightPower> MightPowersDK
        {
            get
            {
                bool flag = mightPowerDK == null;
                if (flag)
                {
                    mightPowerDK = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Shroud
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_WaveOfFear
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Spite,
                            TorannMagicDefOf.TM_Spite_I,
                            TorannMagicDefOf.TM_Spite_II,
                            TorannMagicDefOf.TM_Spite_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_LifeSteal
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_GraveBlade,
                            TorannMagicDefOf.TM_GraveBlade_I,
                            TorannMagicDefOf.TM_GraveBlade_II,
                            TorannMagicDefOf.TM_GraveBlade_III
                        }),
                    };
                }
                return mightPowerDK;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Shroud
        {
            get
            {
                bool flag = mightPowerSkill_Shroud == null;
                if (flag)
                {
                    mightPowerSkill_Shroud = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Shroud_pwr", "TM_Shroud_pwr_desc"), //psionic punch / manipulation
                        new MightPowerSkill("TM_Shroud_eff", "TM_Shroud_eff_desc"), //psionic dash / movement
                        new MightPowerSkill("TM_Shroud_ver", "TM_Shroud_ver_desc")  //psionic preassure
                    };
                }
                return mightPowerSkill_Shroud;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_WaveOfFear
        {
            get
            {
                bool flag = mightPowerSkill_WaveOfFear == null;
                if (flag)
                {
                    mightPowerSkill_WaveOfFear = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_WaveOfFear_pwr", "TM_WaveOfFear_pwr_desc"), //applies skill powers to ability
                        new MightPowerSkill("TM_WaveOfFear_eff", "TM_WaveOfFear_eff_desc"), //increases how long ability is available and reduces stamina cost to acquire
                        new MightPowerSkill("TM_WaveOfFear_ver", "TM_WaveOfFear_ver_desc")
                    };
                }
                return mightPowerSkill_WaveOfFear;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Spite
        {
            get
            {
                bool flag = mightPowerSkill_Spite == null;
                if (flag)
                {
                    mightPowerSkill_Spite = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Spite_pwr", "TM_Spite_pwr_desc"),
                        new MightPowerSkill("TM_Spite_eff", "TM_Spite_eff_desc"),
                        new MightPowerSkill("TM_Spite_ver", "TM_Spite_ver_desc")
                    };
                }
                return mightPowerSkill_Spite;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_LifeSteal
        {
            get
            {
                bool flag = mightPowerSkill_LifeSteal == null;
                if (flag)
                {
                    mightPowerSkill_LifeSteal = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_LifeSteal_pwr", "TM_LifeSteal_pwr_desc"),
                        new MightPowerSkill("TM_LifeSteal_eff", "TM_LifeSteal_eff_desc"),
                        new MightPowerSkill("TM_LifeSteal_ver", "TM_LifeSteal_ver_desc")
                    };
                }
                return mightPowerSkill_LifeSteal;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_GraveBlade
        {
            get
            {
                bool flag = mightPowerSkill_GraveBlade == null;
                if (flag)
                {
                    mightPowerSkill_GraveBlade = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_GraveBlade_pwr", "TM_GraveBlade_pwr_desc"), //duration of possession, 
                        new MightPowerSkill("TM_GraveBlade_eff", "TM_GraveBlade_eff_desc"),
                        new MightPowerSkill("TM_GraveBlade_ver", "TM_GraveBlade_ver_desc") //applies mental states or effects , fewer debuffs during possession
                    };
                }
                return mightPowerSkill_GraveBlade;
            }
        }

        public List<MightPower> mightPowerP;
        public List<MightPowerSkill> mightPowerSkill_PsionicAugmentation;
        public List<MightPowerSkill> mightPowerSkill_PsionicBlast;
        public List<MightPowerSkill> mightPowerSkill_PsionicDash;
        public List<MightPowerSkill> mightPowerSkill_PsionicBarrier;
        public List<MightPowerSkill> mightPowerSkill_PsionicStorm;

        public List<MightPower> MightPowersP
        {
            get
            {
                bool flag = mightPowerP == null;
                if (flag)
                {
                    mightPowerP = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PsionicAugmentation
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PsionicBarrier,
                            TorannMagicDefOf.TM_PsionicBarrier_Projected
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PsionicBlast,
                            TorannMagicDefOf.TM_PsionicBlast_I,
                            TorannMagicDefOf.TM_PsionicBlast_II,
                            TorannMagicDefOf.TM_PsionicBlast_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PsionicDash
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PsionicStorm
                        }),
                    };
                }
                return mightPowerP;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PsionicAugmentation
        {
            get
            {
                bool flag = mightPowerSkill_PsionicAugmentation == null;
                if (flag)
                {
                    mightPowerSkill_PsionicAugmentation = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PsionicAugmentation_pwr", "TM_PsionicAugmentation_pwr_desc"), //psionic punch / manipulation
                        new MightPowerSkill("TM_PsionicAugmentation_eff", "TM_PsionicAugmentation_eff_desc"), //psionic dash / movement
                        new MightPowerSkill("TM_PsionicAugmentation_ver", "TM_PsionicAugmentation_ver_desc")  //psionic preassure
                    };
                }
                return mightPowerSkill_PsionicAugmentation;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PsionicBarrier
        {
            get
            {
                bool flag = mightPowerSkill_PsionicBarrier == null;
                if (flag)
                {
                    mightPowerSkill_PsionicBarrier = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PsionicBarrier_pwr", "TM_PsionicBarrier_pwr_desc"), //applies skill powers to ability
                        new MightPowerSkill("TM_PsionicBarrier_eff", "TM_PsionicBarrier_eff_desc"), //increases how long ability is available and reduces stamina cost to acquire
                        new MightPowerSkill("TM_PsionicBarrier_ver", "TM_PsionicBarrier_ver_desc")
                    };
                }
                return mightPowerSkill_PsionicBarrier;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PsionicBlast
        {
            get
            {
                bool flag = mightPowerSkill_PsionicBlast == null;
                if (flag)
                {
                    mightPowerSkill_PsionicBlast = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PsionicBlast_pwr", "TM_PsionicBlast_pwr_desc"), 
                        new MightPowerSkill("TM_PsionicBlast_eff", "TM_PsionicBlast_eff_desc"),
                        new MightPowerSkill("TM_PsionicBlast_ver", "TM_PsionicBlast_ver_desc") 
                    };
                }
                return mightPowerSkill_PsionicBlast;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PsionicDash
        {
            get
            {
                bool flag = mightPowerSkill_PsionicDash == null;
                if (flag)
                {
                    mightPowerSkill_PsionicDash = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PsionicDash_pwr", "TM_PsionicDash_pwr_desc"),
                        new MightPowerSkill("TM_PsionicDash_eff", "TM_PsionicDash_eff_desc"),
                        new MightPowerSkill("TM_PsionicDash_ver", "TM_PsionicDash_ver_desc") 
                    };
                }
                return mightPowerSkill_PsionicDash;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PsionicStorm
        {
            get
            {
                bool flag = mightPowerSkill_PsionicStorm == null;
                if (flag)
                {
                    mightPowerSkill_PsionicStorm = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PsionicStorm_pwr", "TM_PsionicStorm_pwr_desc"), //duration of possession, 
                        new MightPowerSkill("TM_PsionicStorm_eff", "TM_PsionicStorm_eff_desc"),
                        new MightPowerSkill("TM_PsionicStorm_ver", "TM_PsionicStorm_ver_desc") //applies mental states or effects , fewer debuffs during possession
                    };
                }
                return mightPowerSkill_PsionicStorm;
            }
        }

        public List<MightPower> mightPowerF;
        public List<MightPowerSkill> mightPowerSkill_Disguise;
        public List<MightPowerSkill> mightPowerSkill_Mimic;
        public List<MightPowerSkill> mightPowerSkill_Reversal;
        public List<MightPowerSkill> mightPowerSkill_Transpose;
        public List<MightPowerSkill> mightPowerSkill_Possess;

        public List<MightPower> MightPowersF
        {
            get
            {
                bool flag = mightPowerF == null;
                if (flag)
                {
                    mightPowerF = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Disguise
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Mimic
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Reversal
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Transpose,
                            TorannMagicDefOf.TM_Transpose_I,
                            TorannMagicDefOf.TM_Transpose_II,
                            TorannMagicDefOf.TM_Transpose_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Possess
                        }),
                    };
                }
                return mightPowerF;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Disguise
        {
            get
            {
                bool flag = mightPowerSkill_Disguise == null;
                if (flag)
                {
                    mightPowerSkill_Disguise = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Disguise_pwr", "TM_Disguise_pwr_desc"), //duration of skill
                        new MightPowerSkill("TM_Disguise_eff", "TM_Disguise_eff_desc"), 
                        new MightPowerSkill("TM_Disguise_ver", "TM_Disguise_ver_desc") //chance to avoid detection at different ranges
                    };
                }
                return mightPowerSkill_Disguise;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Mimic
        {
            get
            {
                bool flag = mightPowerSkill_Mimic == null;
                if (flag)
                {
                    mightPowerSkill_Mimic = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Mimic_pwr", "TM_Mimic_pwr_desc"), //applies skill powers to ability
                        new MightPowerSkill("TM_Mimic_eff", "TM_Mimic_eff_desc"), //increases how long ability is available and reduces stamina cost to acquire
                        new MightPowerSkill("TM_Mimic_ver", "TM_Mimic_ver_desc")
                    };
                }
                return mightPowerSkill_Mimic;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Reversal
        {
            get
            {
                bool flag = mightPowerSkill_Reversal == null;
                if (flag)
                {
                    mightPowerSkill_Reversal = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Reversal_pwr", "TM_Reversal_pwr_desc"), //increases duration of skill
                        new MightPowerSkill("TM_Reversal_eff", "TM_Reversal_eff_desc"),
                        new MightPowerSkill("TM_Reversal_ver", "TM_Reversal_ver_desc") //regenerative reversal
                    };
                }
                return mightPowerSkill_Reversal;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Transpose
        {
            get
            {
                bool flag = mightPowerSkill_Transpose == null;
                if (flag)
                {
                    mightPowerSkill_Transpose = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Transpose_eff", "TM_Transpose_eff_desc"),
                        new MightPowerSkill("TM_Transpose_ver", "TM_Transpose_ver_desc") //usable on enemies, usable on friendly beyond los, usable on enemy blos
                    };
                }
                return mightPowerSkill_Transpose;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Possess
        {
            get
            {
                bool flag = mightPowerSkill_Possess == null;
                if (flag)
                {
                    mightPowerSkill_Possess = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Possess_pwr", "TM_Possess_pwr_desc"), //duration of possession, 
                        new MightPowerSkill("TM_Possess_eff", "TM_Possess_eff_desc"),
                        new MightPowerSkill("TM_Possess_ver", "TM_Possess_ver_desc") //applies mental states or effects , fewer debuffs during possession
                    };
                }
                return mightPowerSkill_Possess;
            }
        }

        public List<MightPower> mightPowerG;
        public List<MightPowerSkill> mightPowerSkill_Sprint;
        public List<MightPowerSkill> mightPowerSkill_Fortitude;
        public List<MightPowerSkill> mightPowerSkill_Grapple;
        public List<MightPowerSkill> mightPowerSkill_Cleave;
        public List<MightPowerSkill> mightPowerSkill_Whirlwind;

        public List<MightPower> MightPowersG
        {
            get
            {
                bool flag = mightPowerG == null;
                if (flag)
                {
                    mightPowerG = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Sprint,
                            TorannMagicDefOf.TM_Sprint_I,
                            TorannMagicDefOf.TM_Sprint_II,
                            TorannMagicDefOf.TM_Sprint_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Fortitude
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Grapple,
                            TorannMagicDefOf.TM_Grapple_I,
                            TorannMagicDefOf.TM_Grapple_II,
                            TorannMagicDefOf.TM_Grapple_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Cleave
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Whirlwind
                        }),
                    };
                }
                return mightPowerG;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Sprint
        {
            get
            {
                bool flag = mightPowerSkill_Sprint == null;
                if (flag)
                {
                    mightPowerSkill_Sprint = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Sprint_pwr", "TM_Sprint_pwr_desc"),
                        new MightPowerSkill("TM_Sprint_eff", "TM_Sprint_eff_desc")
                    };
                }
                return mightPowerSkill_Sprint;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Fortitude
        {
            get
            {
                bool flag = mightPowerSkill_Fortitude == null;
                if (flag)
                {
                    mightPowerSkill_Fortitude = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Fortitude_pwr", "TM_Fortitude_pwr_desc"),
                        new MightPowerSkill("TM_Fortitude_ver", "TM_Fortitude_ver_desc")
                    };
                }
                return mightPowerSkill_Fortitude;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Grapple
        {
            get
            {
                bool flag = mightPowerSkill_Grapple == null;
                if (flag)
                {
                    mightPowerSkill_Grapple = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Grapple_eff", "TM_Grapple_eff_desc")
                    };
                }
                return mightPowerSkill_Grapple;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Cleave
        {
            get
            {
                bool flag = mightPowerSkill_Cleave == null;
                if (flag)
                {
                    mightPowerSkill_Cleave = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Cleave_pwr", "TM_Cleave_pwr_desc"),
                        new MightPowerSkill("TM_Cleave_eff", "TM_Cleave_eff_desc"),
                        new MightPowerSkill("TM_Cleave_ver", "TM_Cleave_ver_desc")
                    };
                }
                return mightPowerSkill_Cleave;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Whirlwind
        {
            get
            {
                bool flag = mightPowerSkill_Whirlwind == null;
                if (flag)
                {
                    mightPowerSkill_Whirlwind = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Whirlwind_pwr", "TM_Whirlwind_pwr_desc"),
                        new MightPowerSkill("TM_Whirlwind_eff", "TM_Whirlwind_eff_desc"),
                        new MightPowerSkill("TM_Whirlwind_ver", "TM_Whirlwind_ver_desc")
                    };
                }
                return mightPowerSkill_Whirlwind;
            }
        }

        public List<MightPower> mightPowerS;
        public List<MightPowerSkill> mightPowerSkill_SniperFocus;
        public List<MightPowerSkill> mightPowerSkill_Headshot;
        public List<MightPowerSkill> mightPowerSkill_DisablingShot;
        public List<MightPowerSkill> mightPowerSkill_AntiArmor;
        public List<MightPowerSkill> mightPowerSkill_ShadowSlayer;

        public List<MightPower> MightPowersS
        {
            get
            {
                bool sniperHasSS = false;
                bool flag = mightPowerS == null;
                if (flag)
                {
                    mightPowerS = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_SniperFocus
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_Headshot
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_DisablingShot,
                            TorannMagicDefOf.TM_DisablingShot_I,
                            TorannMagicDefOf.TM_DisablingShot_II,
                            TorannMagicDefOf.TM_DisablingShot_III
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_AntiArmor
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ShadowSlayer
                        }),
                    };
                }
                if (!sniperHasSS)
                {
                    foreach (MightPower p in mightPowerS)
                    {
                        if (p.abilityDef == TorannMagicDefOf.TM_ShadowSlayer)
                        {
                            sniperHasSS = true;
                        }
                    }
                    if (!sniperHasSS)
                    {
                        MightPower pSS = new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ShadowSlayer
                        });
                        mightPowerS.Add(pSS);
                        sniperHasSS = true;
                    }
                }
                return mightPowerS;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_SniperFocus
        {
            get
            {
                bool flag = mightPowerSkill_SniperFocus == null;
                if (flag)
                {
                    mightPowerSkill_SniperFocus = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_SniperFocus_pwr", "TM_SniperFocus_pwr_desc")
                    };
                }
                return mightPowerSkill_SniperFocus;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_Headshot
        {
            get
            {
                bool flag = mightPowerSkill_Headshot == null;
                if (flag)
                {
                    mightPowerSkill_Headshot = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_Headshot_pwr", "TM_Headshot_pwr_desc"),
                        new MightPowerSkill("TM_Headshot_eff", "TM_Headshot_eff_desc"),
                        new MightPowerSkill("TM_Headshot_ver", "TM_Headshot_ver_desc")
                    };
                }
                return mightPowerSkill_Headshot;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_DisablingShot
        {
            get
            {
                bool flag = mightPowerSkill_DisablingShot == null;
                if (flag)
                {
                    mightPowerSkill_DisablingShot = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_DisablingShot_eff", "TM_DisablingShot_eff_desc"),
                        new MightPowerSkill("TM_DisablingShot_ver", "TM_DisablingShot_ver_desc")
                    };
                }
                return mightPowerSkill_DisablingShot;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_AntiArmor
        {
            get
            {
                bool flag = mightPowerSkill_AntiArmor == null;
                if (flag)
                {
                    mightPowerSkill_AntiArmor = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_AntiArmor_pwr", "TM_AntiArmor_pwr_desc"),
                        new MightPowerSkill("TM_AntiArmor_eff", "TM_AntiArmor_eff_desc"),
                        new MightPowerSkill("TM_AntiArmor_ver", "TM_AntiArmor_ver_desc")
                    };
                }
                return mightPowerSkill_AntiArmor;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ShadowSlayer
        {
            get
            {
                bool flag = mightPowerSkill_ShadowSlayer == null;
                if (flag)
                {
                    mightPowerSkill_ShadowSlayer = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ShadowSlayer_pwr", "TM_ShadowSlayer_pwr_desc"),
                        new MightPowerSkill("TM_ShadowSlayer_eff", "TM_ShadowSlayer_eff_desc"),
                        new MightPowerSkill("TM_ShadowSlayer_ver", "TM_ShadowSlayer_ver_desc")
                    };
                }
                return mightPowerSkill_ShadowSlayer;
            }
        }

        public List<MightPower> mightPowerB;
        public List<MightPowerSkill> mightPowerSkill_BladeFocus;
        public List<MightPowerSkill> mightPowerSkill_BladeArt;
        public List<MightPowerSkill> mightPowerSkill_SeismicSlash;
        public List<MightPowerSkill> mightPowerSkill_BladeSpin;
        public List<MightPowerSkill> mightPowerSkill_PhaseStrike;

        public List<MightPower> MightPowersB
        {
            get
            {
                bool flag = mightPowerB == null;
                if (flag)
                {
                    mightPowerB = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BladeFocus
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BladeArt
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_SeismicSlash
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BladeSpin
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PhaseStrike,
                            TorannMagicDefOf.TM_PhaseStrike_I,
                            TorannMagicDefOf.TM_PhaseStrike_II,
                            TorannMagicDefOf.TM_PhaseStrike_III
                        }),
                    };
                }
                return mightPowerB;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_BladeFocus
        {
            get
            {
                bool flag = mightPowerSkill_BladeFocus == null;
                if (flag)
                {
                    mightPowerSkill_BladeFocus = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_BladeFocus_pwr", "TM_BladeFocus_pwr_desc")
                    };
                }
                return mightPowerSkill_BladeFocus;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_BladeArt
        {
            get
            {
                bool flag = mightPowerSkill_BladeArt == null;
                if (flag)
                {
                    mightPowerSkill_BladeArt = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_BladeArt_pwr", "TM_BladeArt_pwr_desc")
                    };
                }
                return mightPowerSkill_BladeArt;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_SeismicSlash
        {
            get
            {
                bool flag = mightPowerSkill_SeismicSlash == null;
                if (flag)
                {
                    mightPowerSkill_SeismicSlash = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_SeismicSlash_pwr", "TM_SeismicSlash_pwr_desc"),
                        new MightPowerSkill("TM_SeismicSlash_eff", "TM_SeismicSlash_eff_desc"),
                        new MightPowerSkill("TM_SeismicSlash_ver", "TM_SeismicSlash_ver_desc")
                    };
                }
                return mightPowerSkill_SeismicSlash;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_BladeSpin
        {
            get
            {
                bool flag = mightPowerSkill_BladeSpin == null;
                if (flag)
                {
                    mightPowerSkill_BladeSpin = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_BladeSpin_pwr", "TM_BladeSpin_pwr_desc"),
                        new MightPowerSkill("TM_BladeSpin_eff", "TM_BladeSpin_eff_desc"),
                        new MightPowerSkill("TM_BladeSpin_ver", "TM_BladeSpin_ver_desc")
                    };
                }
                return mightPowerSkill_BladeSpin;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PhaseStrike
        {
            get
            {
                bool flag = mightPowerSkill_PhaseStrike == null;
                if (flag)
                {
                    mightPowerSkill_PhaseStrike = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PhaseStrike_pwr", "TM_PhaseStrike_pwr_desc"),
                        new MightPowerSkill("TM_PhaseStrike_eff", "TM_PhaseStrike_eff_desc"),
                        new MightPowerSkill("TM_PhaseStrike_ver", "TM_PhaseStrike_ver_desc")
                    };
                }
                return mightPowerSkill_PhaseStrike;
            }
        }

        public List<MightPower> mightPowerR;
        public List<MightPowerSkill> mightPowerSkill_RangerTraining;
        public List<MightPowerSkill> mightPowerSkill_BowTraining;
        public List<MightPowerSkill> mightPowerSkill_PoisonTrap;
        public List<MightPowerSkill> mightPowerSkill_AnimalFriend;
        public List<MightPowerSkill> mightPowerSkill_ArrowStorm;

        public List<MightPower> MightPowersR
        {
            get
            {
                bool flag = mightPowerR == null;
                if (flag)
                {
                    mightPowerR = new List<MightPower>
                    {
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_RangerTraining
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_BowTraining
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_PoisonTrap
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_AnimalFriend
                        }),
                        new MightPower(new List<AbilityUser.AbilityDef>
                        {
                            TorannMagicDefOf.TM_ArrowStorm,
                            TorannMagicDefOf.TM_ArrowStorm_I,
                            TorannMagicDefOf.TM_ArrowStorm_II,
                            TorannMagicDefOf.TM_ArrowStorm_III
                        }),
                    };
                }
                return mightPowerR;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_RangerTraining
        {
            get
            {
                bool flag = mightPowerSkill_RangerTraining == null;
                if (flag)
                {
                    mightPowerSkill_RangerTraining = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_RangerTraining_pwr", "TM_RangerTraining_pwr_desc")
                    };
                }
                return mightPowerSkill_RangerTraining;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_BowTraining
        {
            get
            {
                bool flag = mightPowerSkill_BowTraining == null;
                if (flag)
                {
                    mightPowerSkill_BowTraining = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_BowTraining_pwr", "TM_BowTraining_pwr_desc")
                    };
                }
                return mightPowerSkill_BowTraining;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_PoisonTrap
        {
            get
            {
                bool flag = mightPowerSkill_PoisonTrap == null;
                if (flag)
                {
                    mightPowerSkill_PoisonTrap = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_PoisonTrap_ver", "TM_PoisonTrap_ver_desc")
                    };
                }
                return mightPowerSkill_PoisonTrap;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_AnimalFriend
        {
            get
            {
                bool flag = mightPowerSkill_AnimalFriend == null;
                if (flag)
                {
                    mightPowerSkill_AnimalFriend = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_AnimalFriend_pwr", "TM_AnimalFriend_pwr_desc"),
                        new MightPowerSkill("TM_AnimalFriend_eff", "TM_AnimalFriend_eff_desc"),
                        new MightPowerSkill("TM_AnimalFriend_ver", "TM_AnimalFriend_ver_desc")
                    };
                }
                return mightPowerSkill_AnimalFriend;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_ArrowStorm
        {
            get
            {
                bool flag = mightPowerSkill_ArrowStorm == null;
                if (flag)
                {
                    mightPowerSkill_ArrowStorm = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_ArrowStorm_pwr", "TM_ArrowStorm_pwr_desc"),
                        new MightPowerSkill("TM_ArrowStorm_eff", "TM_ArrowStorm_eff_desc"),
                        new MightPowerSkill("TM_ArrowStorm_ver", "TM_ArrowStorm_ver_desc")
                    };
                }
                return mightPowerSkill_ArrowStorm;
            }
        }

        public List<MightPowerSkill> mightPowerSkill_global_refresh;
        public List<MightPowerSkill> mightPowerSkill_global_seff;
        public List<MightPowerSkill> mightPowerSkill_global_strength;
        public List<MightPowerSkill> mightPowerSkill_global_endurance;

        public List<MightPowerSkill> MightPowerSkill_global_refresh
        {
            get
            {
                bool flag = mightPowerSkill_global_refresh == null;
                if (flag)
                {
                    mightPowerSkill_global_refresh = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_global_refresh_pwr", "TM_global_refresh_pwr_desc")
                    };
                }
                return mightPowerSkill_global_refresh;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_global_seff
        {
            get
            {
                bool flag = mightPowerSkill_global_seff == null;
                if (flag)
                {
                    mightPowerSkill_global_seff = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_global_seff_pwr", "TM_global_seff_pwr_desc")
                    };
                }
                return mightPowerSkill_global_seff;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_global_strength
        {
            get
            {
                bool flag = mightPowerSkill_global_strength == null;
                if (flag)
                {
                    mightPowerSkill_global_strength = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_global_strength_pwr", "TM_global_strength_pwr_desc")
                    };
                }
                return mightPowerSkill_global_strength;
            }
        }
        public List<MightPowerSkill> MightPowerSkill_global_endurance
        {
            get
            {
                bool flag = mightPowerSkill_global_endurance == null;
                if (flag)
                {
                    mightPowerSkill_global_endurance = new List<MightPowerSkill>
                    {
                        new MightPowerSkill("TM_global_endurance_pwr", "TM_global_endurance_pwr_desc")
                    };
                }
                return mightPowerSkill_global_endurance;
            }
        }

        public bool Initialized
        {
            get
            {
                return initialized;
            }
            set
            {
                initialized = value;
            }
        }

        public int MightUserLevel
        {
            get
            {
                return mightUserLevel;
            }
            set
            {
                mightUserLevel = value;
            }
        }

        public int MightUserXP
        {
            get
            {
                return mightUserXP;
            }
            set
            {
                mightUserXP = value;
            }
        }

        public int MightAbilityPoints
        {
            get
            {
                return mightAbilityPoints;
            }
            set
            {
                mightAbilityPoints = value;
            }
        }

        public int TicksToLearnMightXP
        {
            get
            {
                return ticksToLearnMightXP;
            }
            set
            {
                ticksToLearnMightXP = value;
            }
        }

        public int TicksAffiliation
        {
            get
            {
                return ticksAffiliation;
            }
            set
            {
                ticksAffiliation = value;
            }
        }

        public Pawn MightPawn
        {
            get
            {
                return mightPawn;
            }
        }

        public Faction Affiliation
        {
            get
            {
                return affiliation;
            }
            set
            {
                affiliation = value;
            }
        }

        private List<MightPower> allMightPowersList = new List<MightPower>();
        public List<MightPower> AllMightPowers
        {
            get
            {
                if (allMightPowersList == null || allMightPowersList.Count <= 0)
                {
                    allMightPowersList = new List<MightPower>();
                    allMightPowersList.Clear();
                    allMightPowersList.AddRange(AllMightPowersWithSkills);
                    allMightPowersList.AddRange(MightPowersStandalone);
                    allMightPowersList.AddRange(MightPowersCustomStandalone);
                }
                return allMightPowersList;
            }
        }

        public List<MightPower> allMightPowersWithSkills = new List<MightPower>();
        public List<MightPower> AllMightPowersWithSkills
        {
            get
            {
                if (allMightPowersWithSkills == null || allMightPowersWithSkills.Count <= 0)
                {
                    allMightPowersWithSkills = new List<MightPower>();
                    allMightPowersWithSkills.Clear();
                    allMightPowersWithSkills.AddRange(MightPowersCustom);
                    allMightPowersWithSkills.AddRange(MightPowersW);
                    allMightPowersWithSkills.AddRange(MightPowersM);
                    allMightPowersWithSkills.AddRange(MightPowersDK);
                    allMightPowersWithSkills.AddRange(MightPowersG);
                    allMightPowersWithSkills.AddRange(MightPowersS);
                    allMightPowersWithSkills.AddRange(MightPowersB);
                    allMightPowersWithSkills.AddRange(MightPowersR);
                    allMightPowersWithSkills.AddRange(MightPowersF);
                    allMightPowersWithSkills.AddRange(MightPowersP);
                    allMightPowersWithSkills.AddRange(MightPowersC);
                    allMightPowersWithSkills.AddRange(MightPowersSS);
                    allMightPowersWithSkills.AddRange(MightPowersShadow);
                    allMightPowersWithSkills.AddRange(MightPowersApothecary);
                }
                return allMightPowersWithSkills;
            }
        }

        public List<MightPowerSkill> allMightPowerSkills = new List<MightPowerSkill>();
        public List<MightPowerSkill> AllMightPowerSkills
        {
            get
            {
                if (allMightPowerSkills == null || allMightPowerSkills.Count <= 0)
                {
                    allMightPowerSkills = new List<MightPowerSkill>();
                    allMightPowerSkills.Clear();
                    allMightPowerSkills.AddRange(MightPowerSkill_60mmMortar);
                    allMightPowerSkills.AddRange(MightPowerSkill_AnimalFriend);
                    allMightPowerSkills.AddRange(MightPowerSkill_AntiArmor);
                    allMightPowerSkills.AddRange(MightPowerSkill_ArrowStorm);
                    allMightPowerSkills.AddRange(MightPowerSkill_BladeArt);
                    allMightPowerSkills.AddRange(MightPowerSkill_BladeFocus);
                    allMightPowerSkills.AddRange(MightPowerSkill_BladeSpin);
                    allMightPowerSkills.AddRange(MightPowerSkill_BowTraining);
                    allMightPowerSkills.AddRange(MightPowerSkill_Chi);
                    allMightPowerSkills.AddRange(MightPowerSkill_Cleave);
                    allMightPowerSkills.AddRange(MightPowerSkill_CommanderAura);
                    allMightPowerSkills.AddRange(MightPowerSkill_CQC);
                    allMightPowerSkills.AddRange(MightPowerSkill_DisablingShot);
                    allMightPowerSkills.AddRange(MightPowerSkill_Disguise);
                    allMightPowerSkills.AddRange(MightPowerSkill_DragonStrike);
                    allMightPowerSkills.AddRange(MightPowerSkill_FieldTraining);
                    allMightPowerSkills.AddRange(MightPowerSkill_FirstAid);
                    allMightPowerSkills.AddRange(MightPowerSkill_Fortitude);
                    allMightPowerSkills.AddRange(MightPowerSkill_global_endurance);
                    allMightPowerSkills.AddRange(MightPowerSkill_global_refresh);
                    allMightPowerSkills.AddRange(MightPowerSkill_global_seff);
                    allMightPowerSkills.AddRange(MightPowerSkill_global_strength);
                    allMightPowerSkills.AddRange(MightPowerSkill_Grapple);
                    allMightPowerSkills.AddRange(MightPowerSkill_GraveBlade);
                    allMightPowerSkills.AddRange(MightPowerSkill_Headshot);
                    allMightPowerSkills.AddRange(MightPowerSkill_HoldTheLine);
                    allMightPowerSkills.AddRange(MightPowerSkill_LifeSteal);
                    allMightPowerSkills.AddRange(MightPowerSkill_Meditate);
                    allMightPowerSkills.AddRange(MightPowerSkill_Mimic);
                    allMightPowerSkills.AddRange(MightPowerSkill_MindOverBody);
                    allMightPowerSkills.AddRange(MightPowerSkill_MoveOut);
                    allMightPowerSkills.AddRange(MightPowerSkill_PhaseStrike);
                    allMightPowerSkills.AddRange(MightPowerSkill_PistolSpec);
                    allMightPowerSkills.AddRange(MightPowerSkill_PoisonTrap);
                    allMightPowerSkills.AddRange(MightPowerSkill_Possess);
                    allMightPowerSkills.AddRange(MightPowerSkill_ProvisionerAura);
                    allMightPowerSkills.AddRange(MightPowerSkill_PsionicAugmentation);
                    allMightPowerSkills.AddRange(MightPowerSkill_PsionicBarrier);
                    allMightPowerSkills.AddRange(MightPowerSkill_PsionicBlast);
                    allMightPowerSkills.AddRange(MightPowerSkill_PsionicDash);
                    allMightPowerSkills.AddRange(MightPowerSkill_PsionicStorm);
                    allMightPowerSkills.AddRange(MightPowerSkill_RangerTraining);
                    allMightPowerSkills.AddRange(MightPowerSkill_Reversal);
                    allMightPowerSkills.AddRange(MightPowerSkill_RifleSpec);
                    allMightPowerSkills.AddRange(MightPowerSkill_SeismicSlash);
                    allMightPowerSkills.AddRange(MightPowerSkill_ShadowSlayer);                   
                    allMightPowerSkills.AddRange(MightPowerSkill_ShotgunSpec);
                    allMightPowerSkills.AddRange(MightPowerSkill_Shroud);
                    allMightPowerSkills.AddRange(MightPowerSkill_SniperFocus);
                    allMightPowerSkills.AddRange(MightPowerSkill_Spite);
                    allMightPowerSkills.AddRange(MightPowerSkill_Sprint);
                    allMightPowerSkills.AddRange(MightPowerSkill_StayAlert);
                    allMightPowerSkills.AddRange(MightPowerSkill_TaskMasterAura);
                    allMightPowerSkills.AddRange(MightPowerSkill_ThunderStrike);
                    allMightPowerSkills.AddRange(MightPowerSkill_TigerStrike);
                    allMightPowerSkills.AddRange(MightPowerSkill_Transpose);
                    allMightPowerSkills.AddRange(MightPowerSkill_WaveOfFear);
                    allMightPowerSkills.AddRange(MightPowerSkill_WayfarerCraft);
                    allMightPowerSkills.AddRange(MightPowerSkill_Whirlwind);
                    allMightPowerSkills.AddRange(MightPowerSkill_ShadowStrike);
                    allMightPowerSkills.AddRange(MightPowerSkill_Nightshade);
                    allMightPowerSkills.AddRange(MightPowerSkill_VeilOfShadows);
                    allMightPowerSkills.AddRange(MightPowerSkill_Herbalist);
                    allMightPowerSkills.AddRange(MightPowerSkill_PoisonFlask);
                    allMightPowerSkills.AddRange(MightPowerSkill_Elixir);
                    allMightPowerSkills.AddRange(MightPowerSkill_SoothingBalm);
                    allMightPowerSkills.AddRange(MightPowerSkill_Custom);
                }
                return allMightPowerSkills;
            }
        }

        public void ClearSkill_Dictionaries()
        {
            skillPower.Clear();
            skillVersatility.Clear();
            skillEfficiency.Clear();
        }

        private Dictionary<TMAbilityDef, MightPowerSkill> skillEfficiency = new Dictionary<TMAbilityDef, MightPowerSkill>();
        public MightPowerSkill GetSkill_Efficiency(TMAbilityDef ability)
        {            
            if (!skillEfficiency.ContainsKey(ability))
            {
                bool hasSkill = false;
                string s = ability.defName.ToString();
                if (s == "TM_PsionicBarrier_Projected") s = "TM_PsionicBarrier";
                char[] trim = { '_', 'I', 'V', 'X' };
                s = s.TrimEnd(trim) + "_eff";
                for (int i = 0; i < AllMightPowerSkills.Count; i++)
                {
                    MightPowerSkill mps = AllMightPowerSkills[i];                    
                    if (mps.label.Contains(s))
                    {
                        skillEfficiency.Add(ability, mps);
                        hasSkill = true;
                    }
                }
                if (!hasSkill) //check custom powers for different ability pairing for skill names
                {
                    List<TM_CustomPowerDef> customPowers = TM_Data.CustomFighterPowerDefs().ToList();
                    for (int i = 0; i < customPowers.Count; i++)
                    {
                        for (int j = 0; j < customPowers[i].customPower.abilityDefs.Count; j++)
                        {
                            if (ability.defName == customPowers[i].customPower.abilityDefs[j].ToString())
                            {
                                for (int k = 0; k < AllMightPowerSkills.Count; k++)
                                {
                                    MightPowerSkill mps = AllMightPowerSkills[k];
                                    foreach (TM_CustomSkill cs in customPowers[i].customPower.skills)
                                    {
                                        if (cs.label.EndsWith("_eff") && cs.label == mps.label)
                                        {
                                            skillEfficiency.Add(ability, mps);
                                            hasSkill = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!hasSkill)
                {
                    skillEfficiency.Add(ability, null);
                }
            }
            return skillEfficiency[ability];
        }

        private Dictionary<TMAbilityDef, MightPowerSkill> skillVersatility = new Dictionary<TMAbilityDef, MightPowerSkill>();
        public MightPowerSkill GetSkill_Versatility(TMAbilityDef ability)
        {
            if (!skillVersatility.ContainsKey(ability))
            {
                bool hasSkill = false;
                string s = ability.defName.ToString();
                if (s == "TM_PsionicBarrier_Projected") s = "TM_PsionicBarrier";
                char[] trim = { '_', 'I', 'V', 'X' };
                s = s.TrimEnd(trim) + "_ver";
                for (int i = 0; i < AllMightPowerSkills.Count; i++)
                {                    
                    MightPowerSkill mps = AllMightPowerSkills[i];
                    
                    if (mps.label.Contains(s))
                    {
                        skillVersatility.Add(ability, mps);
                        hasSkill = true;
                    }
                }
                if (!hasSkill) //check custom powers for different ability to skill names
                {
                    List<TM_CustomPowerDef> customPowers = TM_Data.CustomFighterPowerDefs().ToList();
                    for (int i = 0; i < customPowers.Count; i++)
                    {
                        for (int j = 0; j < customPowers[i].customPower.abilityDefs.Count; j++)
                        {
                            if (ability.defName == customPowers[i].customPower.abilityDefs[j].ToString())
                            {
                                for (int k = 0; k < AllMightPowerSkills.Count; k++)
                                {
                                    MightPowerSkill mps = AllMightPowerSkills[k];
                                    foreach (TM_CustomSkill cs in customPowers[i].customPower.skills)
                                    {
                                        if (cs.label.EndsWith("_ver") && cs.label == mps.label)
                                        {
                                            skillVersatility.Add(ability, mps);
                                            hasSkill = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!hasSkill)
                {
                    skillVersatility.Add(ability, null);
                }
            }
            return skillVersatility[ability];
        }

        private Dictionary<TMAbilityDef, MightPowerSkill> skillPower = new Dictionary<TMAbilityDef, MightPowerSkill>();
        public MightPowerSkill GetSkill_Power(TMAbilityDef ability)
        {
            if (!skillPower.ContainsKey(ability))
            {
                bool hasSkill = false;
                string s = ability.defName.ToString();
                if (s == "TM_PsionicBarrier_Projected") s = "TM_PsionicBarrier";
                char[] trim = { '_', 'I', 'V', 'X', };
                s = s.TrimEnd(trim) + "_pwr";
                for (int i = 0; i < AllMightPowerSkills.Count; i++)
                {                    
                    MightPowerSkill mps = AllMightPowerSkills[i];
                    
                    if (mps.label.Contains(s))
                    {
                        skillPower.Add(ability, mps);
                        hasSkill = true;
                    }
                }
                if(!hasSkill) //check custom powers for different ability to skill names
                {
                    List<TM_CustomPowerDef> customPowers = TM_Data.CustomFighterPowerDefs().ToList();
                    for (int i = 0; i < customPowers.Count; i++)
                    {
                        for (int j = 0; j < customPowers[i].customPower.abilityDefs.Count; j++)
                        {
                            if(ability.defName == customPowers[i].customPower.abilityDefs[j].ToString())
                            {
                                for (int k = 0; k < AllMightPowerSkills.Count; k++)
                                {
                                    MightPowerSkill mps = AllMightPowerSkills[k];
                                    foreach(TM_CustomSkill cs in customPowers[i].customPower.skills)
                                    {
                                        if(cs.label.EndsWith("_pwr") && cs.label == mps.label)
                                        {
                                            skillPower.Add(ability, mps);
                                            hasSkill = true;
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }
                if (!hasSkill)
                {
                    skillPower.Add(ability, null);
                }
            }
            return skillPower[ability];
        }

        private Dictionary<HediffDef, TMAbilityDef> hediffAbility = new Dictionary<HediffDef, TMAbilityDef>();
        public TMAbilityDef GetHediffAbility(Hediff hd)
        {
            if (!hediffAbility.ContainsKey(hd.def))
            {
                bool hasAbility = false;
                for (int i = 0; i < AllMightPowers.Count; i++)
                {
                    if (AllMightPowers[i].abilityDef is TMAbilityDef)
                    {
                        TMAbilityDef ability = (TMAbilityDef)AllMightPowers[i].abilityDef;
                        if (ability.abilityHediff != null && ability.abilityHediff == hd.def)
                        {
                            hediffAbility.Add(hd.def, ability);
                            hasAbility = true;
                            break;
                        }
                    }
                }
                if(!hasAbility)
                {
                    hediffAbility.Add(hd.def, null);
                }
            }
            return hediffAbility[hd.def];
        }

        private int uniquePowersCount;
        public int GetUniquePowersWithSkillsCount(List<TM_CustomClass> customClassList)
        {
            if(uniquePowersCount != 0)
            {
                return uniquePowersCount;
            }
            List<TMAbilityDef> abilities = new List<TMAbilityDef>();
            abilities.Clear();
            foreach (TM_CustomClass customClass in customClassList)
            {
                for (int i = 0; i < customClass.classFighterAbilities.Count; i++)
                {
                    bool unique = true;
                    for (int j = 0; j < abilities.Count; j++)
                    {
                        if (customClass.classFighterAbilities[i].defName.Contains(abilities[j].defName))
                        {
                            unique = false;
                        }
                    }
                    if (unique)
                    {
                        abilities.Add(customClass.classFighterAbilities[i]);
                    }
                }
            }
            uniquePowersCount = abilities.Count;
            return uniquePowersCount;
        }

        public MightPower ReturnMatchingMightPower(TMAbilityDef def)
        {
            for(int i = 0; i < AllMightPowers.Count; i++)
            {
                if(AllMightPowers[i].TMabilityDefs.Contains(def))
                {
                    return AllMightPowers[i];
                }
            }
            return null;
        }

        public IEnumerable<MightPower> Powers
        {
            get
            {                
                return MightPowersSS.Concat(MightPowersC.Concat(MightPowersW.Concat(MightPowersM.Concat(MightPowersDK.Concat(MightPowersG.Concat(MightPowersS.Concat(MightPowersB.Concat(mightPowerR.Concat(MightPowersF.Concat(mightPowerP.Concat(mightPowerStandalone)))))))))));
            }
        }

        public MightData()
        {
        }

        public MightData(CompAbilityUserMight newUser)
        {
            mightPawn = newUser.AbilityUser;
        }

        public void ClearData()
        {
            mightUserLevel = 0;
            mightUserXP = 0;
            mightPowerW.Clear();
            mightPowerM.Clear();
            mightPowerB.Clear();
            mightPowerDK.Clear();
            mightPowerF.Clear();
            mightPowerG.Clear();
            mightPowerP.Clear();
            mightPowerR.Clear();
            mightPowerS.Clear();
            mightPowerC.Clear();
            mightPowerSS.Clear();
            mightPowerShadow.Clear();
            mightPowerApothecary.Clear();
            mightPowerStandalone.Clear();
            mightPowerCustom.Clear();
            mightPawn = null;
            initialized = false;
        }

        public void ExposeData()
        {
            //Scribe_Deep.Look(ref this.equipmentContainer, "equipmentContainer", new object[] { this });
            Scribe_References.Look<Pawn>(ref mightPawn, "mightPawn", false);
            Scribe_Values.Look<int>(ref mightUserLevel, "mightUserLevel", 0, false);
            Scribe_Values.Look<int>(ref mightUserXP, "mightUserXP", 0, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref mightAbilityPoints, "mightAbilityPoints", 0, false);
            Scribe_Values.Look<int>(ref ticksToLearnMightXP, "ticksToLearnMightXP", -1, false);
            Scribe_Values.Look<int>(ref ticksAffiliation, "ticksAffiliation", -1, false);
            Scribe_Collections.Look<MightPower>(ref mightPowerStandalone, "mightPowerStandalone", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_global_refresh, "mightPowerSkill_global_refresh", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_global_seff, "mightPowerSkill_global_seff", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_global_strength, "mightPowerSkill_global_strength", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_global_endurance, "mightPowerSkill_global_endurance", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerCustom, "mightPowerCustom", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerCustomStandalone, "mightPowerCustomStandalone", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Custom, "mightPowerSkill_Custom", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerW, "mightPowerW", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_WayfarerCraft, "mightPowerSkill_WayfarerCraft", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_FieldTraining, "mightPowerSkill_FieldTraining", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerG, "mightPowerG", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Sprint, "mightPowerSkill_Sprint", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Fortitude, "mightPowerSkill_Fortitude", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Grapple, "mightPowerSkill_Grapple", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Cleave, "mightPowerSkill_Cleave", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Whirlwind, "mightPowerSkill_Whirlwind", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerS, "mightPowerS", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_SniperFocus, "mightPowerSkill_SniperFocus", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Headshot, "mightPowerSkill_Headshot", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_DisablingShot, "mightPowerSkill_DisablingShot", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_AntiArmor, "mightPowerSkill_AntiArmor", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ShadowSlayer, "mightPowerSkill_ShadowSlayer", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerB, "mightPowerB", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_BladeFocus, "mightPowerSkill_BladeFocus", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_BladeArt, "mightPowerSkill_BladeArt", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_SeismicSlash, "mightPowerSkill_SeismicSlash", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_BladeSpin, "mightPowerSkill_BladeSpin", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PhaseStrike, "mightPowerSkill_PhaseStrike", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerR, "mightPowerR", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_RangerTraining, "mightPowerSkill_RangerTraining", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_BowTraining, "mightPowerSkill_BowTraining", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PoisonTrap, "mightPowerSkill_PoisonTrap", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_AnimalFriend, "mightPowerSkill_AnimalFriend", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ArrowStorm, "mightPowerSkill_ArrowStorm", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerF, "mightPowerF", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Disguise, "mightPowerSkill_Disguise", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Mimic, "mightPowerSkill_Mimic", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Reversal, "mightPowerSkill_Reversal", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Transpose, "mightPowerSkill_Transpose", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Possess, "mightPowerSkill_Possess", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerP, "mightPowerP", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PsionicAugmentation, "mightPowerSkill_PsionicAugmentation", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PsionicBarrier, "mightPowerSkill_PsionicBarrier", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PsionicBlast, "mightPowerSkill_PsionicBlast", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PsionicDash, "mightPowerSkill_PsionicDash", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PsionicStorm, "mightPowerSkill_PsionicStorm", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerDK, "mightPowerDK", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Shroud, "mightPowerSkill_Shroud", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_WaveOfFear, "mightPowerSkill_WaveOfFear", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Spite, "mightPowerSkill_Spite", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_LifeSteal, "mightPowerSkill_LifeSteal", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_GraveBlade, "mightPowerSkill_GraveBlade", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerM, "mightPowerM", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Chi, "mightPowerSkill_Chi", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_MindOverBody, "mightPowerSkill_MindOverBody", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Meditate, "mightPowerSkill_Meditate", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_TigerStrike, "mightPowerSkill_TigerStrike", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_DragonStrike, "mightPowerSkill_DragonStrike", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ThunderStrike, "mightPowerSkill_ThunderStrike", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerC, "mightPowerC", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ProvisionerAura, "mightPowerSkill_ProvisionerAura", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_TaskMasterAura, "mightPowerSkill_TaskMasterAura", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_CommanderAura, "mightPowerSkill_CommanderAura", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_StayAlert, "mightPowerSkill_StayAlert", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_MoveOut, "mightPowerSkill_MoveOut", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_HoldTheLine, "mightPowerSkill_HoldTheLine", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerSS, "mightPowerSS", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PistolSpec, "mightPowerSkill_PistolSpec", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_RifleSpec, "mightPowerSkill_RifleSpec", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ShotgunSpec, "mightPowerSkill_ShotgunSpec", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_CQC, "mightPowerSkill_CQC", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_FirstAid, "mightPowerSkill_FirstAid", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_60mmMortar, "mightPowerSkill_60mmMortar", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerShadow, "mightPowerShadow", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_ShadowStrike, "mightPowerSkill_ShadowStrike", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Nightshade, "mightPowerSkill_Nightshade", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_VeilOfShadows, "mightPowerSkill_VeilOfShadows", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPower>(ref mightPowerApothecary, "mightPowerApothecary", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Herbalist, "mightPowerSkill_Herbalist", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_PoisonFlask, "mightPowerSkill_PoisonFlask", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_Elixir, "mightPowerSkill_Elixir", (LookMode)2, new object[0]);
            Scribe_Collections.Look<MightPowerSkill>(ref mightPowerSkill_SoothingBalm, "mightPowerSkill_SoothingBalm", (LookMode)2, new object[0]);
        }
    }
}