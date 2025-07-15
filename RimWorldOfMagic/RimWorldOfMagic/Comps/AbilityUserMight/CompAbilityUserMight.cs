using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using AbilityUser;
using AbilityUserAI;
using Verse.AI;
using UnityEngine;
using CompDeflector;
using TorannMagic.Ideology;
using TorannMagic.Utils;

namespace TorannMagic
{
    [CompilerGenerated]
    [Serializable]
    [StaticConstructorOnStartup]
    public partial class CompAbilityUserMight : CompAbilityUserTMBase
    {
        public string LabelKey = "TM_Might";

        public bool mightPowersInitialized;
        public bool firstMightTick;

        private int fortitudeMitigationDelay = 0;
        private int mightXPRate = 900;
        private int lastMightXPGain;

        private int nextSSTend;

        private List<IntVec3> deathRing = new List<IntVec3>();
        public float weaponCritChance;
        public bool shouldDrawPsionicShield;
        public List<TM_EventRecords> mightUsed = new List<TM_EventRecords>();

        private float G_Sprint_eff = 0.20f;
        private float G_Grapple_eff = 0.10f;
        private float G_Cleave_eff = 0.10f;
        private float G_Whirlwind_eff = 0.10f;
        private float S_Headshot_eff = 0.10f;
        private float S_DisablingShot_eff = 0.10f;
        private float S_AntiArmor_eff = .10f;
        private float B_SeismicSlash_eff = 0.10f;
        private float B_BladeSpin_eff = 0.10f;
        private float B_PhaseStrike_eff = 0.08f;
        private float R_AnimalFriend_eff = 0.15f;
        private float R_ArrowStorm_eff = 0.08f;
        private float F_Disguise_eff = 0.10f;
        private float F_Mimic_eff = 0.08f;
        private float F_Reversal_eff = 0.10f;
        private float F_Transpose_eff = 0.08f;
        private float F_Possess_eff = 0.06f;
        private float P_PsionicBarrier_eff = 0.10f;
        private float P_PsionicBlast_eff = 0.08f;
        private float P_PsionicDash_eff = 0.10f;
        private float P_PsionicStorm_eff = 0.10f;
        private float DK_WaveOfFear_eff = 0.10f;
        private float DK_Spite_eff = 0.10f;
        private float DK_GraveBlade_eff = .08f;
        private float M_TigerStrike_eff = .1f;
        private float M_DragonStrike_eff = .1f;
        private float M_ThunderStrike_eff = .1f;
        private float C_CommanderAura_eff = .1f;
        private float C_TaskMasterAura_eff = .1f;
        private float C_ProvisionerAura_eff = .1f;
        private float C_StayAlert_eff = .1f;
        private float C_MoveOut_eff = .1f;
        private float C_HoldTheLine_eff = .1f;
        private float SS_PistolWhip_eff = .1f;
        private float SS_SuppressingFire_eff = .08f;
        private float SS_Mk203GL_eff = .08f;
        private float SS_Buckshot_eff = .08f;
        private float SS_BreachingCharge_eff = .08f;
        private float SS_CQC_eff = .1f;
        private float SS_FirstAid_eff = .1f;
        private float SS_60mmMortar_eff = .08f;

        private float global_seff = 0.03f;

        public bool skill_Sprint;
        public bool skill_GearRepair;
        public bool skill_InnerHealing;
        public bool skill_HeavyBlow;
        public bool skill_StrongBack;
        public bool skill_ThickSkin;
        public bool skill_FightersFocus;
        public bool skill_Teach;
        public bool skill_ThrowingKnife;
        public bool skill_BurningFury;
        public bool skill_PommelStrike;
        public bool skill_Legion;
        public bool skill_TempestStrike;
        public bool skill_PistolWhip;
        public bool skill_SuppressingFire;
        public bool skill_Mk203GL;
        public bool skill_Buckshot;
        public bool skill_BreachingCharge;

        public float maxSP = 1;
        public float spRegenRate = 1;
        public float spCost = 1;
        public float mightPwr = 1;
        private int resMitigationDelay = 0;
        public float totalApparelWeight;

        public bool animalBondingDisabled;

        public bool usePsionicAugmentationToggle = true;
        public bool usePsionicMindAttackToggle = true;
        public bool useCleaveToggle = true;
        public bool useCQCToggle = true;
        public List<Thing> combatItems = new List<Thing>();
        public int allowMeditateTick;
        public ThingOwner<ThingWithComps> equipmentContainer = new ThingOwner<ThingWithComps>();
        public int specWpnRegNum = -1;        

        public Verb_Deflected deflectVerb;
        private DamageInfo reversal_dinfo;
        private Thing reversalTarget;
        public Pawn bondedPet;

        public Verb_UseAbility lastVerbUsed = null;
        public int lastTickVerbUsed;

        public TMAbilityDef mimicAbility;

        // Cached values calculated in TM_PawnTracker
        private bool initializedIsMightUser;
        private bool isMightUser;  // Cached version

        private static HashSet<ushort> mightTraitIndexes = new HashSet<ushort>()
        {
            TorannMagicDefOf.TM_Monk.index,
            TorannMagicDefOf.DeathKnight.index,
            TorannMagicDefOf.TM_Psionic.index,
            TorannMagicDefOf.Gladiator.index,
            TorannMagicDefOf.TM_Sniper.index,
            TorannMagicDefOf.Bladedancer.index,
            TorannMagicDefOf.Ranger.index,
            TorannMagicDefOf.Faceless.index,
            TorannMagicDefOf.TM_Commander.index,
            TorannMagicDefOf.TM_SuperSoldier.index,
            TorannMagicDefOf.TM_Wayfarer.index
        };

        public class ChainedMightAbility
        {
            public ChainedMightAbility(TMAbilityDef _ability, int _expirationTicks, bool _expires)
            {
                abilityDef = _ability;
                expirationTicks = _expirationTicks;
                expires = _expires;
            }
            public TMAbilityDef abilityDef;
            public int expirationTicks;
            public bool expires = true;
        }
        public List<ChainedMightAbility> chainedAbilitiesList = new List<ChainedMightAbility>();

        private MightData mightData;
        public MightData MightData
        {
            get
            {
                if (mightData == null && IsMightUser)
                {
                    mightData = new MightData(this);
                }
                return mightData;
            }
        }

        public List<TM_EventRecords> MightUsed
        {
            get
            {
                if (mightUsed == null)
                {
                    mightUsed = new List<TM_EventRecords>();
                    mightUsed.Clear();
                }
                return mightUsed;
            }
            set
            {
                if (mightUsed == null)
                {
                    mightUsed = new List<TM_EventRecords>();
                    mightUsed.Clear();
                }
                mightUsed = value;
            }
        }

        public float GetSkillDamage()
        {
            float result;
            float strFactor = 1f;

            if (IsMightUser)
            {
                strFactor = mightPwr;
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

        public bool shouldDraw = true;
        public override void PostDraw()
        {
            if (Pawn.DestroyedOrNull()) return;
            if (Pawn.Dead) return;
            if (!shouldDraw) return;
            
            base.PostDraw();
            if (Pawn.health.hediffSet.hediffs.Any(hediff =>
                    hediff.def == TorannMagicDefOf.TM_PossessionHD
                    || hediff.def == TorannMagicDefOf.TM_CoOpPossessionHD
                    || hediff.def == TorannMagicDefOf.TM_PossessionHD_I
                    || hediff.def == TorannMagicDefOf.TM_PossessionHD_II
                    || hediff.def == TorannMagicDefOf.TM_PossessionHD_III
                    || hediff.def == TorannMagicDefOf.TM_CoOpPossessionHD_I
                    || hediff.def == TorannMagicDefOf.TM_CoOpPossessionHD_II
                    || hediff.def == TorannMagicDefOf.TM_CoOpPossessionHD_III
                ))
            {
                DrawDeceptionTicker(true);
            }
            else if (Pawn.health.hediffSet.hediffs.Any(hediff =>
                 hediff.def == TorannMagicDefOf.TM_DisguiseHD
                 || hediff.def == TorannMagicDefOf.TM_DisguiseHD_I
                 || hediff.def == TorannMagicDefOf.TM_DisguiseHD_II
                 || hediff.def == TorannMagicDefOf.TM_DisguiseHD_III
             ))
            {
                DrawDeceptionTicker(false);
            }

            
            if (ModOptions.Settings.Instance.AIFriendlyMarking && Pawn.IsColonist && IsMightUser)
            {
                DrawMark();
            }
            if (ModOptions.Settings.Instance.AIMarking && !Pawn.IsColonist && IsMightUser)
            {
                DrawMark();
            }

            if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_BurningFuryHD, false))
            {
                float num = Mathf.Lerp(1.2f, 1.55f, 1f);
                Vector3 vector = Pawn.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(1.7f, 1f, 1.7f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.burningFuryMat, 0);                
            }

            if (shouldDrawPsionicShield)
            {
                float radius = 2.5f;
                radius = 2.5f + (.75f * TM_Calc.GetSkillVersatilityLevel(Pawn, TorannMagicDefOf.TM_PsionicBarrier, false));//TM_Calc.GetMightSkillLevel(this.Pawn, this.MightData.MightPowerSkill_PsionicBarrier, "TM_PsionicBarrier", "_ver", true));
                float drawRadius = radius * .23f;
                float num = Mathf.Lerp(drawRadius, 9.5f, drawRadius);
                Vector3 vector = Pawn.CurJob.targetA.CenterVector3;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);
                Vector3 s = new Vector3(num, 9.5f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_MatPool.PsionicBarrier, 0);
            }            
        }

        public void DrawDeceptionTicker(bool possessed)
        {
            if (possessed)
            {
                Vector3 vector = Pawn.Drawer.DrawPos;
                vector.x += -.25f;
                vector.z += .8f;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                const float angle = 0f;
                Vector3 s = new Vector3(.45f, 1f, .4f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.possessionMask, 0);
                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD) || 
                    Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_I) || 
                    Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_II) || 
                    Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_III))
                {
                    vector.z += .35f;
                    matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.deceptionEye, 0);
                }
            }
            else
            {
                float num = Mathf.Lerp(1.2f, 1.55f, 1f);
                Vector3 vector = Pawn.Drawer.DrawPos;
                vector.x += -.25f;
                vector.z += .8f;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                const float angle = 0f;
                Vector3 s = new Vector3(.45f, 1f, .4f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.deceptionEye, 0);
            }
        }

        public static List<TMAbilityDef> MightAbilities = null;   
        

        public override void CompTick()
        {
            bool flag = Pawn != null;
            if (flag)
            {
                if (Pawn.IsShambler || Pawn.IsGhoul)
                {
                    if (mightData != null)
                    {
                        RemoveAbilityUser();
                    }
                    return;
                }
                bool spawned = Pawn.Spawned;
                if (spawned)
                {
                    bool isMightUser = IsMightUser && !Pawn.NonHumanlikeOrWildMan();
                    if (isMightUser)
                    {
                        if (!Initialized)
                        {
                            PostInitializeTick();
                        }
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
                        if (Find.TickManager.TicksGame % 20 == 0)
                        {
                            ResolveSustainedSkills();
                            if (reversalTarget != null)
                            {
                                ResolveReversalDamage();
                            }
                        }
                        if (Find.TickManager.TicksGame % 60 == 0)
                        {                            
                            ResolveClassSkills();
                            //ResolveClassPassions(); currently disabled
                        }
                        
                        if (autocastTick < Find.TickManager.TicksGame)  //180 default
                        {
                            if ( !Pawn.Dead && !Pawn.Downed && Pawn.Map != null && Pawn.story != null && Pawn.story.traits != null && MightData != null && AbilityData != null && !Pawn.InMentalState)
                            {
                                if (Pawn.IsColonist)
                                {
                                    autocastTick = Find.TickManager.TicksGame + (int)Rand.Range(.8f * ModOptions.Settings.Instance.autocastEvaluationFrequency, 1.2f * ModOptions.Settings.Instance.autocastEvaluationFrequency);
                                    ResolveAutoCast();
                                }
                                else if(ModOptions.Settings.Instance.AICasting && (!Pawn.IsPrisoner || Pawn.IsFighting()))
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
                        if (Pawn.needs.AllNeeds.Contains(Stamina) && Stamina.CurLevel > (.99f * Stamina.MaxLevel))
                        {
                            if (age > (lastMightXPGain + mightXPRate))
                            {
                                MightData.MightUserXP++;
                                lastMightXPGain = age;
                            }
                        }
                        bool flag4 = Find.TickManager.TicksGame % 30 == 0;
                        if (flag4)
                        {
                            bool flag5 = MightUserXP > MightUserXPTillNextLevel;
                            if (flag5)
                            {
                                LevelUp(false);
                            }
                        }
                        if (Find.TickManager.TicksGame % 30 == 0)
                        {
                            bool flag6 = Pawn.TargetCurrentlyAimingAt != null;
                            if (flag6)
                            {
                                if (Pawn.TargetCurrentlyAimingAt.Thing is Pawn)
                                {
                                    Pawn targetPawn = Pawn.TargetCurrentlyAimingAt.Thing as Pawn;
                                    if (targetPawn.RaceProps.Humanlike)
                                    {
                                        bool flag7 = (Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_DisguiseHD")) || Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_DisguiseHD_I")) || Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_DisguiseHD_II")) || Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_DisguiseHD_III")));
                                        if (targetPawn.Faction != Pawn.Faction && flag7)
                                        {
                                            using (IEnumerator<Hediff> enumerator = Pawn.health.hediffSet.hediffs.GetEnumerator())
                                            {
                                                while (enumerator.MoveNext())
                                                {
                                                    Hediff rec = enumerator.Current;
                                                    if (rec.def == TorannMagicDefOf.TM_DisguiseHD || rec.def == TorannMagicDefOf.TM_DisguiseHD_I || rec.def == TorannMagicDefOf.TM_DisguiseHD_II || rec.def == TorannMagicDefOf.TM_DisguiseHD_III)
                                                    {
                                                        Pawn.health.RemoveHediff(rec);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
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
                        if(Find.TickManager.TicksGame % 301 == 0) //cache weapon damage for tooltip and damage calculations
                        {
                            weaponDamage = GetSkillDamage(); //TM_Calc.GetSkillDamage(this.Pawn);
                        }
                        if (Find.TickManager.TicksGame % 602 == 0)
                        {
                            ResolveMightUseEvents();
                        }
                    }                    
                }
                else
                {
                    if (Find.TickManager.TicksGame % 600 == 0)
                    {
                        if (Pawn.Map == null)
                        {
                            if (IsMightUser)
                            {
                                int num;
                                if (AbilityData?.AllPowers != null)
                                {
                                    AbilityData obj = AbilityData;
                                    num = ((obj != null && obj.AllPowers.Count > 0) ? 1 : 0);
                                }
                                else
                                {
                                    num = 0;
                                }
                                if (num != 0)
                                {
                                    foreach (PawnAbility allPower in AbilityData.AllPowers)
                                    {
                                        allPower.CooldownTicksLeft -= 600;
                                        if (allPower.CooldownTicksLeft <= 0)
                                        {
                                            allPower.CooldownTicksLeft = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Initialized)
            {
                //custom code
            }          
        }

        private int deathRetaliationDelayCount;
        public void DoDeathRetaliation()
        {
            if (!Pawn.Downed || Pawn.Map == null || Pawn.IsPrisoner || Pawn.Faction == null || !Pawn.Faction.HostileTo(Faction.OfPlayerSilentFail))
            {
                deathRetaliating = false;
                canDeathRetaliate = false;
                deathRetaliationDelayCount = 0;
            }
            if (canDeathRetaliate && deathRetaliating)
            {
                ticksTillRetaliation--;                
                if (deathRing == null || deathRing.Count < 1)
                {
                    deathRing = TM_Calc.GetOuterRing(Pawn.Position, 1f, 2f);
                }
                if (Find.TickManager.TicksGame % 7 == 0)
                {
                    Vector3 moteVec = deathRing.RandomElement().ToVector3Shifted();
                    moteVec.x += Rand.Range(-.4f, .4f);
                    moteVec.z += Rand.Range(-.4f, .4f);
                    float angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(moteVec, Pawn.DrawPos)).ToAngleFlat();
                    ThingDef mote = TorannMagicDefOf.Mote_Psi_Grayscale;
                    mote.graphicData.color = Color.white;
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi_Grayscale, moteVec, Pawn.Map, Rand.Range(.25f, .6f), .1f, .05f, .05f, 0, Rand.Range(4f, 6f), angle, angle);
                }
                if (ticksTillRetaliation <= 0)
                {
                    canDeathRetaliate = false;
                    deathRetaliating = false;
                    TM_Action.CreateMightDeathEffect(Pawn, Pawn.Position);
                }
            }
            else if (canDeathRetaliate)
            {
                if (deathRetaliationDelayCount >= 20 && Rand.Value < .04f)
                {
                    
                    deathRetaliating = true;
                    ticksTillRetaliation = Mathf.RoundToInt(Rand.Range(400, 1200) * ModOptions.Settings.Instance.deathRetaliationDelayFactor);
                    deathRing = TM_Calc.GetOuterRing(Pawn.Position, 1f, 2f);
                }
                else
                {
                    deathRetaliationDelayCount++;
                }
            }
        }

        public void PostInitializeTick()
        {
            if (Pawn?.story == null || !Pawn.Spawned) return;

            Initialize();
            ResolveMightTab();
            ResolveMightPowers();
            ResolveStamina();
        }

        public bool IsMightUser => initializedIsMightUser ? isMightUser : SetIsMightUser();
        public bool SetIsMightUser()
        {
            Pawn pawn = Pawn;
            if (pawn?.story == null) return isMightUser = false;
            initializedIsMightUser = true;
            if (customClass != null) return isMightUser = true;
            if (customClass == null && customIndex == -2)
            {
                customIndex = TM_ClassUtility.CustomClassIndexOfBaseFighterClass(pawn.story.traits.allTraits);
                if (customIndex >= 0)
                {
                    if (!TM_ClassUtility.CustomClasses[customIndex].isFighter)
                    {
                        customIndex = -1;
                        return isMightUser = false;
                    }
                    customClass = TM_ClassUtility.CustomClasses[customIndex];
                    return isMightUser = true;
                }
            }
            for (int i = pawn.story.traits.allTraits.Count - 1; i >= 0; i--)
            {
                if (mightTraitIndexes.Contains(Pawn.story.traits.allTraits[i].def.index)) return isMightUser = true;
            }
            if (AdvancedClasses.Count > 0 || TM_Calc.IsWayfarer(pawn)) return isMightUser = true;
            if (TM_Calc.HasAdvancedClass(pawn))
            {
                foreach (TMDefs.TM_CustomClass cc in TM_ClassUtility.GetAdvancedClassesForPawn(Pawn))
                {
                    if (cc.isFighter)
                    {
                        AdvancedClasses.Add(cc);
                        return isMightUser = true;
                    }
                }
            }
            return isMightUser = false;
        }

        public int MightUserLevel
        {
            get => MightData?.MightUserLevel ?? 0;            
            set
            {
                if (value > MightData.MightUserLevel)
                {
                    MightData.MightAbilityPoints++;
                    if (MightData.MightUserXP < GetXPForLevel(value - 1))
                    {
                        MightData.MightUserXP = GetXPForLevel(value-1);
                    }
                }
                MightData.MightUserLevel = value;
            }
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

        public int MightUserXP
        {
            get
            {
                return MightData.MightUserXP;
            }
            set
            {
                MightData.MightUserXP = value;
            }
        }

        public float XPLastLevel
        {
            get
            {
                bool flag = MightUserLevel > 0;
                if (flag)
                {
                    return GetXPForLevel(MightUserLevel - 1);
                }
                return 0f;
            }
        }

        public float XPTillNextLevelPercent
        {
            get
            {
                return ((float)MightData.MightUserXP - XPLastLevel) / ((float)MightUserXPTillNextLevel - XPLastLevel);
            }
        }

        
        public int MightUserXPTillNextLevel
        {
            get
            {
                if (MightUserXP < XPLastLevel)
                {
                    MightUserXP = (int)XPLastLevel;
                }
                return GetXPForLevel(MightUserLevel);
            }
        }

        public void LevelUp(bool hideNotification = false)
        {
            if (!Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Wanderer))
            {
                if (MightUserLevel < (customClass?.maxFighterLevel ?? 200))
                {
                    MightUserLevel++;
                    bool flag = !hideNotification;
                    if (flag)
                    {
                        
                        if (Pawn.IsColonist && ModOptions.Settings.Instance.showLevelUpMessage)
                        {
                            Messages.Message("TM_MightLevelUp".Translate(
                                parent.Label
                            ), Pawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }                    
                }
            }
            else
            {
                MightUserXP = (int)XPLastLevel;
            }
        }

        public void LevelUpPower(MightPower power)
        {
            foreach (AbilityUser.AbilityDef current in power.TMabilityDefs)
            {
                RemovePawnAbility(current);
            }
            power.level++;
            AddPawnAbility(power.TMabilityDefs[power.level], true, -1f);
            UpdateAbilities();
        }

        public Need_Stamina Stamina
        {
            get
            {
                if (!Pawn.DestroyedOrNull() && Pawn.needs != null)
                {
                    return Pawn.needs.TryGetNeed<Need_Stamina>();
                }
                return null;
            }
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            if(MightAbilities != null) return;

            if (mightPowersInitialized == false && MightData != null)
            {
                AssignAbilities();
            }            
        }

        public void AssignAbilities()
        {
            Pawn abilityUser = Pawn;
            bool flag2;
            MightData.MightUserLevel = 0;
            MightData.MightAbilityPoints = 0;
            List<TMAbilityDef> usedAbilities = new List<TMAbilityDef>();
            usedAbilities.Clear();
            if (customClass != null)
            {
                for (int z = 0; z < MightData.AllMightPowers.Count; z++)
                {
                    TMAbilityDef ability = (TMAbilityDef)MightData.AllMightPowers[z].abilityDef;
                    if (usedAbilities.Contains(ability))
                    {
                        continue;
                    }
                    else
                    {
                        usedAbilities.Add(ability);
                    }
                    if (customClass.classFighterAbilities.Contains(MightData.AllMightPowers[z].abilityDef))
                    {
                        MightData.AllMightPowers[z].learned = true;
                    }                    
                    if (MightData.AllMightPowers[z].learned)
                    {
                        if (ability.shouldInitialize)
                        {
                            AddPawnAbility(ability);
                        }
                        if(ability.childAbilities != null && ability.childAbilities.Count > 0)
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
                //for (int j = 0; j < this.customClass.classFighterAbilities.Count; j++)
                //{
                    
                //}
                if (customClass.classHediff != null)
                {
                    HealthUtility.AdjustSeverity(abilityUser, customClass.classHediff, customClass.hediffSeverity);
                }
            }
            else
            {
                flag2 = TM_Calc.IsWayfarer(abilityUser);
                if (flag2)
                {
                    //Log.Message("Initializing Wayfarer Abilities");
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_WayfarerCraft).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_FieldTraining).learned = true;
                    if (!abilityUser.IsColonist)
                    {
                        skill_ThrowingKnife = true;
                        MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThrowingKnife).learned = true;
                        AddPawnAbility(TorannMagicDefOf.TM_ThrowingKnife);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        MightPower mp = MightData.MightPowersStandalone.RandomElement();
                        if (mp.abilityDef == TorannMagicDefOf.TM_GearRepair)
                        {
                            mp.learned = true;
                            skill_GearRepair = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_InnerHealing)
                        {
                            mp.learned = true;
                            skill_InnerHealing = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_HeavyBlow)
                        {
                            mp.learned = true;
                            skill_HeavyBlow = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_ThickSkin)
                        {
                            mp.learned = true;
                            skill_ThickSkin = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_FightersFocus)
                        {
                            mp.learned = true;
                            skill_FightersFocus = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_StrongBack)
                        {
                            mp.learned = true;
                            skill_StrongBack = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_ThrowingKnife)
                        {
                            mp.learned = true;
                            skill_ThrowingKnife = true;
                        }
                        else if (mp.abilityDef == TorannMagicDefOf.TM_PommelStrike)
                        {
                            mp.learned = true;
                            skill_PommelStrike = true;
                        }
                    }
                    InitializeSkill();
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Gladiator);
                if (flag2)
                {
                    //Log.Message("Initializing Gladiator Abilities");
                    AddPawnAbility(TorannMagicDefOf.TM_Sprint);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Sprint).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Sprint_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Sprint_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Sprint_III).learned = true;
                    //this.AddPawnAbility(TorannMagicDefOf.TM_Fortitude);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Fortitude).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Grapple);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Grapple).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Grapple_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Grapple_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Grapple_III).learned = true;
                    //this.AddPawnAbility(TorannMagicDefOf.TM_Cleave);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Cleave).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Whirlwind);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Whirlwind).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper);
                if (flag2)
                {
                    //Log.Message("Initializing Sniper Abilities");
                    //this.AddPawnAbility(TorannMagicDefOf.TM_SniperFocus);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_SniperFocus).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Headshot);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Headshot).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_DisablingShot);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_DisablingShot).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_DisablingShot_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_DisablingShot_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_DisablingShot_III).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_AntiArmor);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_AntiArmor).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_ShadowSlayer);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ShadowSlayer).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Bladedancer);
                if (flag2)
                {
                    // Log.Message("Initializing Bladedancer Abilities");
                    // this.AddPawnAbility(TorannMagicDefOf.TM_BladeFocus);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BladeFocus).learned = true;
                    //this.AddPawnAbility(TorannMagicDefOf.TM_BladeArt);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BladeArt).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_SeismicSlash);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_SeismicSlash).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_BladeSpin);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BladeSpin).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PhaseStrike);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PhaseStrike).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PhaseStrike_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PhaseStrike_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PhaseStrike_III).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Ranger);
                if (flag2)
                {
                    //Log.Message("Initializing Ranger Abilities");
                    //this.AddPawnAbility(TorannMagicDefOf.TM_RangerTraining);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_RangerTraining).learned = true;
                    // this.AddPawnAbility(TorannMagicDefOf.TM_BowTraining);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BowTraining).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PoisonTrap);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PoisonTrap).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_AnimalFriend);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_AnimalFriend).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_ArrowStorm);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ArrowStorm).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ArrowStorm_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ArrowStorm_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ArrowStorm_III).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Faceless);
                if (flag2)
                {
                    //Log.Message("Initializing Faceless Abilities");
                    AddPawnAbility(TorannMagicDefOf.TM_Disguise);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Disguise).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Mimic);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Mimic).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Reversal);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Reversal).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Transpose);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Transpose).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Transpose_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Transpose_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Transpose_III).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Possess);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Possess).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic);
                if (flag2)
                {
                    //Log.Message("Initializing Psionic Abilities");
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicAugmentation).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PsionicBarrier);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBarrier).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBarrier_Projected).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PsionicBlast);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBlast).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBlast_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBlast_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicBlast_III).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PsionicDash);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicDash).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_PsionicStorm);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PsionicStorm).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.DeathKnight);
                if (flag2)
                {
                    //Log.Message("Initializing Death Knight Abilities");
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Shroud).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_WaveOfFear);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_WaveOfFear).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Spite);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Spite).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Spite_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Spite_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Spite_III).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_LifeSteal).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_GraveBlade);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GraveBlade).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GraveBlade_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GraveBlade_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GraveBlade_III).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Monk);
                if (flag2)
                {
                    //Log.Message("Initializing Monk Abilities");
                    //this.AddPawnAbility(TorannMagicDefOf.TM_Chi);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Chi).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_ChiBurst);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_MindOverBody).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_Meditate);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Meditate).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_TigerStrike);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_TigerStrike).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_DragonStrike);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_DragonStrike).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_ThunderStrike);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThunderStrike).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Commander);
                if (flag2)
                {
                    //Log.Message("Initializing Commander Abilities");
                    AddPawnAbility(TorannMagicDefOf.TM_ProvisionerAura);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ProvisionerAura).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_TaskMasterAura);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_TaskMasterAura).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_CommanderAura);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_CommanderAura).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_StayAlert);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StayAlert).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StayAlert_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StayAlert_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StayAlert_III).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_MoveOut);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_MoveOut).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_MoveOut_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_MoveOut_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_MoveOut_III).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_HoldTheLine);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HoldTheLine).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HoldTheLine_I).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HoldTheLine_II).learned = true;
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HoldTheLine_III).learned = true;
                }
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier);
                if (flag2)
                {
                    //Log.Message("Initializing Super Soldier Abilities");
                    MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PistolSpec).learned = false;
                    MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_RifleSpec).learned = false;
                    MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ShotgunSpec).learned = false;
                    //this.MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PistolWhip).learned = false;
                    //this.MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_SuppressingFire).learned = false;
                    //this.MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Mk203GL).learned = false;
                    //this.MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_Buckshot).learned = false;
                    //this.MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_BreachingCharge).learned = false;

                    //this.AddPawnAbility(TorannMagicDefOf.TM_CQC);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_CQC).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_FirstAid);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_FirstAid).learned = true;
                    AddPawnAbility(TorannMagicDefOf.TM_60mmMortar);
                    MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_60mmMortar).learned = true;
                }
            }
            AssignAdvancedClassAbilities(true);
            mightPowersInitialized = true;
            //base.UpdateAbilities();
        }

        public void AssignAdvancedClassAbilities(bool firstAssignment = false)
        {
            if (AdvancedClasses != null && AdvancedClasses.Count > 0)
            {
                for (int z = 0; z < MightData.AllMightPowers.Count; z++)
                {
                    TMAbilityDef ability = (TMAbilityDef)MightData.AllMightPowers[z].abilityDef;
                    foreach (TMDefs.TM_CustomClass cc in AdvancedClasses)
                    {
                        if (cc.classFighterAbilities.Contains(ability))
                        {
                            MightData.AllMightPowers[z].learned = true;
                        }                       
                        if (MightData.AllMightPowers[z].learned)
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
            }
        }

        public void InitializeSkill()  //used for class independant skills
        {
            Pawn abilityUser = Pawn;
            if (mimicAbility != null)
            {
                RemovePawnAbility(mimicAbility);
                AddPawnAbility(mimicAbility);
            }
            if (customClass != null)
            {
                //for (int j = 0; j < this.MightData.AllMightPowersWithSkills.Count; j++)
                //{
                //    if (this.MightData.AllMightPowersWithSkills[j].learned && !this.customClass.classFighterAbilities.Contains(this.MightData.AllMightPowersWithSkills[j].abilityDef))
                //    {
                //        this.MightData.AllMightPowersWithSkills[j].learned = false;
                //        this.RemovePawnAbility(this.MightData.AllMightPowersWithSkills[j].abilityDef);
                //    }
                //}
                for (int j = 0; j< MightData.AllMightPowers.Count; j++)
                {                    
                    if (MightData.AllMightPowers[j].learned && !customClass.classFighterAbilities.Contains(MightData.AllMightPowers[j].abilityDef))
                    {
                        RemovePawnAbility(MightData.AllMightPowers[j].abilityDef);
                        AddPawnAbility(MightData.AllMightPowers[j].abilityDef);
                    }
                }
            }
            else
            {
                if (skill_Sprint == true && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.Gladiator))
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_Sprint);
                    AddPawnAbility(TorannMagicDefOf.TM_Sprint);
                }
                if (skill_GearRepair == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_GearRepair);
                    AddPawnAbility(TorannMagicDefOf.TM_GearRepair);
                }
                if (skill_InnerHealing == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_InnerHealing);
                    AddPawnAbility(TorannMagicDefOf.TM_InnerHealing);
                }
                if (skill_StrongBack == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_StrongBack);
                    AddPawnAbility(TorannMagicDefOf.TM_StrongBack);
                }
                if (skill_HeavyBlow == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_HeavyBlow);
                    AddPawnAbility(TorannMagicDefOf.TM_HeavyBlow);
                }
                if (skill_ThickSkin == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_ThickSkin);
                    AddPawnAbility(TorannMagicDefOf.TM_ThickSkin);
                }
                if (skill_FightersFocus == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_FightersFocus);
                    AddPawnAbility(TorannMagicDefOf.TM_FightersFocus);
                }
                if (skill_Teach == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_TeachMight);
                    AddPawnAbility(TorannMagicDefOf.TM_TeachMight);
                }
                if (skill_ThrowingKnife == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_ThrowingKnife);
                    AddPawnAbility(TorannMagicDefOf.TM_ThrowingKnife);
                }
                if (skill_BurningFury == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_BurningFury);
                    AddPawnAbility(TorannMagicDefOf.TM_BurningFury);
                }
                if (skill_PommelStrike == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_PommelStrike);
                    AddPawnAbility(TorannMagicDefOf.TM_PommelStrike);
                }
                if (skill_TempestStrike == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_TempestStrike);
                    AddPawnAbility(TorannMagicDefOf.TM_TempestStrike);
                }
                if (skill_Legion == true)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_Legion);
                    AddPawnAbility(TorannMagicDefOf.TM_Legion);
                }
                if (skill_PistolWhip)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_PistolWhip);
                    AddPawnAbility(TorannMagicDefOf.TM_PistolWhip);
                }
                if (skill_SuppressingFire)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_SuppressingFire);
                    AddPawnAbility(TorannMagicDefOf.TM_SuppressingFire);
                }
                if (skill_Mk203GL)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_Mk203GL);
                    AddPawnAbility(TorannMagicDefOf.TM_Mk203GL);
                }
                if (skill_Buckshot)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_Buckshot);
                    AddPawnAbility(TorannMagicDefOf.TM_Buckshot);
                }
                if (skill_BreachingCharge)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_BreachingCharge);
                    AddPawnAbility(TorannMagicDefOf.TM_BreachingCharge);
                }
                if (IsMightUser && MightData.MightPowersCustomAll != null && MightData.MightPowersCustomAll.Count > 0)
                {
                    for (int j = 0; j < MightData.MightPowersCustomAll.Count; j++)
                    {
                        if (MightData.MightPowersCustomAll[j].learned)
                        {
                            RemovePawnAbility(MightData.MightPowersCustomAll[j].abilityDef);
                            AddPawnAbility(MightData.MightPowersCustomAll[j].abilityDef);
                        }
                    }
                }
            }
        }

        public void FixPowers()
        {
            Pawn abilityUser = Pawn;
            if (mightPowersInitialized == true)
            {
                bool flag2;
                flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Gladiator);
                if (flag2)
                {
                    Log.Message("Fixing Gladiator Abilities");
                    foreach (MightPower currentG in MightData.MightPowersG)
                    {
                        if (currentG.abilityDef.defName == "TM_Sprint" || currentG.abilityDef.defName == "TM_Sprint_I" || currentG.abilityDef.defName == "TM_Sprint_II" || currentG.abilityDef.defName == "TM_Sprint_III")
                        {
                            if (currentG.level == 0)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_I);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_III);
                            }
                            else if (currentG.level == 1)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_III);
                            }
                            else if (currentG.level == 2)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_I);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_III);
                            }
                            else if (currentG.level == 3)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Sprint_I);
                            }
                            else
                            {
                                Log.Message("Ability level not found to fix");
                            }
                        }
                        if (currentG.abilityDef.defName == "TM_Grapple" || currentG.abilityDef.defName == "TM_Grapple_I" || currentG.abilityDef.defName == "TM_Grapple_II" || currentG.abilityDef.defName == "TM_Grapple_III")
                        {
                            if (currentG.level == 0)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_I);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_III);
                            }
                            else if (currentG.level == 1)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_III);
                            }
                            else if (currentG.level == 2)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_I);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_III);
                            }
                            else if (currentG.level == 3)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_II);
                                RemovePawnAbility(TorannMagicDefOf.TM_Grapple_I);
                            }
                            else
                            {
                                Log.Message("Ability level not found to fix");
                            }
                        }
                    }            
                }
            }
            //this.UpdateAbilities();
            //base.UpdateAbilities();
        }

        public override bool TryTransformPawn()
        {
            return IsMightUser;
        }

        public bool TryAddPawnAbility(TMAbilityDef ability)
        {
            //add check to verify no ability is already added
            bool result = false;
            AddPawnAbility(ability, true, -1f);
            result = true;
            return result;
        }

        public void RemovePowers(bool clearStandalone = false)
        {
            Pawn abilityUser = Pawn;
            if (mightPowersInitialized == true && MightData != null)
            {
                bool flag2 = true;
                if (customClass != null)
                {
                    for (int i = 0; i < MightData.AllMightPowers.Count; i++)
                    {
                        MightPower mp = MightData.AllMightPowers[i];
                        for (int j = 0; j < mp.TMabilityDefs.Count; j++)
                        {
                            TMAbilityDef tmad = mp.TMabilityDefs[j] as TMAbilityDef;
                            if (tmad.childAbilities != null && tmad.childAbilities.Count > 0)
                            {
                                for (int k = 0; k < tmad.childAbilities.Count; k++)
                                {
                                    RemovePawnAbility(tmad.childAbilities[k]);
                                }
                            }
                            RemovePawnAbility(mp.TMabilityDefs[j]);
                        }
                        mp.learned = false;
                    }
                }
                if (clearStandalone)
                {
                    skill_BurningFury = false;
                    skill_FightersFocus = false;
                    skill_GearRepair = false;
                    skill_HeavyBlow = false;
                    skill_InnerHealing = false;
                    skill_Legion = false;
                    skill_PommelStrike = false;
                    skill_Sprint = false;
                    skill_StrongBack = false;
                    skill_Teach = false;
                    skill_TempestStrike = false;
                    skill_ThickSkin = false;
                    skill_ThrowingKnife = false;
                }
                
                foreach (MightPower current in MightData.MightPowersStandalone)
                {
                    RemovePawnAbility(current.abilityDef);
                }
                foreach(MightPower current in MightData.AllMightPowers)
                {
                    RemovePawnAbility(current.abilityDef);
                }
                if (TM_Calc.IsWayfarer(Pawn))
                {
                    skill_ThrowingKnife = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_ThrowingKnife);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Gladiator);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersG)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Sprint_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Sprint_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Sprint_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_Grapple_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Grapple_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Grapple_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersS)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_DisablingShot_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_DisablingShot_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_DisablingShot_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Bladedancer);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersB)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_PhaseStrike_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_PhaseStrike_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_PhaseStrike_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Ranger);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersR)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_ArrowStorm_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_ArrowStorm_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_ArrowStorm_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Faceless);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersF)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Transpose_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Transpose_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Transpose_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersP)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_PsionicBarrier_Projected);
                    RemovePawnAbility(TorannMagicDefOf.TM_PsionicBlast_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_PsionicBlast_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_PsionicBlast_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.DeathKnight);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersDK)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Spite_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Spite_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Spite_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_GraveBlade_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_GraveBlade_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_GraveBlade_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Monk);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersM)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_ChiBurst);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Commander);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersC)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_StayAlert_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_StayAlert_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_StayAlert_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_MoveOut_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_MoveOut_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_MoveOut_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_HoldTheLine_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_HoldTheLine_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_HoldTheLine_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier);
                if (flag2)
                {
                    foreach (MightPower current in MightData.MightPowersSS)
                    {
                        current.learned = false;
                        RemovePawnAbility(current.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_PistolWhip);
                    skill_PistolWhip = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_SuppressingFire);
                    skill_SuppressingFire = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_Mk203GL);
                    skill_Mk203GL = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_Buckshot);
                    skill_Buckshot = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_BreachingCharge);
                    skill_BreachingCharge = false;
                }                
            }
        }

        public void ResetSkills()
        {
            MightData.MightPowerSkill_global_endurance.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_endurance_pwr").level = 0;
            MightData.MightPowerSkill_global_refresh.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_refresh_pwr").level = 0;
            MightData.MightPowerSkill_global_seff.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_seff_pwr").level = 0;
            MightData.MightPowerSkill_global_strength.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_strength_pwr").level = 0;
            for (int i = 0; i < MightData.AllMightPowersWithSkills.Count; i++)
            {
                MightData.AllMightPowersWithSkills[i].level = 0;
                MightData.AllMightPowersWithSkills[i].learned = false;
                MightData.AllMightPowersWithSkills[i].autocast = false;
                TMAbilityDef ability = (TMAbilityDef)MightData.AllMightPowersWithSkills[i].abilityDef;
                MightPowerSkill mps = MightData.GetSkill_Efficiency(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
                mps = MightData.GetSkill_Power(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
                mps = MightData.GetSkill_Versatility(ability);
                if (mps != null)
                {
                    mps.level = 0;
                }
            }
            for (int i = 0; i < MightData.AllMightPowers.Count; i++)
            {
                for (int j = 0; j < MightData.AllMightPowers[i].TMabilityDefs.Count; j++)
                {
                    TMAbilityDef ability = (TMAbilityDef)MightData.AllMightPowers[i].TMabilityDefs[j];
                    RemovePawnAbility(ability);
                }
                MightData.AllMightPowers[i].learned = false;
                MightData.AllMightPowers[i].autocast = false;
            }
            MightUserLevel = 0;
            MightUserXP = 0;
            MightData.MightAbilityPoints = 0;
            //this.MightPowersInitialized = false;
            //base.IsInitialized = false;
            //CompAbilityUserMight.MightAbilities = null;
            //this.MightData = null;
            AssignAbilities();
        }

        public void RemoveTMagicHediffs()
        {
            List<Hediff> allHediffs = Pawn.health.hediffSet.hediffs.ToList();
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
            mightData = null;
            Initialized = false;
        }

        private void ClearPower(MightPower current)
        {
            Log.Message("Removing ability: " + current.abilityDef.defName.ToString());
            RemovePawnAbility(current.abilityDef);
            UpdateAbilities();
        }

        public void RemoveTraits()
        {
            List<Trait> traits = Pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].def == TorannMagicDefOf.Gladiator || traits[i].def == TorannMagicDefOf.Bladedancer || traits[i].def == TorannMagicDefOf.Ranger || traits[i].def == TorannMagicDefOf.Faceless ||
                    traits[i].def == TorannMagicDefOf.DeathKnight || traits[i].def == TorannMagicDefOf.TM_Psionic || traits[i].def == TorannMagicDefOf.TM_Sniper || traits[i].def == TorannMagicDefOf.TM_Monk ||
                    traits[i].def == TorannMagicDefOf.TM_Wayfarer || traits[i].def == TorannMagicDefOf.TM_Commander || traits[i].def == TorannMagicDefOf.TM_SuperSoldier)
                {
                    Log.Message("Removing trait " + traits[i].Label);
                    traits.Remove(traits[i]);
                    i--;
                }
            }
        }

        private void LoadPowers(Pawn pawn)
        {
            if (pawn.story.traits.HasTrait(TorannMagicDefOf.Gladiator))
            {
                foreach (MightPower currentG in MightData.MightPowersG)
                {
                    Log.Message("Removing ability: " + currentG.abilityDef.defName.ToString());
                    currentG.level = 0;
                    RemovePawnAbility(currentG.abilityDef);
                }
                
            }
        }

        public int MightAttributeEffeciencyLevel(string attributeName)
        {
            int result = 0;

            if (mightData != null && attributeName != null)
            {
                if (attributeName == "TM_Sprint_eff")
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Sprint.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = mightPowerSkill != null;
                    if (flag)
                    {
                        result = mightPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Fortitude_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Fortitude.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Grapple_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Grapple.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Cleave_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Cleave.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Whirlwind_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Whirlwind.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Headshot_eff")
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Headshot.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = mightPowerSkill != null;
                    if (flag)
                    {
                        result = mightPowerSkill.level;
                    }
                }
                if (attributeName == "TM_DisablingShot_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_DisablingShot.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_AntiArmor_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_AntiArmor.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_SeismicSlash_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_SeismicSlash.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_BladeSpin_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_BladeSpin.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_PhaseStrike_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_PhaseStrike.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_AnimalFriend_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_AnimalFriend.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_ArrowStorm_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_ArrowStorm.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Disguise_eff")
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Disguise.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = mightPowerSkill != null;
                    if (flag)
                    {
                        result = mightPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Mimic_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Reversal_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Reversal.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Transpose_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Transpose.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Possess_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Possess.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_PsionicBarrier_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_PsionicBarrier.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_PsionicBlast_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_PsionicBlast.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_PsionicDash_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_PsionicDash.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_PsionicStorm_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_WaveOfFear_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_WaveOfFear.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_Spite_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_Spite.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_GraveBlade_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_GraveBlade.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_TigerStrike_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_TigerStrike.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_DragonStrike_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_DragonStrike.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_ThunderStrike_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_ThunderStrike.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_FieldTraining_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_WayfarerCraft_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_StayAlert_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_StayAlert.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_MoveOut_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_MoveOut.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
                if (attributeName == "TM_HoldTheLine_eff")
                {
                    MightPowerSkill magicPowerSkill = MightData.MightPowerSkill_HoldTheLine.FirstOrDefault((MightPowerSkill x) => x.label == attributeName);
                    bool flag = magicPowerSkill != null;
                    if (flag)
                    {
                        result = magicPowerSkill.level;
                    }
                }
            }

            return result;
        }

        public float ActualHediffCost(TMAbilityDef mightDef)
        {
            float num = 1f;
            if (mightDef != null && MightData.GetSkill_Efficiency(mightDef) != null)
            {
                num = 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Efficiency(mightDef).level);
            }
            return mightDef.hediffCost * num;
        }

        public float ActualNeedCost(TMAbilityDef mightDef)
        {
            float num = 1f;
            if (mightDef != null && MightData.GetSkill_Efficiency(mightDef) != null)
            {
                num = 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Efficiency(mightDef).level);
            }
            return mightDef.needCost * num;
        }

        public float ActualChiCost(TMAbilityDef mightDef)
        {
            float num = mightDef.chiCost;
            num *= (1 - .06f * MightData.MightPowerSkill_Chi.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Chi_eff").level);          
            
            MightPowerSkill mps = MightData.GetSkill_Efficiency(mightDef);
            if (mps != null)
            {
                num *= (1 - (mightDef.efficiencyReductionPercent * mps.level));
            }
            return num;            
        }

        public float ActualStaminaCost(TMAbilityDef mightDef)
        {
            float adjustedStaminaCost = mightDef.staminaCost;
            if (mightDef.efficiencyReductionPercent != 0)
            {                
                if(mightDef == TorannMagicDefOf.TM_PistolWhip)
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Versatility(TorannMagicDefOf.TM_PistolSpec).level);
                }
                else if(mightDef == TorannMagicDefOf.TM_SuppressingFire)
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Efficiency(TorannMagicDefOf.TM_RifleSpec).level);
                }
                else if(mightDef == TorannMagicDefOf.TM_Mk203GL)
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Versatility(TorannMagicDefOf.TM_RifleSpec).level);
                }
                else if(mightDef == TorannMagicDefOf.TM_Buckshot)
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Efficiency(TorannMagicDefOf.TM_ShotgunSpec).level);
                }
                else if(mightDef == TorannMagicDefOf.TM_BreachingCharge)
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * MightData.GetSkill_Versatility(TorannMagicDefOf.TM_ShotgunSpec).level);
                }
                else if(Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless) && (mightDef != TorannMagicDefOf.TM_Possess && mightDef != TorannMagicDefOf.TM_Disguise && mightDef != TorannMagicDefOf.TM_Transpose &&
                    mightDef != TorannMagicDefOf.TM_Transpose_I && mightDef != TorannMagicDefOf.TM_Transpose_II && mightDef != TorannMagicDefOf.TM_Transpose_III && mightDef != TorannMagicDefOf.TM_Mimic && mightDef != TorannMagicDefOf.TM_Reversal))
                {
                    adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * mightData.GetSkill_Efficiency(TorannMagicDefOf.TM_Mimic).level);
                }
                else if(mightDef == TorannMagicDefOf.TM_AnimalFriend)
                {
                    return .5f * mightDef.staminaCost;
                }
                else if(mightDef == TorannMagicDefOf.TM_ProvisionerAura && Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_ProvisionerAuraHD))
                {
                    return 0f;
                }
                else if (mightDef == TorannMagicDefOf.TM_TaskMasterAura && Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_TaskMasterAuraHD))
                {
                    return 0f;
                }
                else if (mightDef == TorannMagicDefOf.TM_CommanderAura && Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_CommanderAuraHD))
                {
                    return 0f;
                }
                else
                {
                    MightPowerSkill mps = MightData.GetSkill_Efficiency(mightDef);
                    if (mps != null)
                    {
                        adjustedStaminaCost *= 1f - (mightDef.efficiencyReductionPercent * mps.level);
                    }
                }
            }
            else
            {
                if (mightDef == TorannMagicDefOf.TM_Sprint || mightDef == TorannMagicDefOf.TM_Sprint_I || mightDef == TorannMagicDefOf.TM_Sprint_II || mightDef == TorannMagicDefOf.TM_Sprint_III)
                {
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (G_Sprint_eff * (float)MightAttributeEffeciencyLevel("TM_Sprint_eff"));
                }
                if (mightDef == TorannMagicDefOf.TM_Grapple || mightDef == TorannMagicDefOf.TM_Grapple_I || mightDef == TorannMagicDefOf.TM_Grapple_II || mightDef == TorannMagicDefOf.TM_Grapple_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Grapple.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Grapple_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (G_Grapple_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Cleave)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Cleave.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Cleave_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (G_Cleave_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Whirlwind)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Whirlwind.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Whirlwind_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (G_Whirlwind_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Headshot)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Headshot.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Headshot_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (S_Headshot_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_DisablingShot || mightDef == TorannMagicDefOf.TM_DisablingShot_I || mightDef == TorannMagicDefOf.TM_DisablingShot_II || mightDef == TorannMagicDefOf.TM_DisablingShot_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_DisablingShot.FirstOrDefault((MightPowerSkill x) => x.label == "TM_DisablingShot_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (S_DisablingShot_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_AntiArmor)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_AntiArmor.FirstOrDefault((MightPowerSkill x) => x.label == "TM_AntiArmor_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (S_AntiArmor_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_SeismicSlash)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_SeismicSlash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_SeismicSlash_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (B_SeismicSlash_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_BladeSpin)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_BladeSpin.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BladeSpin_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (B_BladeSpin_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PhaseStrike || mightDef == TorannMagicDefOf.TM_PhaseStrike_I || mightDef == TorannMagicDefOf.TM_PhaseStrike_II || mightDef == TorannMagicDefOf.TM_PhaseStrike_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PhaseStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PhaseStrike_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (B_PhaseStrike_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_AnimalFriend)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_AnimalFriend.FirstOrDefault((MightPowerSkill x) => x.label == "TM_AnimalFriend_eff");
                    if (bondedPet != null)
                    {
                        adjustedStaminaCost = (mightDef.staminaCost - (mightDef.staminaCost * (R_AnimalFriend_eff * (float)mightPowerSkill.level)) / 2);
                    }
                    else
                    {
                        adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (R_AnimalFriend_eff * (float)mightPowerSkill.level);
                    }
                }
                if (mightDef == TorannMagicDefOf.TM_ArrowStorm || mightDef == TorannMagicDefOf.TM_ArrowStorm_I || mightDef == TorannMagicDefOf.TM_ArrowStorm_II || mightDef == TorannMagicDefOf.TM_ArrowStorm_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_ArrowStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ArrowStorm_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (R_ArrowStorm_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Disguise)
                {
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (F_Disguise_eff * (float)MightAttributeEffeciencyLevel("TM_Disguise_eff"));
                }
                if (mightDef == TorannMagicDefOf.TM_Transpose || mightDef == TorannMagicDefOf.TM_Transpose_I || mightDef == TorannMagicDefOf.TM_Transpose_II || mightDef == TorannMagicDefOf.TM_Transpose_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Transpose.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Transpose_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (F_Transpose_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Mimic)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (F_Mimic_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Reversal)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Reversal.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Reversal_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (F_Reversal_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Possess)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Possess.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Possess_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (F_Possess_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PsionicBarrier || mightDef == TorannMagicDefOf.TM_PsionicBarrier_Projected)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PsionicBarrier.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicBarrier_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (P_PsionicBarrier_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PsionicBlast || mightDef == TorannMagicDefOf.TM_PsionicBlast_I || mightDef == TorannMagicDefOf.TM_PsionicBlast_II || mightDef == TorannMagicDefOf.TM_PsionicBlast_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PsionicBlast.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicBlast_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (P_PsionicBlast_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PsionicDash)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PsionicDash.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicDash_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (P_PsionicDash_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PsionicStorm)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PsionicStorm.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PsionicStorm_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (P_PsionicStorm_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Spite || mightDef == TorannMagicDefOf.TM_Spite_I || mightDef == TorannMagicDefOf.TM_Spite_II || mightDef == TorannMagicDefOf.TM_Spite_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_Spite.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Spite_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (DK_Spite_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_WaveOfFear)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_WaveOfFear.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WaveOfFear_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (DK_WaveOfFear_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_GraveBlade || mightDef == TorannMagicDefOf.TM_GraveBlade_I || mightDef == TorannMagicDefOf.TM_GraveBlade_II || mightDef == TorannMagicDefOf.TM_GraveBlade_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_GraveBlade.FirstOrDefault((MightPowerSkill x) => x.label == "TM_GraveBlade_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (DK_GraveBlade_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_TigerStrike)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_TigerStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_TigerStrike_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (M_TigerStrike_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_DragonStrike)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_DragonStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_DragonStrike_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (M_DragonStrike_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_ThunderStrike)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_ThunderStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ThunderStrike_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (M_ThunderStrike_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_ProvisionerAura)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_ProvisionerAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ProvisionerAura_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_ProvisionerAura_eff * (float)mightPowerSkill.level);
                    if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_ProvisionerAuraHD))
                    {
                        return 0f;
                    }
                }
                if (mightDef == TorannMagicDefOf.TM_TaskMasterAura)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_TaskMasterAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_TaskMasterAura_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_TaskMasterAura_eff * (float)mightPowerSkill.level);
                    if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_TaskMasterAuraHD))
                    {
                        return 0f;
                    }
                }
                if (mightDef == TorannMagicDefOf.TM_CommanderAura)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_CommanderAura.FirstOrDefault((MightPowerSkill x) => x.label == "TM_CommanderAura_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_CommanderAura_eff * (float)mightPowerSkill.level);
                    if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_CommanderAuraHD))
                    {
                        return 0f;
                    }
                }
                if (mightDef == TorannMagicDefOf.TM_StayAlert || mightDef == TorannMagicDefOf.TM_StayAlert_I || mightDef == TorannMagicDefOf.TM_StayAlert_II || mightDef == TorannMagicDefOf.TM_StayAlert_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_StayAlert.FirstOrDefault((MightPowerSkill x) => x.label == "TM_StayAlert_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_StayAlert_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_MoveOut || mightDef == TorannMagicDefOf.TM_MoveOut_I || mightDef == TorannMagicDefOf.TM_MoveOut_II || mightDef == TorannMagicDefOf.TM_MoveOut_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_MoveOut.FirstOrDefault((MightPowerSkill x) => x.label == "TM_MoveOut_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_MoveOut_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_HoldTheLine || mightDef == TorannMagicDefOf.TM_HoldTheLine_I || mightDef == TorannMagicDefOf.TM_HoldTheLine_II || mightDef == TorannMagicDefOf.TM_HoldTheLine_III)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_HoldTheLine.FirstOrDefault((MightPowerSkill x) => x.label == "TM_HoldTheLine_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (C_HoldTheLine_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_PistolWhip)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_PistolSpec.FirstOrDefault((MightPowerSkill x) => x.label == "TM_PistolSpec_ver");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_PistolWhip_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_SuppressingFire)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_RifleSpec.FirstOrDefault((MightPowerSkill x) => x.label == "TM_RifleSpec_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_SuppressingFire_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Mk203GL)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_RifleSpec.FirstOrDefault((MightPowerSkill x) => x.label == "TM_RifleSpec_ver");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_Mk203GL_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_Buckshot)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_ShotgunSpec.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ShotgunSpec_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_Buckshot_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_BreachingCharge)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_ShotgunSpec.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ShotgunSpec_ver");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_BreachingCharge_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_CQC)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_CQC.FirstOrDefault((MightPowerSkill x) => x.label == "TM_CQC_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_CQC_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_FirstAid)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_FirstAid.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FirstAid_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_FirstAid_eff * (float)mightPowerSkill.level);
                }
                if (mightDef == TorannMagicDefOf.TM_60mmMortar)
                {
                    MightPowerSkill mightPowerSkill = MightData.MightPowerSkill_60mmMortar.FirstOrDefault((MightPowerSkill x) => x.label == "TM_60mmMortar_eff");
                    adjustedStaminaCost = mightDef.staminaCost - mightDef.staminaCost * (SS_60mmMortar_eff * (float)mightPowerSkill.level);
                }
            }
            if (spCost != 1f && (mightDef != TorannMagicDefOf.TM_ProvisionerAura && mightDef != TorannMagicDefOf.TM_CommanderAura && mightDef != TorannMagicDefOf.TM_TaskMasterAura))
            {
                adjustedStaminaCost = adjustedStaminaCost * spCost;
            }            

            MightPowerSkill globalSkill = MightData.MightPowerSkill_global_seff.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_seff_pwr");
            if (globalSkill != null && (mightDef != TorannMagicDefOf.TM_ProvisionerAura && mightDef != TorannMagicDefOf.TM_CommanderAura && mightDef != TorannMagicDefOf.TM_TaskMasterAura))
            {
                adjustedStaminaCost -= (adjustedStaminaCost * (global_seff * globalSkill.level));
            }

            return Mathf.Max(adjustedStaminaCost, (.5f * mightDef.staminaCost));           

        }

        public override List<HediffDef> IgnoredHediffs()
        {
            return new List<HediffDef>
            {
                TorannMagicDefOf.TM_MagicUserHD
            };
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            //Moved to harmony patches

            //Pawn abilityUser = base.Pawn;
            //absorbed = false;

            //List<Hediff> list = new List<Hediff>();
            //List<Hediff> arg_32_0 = list;
            //IEnumerable<Hediff> arg_32_1;
            //if (abilityUser == null)
            //{
            //    arg_32_1 = null;
            //}
            //else
            //{
            //    Pawn_HealthTracker expr_1A = abilityUser.health;
            //    if (expr_1A == null)
            //    {
            //        arg_32_1 = null;
            //    }
            //    else
            //    {
            //        HediffSet expr_26 = expr_1A.hediffSet;
            //        arg_32_1 = ((expr_26 != null) ? expr_26.hediffs : null);
            //    }
            //}
            //arg_32_0.AddRange(arg_32_1);
            //Pawn expr_3E = abilityUser;
            //int? arg_84_0;
            //if (expr_3E == null)
            //{
            //    arg_84_0 = null;
            //}
            //else
            //{
            //    Pawn_HealthTracker expr_52 = expr_3E.health;
            //    if (expr_52 == null)
            //    {
            //        arg_84_0 = null;
            //    }
            //    else
            //    {
            //        HediffSet expr_66 = expr_52.hediffSet;
            //        arg_84_0 = ((expr_66 != null) ? new int?(expr_66.hediffs.Count<Hediff>()) : null);
            //    }
            //}
            //bool flag = (arg_84_0 ?? 0) > 0;
            //if (flag)
            //{
            //    foreach (Hediff current in list)
            //    {
            //        if(current.def ==  TorannMagicDefOf.TM_PsionicHD)
            //        {
            //            if(dinfo.Def == TMDamageDefOf.DamageDefOf.TM_PsionicInjury)
            //            {
            //                absorbed = true;
            //                dinfo.SetAmount(0);
            //                return;
            //            }
            //        }
            //        if (current.def == TorannMagicDefOf.TM_ReversalHD)
            //        {
            //            Pawn instigator = dinfo.Instigator as Pawn;
            //            if (instigator != null)
            //            {
            //                if (instigator.equipment != null && instigator.equipment.PrimaryEq != null)
            //                {
            //                    if (instigator.equipment.PrimaryEq.PrimaryVerb != null)
            //                    {
            //                        absorbed = true;
            //                        Vector3 drawPos = Pawn.DrawPos;
            //                        drawPos.x += ((instigator.DrawPos.x - drawPos.x) / 20f) + Rand.Range(-.2f, .2f);
            //                        drawPos.z += ((instigator.DrawPos.z - drawPos.z) / 20f) + Rand.Range(-.2f, .2f);
            //                        TM_MoteMaker.ThrowSparkFlashMote(drawPos, this.Pawn.Map, 2f);                                    
            //                        DoReversal(dinfo);
            //                        dinfo.SetAmount(0);
            //                        MightPowerSkill ver = this.MightData.MightPowerSkill_Reversal.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Reversal_ver");
            //                        if(ver.level > 0)
            //                        {
            //                            SiphonReversal(ver.level);
            //                        }
            //                        return;
            //                    }
            //                }
            //                else if(instigator.RaceProps.Animal && dinfo.Amount != 0 && (instigator.Position - this.Pawn.Position).LengthHorizontal <= 2)
            //                {
            //                    absorbed = true;
            //                    Vector3 drawPos = Pawn.DrawPos;
            //                    drawPos.x += ((instigator.DrawPos.x - drawPos.x) / 20f) + Rand.Range(-.2f, .2f);
            //                    drawPos.z += ((instigator.DrawPos.z - drawPos.z) / 20f) + Rand.Range(-.2f, .2f);
            //                    TM_MoteMaker.ThrowSparkFlashMote(drawPos, this.Pawn.Map, 2f);
            //                    DoMeleeReversal(dinfo);
            //                    dinfo.SetAmount(0);
            //                    MightPowerSkill ver = this.MightData.MightPowerSkill_Reversal.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Reversal_ver");
            //                    if (ver.level > 0)
            //                    {
            //                        SiphonReversal(ver.level);
            //                    }
            //                    return;
            //                }
            //            }
            //            Building instigatorBldg = dinfo.Instigator as Building;
            //            if(instigatorBldg != null)
            //            {
            //                if(instigatorBldg.def.Verbs != null)
            //                {
            //                    absorbed = true;
            //                    Vector3 drawPos = Pawn.DrawPos;
            //                    drawPos.x += ((instigatorBldg.DrawPos.x - drawPos.x) / 20f) + Rand.Range(-.2f, .2f);
            //                    drawPos.z += ((instigatorBldg.DrawPos.z - drawPos.z) / 20f) + Rand.Range(-.2f, .2f);
            //                    TM_MoteMaker.ThrowSparkFlashMote(drawPos, this.Pawn.Map, 2f);
            //                    DoReversal(dinfo);
            //                    dinfo.SetAmount(0);
            //                    MightPowerSkill ver = this.MightData.MightPowerSkill_Reversal.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Reversal_ver");
            //                    if (ver.level > 0)
            //                    {
            //                        SiphonReversal(ver.level);
            //                    }
            //                    return;
            //                }
            //            }
            //        }                   
            //        if (fortitudeMitigationDelay < this.age )
            //        {
            //            if (current.def == TorannMagicDefOf.TM_HediffFortitude)
            //            {
            //                MightPowerSkill pwr = this.MightData.MightPowerSkill_Fortitude.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Fortitude_pwr");
            //                MightPowerSkill ver = this.MightData.MightPowerSkill_Fortitude.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Fortitude_ver");
            //                absorbed = true;
            //                int mitigationAmt = 5 + pwr.level;
                            
            //                if (ModOptions.Settings.Instance.AIHardMode && !abilityUser.IsColonist)
            //                {
            //                    mitigationAmt = 8;
            //                }
            //                float actualDmg;
            //                float dmgAmt = dinfo.Amount;
            //                this.Stamina.GainNeed((.01f * dmgAmt) + (.005f * (float)ver.level));
            //                if (dmgAmt < mitigationAmt)
            //                {
            //                    actualDmg = 0;
            //                    return;
            //                }
            //                else
            //                {
            //                    actualDmg = dmgAmt - mitigationAmt;
            //                }
            //                fortitudeMitigationDelay = this.age + 5;
            //                dinfo.SetAmount(actualDmg);
            //                abilityUser.TakeDamage(dinfo);
            //                return;
            //            }
            //            if (current.def == TorannMagicDefOf.TM_MindOverBodyHD)
            //            {
            //                MightPowerSkill ver = this.MightData.MightPowerSkill_MindOverBody.FirstOrDefault((MightPowerSkill x) => x.label == "TM_MindOverBody_ver");
            //                absorbed = true;
            //                int mitigationAmt = Mathf.Clamp((7 + (2 * ver.level) - Mathf.RoundToInt(totalApparelWeight/2)), 0, 13);
                            
            //                if (ModOptions.Settings.Instance.AIHardMode && !abilityUser.IsColonist)
            //                {
            //                    mitigationAmt = 10;
            //                }
            //                float actualDmg;
            //                float dmgAmt = dinfo.Amount;
            //                if (dmgAmt < mitigationAmt)
            //                {
            //                    Vector3 drawPos = this.Pawn.DrawPos;
            //                    Thing instigator = dinfo.Instigator;
            //                    if (instigator != null && instigator.DrawPos != null)
            //                    {
            //                        float drawAngle = (instigator.DrawPos - drawPos).AngleFlat();
            //                        drawPos.x += Mathf.Clamp(((instigator.DrawPos.x - drawPos.x) / 5f) + Rand.Range(-.1f, .1f), -.45f, .45f);
            //                        drawPos.z += Mathf.Clamp(((instigator.DrawPos.z - drawPos.z) / 5f) + Rand.Range(-.1f, .1f), -.45f, .45f);
            //                        TM_MoteMaker.ThrowSparkFlashMote(drawPos, this.Pawn.Map, 1f);
            //                    }
            //                    actualDmg = 0;
            //                    return;
            //                }
            //                else
            //                {
            //                    actualDmg = dmgAmt - mitigationAmt;
            //                }
            //                fortitudeMitigationDelay = this.age + 6;
            //                dinfo.SetAmount(actualDmg);
            //                abilityUser.TakeDamage(dinfo);
            //                return;
            //            }
            //        }
            //    }
            //}
            //list.Clear();
            //list = null;
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        public void DoMeleeReversal(DamageInfo dinfo)
        {
            TM_Action.DoMeleeReversal(dinfo, Pawn);          
        }

        public void DoReversal(DamageInfo dinfo)
        {
            TM_Action.DoReversal(dinfo, Pawn);       
        }

        public void SiphonReversal(int verVal)
        {
            Pawn pawn = Pawn;
            CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
            comp.Stamina.CurLevel += (.015f * verVal);         
            int num = 1 + verVal;
            
            int numberOfInjuriesPerPart = !pawn.IsColonist && ModOptions.Settings.Instance.AIHardMode ? 5 : 1 + verVal;

            IEnumerable<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury => injury.CanHealNaturally())
                .DistinctBy(injury => injury.Part, numberOfInjuriesPerPart)
                .Take(1 + verVal);

            float amountToHeal = pawn.IsColonist ? 2.0f + verVal : 20.0f + verVal * 3f;
            foreach (Hediff_Injury injury in injuries)
            {
                injury.Heal(amountToHeal);
                TM_MoteMaker.ThrowRegenMote(pawn.Position.ToVector3Shifted(), pawn.Map, .6f);
                TM_MoteMaker.ThrowRegenMote(pawn.Position.ToVector3Shifted(), pawn.Map, .4f);
            }

        }

        public void GiveReversalJob(DamageInfo dinfo)  // buggy AF due to complications with CompDeflector
        {
            try
            {
                Pawn pawn;
                bool flag = (pawn = (dinfo.Instigator as Pawn)) != null && dinfo.Weapon != null;
                if (flag)
                {
                    if (dinfo.Weapon.IsMeleeWeapon || dinfo.WeaponBodyPartGroup != null)
                    {                        
                        reversal_dinfo = new DamageInfo(dinfo.Def, dinfo.Amount, dinfo.ArmorPenetrationInt, dinfo.Angle - 180, Pawn, dinfo.HitPart, dinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown);
                        reversalTarget = dinfo.Instigator;
                    }
                    else
                    {
                        Job job = new Job(CompDeflectorDefOf.CastDeflectVerb)
                        {
                            playerForced = true,
                            locomotionUrgency = LocomotionUrgency.Sprint
                        };
                        bool flag2 = pawn.equipment != null;
                        if (flag2)
                        {
                            CompEquippable primaryEq = pawn.equipment.PrimaryEq;
                            bool flag3 = primaryEq != null;
                            if (flag3)
                            {
                                bool flag4 = primaryEq.PrimaryVerb != null;
                                if (flag4)
                                {
                                    Verb_Deflected verb_Deflected = (Verb_Deflected)CopyAndReturnNewVerb(primaryEq.PrimaryVerb);
                                    //verb_Deflected = this.ReflectionHandler(verb_Deflected);
                                    //Log.Message("verb deflected with properties is " + verb_Deflected.ToString()); //throwing an error, so nothing is happening in jobdriver_castdeflectverb
                                    pawn = dinfo.Instigator as Pawn;
                                    job.targetA = pawn;
                                    job.verbToUse = verb_Deflected;
                                    job.killIncappedTarget = pawn.Downed;
                                    Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
        }

        public Verb CopyAndReturnNewVerb(Verb newVerb = null)
        {
            if (newVerb != null)
            {
                deflectVerb = newVerb as Verb_Deflected;
                deflectVerb = (Verb_Deflected)Activator.CreateInstance(typeof(Verb_Deflected));
                deflectVerb.caster = Pawn;
                

                //Initialize VerbProperties
                var newVerbProps = new VerbProperties
                {
                    //Copy values over to a new verb props
                    
                    hasStandardCommand = newVerb.verbProps.hasStandardCommand,
                    defaultProjectile = newVerb.verbProps.defaultProjectile,
                    range = newVerb.verbProps.range,
                    muzzleFlashScale = newVerb.verbProps.muzzleFlashScale,                    
                    warmupTime = 0,
                    defaultCooldownTime = 0,
                    soundCast = SoundDefOf.MetalHitImportant,
                    impactMote = newVerb.verbProps.impactMote,
                    label = newVerb.verbProps.label,
                    ticksBetweenBurstShots = 0,
                    rangedFireRulepack = RulePackDef.Named("TM_Combat_Reflection"),
                    accuracyLong = 70f * Rand.Range(1f, 2f),
                    accuracyMedium = 80f * Rand.Range(1f, 2f),
                    accuracyShort = 90f * Rand.Range(1f, 2f)
                };

                //Apply values
                deflectVerb.verbProps = newVerbProps;
            }
            else
            {
                if (deflectVerb != null) return deflectVerb;
                deflectVerb = (Verb_Deflected)Activator.CreateInstance(typeof(Verb_Deflected));
                deflectVerb.caster = Pawn;
                deflectVerb.verbProps = newVerb.verbProps;
            }
            return deflectVerb;
        }

        public Verb_Deflected ReflectionHandler(Verb_Deflected newVerb)
        {
            VerbProperties verbProperties = new VerbProperties
            {
                hasStandardCommand = newVerb.verbProps.hasStandardCommand,
                defaultProjectile = newVerb.verbProps.defaultProjectile,
                range = newVerb.verbProps.range,
                muzzleFlashScale = newVerb.verbProps.muzzleFlashScale,
                warmupTime = 0f,
                defaultCooldownTime = 0f,
                soundCast = SoundDefOf.MetalHitImportant,
                accuracyLong = 70f * Rand.Range(1f, 2f),
                accuracyMedium = 80f * Rand.Range(1f, 2f),
                accuracyShort = 90f * Rand.Range(1f, 2f)
            };

            newVerb.verbProps = verbProperties;
            return newVerb;
        }

        public void ResolveReversalDamage()
        {
            reversalTarget.TakeDamage(reversal_dinfo);
            reversalTarget = null;
        }

        public void ResolveMightUseEvents()
        {
            List<TM_EventRecords> tmpList = new List<TM_EventRecords>();
            tmpList.Clear();
            foreach (TM_EventRecords ev in MightUsed)
            {
                if (Find.TickManager.TicksGame - 60000 > ev.eventTick)
                {
                    tmpList.Add(ev);
                }
            }
            foreach (TM_EventRecords rem_ev in tmpList)
            {
                MightUsed.Remove(rem_ev);
            }
        }

        

        public void ResolveAIAutoCast()
        {
            
            if (ModOptions.Settings.Instance.AICasting && Pawn.jobs != null && Pawn.CurJob != null && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilityVerb && Pawn.CurJob.def != TorannMagicDefOf.TMCastAbilitySelf && Pawn.CurJob.def != JobDefOf.Ingest && Pawn.CurJob.def != JobDefOf.ManTurret && Pawn.GetPosture() == PawnPosture.Standing)
            {
                //Log.Message("pawn " + this.Pawn.LabelShort + " current job is " + this.Pawn.CurJob.def.defName);
                bool castSuccess = false;
                if (Stamina != null && Stamina.CurLevelPercentage >= ModOptions.Settings.Instance.autocastMinThreshold)
                {
                    foreach (MightPower mp in MightData.AllMightPowersWithSkills)
                    {
                        if (mp.learned && mp.autocasting != null && mp.autocasting.mightUser && mp.autocasting.AIUsable)
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
                                if (mp.autocasting.type == TMDefs.AutocastType.OnTarget && Pawn.TargetCurrentlyAimingAt != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
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
                                            if (targetPawn.Downed)
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
                                        //if(targetThing is Pawn)
                                        //{
                                        //    Pawn targetPawn = targetThing as Pawn;
                                        //    if(targetPawn.IsPrisoner)
                                        //    {
                                        //        continue;
                                        //    }
                                        //}
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnSelf)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
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
                                        AutoCast.CombatAbility_OnSelf.Evaluate(this, tmad, ability, mp, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnCell && Pawn.CurJob.targetA != null)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
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
                                        AutoCast.CombatAbility_OnCell.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
                                    }
                                }
                                if (mp.autocasting.type == TMDefs.AutocastType.OnNearby)
                                {
                                    LocalTargetInfo localTarget = TM_Calc.GetAutocastTarget(Pawn, mp.autocasting, Pawn.TargetCurrentlyAimingAt);
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
                                            if (targetPawn.Downed)
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
                                        //if (targetThing is Pawn)
                                        //{
                                        //    Pawn targetPawn = targetThing as Pawn;
                                        //    if (targetPawn.IsPrisoner)
                                        //    {
                                        //        continue;
                                        //    }
                                        //}
                                        if (!mp.autocasting.ValidConditions(Pawn, targetThing))
                                        {
                                            continue;
                                        }
                                        AutoCast.CombatAbility_OnTarget.TryExecute(this, tmad, ability, mp, targetThing, mp.autocasting.minRange, out castSuccess);
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

        public void ResolveClassSkills()               
        {
            if (MightUserLevel >= 20 && (skill_Teach == false || !MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TeachMight).learned))
            {
                MightData.MightPowersStandalone.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_TeachMight).learned = true;
                AddPawnAbility(TorannMagicDefOf.TM_TeachMight);
                skill_Teach = true;
            }

            if (customClass != null && customClass.classHediff != null && !Pawn.health.hediffSet.HasHediff(customClass.classHediff))
            {
                HealthUtility.AdjustSeverity(Pawn, customClass.classHediff, customClass.hediffSeverity);
            }

            if (IsMightUser && !Pawn.Dead && !Pawn.Downed)
            {                
                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer))
                {
                    MightPowerSkill bladefocus_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_BladeFocus.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BladeFocus_pwr");

                    List<Trait> traits = Pawn.story.traits.allTraits;
                    for (int i = 0; i < traits.Count; i++)
                    {
                        if (traits[i].def.defName == "Bladedancer")
                        {
                            if (traits[i].Degree != bladefocus_pwr.level)
                            {
                                traits.Remove(traits[i]);
                                Pawn.story.traits.GainTrait(new Trait(TraitDef.Named("Bladedancer"), bladefocus_pwr.level, false));
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 2);
                                break;
                            }
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Gladiator) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_Fortitude)))
                {
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_HediffFortitude))
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_HediffFortitude, -5f);
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_HediffFortitude, 1f);
                    }                    
                }
                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_HediffSprint))
                {
                    Hediff rec = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_HediffSprint, false);
                    if (rec != null && rec.Severity != (.5f + MightData.MightPowerSkill_Sprint.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Sprint_pwr").level))
                    {
                        Pawn.health.RemoveHediff(rec);
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_HediffSprint, (.5f + MightData.MightPowerSkill_Sprint.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Sprint_pwr").level));
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger))
                {
                    MightPowerSkill rangertraining_pwr = MightData.MightPowerSkill_RangerTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_RangerTraining_pwr");

                    List<Trait> traits = Pawn.story.traits.allTraits;
                    for (int i = 0; i < traits.Count; i++)
                    {
                        if (traits[i].def == TorannMagicDefOf.Ranger)
                        {
                            if (traits[i].Degree != rangertraining_pwr.level)
                            {
                                traits.Remove(traits[i]);
                                Pawn.story.traits.GainTrait(new Trait(TraitDef.Named("Ranger"), rangertraining_pwr.level, false));
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 2);
                                break;
                            }
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper))
                {
                    MightPowerSkill sniperfocus_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_SniperFocus.FirstOrDefault((MightPowerSkill x) => x.label == "TM_SniperFocus_pwr");

                    List<Trait> traits = Pawn.story.traits.allTraits;
                    for (int i = 0; i < traits.Count; i++)
                    {
                        if (traits[i].def.defName == "TM_Sniper")
                        {
                            if (traits[i].Degree != sniperfocus_pwr.level)
                            {
                                traits.Remove(traits[i]);
                                Pawn.story.traits.GainTrait(new Trait(TorannMagicDefOf.TM_Sniper, sniperfocus_pwr.level, false));
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 2);
                            }
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic) || TM_ClassUtility.ClassHasAbility(TorannMagicDefOf.TM_PsionicAugmentation))
                {
                    if (!Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_PsionicHD"), false))
                    {
                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_PsionicHD"), 1f);
                    }
                }

                
                if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_BladeArt))) && !Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_BladeArtHD))
                {
                    MightPowerSkill bladeart_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_BladeArt.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BladeArt_pwr");

                    //HealthUtility.AdjustSeverity(this.Pawn, TorannMagicDefOf.TM_BladeArtHD, -5f);
                    HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BladeArtHD, (.5f) + bladeart_pwr.level);
                    if (!Pawn.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BladeArtHD, 4);
                    }
                }
                if ((Pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_BowTraining))))
                {                    
                    MightPowerSkill bowtraining_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_BowTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BowTraining_pwr");
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_BowTrainingHD))
                    {
                        //HealthUtility.AdjustSeverity(this.Pawn, TorannMagicDefOf.TM_BowTrainingHD, -5f);
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BowTrainingHD, (.5f) + bowtraining_pwr.level);
                        if (!Pawn.IsColonist && ModOptions.Settings.Instance.AIHardMode)
                        {
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BowTrainingHD, 4);
                        }
                    }
                }

                using (IEnumerator<Hediff> enumerator = Pawn.health.hediffSet.hediffs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Hediff rec = enumerator.Current;

                        if (rec.def == TorannMagicDefOf.TM_BladeArtHD && Pawn.IsColonist)
                        {
                            MightPowerSkill bladeart_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_BladeArt.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BladeArt_pwr");
                            if (rec.Severity < (float)(.5f + bladeart_pwr.level) || rec.Severity > (float)(.6f + bladeart_pwr.level))
                            {
                                HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BladeArtHD, -5f);
                                HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BladeArtHD, (.5f) + bladeart_pwr.level);
                                FleckMaker.ThrowDustPuff(Pawn.Position.ToVector3Shifted(), Pawn.Map, .6f);
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 1.6f);
                            }
                        }

                        if (rec.def == TorannMagicDefOf.TM_BowTrainingHD && Pawn.IsColonist)
                        {
                            MightPowerSkill bowtraining_pwr = Pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_BowTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_BowTraining_pwr");
                            if (rec.Severity < (float)(.5f + bowtraining_pwr.level) || rec.Severity > (float)(.6f + bowtraining_pwr.level))
                            {
                                HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BowTrainingHD, -5f);
                                HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_BowTrainingHD, (.5f) + bowtraining_pwr.level);
                                FleckMaker.ThrowDustPuff(Pawn.Position.ToVector3Shifted(), Pawn.Map, .6f);
                                FleckMaker.ThrowHeatGlow(Pawn.Position, Pawn.Map, 1.6f);
                            }
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight) || (customClass != null && (customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_Shroud) || customClass.classHediff == TorannMagicDefOf.TM_HateHD)))
                {
                    int hatePwr = MightData.MightPowerSkill_Shroud.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Shroud_pwr").level;
                    float sevSvr = 0;
                    Hediff hediff = null;
                    for (int h = 0; h < Pawn.health.hediffSet.hediffs.Count; h++)
                    {
                        if (Pawn.health.hediffSet.hediffs[h].def.defName.Contains("TM_HateH"))
                        {
                            hediff = Pawn.health.hediffSet.hediffs[h];
                        }
                    }

                    if (hediff != null)
                    {
                        sevSvr = hediff.Severity;
                        if (hatePwr == 5 && hediff.def.defName != "TM_HateHD_V")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD_V"), sevSvr);
                        }
                        else if (hatePwr == 4 && hediff.def.defName != "TM_HateHD_IV")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD_IV"), sevSvr);
                        }
                        else if (hatePwr == 3 && hediff.def.defName != "TM_HateHD_III")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD_III"), sevSvr);
                        }
                        else if (hatePwr == 2 && hediff.def.defName != "TM_HateHD_II")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD_II"), sevSvr);
                        }
                        else if (hatePwr == 1 && hediff.def.defName != "TM_HateHD_I")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD_I"), sevSvr);
                        }
                        else if (hatePwr == 0 && hediff.def.defName != "TM_HateHD")
                        {
                            Pawn.health.RemoveHediff(hediff);
                            HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD"), sevSvr);
                        }
                    }

                    if (!TM_Calc.HasHateHediff(Pawn))
                    {
                        HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HateHD"), 1);
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk) || (customClass != null && (customClass.classHediff == TorannMagicDefOf.TM_ChiHD || customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_Chi))))
                {
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_ChiHD, false))
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_ChiHD, 1f);
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_MindOverBody)))
                {
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MindOverBodyHD, false))
                    {
                        HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MindOverBodyHD, .5f);
                    }
                    else
                    {
                        Hediff mob = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_MindOverBodyHD, false);
                        int mobPwr = MightData.MightPowerSkill_MindOverBody.FirstOrDefault((MightPowerSkill x) => x.label == "TM_MindOverBody_pwr").level;
                        if (mobPwr == 3 && mob.Severity < 3)
                        {
                            Pawn.health.RemoveHediff(mob);
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MindOverBodyHD, 3.5f);
                        }
                        else if (mobPwr == 2 && mob.Severity < 2)
                        {
                            Pawn.health.RemoveHediff(mob);
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MindOverBodyHD, 2.5f);
                        }
                        else if (mobPwr == 1 && mob.Severity < 1)
                        {
                            Pawn.health.RemoveHediff(mob);
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MindOverBodyHD, 1.5f);
                        }
                        else if (mobPwr == 0 && mob.Severity >= 1)
                        {
                            Pawn.health.RemoveHediff(mob);
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_MindOverBodyHD, .5f);
                        }
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Wayfarer) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_FieldTraining)))
                {
                    int pwrVal = MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_pwr").level;
                    int verVal = MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_ver").level;
                    using (IEnumerator<Hediff> enumerator = Pawn.health.hediffSet.hediffs.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Hediff rec = enumerator.Current;
                            if (rec.def == TorannMagicDefOf.TM_HediffHeavyBlow && rec.Severity != (.95f + (.19f * pwrVal)))
                            {
                                rec.Severity = .95f + (.19f * pwrVal);
                            }
                            if (rec.def == TorannMagicDefOf.TM_HediffStrongBack)
                            {
                                if (verVal >= 8)
                                {
                                    if (rec.Severity != 2.5f)
                                    {
                                        rec.Severity = 2.5f;
                                    }
                                }
                                else if (verVal >= 3)
                                {
                                    if (rec.Severity != 1.5f)
                                    {
                                        //rec.Severity = 1.5f;
                                        //this.Pawn.health.RemoveHediff(rec);
                                        //HealthUtility.AdjustSeverity(this.Pawn, TorannMagicDefOf.TM_HediffStrongBack, 1.5f);
                                    }
                                }
                            }
                            if (rec.def == TorannMagicDefOf.TM_HediffThickSkin)
                            {
                                if (verVal >= 12)
                                {
                                    if (rec.Severity != 3.5f)
                                    {
                                        rec.Severity = 3.5f;
                                        //this.Pawn.health.RemoveHediff(rec);
                                        //HealthUtility.AdjustSeverity(this.Pawn, TorannMagicDefOf.TM_HediffThickSkin, 3.5f);
                                    }
                                }
                                else if (verVal >= 7)
                                {
                                    if (rec.Severity != 2.5f)
                                    {
                                        rec.Severity = 2.5f;
                                    }
                                }
                                else if (verVal >= 2)
                                {
                                    if (rec.Severity != 1.5f)
                                    {
                                        rec.Severity = 1.5f;
                                    }
                                }
                            }
                            if (rec.def == TorannMagicDefOf.TM_HediffFightersFocus)
                            {
                                if (verVal >= 1)
                                {
                                    if (rec.Severity != 1.5f)
                                    {
                                        rec.Severity = 1.5f;
                                    }
                                }
                            }
                            if (rec.def == TorannMagicDefOf.TM_HediffSprint)
                            {
                                if (rec.Severity != (.5f + (int)(pwrVal / 3)))
                                {
                                    rec.Severity = (.5f + (int)(pwrVal / 3));
                                }
                            }
                        }
                    }
                    if (verVal >= 6 && !skill_Legion)
                    {
                        skill_Legion = true;
                        AddPawnAbility(TorannMagicDefOf.TM_Legion);
                    }
                }

                if (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier) || (customClass != null && customClass.classHediff == TorannMagicDefOf.TM_SS_SerumHD))
                {
                    if (!Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SS_SerumHD, false))
                    {
                        if (!Pawn.IsColonist)
                        {
                            float range = Rand.Range(5f, 25f);
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_SS_SerumHD, range);
                        }
                        else
                        {
                            HealthUtility.AdjustSeverity(Pawn, TorannMagicDefOf.TM_SS_SerumHD, 2.2f);
                        }
                    }                    
                }

                if (Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_SS_SerumHD) && Pawn.Downed && nextSSTend < Find.TickManager.TicksGame && 
                    (Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier) || (customClass != null && customClass.classFighterAbilities.Contains(TorannMagicDefOf.TM_FirstAid))))
                {
                    Hediff_Injury wound = Pawn.health.hediffSet.GetHediffsTendable().RandomElement() as Hediff_Injury;
                    if (wound != null && wound.CanHealNaturally())
                    {
                        wound.Tended(Rand.Range(0, .3f), .3f);
                    }
                    nextSSTend = Find.TickManager.TicksGame + Rand.Range(6000, 8000);
                }

            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            return base.CompGetGizmosExtra();
        }

        private void ResolveSustainedSkills()
        {
            float _maxSP = 0;
            float _maxSPUpkeep = 0;
            float _spRegenRate = 0;
            float _spRegenRateUpkeep = 0;
            float _coolDown = 0;
            float _spCost = 0;
            float _xpGain = 0;
            float _arcaneRes = 0;
            float _arcaneDmg = 0;
            bool _arcaneSpectre = false;
            bool _phantomShift = false;
            float _arcalleumCooldown = 0f;

            //Determine trait adjustments
            IEnumerable<TMDefs.DefModExtension_TraitEnchantments> traitEnum = Pawn.story.traits.allTraits
                .Select((Trait t) => t.def.GetModExtension<TMDefs.DefModExtension_TraitEnchantments>());
            foreach (TMDefs.DefModExtension_TraitEnchantments e in traitEnum)
            {
                if (e != null)
                {
                    _maxSP += e.maxSP;
                    _spCost += e.spCost;
                    _spRegenRate += e.spRegenRate;
                    _coolDown += e.mightCooldown;
                    _xpGain += e.xpGain;
                    _arcaneRes += e.arcaneRes;
                    _arcaneDmg += e.combatDmg;
                }
            }

            //Determine hediff adjustments
            foreach (Hediff hd in Pawn.health.hediffSet.hediffs)
            {
                if (hd.def.GetModExtension<TMDefs.DefModExtension_HediffEnchantments>() != null)
                {
                    foreach (TMDefs.HediffEnchantment hdStage in hd.def.GetModExtension<TMDefs.DefModExtension_HediffEnchantments>().stages)
                    {
                        if (hd.Severity >= hdStage.minSeverity && hd.Severity < hdStage.maxSeverity)
                        {
                            TMDefs.DefModExtension_TraitEnchantments e = hdStage.enchantments;
                            if (e != null)
                            {
                                _maxSP += e.maxSP;
                                _spCost += e.spCost;
                                _spRegenRate += e.spRegenRate;
                                _coolDown += e.mightCooldown;
                                _xpGain += e.xpGain;
                                _arcaneRes += e.arcaneRes;
                                _arcaneDmg += e.arcaneDmg;
                            }
                            break;
                        }
                    }
                }
            }

            //Determine apparel and equipment enchantments
            List<Apparel> apparel = Pawn.apparel.WornApparel;
            if (apparel != null)
            {
                totalApparelWeight = 0;
                for (int i = 0; i < Pawn.apparel.WornApparelCount; i++)
                {
                    Enchantment.CompEnchantedItem item = apparel[i].GetComp<Enchantment.CompEnchantedItem>();
                    if (item != null)
                    {
                        if (item.HasEnchantment)
                        {
                            float enchantmentFactor = 1f;
                            totalApparelWeight += apparel[i].def.GetStatValueAbstract(StatDefOf.Mass, apparel[i].Stuff);
                            if (item.MadeFromEnchantedStuff)
                            {                                
                                Enchantment.CompProperties_EnchantedStuff compES = apparel[i].Stuff.GetCompProperties<Enchantment.CompProperties_EnchantedStuff>();
                                enchantmentFactor = compES.enchantmentBonusMultiplier;                                    

                                float arcalleumFactor = compES.arcalleumCooldownPerMass;
                                if (apparel[i].Stuff.defName == "TM_Arcalleum")
                                {
                                    _arcaneRes += .05f;
                                }
                                _arcalleumCooldown += (totalApparelWeight * (arcalleumFactor / 100));                                
                            }

                            _maxSP += item.maxMP * enchantmentFactor;
                            _spRegenRate += item.mpRegenRate * enchantmentFactor;
                            _coolDown += item.coolDown * enchantmentFactor;
                            _xpGain += item.xpGain * enchantmentFactor;
                            _spCost += item.mpCost * enchantmentFactor;
                            _arcaneRes += item.arcaneRes * enchantmentFactor;
                            _arcaneDmg += item.arcaneDmg * enchantmentFactor;

                            if (item.arcaneSpectre == true)
                            {
                                _arcaneSpectre = true;
                            }
                            if (item.phantomShift == true)
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

                        _maxSP += item.maxMP * enchantmentFactor;
                        _spRegenRate += item.mpRegenRate * enchantmentFactor;
                        _coolDown += item.coolDown * enchantmentFactor;
                        _xpGain += item.xpGain * enchantmentFactor;
                        _spCost += item.mpCost * enchantmentFactor;
                        _arcaneRes += item.arcaneRes * enchantmentFactor;
                        _arcaneDmg += item.arcaneDmg * enchantmentFactor;

                    }
                    if (Pawn.story != null && Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk) && Pawn.Faction != null && Pawn.Faction.HostileTo(Faction.OfPlayer))
                    {
                        ThingWithComps outItem;
                        Pawn.equipment.TryDropEquipment(Pawn.equipment.Primary, out outItem, Pawn.Position, true);
                    }
                }
            }

            //Determine active or sustained abilities            
            using (IEnumerator<Hediff> enumerator = Pawn.health.hediffSet.hediffs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Hediff rec = enumerator.Current;
                    TMAbilityDef ability = MightData.GetHediffAbility(rec);
                    if (ability != null)
                    {
                        MightPowerSkill skill = MightData.GetSkill_Efficiency(ability);
                        int level = 0;
                        if (skill != null)
                        {
                            level = skill.level;
                        }

                        _maxSPUpkeep += (ability.upkeepEnergyCost * (1f - (ability.upkeepEfficiencyPercent * level)));
                        _spRegenRateUpkeep += (ability.upkeepRegenCost * (1f - (ability.upkeepEfficiencyPercent * level)));
                        
                    }                    
                    if(rec.def == TorannMagicDefOf.TM_SS_SerumHD)
                    {
                        _spRegenRate += (float)(.1f * rec.CurStageIndex);
                        _arcaneRes += (float)(.15f * rec.CurStageIndex);
                        _arcaneDmg += (float)(.05f * rec.CurStageIndex);
                    }
                }
            }
            //Bonded animal upkeep
            if (bondedPet != null)
            {
                _maxSPUpkeep += (TorannMagicDefOf.TM_AnimalFriend.upkeepEnergyCost * (1f - (TorannMagicDefOf.TM_AnimalFriend.upkeepEfficiencyPercent * MightData.GetSkill_Efficiency(TorannMagicDefOf.TM_AnimalFriend).level)));
                if (bondedPet.Dead || bondedPet.Destroyed)
                {
                    if(bondedPet.Dead)
                    {
                        bondedPet.health.RemoveHediff(bondedPet.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_RangerBondHD, false));
                    }
                    Pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.RangerPetDied, null);
                    bondedPet = null;
                }
                else if (bondedPet.Faction != null && bondedPet.Faction != Pawn.Faction)
                {
                    //sold? punish evil
                    Pawn.needs.mood.thoughts.memories.TryGainMemory(TorannMagicDefOf.RangerSoldBondedPet, null);
                    bondedPet = null;
                }
                else if(!bondedPet.health.hediffSet.HasHediff(TorannMagicDefOf.TM_RangerBondHD))
                {
                    HealthUtility.AdjustSeverity(bondedPet, TorannMagicDefOf.TM_RangerBondHD, .5f);
                }
            }
            if(Pawn.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDef.Named("RangerSoldBondedPet")) > 0)
            {
                if(animalBondingDisabled == false)
                {
                    RemovePawnAbility(TorannMagicDefOf.TM_AnimalFriend);
                    animalBondingDisabled = true;
                }
            }
            else
            {
                if(animalBondingDisabled == true)
                {
                    AddPawnAbility(TorannMagicDefOf.TM_AnimalFriend);
                    animalBondingDisabled = false;
                }
            }

            if(MightData.MightAbilityPoints < 0)
            {
                MightData.MightAbilityPoints = 0;
            }      
            //Class and global bonuses

            _arcaneDmg += (.01f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_pwr").level);
            _arcaneRes += (.02f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_pwr").level);
            _spCost -= (.01f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_eff").level);
            _xpGain += (.02f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_eff").level);
            _coolDown -= (.01f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_ver").level);
            _spRegenRate += (.01f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_ver").level);
            _maxSP += (.02f * MightData.MightPowerSkill_WayfarerCraft.FirstOrDefault((MightPowerSkill x) => x.label == "TM_WayfarerCraft_ver").level);
            _maxSPUpkeep *= (1f - (.03f * MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_eff").level));
            _spRegenRateUpkeep *= (1f - (.03f * MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_eff").level));

            _maxSP += (.04f * MightData.MightPowerSkill_global_endurance.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_endurance_pwr").level);
            _spRegenRate += (.05f * MightData.MightPowerSkill_global_refresh.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_refresh_pwr").level);
            _spCost += (-.025f * MightData.MightPowerSkill_global_seff.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_seff_pwr").level);
            _arcaneDmg += (.05f * MightData.MightPowerSkill_global_strength.FirstOrDefault((MightPowerSkill x) => x.label == "TM_global_strength_pwr").level);
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

            //resolve upkeep costs            
            _maxSP -= (_maxSPUpkeep);
            _spRegenRate -= (_spRegenRateUpkeep);

            //finalize
            maxSP = Mathf.Clamp(1 + _maxSP, 0f, 5f);
            spRegenRate = 1f + _spRegenRate;
            coolDown = Mathf.Clamp(1f + _coolDown, .25f, 10f);
            xpGain = Mathf.Clamp(1f + _xpGain, 0.01f, 5f);
            spCost = Mathf.Clamp(1f + _spCost, 0.1f, 5f);
            arcaneRes = 1 + _arcaneRes;
            mightPwr = 1 + _arcaneDmg;

            if (IsMightUser && !TM_Calc.IsCrossClass(Pawn, false))
            {
                if (_maxSP != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_maxEnergy")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_maxEnergy"), .5f);
                }
                if (_spRegenRate != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_energyRegen")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_energyRegen"), .5f);
                }
                if (_coolDown != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_coolDown")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_coolDown"), .5f);
                }
                if (_xpGain != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_xpGain")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_xpGain"), .5f);
                }
                if (_spCost != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_energyCost")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_energyCost"), .5f);
                }
                if (_arcaneRes != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_dmgResistance")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_dmgResistance"), .5f);
                }
                if (_arcaneDmg != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_dmgBonus")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_dmgBonus"), .5f);
                }
                if (_arcalleumCooldown != 0 && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcalleumCooldown")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_arcalleumCooldown"), .5f);
                }
                if (_arcaneSpectre == true && !Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")))
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_arcaneSpectre"), .5f);
                }
                else if (_arcaneSpectre == false && Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")))
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_HediffEnchantment_arcaneSpectre")));
                }
                if (_phantomShift == true)
                {
                    HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("TM_HediffEnchantment_phantomShift"), .5f);
                }
                else if (_phantomShift == false && Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_HediffEnchantment_phantomShift")))
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_HediffEnchantment_phantomShift")));
                }
            }            
        }

        public void ResolveStamina()
        {
            if (Stamina == null)
            {
                Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_MightUserHD, false);
                bool flag2 = firstHediffOfDef != null;
                if (flag2)
                {
                    firstHediffOfDef.Severity = 1f;
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(TorannMagicDefOf.TM_MightUserHD, Pawn, null);
                    hediff.Severity = 1f;
                    Pawn.health.AddHediff(hediff, null, null);
                }
            }
        }
        public void ResolveMightPowers()
        {
            if (!mightPowersInitialized)
            {
                mightPowersInitialized = true;
            }
        }
        public void ResolveMightTab()
        {
            InspectTabBase inspectTabsx = Pawn.GetInspectTabs().FirstOrDefault((InspectTabBase x) => x.labelKey == "TM_TabMight");
            IEnumerable<InspectTabBase> inspectTabs = Pawn.GetInspectTabs();
            bool flag = inspectTabs != null && inspectTabs.Count<InspectTabBase>() > 0;
            if (flag)
            {         
                if (inspectTabsx == null)
                {
                    try
                    {
                        Pawn.def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Might)));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Concat(new object[]
                        {
                            "Could not instantiate inspector tab of type ",
                            typeof(ITab_Pawn_Might),
                            ": ",
                            ex
                        }));
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            //base.PostExposeData();
            Scribe_Values.Look<bool>(ref mightPowersInitialized, "mightPowersInitialized", false, false);
            Scribe_Collections.Look<Thing>(ref combatItems, "combatItems", LookMode.Reference);
            Scribe_Deep.Look(ref equipmentContainer, "equipmentContainer", new object[0]);
            Scribe_References.Look<Pawn>(ref bondedPet, "bondedPet", false);
            Scribe_Values.Look<bool>(ref skill_GearRepair, "skill_GearRepair", false, false);
            Scribe_Values.Look<bool>(ref skill_InnerHealing, "skill_InnerHealing", false, false);
            Scribe_Values.Look<bool>(ref skill_HeavyBlow, "skill_HeavyBlow", false, false);
            Scribe_Values.Look<bool>(ref skill_Sprint, "skill_Sprint", false, false);
            Scribe_Values.Look<bool>(ref skill_StrongBack, "skill_StrongBack", false, false);
            Scribe_Values.Look<bool>(ref skill_ThickSkin, "skill_ThickSkin", false, false);
            Scribe_Values.Look<bool>(ref skill_FightersFocus, "skill_FightersFocus", false, false);
            Scribe_Values.Look<bool>(ref skill_BurningFury, "skill_BurningFury", false, false);
            Scribe_Values.Look<bool>(ref skill_ThrowingKnife, "skill_ThrowingKnife", false, false);
            Scribe_Values.Look<bool>(ref skill_PommelStrike, "skill_PommelStrike", false, false);
            Scribe_Values.Look<bool>(ref skill_Legion, "skill_Legion", false, false);
            Scribe_Values.Look<bool>(ref skill_TempestStrike, "skill_TempestStrike", false, false);
            Scribe_Values.Look<bool>(ref skill_PistolWhip, "skill_PistolWhip", false, false);
            Scribe_Values.Look<bool>(ref skill_SuppressingFire, "skill_SuppressingFire", false, false);
            Scribe_Values.Look<bool>(ref skill_Mk203GL, "skill_Mk203GL", false, false);
            Scribe_Values.Look<bool>(ref skill_Buckshot, "skill_Buckshot", false, false);
            Scribe_Values.Look<bool>(ref skill_BreachingCharge, "skill_BreachingCharge", false, false);
            Scribe_Values.Look<bool>(ref skill_Teach, "skill_Teach", false, false);
            Scribe_Values.Look<int>(ref allowMeditateTick, "allowMeditateTick", 0, false);
            Scribe_Values.Look<bool>(ref deathRetaliating, "deathRetaliating", false, false);
            Scribe_Values.Look<bool>(ref canDeathRetaliate, "canDeathRetaliate", false, false);
            Scribe_Values.Look<int>(ref ticksTillRetaliation, "ticksTillRetaliation", 600, false);
            Scribe_Values.Look<bool>(ref useCleaveToggle, "useCleaveToggle", true, false);
            Scribe_Values.Look<bool>(ref useCQCToggle, "useCQCToggle", true, false);
            Scribe_Defs.Look<TMAbilityDef>(ref mimicAbility, "mimicAbility");
            Scribe_Values.Look<float>(ref maxSP, "maxSP", 1f, false);
            Scribe_Deep.Look<MightData>(ref mightData, "mightData", this);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                TM_PawnTracker.ResolveMightComp(this);
                Pawn abilityUser = Pawn;
                int index = TM_ClassUtility.CustomClassIndexOfBaseFighterClass(abilityUser.story.traits.allTraits);
                if (index >= 0)
                {
                    customClass = TM_ClassUtility.CustomClasses[index];
                    customIndex = index;

                    LoadCustomClassAbilities(customClass);                    
                }
                else
                {
                    bool flag40 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Gladiator);
                    if (flag40)
                    {
                        bool flag14 = !MightData.MightPowersG.NullOrEmpty<MightPower>();
                        if (flag14)
                        {
                            foreach (MightPower current3 in MightData.MightPowersG)
                            {
                                bool flag15 = current3.abilityDef != null;
                                if (flag15)
                                {
                                    if ((current3.abilityDef == TorannMagicDefOf.TM_Sprint || current3.abilityDef == TorannMagicDefOf.TM_Sprint_I || current3.abilityDef == TorannMagicDefOf.TM_Sprint_II || current3.abilityDef == TorannMagicDefOf.TM_Sprint_III))
                                    {
                                        if (current3.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Sprint);
                                        }
                                        else if (current3.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Sprint_I);
                                        }
                                        else if (current3.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Sprint_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Sprint_III);
                                        }
                                    }
                                    if ((current3.abilityDef == TorannMagicDefOf.TM_Grapple || current3.abilityDef == TorannMagicDefOf.TM_Grapple_I || current3.abilityDef == TorannMagicDefOf.TM_Grapple_II || current3.abilityDef == TorannMagicDefOf.TM_Grapple_III))
                                    {
                                        if (current3.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Grapple);
                                        }
                                        else if (current3.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Grapple_I);
                                        }
                                        else if (current3.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Grapple_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Grapple_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag40)
                    {
                        // Log.Message("Loading Gladiator Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_Fortitude);
                        //this.AddPawnAbility(TorannMagicDefOf.TM_Cleave);
                        AddPawnAbility(TorannMagicDefOf.TM_Whirlwind);
                    }
                    bool flag41 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper);
                    if (flag41)
                    {
                        bool flag17 = !MightData.MightPowersS.NullOrEmpty<MightPower>();
                        if (flag17)
                        {
                            foreach (MightPower current4 in MightData.MightPowersS)
                            {
                                bool flag18 = current4.abilityDef != null;
                                if (flag18)
                                {
                                    if ((current4.abilityDef == TorannMagicDefOf.TM_DisablingShot || current4.abilityDef == TorannMagicDefOf.TM_DisablingShot_I || current4.abilityDef == TorannMagicDefOf.TM_DisablingShot_II || current4.abilityDef == TorannMagicDefOf.TM_DisablingShot_III))
                                    {
                                        if (current4.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DisablingShot);
                                        }
                                        else if (current4.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DisablingShot_I);
                                        }
                                        else if (current4.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DisablingShot_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_DisablingShot_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag41)
                    {
                        //Log.Message("Loading Sniper Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_SniperFocus);
                        AddPawnAbility(TorannMagicDefOf.TM_Headshot);
                        AddPawnAbility(TorannMagicDefOf.TM_AntiArmor);
                        AddPawnAbility(TorannMagicDefOf.TM_ShadowSlayer);
                    }
                    bool flag42 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Bladedancer);
                    if (flag42)
                    {
                        bool flag19 = !MightData.MightPowersB.NullOrEmpty<MightPower>();
                        if (flag19)
                        {
                            foreach (MightPower current5 in MightData.MightPowersB)
                            {
                                bool flag20 = current5.abilityDef != null;
                                if (flag20)
                                {
                                    if ((current5.abilityDef == TorannMagicDefOf.TM_PhaseStrike || current5.abilityDef == TorannMagicDefOf.TM_PhaseStrike_I || current5.abilityDef == TorannMagicDefOf.TM_PhaseStrike_II || current5.abilityDef == TorannMagicDefOf.TM_PhaseStrike_III))
                                    {
                                        if (current5.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PhaseStrike);
                                        }
                                        else if (current5.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PhaseStrike_I);
                                        }
                                        else if (current5.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PhaseStrike_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PhaseStrike_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag42)
                    {
                        //Log.Message("Loading Bladedancer Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_BladeFocus);
                        //this.AddPawnAbility(TorannMagicDefOf.TM_BladeArt);
                        AddPawnAbility(TorannMagicDefOf.TM_SeismicSlash);
                        AddPawnAbility(TorannMagicDefOf.TM_BladeSpin);
                    }
                    bool flag43 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Ranger);
                    if (flag43)
                    {
                        bool flag21 = !MightData.MightPowersR.NullOrEmpty<MightPower>();
                        if (flag21)
                        {
                            foreach (MightPower current6 in MightData.MightPowersR)
                            {
                                bool flag22 = current6.abilityDef != null;
                                if (flag22)
                                {
                                    if ((current6.abilityDef == TorannMagicDefOf.TM_ArrowStorm || current6.abilityDef == TorannMagicDefOf.TM_ArrowStorm_I || current6.abilityDef == TorannMagicDefOf.TM_ArrowStorm_II || current6.abilityDef == TorannMagicDefOf.TM_ArrowStorm_III))
                                    {
                                        if (current6.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ArrowStorm);
                                        }
                                        else if (current6.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ArrowStorm_I);
                                        }
                                        else if (current6.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ArrowStorm_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_ArrowStorm_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag43)
                    {
                        //Log.Message("Loading Ranger Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_RangerTraining);
                        //this.AddPawnAbility(TorannMagicDefOf.TM_BowTraining);
                        AddPawnAbility(TorannMagicDefOf.TM_PoisonTrap);
                        AddPawnAbility(TorannMagicDefOf.TM_AnimalFriend);
                    }

                    bool flag44 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Faceless);
                    if (flag44)
                    {
                        bool flag21 = !MightData.MightPowersF.NullOrEmpty<MightPower>();
                        if (flag21)
                        {
                            foreach (MightPower current7 in MightData.MightPowersF)
                            {
                                bool flag22 = current7.abilityDef != null;
                                if (flag22)
                                {
                                    if ((current7.abilityDef == TorannMagicDefOf.TM_Transpose || current7.abilityDef == TorannMagicDefOf.TM_Transpose_I || current7.abilityDef == TorannMagicDefOf.TM_Transpose_II || current7.abilityDef == TorannMagicDefOf.TM_Transpose_III))
                                    {
                                        if (current7.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Transpose);
                                        }
                                        else if (current7.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Transpose_I);
                                        }
                                        else if (current7.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Transpose_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Transpose_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag44)
                    {
                        //Log.Message("Loading Faceless Abilities");
                        AddPawnAbility(TorannMagicDefOf.TM_Disguise);
                        AddPawnAbility(TorannMagicDefOf.TM_Mimic);
                        AddPawnAbility(TorannMagicDefOf.TM_Reversal);
                        AddPawnAbility(TorannMagicDefOf.TM_Possess);
                    }

                    bool flag45 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Psionic);
                    if (flag45)
                    {
                        bool flag21 = !MightData.MightPowersP.NullOrEmpty<MightPower>();
                        if (flag21)
                        {
                            foreach (MightPower current8 in MightData.MightPowersP)
                            {
                                bool flag22 = current8.abilityDef != null;
                                if (flag22)
                                {
                                    if ((current8.abilityDef == TorannMagicDefOf.TM_PsionicBlast || current8.abilityDef == TorannMagicDefOf.TM_PsionicBlast_I || current8.abilityDef == TorannMagicDefOf.TM_PsionicBlast_II || current8.abilityDef == TorannMagicDefOf.TM_PsionicBlast_III))
                                    {
                                        if (current8.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBlast);
                                        }
                                        else if (current8.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBlast_I);
                                        }
                                        else if (current8.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBlast_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBlast_III);
                                        }
                                    }
                                    if ((current8.abilityDef == TorannMagicDefOf.TM_PsionicBarrier || current8.abilityDef == TorannMagicDefOf.TM_PsionicBarrier_Projected))
                                    {
                                        if (current8.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBarrier);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_PsionicBarrier_Projected);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag45)
                    {
                        //Log.Message("Loading Psionic Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_PsionicBarrier);
                        AddPawnAbility(TorannMagicDefOf.TM_PsionicDash);
                        AddPawnAbility(TorannMagicDefOf.TM_PsionicStorm);
                    }

                    bool flag46 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.DeathKnight);
                    if (flag46)
                    {
                        bool flag21 = !MightData.MightPowersDK.NullOrEmpty<MightPower>();
                        if (flag21)
                        {
                            foreach (MightPower current9 in MightData.MightPowersDK)
                            {
                                bool flag22 = current9.abilityDef != null;
                                if (flag22)
                                {
                                    if ((current9.abilityDef == TorannMagicDefOf.TM_Spite || current9.abilityDef == TorannMagicDefOf.TM_Spite_I || current9.abilityDef == TorannMagicDefOf.TM_Spite_II || current9.abilityDef == TorannMagicDefOf.TM_Spite_III))
                                    {
                                        if (current9.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Spite);
                                        }
                                        else if (current9.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Spite_I);
                                        }
                                        else if (current9.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Spite_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_Spite_III);
                                        }
                                    }
                                    if ((current9.abilityDef == TorannMagicDefOf.TM_GraveBlade || current9.abilityDef == TorannMagicDefOf.TM_GraveBlade_I || current9.abilityDef == TorannMagicDefOf.TM_GraveBlade_II || current9.abilityDef == TorannMagicDefOf.TM_GraveBlade_III))
                                    {
                                        if (current9.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_GraveBlade);
                                        }
                                        else if (current9.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_GraveBlade_I);
                                        }
                                        else if (current9.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_GraveBlade_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_GraveBlade_III);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag46)
                    {
                        //Log.Message("Loading Death Knight Abilities");
                        AddPawnAbility(TorannMagicDefOf.TM_WaveOfFear);
                    }

                    bool flag47 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Monk);
                    if (flag47)
                    {
                        //Log.Message("Loading Monk Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_Chi);
                        AddPawnAbility(TorannMagicDefOf.TM_ChiBurst);
                        //this.AddPawnAbility(TorannMagicDefOf.TM_MindOverBody);
                        AddPawnAbility(TorannMagicDefOf.TM_Meditate);
                        AddPawnAbility(TorannMagicDefOf.TM_TigerStrike);
                        AddPawnAbility(TorannMagicDefOf.TM_DragonStrike);
                        AddPawnAbility(TorannMagicDefOf.TM_ThunderStrike);
                    }

                    bool flag48 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Commander);
                    if (flag48)
                    {
                        // Log.Message("Loading Commander Abilities");
                        AddPawnAbility(TorannMagicDefOf.TM_ProvisionerAura);
                        AddPawnAbility(TorannMagicDefOf.TM_TaskMasterAura);
                        AddPawnAbility(TorannMagicDefOf.TM_CommanderAura);
                    }
                    if (flag48)
                    {
                        bool flag14 = !MightData.MightPowersC.NullOrEmpty<MightPower>();
                        if (flag14)
                        {
                            foreach (MightPower current10 in MightData.MightPowersC)
                            {
                                bool flag15 = current10.abilityDef != null;
                                if (flag15)
                                {
                                    if ((current10.abilityDef == TorannMagicDefOf.TM_StayAlert || current10.abilityDef == TorannMagicDefOf.TM_StayAlert_I || current10.abilityDef == TorannMagicDefOf.TM_StayAlert_II || current10.abilityDef == TorannMagicDefOf.TM_StayAlert_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_StayAlert);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_StayAlert_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_StayAlert_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_StayAlert_III);
                                        }
                                    }
                                    if ((current10.abilityDef == TorannMagicDefOf.TM_MoveOut || current10.abilityDef == TorannMagicDefOf.TM_MoveOut_I || current10.abilityDef == TorannMagicDefOf.TM_MoveOut_II || current10.abilityDef == TorannMagicDefOf.TM_MoveOut_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MoveOut);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MoveOut_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MoveOut_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_MoveOut_III);
                                        }
                                    }
                                    if ((current10.abilityDef == TorannMagicDefOf.TM_HoldTheLine || current10.abilityDef == TorannMagicDefOf.TM_HoldTheLine_I || current10.abilityDef == TorannMagicDefOf.TM_HoldTheLine_II || current10.abilityDef == TorannMagicDefOf.TM_HoldTheLine_III))
                                    {
                                        if (current10.level == 0)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HoldTheLine);
                                        }
                                        else if (current10.level == 1)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HoldTheLine_I);
                                        }
                                        else if (current10.level == 2)
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HoldTheLine_II);
                                        }
                                        else
                                        {
                                            AddPawnAbility(TorannMagicDefOf.TM_HoldTheLine_III);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool flag49 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_SuperSoldier);
                    if (flag49)
                    {
                        //Log.Message("Loading Super Soldier Abilities");
                        //this.AddPawnAbility(TorannMagicDefOf.TM_CQC);
                        AddPawnAbility(TorannMagicDefOf.TM_FirstAid);
                        AddPawnAbility(TorannMagicDefOf.TM_60mmMortar);                        
                    }
                }
                if (equipmentContainer != null && equipmentContainer.Count > 0)
                {
                    //Thing outThing = new Thing();
                    try
                    {
                        //Log.Message("primary is " + this.Pawn.equipment.Primary);
                        //Log.Message("equipment container is " + this.equipmentContainer[0]);                        
                        for (int i = 0; i < Pawn.equipment.AllEquipmentListForReading.Count; i++)
                        {
                            ThingWithComps t = Pawn.equipment.AllEquipmentListForReading[i];
                            if (t.def.defName.Contains("Spec_Base"))
                            {
                                t.Destroy(DestroyMode.Vanish);
                            }
                        }
                        if(ModCheck.Validate.SimpleSidearms.IsInitialized())
                        {
                            ModCheck.SS.ClearWeaponMemory(Pawn);
                        }
                        if (specWpnRegNum == -1)
                        {
                            if (MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_PistolSpec).learned)
                            {
                                TM_Action.DoAction_PistolSpecCopy(Pawn, equipmentContainer[0]);
                            }
                            else if (MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_RifleSpec).learned)
                            {
                                TM_Action.DoAction_RifleSpecCopy(Pawn, equipmentContainer[0]);
                            }
                            else if (MightData.MightPowersSS.FirstOrDefault<MightPower>((MightPower x) => x.abilityDef == TorannMagicDefOf.TM_ShotgunSpec).learned)
                            {
                                TM_Action.DoAction_ShotgunSpecCopy(Pawn, equipmentContainer[0]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Message("exception on load: " + ex);
                        //do nothing
                    }
                }
                UpdateAutocastDef();
                InitializeSkill();
                //base.UpdateAbilities();
            }            
        }

        public void LoadCustomClassAbilities(TMDefs.TM_CustomClass cc, Pawn fromPawn = null)
        {
            for (int i = 0; i < cc.classFighterAbilities.Count; i++)
            {
                TMAbilityDef ability = cc.classFighterAbilities[i];
                MightData fromData = null;
                if (fromPawn != null)
                {
                    fromData = fromPawn.GetCompAbilityUserMight().MightData;
                }
                if (fromData != null)
                {
                    foreach (MightPower fp in fromData.AllMightPowers)
                    {
                        if (fp.learned && cc.classFighterAbilities.Contains(fp.abilityDef))
                        {
                            MightPower mp = MightData.AllMightPowers.FirstOrDefault((MightPower x) => x.abilityDef == fp.TMabilityDefs[0]);
                            if (mp != null)
                            {
                                mp.learned = true;
                                mp.level = fp.level;
                            }
                        }
                    }
                }

                for (int j = 0; j < MightData.AllMightPowers.Count; j++)
                {
                    if (MightData.AllMightPowers[j].TMabilityDefs.Contains(ability) && MightData.AllMightPowers[j].learned)
                    {
                        if (ability.shouldInitialize)
                        {
                            int level = MightData.AllMightPowers[j].level;
                            AddPawnAbility(MightData.AllMightPowers[j].TMabilityDefs[level]);
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

        public void AddAdvancedClass(TMDefs.TM_CustomClass ac, Pawn fromPawn = null)
        {
            if (ac != null && ac.isFighter && ac.isAdvancedClass)
            {
                if (!AdvancedClasses.Contains(ac))
                {
                    AdvancedClasses.Add(ac);
                }
                else // clear all abilities and re-add
                {
                    foreach (TMAbilityDef ability in ac.classFighterAbilities)
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
                if (fromPawn != null)
                {
                    MightData fromData = fromPawn.GetCompAbilityUserMight().MightData;
                    if (fromData != null)
                    {
                        foreach (TMAbilityDef ability in ac.classMageAbilities)
                        {
                            MightPowerSkill mps_e = MightData.GetSkill_Efficiency(ability);
                            MightPowerSkill fps_e = fromData.GetSkill_Efficiency(ability);
                            if (mps_e != null && fps_e != null)
                            {
                                mps_e.level = fps_e.level;
                            }
                            MightPowerSkill mps_p = MightData.GetSkill_Power(ability);
                            MightPowerSkill fps_p = fromData.GetSkill_Power(ability);
                            if (mps_p != null && fps_p != null)
                            {
                                mps_p.level = fps_p.level;
                            }
                            MightPowerSkill mps_v = MightData.GetSkill_Versatility(ability);
                            MightPowerSkill fps_v = fromData.GetSkill_Versatility(ability);
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

        public void RemoveAdvancedClass(TMDefs.TM_CustomClass ac)
        {
            for (int i = 0; i < ac.classFighterAbilities.Count; i++)
            {
                TMAbilityDef ability = ac.classFighterAbilities[i];

                for (int j = 0; j < MightData.AllMightPowers.Count; j++)
                {
                    MightPower power = MightData.AllMightPowers[j];
                    if (power.abilityDef == ability)
                    {                        
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
            if (ac != null && ac.isFighter && ac.isAdvancedClass)
            {
                foreach (TMAbilityDef ability in ac.classFighterAbilities)
                {
                    MightPowerSkill mps_e = MightData.GetSkill_Efficiency(ability);
                    if (mps_e != null)
                    {
                        mps_e.level = 0;
                    }
                    MightPowerSkill mps_p = MightData.GetSkill_Power(ability);
                    if (mps_p != null)
                    {
                        mps_p.level = 0;
                    }
                    MightPowerSkill mps_v = MightData.GetSkill_Versatility(ability);
                    if (mps_v != null)
                    {
                        mps_v.level = 0;
                    }
                }
            }
            if (AdvancedClasses.Contains(ac))
            {
                AdvancedClasses.Remove(ac);
            }
        }

        public void UpdateAutocastDef()
        {
            IEnumerable <TM_CustomPowerDef> mpDefs = TM_Data.CustomFighterPowerDefs();
            if (IsMightUser && MightData != null && MightData.MightPowersCustom != null)
            {
                foreach (MightPower mp in MightData.MightPowersCustom)
                {
                    foreach (TM_CustomPowerDef mpDef in mpDefs)
                    {
                        if (mpDef.customPower.abilityDefs.FirstOrDefault().ToString() == mp.GetAbilityDef(0).ToString())
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
                if (key == "wayfarer")
                {
                    Command_Action itemWayfarer = new Command_Action
                    {

                        action = new Action(delegate
                        {
                            TM_Action.PromoteWayfarer(p);
                        }),
                        Order = 52,
                        defaultLabel = TM_TextPool.TM_PromoteWayfarer,
                        defaultDesc = TM_TextPool.TM_PromoteWayfarerDesc,
                        icon = ContentFinder<Texture2D>.Get("UI/wayfarer", true),
                    };
                    gizmoCommands.Add(key, itemWayfarer);
                }
                if(key == "cleave")
                {
                    String toggle = "cleave";
                    String label = "TM_CleaveEnabled".Translate();
                    String desc = "TM_CleaveToggleDesc".Translate();
                    if (!useCleaveToggle)
                    {
                        toggle = "cleavetoggle_off";
                        label = "TM_CleaveDisabled".Translate();
                    }
                    Command_Toggle itemCleave = new Command_Toggle
                    {
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = -90,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle, true),
                        isActive = (() => useCleaveToggle),
                        toggleAction = delegate
                        {
                            useCleaveToggle = !useCleaveToggle;
                        }
                    };
                    gizmoCommands.Add(key, itemCleave);
                }
                if(key == "cqc")
                {
                    String toggle = "cqc";
                    String label = "TM_CQCEnabled".Translate();
                    String desc = "TM_CQCToggleDesc".Translate();
                    if (!useCQCToggle)
                    {
                        //toggle = "cqc_off";
                        label = "TM_CQCDisabled".Translate();
                    }
                    Command_Toggle itemCQC = new Command_Toggle
                    {
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = -90,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle, true),
                        isActive = (() => useCQCToggle),
                        toggleAction = delegate
                        {
                            useCQCToggle = !useCQCToggle;
                        }
                    };
                    gizmoCommands.Add(key, itemCQC);
                }
                if(key == "psiAugmentation")
                {
                    String toggle = "psionicaugmentation";
                    String label = "TM_AugmentationsEnabled".Translate();
                    String desc = "TM_AugmentationsToggleDesc".Translate();
                    if (!usePsionicAugmentationToggle)
                    {
                        toggle = "psionicaugmentation_off";
                        label = "TM_AugmentationsDisabled".Translate();
                    }
                    Command_Toggle item = new Command_Toggle
                    {
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = -90,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle, true),
                        isActive = (() => usePsionicAugmentationToggle),
                        toggleAction = delegate
                        {
                            usePsionicAugmentationToggle = !usePsionicAugmentationToggle;
                        }
                    };
                    gizmoCommands.Add(key, item);
                }
                if(key == "psiMindAttack")
                {

                    String toggle2 = "psionicmindattack";
                    String label2 = "TM_MindAttackEnabled".Translate();
                    String desc2 = "TM_MindAttackToggleDesc".Translate();
                    if (!usePsionicMindAttackToggle)
                    {
                        toggle2 = "psionicmindattack_off";
                        label2 = "TM_MindAttackDisabled".Translate();
                    }
                    Command_Toggle item2 = new Command_Toggle
                    {
                        defaultLabel = label2,
                        defaultDesc = desc2,
                        Order = -89,
                        icon = ContentFinder<Texture2D>.Get("UI/" + toggle2, true),
                        isActive = (() => usePsionicMindAttackToggle),
                        toggleAction = delegate
                        {
                            usePsionicMindAttackToggle = !usePsionicMindAttackToggle;
                        }
                    };
                    gizmoCommands.Add(key, item2);
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
