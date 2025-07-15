using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AbilityUser;
using RimWorld;
using TorannMagic.ModOptions;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMElementalRift : Building
    {
        private float arcaneEnergyCur;
        private float arcaneEnergyMax = 1;

        private static readonly Material portalMat_1 = MaterialPool.MatFrom("Motes/rift_swirl1", false);
        private static readonly Material portalMat_2 = MaterialPool.MatFrom("Motes/rift_swirl2", false);
        private static readonly Material portalMat_3 = MaterialPool.MatFrom("Motes/rift_swirl3", false);

        private int ticksTillNextAssault;
        private float eventFrequencyMultiplier = 1;
        private float difficultyMultiplier = 1;
        private float STDMultiplier;
        private int ticksTillNextEvent;
        private int eventTimer;
        private int assaultTimer;
        private int matRng;
        private float matMagnitude;
        private int rnd;
        private int areaRadius = 1;
        private bool notifier;
        private IntVec2 centerLocation;

        private bool initialized;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref arcaneEnergyCur, "arcaneEnergyCur", 0f, false);
            Scribe_Values.Look<float>(ref arcaneEnergyMax, "arcaneEnergyMax", 0f, false);
            Scribe_Values.Look<int>(ref rnd, "rnd", 0, false);
            Scribe_Values.Look<int>(ref ticksTillNextAssault, "ticksTillNextAssault", 0, false);
            Scribe_Values.Look<int>(ref ticksTillNextEvent, "ticksTillNextEvent", 0, false);
            Scribe_Values.Look<int>(ref assaultTimer, "assaultTimer", 0, false);
            Scribe_Values.Look<int>(ref eventTimer, "eventTimer", 0, false);
            Scribe_Values.Look<int>(ref areaRadius, "areaRadius", 1, false);
            Scribe_Values.Look<float>(ref eventFrequencyMultiplier, "eventFrequencyMultiplier", 1f, false);
            Scribe_Values.Look<bool>(ref notifier, "notifier", false, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
        }

        private const int AssaultTickIntervalMin = 2600;
        private const int AssaultTickIntervalMax = 4000;
        private const int AssaultTickIntervalMinSubsequent = 2000;
        private const int AssaultTickIntervalMaxSubsequent = 3500;
        private const int EventTickIntervalMin = 160;
        private const int EventTickIntervalMax = 300;
        private const float DifficultyDivider = 20f;

        protected override void Tick()
        {
            InitializeIfNeeded();
            if (Find.TickManager.TicksGame % 8 == 0)
            {
                HandleVisualEffectsAndTimers();
            }

            HandleEventTrigger();
            HandleAssaultNotifier();
            HandleAssaultTrigger();
        }

        private void InitializeIfNeeded()
        {
            if (initialized) return;

            DetermineElementalType();
            BeginAssaultCondition();
            SpawnCycle();

            SettingsRef riftSettings = new ModOptions.SettingsRef();

            if (Find.Storyteller.difficulty.threatScale != 0)
            {
                STDMultiplier = (float)(Find.Storyteller.difficulty.threatScale / DifficultyDivider);
            }

            if (riftSettings.riftChallenge < 2f)
            {
                difficultyMultiplier = 1f;
            }
            else if (riftSettings.riftChallenge < 3f)
            {
                difficultyMultiplier = 0.85f;
            }
            else
            {
                difficultyMultiplier = 0.75f;
            }

            difficultyMultiplier -= STDMultiplier;
            ticksTillNextAssault = (int)(Rand.Range(AssaultTickIntervalMin, AssaultTickIntervalMax) *
                                         difficultyMultiplier);
            ticksTillNextEvent = (int)(Rand.Range(EventTickIntervalMin, EventTickIntervalMax) *
                                       eventFrequencyMultiplier);
            initialized = true;
        }

        private void HandleVisualEffectsAndTimers()
        {
            assaultTimer += 8;
            eventTimer += 8;
            matRng = Rand.RangeInclusive(0, 2);
            matMagnitude = 4 * arcaneEnergyMax;
            Vector3 rndVec = DrawPos;

            // Assume 'rnd' is defined elsewhere and used as intended
            if (rnd < 2) // Earth
            {
                rndVec.x += Rand.Range(-5, 5);
                rndVec.z += Rand.Range(-5, 5);
                FleckMaker.ThrowSmoke(rndVec, Map, Rand.Range(0.6f, 1.2f));
            }
            else if (rnd < 4) // Fire
            {
                rndVec.x += Rand.Range(-1.2f, 1.2f);
                rndVec.z += Rand.Range(-1.2f, 1.2f);
                TM_MoteMaker.ThrowFlames(rndVec, Map, Rand.Range(0.6f, 1f));
            }
            else if (rnd < 6) // Water
            {
                rndVec.x += Rand.Range(-2f, 2f);
                rndVec.z += Rand.Range(-2f, 2f);
                TM_MoteMaker.ThrowManaPuff(rndVec, Map, Rand.Range(0.6f, 1f));
            }
            else // Air
            {
                rndVec.x += Rand.Range(-2f, 2f);
                rndVec.z += Rand.Range(-2f, 2f);
                FleckMaker.ThrowLightningGlow(rndVec, Map, Rand.Range(0.6f, 0.8f));
            }
        }

        private void HandleEventTrigger()
        {
            if (eventTimer > ticksTillNextEvent)
            {
                DoMapEvent();
                eventTimer = 0;
                ticksTillNextEvent = (int)(Rand.Range(EventTickIntervalMin, EventTickIntervalMax) *
                                           eventFrequencyMultiplier);
            }
        }

        private void HandleAssaultNotifier()
        {
            if (!notifier && assaultTimer > (0.9f * ticksTillNextAssault))
            {
                Messages.Message("TM_AssaultPending".Translate(), MessageTypeDefOf.ThreatSmall);
                notifier = true;
            }
        }

        private void HandleAssaultTrigger()
        {
            if (assaultTimer > ticksTillNextAssault)
            {
                SpawnCycle();
                assaultTimer = 0;
                ticksTillNextAssault =
                    Mathf.RoundToInt(
                        Rand.Range(AssaultTickIntervalMinSubsequent, AssaultTickIntervalMaxSubsequent) *
                        difficultyMultiplier);
                notifier = false;
            }
        }

        public void DoMapEvent()
        {
            if (rnd < 2) //earth
            {
                //berserk random animal
                List<Pawn> animalList = Map.mapPawns.AllPawnsSpawned.ToList();
                for (int i = 0; i < animalList.Count; i++)
                {
                    int j = Rand.Range(0, animalList.Count);
                    if (animalList[j].RaceProps.Animal && !animalList[j].IsColonist &&
                        !animalList[j].def.defName.Contains("Elemental") && animalList[j].Faction == null)
                    {
                        animalList[j].mindState.mentalStateHandler
                            .TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        i = animalList.Count;
                    }
                }
            }
            else if (rnd < 4) //fire
            {
                FindGoodCenterLocation();
                if (Rand.Chance(.6f))
                {
                    SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Firestorm_Small,
                        centerLocation.ToIntVec3, Map);
                }
                else
                {
                    SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Firestorm_Tiny,
                        centerLocation.ToIntVec3, Map);
                }
            }
            else if (rnd < 6) //water
            {
                FindGoodCenterLocation();
                if (Rand.Chance(.6f))
                {
                    SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Blizzard_Small,
                        centerLocation.ToIntVec3, Map);
                }
                else
                {
                    SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Blizzard_Large,
                        centerLocation.ToIntVec3, Map);
                }
            }
            else //air
            {
                FindGoodCenterLocation();
                Map.weatherManager.eventHandler.AddEvent(
                    new WeatherEvent_LightningStrike(Map, centerLocation.ToIntVec3));
                ExplosionHelper.Explode(centerLocation.ToIntVec3, Map, areaRadius, DamageDefOf.Bomb, null,
                    Rand.Range(6, 16), 0, SoundDefOf.Thunder_OffMap, null, null, null, null, 0f, 1, null,
                    false, null, 0f, 1, 0.1f, true);
            }
        }

        private void FindGoodCenterLocation()
        {
            if (Map.Size.x <= 16 || Map.Size.z <= 16)
            {
                throw new Exception("Map too small for elemental assault");
            }

            for (int i = 0; i < 10; i++)
            {
                centerLocation = new IntVec2(Rand.Range(8, Map.Size.x - 8), Rand.Range(8, Map.Size.z - 8));
                if (IsGoodCenterLocation(centerLocation))
                {
                    break;
                }
            }
        }

        private bool IsGoodLocationForStrike(IntVec3 loc)
        {
            bool flag1 = loc.InBoundsWithNullCheck(Map);
            bool flag2 = loc.IsValid;
            bool flag3 = !loc.Fogged(Map);
            bool flag4 = loc.DistanceToEdge(Map) > 2;
            if (!flag1 || !flag2 || !flag3 || !flag4) return false;
            return !loc.Roofed(Map);
        }

        private bool IsGoodCenterLocation(IntVec2 loc)
        {
            int num = 0;
            int num2 = (int)(3.14159274f * (float)areaRadius * (float)areaRadius / 2f);
            foreach (IntVec3 current in GetPotentiallyAffectedCells(loc))
            {
                if (IsGoodLocationForStrike(current))
                {
                    num++;
                }

                if (num >= num2)
                {
                    break;
                }
            }

            return num >= num2 && (IsGoodLocationForStrike(loc.ToIntVec3));
        }

        [DebuggerHidden]
        private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
        {
            for (int x = center.x - areaRadius; x <= center.x + areaRadius; x++)
            {
                for (int z = center.z - areaRadius; z <= center.z + areaRadius; z++)
                {
                    if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <=
                        areaRadius * areaRadius)
                    {
                        yield return new IntVec3(x, 0, z);
                    }
                }
            }
        }

        public void BeginAssaultCondition()
        {
            List<GameCondition> currentGameConditions = Map.gameConditionManager.ActiveConditions;
            WeatherDef weatherDef = new WeatherDef();
            if (rnd < 2) //earth
            {
                if (!Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
                {
                    Map.GameConditionManager.RegisterCondition(
                        GameConditionMaker.MakeCondition(GameConditionDefOf.ToxicFallout, 500000));
                }

                eventFrequencyMultiplier = 4;
            }
            else if (rnd < 4) //fire
            {
                for (int i = 0; i < currentGameConditions.Count; i++)
                {
                    if (currentGameConditions[i].def == GameConditionDefOf.ColdSnap)
                    {
                        currentGameConditions[i].Duration = 0;
                        currentGameConditions[i].End();
                    }
                }

                if (!Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.HeatWave))
                {
                    Map.GameConditionManager.RegisterCondition(
                        GameConditionMaker.MakeCondition(GameConditionDefOf.HeatWave, 500000));
                }

                eventFrequencyMultiplier = .5f;
                areaRadius = 2;
            }
            else if (rnd < 6) //water
            {
                for (int i = 0; i < currentGameConditions.Count; i++)
                {
                    if (currentGameConditions[i].def == GameConditionDefOf.HeatWave)
                    {
                        currentGameConditions[i].Duration = 0;
                        currentGameConditions[i].End();
                    }
                }

                if (!Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ColdSnap))
                {
                    Map.GameConditionManager.RegisterCondition(
                        GameConditionMaker.MakeCondition(GameConditionDefOf.ColdSnap, 500000));
                }

                weatherDef = WeatherDef.Named("SnowHard");
                Map.weatherManager.TransitionTo(weatherDef);
                eventFrequencyMultiplier = .5f;
                areaRadius = 3;
            }
            else //air
            {
                weatherDef = WeatherDef.Named("RainyThunderstorm");
                Map.weatherManager.TransitionTo(weatherDef);
                eventFrequencyMultiplier = .4f;
                areaRadius = 2;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            //end conditions
            List<GameCondition> currentGameConditions = Map.gameConditionManager.ActiveConditions;
            foreach (GameCondition condition in currentGameConditions)
            {
                if (condition.def == GameConditionDefOf.ColdSnap ||
                    condition.def == GameConditionDefOf.HeatWave ||
                    condition.def == GameConditionDefOf.ToxicFallout)
                {
                    condition.End();
                }
            }

            base.Destroy(mode);
        }

        private void DetermineElementalType()
        {
            System.Random random = new System.Random();
            random = new System.Random();
            rnd = GenMath.RoundRandom(random.Next(0, 8));
        }

        private const float BaseWealthMultiplier = 0.7f;
        private static readonly float[] WealthThresholds = { 20000, 50000, 100000, 250000, 500000, 1000000 };
        private static readonly float[] WealthMultipliers = { 0.8f, 1f, 1.25f, 1.5f, 2f, 2.5f };
        private const float BaseGeChance = 0.007f;
        private const float BaseEChance = 0.035f;
        private const float BaseLeChance = 0.12f;

        private float CalculateWealthMultiplier(float wealth)
        {
            float multiplier = BaseWealthMultiplier;
            for (int i = 0; i < WealthThresholds.Length; i++)
            {
                if (wealth > WealthThresholds[i])
                    multiplier = WealthMultipliers[i];
            }

            return multiplier;
        }

        private float CalculateDifficultyModifier(float riftChallenge)
        {
            if (riftChallenge >= 3f)
                return 1.1f;
            else if (riftChallenge >= 2f)
                return 0.8f;
            else
                return 0.65f;
        }

        private void ShowEarthFX(IntVec3 cell)
        {
            FleckMaker.ThrowSmoke(cell.ToVector3(), Map, 1f);
            FleckMaker.ThrowMicroSparks(cell.ToVector3(), Map);
        }

        private void ShowFireFX(IntVec3 cell)
        {
            FleckMaker.ThrowSmoke(cell.ToVector3(), Map, 1);
            FleckMaker.ThrowMicroSparks(cell.ToVector3(), Map);
            FleckMaker.ThrowFireGlow(cell.ToVector3Shifted(), Map, 1);
            FleckMaker.ThrowHeatGlow(cell, Map, 1);
        }

        private void ShowWaterFX(IntVec3 cell)
        {
            FleckMaker.ThrowSmoke(cell.ToVector3(), Map, 1);
            for (int i = 0; i < 3; i++)
                FleckMaker.ThrowTornadoDustPuff(cell.ToVector3(), Map, 1, Color.blue);
        }


        private enum ElementType
        {
            Earth = 0,
            Fire = 1,
            Water = 2,
            MAX = 3
        }

        private void SpawnCycle()
        {
            float wealth = Map.PlayerWealthForStoryteller;
            float wealthMultiplier = CalculateWealthMultiplier(wealth);

            float riftChallengeSetting = ModOptions.Settings.Instance.riftChallenge;
            float riftChallenge = Mathf.Min(riftChallengeSetting, 1f);
            float difficultyMod = CalculateDifficultyModifier(riftChallenge);
            float geChance = riftChallengeSetting > 1
                ? BaseGeChance * wealthMultiplier * (difficultyMod * riftChallenge)
                : 0f;
            float eChance = BaseEChance * riftChallenge * (difficultyMod * wealthMultiplier);
            float leChance = BaseLeChance * riftChallenge * (difficultyMod * wealthMultiplier);

            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(Position, 3, true);

            foreach (IntVec3 currentCell in targets)
            {
                if (!currentCell.InBoundsWithNullCheck(Map) || !currentCell.IsValid ||
                    !currentCell.Walkable(Map))
                {
                    continue;
                }

                System.Random random = new System.Random();
                random = new System.Random();
                int rng = random.Next(0, (int)ElementType.MAX);
                ElementType elementType = (ElementType)rng;

                TrySpawnElemental(elementType, currentCell, geChance, eChance, leChance);
            }
        }

        private SpawnThings TrySpawnElemental(ElementType elementType, IntVec3 cell, float geChance,
            float eChance, float leChance)
        {
            Action<IntVec3> showFX;
            ThingDef greaterDef, elementalDef, lesserDef;
            string greaterKind, elementalKind, lesserKind;

            switch (elementType)
            {
                case ElementType.Earth:
                    showFX = ShowEarthFX;
                    greaterDef = TorannMagicDefOf.TM_GreaterEarth_ElementalR;
                    elementalDef = TorannMagicDefOf.TM_Earth_ElementalR;
                    lesserDef = TorannMagicDefOf.TM_LesserEarth_ElementalR;
                    greaterKind = "TM_GreaterEarth_Elemental";
                    elementalKind = "TM_Earth_Elemental";
                    lesserKind = "TM_LesserEarth_Elemental";
                    break;
                case ElementType.Fire:
                    showFX = ShowFireFX;
                    greaterDef = TorannMagicDefOf.TM_GreaterFire_ElementalR;
                    elementalDef = TorannMagicDefOf.TM_Fire_ElementalR;
                    lesserDef = TorannMagicDefOf.TM_LesserFire_ElementalR;
                    greaterKind = "TM_GreaterFire_Elemental";
                    elementalKind = "TM_Fire_Elemental";
                    lesserKind = "TM_LesserFire_Elemental";
                    break;
                case ElementType.Water:
                default:
                    showFX = ShowWaterFX;
                    greaterDef = TorannMagicDefOf.TM_GreaterWater_ElementalR;
                    elementalDef = TorannMagicDefOf.TM_Water_ElementalR;
                    lesserDef = TorannMagicDefOf.TM_LesserWater_ElementalR;
                    greaterKind = "TM_GreaterWater_Elemental";
                    elementalKind = "TM_Water_Elemental";
                    lesserKind = "TM_LesserWater_Elemental";
                    break;
            }

            if (Rand.Chance(geChance))
            {
                showFX(cell);
                return new SpawnThings
                {
                    def = greaterDef,
                    kindDef = PawnKindDef.Named(greaterKind)
                };
            }

            if (Rand.Chance(eChance))
            {
                showFX(cell);
                return new SpawnThings
                {
                    def = elementalDef,
                    kindDef = PawnKindDef.Named(elementalKind)
                };
            }

            if (Rand.Chance(leChance))
            {
                showFX(cell);
                return new SpawnThings
                {
                    def = lesserDef,
                    kindDef = PawnKindDef.Named(lesserKind)
                };
            }

            return null;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 vector = base.DrawPos;
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 s = new Vector3(matMagnitude, matMagnitude, matMagnitude);
            Matrix4x4 matrix = default(Matrix4x4);
            float angle = 0f;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            if (matRng == 0)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, portalMat_1, 0);
            }
            else if (matRng == 1)
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, portalMat_2, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, portalMat_3, 0);
            }
        }
    }
}