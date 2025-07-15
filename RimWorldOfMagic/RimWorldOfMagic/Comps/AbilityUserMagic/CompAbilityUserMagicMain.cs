using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using RimWorld;
using UnityEngine;
using AbilityUser;
using Verse;
using Verse.AI;
using Verse.Sound;
using AbilityUserAI;
using TorannMagic.Ideology;
using TorannMagic.TMDefs;
using TorannMagic.Utils;

namespace TorannMagic
{
    [CompilerGenerated]
    [Serializable]
    [StaticConstructorOnStartup]
    public partial class CompAbilityUserMagic : CompAbilityUserTMBase
    {
        public static List<TMAbilityDef> MagicAbilities = null;
        
        public string LabelKey = "TM_Magic";

        public bool firstTick;
        public bool magicPowersInitialized;
        public bool magicPowersInitializedForColonist = true;
        private bool colonistPowerCheck = true;
        private int resMitigationDelay = 0;
        private int damageMitigationDelay = 0;
        private int damageMitigationDelayMS = 0;
        public int magicXPRate = 1000;
        public int lastXPGain;
        
        private bool doOnce = true;
        private List<IntVec3> deathRing = new List<IntVec3>();
        public float weaponCritChance;
        public LocalTargetInfo SecondTarget = null;
        public List<TM_EventRecords> magicUsed = new List<TM_EventRecords>();

        public bool spell_Rain;
        public bool spell_Blink;
        public bool spell_Teleport;
        public bool spell_Heal;
        public bool spell_Heater;
        public bool spell_Cooler;
        public bool spell_DryGround;
        public bool spell_WetGround;
        public bool spell_ChargeBattery;
        public bool spell_SmokeCloud;
        public bool spell_Extinguish;
        public bool spell_EMP;
        public bool spell_Firestorm;
        public bool spell_Blizzard;
        public bool spell_SummonMinion;
        public bool spell_TransferMana;
        public bool spell_SiphonMana;
        public bool spell_RegrowLimb;
        public bool spell_EyeOfTheStorm;
        public bool spell_ManaShield;
        public bool spell_FoldReality;
        public bool spell_Resurrection;
        public bool spell_PowerNode;
        public bool spell_Sunlight;
        public bool spell_HolyWrath;
        public bool spell_LichForm;
        public bool spell_Flight;
        public bool spell_SummonPoppi;
        public bool spell_BattleHymn;
        public bool spell_CauterizeWound;
        public bool spell_FertileLands;
        public bool spell_SpellMending;
        public bool spell_ShadowStep;
        public bool spell_ShadowCall;
        public bool spell_Scorn;
        public bool spell_PsychicShock;
        public bool spell_SummonDemon;
        public bool spell_Meteor;
        public bool spell_Teach;
        public bool spell_OrbitalStrike;
        public bool spell_BloodMoon;
        public bool spell_EnchantedAura;
        public bool spell_Shapeshift;
        public bool spell_ShapeshiftDW;
        public bool spell_Blur;
        public bool spell_BlankMind;
        public bool spell_DirtDevil;
        public bool spell_MechaniteReprogramming;
        public bool spell_ArcaneBolt;
        public bool spell_LightningTrap;
        public bool spell_Invisibility;
        public bool spell_BriarPatch;
        public bool spell_Recall;
        public bool spell_MageLight;
        public bool spell_SnapFreeze;
        public bool spell_Ignite;
        public bool spell_CreateLight;
        public bool spell_EqualizeLight;
        public bool spell_HeatShield;

        private bool item_StaffOfDefender;

        public float maxMP = 1;
        public float mpRegenRate = 1;
        public float mpCost = 1;
        public float arcaneDmg = 1;

        public List<TM_ChaosPowers> chaosPowers = new List<TM_ChaosPowers>();
        public TMAbilityDef mimicAbility = null;
        public List<Thing> summonedMinions = new List<Thing>();
        public List<Thing> supportedUndead = new List<Thing>();
        public List<Thing> summonedSentinels = new List<Thing>();
        public List<Pawn> stoneskinPawns = new List<Pawn>();
        public IntVec3 earthSprites = default(IntVec3);
        public bool earthSpritesInArea;
        public Map earthSpriteMap;
        public int nextEarthSpriteAction;
        public int nextEarthSpriteMote;
        public int earthSpriteType;
        private bool dismissEarthSpriteSpell;
        public List<Thing> summonedLights = new List<Thing>();
        public List<Thing> summonedHeaters = new List<Thing>();
        public List<Thing> summonedCoolers = new List<Thing>();
        public List<Thing> summonedPowerNodes = new List<Thing>();
        public ThingDef guardianSpiritType;
        public Pawn soulBondPawn;
        private bool dismissMinionSpell;
        private bool dismissUndeadSpell;
        private bool dismissSunlightSpell;
        private bool dispelStoneskin;
        private bool dismissCoolerSpell;
        private bool dismissHeaterSpell;
        private bool dismissPowerNodeSpell;
        private bool dispelEnchantWeapon;
        private bool dismissEnchanterStones;
        private bool dismissLightningTrap;
        private bool shatterSentinel;
        private bool dismissGuardianSpirit;
        private bool dispelLivingWall;
        private bool dispelBrandings;
        public List<IntVec3> fertileLands = new List<IntVec3>();
        public Thing mageLightThing;
        public bool mageLightActive;
        public bool mageLightSet;
        public bool useTechnoBitToggle = true;
        public bool useTechnoBitRepairToggle = true;
        public Vector3 bitPosition = Vector3.zero;
        private bool bitFloatingDown = true;
        private float bitOffset = .45f;
        public int technoWeaponDefNum = -1;
        public Thing technoWeaponThing;
        public ThingDef technoWeaponThingDef;
        public QualityCategory technoWeaponQC = QualityCategory.Normal;
        public bool useElementalShotToggle = true;
        public Building overdriveBuilding;
        public int overdriveDuration;
        public float overdrivePowerOutput;
        public int overdriveFrequency = 100;
        public Building sabotageBuilding = null;
        public bool ArcaneForging;
        public List<Pawn> weaponEnchants = new List<Pawn>();
        public Thing enchanterStone;
        public List<Thing> enchanterStones = new List<Thing>();
        public List<Thing> lightningTraps = new List<Thing>();        
        public IncidentDef predictionIncidentDef;
        public int predictionTick;
        public int predictionHash;
        private List<Pawn> hexedPawns = new List<Pawn>();
        //Recall fields
        //position, hediffs, needs, mana, manual recall bool, recall duration
        public IntVec3 recallPosition = default(IntVec3);
        public Map recallMap;
        public List<string> recallNeedDefnames;
        public List<float> recallNeedValues;
        public List<Hediff> recallHediffList;
        public List<float> recallHediffDefSeverityList;
        public List<int> recallHediffDefTicksRemainingList;
        public List<Hediff_Injury> recallInjuriesList;
        public bool recallSet;
        public int recallExpiration;
        public bool recallSpell;
        public FlyingObject_SpiritOfLight SoL;
        public Pawn bondedSpirit;
        //public List<TMDefs.TM_Branding> brandings = new List<TMDefs.TM_Branding>();
        public List<Pawn> brandedPawns = new List<Pawn>();
        public List<Pawn> brands = new List<Pawn>();
        public List<HediffDef> brandDefs = new List<HediffDef>();
        public bool sigilSurging;
        public bool sigilDraining;
        public FlyingObject_LivingWall livingWall;
        public int lastChaosTraditionTick;
        public ThingOwner<ThingWithComps> magicWardrobe;
        public SkillRecord incitePassionSkill = null;

        // Cached values calculated in TM_PawnTracker
        private bool initializedIsMagicUser;
        private bool isMagicUser;  // Cached version

        private static HashSet<ushort> magicTraitIndexes = new HashSet<ushort>()
        {
            TorannMagicDefOf.Enchanter.index,
            TorannMagicDefOf.BloodMage.index,
            TorannMagicDefOf.Technomancer.index,
            TorannMagicDefOf.Geomancer.index,
            TorannMagicDefOf.Warlock.index,
            TorannMagicDefOf.Succubus.index,
            TorannMagicDefOf.Faceless.index,
            TorannMagicDefOf.InnerFire.index,
            TorannMagicDefOf.HeartOfFrost.index,
            TorannMagicDefOf.StormBorn.index,
            TorannMagicDefOf.Arcanist.index,
            TorannMagicDefOf.Paladin.index,
            TorannMagicDefOf.Summoner.index,
            TorannMagicDefOf.Druid.index,
            TorannMagicDefOf.Necromancer.index,
            TorannMagicDefOf.Lich.index,
            TorannMagicDefOf.Priest.index,
            TorannMagicDefOf.TM_Bard.index,
            TorannMagicDefOf.Chronomancer.index,
            TorannMagicDefOf.ChaosMage.index,
            TorannMagicDefOf.TM_Wanderer.index
        };

        public class ChainedMagicAbility
        {
            public ChainedMagicAbility(TMAbilityDef _ability, int _expirationTicks, bool _expires)
            {
                abilityDef = _ability;
                expirationTicks = _expirationTicks;
                expires = _expires;
            }
            public TMAbilityDef abilityDef;
            public int expirationTicks;
            public bool expires = true;
        }
        public List<ChainedMagicAbility> chainedAbilitiesList = new List<ChainedMagicAbility>();

        private Effecter powerEffecter;
        private int powerModifier;
        private int maxPower = 10;
        private int previousHexedPawns;
        public int nextEntertainTick = -1;
        public int nextSuccubusLovinTick = -1;

        public List<Pawn> BrandPawns
        {
            get
            {
                if (brands == null)
                {
                    brands = new List<Pawn>();
                    brands.Clear();
                }
                return brands;
            }
        }

        public List<HediffDef> BrandDefs
        {
            get
            {
                if (brandDefs == null)
                {
                    brandDefs = new List<HediffDef>();
                    brandDefs.Clear();
                }
                return brandDefs;
            }
        }

        public ThingOwner<ThingWithComps> MagicWardrobe
        {
            get
            {
                if(magicWardrobe == null)
                {
                    magicWardrobe = new ThingOwner<ThingWithComps>();
                }
                return magicWardrobe;
            }
        }

        public List<TM_EventRecords> MagicUsed
        {
            get
            {
                if (magicUsed == null)
                {
                    magicUsed = new List<TM_EventRecords>();
                }
                return magicUsed;
            }
            set
            {
                if (magicUsed == null)
                {
                    magicUsed = new List<TM_EventRecords>();
                }
                magicUsed = value;                
            }
        }

        public List<Pawn> StoneskinPawns
        {
            get
            {
                if(stoneskinPawns == null)
                {
                    stoneskinPawns = new List<Pawn>();
                }
                List<Pawn> tmpList = new List<Pawn>();
                foreach(Pawn p in stoneskinPawns)
                {
                    if(p.DestroyedOrNull() || p.Dead)
                    {
                        tmpList.Add(p);
                    }
                }
                for(int i = 0; i < tmpList.Count; i++)
                {
                    stoneskinPawns.Remove(tmpList[i]);
                }
                return stoneskinPawns;
            }
        }

        public ThingDef GuardianSpiritType
        {
            get
            {
                if(guardianSpiritType == null)
                {
                    float rnd = Rand.Value;
                    
                    if(rnd < .34f)
                    {
                        guardianSpiritType = TorannMagicDefOf.TM_SpiritBearR;
                    }
                    else if (rnd < .67f)
                    {
                        guardianSpiritType = TorannMagicDefOf.TM_SpiritMongooseR;
                    }
                    else
                    {
                        guardianSpiritType = TorannMagicDefOf.TM_SpiritCrowR;
                    }
                }
                return guardianSpiritType;
            }
        }

        public bool HasTechnoBit
        {
            get
            {
                return IsMagicUser && MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoBit).learned;
            }
        }
        public bool HasTechnoTurret
        {
            get
            {
                return IsMagicUser && MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoTurret).learned;
            }
        }

        public bool HasTechnoWeapon
        {
            get
            {
                return IsMagicUser && MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoWeapon).learned;
            }
        }

        public int PowerModifier
        {
            get => powerModifier;
            set
            {
                TM_MoteMaker.ThrowSiphonMote(Pawn.DrawPos, Pawn.Map, 1f);
                powerModifier = Mathf.Clamp(value, 0, maxPower);

                if (powerModifier != 0 || powerEffecter == null) return;
                powerEffecter.Cleanup();
                powerEffecter = null;
            }
        }

        public float GetSkillDamage()
        {
            float result;
            float strFactor = 1f;
            if (IsMagicUser)
            {
                strFactor = arcaneDmg;
            }

            if (Pawn.equipment?.Primary != null)
            {
                if (Pawn.equipment.Primary.def.IsMeleeWeapon)
                {
                    result = TM_Calc.GetSkillDamage_Melee(Pawn, strFactor);
                    weaponCritChance = TM_Calc.GetWeaponCritChance(Pawn.equipment.Primary);
                }
                else
                {
                    result = TM_Calc.GetSkillDamage_Range(Pawn, strFactor);
                    weaponCritChance = 0f;
                }
            }
            else
            {
                result = Pawn.GetStatValue(StatDefOf.MeleeDPS, false) * strFactor;
            }

            return result;
        }

        private MagicData magicData;
        public MagicData MagicData
        {
            get
            {
                bool flag = magicData == null && IsMagicUser;
                if (flag)
                {
                    magicData = new MagicData(this);
                }
                return magicData;
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            bool flag = powerEffecter != null;
            if (flag)
            {
                powerEffecter.Cleanup();
            }
        }

        public List<Pawn> HexedPawns
        {
            get
            {
                if(hexedPawns == null)
                {
                    hexedPawns = new List<Pawn>();
                    hexedPawns.Clear();
                }
                List<Pawn> validPawns = new List<Pawn>();
                validPawns.Clear();
                foreach (Pawn p in hexedPawns)
                {
                    if (p != null && !p.Destroyed && !p.Dead)
                    {
                        if (p.health != null && p.health.hediffSet != null && p.health.hediffSet.HasHediff(TorannMagicDefOf.TM_HexHD))
                        {
                            validPawns.Add(p);
                        }
                    }
                }
                hexedPawns = validPawns;
                return hexedPawns;
            }
        }

        public bool shouldDraw = true;
        public override void PostDraw()
        {
            if (Pawn.DestroyedOrNull()) return;
            if (Pawn.Dead) return;
            if (Pawn.Map == null) return;
            if (shouldDraw && IsMagicUser)
            {
                
                if (ModOptions.Settings.Instance.AIFriendlyMarking && Pawn.IsColonist && IsMagicUser)
                {
                    if (!Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                    {
                        DrawMark();
                    }
                }
                if (ModOptions.Settings.Instance.AIMarking && !Pawn.IsColonist && IsMagicUser)
                {
                    if (!Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                    {
                        DrawMark();
                    }
                }

                if (MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower mp) => mp.abilityDef == TorannMagicDefOf.TM_TechnoBit).learned && Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_TechnoBitHD))
                {
                    DrawTechnoBit();
                }

                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DemonScornHD) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DemonScornHD_I) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DemonScornHD_II) || Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DemonScornHD_III))
                {
                    DrawScornWings();
                }

                if (mageLightActive)
                {
                    DrawMageLight();
                }

                Enchantment.CompEnchant compEnchant = Pawn.GetComp<Enchantment.CompEnchant>();

                if (IsMagicUser && compEnchant != null && compEnchant.enchantingContainer != null && compEnchant.enchantingContainer.Count > 0)
                {
                    DrawEnchantMark();
                }
            }
            base.PostDraw();
        }
        
        private void SingleEvent()
        {
            doOnce = false;
        }

        private void DoOncePerLoad()
        {
            if (spell_FertileLands)
            {
                if (fertileLands.Count > 0)
                {
                    List<IntVec3> cellList = ModOptions.Constants.GetGrowthCells();
                    if (cellList.Count != 0)
                    {
                        for (int i = 0; i < fertileLands.Count; i++)
                        {
                            ModOptions.Constants.RemoveGrowthCell(fertileLands[i]);
                        }
                    }
                    ModOptions.Constants.SetGrowthCells(fertileLands);
                    RemovePawnAbility(TorannMagicDefOf.TM_FertileLands);
                    AddPawnAbility(TorannMagicDefOf.TM_DismissFertileLands);
                }
            }
            //to fix filtering of succubus abilities
            if(Pawn.story.traits.HasTrait(TorannMagicDefOf.Succubus))
            {
                for(int i = 0; i < MagicData.MagicPowersWD.Count; i++)
                {
                    MagicPower wd = MagicData.MagicPowersWD[i];
                    if (wd.learned && wd.abilityDef == TorannMagicDefOf.TM_SoulBond)
                    {
                        MagicData.MagicPowersSD.FirstOrDefault((MagicPower p) => p.abilityDef == TorannMagicDefOf.TM_SoulBond).learned = true;
                    }
                    else if(wd.learned && wd.abilityDef == TorannMagicDefOf.TM_ShadowBolt)
                    {
                        MagicData.MagicPowersSD.FirstOrDefault((MagicPower p) => p.abilityDef == TorannMagicDefOf.TM_ShadowBolt).learned = true;
                    }
                    else if (wd.learned && wd.abilityDef == TorannMagicDefOf.TM_Dominate)
                    {
                        MagicData.MagicPowersSD.FirstOrDefault((MagicPower p) => p.abilityDef == TorannMagicDefOf.TM_Dominate).learned = true;
                    }
                }
            }
        }

        public override void CompTick()
        {

            Pawn pawn = Pawn;
            if (pawn?.story == null) return;
            if (Pawn.IsShambler || Pawn.IsGhoul)
            {
                if (magicData != null)
                {
                    RemoveAbilityUser();
                }
                return;
            }

            // If we aren't on map, handle ability cooldown per long tick
            if (!pawn.Spawned)
            {
                if (pawn.Map != null || Find.TickManager.TicksGame % 600 != 0) return;  // Not time to caravan tick.
                if (!IsMagicUser) return;  // We won't tick at all if we aren't a magic user

                var allPowers = AbilityData.AllPowers;
                for (int i = allPowers.Count - 1; i >= 0; i--)
                {
                    allPowers[i].CooldownTicksLeft = Math.Max(allPowers[i].CooldownTicksLeft - 600, 0);
                }
                return;
            }

            // If we aren't magic, check if we can be inspired
            if (!IsMagicUser)
            {
                if (!ModsConfig.IdeologyActive) return;
                if (Find.TickManager.TicksGame % 2501 != 0) return;
                if (!pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted)) return;

                if (!pawn.Inspired && pawn.CurJobDef == JobDefOf.LayDown && Rand.Chance(.025f))
                {
                    pawn.mindState.inspirationHandler.TryStartInspiration(TorannMagicDefOf.ID_ArcanePathways);
                }
                return;
            }

            if (!TickConditionsMet) return;  // Cached in TM_PawnTracker

            // Finally, let's do a magic tick!
            if (!firstTick) PostInitializeTick();
            base.CompTick();
            age++;
            if (chainedAbilitiesList != null && chainedAbilitiesList.Count > 0)
            {
                for (int i = 0; i < chainedAbilitiesList.Count; i++)
                {
                    chainedAbilitiesList[i].expirationTicks--;
                    if (chainedAbilitiesList[i].expires && chainedAbilitiesList[i].expirationTicks <= 0)
                    {
                        RemovePawnAbility(chainedAbilitiesList[i].abilityDef);
                        chainedAbilitiesList.Remove(chainedAbilitiesList[i]);
                        break;
                    }
                }
            }
            if (Mana != null)
            {
                if (Find.TickManager.TicksGame % 4 == 0 && Pawn.CurJob != null && Pawn.CurJobDef == JobDefOf.DoBill && Pawn.CurJob.targetA != null && Pawn.CurJob.targetA.Thing != null)
                {
                    DoArcaneForging();
                }
                if (Mana.CurLevel >= (.99f * Mana.MaxLevel))
                {
                    if (age > (lastXPGain + magicXPRate))
                    {
                        MagicData.MagicUserXP++;
                        lastXPGain = age;
                    }
                }
                if (Find.TickManager.TicksGame % 30 == 0)
                {
                    bool flag5 = MagicUserXP > MagicUserXPTillNextLevel;
                    if (flag5)
                    {
                        LevelUp(false);
                    }
                }
                if (Find.TickManager.TicksGame % 60 == 0)
                {
                    if (Pawn.IsColonist && !magicPowersInitializedForColonist)
                    {
                        ResolveFactionChange();
                    }
                    else if (!Pawn.IsColonist)
                    {
                        magicPowersInitializedForColonist = false;
                    }
                    if (Pawn.IsColonist)
                    {
                        ResolveEnchantments();
                        for (int i = 0; i < summonedMinions.Count; i++)
                        {
                            Pawn evaluateMinion = summonedMinions[i] as Pawn;
                            if (evaluateMinion == null || evaluateMinion.Dead || evaluateMinion.Destroyed)
                            {
                                summonedMinions.Remove(summonedMinions[i]);
                            }
                        }
                        ResolveMinions();
                        ResolveSustainers();
                        if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Necromancer) || Pawn.story.traits.HasTrait(TorannMagicDefOf.Lich) || (customClass != null && customClass.isNecromancer))
                        {
                            ResolveUndead();
                        }
                        ResolveEffecter();
                        ResolveClassSkills();
                        ResolveSpiritOfLight();
                        ResolveChronomancerTimeMark();
                    }
                }

                if (autocastTick < Find.TickManager.TicksGame)  //180 default
                {
                    if (!Pawn.Dead && !Pawn.Downed && Pawn.Map != null && Pawn.story != null && Pawn.story.traits != null && MagicData != null && AbilityData != null && !Pawn.InMentalState)
                    {
                        if (Pawn.IsColonist)
                        {
                            autocastTick = Find.TickManager.TicksGame + (int)Rand.Range(.8f * ModOptions.Settings.Instance.autocastEvaluationFrequency, 1.2f * ModOptions.Settings.Instance.autocastEvaluationFrequency);
                            ResolveAutoCast();
                        }
                        else if (ModOptions.Settings.Instance.AICasting && (!Pawn.IsPrisoner || Pawn.IsFighting()) && (Pawn.guest != null && !Pawn.IsSlave))
                        {
                            float tickMult = ModOptions.Settings.Instance.AIAggressiveCasting ? 1f : 2f;
                            autocastTick = Find.TickManager.TicksGame + (int)(Rand.Range(.75f * ModOptions.Settings.Instance.autocastEvaluationFrequency, 1.25f * ModOptions.Settings.Instance.autocastEvaluationFrequency) * tickMult);
                            ResolveAIAutoCast();
                        }
                    }
                }
                if (!Pawn.IsColonist && ModOptions.Settings.Instance.AICasting && ModOptions.Settings.Instance.AIAggressiveCasting && Find.TickManager.TicksGame > nextAICastAttemptTick) //Aggressive AI Casting
                {
                    nextAICastAttemptTick = Find.TickManager.TicksGame + Rand.Range(300, 500);
                    if (Pawn.jobs != null && Pawn.CurJobDef != TorannMagicDefOf.TMCastAbilitySelf && Pawn.CurJobDef != TorannMagicDefOf.TMCastAbilityVerb)
                    {
                        IEnumerable<AbilityUserAIProfileDef> enumerable = Pawn.EligibleAIProfiles();
                        if (enumerable != null && enumerable.Count() > 0)
                        {
                            foreach (AbilityUserAIProfileDef item in enumerable)
                            {
                                if (item != null)
                                {
                                    AbilityAIDef useThisAbility = null;
                                    if (item.decisionTree != null)
                                    {
                                        useThisAbility = item.decisionTree.RecursivelyGetAbility(Pawn);
                                    }
                                    if (useThisAbility != null)
                                    {
                                        ThingComp val = Pawn.AllComps.First((ThingComp comp) => ((object)comp).GetType() == item.compAbilityUserClass);
                                        CompAbilityUser compAbilityUser = val as CompAbilityUser;
                                        if (compAbilityUser != null)
                                        {
                                            PawnAbility pawnAbility = compAbilityUser.AbilityData.AllPowers.First((PawnAbility ability) => ability.Def == useThisAbility.ability);
                                            string reason = "";
                                            if (pawnAbility.CanCastPowerCheck(AbilityContext.AI, out reason))
                                            {
                                                LocalTargetInfo target = useThisAbility.Worker.TargetAbilityFor(useThisAbility, Pawn);
                                                if (target.IsValid)
                                                {
                                                    pawnAbility.UseAbility(AbilityContext.Player, target);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Find.TickManager.TicksGame % overdriveFrequency == 0)
            {
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Technomancer) || (TM_ClassUtility.ClassHasAbility(TorannMagicDefOf.TM_Overdrive)))
                {
                    ResolveTechnomancerOverdrive();
                }
            }
            if (Find.TickManager.TicksGame % 299 == 0) //cache weapon damage for tooltip and damage calculations
            {
                weaponDamage = GetSkillDamage(); // TM_Calc.GetSkillDamage(this.Pawn);
            }
            if (Find.TickManager.TicksGame % 601 == 0)
            {
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Warlock))
                {
                    ResolveWarlockEmpathy();
                }
            }
            if (Find.TickManager.TicksGame % 602 == 0)
            {
                ResolveMagicUseEvents();
            }
            if (Find.TickManager.TicksGame % 2001 == 0)
            {
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Succubus))
                {
                    ResolveSuccubusLovin();
                }
            }
            if (deathRetaliating)
            {
                DoDeathRetaliation();
            }
            else if (Find.TickManager.TicksGame % 67 == 0 && !Pawn.IsColonist && Pawn.Downed)
            {
                DoDeathRetaliation();
            }
        }

        public void PostInitializeTick()
        {
            if (doOnce) SingleEvent();
            Trait t = Pawn.story.traits.GetTrait(TorannMagicDefOf.TM_Possessed);
            if (t != null && !Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SpiritPossessionHD))
            {
                Pawn.story.traits.RemoveTrait(t);
            }
            else
            {
                firstTick = true;
                Initialize();
                ResolveMagicTab();
                ResolveMagicPowers();
                ResolveMana();
                DoOncePerLoad();
            }
        }

        public bool IsMagicUser => initializedIsMagicUser ? isMagicUser : SetIsMagicUser();
        public bool SetIsMagicUser()
        {
            if (Pawn?.story == null) return isMagicUser = false;
            initializedIsMagicUser = true;
            if (customClass != null) return isMagicUser = true;
            if (customClass == null && customIndex == -2)
            {
                customIndex = TM_ClassUtility.CustomClassIndexOfBaseMageClass(Pawn.story.traits.allTraits);
                if (customIndex >= 0)
                {
                    TM_CustomClass foundCustomClass = TM_ClassUtility.CustomClasses[customIndex];
                    if (!foundCustomClass.isMage)
                    {
                        customIndex = -1;
                        return isMagicUser = false;
                    }
                    customClass = foundCustomClass;
                    return isMagicUser = true;
                }
            }
            // If any traits are in our generated set of magic traits, we are magic.
            for (int i = Pawn.story.traits.allTraits.Count - 1; i >= 0; i--)
            {
                if (magicTraitIndexes.Contains(Pawn.story.traits.allTraits[i].def.index))
                    return isMagicUser = true;
            }
            if (AdvancedClasses.Count > 0 || TM_Calc.IsWanderer(Pawn)) return isMagicUser = true;
            if (TM_Calc.HasAdvancedClass(Pawn))
            {
                foreach (TM_CustomClass cc in TM_ClassUtility.GetAdvancedClassesForPawn(Pawn))
                {
                    if (cc.isMage)
                    {
                        AdvancedClasses.Add(cc);
                        return isMagicUser = true;
                    }
                }
            }
            return isMagicUser = false;
        }

        private Dictionary<int, int> cacheXPFL = new Dictionary<int, int>();
        public int GetXPForLevel(int lvl)
        {
            if (!cacheXPFL.ContainsKey(lvl))
            {
                IntVec2 c1 = new IntVec2(0, 40); 
                IntVec2 c2 = new IntVec2(5, 30);
                IntVec2 c3 = new IntVec2(15, 20); 
                IntVec2 c4 = new IntVec2(30, 10);
                IntVec2 c5 = new IntVec2(200, 0);

                int val = 0;

                for (int i = 0; i < lvl + 1; i++)
                {
                    val += (Mathf.Clamp(i, c1.x, c2.x - 1) * c1.z) + c1.z;
                    if (i >= c2.x)
                    {
                        val += (Mathf.Clamp(i, c2.x, c3.x - 1) * c2.z) + c2.z;
                    }
                    if (i >= c3.x)
                    {
                        val += (Mathf.Clamp(i, c3.x, c4.x - 1) * c3.z) + c3.z;
                    }
                    if (i >= c4.x)
                    {
                        val += (Mathf.Clamp(i, c4.x, c5.x - 1) * c4.z) + c4.z;
                    }
                }
                cacheXPFL.Add(lvl, val);
            }
            if (cacheXPFL.ContainsKey(lvl))
            {
                return cacheXPFL[lvl];
            }
            else
            {
                return 0;
            }
        }

        public int MagicUserLevel
        {
            get
            {                
                return MagicData.MagicUserLevel;
            }
            set
            {
                bool flag = value > MagicData.MagicUserLevel;
                if (flag)
                {
                    MagicData.MagicAbilityPoints++;
                    bool flag2 = MagicData.MagicUserXP < GetXPForLevel(value - 1);
                    if (flag2)
                    {
                        MagicData.MagicUserXP = GetXPForLevel(value - 1);
                    }
                }
                MagicData.MagicUserLevel = value;
            }
        }

        public int MagicUserXP
        {
            get
            {
                return MagicData.MagicUserXP;
            }
            set
            {
                MagicData.MagicUserXP = value;
            }
        }
        
        public float XPLastLevel
        {
            get
            {
                float result = 0f;
                bool flag = MagicUserLevel > 0;
                if (flag)
                {
                    
                    result = GetXPForLevel(MagicUserLevel - 1);
                }
                return result;
            }
        }

        public float XPTillNextLevelPercent
        {
            get
            {
                return ((float)MagicData.MagicUserXP - XPLastLevel) / ((float)MagicUserXPTillNextLevel - XPLastLevel);
            }
        }

        public int MagicUserXPTillNextLevel
        {
            get
            {
                if(MagicUserXP < XPLastLevel)
                {
                    MagicUserXP = (int)XPLastLevel;
                }
                return GetXPForLevel(MagicUserLevel);
            }
        }

        public void LevelUp(bool hideNotification = false)
        {
            if (!(Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless) || Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Wayfarer)))
            {
                if (MagicUserLevel < (customClass?.maxMageLevel ?? 200))
                {
                    MagicUserLevel++;
                    bool flag = !hideNotification;
                    if (flag)
                    {                        
                        if (Pawn.IsColonist && ModOptions.Settings.Instance.showLevelUpMessage)
                        {
                            Messages.Message(TranslatorFormattedStringExtensions.Translate("TM_MagicLevelUp",
                        parent.Label
                            ), Pawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
                else
                {
                    MagicUserXP = (int)XPLastLevel;
                }
            }
        }

        public void LevelUpPower(MagicPower power)
        {
            foreach (AbilityUser.AbilityDef current in power.TMabilityDefs)
            {
                RemovePawnAbility(current);
            }
            power.level++;
            AddPawnAbility(power.abilityDef, true, -1f);
            UpdateAbilities();
        }

        public Need_Mana Mana
        {
            get
            {
                if (!Pawn.DestroyedOrNull() && !Pawn.Dead)
                {
                    return Pawn.needs.TryGetNeed<Need_Mana>();
                }
                return null;
            }
        }

        public void ResolveFactionChange()
        {
            if (!colonistPowerCheck)
            {
                RemovePowers();
                spell_BattleHymn = false;
                RemovePawnAbility(TorannMagicDefOf.TM_BattleHymn);
                spell_Blizzard = false;
                RemovePawnAbility(TorannMagicDefOf.TM_Blizzard);
                spell_BloodMoon = false;
                RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon);
                spell_EyeOfTheStorm = false;
                RemovePawnAbility(TorannMagicDefOf.TM_EyeOfTheStorm);
                spell_Firestorm = false;
                RemovePawnAbility(TorannMagicDefOf.TM_Firestorm);
                spell_FoldReality = false;
                RemovePawnAbility(TorannMagicDefOf.TM_FoldReality);
                spell_HolyWrath = false;
                RemovePawnAbility(TorannMagicDefOf.TM_HolyWrath);
                spell_LichForm = false;
                RemovePawnAbility(TorannMagicDefOf.TM_BattleHymn);
                spell_Meteor = false;
                RemovePawnAbility(TorannMagicDefOf.TM_Meteor);
                spell_OrbitalStrike = false;
                RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike);
                spell_PsychicShock = false;
                RemovePawnAbility(TorannMagicDefOf.TM_PsychicShock);
                spell_RegrowLimb = false;
                spell_Resurrection = false;
                spell_Scorn = false;
                RemovePawnAbility(TorannMagicDefOf.TM_Scorn);
                spell_Shapeshift = false;
                spell_SummonPoppi = false;
                RemovePawnAbility(TorannMagicDefOf.TM_SummonPoppi);
                RemovePawnAbility(TorannMagicDefOf.TM_Recall);
                spell_Recall = false;
                RemovePawnAbility(TorannMagicDefOf.TM_LightSkipGlobal);
                RemovePawnAbility(TorannMagicDefOf.TM_LightSkipMass);
                RemovePawnAbility(TorannMagicDefOf.TM_SpiritOfLight);
                AssignAbilities();
            }
            magicPowersInitializedForColonist = true;
            colonistPowerCheck = true;
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            bool flag = MagicAbilities == null;
            if (flag)
            {
                if (magicPowersInitialized == false && MagicData != null)
                {
                    MagicData.MagicUserLevel = 0;
                    MagicData.MagicAbilityPoints = 0;
                    AssignAbilities();
                    if (!Pawn.IsColonist)
                    {
                        InitializeSpell();
                        colonistPowerCheck = false;
                    }
                }
                magicPowersInitialized = true;
                UpdateAbilities();
            }
        }

        public void AssignAdvancedClassAbilities(bool firstAssignment = false)
        {
            if (AdvancedClasses != null && AdvancedClasses.Count > 0)
            {
                for (int z = 0; z < MagicData.AllMagicPowers.Count; z++)
                {
                    TMAbilityDef ability = (TMAbilityDef)MagicData.AllMagicPowers[z].abilityDef;
                    foreach (TM_CustomClass cc in AdvancedClasses)
                    {
                        if (cc.classMageAbilities.Contains(ability))
                        {
                            MagicData.AllMagicPowers[z].learned = true;
                        }
                        if (MagicData.AllMagicPowers[z] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond) ||
                        MagicData.AllMagicPowers[z] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShadowBolt) ||
                        MagicData.AllMagicPowers[z] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate))
                        {
                            MagicData.AllMagicPowers[z].learned = false;
                        }
                        if (MagicData.AllMagicPowers[z].requiresScroll)
                        {
                            MagicData.AllMagicPowers[z].learned = false;
                        }
                        if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false) && !Rand.Chance(ability.learnChance))
                        {
                            MagicData.AllMagicPowers[z].learned = false;
                        }
                        if (MagicData.AllMagicPowers[z].learned)
                        {
                            if (ability.shouldInitialize)
                            {
                                AddPawnAbility(ability);
                            }
                            if (ability.childAbilities != null && ability.childAbilities.Count > 0)
                            {
                                for (int c = 0; c < ability.childAbilities.Count; c++)
                                {
                                    if (ability.childAbilities[c].shouldInitialize)
                                    {
                                        AddPawnAbility(ability.childAbilities[c]);
                                    }
                                }
                            }
                        }
                        if (cc.classHediff != null)
                        {
                            HealthUtility.AdjustSeverity(Pawn, customClass.classHediff, customClass.hediffSeverity);
                        }
                    }
                }
                MagicPower branding = MagicData.AllMagicPowers.FirstOrDefault((MagicPower p) => p.abilityDef == TorannMagicDefOf.TM_Branding);
                if (branding != null && branding.learned && firstAssignment)
                {
                    int count = 0;
                    while (count < 2)
                    {
                        TMAbilityDef tmpAbility = TM_Data.BrandList().RandomElement();
                        for (int i = 0; i < MagicData.AllMagicPowers.Count; i++)
                        {
                            TMAbilityDef ad = (TMAbilityDef)MagicData.AllMagicPowers[i].abilityDef;
                            if (!MagicData.AllMagicPowers[i].learned && ad == tmpAbility)
                            {
                                count++;
                                MagicData.AllMagicPowers[i].learned = true;
                                RemovePawnAbility(ad);
                                TryAddPawnAbility(ad);
                            }
                        }
                    }
                }                
            }
        }

        public override bool TryTransformPawn()
        {
            return IsMagicUser;
        }

        public bool TryAddPawnAbility(TMAbilityDef ability)
        {
            //add check to verify no ability is already added
            bool result = false;
            AddPawnAbility(ability, true, -1f);
            result = true;
            return result;
        }

        private void ClearPower(MagicPower current)
        {
            Log.Message("Removing ability: " + current.abilityDef.defName.ToString());
            RemovePawnAbility(current.abilityDef);
            UpdateAbilities();
        }

        public void ResetSkills()
        {
            MagicData.MagicPowerSkill_global_regen.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_regen_pwr").level = 0;
            MagicData.MagicPowerSkill_global_eff.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_eff_pwr").level = 0;
            MagicData.MagicPowerSkill_global_spirit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_spirit_pwr").level = 0;
            for (int i = 0; i < MagicData.AllMagicPowersWithSkills.Count; i++)
            {
                MagicData.AllMagicPowersWithSkills[i].level = 0;
                MagicData.AllMagicPowersWithSkills[i].learned = false;
                MagicData.AllMagicPowersWithSkills[i].autocast = false;
                TMAbilityDef ability = (TMAbilityDef)MagicData.AllMagicPowersWithSkills[i].abilityDef;
                MagicPowerSkill mps = MagicData.GetSkill_Efficiency(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
                mps = MagicData.GetSkill_Power(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
                mps = MagicData.GetSkill_Versatility(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
            }
            for(int i = 0; i < MagicData.AllMagicPowers.Count; i++)
            {                
                for(int j = 0; j < MagicData.AllMagicPowers[i].TMabilityDefs.Count; j++)
                {
                    TMAbilityDef ability = (TMAbilityDef)MagicData.AllMagicPowers[i].TMabilityDefs[j];
                    RemovePawnAbility(ability);
                }
                MagicData.AllMagicPowers[i].learned = false;
                MagicData.AllMagicPowers[i].autocast = false;
            }
            MagicUserLevel = 0;
            MagicUserXP = 0;
            MagicData.MagicAbilityPoints = 0;
            AssignAbilities();
          
        }

        public void RemoveTraits()
        {
            List<Trait> traits = Pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].def == TorannMagicDefOf.InnerFire || traits[i].def == TorannMagicDefOf.HeartOfFrost || traits[i].def == TorannMagicDefOf.StormBorn || traits[i].def == TorannMagicDefOf.Arcanist || traits[i].def == TorannMagicDefOf.Paladin ||
                    traits[i].def == TorannMagicDefOf.Druid || traits[i].def == TorannMagicDefOf.Priest || traits[i].def == TorannMagicDefOf.Necromancer || traits[i].def == TorannMagicDefOf.Warlock || traits[i].def == TorannMagicDefOf.Succubus ||
                    traits[i].def == TorannMagicDefOf.TM_Bard || traits[i].def == TorannMagicDefOf.Geomancer || traits[i].def == TorannMagicDefOf.Technomancer || traits[i].def == TorannMagicDefOf.BloodMage || traits[i].def == TorannMagicDefOf.Enchanter ||
                    traits[i].def == TorannMagicDefOf.Chronomancer || traits[i].def == TorannMagicDefOf.ChaosMage || traits[i].def == TorannMagicDefOf.TM_Wanderer)
                {
                    Log.Message("Removing trait " + traits[i].Label);
                    traits.Remove(traits[i]);
                    i--;
                }
                if (customClass != null)
                {
                    traits.Remove(Pawn.story.traits.GetTrait(customClass.classTrait));
                    customClass = null;
                    customIndex = -2;
                }
            }
        }

        public void RemoveTMagicHediffs()
        {
            List<Hediff> allHediffs = Pawn.health.hediffSet.hediffs;
            for (int i = 0; i < allHediffs.Count(); i++)
            {
                if (allHediffs[i].def.defName.StartsWith("TM_"))
                {
                    Pawn.health.RemoveHediff(allHediffs[i]);
                }
            }
        }

        public void RemoveAbilityUser()
        {
            RemovePowers();
            RemoveTMagicHediffs();
            RemoveTraits();
            magicData = null;
            Initialized = false;
            isMagicUser = false;
        }     

        public override List<HediffDef> IgnoredHediffs()
        {
            return new List<HediffDef>
            {
                TorannMagicDefOf.TM_MightUserHD
            };
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }        

        public void ResolveMagicUseEvents()
        {
            int expiryTick = Find.TickManager.TicksGame - 150000;
            for (int i = MagicUsed.Count - 1; i >= 0; i--)
            {
                if (expiryTick > MagicUsed[i].eventTick) MagicUsed.RemoveAt(i);
            }
        }

        public void ResolveAIAutoCast()
        {
            
            if (ModOptions.Settings.Instance.AICasting && Pawn.jobs != null && Pawn.CurJob != null && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilityVerb && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilitySelf && 
                Pawn.CurJob.def != JobDefOf.Ingest && Pawn.CurJob.def != JobDefOf.ManTurret && Pawn.GetPosture() == PawnPosture.Standing)
            {
                bool castSuccess = false;
                if (Mana != null && Mana.CurLevelPercentage >= ModOptions.Settings.Instance.autocastMinThreshold)
                {
                    foreach (MagicPower mp in MagicData.AllMagicPowersWithSkills)
                    {
                        if (mp.learned && mp.autocasting != null && mp.autocasting.magicUser && mp.autocasting.AIUsable)
                        {                            
                            //try
                            //{                             
                            TMAbilityDef tmad = mp.TMabilityDefs[mp.level] as TMAbilityDef; // issues with index?
                            bool canUseWithEquippedWeapon = true;
                            bool canUseIfViolentAbility = Pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) ? !tmad.MainVerb.isViolent : true;
                            if (!TM_Calc.HasResourcesForAbility(Pawn, tmad))
                            {
                                continue;
                            }
                            if (canUseWithEquippedWeapon && canUseIfViolentAbility)
                            {
                                PawnAbility ability = AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == tmad);
                                LocalTargetInfo currentTarget = Pawn.TargetCurrentlyAimingAt != null ? Pawn.TargetCurrentlyAimingAt : (Pawn.CurJob != null ? Pawn.CurJob.targetA : null);
                                if (mp.autocasting.type == AutocastType.OnTarget && currentTarget != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, currentTarget);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if(TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if(targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);                                        
                                        if (TN && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }
                                            if (mp.abilityDef.MainVerb.isViolent && !targetPawn.InMentalState)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        if (TNF && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }                                            
                                        }
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.MagicAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == AutocastType.OnSelf)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Pawn targetThing = localTarget.Pawn;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.MagicAbility_OnSelf.Evaluate(this, tmad, ability, mp, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == AutocastType.OnCell && currentTarget != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, currentTarget);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        IntVec3 targetThing = localTarget.Cell;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.MagicAbility_OnCell.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == AutocastType.OnNearby)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, currentTarget);
                                    if (localTarget != null && localTarget.IsValid)
                                    {
                                        Thing targetThing = localTarget.Thing;
                                        if (!mp.autocasting.ValidType(mp.autocasting.GetTargetType, localTarget))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.requiresLoS && !TM_Calc.HasLoSFromTo(Pawn.Position, targetThing, Pawn, mp.autocasting.minRange, ability.Def.MainVerb.range))
                                        {
                                            continue;
                                        }
                                        if (mp.autocasting.maxRange != 0f && mp.autocasting.maxRange < (Pawn.Position - targetThing.Position).LengthHorizontal)
                                        {
                                            continue;
                                        }
                                        bool TE = mp.autocasting.targetEnemy && targetThing.Faction != null && targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TE && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TN = mp.autocasting.targetNeutral && targetThing.Faction != null && !targetThing.Faction.HostileTo(Pawn.Faction);
                                        if (TN && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }
                                            if (mp.abilityDef.MainVerb.isViolent && !targetPawn.InMentalState)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TNF = mp.autocasting.targetNoFaction && targetThing.Faction == null;
                                        if (TNF && targetThing is Pawn)
                                        {
                                            Pawn targetPawn = targetThing as Pawn;
                                            if (targetPawn.Downed || targetPawn.IsPrisoner)
                                            {
                                                continue;
                                            }
                                        }
                                        bool TF = mp.autocasting.targetFriendly && targetThing.Faction == Pawn.Faction;
                                        if (!(TE || TN || TF || TNF))
                                        {
                                            continue;
                                        }
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.MagicAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                            }
                            //}
                            //catch
                            //{
                            //    Log.Message("no index found at " + mp.level + " for " + mp.abilityDef.defName);
                            //}
                        }
                        if (castSuccess) goto AIAutoCastExit;
                    }
                    AIAutoCastExit:;
                }
            }
        }

        private void ResolveSpiritOfLight()
        {
            if(SoL != null)
            {
                //if(!this.spell_CreateLight)
                //{
                //    this.RemovePawnAbility(TorannMagicDefOf.TM_SoL_CreateLight);
                //    this.AddPawnAbility(TorannMagicDefOf.TM_SoL_CreateLight);
                //    this.spell_CreateLight = true;
                //}
                if(!spell_EqualizeLight)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_SoL_Equalize);
                    AddPawnAbility(TorannMagicDefOf.TM_SoL_Equalize);
                    spell_EqualizeLight = true;
                }
            }
            if(SoL == null)
            {
                if(spell_CreateLight || spell_EqualizeLight)
                {
                    //this.RemovePawnAbility(TorannMagicDefOf.TM_SoL_CreateLight);
                    RemovePawnAbility(TorannMagicDefOf.TM_SoL_Equalize);
                    spell_EqualizeLight = false;
                    //this.spell_CreateLight = false;
                }
            }
        }

        private void ResolveEarthSpriteAction()
        {
            MagicPowerSkill magicPowerSkill = MagicData.MagicPowerSkill_EarthSprites.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EarthSprites_pwr");
            //Log.Message("resolving sprites");
            if (earthSpriteMap == null)
            {
                earthSpriteMap = Pawn.Map;
            }
            if (earthSpriteType == 1) //mining stone
            {
                //Log.Message("stone");
                Building mineTarget = earthSprites.GetFirstBuilding(earthSpriteMap);
                nextEarthSpriteAction = Find.TickManager.TicksGame + Mathf.RoundToInt((300 * (1 - (.1f * magicPowerSkill.level))) / arcaneDmg);
                TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.SparkFlash, earthSprites.ToVector3Shifted(), earthSpriteMap, Rand.Range(2f, 5f), .05f, 0f, .1f, 0, 0f, 0f, 0f);
                var mineable = mineTarget as Mineable;
                int num = 80;
                if (mineable != null && mineTarget.HitPoints > num)
                {
                    var dinfo = new DamageInfo(DamageDefOf.Mining, num, 0, -1f, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                    mineTarget.TakeDamage(dinfo);
                    
                    if (Rand.Chance(ModOptions.Settings.Instance.magicyteChance * 2))
                    {
                        Thing thing = null;
                        thing = ThingMaker.MakeThing(TorannMagicDefOf.RawMagicyte);
                        thing.stackCount = Rand.Range(8, 16);
                        if (thing != null)
                        {
                            GenPlace.TryPlaceThing(thing, earthSprites, earthSpriteMap, ThingPlaceMode.Near, null);
                        }
                    }
                }
                else if (mineable != null && mineTarget.HitPoints <= num)
                {
                    mineable.DestroyMined(Pawn);
                }

                if (mineable.DestroyedOrNull())
                {
                    IntVec3 oldEarthSpriteLoc = earthSprites;
                    Building newMineSpot = null;
                    if (earthSpritesInArea)
                    {
                        //Log.Message("moving in area");
                        List<IntVec3> spriteAreaCells = GenRadial.RadialCellsAround(oldEarthSpriteLoc, 6f, false).ToList();
                        spriteAreaCells.Shuffle();
                        for (int i = 0; i < spriteAreaCells.Count; i++)
                        {
                            IntVec3 intVec = spriteAreaCells[i];
                            newMineSpot = intVec.GetFirstBuilding(earthSpriteMap);
                            if (newMineSpot != null && !intVec.Fogged(earthSpriteMap) && TM_Calc.GetSpriteArea() != null && TM_Calc.GetSpriteArea().ActiveCells.Contains(intVec))
                            {
                                mineable = newMineSpot as Mineable;
                                if (mineable != null)
                                {
                                    earthSprites = intVec;
                                    //Log.Message("assigning");
                                    break;
                                }
                                newMineSpot = null;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            IntVec3 intVec = earthSprites + GenAdj.AdjacentCells.RandomElement();
                            newMineSpot = intVec.GetFirstBuilding(earthSpriteMap);
                            if (newMineSpot != null)
                            {
                                mineable = newMineSpot as Mineable;
                                if (mineable != null)
                                {
                                    earthSprites = intVec;
                                    i = 20;
                                }
                                newMineSpot = null;
                            }
                        }
                    }

                    if (oldEarthSpriteLoc == earthSprites)
                    {
                        earthSpriteType = 0;
                        earthSprites = IntVec3.Invalid;
                        earthSpritesInArea = false;
                    }
                }
            }
            else if (earthSpriteType == 2) //transforming soil
            {
                //Log.Message("earth");
                nextEarthSpriteAction = Find.TickManager.TicksGame + Mathf.RoundToInt((24000 * (1 - (.1f * magicPowerSkill.level))) / arcaneDmg);
                for (int m = 0; m < 4; m++)
                {
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ThickDust, earthSprites.ToVector3Shifted(), earthSpriteMap, Rand.Range(.3f, .5f), Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), Rand.Range(-20, 20), Rand.Range(.5f, 1f), Rand.Range(0, 360), Rand.Range(0, 360));
                }
                Map map = earthSpriteMap;
                IntVec3 curCell = earthSprites;
                TerrainDef terrain = curCell.GetTerrain(map);
                if (Rand.Chance(.8f))
                {
                    Thing thing = null;
                    thing = ThingMaker.MakeThing(TorannMagicDefOf.RawMagicyte);
                    thing.stackCount = Rand.Range(10, 20);
                    if (thing != null)
                    {
                        GenPlace.TryPlaceThing(thing, earthSprites, earthSpriteMap, ThingPlaceMode.Near, null);
                    }
                }
                if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid && terrain != null)
                {
                    if (terrain.defName == "MarshyTerrain" || terrain.defName == "Mud" || terrain.defName == "Marsh")
                    {
                        map.terrainGrid.SetTerrain(curCell, terrain.driesTo);
                    }
                    else if (terrain.defName == "WaterShallow")
                    {
                        map.terrainGrid.SetTerrain(curCell, TerrainDef.Named("Marsh"));
                    }
                    else if (terrain.defName == "Ice")
                    {
                        map.terrainGrid.SetTerrain(curCell, TerrainDef.Named("Mud"));
                    }
                    else if (terrain.defName == "Soil")
                    {
                        map.terrainGrid.SetTerrain(curCell, TerrainDef.Named("SoilRich"));
                    }
                    else if (terrain.defName == "Sand" || terrain.defName == "Gravel" || terrain.defName == "MossyTerrain")
                    {
                        map.terrainGrid.SetTerrain(curCell, TerrainDef.Named("Soil"));
                    }
                    else if (terrain.defName == "SoftSand")
                    {
                        map.terrainGrid.SetTerrain(curCell, TerrainDef.Named("Sand"));
                    }
                    else
                    {
                        Log.Message("unable to resolve terraindef - resetting earth sprite parameters");
                        earthSprites = IntVec3.Invalid;
                        earthSpriteMap = null;
                        earthSpriteType = 0;
                        earthSpritesInArea = false;
                    }

                    terrain = curCell.GetTerrain(map);
                    if (terrain.defName == "SoilRich")
                    {
                        //look for new spot to transform
                        IntVec3 oldEarthSpriteLoc = earthSprites;
                        if (earthSpritesInArea)
                        {
                            //Log.Message("moving in area");
                            List<IntVec3> spriteAreaCells = GenRadial.RadialCellsAround(oldEarthSpriteLoc, 6f, false).ToList();
                            spriteAreaCells.Shuffle();
                            for (int i = 0; i < spriteAreaCells.Count; i++)
                            {
                                IntVec3 intVec = spriteAreaCells[i];
                                terrain = intVec.GetTerrain(map);
                                if (terrain.defName == "MarshyTerrain" || terrain.defName == "Mud" || terrain.defName == "Marsh" || terrain.defName == "WaterShallow" || terrain.defName == "Ice" ||
                            terrain.defName == "Sand" || terrain.defName == "Gravel" || terrain.defName == "Soil" || terrain.defName == "MossyTerrain" || terrain.defName == "SoftSand")
                                {
                                    Building terrainHasBuilding = null;
                                    terrainHasBuilding = intVec.GetFirstBuilding(earthSpriteMap);
                                    if (TM_Calc.GetSpriteArea() != null && TM_Calc.GetSpriteArea().ActiveCells.Contains(intVec)) //dont transform terrain underneath buildings
                                    {
                                        //Log.Message("assigning");
                                        earthSprites = intVec;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                IntVec3 intVec = earthSprites + GenAdj.AdjacentCells.RandomElement();
                                terrain = intVec.GetTerrain(map);
                                if (terrain.defName == "MarshyTerrain" || terrain.defName == "Mud" || terrain.defName == "Marsh" || terrain.defName == "WaterShallow" || terrain.defName == "Ice" ||
                            terrain.defName == "Sand" || terrain.defName == "Gravel" || terrain.defName == "Soil" || terrain.defName == "MossyTerrain" || terrain.defName == "SoftSand")
                                {
                                    Building terrainHasBuilding = null;
                                    terrainHasBuilding = intVec.GetFirstBuilding(earthSpriteMap);
                                    if (terrainHasBuilding == null) //dont transform terrain underneath buildings
                                    {
                                        earthSprites = intVec;
                                        i = 20;
                                    }
                                }
                            }
                        }

                        if (oldEarthSpriteLoc == earthSprites)
                        {
                            earthSpriteType = 0;
                            earthSpriteMap = null;
                            earthSprites = IntVec3.Invalid;
                            earthSpritesInArea = false;
                            //Log.Message("ending");
                        }
                    }
                }
            }
        }

        public void ResolveEffecter()
        {
            if (PowerModifier <= 0) return;

            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                if (powerEffecter != null)
                {
                    powerEffecter = EffecterDefOf.ProgressBar.Spawn();
                    powerEffecter.EffectTick(Pawn, TargetInfo.Invalid);
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)powerEffecter.children[0]).mote;
                    if (mote == null) return;

                    mote.progress = Mathf.Clamp01((float)powerModifier / maxPower);
                    mote.offsetZ = +0.85f;                    
                }
            }
        }

        public void ResolveUndead()
        {
            if (supportedUndead != null)
            {
                List<Thing> tmpList = new List<Thing>();
                tmpList.Clear();
                for(int i =0; i < supportedUndead.Count; i++)
                {
                    Pawn p = supportedUndead[i] as Pawn;
                    if(p.DestroyedOrNull() || p.Dead)
                    {
                        tmpList.Add(p);
                    }
                }
                for(int i = 0; i < tmpList.Count; i++)
                {
                    supportedUndead.Remove(tmpList[i]);
                }
                if (supportedUndead.Count > 0 && dismissUndeadSpell == false)
                {
                    AddPawnAbility(TorannMagicDefOf.TM_DismissUndead);
                    dismissUndeadSpell = true;
                }
                if (supportedUndead.Count <= 0 && dismissUndeadSpell)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_DismissUndead);
                    dismissUndeadSpell = false;
                }
            }
            else
            {
                supportedUndead = new List<Thing>();
            }
        }

        public void ResolveSuccubusLovin()
        {
            if (Pawn.CurrentBed() != null && Pawn.ageTracker.AgeBiologicalYears > 17 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_VitalityBoostHD"), false))
            {
                Pawn pawnInMyBed = TM_Calc.FindNearbyOtherPawn(Pawn, 1);
                if (pawnInMyBed != null)
                {
                    if (pawnInMyBed.CurrentBed() != null && pawnInMyBed.RaceProps.Humanlike && pawnInMyBed.ageTracker.AgeBiologicalYears > 17)
                    {
                        Job job = new Job(JobDefOf.Lovin, pawnInMyBed, Pawn.CurrentBed());
                        Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        HealthUtility.AdjustSeverity(pawnInMyBed, HediffDef.Named("TM_VitalityDrainHD"), 8);
                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_VitalityBoostHD"), 6);
                    }
                }
            }
        }

        public void ResolveWarlockEmpathy()
        {
            //strange bug observed where other pawns will get the old offset of the previous pawn's offset unless other pawn has no empathy existing
            //in other words, empathy base mood effect seems to carry over from last otherpawn instead of using current otherpawn values
            if (Rand.Chance(Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false) - 1))
            {
                Pawn otherPawn = TM_Calc.FindNearbyOtherPawn(Pawn, 5);
                if (otherPawn != null && otherPawn.RaceProps.Humanlike && otherPawn.IsColonist)
                {
                    if (Rand.Chance(otherPawn.GetStatValue(StatDefOf.PsychicSensitivity, false) - .3f))
                    {
                        ThoughtHandler pawnThoughtHandler = Pawn.needs.mood.thoughts;
                        List<Thought> pawnThoughts = new List<Thought>();
                        pawnThoughtHandler.GetAllMoodThoughts(pawnThoughts);
                        List<Thought> otherThoughts = new List<Thought>();
                        otherPawn.needs.mood.thoughts.GetAllMoodThoughts(otherThoughts);
                        List<Thought_Memory> memoryThoughts = new List<Thought_Memory>();
                        memoryThoughts.Clear();
                        float oldMemoryOffset = 0;
                        if (Rand.Chance(.3f)) //empathy absorbed by warlock
                        {
                            ThoughtDef empathyThought = ThoughtDef.Named("WarlockEmpathy");
                            memoryThoughts = Pawn.needs.mood.thoughts.memories.Memories;
                            for (int i = 0; i < memoryThoughts.Count; i++)
                            {
                                if (memoryThoughts[i].def.defName == "WarlockEmpathy")
                                {
                                    oldMemoryOffset = memoryThoughts[i].MoodOffset();
                                    if (oldMemoryOffset > 30)
                                    {
                                        oldMemoryOffset = 30;
                                    }
                                    else if (oldMemoryOffset < -30)
                                    {
                                        oldMemoryOffset = -30;
                                    }
                                    Pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(memoryThoughts[i].def);
                                }
                            }
                            Thought transferThought = otherThoughts.RandomElement();
                            float newOffset = Mathf.RoundToInt(transferThought.CurStage.baseMoodEffect / 2);
                            empathyThought.stages.FirstOrDefault().baseMoodEffect = newOffset + oldMemoryOffset;

                            Pawn.needs.mood.thoughts.memories.TryGainMemory(empathyThought, null);
                            Vector3 drawPosOffset = Pawn.DrawPos;
                            drawPosOffset.z += .3f;
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ArcaneCircle, drawPosOffset, Pawn.Map, newOffset / 20, .2f, .1f, .1f, Rand.Range(100, 200), 0, 0, Rand.Range(0, 360));
                        }
                        else //empathy bleeding to other pawn
                        {
                            ThoughtDef empathyThought = ThoughtDef.Named("PsychicEmpathy");
                            memoryThoughts = otherPawn.needs.mood.thoughts.memories.Memories;
                            for (int i = 0; i < memoryThoughts.Count; i++)
                            {
                                if (memoryThoughts[i].def.defName == "PsychicEmpathy")
                                {
                                    oldMemoryOffset = memoryThoughts[i].CurStage.baseMoodEffect;
                                    if (oldMemoryOffset > 30)
                                    {
                                        oldMemoryOffset = 30;
                                    }
                                    else if (oldMemoryOffset < -30)
                                    {
                                        oldMemoryOffset = -30;
                                    }
                                    otherPawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(memoryThoughts[i].def);
                                }
                            }
                            Thought transferThought = pawnThoughts.RandomElement();
                            float newOffset = Mathf.RoundToInt(transferThought.CurStage.baseMoodEffect / 2);
                            empathyThought.stages.FirstOrDefault().baseMoodEffect = newOffset + oldMemoryOffset;

                            otherPawn.needs.mood.thoughts.memories.TryGainMemory(empathyThought, null);
                            Vector3 drawPosOffset = otherPawn.DrawPos;
                            drawPosOffset.z += .3f;
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ArcaneCircle, drawPosOffset, otherPawn.Map, newOffset / 20, .2f, .1f, .1f, Rand.Range(100, 200), 0, 0, Rand.Range(0, 360));
                        }
                    }
                }
            }
        }

        public void ResolveTechnomancerOverdrive()
        {
            if (overdriveBuilding != null)
            {
                List<Pawn> odPawns = ModOptions.Constants.GetOverdrivePawnList();

                if (!odPawns.Contains(Pawn))
                {
                    odPawns.Add(Pawn);
                    ModOptions.Constants.SetOverdrivePawnList(odPawns);
                }
                Vector3 rndPos = overdriveBuilding.DrawPos;
                rndPos.x += Rand.Range(-.4f, .4f);
                rndPos.z += Rand.Range(-.4f, .4f);
                TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.SparkFlash, rndPos, overdriveBuilding.Map, Rand.Range(.6f, .8f), .1f, .05f, .05f, 0, 0, 0, Rand.Range(0, 360));
                FleckMaker.ThrowSmoke(rndPos, overdriveBuilding.Map, Rand.Range(.8f, 1.2f));
                rndPos = overdriveBuilding.DrawPos;
                rndPos.x += Rand.Range(-.4f, .4f);
                rndPos.z += Rand.Range(-.4f, .4f);
                TM_MoteMaker.ThrowGenericFleck(TorannMagicDefOf.ElectricalSpark, rndPos, overdriveBuilding.Map, Rand.Range(.4f, .7f), .2f, .05f, .1f, 0, 0, 0, Rand.Range(0, 360));
                SoundInfo info = SoundInfo.InMap(new TargetInfo(overdriveBuilding.Position, overdriveBuilding.Map, false), MaintenanceType.None);
                info.pitchFactor = .4f;
                info.volumeFactor = .3f;
                SoundDefOf.TurretAcquireTarget.PlayOneShot(info);
                MagicPowerSkill damageControl = MagicData.MagicPowerSkill_Overdrive.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Overdrive_ver");
                if (Rand.Chance(.6f - (.06f * damageControl.level)))
                {
                    TM_Action.DamageEntities(overdriveBuilding, null, Rand.Range(3f, (7f - (1f * damageControl.level))), DamageDefOf.Burn, overdriveBuilding);
                }
                overdriveFrequency = 100 + (10 * damageControl.level);
                if (Rand.Chance(.4f))
                {
                    overdriveFrequency /= 2;
                }
                overdriveDuration--;
                if (overdriveDuration <= 0)
                {
                    if (odPawns != null && odPawns.Contains(Pawn))
                    {
                        ModOptions.Constants.ClearOverdrivePawns();
                        odPawns.Remove(Pawn);
                        ModOptions.Constants.SetOverdrivePawnList(odPawns);
                    }
                    overdrivePowerOutput = 0;
                    overdriveBuilding = null;
                }
            }
        }

        public void ResolveChronomancerTimeMark()
        {
            //Log.Message("pawn " + this.Pawn.LabelShort + " recallset: " + this.recallSet + " expiration: " + this.recallExpiration + " / " + Find.TickManager.TicksGame + " recallSpell: " + this.recallSpell + " position: " + this.recallPosition);
            if(customClass != null && MagicData.MagicPowersC.FirstOrDefault((MagicPower x ) => x.abilityDef == TorannMagicDefOf.TM_Recall).learned && !MagicData.MagicPowersStandalone.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TimeMark).learned)
            {
                MagicData.MagicPowersStandalone.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TimeMark).learned = true;
                RemovePawnAbility(TorannMagicDefOf.TM_TimeMark);
                AddPawnAbility(TorannMagicDefOf.TM_TimeMark);
            }
            if (recallExpiration <= Find.TickManager.TicksGame)
            {
                recallSet = false;
            }
            if (recallSet && !recallSpell)
            {
                AddPawnAbility(TorannMagicDefOf.TM_Recall);
                recallSpell = true;
            }
            if (recallSpell && (!recallSet || recallPosition == default(IntVec3)))
            {
                recallSpell = false;
                RemovePawnAbility(TorannMagicDefOf.TM_Recall);
            }
        }

        public void ResolveSustainers()
        {
            if(BrandPawns != null && BrandPawns.Count > 0)
            {
                if(!dispelBrandings)
                {
                    AddPawnAbility(TorannMagicDefOf.TM_DispelBranding);
                    dispelBrandings = true;
                }
                List<Pawn> tmpBrands = new List<Pawn>();
                tmpBrands.Clear();
                for(int i = 0; i < BrandPawns.Count; i++)
                {
                    Pawn p = BrandPawns[i];
                    if(p != null && (p.Destroyed || p.Dead))
                    {
                        BrandPawns.Remove(BrandPawns[i]);
                        BrandDefs.Remove(BrandDefs[i]);
                        break;
                    }
                }
                if(sigilSurging && Mana.CurLevel <= .01f)
                {
                    sigilSurging = false;
                }
            }
            else if(dispelBrandings)
            {
                dispelBrandings = false;
                RemovePawnAbility(TorannMagicDefOf.TM_DispelBranding);
            }
            if (livingWall != null)
            {
                if (!dispelLivingWall)
                {
                    dispelLivingWall = true;
                    RemovePawnAbility(TorannMagicDefOf.TM_DispelLivingWall);
                    AddPawnAbility(TorannMagicDefOf.TM_DispelLivingWall);
                }
            }
            else if(dispelLivingWall)
            {
                dispelLivingWall = false;
                RemovePawnAbility(TorannMagicDefOf.TM_DispelLivingWall);
            }

            if (stoneskinPawns.Count() > 0)
            {
                if (!dispelStoneskin)
                {
                    dispelStoneskin = true;
                    AddPawnAbility(TorannMagicDefOf.TM_DispelStoneskin);
                }
                for (int i = 0; i < stoneskinPawns.Count(); i++)
                {
                    if (stoneskinPawns[i].DestroyedOrNull() || stoneskinPawns[i].Dead)
                    {
                        stoneskinPawns.Remove(stoneskinPawns[i]);
                    }
                    else
                    {
                        if (!stoneskinPawns[i].health.hediffSet.HasHediff(HediffDef.Named("TM_StoneskinHD"), false))
                        {
                            stoneskinPawns.Remove(stoneskinPawns[i]);
                        }
                    }
                }
            }
            else if (dispelStoneskin)
            {
                dispelStoneskin = false;
                RemovePawnAbility(TorannMagicDefOf.TM_DispelStoneskin);
            }

            if(bondedSpirit != null && !dismissGuardianSpirit)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissGuardianSpirit);
                dismissGuardianSpirit = true;
            }
            if (bondedSpirit == null && dismissGuardianSpirit)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissGuardianSpirit);
                dismissGuardianSpirit = false;
            }

            if (summonedLights.Count > 0 && dismissSunlightSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissSunlight);
                dismissSunlightSpell = true;
            }

            if (summonedLights.Count <= 0 && dismissSunlightSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissSunlight);
                dismissSunlightSpell = false;
            }

            if (summonedPowerNodes.Count > 0 && dismissPowerNodeSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissPowerNode);
                dismissPowerNodeSpell = true;
            }

            if (summonedPowerNodes.Count <= 0 && dismissPowerNodeSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissPowerNode);
                dismissPowerNodeSpell = false;
            }

            if (summonedCoolers.Count > 0 && dismissCoolerSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissCooler);
                dismissCoolerSpell = true;
            }

            if (summonedCoolers.Count <= 0 && dismissCoolerSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissCooler);
                dismissCoolerSpell = false;
            }

            if (summonedHeaters.Count > 0 && dismissHeaterSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissHeater);
                dismissHeaterSpell = true;
            }

            if (summonedHeaters.Count <= 0 && dismissHeaterSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissHeater);
                dismissHeaterSpell = false;
            }

            if (enchanterStones.Count > 0 && dismissEnchanterStones == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissEnchanterStones);
                dismissEnchanterStones = true;
            }
            if (enchanterStones.Count <= 0 && dismissEnchanterStones)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissEnchanterStones);
                dismissEnchanterStones = false;
            }

            if (lightningTraps.Count > 0 && dismissLightningTrap == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissLightningTrap);
                dismissLightningTrap = true;
            }
            if (lightningTraps.Count <= 0 && dismissLightningTrap)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissLightningTrap);
                dismissLightningTrap = false;
            }

            if (summonedSentinels.Count > 0 && shatterSentinel == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_ShatterSentinel);
                shatterSentinel = true;
            }
            if (summonedSentinels.Count <= 0 && shatterSentinel)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_ShatterSentinel);
                shatterSentinel = false;
            }

            if (soulBondPawn.DestroyedOrNull() && (spell_ShadowStep || spell_ShadowCall))
            {
                soulBondPawn = null;
                spell_ShadowCall = false;
                spell_ShadowStep = false;
                RemovePawnAbility(TorannMagicDefOf.TM_ShadowCall);
                RemovePawnAbility(TorannMagicDefOf.TM_ShadowStep);
            }
            if (soulBondPawn != null)
            {
                if (spell_ShadowStep == false)
                {
                    spell_ShadowStep = true;
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowStep);
                    AddPawnAbility(TorannMagicDefOf.TM_ShadowStep);
                }
                if (spell_ShadowCall == false)
                {
                    spell_ShadowCall = true;
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowCall);
                    AddPawnAbility(TorannMagicDefOf.TM_ShadowCall);
                }
            }

            if (weaponEnchants != null && weaponEnchants.Count > 0)
            {
                for (int i = 0; i < weaponEnchants.Count; i++)
                {
                    Pawn ewPawn = weaponEnchants[i];
                    if (ewPawn.DestroyedOrNull() || ewPawn.Dead)
                    {
                        weaponEnchants.Remove(ewPawn);
                    }
                }

                if (dispelEnchantWeapon == false)
                {
                    dispelEnchantWeapon = true;
                    AddPawnAbility(TorannMagicDefOf.TM_DispelEnchantWeapon);
                }
            }
            else if (dispelEnchantWeapon)
            {
                dispelEnchantWeapon = false;
                RemovePawnAbility(TorannMagicDefOf.TM_DispelEnchantWeapon);
            }

            if (mageLightActive)
            {
                if (Pawn.Map == null && mageLightSet)
                {
                    mageLightActive = false;
                    mageLightThing = null;
                    mageLightSet = false;
                }
                Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_MageLightHD);
                if (hediff == null && !mageLightSet)
                {
                    HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MageLightHD, .5f);
                }
                if (mageLightSet && mageLightThing == null)
                {
                    mageLightActive = false;
                }
            }
            else
            {
                Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_MageLightHD);
                if (hediff != null)
                {
                    Pawn.health.RemoveHediff(hediff);
                }
                if (!mageLightThing.DestroyedOrNull())
                {
                    mageLightThing.Destroy(DestroyMode.Vanish);
                    mageLightThing = null;
                }
                mageLightSet = false;
            }            
        }

        public void ResolveMinions()
        {
            if (summonedMinions.Count > 0 && dismissMinionSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissMinion);
                dismissMinionSpell = true;
            }

            if (summonedMinions.Count <= 0 && dismissMinionSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissMinion);
                dismissMinionSpell = false;
            }

            if (summonedMinions.Count > 0)
            {
                for (int i = 0; i < summonedMinions.Count(); i++)
                {
                    Pawn minion = summonedMinions[i] as Pawn;
                    if (minion != null)
                    {
                        if (minion.DestroyedOrNull() || minion.Dead)
                        {
                            summonedMinions.Remove(summonedMinions[i]);
                            i--;
                        }
                    }
                    else
                    {
                        summonedMinions.Remove(summonedMinions[i]);
                        i--;
                    }
                }
            }

            if (earthSpriteType != 0 && dismissEarthSpriteSpell == false)
            {
                AddPawnAbility(TorannMagicDefOf.TM_DismissEarthSprites);
                dismissEarthSpriteSpell = true;
            }

            if (earthSpriteType == 0 && dismissEarthSpriteSpell)
            {
                RemovePawnAbility(TorannMagicDefOf.TM_DismissEarthSprites);
                dismissEarthSpriteSpell = false;
            }
        }

        public void ResolveMana()
        {
            bool flag = Mana == null;
            if (flag)
            {                
                Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_MagicUserHD, false);
                bool flag2 = firstHediffOfDef != null;
                if (flag2)
                {
                    firstHediffOfDef.Severity = 1f;
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(TorannMagicDefOf.TM_MagicUserHD, Pawn, null);
                    hediff.Severity = 1f;
                    Pawn.health.AddHediff(hediff, null, null);
                }
                Pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }
        }
        public void ResolveMagicPowers()
        {
            bool flag = magicPowersInitialized;
            if (!flag)
            {
                magicPowersInitialized = true;
            }
        }
        public void ResolveMagicTab()
        {
            if (!Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                InspectTabBase inspectTabsx = Pawn.GetInspectTabs().FirstOrDefault((InspectTabBase x) => x.labelKey == "TM_TabMagic");
                IEnumerable<InspectTabBase> inspectTabs = Pawn.GetInspectTabs();
                bool flag = inspectTabs != null && inspectTabs.Count<InspectTabBase>() > 0;
                if (flag)
                {
                    if (inspectTabsx == null)
                    {
                        try
                        {
                            Pawn.def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Magic)));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Concat(new object[]
                            {
                            "Could not instantiate inspector tab of type ",
                            typeof(ITab_Pawn_Magic),
                            ": ",
                            ex
                            }));
                        }
                    }
                }
            }
        }

        public void ResolveClassSkills()
        {
            bool flagCM = Pawn.story.traits.HasTrait(TorannMagicDefOf.ChaosMage);
            bool isCustom = customClass != null;

            if(isCustom && customClass.classHediff != null && !Pawn.health.hediffSet.HasHediff(customClass.classHediff))
            {
                HealthUtility.AdjustSeverity(Pawn, customClass.classHediff, customClass.hediffSeverity);                
            }

            if(Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_CursedTD) && !Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_CursedHD))
            {
                HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_CursedHD, .1f);
            }

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.BloodMage) || (isCustom && (customClass.classMageAbilities.Contains(TorannMagicDefOf.TM_BloodGift) || customClass.classHediff == TorannMagicDefOf.TM_BloodHD)))
            {
                if (!Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_BloodHD")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_BloodHD"), .1f);
                    for (int i = 0; i < 4; i++)
                    {
                        TM_MoteMaker.ThrowBloodSquirt(Pawn.DrawPos, Pawn.Map, Rand.Range(.5f, .8f));
                    }
                }
            }

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Chronomancer) || flagCM || (isCustom && customClass.classMageAbilities.Contains(TorannMagicDefOf.TM_Prediction)))
            {
                if (predictionIncidentDef != null && (predictionTick + 30) < Find.TickManager.TicksGame)
                {
                    predictionIncidentDef = null;
                    Find.Storyteller.incidentQueue.Clear();
                    //Log.Message("prediction failed to execute, clearing prediction");
                }
            }


            if(HexedPawns != null && HexedPawns.Count <= 0 && previousHexedPawns > 0)
            {
                if (HexedPawns.Count > 0)
                {
                    previousHexedPawns = HexedPawns.Count;
                }
                else if (previousHexedPawns > 0)
                {
                    //remove abilities
                    previousHexedPawns = 0;
                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                }
            }

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Enchanter) || flagCM || isCustom)
            {
                if (MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedBody).learned && (spell_EnchantedAura == false || !MagicData.MagicPowersStandalone.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedAura).learned))
                {
                    spell_EnchantedAura = true;
                    MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedAura).learned = true;
                    InitializeSpell();
                }

                if (MagicData.MagicPowerSkill_Shapeshift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Shapeshift_ver").level >= 3 && (spell_ShapeshiftDW != true || !MagicData.MagicPowersStandalone.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShapeshiftDW).learned))
                {
                    spell_ShapeshiftDW = true;
                    MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShapeshiftDW).learned = true;
                    InitializeSpell();
                }
            }

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Technomancer) || flagCM || isCustom)
            {
                if (HasTechnoBit)
                {
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_TechnoBitHD))
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_TechnoBitHD, .5f);
                        Vector3 bitDrawPos = Pawn.DrawPos;
                        bitDrawPos.x -= .5f;
                        bitDrawPos.z += .45f;
                        for (int i = 0; i < 4; i++)
                        {
                            FleckMaker.ThrowSmoke(bitDrawPos, Pawn.Map, Rand.Range(.6f, .8f));
                        }
                    }
                }
                if (HasTechnoWeapon && Pawn.equipment != null && Pawn.equipment.Primary != null)
                {
                    if (Pawn.equipment.Primary.def.defName.Contains("TM_TechnoWeapon_Base") && Pawn.equipment.Primary.def.Verbs != null && Pawn.equipment.Primary.def.Verbs.FirstOrDefault().range < 2)
                    {
                        TM_Action.DoAction_TechnoWeaponCopy(Pawn, technoWeaponThing, technoWeaponThingDef, technoWeaponQC);
                    }

                    if (!Pawn.equipment.Primary.def.defName.Contains("TM_TechnoWeapon_Base") && (technoWeaponThing != null || technoWeaponThingDef != null))
                    {
                        technoWeaponThing = null;
                        technoWeaponThingDef = null;
                    }
                }
            }

            if (MagicUserLevel >= 20 && (spell_Teach == false || !MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TeachMagic).learned))
            {
                AddPawnAbility(TorannMagicDefOf.TM_TeachMagic);
                MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TeachMagic).learned = true;
                spell_Teach = true;
            }

            if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Geomancer) || flagCM || isCustom) && earthSpriteType != 0 && earthSprites.IsValid)
            {
                if (nextEarthSpriteAction < Find.TickManager.TicksGame)
                {
                    ResolveEarthSpriteAction();
                }

                if (nextEarthSpriteMote < Find.TickManager.TicksGame)
                {
                    nextEarthSpriteMote += Rand.Range(7, 12);
                    Vector3 shiftLoc = earthSprites.ToVector3Shifted();
                    shiftLoc.x += Rand.Range(-.3f, .3f);
                    shiftLoc.z += Rand.Range(-.3f, .3f);
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Twinkle, shiftLoc, Pawn.Map, Rand.Range(.6f, 1.4f), .15f, Rand.Range(.2f, .5f), Rand.Range(.2f, .5f), Rand.Range(-100, 100), Rand.Range(0f, .3f), Rand.Range(0, 360), 0);
                    if(Rand.Chance(.3f))
                    {
                        shiftLoc = earthSprites.ToVector3Shifted();
                        shiftLoc.x += Rand.Range(-.3f, .3f);
                        shiftLoc.z += Rand.Range(-.3f, .3f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_GreenTwinkle, shiftLoc, Pawn.Map, Rand.Range(.6f, 1f), .15f, Rand.Range(.2f, .9f), Rand.Range(.5f, .9f), Rand.Range(-200, 200), Rand.Range(0f, .3f), Rand.Range(0, 360), 0);
                    }
                }
            }

            if (summonedSentinels.Count > 0)
            {
                for (int i = 0; i < summonedSentinels.Count(); i++)
                {
                    if (summonedSentinels[i].DestroyedOrNull())
                    {
                        summonedSentinels.Remove(summonedSentinels[i]);
                    }
                }
            }

            if (lightningTraps.Count > 0)
            {
                for (int i = 0; i < lightningTraps.Count(); i++)
                {
                    if (lightningTraps[i].DestroyedOrNull())
                    {
                        lightningTraps.Remove(lightningTraps[i]);
                    }
                }
            }

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Lich))
            {
                if (!Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_LichHD")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_LichHD"), .5f);
                }
                if (spell_Flight != true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathBolt);
                    AddPawnAbility(TorannMagicDefOf.TM_DeathBolt);
                    spell_Flight = true;
                    InitializeSpell();
                }
            }

            if (IsMagicUser && !Pawn.Dead && !Pawn.Downed)
            {
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Bard))
                {
                    MagicPowerSkill bardtraining_pwr = Pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_BardTraining.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BardTraining_pwr");

                    List<Trait> traits = Pawn.story.traits.allTraits;
                    for (int i = 0; i < traits.Count; i++)
                    {
                        if (traits[i].def.defName == "TM_Bard")
                        {
                            if (traits[i].Degree != bardtraining_pwr.level)
                            {
                                traits.Remove(traits[i]);
                                Pawn.story.traits.GainTrait(new Trait(TorannMagicDefOf.TM_Bard, bardtraining_pwr.level, false));
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 2);
                            }
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Succubus) || Pawn.story.traits.HasTrait(TorannMagicDefOf.Warlock))
                {
                    if (soulBondPawn != null)
                    {
                        if (!soulBondPawn.Spawned)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_SummonDemon);
                            spell_SummonDemon = false;
                        }
                        else if (soulBondPawn.health.hediffSet.HasHediff(HediffDef.Named("TM_DemonicPriceHD"), false))
                        {
                            if (spell_SummonDemon)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_SummonDemon);
                                spell_SummonDemon = false;
                            }
                        }
                        else if (soulBondPawn.health.hediffSet.HasHediff(HediffDef.Named("TM_SoulBondMentalHD")) && soulBondPawn.health.hediffSet.HasHediff(HediffDef.Named("TM_SoulBondPhysicalHD")))
                        {
                            if (spell_SummonDemon == false)
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SummonDemon);
                                spell_SummonDemon = true;
                            }
                        }
                        else
                        {
                            if (spell_SummonDemon)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_SummonDemon);
                                spell_SummonDemon = false;
                            }
                        }
                    }
                    else if (spell_SummonDemon)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SummonDemon);
                        spell_SummonDemon = false;
                    }
                }
            }

            if (IsMagicUser && !Pawn.Dead & !Pawn.Downed && (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Bard) || (isCustom && customClass.classMageAbilities.Contains(TorannMagicDefOf.TM_Inspire))))
            {
                if (!Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_InspirationalHD")) && MagicData.MagicPowersB.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Inspire).learned)
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_InspirationalHD"), 0.95f);
                }
            }
        }

        public void ResolveEnchantments()
        {
            float _maxMP = 0;
            float _maxMPUpkeep = 0;
            float _mpRegenRate = 0;
            float _mpRegenRateUpkeep = 0;
            float _coolDown = 0;
            float _xpGain = 0;
            float _mpCost = 0;
            float _arcaneRes = 0;
            float _arcaneDmg = 0;
            bool _arcaneSpectre = false;
            bool _phantomShift = false;
            float _arcalleumCooldown = 0f;

            //Determine trait adjustments
            IEnumerable<DefModExtension_TraitEnchantments> traitEnum = Pawn.story.traits.allTraits
                .Select((Trait t) => t.def.GetModExtension<DefModExtension_TraitEnchantments>());
            foreach (DefModExtension_TraitEnchantments e in traitEnum)
            {
                if (e != null)
                {
                    _maxMP += e.maxMP;
                    _mpCost += e.mpCost;
                    _mpRegenRate += e.mpRegenRate;
                    _coolDown += e.magicCooldown;
                    _xpGain += e.xpGain;
                    _arcaneRes += e.arcaneRes;
                    _arcaneDmg += e.arcaneDmg;
                }
            }

            //Determine hediff adjustments
            foreach(Hediff hd in Pawn.health.hediffSet.hediffs)
            {
                if(hd.def.GetModExtension<DefModExtension_HediffEnchantments>() != null)
                {                    
                    foreach(HediffEnchantment hdStage in hd.def.GetModExtension<DefModExtension_HediffEnchantments>().stages)
                    {
                        if(hd.Severity >= hdStage.minSeverity && hd.Severity < hdStage.maxSeverity)
                        {
                            DefModExtension_TraitEnchantments e = hdStage.enchantments;
                            if (e != null)
                            {
                                _maxMP += e.maxMP;
                                _mpCost += e.mpCost;
                                _mpRegenRate += e.mpRegenRate;
                                _coolDown += e.magicCooldown;
                                _xpGain += e.xpGain;
                                _arcaneRes += e.arcaneRes;
                                _arcaneDmg += e.arcaneDmg;
                            }
                            break;
                        }
                    }
                }
            }

            List<Apparel> apparel = Pawn.apparel.WornApparel;
            if (apparel != null)
            {
                for (int i = 0; i < Pawn.apparel.WornApparelCount; i++)
                {
                    Enchantment.CompEnchantedItem item = apparel[i].GetComp<Enchantment.CompEnchantedItem>();
                    if (item != null)
                    {
                        if (item.HasEnchantment)
                        {
                            float enchantmentFactor = 1f;
                            if (item.MadeFromEnchantedStuff)
                            {
                                enchantmentFactor = item.EnchantedStuff.enchantmentBonusMultiplier;

                                float arcalleumFactor = item.EnchantedStuff.arcalleumCooldownPerMass;
                                float apparelWeight = apparel[i].def.GetStatValueAbstract(StatDefOf.Mass, apparel[i].Stuff);
                                if (apparel[i].Stuff.defName == "TM_Arcalleum")
                                {
                                    _arcaneRes += .05f;
                                }
                                _arcalleumCooldown += (apparelWeight * (arcalleumFactor / 100));

                            }
                            _maxMP += item.maxMP * enchantmentFactor;
                            _mpRegenRate += item.mpRegenRate * enchantmentFactor;
                            _coolDown += item.coolDown * enchantmentFactor;
                            _xpGain += item.xpGain * enchantmentFactor;
                            _mpCost += item.mpCost * enchantmentFactor;
                            _arcaneRes += item.arcaneRes * enchantmentFactor;
                            _arcaneDmg += item.arcaneDmg * enchantmentFactor;

                            if (item.arcaneSpectre)
                            {
                                _arcaneSpectre = true;
                            }
                            if (item.phantomShift)
                            {
                                _phantomShift = true;
                            }
                        }
                    }
                }
            }
            if (Pawn.equipment != null && Pawn.equipment.Primary != null)
            {
                Enchantment.CompEnchantedItem item = Pawn.equipment.Primary.GetComp<Enchantment.CompEnchantedItem>();
                if (item != null)
                {
                    if (item.HasEnchantment)
                    {
                        float enchantmentFactor = 1f;
                        if (item.MadeFromEnchantedStuff)
                        {
                            Enchantment.CompProperties_EnchantedStuff compES = Pawn.equipment.Primary.Stuff.GetCompProperties<Enchantment.CompProperties_EnchantedStuff>();
                            enchantmentFactor = compES.enchantmentBonusMultiplier;

                            float arcalleumFactor = compES.arcalleumCooldownPerMass;
                            if (Pawn.equipment.Primary.Stuff.defName == "TM_Arcalleum")
                            {
                                _arcaneDmg += .1f;
                            }
                            _arcalleumCooldown += (Pawn.equipment.Primary.def.GetStatValueAbstract(StatDefOf.Mass, Pawn.equipment.Primary.Stuff) * (arcalleumFactor / 100f));

                        }
                        else
                        {
                            _maxMP += item.maxMP * enchantmentFactor;
                            _mpRegenRate += item.mpRegenRate * enchantmentFactor;
                            _coolDown += item.coolDown * enchantmentFactor;
                            _xpGain += item.xpGain * enchantmentFactor;
                            _mpCost += item.mpCost * enchantmentFactor;
                            _arcaneRes += item.arcaneRes * enchantmentFactor;
                            _arcaneDmg += item.arcaneDmg * enchantmentFactor;
                        }
                    }
                }
                if (Pawn.equipment.Primary.def.defName == "TM_DefenderStaff")
                {
                    if (item_StaffOfDefender == false)
                    {
                        AddPawnAbility(TorannMagicDefOf.TM_ArcaneBarrier);
                        item_StaffOfDefender = true;
                    }
                }
                else
                {
                    if (item_StaffOfDefender)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ArcaneBarrier);
                        item_StaffOfDefender = false;
                    }
                }
            }
            CleanupSummonedStructures();

            //Determine active or sustained hediffs and abilities
            if(SoL != null)
            {
                _maxMPUpkeep += (TorannMagicDefOf.TM_SpiritOfLight.upkeepEnergyCost * (1 - (TorannMagicDefOf.TM_SpiritOfLight.upkeepEfficiencyPercent * MagicData.MagicPowerSkill_SpiritOfLight.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SpiritOfLight_eff").level)));
                _mpRegenRateUpkeep += (TorannMagicDefOf.TM_SpiritOfLight.upkeepRegenCost * (1 - (TorannMagicDefOf.TM_SpiritOfLight.upkeepEfficiencyPercent * MagicData.MagicPowerSkill_SpiritOfLight.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_SpiritOfLight_eff").level)));
            }
            if (summonedLights.Count > 0)
            {
                _maxMPUpkeep += (summonedLights.Count * TorannMagicDefOf.TM_Sunlight.upkeepEnergyCost);
                _mpRegenRateUpkeep += (summonedLights.Count * TorannMagicDefOf.TM_Sunlight.upkeepRegenCost);
            }
            if (summonedHeaters.Count > 0)
            {
                _maxMPUpkeep += (summonedHeaters.Count * TorannMagicDefOf.TM_Heater.upkeepEnergyCost);
            }
            if (summonedCoolers.Count > 0)
            {
                _maxMPUpkeep += (summonedCoolers.Count * TorannMagicDefOf.TM_Cooler.upkeepEnergyCost);
            }
            if (summonedPowerNodes.Count > 0)
            {
                _maxMPUpkeep += (summonedPowerNodes.Count * TorannMagicDefOf.TM_PowerNode.upkeepEnergyCost);
                _mpRegenRateUpkeep += (summonedPowerNodes.Count * TorannMagicDefOf.TM_PowerNode.upkeepRegenCost);
            }
            if (weaponEnchants.Count > 0)
            {
                _maxMPUpkeep += (weaponEnchants.Count * ActualManaCost(TorannMagicDefOf.TM_EnchantWeapon));
            }
            if (stoneskinPawns != null && stoneskinPawns.Count > 0)
            {
                _maxMPUpkeep += (stoneskinPawns.Count * (TorannMagicDefOf.TM_Stoneskin.upkeepEnergyCost - (.02f * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Stoneskin).level)));
            }
            if (summonedSentinels != null && summonedSentinels.Count > 0)
            {
                MagicPowerSkill heartofstone = MagicData.MagicPowerSkill_Sentinel.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Sentinel_eff");

                if (heartofstone.level == 3)
                {
                    _maxMPUpkeep += (.15f * summonedSentinels.Count);
                }
                else
                {
                    _maxMPUpkeep += ((.2f - (.02f * heartofstone.level)) * summonedSentinels.Count);
                }
            }
            if(BrandPawns != null && BrandPawns.Count > 0)
            {
                float brandCost = BrandPawns.Count * (TorannMagicDefOf.TM_Branding.upkeepRegenCost * (1f - (TorannMagicDefOf.TM_Branding.upkeepEfficiencyPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Branding).level)));
                if(sigilSurging)
                {
                    brandCost *= (5f * (1f - (.1f * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_SigilSurge).level)));
                }
                if(sigilDraining)
                {
                    brandCost *= (1.5f * (1f - (.2f * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_SigilDrain).level)));
                }
                _mpRegenRateUpkeep += brandCost; 
            }
            if(livingWall != null && livingWall.Spawned)
            {
                _maxMPUpkeep += (TorannMagicDefOf.TM_LivingWall.upkeepEnergyCost * (1f - (TorannMagicDefOf.TM_LivingWall.upkeepEfficiencyPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_LivingWall).level)));
            }
            //Bonded spirit animal
            if (bondedSpirit != null)
            {
                _maxMPUpkeep += (TorannMagicDefOf.TM_GuardianSpirit.upkeepEnergyCost * (1f - (TorannMagicDefOf.TM_GuardianSpirit.upkeepEfficiencyPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_GuardianSpirit).level)));
                _mpRegenRateUpkeep += (TorannMagicDefOf.TM_GuardianSpirit.upkeepRegenCost * (1f - (TorannMagicDefOf.TM_GuardianSpirit.upkeepEfficiencyPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_GuardianSpirit).level)));
                if (bondedSpirit.Dead || bondedSpirit.Destroyed)
                {
                    bondedSpirit = null;
                }
                else if (bondedSpirit.Faction != null && bondedSpirit.Faction != Pawn.Faction)
                {
                    bondedSpirit = null;
                }
                else if (!bondedSpirit.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SpiritBondHD))
                {
                    HealthUtility.AdjustSeverity(bondedSpirit, TorannMagicDefOf.TM_SpiritBondHD, .5f);
                }
                if(TorannMagicDefOf.TM_SpiritCrowR == GuardianSpiritType)
                {
                    Hediff hd = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_CrowInsightHD);
                    if(hd != null && hd.Severity != (.5f + MagicData.GetSkill_Power(TorannMagicDefOf.TM_GuardianSpirit).level))
                    {
                        Pawn.health.RemoveHediff(hd);
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_CrowInsightHD, .5f + MagicData.GetSkill_Power(TorannMagicDefOf.TM_GuardianSpirit).level);
                    }
                    else
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_CrowInsightHD, .5f + MagicData.GetSkill_Power(TorannMagicDefOf.TM_GuardianSpirit).level);
                    }
                }
            }
            if (enchanterStones != null && enchanterStones.Count > 0)
            {
                for (int i = 0; i < enchanterStones.Count; i++)
                {
                    if (enchanterStones[i].DestroyedOrNull())
                    {
                        enchanterStones.Remove(enchanterStones[i]);
                    }
                }
                _maxMPUpkeep += (enchanterStones.Count * (TorannMagicDefOf.TM_EnchanterStone.upkeepEnergyCost * (1f - TorannMagicDefOf.TM_EnchanterStone.upkeepEfficiencyPercent * MagicData.MagicPowerSkill_EnchanterStone.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EnchanterStone_eff").level)));
            }
            try
            {
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Druid) && fertileLands.Count > 0)
                {
                    _mpRegenRateUpkeep += TorannMagicDefOf.TM_FertileLands.upkeepRegenCost;
                }
            }
            catch
            {

            }
            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Lich))
            {
                if (spell_LichForm || (customClass != null && MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_LichForm).learned))
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_LichForm);
                    MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_LichForm).learned = false;
                    spell_LichForm = false;
                }
                _maxMP += .5f;
                _mpRegenRate += .5f;
            }
            if (Pawn.Inspired && Pawn.Inspiration.def == TorannMagicDefOf.ID_ManaRegen)
            {
                _mpRegenRate += 1f;
            }
            if (recallSet)
            {
                _maxMPUpkeep += TorannMagicDefOf.TM_Recall.upkeepEnergyCost * (1 - (TorannMagicDefOf.TM_Recall.upkeepEfficiencyPercent * MagicData.MagicPowerSkill_Recall.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Recall_eff").level));
                _mpRegenRateUpkeep += TorannMagicDefOf.TM_Recall.upkeepRegenCost * (1 - (TorannMagicDefOf.TM_Recall.upkeepEfficiencyPercent * MagicData.MagicPowerSkill_Recall.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Recall_eff").level));
            }
            using (IEnumerator<Hediff> enumerator = Pawn.health.hediffSet.hediffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Hediff rec = enumerator.Current;
                    TMAbilityDef ability = MagicData.GetHediffAbility(rec);
                    if (ability != null)
                    {
                        MagicPowerSkill skill = MagicData.GetSkill_Efficiency(ability);
                        int level = 0;
                        if (skill != null)
                        {
                            level = skill.level;
                        }
                        if (ability == TorannMagicDefOf.TM_EnchantedAura || ability == TorannMagicDefOf.TM_EnchantedBody)
                        {
                            level = MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_EnchantedBody).level;
                        }

                        _maxMPUpkeep += (ability.upkeepEnergyCost * (1f - (ability.upkeepEfficiencyPercent * level)));

                        if (ability == TorannMagicDefOf.TM_EnchantedAura || ability == TorannMagicDefOf.TM_EnchantedBody)
                        {
                            level = MagicData.GetSkill_Versatility(TorannMagicDefOf.TM_EnchantedBody).level;
                        }
                        _mpRegenRateUpkeep += (ability.upkeepRegenCost * (1f - (ability.upkeepEfficiencyPercent * level)));
                    }
                    //else
                    //{
                    //    if (this.Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_EntertainingHD"), false))
                    //    {
                    //        _maxMPUpkeep += .3f;
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_PredictionHD, false))
                    //    {
                    //        _mpRegenRateUpkeep += .5f * (1 - (.10f * this.MagicData.MagicPowerSkill_Prediction.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Prediction_eff").level));
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Shadow_AuraHD, false))
                    //    {
                    //        _maxMPUpkeep += .4f * (1 - (.08f * this.MagicData.MagicPowerSkill_Shadow.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Shadow_eff").level));
                    //        _mpRegenRateUpkeep += .3f * (1 - (.08f * this.MagicData.MagicPowerSkill_Shadow.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Shadow_eff").level));
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_RayOfHope_AuraHD, false))
                    //    {
                    //        _maxMPUpkeep += .4f * (1 - (.08f * this.MagicData.MagicPowerSkill_RayofHope.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_RayofHope_eff").level));
                    //        _mpRegenRateUpkeep += .3f * (1 - (.08f * this.MagicData.MagicPowerSkill_RayofHope.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_RayofHope_eff").level));
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SoothingBreeze_AuraHD, false))
                    //    {
                    //        _maxMPUpkeep += .4f * (1 - (.08f * this.MagicData.MagicPowerSkill_Soothe.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Soothe_eff").level));
                    //        _mpRegenRateUpkeep += .3f * (1 - (.08f * this.MagicData.MagicPowerSkill_Soothe.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Soothe_eff").level));
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_EnchantedAuraHD) || this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_EnchantedBodyHD))
                    //    {
                    //        _maxMPUpkeep += .2f + (1f - (.04f * this.MagicData.MagicPowerSkill_EnchantedBody.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EnchantedBody_eff").level));
                    //        _mpRegenRateUpkeep += .4f + (1f - (.04f * this.MagicData.MagicPowerSkill_EnchantedBody.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_EnchantedBody_ver").level));
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_BlurHD))
                    //    {
                    //        _maxMPUpkeep += .2f;
                    //    }
                    //    if (this.Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MageLightHD))
                    //    {
                    //        _maxMPUpkeep += .1f;
                    //        _mpRegenRateUpkeep += .05f;
                    //    }
                    //}
                }
            }

            if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SS_SerumHD))
            {
                Hediff def = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_SS_SerumHD, false);
                _mpRegenRate -= (float)(.15f * def.CurStageIndex);
                _maxMP -= .25f;
                _arcaneRes += (float)(.15f * def.CurStageIndex);
                _arcaneDmg -= (float)(.1f * def.CurStageIndex);
            }

            //class and global modifiers
            _arcaneDmg += (.01f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_pwr").level);
            _arcaneRes += (.02f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_pwr").level);
            _mpCost -= (.01f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_eff").level);
            _xpGain += (.02f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_eff").level);
            _coolDown -= (.01f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_ver").level);
            _mpRegenRate += (.01f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_ver").level);
            _maxMP += (.02f * MagicData.MagicPowerSkill_WandererCraft.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_WandererCraft_ver").level);

            _maxMP += (.04f * MagicData.MagicPowerSkill_global_spirit.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_spirit_pwr").level);
            _mpRegenRate += (.05f * MagicData.MagicPowerSkill_global_regen.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_regen_pwr").level);
            _mpCost -= (.025f * MagicData.MagicPowerSkill_global_eff.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_global_eff_pwr").level);
            _arcaneRes += ((1f - Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false)) / 2f);
            _arcaneDmg += ((Pawn.GetStatValue(StatDefOf.PsychicSensitivity, false) - 1f) / 4f);

            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_BoundlessTD))
            {
                arcalleumCooldown = Mathf.Clamp(0f + _arcalleumCooldown, 0f, .1f);
            }
            else
            {
                arcalleumCooldown = Mathf.Clamp(0f + _arcalleumCooldown, 0f, .5f);
            }

            float val = (1f - (.03f * MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_eff").level));
            _maxMPUpkeep *= val;
            _mpRegenRateUpkeep *= val;

            //resolve upkeep costs
            _maxMP -= (_maxMPUpkeep);
            _mpRegenRate -= (_mpRegenRateUpkeep);

            //finalize
            maxMP = Mathf.Clamp(1f + _maxMP, 0f, 5f);
            mpRegenRate = 1f + _mpRegenRate;
            coolDown = Mathf.Clamp(1f + _coolDown, 0.25f, 10f);
            xpGain = Mathf.Clamp(1f + _xpGain, 0.01f, 5f);
            mpCost = Mathf.Clamp(1f + _mpCost, 0.1f, 5f);
            arcaneRes = Mathf.Clamp(1 + _arcaneRes, 0.01f, 5f);
            arcaneDmg = 1 + _arcaneDmg;

            if (IsMagicUser && !TM_Calc.IsCrossClass(Pawn, true))
            {
                if (maxMP != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_maxEnergy")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_maxEnergy"), .5f);
                }
                if (mpRegenRate != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_energyRegen")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_energyRegen"), .5f);
                }
                if (coolDown != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_coolDown")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_coolDown"), .5f);
                }
                if (xpGain != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_xpGain")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_xpGain"), .5f);
                }
                if (mpCost != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_energyCost")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_energyCost"), .5f);
                }
                if (arcaneRes != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_dmgResistance")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_dmgResistance"), .5f);
                }
                if (arcaneDmg != 1f && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_dmgBonus")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_dmgBonus"), .5f);
                }
                if(_arcalleumCooldown != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcalleumCooldown")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_arcalleumCooldown"), .5f);
                }
                if (_arcaneSpectre && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_arcaneSpectre"), .5f);
                }
                else if(_arcaneSpectre == false && Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")))
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")));
                }
                if (_phantomShift)
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_phantomShift"), .5f);
                }
                else if (_phantomShift == false && Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_phantomShift")))
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_HediffEnchantment_phantomShift")));
                }               
            }
        }

        private void CleanupSummonedStructures()
        {
            for (int i = 0; i < summonedLights.Count; i++)
            {
                if (summonedLights[i].DestroyedOrNull())
                {
                    summonedLights.Remove(summonedLights[i]);
                    i--;
                }
            }
            for (int i = 0; i < summonedHeaters.Count; i++)
            {
                if (summonedHeaters[i].DestroyedOrNull())
                {
                    summonedHeaters.Remove(summonedHeaters[i]);
                    i--;
                }
            }
            for (int i = 0; i < summonedCoolers.Count; i++)
            {
                if (summonedCoolers[i].DestroyedOrNull())
                {
                    summonedCoolers.Remove(summonedCoolers[i]);
                    i--;
                }
            }
            for (int i = 0; i < summonedPowerNodes.Count; i++)
            {
                if (summonedPowerNodes[i].DestroyedOrNull())
                {
                    summonedPowerNodes.Remove(summonedPowerNodes[i]);
                    i--;
                }
            }
            for (int i = 0; i < lightningTraps.Count; i++)
            {
                if (lightningTraps[i].DestroyedOrNull())
                {
                    lightningTraps.Remove(lightningTraps[i]);
                    i--;
                }
            }
        }

        public override void PostExposeData()
        {
            //base.PostExposeData();            
            Scribe_Values.Look<bool>(ref magicPowersInitialized, "magicPowersInitialized", false, false);
            Scribe_Values.Look<bool>(ref magicPowersInitializedForColonist, "magicPowersInitializedForColonist", true, false);
            Scribe_Values.Look<bool>(ref colonistPowerCheck, "colonistPowerCheck", true, false);
            Scribe_Values.Look<bool>(ref spell_Rain, "spell_Rain", false, false);
            Scribe_Values.Look<bool>(ref spell_Blink, "spell_Blink", false, false);
            Scribe_Values.Look<bool>(ref spell_Teleport, "spell_Teleport", false, false);
            Scribe_Values.Look<bool>(ref spell_Heal, "spell_Heal", false, false);
            Scribe_Values.Look<bool>(ref spell_Heater, "spell_Heater", false, false);
            Scribe_Values.Look<bool>(ref spell_Cooler, "spell_Cooler", false, false);
            Scribe_Values.Look<bool>(ref spell_PowerNode, "spell_PowerNode", false, false);
            Scribe_Values.Look<bool>(ref spell_Sunlight, "spell_Sunlight", false, false);
            Scribe_Values.Look<bool>(ref spell_DryGround, "spell_DryGround", false, false);
            Scribe_Values.Look<bool>(ref spell_WetGround, "spell_WetGround", false, false);
            Scribe_Values.Look<bool>(ref spell_ChargeBattery, "spell_ChargeBattery", false, false);
            Scribe_Values.Look<bool>(ref spell_SmokeCloud, "spell_SmokeCloud", false, false);
            Scribe_Values.Look<bool>(ref spell_Extinguish, "spell_Extinguish", false, false);
            Scribe_Values.Look<bool>(ref spell_EMP, "spell_EMP", false, false);
            Scribe_Values.Look<bool>(ref spell_Blizzard, "spell_Blizzard", false, false);
            Scribe_Values.Look<bool>(ref spell_Firestorm, "spell_Firestorm", false, false);
            Scribe_Values.Look<bool>(ref spell_EyeOfTheStorm, "spell_EyeOfTheStorm", false, false);
            Scribe_Values.Look<bool>(ref spell_SummonMinion, "spell_SummonMinion", false, false);
            Scribe_Values.Look<bool>(ref spell_TransferMana, "spell_TransferMana", false, false);
            Scribe_Values.Look<bool>(ref spell_SiphonMana, "spell_SiphonMana", false, false);
            Scribe_Values.Look<bool>(ref spell_RegrowLimb, "spell_RegrowLimb", false, false);
            Scribe_Values.Look<bool>(ref spell_ManaShield, "spell_ManaShield", false, false);
            Scribe_Values.Look<bool>(ref spell_FoldReality, "spell_FoldReality", false, false);
            Scribe_Values.Look<bool>(ref spell_Resurrection, "spell_Resurrection", false, false);
            Scribe_Values.Look<bool>(ref spell_HolyWrath, "spell_HolyWrath", false, false);
            Scribe_Values.Look<bool>(ref spell_LichForm, "spell_LichForm", false, false);
            Scribe_Values.Look<bool>(ref spell_Flight, "spell_Flight", false, false);
            Scribe_Values.Look<bool>(ref spell_SummonPoppi, "spell_SummonPoppi", false, false);
            Scribe_Values.Look<bool>(ref spell_BattleHymn, "spell_BattleHymn", false, false);
            Scribe_Values.Look<bool>(ref spell_FertileLands, "spell_FertileLands", false, false);
            Scribe_Values.Look<bool>(ref spell_CauterizeWound, "spell_CauterizeWound", false, false);
            Scribe_Values.Look<bool>(ref spell_SpellMending, "spell_SpellMending", false, false);
            Scribe_Values.Look<bool>(ref spell_PsychicShock, "spell_PsychicShock", false, false);
            Scribe_Values.Look<bool>(ref spell_Scorn, "spell_Scorn", false, false);
            Scribe_Values.Look<bool>(ref spell_Meteor, "spell_Meteor", false, false);
            Scribe_Values.Look<bool>(ref spell_Teach, "spell_Teach", false, false);
            Scribe_Values.Look<bool>(ref spell_OrbitalStrike, "spell_OrbitalStrike", false, false);
            Scribe_Values.Look<bool>(ref spell_BloodMoon, "spell_BloodMoon", false, false);
            Scribe_Values.Look<bool>(ref spell_Shapeshift, "spell_Shapeshift", false, false);
            Scribe_Values.Look<bool>(ref spell_ShapeshiftDW, "spell_ShapeshiftDW", false, false);
            Scribe_Values.Look<bool>(ref spell_Blur, "spell_Blur", false, false);
            Scribe_Values.Look<bool>(ref spell_BlankMind, "spell_BlankMind", false, false);
            Scribe_Values.Look<bool>(ref spell_DirtDevil, "spell_DirtDevil", false, false);
            Scribe_Values.Look<bool>(ref spell_ArcaneBolt, "spell_ArcaneBolt", false, false);
            Scribe_Values.Look<bool>(ref spell_LightningTrap, "spell_LightningTrap", false, false);
            Scribe_Values.Look<bool>(ref spell_Invisibility, "spell_Invisibility", false, false);
            Scribe_Values.Look<bool>(ref spell_BriarPatch, "spell_BriarPatch", false, false);
            Scribe_Values.Look<bool>(ref spell_MechaniteReprogramming, "spell_MechaniteReprogramming", false, false);
            Scribe_Values.Look<bool>(ref spell_Recall, "spell_Recall", false, false);
            Scribe_Values.Look<bool>(ref spell_MageLight, "spell_MageLight", false, false);
            Scribe_Values.Look<bool>(ref spell_SnapFreeze, "spell_SnapFreeze", false, false);
            Scribe_Values.Look<bool>(ref spell_Ignite, "spell_Ignite", false, false);
            Scribe_Values.Look<bool>(ref spell_HeatShield, "spell_HeatShield", false, false);
            Scribe_Values.Look<bool>(ref useTechnoBitToggle, "useTechnoBitToggle", true, false);
            Scribe_Values.Look<bool>(ref useTechnoBitRepairToggle, "useTechnoBitRepairToggle", true, false);
            Scribe_Values.Look<bool>(ref useElementalShotToggle, "useElementalShotToggle", true, false);
            Scribe_Values.Look<int>(ref powerModifier, "powerModifier", 0, false);
            Scribe_Values.Look<int>(ref technoWeaponDefNum, "technoWeaponDefNum");
            Scribe_Values.Look<bool>(ref doOnce, "doOnce", true, false);
            Scribe_Values.Look<int>(ref predictionTick, "predictionTick", 0, false);
            Scribe_Values.Look<int>(ref predictionHash, "predictionHash", 0, false);
            Scribe_References.Look<Thing>(ref mageLightThing, "mageLightThing", false);
            Scribe_Values.Look<bool>(ref mageLightActive, "mageLightActive", false, false);
            Scribe_Values.Look<bool>(ref mageLightSet, "mageLightSet", false, false);
            Scribe_Values.Look<bool>(ref deathRetaliating, "deathRetaliating", false, false);
            Scribe_Values.Look<bool>(ref canDeathRetaliate, "canDeathRetaliate", false, false);
            Scribe_Values.Look<int>(ref ticksTillRetaliation, "ticksTillRetaliation", 600, false);
            Scribe_Defs.Look<IncidentDef>(ref predictionIncidentDef, "predictionIncidentDef");
            Scribe_References.Look<Pawn>(ref soulBondPawn, "soulBondPawn", false);
            //Scribe_References.Look<Thing>(ref this.technoWeaponThing, "technoWeaponThing", false);
            Scribe_Defs.Look<ThingDef>(ref technoWeaponThingDef, "technoWeaponThingDef");
            Scribe_Values.Look<QualityCategory>(ref technoWeaponQC, "technoWeaponQC");
            Scribe_References.Look<Thing>(ref enchanterStone, "enchanterStone", false);
            Scribe_Collections.Look<Thing>(ref enchanterStones, "enchanterStones", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedMinions, "summonedMinions", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref supportedUndead, "supportedUndead", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedLights, "summonedLights", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedPowerNodes, "summonedPowerNodes", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedCoolers, "summonedCoolers", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedHeaters, "summonedHeaters", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref summonedSentinels, "summonedSentinels", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref stoneskinPawns, "stoneskinPawns", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref weaponEnchants, "weaponEnchants", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref lightningTraps, "lightningTraps", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref hexedPawns, "hexedPawns", LookMode.Reference);
            Scribe_Values.Look<IntVec3>(ref earthSprites, "earthSprites", default(IntVec3), false);
            Scribe_Values.Look<int>(ref earthSpriteType, "earthSpriteType", 0, false);
            Scribe_References.Look<Map>(ref earthSpriteMap, "earthSpriteMap", false);
            Scribe_Values.Look<bool>(ref earthSpritesInArea, "earthSpritesInArea", false, false);
            Scribe_Values.Look<int>(ref nextEarthSpriteAction, "nextEarthSpriteAction", 0, false);
            Scribe_Collections.Look<IntVec3>(ref fertileLands, "fertileLands", LookMode.Value);
            Scribe_Values.Look<float>(ref maxMP, "maxMP", 1f, false);
            Scribe_Values.Look<int>(ref lastChaosTraditionTick, "lastChaosTraditionTick", 0);
            //Scribe_Collections.Look<TM_ChaosPowers>(ref this.chaosPowers, "chaosPowers", LookMode.Deep, new object[0]);
            //Recall variables 
            Scribe_Values.Look<bool>(ref recallSet, "recallSet", false, false);
            Scribe_Values.Look<bool>(ref recallSpell, "recallSpell", false, false);
            Scribe_Values.Look<int>(ref recallExpiration, "recallExpiration", 0, false);
            Scribe_Values.Look<IntVec3>(ref recallPosition, "recallPosition", default(IntVec3), false);
            Scribe_References.Look<Map>(ref recallMap, "recallMap", false);
            Scribe_Collections.Look<string>(ref recallNeedDefnames, "recallNeedDefnames", LookMode.Value);
            Scribe_Collections.Look<float>(ref recallNeedValues, "recallNeedValues", LookMode.Value);
            Scribe_Collections.Look<Hediff>(ref recallHediffList, "recallHediffList", LookMode.Deep);
            Scribe_Collections.Look<float>(ref recallHediffDefSeverityList, "recallHediffSeverityList", LookMode.Value);
            Scribe_Collections.Look<int>(ref recallHediffDefTicksRemainingList, "recallHediffDefTicksRemainingList", LookMode.Value);
            Scribe_Collections.Look<Hediff_Injury>(ref recallInjuriesList, "recallInjuriesList", LookMode.Deep);
            Scribe_References.Look<FlyingObject_SpiritOfLight>(ref SoL, "SoL", false);
            Scribe_Defs.Look<ThingDef>(ref guardianSpiritType, "guardianSpiritType");
            Scribe_References.Look<Pawn>(ref bondedSpirit, "bondedSpirit", false);
            Scribe_Collections.Look<Pawn>(ref brands, "brands", LookMode.Reference);
            Scribe_Collections.Look<HediffDef>(ref brandDefs, "brandDefs", LookMode.Def);
            Scribe_Values.Look<bool>(ref sigilSurging, "sigilSurging", false, false);
            Scribe_Values.Look<bool>(ref sigilDraining, "sigilDraining", false, false);
            Scribe_References.Look<FlyingObject_LivingWall>(ref livingWall, "livingWall");
            Scribe_Deep.Look(ref magicWardrobe, "magicWardrobe");
            Scribe_Deep.Look<MagicData>(ref magicData, "magicData", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                TM_PawnTracker.ResolveMagicComp(this);
                Pawn abilityUser = Pawn;
                int index = TM_ClassUtility.CustomClassIndexOfBaseMageClass(abilityUser.story.traits.allTraits);
                if (index >= 0)
                {                   
                    customClass = TM_ClassUtility.CustomClasses[index];
                    customIndex = index;
                    LoadCustomClassAbilities(customClass);                    
                }                
                else
                {
                    bool flagCM = abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage);
                    bool flag40 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.InnerFire) || flagCM;
                    if (flag40)
                    {
                        bool flag14 = !MagicData.MagicPowersIF.NullOrEmpty<MagicPower>();
                        if (flag14)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current3 in MagicData.MagicPowersIF)
                            {
                                bool flag15 = current3.abilityDef != null;
                                if (flag15)
                                {
                                    if (current3.learned && (current3.abilityDef == TorannMagicDefOf.TM_RayofHope || current3.abilityDef == TorannMagicDefOf.TM_RayofHope_I || current3.abilityDef == TorannMagicDefOf.TM_RayofHope_II || current3.abilityDef == TorannMagicDefOf.TM_RayofHope_III))
                                    {
                                        if (current3.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_RayofHope);
                                        }
                                        else if (current3.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_RayofHope_I);
                                        }
                                        else if (current3.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_RayofHope_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_RayofHope_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag41 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost) || flagCM;
                    if (flag41)
                    {
                        bool flag17 = !MagicData.MagicPowersHoF.NullOrEmpty<MagicPower>();
                        if (flag17)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current4 in MagicData.MagicPowersHoF)
                            {
                                bool flag18 = current4.abilityDef != null;
                                if (flag18)
                                {
                                    if (current4.learned && (current4.abilityDef == TorannMagicDefOf.TM_Soothe || current4.abilityDef == TorannMagicDefOf.TM_Soothe_I || current4.abilityDef == TorannMagicDefOf.TM_Soothe_II || current4.abilityDef == TorannMagicDefOf.TM_Soothe_III))
                                    {
                                        if (current4.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Soothe);
                                        }
                                        else if (current4.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Soothe_I);
                                        }
                                        else if (current4.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Soothe_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Soothe_III);
                                        }
                                    }
                                    if (current4.learned && (current4.abilityDef == TorannMagicDefOf.TM_FrostRay || current4.abilityDef == TorannMagicDefOf.TM_FrostRay_I || current4.abilityDef == TorannMagicDefOf.TM_FrostRay_II || current4.abilityDef == TorannMagicDefOf.TM_FrostRay_III))
                                    {
                                        if (current4.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_FrostRay);
                                        }
                                        else if (current4.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_FrostRay_I);
                                        }
                                        else if (current4.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_FrostRay_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_FrostRay_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag42 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.StormBorn) || flagCM;
                    if (flag42)
                    {
                        bool flag20 = !MagicData.MagicPowersSB.NullOrEmpty<MagicPower>();
                        if (flag20)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current5 in MagicData.MagicPowersSB)
                            {
                                bool flag21 = current5.abilityDef != null;
                                if (current5.learned && (current5.abilityDef == TorannMagicDefOf.TM_AMP || current5.abilityDef == TorannMagicDefOf.TM_AMP_I || current5.abilityDef == TorannMagicDefOf.TM_AMP_II || current5.abilityDef == TorannMagicDefOf.TM_AMP_III))
                                {
                                    if (current5.level == 0)
                                    {
                                        AddPawnAbility(TorannMagicDefOf.TM_AMP);
                                    }
                                    else if (current5.level == 1)
                                    {
                                        AddPawnAbility(TorannMagicDefOf.TM_AMP_I);
                                    }
                                    else if (current5.level == 2)
                                    {
                                        AddPawnAbility(TorannMagicDefOf.TM_AMP_II);
                                    }
                                    else
                                    {
                                        AddPawnAbility(TorannMagicDefOf.TM_AMP_III);
                                    }
                                }
                            }
                        }
                    }
                    bool flag43 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Arcanist) || flagCM;
                    if (flag43)
                    {
                        bool flag23 = !MagicData.MagicPowersA.NullOrEmpty<MagicPower>();
                        if (flag23)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current6 in MagicData.MagicPowersA)
                            {
                                bool flag24 = current6.abilityDef != null;
                                if (flag24)
                                {
                                    if (current6.learned && (current6.abilityDef == TorannMagicDefOf.TM_Shadow || current6.abilityDef == TorannMagicDefOf.TM_Shadow_I || current6.abilityDef == TorannMagicDefOf.TM_Shadow_II || current6.abilityDef == TorannMagicDefOf.TM_Shadow_III))
                                    {
                                        if (current6.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shadow);
                                        }
                                        else if (current6.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shadow_I);
                                        }
                                        else if (current6.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shadow_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shadow_III);
                                        }
                                    }
                                    if (current6.learned && (current6.abilityDef == TorannMagicDefOf.TM_MagicMissile || current6.abilityDef == TorannMagicDefOf.TM_MagicMissile_I || current6.abilityDef == TorannMagicDefOf.TM_MagicMissile_II || current6.abilityDef == TorannMagicDefOf.TM_MagicMissile_III))
                                    {
                                        if (current6.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MagicMissile);
                                        }
                                        else if (current6.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MagicMissile_I);
                                        }
                                        else if (current6.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MagicMissile_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MagicMissile_III);
                                        }
                                    }
                                    if (current6.learned && (current6.abilityDef == TorannMagicDefOf.TM_Blink || current6.abilityDef == TorannMagicDefOf.TM_Blink_I || current6.abilityDef == TorannMagicDefOf.TM_Blink_II || current6.abilityDef == TorannMagicDefOf.TM_Blink_III))
                                    {
                                        if (current6.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Blink);
                                        }
                                        else if (current6.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Blink_I);
                                        }
                                        else if (current6.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Blink_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Blink_III);
                                        }
                                    }
                                    if (current6.learned && (current6.abilityDef == TorannMagicDefOf.TM_Summon || current6.abilityDef == TorannMagicDefOf.TM_Summon_I || current6.abilityDef == TorannMagicDefOf.TM_Summon_II || current6.abilityDef == TorannMagicDefOf.TM_Summon_III))
                                    {
                                        if (current6.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Summon);
                                        }
                                        else if (current6.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Summon_I);
                                        }
                                        else if (current6.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Summon_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Summon_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag44 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Paladin) || flagCM;
                    if (flag44)
                    {
                        bool flag26 = !MagicData.MagicPowersP.NullOrEmpty<MagicPower>();
                        if (flag26)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current7 in MagicData.MagicPowersP)
                            {
                                bool flag27 = current7.abilityDef != null;
                                if (flag27)
                                {
                                    if (current7.learned && (current7.abilityDef == TorannMagicDefOf.TM_Shield || current7.abilityDef == TorannMagicDefOf.TM_Shield_I || current7.abilityDef == TorannMagicDefOf.TM_Shield_II || current7.abilityDef == TorannMagicDefOf.TM_Shield_III))
                                    {
                                        if (current7.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shield);
                                        }
                                        else if (current7.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shield_I);
                                        }
                                        else if (current7.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shield_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Shield_III);
                                        }
                                    }
                                    if (current7.learned && (current7.abilityDef == TorannMagicDefOf.TM_P_RayofHope || current7.abilityDef == TorannMagicDefOf.TM_P_RayofHope_I || current7.abilityDef == TorannMagicDefOf.TM_P_RayofHope_II || current7.abilityDef == TorannMagicDefOf.TM_P_RayofHope_III))
                                    {
                                        if (current7.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope);
                                        }
                                        else if (current7.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope_I);
                                        }
                                        else if (current7.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag45 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Summoner) || flagCM;
                    if (flag45)
                    {
                        bool flag28 = !MagicData.MagicPowersS.NullOrEmpty<MagicPower>();
                        if (flag28)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current8 in MagicData.MagicPowersS)
                            {
                                bool flag29 = current8.abilityDef != null;
                                if (flag29)
                                {
                                    //if ((current7.abilityDef == TorannMagicDefOf.TM_Shield || current7.abilityDef == TorannMagicDefOf.TM_Shield_I || current7.abilityDef == TorannMagicDefOf.TM_Shield_II || current7.abilityDef == TorannMagicDefOf.TM_Shield_III))
                                    //{
                                    //    if (current7.level == 0)
                                    //    {
                                    //        base.AddPawnAbility(TorannMagicDefOf.TM_Shield);
                                    //    }
                                    //    else if (current7.level == 1)
                                    //    {
                                    //        base.AddPawnAbility(TorannMagicDefOf.TM_Shield_I);
                                    //    }
                                    //    else if (current7.level == 2)
                                    //    {
                                    //        base.AddPawnAbility(TorannMagicDefOf.TM_Shield_II);
                                    //    }
                                    //    else
                                    //    {
                                    //        base.AddPawnAbility(TorannMagicDefOf.TM_Shield_III);
                                    //    }
                                    //}
                                }
                            }
                        }
                    }
                    bool flag46 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Druid) || flagCM;
                    if (flag46)
                    {
                        bool flag30 = !MagicData.MagicPowersD.NullOrEmpty<MagicPower>();
                        if (flag30)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current9 in MagicData.MagicPowersD)
                            {
                                bool flag31 = current9.abilityDef != null;
                                if (flag31)
                                {
                                    if (current9.learned && (current9.abilityDef == TorannMagicDefOf.TM_SootheAnimal || current9.abilityDef == TorannMagicDefOf.TM_SootheAnimal_I || current9.abilityDef == TorannMagicDefOf.TM_SootheAnimal_II || current9.abilityDef == TorannMagicDefOf.TM_SootheAnimal_III))
                                    {
                                        if (current9.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal);
                                        }
                                        else if (current9.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal_I);
                                        }
                                        else if (current9.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag47 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Necromancer) || abilityUser.story.traits.HasTrait(TorannMagicDefOf.Lich) || flagCM;
                    if (flag47)
                    {
                        bool flag32 = !MagicData.MagicPowersN.NullOrEmpty<MagicPower>();
                        if (flag32)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current10 in MagicData.MagicPowersN)
                            {
                                bool flag33 = current10.abilityDef != null;
                                if (flag33)
                                {
                                    if (current10.learned && (current10.abilityDef == TorannMagicDefOf.TM_DeathMark || current10.abilityDef == TorannMagicDefOf.TM_DeathMark_I || current10.abilityDef == TorannMagicDefOf.TM_DeathMark_II || current10.abilityDef == TorannMagicDefOf.TM_DeathMark_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathMark);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathMark_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathMark_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathMark_III);
                                        }
                                    }
                                    if (current10.learned && (current10.abilityDef == TorannMagicDefOf.TM_ConsumeCorpse || current10.abilityDef == TorannMagicDefOf.TM_ConsumeCorpse_I || current10.abilityDef == TorannMagicDefOf.TM_ConsumeCorpse_II || current10.abilityDef == TorannMagicDefOf.TM_ConsumeCorpse_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_III);
                                        }
                                    }
                                    if (current10.learned && (current10.abilityDef == TorannMagicDefOf.TM_CorpseExplosion || current10.abilityDef == TorannMagicDefOf.TM_CorpseExplosion_I || current10.abilityDef == TorannMagicDefOf.TM_CorpseExplosion_II || current10.abilityDef == TorannMagicDefOf.TM_CorpseExplosion_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion_III);
                                        }
                                    }
                                    if (abilityUser.story.traits.HasTrait(TorannMagicDefOf.Lich) && (current10.learned && (current10.abilityDef == TorannMagicDefOf.TM_DeathBolt || current10.abilityDef == TorannMagicDefOf.TM_DeathBolt_I || current10.abilityDef == TorannMagicDefOf.TM_DeathBolt_II || current10.abilityDef == TorannMagicDefOf.TM_DeathBolt_III)))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathBolt);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathBolt_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathBolt_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DeathBolt_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag48 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Priest) || flagCM;
                    if (flag48)
                    {
                        bool flag34 = !MagicData.MagicPowersPR.NullOrEmpty<MagicPower>();
                        if (flag34)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current11 in MagicData.MagicPowersPR)
                            {
                                bool flag33 = current11.abilityDef != null;
                                if (flag33)
                                {
                                    if (current11.learned && (current11.abilityDef == TorannMagicDefOf.TM_HealingCircle || current11.abilityDef == TorannMagicDefOf.TM_HealingCircle_I || current11.abilityDef == TorannMagicDefOf.TM_HealingCircle_II || current11.abilityDef == TorannMagicDefOf.TM_HealingCircle_III))
                                    {
                                        if (current11.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HealingCircle);
                                        }
                                        else if (current11.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HealingCircle_I);
                                        }
                                        else if (current11.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HealingCircle_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HealingCircle_III);
                                        }
                                    }
                                    if (current11.learned && (current11.abilityDef == TorannMagicDefOf.TM_BestowMight || current11.abilityDef == TorannMagicDefOf.TM_BestowMight_I || current11.abilityDef == TorannMagicDefOf.TM_BestowMight_II || current11.abilityDef == TorannMagicDefOf.TM_BestowMight_III))
                                    {
                                        if (current11.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_BestowMight);
                                        }
                                        else if (current11.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_BestowMight_I);
                                        }
                                        else if (current11.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_BestowMight_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_BestowMight_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag49 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Bard) || flagCM;
                    if (flag49)
                    {
                        bool flag35 = !MagicData.MagicPowersB.NullOrEmpty<MagicPower>();
                        if (flag35)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current12 in MagicData.MagicPowersB)
                            {
                                bool flag36 = current12.abilityDef != null;
                                if (flag36)
                                {
                                    if (current12.learned && (current12.abilityDef == TorannMagicDefOf.TM_Lullaby || current12.abilityDef == TorannMagicDefOf.TM_Lullaby_I || current12.abilityDef == TorannMagicDefOf.TM_Lullaby_II || current12.abilityDef == TorannMagicDefOf.TM_Lullaby_III))
                                    {
                                        if (current12.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Lullaby);
                                        }
                                        else if (current12.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Lullaby_I);
                                        }
                                        else if (current12.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Lullaby_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Lullaby_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag50 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Succubus) || flagCM;
                    if (flag50)
                    {
                        bool flag37 = !MagicData.MagicPowersSD.NullOrEmpty<MagicPower>();
                        if (flag37)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current13 in MagicData.MagicPowersSD)
                            {
                                bool flag38 = current13.abilityDef != null;
                                if (flag38)
                                {
                                    if (current13.learned && (current13.abilityDef == TorannMagicDefOf.TM_ShadowBolt || current13.abilityDef == TorannMagicDefOf.TM_ShadowBolt_I || current13.abilityDef == TorannMagicDefOf.TM_ShadowBolt_II || current13.abilityDef == TorannMagicDefOf.TM_ShadowBolt_III))
                                    {
                                        if (current13.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                                        }
                                        else if (current13.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_I);
                                        }
                                        else if (current13.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_III);
                                        }
                                    }
                                    if (current13.learned && (current13.abilityDef == TorannMagicDefOf.TM_Attraction || current13.abilityDef == TorannMagicDefOf.TM_Attraction_I || current13.abilityDef == TorannMagicDefOf.TM_Attraction_II || current13.abilityDef == TorannMagicDefOf.TM_Attraction_III))
                                    {
                                        if (current13.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Attraction);
                                        }
                                        else if (current13.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Attraction_I);
                                        }
                                        else if (current13.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Attraction_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Attraction_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag51 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Warlock) || flagCM;
                    if (flag51)
                    {
                        bool flagWD1 = !MagicData.MagicPowersWD.NullOrEmpty<MagicPower>();
                        if (flagWD1)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current14 in MagicData.MagicPowersWD)
                            {
                                bool flagWD2 = current14.abilityDef != null;
                                if (flagWD2)
                                {
                                    if (current14.learned && (current14.abilityDef == TorannMagicDefOf.TM_ShadowBolt || current14.abilityDef == TorannMagicDefOf.TM_ShadowBolt_I || current14.abilityDef == TorannMagicDefOf.TM_ShadowBolt_II || current14.abilityDef == TorannMagicDefOf.TM_ShadowBolt_III))
                                    {
                                        if (current14.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                                        }
                                        else if (current14.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_I);
                                        }
                                        else if (current14.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt_III);
                                        }
                                    }
                                    if (current14.learned && (current14.abilityDef == TorannMagicDefOf.TM_Repulsion || current14.abilityDef == TorannMagicDefOf.TM_Repulsion_I || current14.abilityDef == TorannMagicDefOf.TM_Repulsion_II || current14.abilityDef == TorannMagicDefOf.TM_Repulsion_III))
                                    {
                                        if (current14.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Repulsion);
                                        }
                                        else if (current14.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Repulsion_I);
                                        }
                                        else if (current14.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Repulsion_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Repulsion_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag52 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Geomancer) || flagCM;
                    if (flag52)
                    {
                        bool flagG = !MagicData.MagicPowersG.NullOrEmpty<MagicPower>();
                        if (flagG)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current15 in MagicData.MagicPowersG)
                            {
                                bool flagWD2 = current15.abilityDef != null;
                                if (flagWD2)
                                {
                                    if (current15.learned && (current15.abilityDef == TorannMagicDefOf.TM_Encase || current15.abilityDef == TorannMagicDefOf.TM_Encase_I || current15.abilityDef == TorannMagicDefOf.TM_Encase_II || current15.abilityDef == TorannMagicDefOf.TM_Encase_III))
                                    {
                                        if (current15.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Encase);
                                        }
                                        else if (current15.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Encase_I);
                                        }
                                        else if (current15.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Encase_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Encase_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag53 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Technomancer) || flagCM;
                    if (flag53)
                    {
                        bool flagT = !MagicData.MagicPowersT.NullOrEmpty<MagicPower>();
                        if (flagT)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current16 in MagicData.MagicPowersT)
                            {
                                bool flagT2 = current16.abilityDef != null;
                                if (flagT2)
                                {

                                }
                            }
                        }
                    }
                    bool flag54 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.BloodMage);
                    if (flag54)
                    {
                        bool flagBM = !MagicData.MagicPowersBM.NullOrEmpty<MagicPower>();
                        if (flagBM)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current16 in MagicData.MagicPowersBM)
                            {
                                bool flagBM2 = current16.abilityDef != null;
                                if (flagBM2)
                                {
                                    if (current16.learned && (current16.abilityDef == TorannMagicDefOf.TM_Rend || current16.abilityDef == TorannMagicDefOf.TM_Rend_I || current16.abilityDef == TorannMagicDefOf.TM_Rend_II || current16.abilityDef == TorannMagicDefOf.TM_Rend_III))
                                    {
                                        if (current16.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Rend);
                                        }
                                        else if (current16.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Rend_I);
                                        }
                                        else if (current16.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Rend_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Rend_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag55 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Enchanter) || flagCM;
                    if (flag55)
                    {
                        bool flagE = !MagicData.MagicPowersE.NullOrEmpty<MagicPower>();
                        if (flagE)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current17 in MagicData.MagicPowersE)
                            {
                                bool flagE2 = current17.abilityDef != null;
                                if (flagE2)
                                {
                                    if (current17.learned && (current17.abilityDef == TorannMagicDefOf.TM_Polymorph || current17.abilityDef == TorannMagicDefOf.TM_Polymorph_I || current17.abilityDef == TorannMagicDefOf.TM_Polymorph_II || current17.abilityDef == TorannMagicDefOf.TM_Polymorph_III))
                                    {
                                        if (current17.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Polymorph);
                                        }
                                        else if (current17.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Polymorph_I);
                                        }
                                        else if (current17.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Polymorph_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Polymorph_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag56 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Chronomancer) || flagCM;
                    if (flag56)
                    {
                        bool flagC = !MagicData.MagicPowersC.NullOrEmpty<MagicPower>();
                        if (flagC)
                        {
                            //this.LoadPowers();
                            foreach (MagicPower current18 in MagicData.MagicPowersC)
                            {
                                bool flagC2 = current18.abilityDef != null;
                                if (flagC2)
                                {
                                    if (current18.learned && (current18.abilityDef == TorannMagicDefOf.TM_ChronostaticField || current18.abilityDef == TorannMagicDefOf.TM_ChronostaticField_I || current18.abilityDef == TorannMagicDefOf.TM_ChronostaticField_II || current18.abilityDef == TorannMagicDefOf.TM_ChronostaticField_III))
                                    {
                                        if (current18.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField);
                                        }
                                        else if (current18.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField_I);
                                        }
                                        else if (current18.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag40)
                    {
                        //Log.Message("Loading Inner Fire Abilities");
                        MagicPower mpIF = MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Firebolt);
                        if (mpIF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Firebolt);
                        }
                        mpIF = MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireclaw);
                        if (mpIF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Fireclaw);
                        }
                        mpIF = MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireball);
                        if (mpIF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Fireball);
                        }
                    }
                    if (flag41)
                    {
                        //Log.Message("Loading Heart of Frost Abilities");
                        MagicPower mpHoF = MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Icebolt);
                        if (mpHoF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Icebolt);
                        }
                        mpHoF = MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Snowball);
                        if (mpHoF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Snowball);
                        }
                        mpHoF = MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rainmaker);
                        if (mpHoF.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Rainmaker);
                        }

                    }
                    if (flag42)
                    {
                        //Log.Message("Loading Storm Born Abilities");
                        MagicPower mpSB = MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningBolt);
                        if (mpSB.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_LightningBolt);
                        }
                        mpSB = MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningCloud);
                        if (mpSB.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_LightningCloud);
                        }
                        mpSB = MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningStorm);
                        if (mpSB.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_LightningStorm);
                        }
                    }
                    if (flag43)
                    {
                        //Log.Message("Loading Arcane Abilities");
                        MagicPower mpA = MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Teleport);
                        if (mpA.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Teleport);
                        }
                    }
                    if (flag44)
                    {
                        //Log.Message("Loading Paladin Abilities");
                        MagicPower mpP = MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Heal);
                        if (mpP.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Heal);
                        }
                        mpP = MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ValiantCharge);
                        if (mpP.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_ValiantCharge);
                        }
                        mpP = MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overwhelm);
                        if (mpP.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Overwhelm);
                        }
                    }
                    if (flag45)
                    {
                        //Log.Message("Loading Summoner Abilities");
                        MagicPower mpS = MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonMinion);
                        if (mpS.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SummonMinion);
                        }
                        mpS = MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonPylon);
                        if (mpS.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SummonPylon);
                        }
                        mpS = MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonExplosive);
                        if (mpS.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SummonExplosive);
                        }
                        mpS = MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonElemental);
                        if (mpS.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SummonElemental);
                        }
                    }
                    if (flag46)
                    {
                        //Log.Message("Loading Druid Abilities");
                        MagicPower mpD = MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Poison);
                        if (mpD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Poison);
                        }
                        mpD = MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Regenerate);
                        if (mpD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Regenerate);
                        }
                        mpD = MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_CureDisease);
                        if (mpD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_CureDisease);
                        }
                    }
                    if (flag47)
                    {
                        //Log.Message("Loading Necromancer Abilities");
                        MagicPower mpN = MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RaiseUndead);
                        if (mpN.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_RaiseUndead);
                        }
                        mpN = MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_FogOfTorment);
                        if (mpN.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_FogOfTorment);
                        }
                    }
                    if (flag48)
                    {
                        //Log.Message("Loading Priest Abilities");
                        MagicPower mpPR = MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AdvancedHeal);
                        if (mpPR.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_AdvancedHeal);
                        }
                        mpPR = MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Purify);
                        if (mpPR.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Purify);
                        }
                    }
                    if (flag49)
                    {
                        //Log.Message("Loading Bard Abilities");
                        MagicPower mpB = MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BardTraining);
                        //if (mpB.learned == true)
                        //{
                        //    this.AddPawnAbility(TorannMagicDefOf.TM_BardTraining);
                        //}
                        mpB = MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Entertain);
                        if (mpB.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Entertain);
                        }
                        //mpB = this.MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Inspire);
                        //if (mpB.learned == true)
                        //{
                        //    this.AddPawnAbility(TorannMagicDefOf.TM_Inspire);
                        //}
                    }
                    if (flag50)
                    {
                        //Log.Message("Loading Succubus Abilities");
                        MagicPower mpSD = MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate);
                        if (mpSD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                        }
                        mpSD = MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond);
                        if (mpSD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                        }
                    }
                    if (flag51)
                    {
                        //Log.Message("Loading Warlock Abilities");
                        MagicPower mpWD = MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate);
                        if (mpWD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                        }
                        mpWD = MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond);
                        if (mpWD.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                        }
                    }
                    if (flag52)
                    {
                        //Log.Message("Loading Geomancer Abilities");
                        MagicPower mpG = MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Stoneskin);
                        if (mpG.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Stoneskin);
                        }
                        mpG = MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EarthSprites);
                        if (mpG.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_EarthSprites);
                        }
                        mpG = MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EarthernHammer);
                        if (mpG.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_EarthernHammer);
                        }
                        mpG = MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sentinel);
                        if (mpG.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Sentinel);
                        }
                    }
                    if (flag53)
                    {
                        //Log.Message("Loading Geomancer Abilities");
                        MagicPower mpT = MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoTurret);
                        if (mpT.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_TechnoTurret);
                        }
                        mpT = MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoWeapon);
                        if (mpT.learned)
                        {
                            //nano weapon applies only when equipping a new weapon
                            AddPawnAbility(TorannMagicDefOf.TM_TechnoWeapon);
                            AddPawnAbility(TorannMagicDefOf.TM_NanoStimulant);
                        }
                        mpT = MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoShield);
                        if (mpT.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_TechnoShield);
                        }
                        mpT = MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sabotage);
                        if (mpT.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Sabotage);
                        }
                        mpT = MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overdrive);
                        if (mpT.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Overdrive);
                        }
                    }
                    if (flag54)
                    {
                        //Log.Message("Loading BloodMage Abilities");
                        MagicPower mpBM = MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodGift);
                        if (mpBM.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_BloodGift);
                        }
                        mpBM = MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_IgniteBlood);
                        if (mpBM.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_IgniteBlood);
                        }
                        mpBM = MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodForBlood);
                        if (mpBM.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_BloodForBlood);
                        }
                        mpBM = MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodShield);
                        if (mpBM.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_BloodShield);
                        }
                    }
                    if (flag55)
                    {
                        //Log.Message("Loading Enchanter Abilities");
                        MagicPower mpE = MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedBody);
                        if (mpE.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_EnchantedBody);
                            spell_EnchantedAura = true;
                        }
                        mpE = MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Transmutate);
                        if (mpE.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Transmutate);
                        }
                        mpE = MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchanterStone);
                        if (mpE.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_EnchanterStone);
                        }
                        mpE = MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantWeapon);
                        if (mpE.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_EnchantWeapon);
                        }
                    }
                    if (flag56)
                    {
                        //Log.Message("Loading Chronomancer Abilities");
                        MagicPower mpC = MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Prediction);
                        if (mpC.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_Prediction);
                        }
                        mpC = MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AlterFate);
                        if (mpC.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_AlterFate);
                        }
                        mpC = MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AccelerateTime);
                        if (mpC.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_AccelerateTime);
                        }
                        mpC = MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ReverseTime);
                        if (mpC.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_ReverseTime);
                        }
                    }
                    if (flagCM)
                    {
                        //Log.Message("Loading Chaos Mage Abilities");
                        MagicPower mpCM = MagicData.MagicPowersCM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ChaosTradition);
                        if (mpCM.learned)
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_ChaosTradition);
                            chaosPowers = new List<TM_ChaosPowers>();
                            chaosPowers.Clear();
                            List<MagicPower> learnedList = new List<MagicPower>();
                            learnedList.Clear();
                            for (int i = 0; i < MagicData.AllMagicPowersForChaosMage.Count; i++)
                            {
                                MagicPower mp = MagicData.AllMagicPowersForChaosMage[i];
                                if (mp.learned)
                                {
                                    learnedList.Add(mp);
                                }
                            }
                            int count = learnedList.Count;
                            for (int i = 0; i < 5; i++)
                            {
                                if (i < count)
                                {
                                    chaosPowers.Add(new TM_ChaosPowers((TMAbilityDef)learnedList[i].GetAbilityDef(0), TM_ClassUtility.GetAssociatedMagicPowerSkill(this, learnedList[i])));
                                    foreach(MagicPower mp in learnedList)
                                    {
                                        for (int j = 0; j < mp.TMabilityDefs.Count; j++)
                                        {
                                            TMAbilityDef tmad = mp.TMabilityDefs[j] as TMAbilityDef;
                                            if(tmad.shouldInitialize)
                                            {
                                                int level = mp.level;
                                                if (mp.TMabilityDefs[level] == TorannMagicDefOf.TM_LightSkip)
                                                {
                                                    if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 1)
                                                    {
                                                        AddPawnAbility(TorannMagicDefOf.TM_LightSkipMass);
                                                    }
                                                    if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 2)
                                                    {
                                                        AddPawnAbility(TorannMagicDefOf.TM_LightSkipGlobal);
                                                    }
                                                }
                                                if (tmad == TorannMagicDefOf.TM_Hex && HexedPawns.Count > 0)
                                                {
                                                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                                                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                                                    RemovePawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                                                    AddPawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                                                    AddPawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                                                    AddPawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                                                }
                                                
                                                RemovePawnAbility(tmad);
                                                AddPawnAbility(tmad);
                                            }
                                            if(tmad.childAbilities != null && tmad.childAbilities.Count > 0)
                                            {
                                                foreach(TMAbilityDef ad in tmad.childAbilities)
                                                {
                                                    if(ad.shouldInitialize)
                                                    {
                                                        RemovePawnAbility(ad);
                                                        AddPawnAbility(ad);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    chaosPowers.Add(new TM_ChaosPowers((TMAbilityDef)TM_Calc.GetRandomMagicPower(this).abilityDef, null));
                                }
                            }
                        }
                    }
                }
                if(TM_Calc.HasAdvancedClass(Pawn))
                {
                    List<TM_CustomClass> ccList = TM_ClassUtility.GetAdvancedClassesForPawn(Pawn);
                    foreach(TM_CustomClass cc in ccList)
                    {
                        if(cc.isMage)
                        {
                            AdvancedClasses.Add(cc);
                            LoadCustomClassAbilities(cc);
                        }
                    }                    
                }
                UpdateAutocastDef();
                InitializeSpell();
                //base.UpdateAbilities();
            }
        }

        public void LoadCustomClassAbilities(TM_CustomClass cc, Pawn fromPawn = null)
        {
            for (int i = 0; i < cc.classMageAbilities.Count; i++)
            {
                TMAbilityDef ability = cc.classMageAbilities[i];
                MagicData fromData = null;
                if (fromPawn != null)
                {
                   fromData = fromPawn.GetCompAbilityUserMagic().MagicData;
                }
                if (fromData != null)
                {
                    foreach (MagicPower fp in fromData.AllMagicPowers)
                    {
                        if (fp.learned && cc.classMageAbilities.Contains(fp.abilityDef))
                        {
                            MagicPower mp = MagicData.AllMagicPowers.FirstOrDefault((MagicPower x) => x.abilityDef == fp.TMabilityDefs[0]);                            
                            if (mp != null)
                            {
                                mp.learned = true;
                                mp.level = fp.level;
                            }
                        }
                    }
                }

                for (int j = 0; j < MagicData.AllMagicPowers.Count; j++)
                {
                    if (MagicData.AllMagicPowers[j] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond) ||
                            MagicData.AllMagicPowers[j] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShadowBolt) ||
                            MagicData.AllMagicPowers[j] == MagicData.MagicPowersWD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate))
                    {
                        MagicData.AllMagicPowers[j].learned = false;
                    }
                    
                    if (MagicData.AllMagicPowers[j].TMabilityDefs.Contains(cc.classMageAbilities[i]) && MagicData.AllMagicPowers[j].learned)
                    {
                        if (cc.classMageAbilities[i].shouldInitialize)
                        {
                            int level = MagicData.AllMagicPowers[j].level;                                                        
                            AddPawnAbility(MagicData.AllMagicPowers[j].TMabilityDefs[level]);
                            if (magicData.AllMagicPowers[j].TMabilityDefs[level] == TorannMagicDefOf.TM_LightSkip)
                            {
                                if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 1)
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_LightSkipMass);
                                }
                                if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 2)
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_LightSkipGlobal);
                                }
                            }
                            if (cc.classMageAbilities[i] == TorannMagicDefOf.TM_Hex && HexedPawns.Count > 0)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                                RemovePawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                                RemovePawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                                AddPawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                                AddPawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                                AddPawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                            }
                        }
                        if (ability.childAbilities != null && ability.childAbilities.Count > 0)
                        {
                            for (int c = 0; c < ability.childAbilities.Count; c++)
                            {
                                if (ability.childAbilities[c].shouldInitialize)
                                {
                                    AddPawnAbility(ability.childAbilities[c]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddAdvancedClass(TM_CustomClass ac, Pawn fromPawn = null)
        {
            if (ac != null && ac.isMage && ac.isAdvancedClass)
            {
                Trait t = Pawn.story.traits.GetTrait(TorannMagicDefOf.TM_Possessed);
                if (t != null && !Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SpiritPossessionHD))
                {
                    Pawn.story.traits.RemoveTrait(t);
                    return;
                }
                if (!AdvancedClasses.Contains(ac))
                {
                    AdvancedClasses.Add(ac);
                }
                else // clear all abilities and re-add
                {
                    foreach (TMAbilityDef ability in ac.classMageAbilities)
                    {
                        RemovePawnAbility(ability);
                        if (ability.childAbilities != null && ability.childAbilities.Count > 0)
                        {
                            foreach (TMAbilityDef cab in ability.childAbilities)
                            {
                                RemovePawnAbility(cab);
                            }
                        }
                    }
                }
                if(fromPawn != null)
                {
                    MagicData fromData = fromPawn.GetCompAbilityUserMagic().MagicData;
                    if(fromData != null)
                    {
                        foreach(TMAbilityDef ability in ac.classMageAbilities)
                        {
                            MagicPowerSkill mps_e = MagicData.GetSkill_Efficiency(ability);
                            MagicPowerSkill fps_e = fromData.GetSkill_Efficiency(ability);
                            if (mps_e != null && fps_e != null)
                            {
                                mps_e.level = fps_e.level;
                            }
                            MagicPowerSkill mps_p = MagicData.GetSkill_Power(ability);
                            MagicPowerSkill fps_p = fromData.GetSkill_Power(ability);
                            if (mps_p != null && fps_p != null)
                            {
                                mps_p.level = fps_p.level;
                            }
                            MagicPowerSkill mps_v = MagicData.GetSkill_Versatility(ability);
                            MagicPowerSkill fps_v = fromData.GetSkill_Versatility(ability);
                            if (mps_v != null && fps_v != null)
                            {
                                mps_v.level = fps_v.level;
                            }
                        }
                    }
                }
                LoadCustomClassAbilities(ac, fromPawn);
            }
        }

        public void RemoveAdvancedClass(TM_CustomClass ac)
        {
            for (int i = 0; i < ac.classMageAbilities.Count; i++)
            {
                TMAbilityDef ability = ac.classMageAbilities[i];

                for (int j = 0; j < MagicData.AllMagicPowers.Count; j++)
                {
                    MagicPower power = MagicData.AllMagicPowers[j];
                    if (power.abilityDef == ability)
                    {
                        if (magicData.AllMagicPowers[j].TMabilityDefs[power.level] == TorannMagicDefOf.TM_LightSkip)
                        {
                            if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 1)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_LightSkipMass);
                            }
                            if (TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_LightSkip) >= 2)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_LightSkipGlobal);
                            }
                        }
                        if (ac.classMageAbilities[i] == TorannMagicDefOf.TM_Hex && HexedPawns.Count > 0)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Hex_CriticalFail);
                            RemovePawnAbility(TorannMagicDefOf.TM_Hex_MentalAssault);
                            RemovePawnAbility(TorannMagicDefOf.TM_Hex_Pain);
                        }                        
                        power.autocast = false;
                        power.learned = false;
                        power.level = 0;

                        if (ability.childAbilities != null && ability.childAbilities.Count > 0)
                        {
                            for (int c = 0; c < ability.childAbilities.Count; c++)
                            {
                                RemovePawnAbility(ability.childAbilities[c]);
                            }
                        }
                    }
                    RemovePawnAbility(ability);
                }
            }
            if (ac != null && ac.isMage && ac.isAdvancedClass)
            {
                foreach (TMAbilityDef ability in ac.classMageAbilities)
                {
                    MagicPowerSkill mps_e = MagicData.GetSkill_Efficiency(ability);
                    if (mps_e != null)
                    {
                        mps_e.level = 0;
                    }
                    MagicPowerSkill mps_p = MagicData.GetSkill_Power(ability);
                    if (mps_p != null)
                    {
                        mps_p.level = 0;
                    }
                    MagicPowerSkill mps_v = MagicData.GetSkill_Versatility(ability);
                    if (mps_v != null)
                    {
                        mps_v.level = 0;
                    }
                }
            }
            if(AdvancedClasses.Contains(ac))
            {
                AdvancedClasses.Remove(ac);
            }
        }

        public void UpdateAutocastDef()
        {
            IEnumerable<TM_CustomPowerDef> mpDefs = TM_Data.CustomMagePowerDefs();
            if (IsMagicUser && MagicData != null && MagicData.MagicPowersCustom != null)
            {
                foreach (MagicPower mp in MagicData.MagicPowersCustom)
                {
                    foreach (TM_CustomPowerDef mpDef in mpDefs)
                    {                        
                        if (mpDef.customPower.abilityDefs[0].ToString() == mp.GetAbilityDef(0).ToString())
                        {
                            if (mpDef.customPower.autocasting != null)
                            {
                                mp.autocasting = mpDef.customPower.autocasting;
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<string, Command> gizmoCommands = new Dictionary<string, Command>();
        public Command GetGizmoCommands(string key)
        {
            if (!gizmoCommands.ContainsKey(key))
            {
                Pawn p = Pawn;
                if (key == "symbiosis")
                {
                    Command_Action itemSymbiosis = new Command_Action
                    {
                        action = new Action(delegate
                        {
                            TM_Action.RemoveSymbiosisCommand(p);
                        }),
                        Order = 61,
                        defaultLabel = TM_TextPool.TM_RemoveSymbiosis,
                        defaultDesc = TM_TextPool.TM_RemoveSymbiosisDesc,
                        icon = ContentFinder<Texture2D>.Get("UI/remove_spiritpossession", true),
                    };
                    gizmoCommands.Add(key, itemSymbiosis);
                }
                if (key == "wanderer")
                {
                    Command_Action itemWanderer = new Command_Action
                    {
                        action = new Action(delegate
                        {
                            TM_Action.PromoteWanderer(p);
                        }),
                        Order = 51,
                        defaultLabel = TM_TextPool.TM_PromoteWanderer,
                        defaultDesc = TM_TextPool.TM_PromoteWandererDesc,
                        icon = ContentFinder<Texture2D>.Get("UI/wanderer", true),
                    };
                    gizmoCommands.Add(key, itemWanderer);
                }
                if(key == "technoBit")
                {
                    String toggle = "bit_c";
                    String label = "TM_TechnoBitEnabled".Translate();
                    String desc = "TM_TechnoBitToggleDesc".Translate();
                    if (!useTechnoBitToggle)
                    {
                        toggle = "bit_off";
                        label = "TM_TechnoBitDisabled".Translate();
                    }
                    var item = new Command_Toggle
                    {
                        isActive = () => useTechnoBitToggle,
                        toggleAction = () =>
                        {
                            useTechnoBitToggle = !useTechnoBitToggle;
                        },
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = -89,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle, true)
                    };
                    gizmoCommands.Add(key, item);
                }
                if(key == "technoRepair")
                {
                    String toggle_repair = "bit_repairon";
                    String label_repair = "TM_TechnoBitRepair".Translate();
                    String desc_repair = "TM_TechnoBitRepairDesc".Translate();
                    if (!useTechnoBitRepairToggle)
                    {
                        toggle_repair = "bit_repairoff";
                    }
                    var item_repair = new Command_Toggle
                    {
                        isActive = () => useTechnoBitRepairToggle,
                        toggleAction = () =>
                        {
                            useTechnoBitRepairToggle = !useTechnoBitRepairToggle;
                        },
                        defaultLabel = label_repair,
                        defaultDesc = desc_repair,
                        Order = -88,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle_repair, true)
                    };
                    gizmoCommands.Add(key, item_repair);
                }
                if(key == "elementalShot")
                {
                    String toggle = "elementalshot";
                    String label = "TM_TechnoWeapon_ver".Translate();
                    String desc = "TM_ElementalShotToggleDesc".Translate();
                    if (!useElementalShotToggle)
                    {
                        toggle = "elementalshot_off";
                    }
                    var item = new Command_Toggle
                    {
                        isActive = () => useElementalShotToggle,
                        toggleAction = () =>
                        {
                            useElementalShotToggle = !useElementalShotToggle;
                        },
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = -88,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle, true)
                    };
                    gizmoCommands.Add(key, item);
                }
            }
            if (gizmoCommands.ContainsKey(key))
            {
                return gizmoCommands[key];
            }
            else
            {
                return null;
            }
        }
    }
}
