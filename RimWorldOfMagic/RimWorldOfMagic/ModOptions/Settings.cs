using System;
using Verse;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;


namespace TorannMagic.ModOptions
{
    public class Settings : ModSettings
    {
        public float xpMultiplier = 1f;
        public float needMultiplier = 1f;        
        public bool AICasting = true;
        public bool AIAggressiveCasting = true;
        public bool AIHardMode;        
        public float baseMageChance = 1f;
        public float baseFighterChance = 1f;
        public float advMageChance = 0.5f;
        public float advFighterChance = 0.5f;
        public float supportTraitChance = 0.1f;
        public float magicyteChance = .005f;        
        public float riftChallenge = 1f;
        public float demonAssaultChallenge = 1f;
        public float wanderingLichChallenge = 1f;        
        public bool showLevelUpMessage = true;        
        public bool unrestrictedBloodTypes = true;
        public float paracyteSoftCap = 50f;
        public bool paracyteMagesCount = true;
        public bool unrestrictedWeaponCopy;
        public float undeadUpkeepMultiplier = 1f;      
        public bool cameraSnap = true;
        public bool autoCreateAreas = true;

        //Draw Options
        public bool AIMarking = true;
        public bool AIFighterMarking;
        public bool AIFriendlyMarking;
        public bool showIconsMultiSelect = true;
        public bool showGizmo = true;
        public bool changeUndeadPawnAppearance = true;
        public bool changeUndeadAnimalAppearance = true;
        public bool showClassIconOnColonistBar = true;
        public float classIconSize = 1f;
        public bool shrinkIcons;
        public Vector2 iconPosition = Vector2.zero;
        public float cloakDepth;
        public float cloakDepthNorth;
        public bool offSetClothing;
        public float offsetApplyAtValue = .0288f;
        public float offsetMultiLayerClothingAmount = -.025387f;

        //Death Retaliation
        public float deathRetaliationChance = 1f;
        public float deathRetaliationDelayFactor = 1f;
        public bool deathRetaliationIsLethal = true;
        public float deathExplosionRadius = 3f;
        public int deathExplosionMin = 20;
        public int deathExplosionMax = 50;

        //autocast options
        public bool autocastEnabled = true;
        public bool autocastAnimals;
        public float autocastMinThreshold = 0.7f;
        public float autocastCombatMinThreshold = 0.2f;
        public int autocastEvaluationFrequency = 180;
        public bool autocastQueueing;

        //Golem options
        public bool showDormantFrames;
        public bool showGolemsOnColonistBar;
        public bool golemScreenShake = true;

        //class options
        public bool Arcanist = true;
        public bool FireMage = true;
        public bool IceMage = true;
        public bool LitMage = true;
        public bool Druid = true;
        public bool Paladin = true;
        public bool Necromancer = true;
        public bool Bard = true;
        public bool Priest = true;
        public bool Demonkin = true;
        public bool Geomancer = true;
        public bool Summoner = true;
        public bool Technomancer = true;
        public bool BloodMage = true;
        public bool Enchanter = true;
        public bool Chronomancer = true;
        public bool Wanderer = true;
        public bool ChaosMage = true;
        public bool Brightmage = true;
        public bool Shaman = true;
        public bool Golemancer = true;
        public bool Empath = true;

        public bool Gladiator = true;
        public bool Bladedancer = true;
        public bool Sniper = true;
        public bool Ranger = true;
        public bool Faceless = true;
        public bool Psionic = true;
        public bool DeathKnight = true;
        public bool Monk = true;
        public bool Wayfarer = true;
        public bool Commander = true;
        public bool SuperSoldier = true;
        public bool Shadow = true;
        public bool Apothecary = true;

        public bool ArcaneConduit = true;
        public bool ManaWell = true;
        public bool Boundless = true;
        public bool Enlightened = true;
        public bool Cursed = true;
        public bool FaeBlood = true;
        public bool GiantsBlood = true;

        //Faction settings
        public Dictionary<string, float> FactionFighterSettings = new Dictionary<string, float>();
        public Dictionary<string, float> FactionMageSettings = new Dictionary<string, float>();
        //Custom Class options
        public Dictionary<string, bool> CustomClass = new Dictionary<string, bool>();

        public static Settings Instance;

        public Settings()
        {
            Instance = this;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<float>(ref xpMultiplier, "xpMultiplier", 1f, false);
            Scribe_Values.Look<float>(ref needMultiplier, "needMultiplier", 1f, false);            
            Scribe_Values.Look<bool>(ref AICasting, "AICasting", true, false);
            Scribe_Values.Look<bool>(ref AIAggressiveCasting, "AIAggressiveCasting", true, false);
            Scribe_Values.Look<bool>(ref AIHardMode, "AIHardMode", false, false);
            Scribe_Values.Look<bool>(ref AIMarking, "AIMarking", false, false);
            Scribe_Values.Look<bool>(ref AIFighterMarking, "AIFighterMarking", false, false);
            Scribe_Values.Look<bool>(ref AIFriendlyMarking, "AIFriendlyMarking", false, false);
            Scribe_Values.Look<float>(ref baseMageChance, "baseMageChance", 1f, false);
            Scribe_Values.Look<float>(ref baseFighterChance, "baseFighterChance", 1f, false);
            Scribe_Values.Look<float>(ref advMageChance, "advMageChance", 0.5f, false);
            Scribe_Values.Look<float>(ref advFighterChance, "advFighterChance", 0.5f, false);
            Scribe_Values.Look<float>(ref supportTraitChance, "supportTraitChance", 0.1f, false);
            Scribe_Values.Look<float>(ref magicyteChance, "magicyteChance", 0.005f, false);
            Scribe_Values.Look<bool>(ref showIconsMultiSelect, "showIconsMultiSelect", true, false);
            Scribe_Values.Look<float>(ref riftChallenge, "riftChallenge", 1f, false);
            Scribe_Values.Look<float>(ref demonAssaultChallenge, "demonAssaultChallenge", 1f, false);
            Scribe_Values.Look<float>(ref wanderingLichChallenge, "wanderingLichChallenge", 1f, false);
            Scribe_Values.Look<float>(ref undeadUpkeepMultiplier, "undeadUpkeepMultiplier", 1f, false);
            Scribe_Values.Look<bool>(ref showGizmo, "showGizmo", true, false);
            Scribe_Values.Look<bool>(ref showLevelUpMessage, "showLevelUpMessage", true, false);
            Scribe_Values.Look<bool>(ref changeUndeadPawnAppearance, "changeUndeadPawnAppearance", true, false);
            Scribe_Values.Look<bool>(ref changeUndeadAnimalAppearance, "changeUndeadAnimalAppearance", true, false);
            Scribe_Values.Look<bool>(ref showClassIconOnColonistBar, "showClassIconOnColonistBar", true, false);
            Scribe_Values.Look<float>(ref classIconSize, "classIconSize", 1f, false);
            Scribe_Values.Look<bool>(ref unrestrictedBloodTypes, "unrestrictedBloodTypes", true, false);
            Scribe_Values.Look<float>(ref paracyteSoftCap, "paracyteSoftCap", 1f, false);
            Scribe_Values.Look<bool>(ref paracyteMagesCount, "paracyteMagesCount", true, false);
            Scribe_Values.Look<bool>(ref unrestrictedWeaponCopy, "unrestrictedWeaponCopy", false, false);
            Scribe_Values.Look<bool>(ref shrinkIcons, "shrinkIcons", false, false);
            Scribe_Values.Look<Vector2>(ref iconPosition, "iconPosition", default(Vector2));
            Scribe_Values.Look<bool>(ref cameraSnap, "cameraSnap", true, false);
            Scribe_Values.Look<float>(ref cloakDepth, "cloakDepth", 0f, false);
            Scribe_Values.Look<float>(ref cloakDepthNorth, "cloakDepthNorth", 0f, false);
            Scribe_Values.Look<bool>(ref offSetClothing, "offsetClothing", false, false);
            Scribe_Values.Look<float>(ref offsetApplyAtValue, "offsetApplyAtValue", .0288f, false);
            Scribe_Values.Look<float>(ref offsetMultiLayerClothingAmount, "offsetMultiLayerClothingAmount", -.025387f, false);
            Scribe_Values.Look<bool>(ref autoCreateAreas, "autoCreateAreas", true, false);

            Scribe_Values.Look<float>(ref deathExplosionRadius, "deathExplosionRadius", 3f, false);
            Scribe_Values.Look<int>(ref deathExplosionMin, "deathExplosionMin", 20, false);
            Scribe_Values.Look<int>(ref deathExplosionMax, "deathExplosionMax", 50, false);
            Scribe_Values.Look<float>(ref deathRetaliationChance, "deathRetaliationChance", 1f, false);
            Scribe_Values.Look<float>(ref deathRetaliationDelayFactor, "deathRetaliationDelayFactor", 1f, false);
            Scribe_Values.Look<bool>(ref deathRetaliationIsLethal, "deathRetaliationIsLethal", true, false);

            Scribe_Values.Look<bool>(ref autocastEnabled, "autocastEnabled", true, false);
            Scribe_Values.Look<float>(ref autocastMinThreshold, "autocastMinThreshold", 0.7f, false);
            Scribe_Values.Look<float>(ref autocastCombatMinThreshold, "autocastCombatMinThreshold", 0.2f, false);
            Scribe_Values.Look<int>(ref autocastEvaluationFrequency, "autocastEvaluationFrequency", 180, false);
            Scribe_Values.Look<bool>(ref autocastAnimals, "autocastAnimals", false, false);
            Scribe_Values.Look<bool>(ref autocastQueueing, "autocastQueueing", false, false);

            Scribe_Values.Look<bool>(ref showDormantFrames, "showDormantFrames", false, false);
            Scribe_Values.Look<bool>(ref showGolemsOnColonistBar, "showGolemsOnColonistBar", false, false);

            Scribe_Values.Look<bool>(ref Arcanist, "Arcanist", true, false);
            Scribe_Values.Look<bool>(ref FireMage, "FireMage", true, false);
            Scribe_Values.Look<bool>(ref IceMage, "IceMage", true, false);
            Scribe_Values.Look<bool>(ref LitMage, "LitMage", true, false);
            Scribe_Values.Look<bool>(ref Geomancer, "Geomancer", true, false);
            Scribe_Values.Look<bool>(ref Druid, "Druid", true, false);
            Scribe_Values.Look<bool>(ref Paladin, "Paladin", true, false);
            Scribe_Values.Look<bool>(ref Priest, "Priest", true, false);
            Scribe_Values.Look<bool>(ref Bard, "Bard", true, false);
            Scribe_Values.Look<bool>(ref Summoner, "Summoner", true, false);
            Scribe_Values.Look<bool>(ref Necromancer, "Necromancer", true, false);
            Scribe_Values.Look<bool>(ref Technomancer, "Technomancer", true, false);
            Scribe_Values.Look<bool>(ref Demonkin, "Demonkin", true, false);
            Scribe_Values.Look<bool>(ref BloodMage, "BloodMage", true, false);
            Scribe_Values.Look<bool>(ref Enchanter, "Enchanter", true, false);
            Scribe_Values.Look<bool>(ref Chronomancer, "Chronomancer", true, false);
            Scribe_Values.Look<bool>(ref Gladiator, "Gladiator", true, false);
            Scribe_Values.Look<bool>(ref Bladedancer, "Bladedancer", true, false);
            Scribe_Values.Look<bool>(ref Sniper, "Sniper", true, false);
            Scribe_Values.Look<bool>(ref Ranger, "Ranger", true, false);
            Scribe_Values.Look<bool>(ref Faceless, "Faceless", true, false);
            Scribe_Values.Look<bool>(ref Psionic, "Psionic", true, false);
            Scribe_Values.Look<bool>(ref DeathKnight, "DeathKnight", true, false);
            Scribe_Values.Look<bool>(ref Wanderer, "Wanderer", true, false);
            Scribe_Values.Look<bool>(ref Wayfarer, "Wayfarer", true, false);
            Scribe_Values.Look<bool>(ref ChaosMage, "ChaosMage", true, false);
            Scribe_Values.Look<bool>(ref Monk, "Monk", true, false);
            Scribe_Values.Look<bool>(ref Commander, "Commander", true, false);
            Scribe_Values.Look<bool>(ref SuperSoldier, "SuperSoldier", true, false);
            Scribe_Values.Look<bool>(ref Shadow, "Shadow", true, false);
            Scribe_Values.Look<bool>(ref Brightmage, "Brightmage", true, false);
            Scribe_Values.Look<bool>(ref Shaman, "Shaman", true, false);
            Scribe_Values.Look<bool>(ref Golemancer, "Golemancer", true, false);
            Scribe_Values.Look<bool>(ref Empath, "Empath", true, false);
            Scribe_Values.Look<bool>(ref Apothecary, "Apothecary", true, false);
            Scribe_Collections.Look(ref CustomClass, "CustomClass");
            Scribe_Values.Look<bool>(ref ManaWell, "ManaWell", true, false);
            Scribe_Values.Look<bool>(ref ArcaneConduit, "ArcaneConduit", true, false);
            Scribe_Values.Look<bool>(ref Boundless, "Boundless", true, false);
            Scribe_Values.Look<bool>(ref Enlightened, "Enlightened", true, false);
            Scribe_Values.Look<bool>(ref Cursed, "Cursed", true, false);
            Scribe_Values.Look<bool>(ref FaeBlood, "FaeBlood", true, false);
            Scribe_Values.Look<bool>(ref GiantsBlood, "GiantsBlood", true, false);
            Scribe_Collections.Look(ref FactionFighterSettings, "FactionFighterSettings");
            Scribe_Collections.Look(ref FactionMageSettings, "FactionMageSettings");
        }
    }
}
