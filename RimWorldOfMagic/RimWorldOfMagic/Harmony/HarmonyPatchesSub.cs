using System;
using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using AbilityUserAI;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using TorannMagic.Golems;
using TorannMagic.ModOptions;
using TorannMagic.TMDefs;
using TorannMagic.Utils;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TorannMagic
{
    public partial class TorannMagicMod
    {
        [HarmonyPatch(typeof(JobGiver_Mate), "TryGiveJob", null)]
        public class JobGiver_Mate_Patch
        {
            public static void Postfix(Pawn pawn, ref Job __result)
            {
                if (pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD"), false) ||
                    pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadAnimalHD")))
                {
                    __result = null;
                }
            }
        }

        [HarmonyPatch(typeof(MapParent), "CheckRemoveMapNow", null)]
        public class CheckRemoveMapNow_Patch
        {
            public static bool Prefix()
            {
                return !ModOptions.Constants.GetPawnInFlight();
            }
        }

        [HarmonyPatch(typeof(CompMilkable), "CompInspectStringExtra", null)]
        public class CompMilkable_Patch
        {
            public static void Postfix(CompMilkable __instance, ref string __result)
            {
                if (__instance.parent.def.defName == "Poppi")
                {
                    __result = "Poppi_fuelGrowth".Translate() + ": " + __instance.Fullness.ToStringPercent();
                }
            }
        }

        [HarmonyPatch(typeof(JobGiver_Kidnap), "TryGiveJob", null)]
        public class JobGiver_Kidnap_Patch
        {
            public static void Postfix(Pawn pawn, ref Job __result)
            {
                if (pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD")))
                {
                    __result = null;
                }
            }
        }

        [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow", null)]
        public class ITab_Pawn_Gear_Patch
        {
            public static Rect GetRowRect(Rect inRect, int row)
            {
                float y = 20f * (float)row;
                Rect result = new Rect(inRect.x, y, inRect.width, 18f);
                return result;
            }

            public static void Postfix(ref float y, float width, Thing thing)
            {
                bool valid = !thing.DestroyedOrNull() && thing.TryGetQuality(out QualityCategory qc);
                if (valid)
                {
                    if (((thing.def.thingClass != null &&
                          thing.def.thingClass.ToString() == "RimWorld.Apparel") ||
                         thing.TryGetComp<CompEquippable>() != null) &&
                        thing.TryGetComp<Enchantment.CompEnchantedItem>() != null)
                    {
                        if (thing.TryGetComp<Enchantment.CompEnchantedItem>().HasEnchantment)
                        {
                            Text.Font = GameFont.Tiny;
                            string str1 = "-- Enchanted (";
                            string str2 = "Enchanted \n\n";

                            Enchantment.CompEnchantedItem enchantedItem =
                                thing.TryGetComp<Enchantment.CompEnchantedItem>();
                            if (enchantedItem.maxMP != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem.maxMPTier);
                                str1 += "M";
                                str2 += enchantedItem.MaxMPLabel + "\n";
                            }

                            if (enchantedItem.mpCost != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor
                                        .EnchantmentColor(enchantedItem.mpCostTier);
                                str1 += "C";
                                str2 += enchantedItem.MPCostLabel + "\n";
                            }

                            if (enchantedItem.mpRegenRate != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem
                                        .mpRegenRateTier);
                                str1 += "R";
                                str2 += enchantedItem.MPRegenRateLabel + "\n";
                            }

                            if (enchantedItem.coolDown != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem
                                        .coolDownTier);
                                str1 += "D";
                                str2 += enchantedItem.CoolDownLabel + "\n";
                            }

                            if (enchantedItem.xpGain != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor
                                        .EnchantmentColor(enchantedItem.xpGainTier);
                                str1 += "G";
                                str2 += enchantedItem.XPGainLabel + "\n";
                            }

                            if (enchantedItem.arcaneRes != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem
                                        .arcaneResTier);
                                str1 += "X";
                                str2 += enchantedItem.ArcaneResLabel + "\n";
                            }

                            if (enchantedItem.arcaneDmg != 0)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem
                                        .arcaneDmgTier);
                                str1 += "Z";
                                str2 += enchantedItem.ArcaneDmgLabel + "\n";
                            }

                            if (enchantedItem.arcaneSpectre)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem.skillTier);
                                str1 += "*S";
                                str2 += enchantedItem.ArcaneSpectreLabel + "\n";
                            }

                            if (enchantedItem.phantomShift)
                            {
                                GUI.color =
                                    Enchantment.GenEnchantmentColor.EnchantmentColor(enchantedItem.skillTier);
                                str1 += "*P";
                                str2 += enchantedItem.PhantomShiftLabel + "\n";
                            }

                            str1 += ")";
                            y -= 6f;
                            Rect rect = new Rect(48f, y, width - 36f, 28f);
                            Widgets.Label(rect, str1);

                            TooltipHandler.TipRegion(rect, () => string.Concat(new string[]
                            {
                                str2,
                            }), 398512);

                            y += 28f;
                            GUI.color = Color.white;
                            Text.Font = GameFont.Small;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawOverallArmor", null)]
        public class ITab_Pawn_GearFillTab_Patch
        {
            //public static FieldInfo pawn = typeof(ITab_Pawn_Gear).GetField("SelPawnForGear", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            public static void Postfix(ITab_Pawn_Gear __instance, ref float curY, float width, StatDef stat,
                string label)
            {
                if (stat.defName == "ArmorRating_Heat")
                {
                    //Traverse traverse = Traverse.Create(__instance);
                    Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
                    if (!pawn.DestroyedOrNull() && !pawn.Dead)
                    {
                        stat = StatDef.Named("ArmorRating_Alignment");
                        label = "TM_ArmorHarmony".Translate();
                        float num = 0f;
                        float num2 = Mathf.Clamp01(pawn.GetStatValue(stat, true) / 2f);
                        List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
                        List<Apparel> list = pawn.apparel?.WornApparel;
                        for (int i = 0; i < allParts.Count; i++)
                        {
                            float num3 = 1f - num2;
                            if (list != null)
                            {
                                for (int j = 0; j < list.Count; j++)
                                {
                                    if (list[j].def.apparel.CoversBodyPart(allParts[i]))
                                    {
                                        float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
                                        num3 *= 1f - num4;
                                    }
                                }
                            }

                            num += allParts[i].coverageAbs * (1f - num3);
                        }

                        num = Mathf.Clamp(num * 2f, 0f, 2f);
                        Rect rect = new Rect(0f, curY, width, 100f);
                        Widgets.Label(rect, label.Truncate(120f, null));
                        rect.xMin += 120f;
                        Widgets.Label(rect, num.ToStringPercent());
                        curY += 22f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NegativeInteractionUtility), "NegativeInteractionChanceFactor", null)]
        public class NegativeInteractionChanceFactor_Patch
        {
            public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
            {
                CompAbilityUserMagic comp = initiator.GetCompAbilityUserMagic();
                if (initiator.story?.traits != null)
                {
                    if ((initiator.story.traits.HasTrait(TorannMagicDefOf.TM_Bard) ||
                         TM_ClassUtility.ClassHasAbility(TorannMagicDefOf.TM_Entertain, comp, null)))
                    {
                        MagicPowerSkill ver =
                            comp.MagicData.MagicPowerSkill_Entertain.FirstOrDefault((MagicPowerSkill x) =>
                                x.label == "TM_Entertain_ver");
                        __result = __result / (1 + ver.level);
                    }

                    if (initiator.story.traits.HasTrait(TorannMagicDefOf.TM_Sniper))
                    {
                        __result *= 1.2f;
                    }
                }

                if (initiator.mindState != null && initiator.mindState.mentalStateHandler != null &&
                    initiator.Inspired && initiator.InspirationDef.defName == "Outgoing")
                {
                    __result = __result * .5f;
                }
            }
        }

        [HarmonyPatch(typeof(InspirationHandler), "TryStartInspiration", null)]
        public class InspirationHandler_Patch
        {
            public static bool Prefix(InspirationHandler __instance, ref bool __result)
            {
                if (__instance.pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD")) ||
                    __instance.pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_LichHD")))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawIcons", null)]
        public class ColonistBarColonistDrawer_Patch
        {
            public static void Postfix(ColonistBarColonistDrawer __instance, ref Rect rect, Pawn colonist)
            {
                if (colonist.Dead) return;

                TraitIconMap.TraitIconValue traitIconValue = null;
                if (colonist.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadHD))
                {
                    traitIconValue = new TraitIconMap.TraitIconValue(
                        TM_RenderQueue.necroMarkMat,
                        TM_MatPool.Icon_Undead,
                        "TM_Icon_Undead"
                    );
                }
                else if (ModOptions.Settings.Instance.showClassIconOnColonistBar && colonist.story != null)
                {
                    foreach (Trait trait in colonist.story.traits.allTraits.Where(trait =>
                                 TraitIconMap.ContainsKey(trait.def)))
                    {
                        traitIconValue = TraitIconMap.Get(trait.def);
                        break;
                    }
                }

                // Skip rendering if there's nothing to render
                if (traitIconValue == null) return;

                float num = 20f * Find.ColonistBar.Scale * ModOptions.Settings.Instance.classIconSize;
                Vector2 vector = new Vector2(rect.x + 1f, rect.yMin + 1f);
                rect = new Rect(vector.x, vector.y, num, num);
                GUI.DrawTexture(rect, traitIconValue.IconTexture);
                TooltipHandler.TipRegion(rect, traitIconValue.IconType.Translate());
            }
        }

        [HarmonyPatch(typeof(Pawn_InteractionsTracker), "InteractionsTrackerTickInterval", null)]
        public class InteractionsTrackerTick_Patch
        {
            public static void Postfix(Pawn_InteractionsTracker __instance, Pawn ___pawn,
                ref bool ___wantsRandomInteract, int ___lastInteractionTime)
            {
                if (Find.TickManager.TicksGame % 1200 == 0)
                {
                    if (___pawn.IsColonist && !___pawn.Downed && !___pawn.Dead && ___pawn.RaceProps.Humanlike)
                    {
                        CompAbilityUserMagic comp = ___pawn.GetCompAbilityUserMagic();

                        if (comp != null && comp.IsMagicUser &&
                            (comp.Pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Bard) ||
                             TM_ClassUtility.ClassHasAbility(TorannMagicDefOf.TM_Entertain, comp, null)))
                        {
                            MagicPowerSkill pwr =
                                comp.MagicData.MagicPowerSkill_Entertain.FirstOrDefault((MagicPowerSkill x) =>
                                    x.label == "TM_Entertain_pwr");
                            if ((Find.TickManager.TicksGame - ___lastInteractionTime) >
                                (3000 - (450 * pwr.level)))
                            {
                                ___wantsRandomInteract = true;
                            }
                        }

                        if (___pawn.Inspired && ___pawn.InspirationDef.defName == "ID_Outgoing")
                        {
                            if ((Find.TickManager.TicksGame - ___lastInteractionTime) > (1800))
                            {
                                ___wantsRandomInteract = true;
                            }
                        }

                        if (___pawn.health != null && ___pawn.health.hediffSet != null &&
                            ___pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_TaskMasterHD))
                        {
                            if ((Find.TickManager.TicksGame - ___lastInteractionTime) < 30000)
                            {
                                ___wantsRandomInteract = false;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState", null)]
        public class MentalStateHandler_Patch
        {
            //public static FieldInfo pawn = typeof(MentalStateHandler).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            public static bool Prefix(MentalStateHandler __instance, MentalStateDef stateDef, Pawn otherPawn,
                Pawn ___pawn, ref bool __result)
            {
                if (___pawn.RaceProps.Humanlike && (TM_Calc.IsUndeadNotVamp(___pawn)))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPriority(100)]
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed", null)]
        public static class Pawn_NeedsTracker_Patch
        {
            //public static FieldInfo pawn = typeof(Pawn_NeedsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            public static bool Prefix(Pawn_NeedsTracker __instance, NeedDef nd, Pawn ___pawn,
                ref bool __result)
            {
                //Traverse traverse = Traverse.Create(__instance);
                Pawn pawn = ___pawn;
                if (pawn != null)
                {
                    if (pawn.def == TorannMagicDefOf.TM_SpiritTD)
                    {
                        if (nd == TorannMagicDefOf.TM_SpiritND || nd == TorannMagicDefOf.TM_Mana)
                        {
                            return true;
                        }

                        return false;
                    }

                    if (nd.defName == "ROMV_Blood" &&
                        (pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD")) ||
                         pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadAnimalHD")) ||
                         pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_LichHD"))))
                    {
                        bool hasVampHediff =
                            pawn.health.hediffSet.HasHediff(HediffDef.Named("ROM_Vampirism"));
                        if (hasVampHediff)
                        {
                            return true;
                        }

                        __result = false;
                        return false;
                    }

                    if ((nd.defName == "TM_Mana" || nd.defName == "TM_Stamina") &&
                        pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD")))
                    {
                        __result = false;
                        return false;
                    }

                    if (nd == TorannMagicDefOf
                            .TM_Travel) // && pawn.story != null && pawn.story.traits != null)
                    {
                        __result = false;
                        return false;
                    }

                    if (TM_GolemUtility.GolemPawns.Contains(pawn.def))
                    {
                        foreach (NeedDef n in TM_GolemUtility.GetGolemDefFromThing(___pawn).needs)
                        {
                            if (n != null && n == nd)
                            {
                                __result = true;
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits", null)]
        public static class PawnGenerator_Patch
        {
            private static bool CanReceiveClass(bool setting, Pawn pawn, TraitDef traitDef)
            {
                return setting &&
                       !pawn.story.AllBackstories.Any(bs =>
                           bs.DisallowsTrait(traitDef, 0)) &&
                       !pawn.story.traits.allTraits.Any(td =>
                           td.def.conflictingTraits.Contains(traitDef));
            }

            private static void Postfix(Pawn pawn)
            {
                if (pawn.IsShambler || pawn.IsGhoul) return;

                float fighterFactor = 1f, mageFactor = 1f;
                if (pawn.Faction != null)
                {
                    ModOptions.Settings.Instance.FactionFighterSettings.TryGetValue(pawn.Faction.def.defName,
                        out fighterFactor);
                    ModOptions.Settings.Instance.FactionMageSettings.TryGetValue(pawn.Faction.def.defName,
                        out mageFactor);
                }

                if (TM_ClassUtility.CustomFighterClasses == null || TM_ClassUtility.CustomMageClasses == null)
                {
                    TM_ClassUtility.LoadCustomClasses();
                }

                List<TraitDef> fighterClassTraits = new List<TraitDef>();
                AddFighterClassTraits(fighterClassTraits, pawn);

                if (TM_ClassUtility.CustomFighterClasses != null)
                {
                    foreach (TM_CustomClass customFighter in TM_ClassUtility.CustomFighterClasses)
                    {
                        if (CanReceiveClass(true, pawn, customFighter.classTrait))
                        {
                            fighterClassTraits.Add(customFighter.classTrait);
                        }
                    }
                }

                List<TraitDef> mageClassTraits = new List<TraitDef>();
                AddMageClassTraits(mageClassTraits, pawn);

                if (TM_ClassUtility.CustomMageClasses != null)
                {
                    foreach (TM_CustomClass customMage in TM_ClassUtility.CustomMageClasses)
                    {
                        if (CanReceiveClass(true, pawn, customMage.classTrait))
                        {
                            mageClassTraits.Add(customMage.classTrait);
                        }
                    }
                }

                // Helper methods for trait collection
                void AddFighterClassTraits(List<TraitDef> traits, Pawn p)
                {
                    Settings s = ModOptions.Settings.Instance;
                    if (CanReceiveClass(s.Gladiator, p, TorannMagicDefOf.Gladiator))
                        traits.Add(TorannMagicDefOf.Gladiator);
                    if (CanReceiveClass(s.Sniper, p, TorannMagicDefOf.TM_Sniper))
                        traits.Add(TorannMagicDefOf.TM_Sniper);
                    if (CanReceiveClass(s.Bladedancer, p, TorannMagicDefOf.Bladedancer))
                        traits.Add(TorannMagicDefOf.Bladedancer);
                    if (CanReceiveClass(s.Ranger, p, TorannMagicDefOf.Ranger))
                        traits.Add(TorannMagicDefOf.Ranger);
                    if (CanReceiveClass(s.Faceless, p, TorannMagicDefOf.Faceless))
                        traits.Add(TorannMagicDefOf.Faceless);
                    if (CanReceiveClass(s.Psionic, p, TorannMagicDefOf.TM_Psionic))
                        traits.Add(TorannMagicDefOf.TM_Psionic);
                    if (CanReceiveClass(s.DeathKnight, p, TorannMagicDefOf.DeathKnight))
                        traits.Add(TorannMagicDefOf.DeathKnight);
                    if (CanReceiveClass(s.Monk, p, TorannMagicDefOf.TM_Monk))
                        traits.Add(TorannMagicDefOf.TM_Monk);
                    if (CanReceiveClass(s.Wayfarer, p, TorannMagicDefOf.TM_Wayfarer))
                        traits.Add(TorannMagicDefOf.TM_Wayfarer);
                    if (CanReceiveClass(s.Commander, p, TorannMagicDefOf.TM_Commander))
                        traits.Add(TorannMagicDefOf.TM_Commander);
                    if (CanReceiveClass(s.SuperSoldier, p, TorannMagicDefOf.TM_SuperSoldier))
                        traits.Add(TorannMagicDefOf.TM_SuperSoldier);
                }

                void AddMageClassTraits(List<TraitDef> traits, Pawn p)
                {
                    Settings s = ModOptions.Settings.Instance;
                    if (CanReceiveClass(s.FireMage, p, TorannMagicDefOf.InnerFire))
                        traits.Add(TorannMagicDefOf.InnerFire);
                    if (CanReceiveClass(s.IceMage, p, TorannMagicDefOf.HeartOfFrost))
                        traits.Add(TorannMagicDefOf.HeartOfFrost);
                    if (CanReceiveClass(s.LitMage, p, TorannMagicDefOf.StormBorn))
                        traits.Add(TorannMagicDefOf.StormBorn);
                    if (CanReceiveClass(s.Druid, p, TorannMagicDefOf.Druid))
                        traits.Add(TorannMagicDefOf.Druid);
                    if (CanReceiveClass(s.Paladin, p, TorannMagicDefOf.Paladin))
                        traits.Add(TorannMagicDefOf.Paladin);
                    if (CanReceiveClass(s.Summoner, p, TorannMagicDefOf.Summoner))
                        traits.Add(TorannMagicDefOf.Summoner);
                    if (CanReceiveClass(s.Priest, p, TorannMagicDefOf.Priest))
                        traits.Add(TorannMagicDefOf.Priest);
                    if (CanReceiveClass(s.Necromancer, p, TorannMagicDefOf.Necromancer))
                        traits.Add(TorannMagicDefOf.Necromancer);
                    if (CanReceiveClass(s.Bard, p, TorannMagicDefOf.TM_Bard))
                        traits.Add(TorannMagicDefOf.TM_Bard);
                    if (p.gender != Gender.Female && CanReceiveClass(s.Demonkin, p, TorannMagicDefOf.Warlock))
                        traits.Add(TorannMagicDefOf.Warlock);
                    if (p.gender == Gender.Female &&
                        CanReceiveClass(s.Demonkin, p, TorannMagicDefOf.Succubus))
                        traits.Add(TorannMagicDefOf.Succubus);
                    if (CanReceiveClass(s.Geomancer, p, TorannMagicDefOf.Geomancer))
                        traits.Add(TorannMagicDefOf.Geomancer);
                    if (CanReceiveClass(s.Technomancer, p, TorannMagicDefOf.Technomancer))
                        traits.Add(TorannMagicDefOf.Technomancer);
                    if (CanReceiveClass(s.BloodMage, p, TorannMagicDefOf.BloodMage))
                        traits.Add(TorannMagicDefOf.BloodMage);
                    if (CanReceiveClass(s.Enchanter, p, TorannMagicDefOf.Enchanter))
                        traits.Add(TorannMagicDefOf.Enchanter);
                    if (CanReceiveClass(s.Chronomancer, p, TorannMagicDefOf.Chronomancer))
                        traits.Add(TorannMagicDefOf.Chronomancer);
                    if (CanReceiveClass(s.Wanderer, p, TorannMagicDefOf.TM_Wanderer))
                        traits.Add(TorannMagicDefOf.TM_Wanderer);
                    if (CanReceiveClass(s.ChaosMage, p, TorannMagicDefOf.ChaosMage))
                        traits.Add(TorannMagicDefOf.ChaosMage);
                }
            }
        }

        [HarmonyPatch(typeof(IncidentParmsUtility), "GetDefaultPawnGroupMakerParms", null)]
        public static class GetDefaultPawnGroupMakerParms_Patch
        {
            public static void Postfix(ref PawnGroupMakerParms __result)
            {
                if (__result.faction != null && __result.faction.def.defName == "Seers")
                {
                    __result.points *= 1.65f;
                }
            }
        }

        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob", null)]
        public static class JobGiver_GetFood_Patch
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadHD) ||
                    pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadAnimalHD))
                {
                    __result = null;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(JobGiver_EatRandom), "TryGiveJob", null)]
        public static class JobGiver_EatRandom_Patch
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadHD) ||
                    pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadAnimalHD))
                {
                    __result = null;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(JobGiver_Haul), "TryGiveJob", null)]
        public static class JobGiver_MinionHaul_Patch
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn != null && (pawn.def == TorannMagicDefOf.TM_MinionR ||
                                     pawn.def == TorannMagicDefOf.TM_GreaterMinionR))
                {
                    if (pawn.jobs != null && pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.HaulToCell ||
                                                                     pawn.CurJob.def ==
                                                                     JobDefOf.HaulToContainer ||
                                                                     pawn.CurJob.def ==
                                                                     JobDefOf.HaulToTransporter))
                    {
                        __result = null;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AbilityUser.AbilityDef), "GetJob", null)]
        public static class AbilityDef_Patch
        {
            private static bool Prefix(AbilityUser.AbilityDef __instance, AbilityTargetCategory cat,
                LocalTargetInfo target, ref Job __result)
            {
                if (__instance.abilityClass.FullName == "TorannMagic.MagicAbility" ||
                    __instance.abilityClass.FullName == "TorannMagic.MightAbility" ||
                    __instance.defName.Contains("TM_Artifact"))
                {
                    Job result;
                    switch (cat)
                    {
                        case AbilityTargetCategory.TargetSelf:
                            result = new Job(TorannMagicDefOf.TMCastAbilitySelf, target);
                            __result = result;
                            return false;
                        case AbilityTargetCategory.TargetThing:
                            result = new Job(TorannMagicDefOf.TMCastAbilityVerb, target);
                            __result = result;
                            return false;
                        case AbilityTargetCategory.TargetAoE:
                            result = new Job(TorannMagicDefOf.TMCastAbilityVerb, target);
                            __result = result;
                            return false;
                    }

                    result = new Job(TorannMagicDefOf.TMCastAbilityVerb, target);
                    __result = result;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(JobDriver_Mine), "ResetTicksToPickHit", null)]
        public static class JobDriver_Mine_Patch
        {
            private static void Postfix(JobDriver_Mine __instance)
            {
                if (Rand.Chance(ModOptions.Settings.Instance.magicyteChance))
                {
                    Thing thing = null;
                    thing = ThingMaker.MakeThing(TorannMagicDefOf.RawMagicyte);
                    thing.stackCount = Rand.Range(9, 25);
                    if (thing != null)
                    {
                        GenPlace.TryPlaceThing(thing, __instance.pawn.Position, __instance.pawn.Map,
                            ThingPlaceMode.Near, null);
                        if (!__instance.pawn.Faction.IsPlayer)
                        {
                            thing.SetForbidden(true, false);
                        }
                    }
                }
            }
        }

        [HarmonyPriority(100)] //Go last
        public static void AddHumanLikeOrders_RestrictEquipmentPatch(Vector3 clickPos, Pawn pawn,
            ref List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (pawn.equipment != null)
            {
                if (pawn.def == TorannMagicDefOf.TM_SpiritTD)
                {
                    List<FloatMenuOption> remop = new List<FloatMenuOption>();
                    remop.Clear();
                    foreach (FloatMenuOption op in opts)
                    {
                        if (op.Label.StartsWith("Pick"))
                        {
                            remop.Add(op);
                        }
                    }

                    foreach (FloatMenuOption op in remop)
                    {
                        opts.Remove(op);
                    }
                }

                ThingWithComps equipment = null;
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].def == TorannMagicDefOf.TM_Artifact_BracersOfThePacifist)
                    {
                        equipment = (ThingWithComps)thingList[i];
                        break;
                    }
                }

                if (equipment != null)
                {
                    string labelShort = equipment.LabelShort;
                    FloatMenuOption nve_option;
                    if (!(pawn.story.traits.HasTrait(TorannMagicDefOf.Priest) ||
                          pawn.WorkTagIsDisabled(WorkTags.Violent)))
                    {
                        for (int j = 0; j < opts.Count; j++)
                        {
                            if (opts[j].Label.Contains("wear"))
                            {
                                opts.Remove(opts[j]);
                            }
                        }

                        nve_option =
                            new FloatMenuOption(
                                "TM_ViolentCannotEquip".Translate(pawn.LabelShort, labelShort), null);
                        opts.Add(nve_option);
                    }
                }
            }

            foreach (FloatMenuOption op in opts)
            {
                if (op.Label.StartsWith("TM_Use"))
                {
                    op.Label = "TM_Use".Translate(op.revalidateClickTarget.Label);
                }
                else if (op.Label.StartsWith("TM_Learn"))
                {
                    op.Label = "TM_Learn".Translate(op.revalidateClickTarget.Label);
                }
                else if (op.Label.StartsWith("TM_Read"))
                {
                    op.Label = "TM_Read".Translate(op.revalidateClickTarget.Label);
                }
                else if (op.Label.StartsWith("TM_Inject"))
                {
                    op.Label = "TM_Inject".Translate(op.revalidateClickTarget.Label);
                }
            }
        }

        //todo: fix this
        // [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders", null)]
        // public static class FloatMenuMakerMap_Patch
        // {
        //     public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        //     {
        //         if (pawn == null)
        //         {
        //             return;
        //         }
        //         IntVec3 c = IntVec3.FromVector3(clickPos);
        //         Enchantment.CompEnchant comp = pawn.TryGetComp<Enchantment.CompEnchant>();
        //         CompAbilityUserMagic pawnComp = pawn.GetCompAbilityUserMagic();
        //         if (comp != null && pawnComp != null && pawnComp.IsMagicUser && pawn.story != null && pawn.story.traits != null && !pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
        //         {
        //             if (comp.enchantingContainer == null)
        //             {
        //                 Log.Warning($"Enchanting container is null for {pawn}, initializing.");
        //                 comp.enchantingContainer = new ThingOwner<Thing>();
        //                 //comp.enchantingContainer = new ThingOwner<Thing>(comp);
        //             }
        //             bool emptyGround = true;
        //             foreach (Thing current in c.GetThingList(pawn.Map))
        //             {
        //                 if (current != null && current.def.EverHaulable)
        //                 {
        //                     emptyGround = false;
        //                 }
        //             }
        //             if (emptyGround && !pawn.Drafted) //c.GetThingList(pawn.Map).Count == 0 &&
        //             {
        //                 if (comp.enchantingContainer?.Count > 0)
        //                 {
        //                     if (!pawn.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
        //                     {
        //                         opts.Add(new FloatMenuOption("TM_CannotDrop".Translate(
        //                             comp.enchantingContainer[0].Label
        //                         ) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
        //                     }
        //                     else
        //                     {
        //                         opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TM_DropGem".Translate(
        //                         comp.enchantingContainer.ContentsString
        //                         ), delegate
        //                         {
        //                             Job job = new Job(TorannMagicDefOf.JobDriver_RemoveEnchantingGem, c);
        //                             pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //                         }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, c, "ReservedBy"));
        //                     }
        //                 }
        //
        //             }
        //             foreach (Thing current in c.GetThingList(pawn.Map))
        //             {
        //                 Thing t = current;
        //                 if (t != null && t.def.EverHaulable && t.def.defName.ToString().Contains("TM_EStone_"))
        //                 {
        //                     if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
        //                     {
        //                         opts.Add(new FloatMenuOption("CannotPickUp".Translate(
        //                         t.Label
        //                         ) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
        //                     }
        //                     else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, t, 1))
        //                     {
        //                         opts.Add(new FloatMenuOption("CannotPickUp".Translate(
        //                         t.Label
        //                         ) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
        //                     }
        //                     else// if (item.stackCount == 1)
        //                     {
        //                         opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TM_PickupGem".Translate(
        //                         t.Label
        //                         ), delegate
        //                         {
        //                             t.SetForbidden(false, false);
        //                             Job job = new Job(TorannMagicDefOf.JobDriver_AddEnchantingGem, t);
        //                             job.count = 1;
        //                             pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //                         }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, t, "ReservedBy"));
        //                     }
        //                 }
        //                 else if ((current.def.IsApparel || current.def.IsWeapon || current.def.IsRangedWeapon) && comp.enchantingContainer?.Count > 0)
        //                 {
        //                     if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
        //                     {
        //                         opts.Add(new FloatMenuOption("TM_CannotReach".Translate(
        //                         t.Label
        //                         ) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
        //                     }
        //                     else if (pawnComp.Mana.CurLevel < .5f)
        //                     {
        //                         opts.Add(new FloatMenuOption("TM_NeedManaForEnchant".Translate(
        //                         pawnComp.Mana.CurLevel.ToString("0.000")
        //                         ), null, MenuOptionPriority.Default, null, null, 0f, null, null));
        //                     }
        //                     else// if (item.stackCount == 1)
        //                     {
        //                         if (current.stackCount == 1)
        //                         {
        //                             opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TM_EnchantItem".Translate(
        //                                 t.Label
        //                             ), delegate
        //                             {
        //                                 t.SetForbidden(true, false);
        //                                 Job job = new Job(TorannMagicDefOf.JobDriver_EnchantItem, t);
        //                                 job.count = 1;
        //                                 pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //                             }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, t, "ReservedBy"));
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        [HarmonyPatch(typeof(FloatMenuOptionProvider_WorkGivers), "GetWorkGiversOptionsFor", null)]
        public static class FloatMenuMakerMap_MagicJobGiver_Patch
        {
            public static void Postfix(Pawn pawn, LocalTargetInfo target, FloatMenuContext context,
                ref IEnumerable<FloatMenuOption> __result)
            {
                RimWorld.JobGiver_Work jobGiver_Work =
                    pawn.thinker.TryGetMainTreeThinkNode<RimWorld.JobGiver_Work>();
                if (jobGiver_Work != null)
                {
                    foreach (Thing item in pawn.Map.thingGrid.ThingsAt(target.Cell))
                    {
                        if (item is Building && (item.def == TorannMagicDefOf.TableArcaneForge))
                        {
                            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                            if (comp != null && comp.Mana != null && comp.Mana.CurLevel < .5f)
                            {
                                string text = null;
                                Action action = null;
                                text = "TM_InsufficientManaForJob".Translate(
                                    (comp.Mana.CurLevel * 100).ToString("0.##"));
                                FloatMenuOption menuOption =
                                    FloatMenuUtility.DecoratePrioritizedTask(
                                        new FloatMenuOption(text, action), pawn, item);
                                if (!__result.Any((FloatMenuOption op) => op.Label == menuOption.Label))
                                {
                                    menuOption.Disabled = true;
                                    __result.AddItem(menuOption);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GenGrid), "Standable", null)]
        public class Standable_Patch
        {
            public static bool Prefix(ref IntVec3 c, ref Map map, ref bool __result)
            {
                if (map != null && c != default(IntVec3))
                {
                    return true;
                }

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(AttackTargetFinder), "CanSee", null)]
        public class AttackTargetFinder_CanSee_Patch
        {
            public static bool Prefix(Thing target, ref bool __result)
            {
                if (target is Pawn)
                {
                    Pawn targetPawn = target as Pawn;
                    if (targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_I, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_II, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_III, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_InvisibilityHD, false))
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AttackTargetFinder), "CanReach", null)]
        public class AttackTargetFinder_CanReach_Patch
        {
            public static bool Prefix(Thing target, ref bool __result)
            {
                if (target is Pawn)
                {
                    Pawn targetPawn = target as Pawn;
                    if (targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_I, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_II, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DisguiseHD_III, false) ||
                        targetPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_InvisibilityHD, false))
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn",
            new[] { typeof(Pawn), typeof(SkillDef), typeof(bool) })]
        public static class ArcaneForge_Quality_Patch
        {
            public static void Postfix(Pawn pawn, SkillDef relevantSkill, bool consumeInspiration,
                ref QualityCategory __result)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if (comp != null && comp.IsMagicUser && pawn.story.traits != null &&
                    !pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless) && comp.ArcaneForging)
                {
                    List<IntVec3> cellList = GenRadial.RadialCellsAround(pawn.Position, 2, true).ToList();
                    bool forgeNearby = false;
                    for (int i = 0; i < cellList.Count; i++)
                    {
                        List<Thing> thingList = cellList[i].GetThingList(pawn.Map);
                        if (thingList != null && thingList.Count > 0)
                        {
                            for (int j = 0; j < thingList.Count; j++)
                            {
                                if (thingList[j] != null && thingList[j] is Building)
                                {
                                    Building bldg = thingList[j] as Building;
                                    if (bldg.def == TorannMagicDefOf.TableArcaneForge)
                                    {
                                        forgeNearby = true;
                                        break;
                                    }
                                }
                            }

                            if (forgeNearby)
                            {
                                break;
                            }
                        }
                    }

                    if (forgeNearby)
                    {
                        int mageLevel = Rand.Range(0, Mathf.RoundToInt(comp.MagicUserLevel / 15));
                        __result = (QualityCategory)Mathf.Min((int)__result + mageLevel, 6);
                        SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map, false),
                            MaintenanceType.None);
                        info.pitchFactor = .6f;
                        info.volumeFactor = 1.6f;
                        TorannMagicDefOf.TM_Gong.PlayOneShot(info);
                        cellList.Clear();
                        cellList = GenRadial.RadialCellsAround(pawn.Position, (int)__result, false)
                            .ToList<IntVec3>();
                        for (int i = 0; i < cellList.Count; i++)
                        {
                            IntVec3 curCell = cellList[i];
                            Vector3 angle = TM_Calc.GetVector(pawn.Position, curCell);
                            TM_MoteMaker.ThrowArcaneWaveMote(curCell.ToVector3(), pawn.Map,
                                .4f * (curCell - pawn.Position).LengthHorizontal, .1f, .05f, .1f, 0, 3,
                                (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat(),
                                (Quaternion.AngleAxis(90, Vector3.up) * angle).ToAngleFlat());
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DamageWorker), "ExplosionStart", null)]
        public static class ExplosionNoShaker_Patch
        {
            public static bool Prefix(DamageWorker __instance, Explosion explosion)
            {
                if (explosion.damType == TMDamageDefOf.DamageDefOf.TM_BlazingPower ||
                    explosion.damType == TMDamageDefOf.DamageDefOf.TM_BloodBurn ||
                    explosion.damType == TMDamageDefOf.DamageDefOf.TM_HailDD)
                {
                    float radMod = 6f;
                    if (explosion.damType == TMDamageDefOf.DamageDefOf.TM_HailDD)
                    {
                        radMod = 1f;
                    }

                    FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash,
                        explosion.radius * radMod);
                    if (explosion.damType.explosionSnowMeltAmount < 0)
                    {
                        Projectile_Snowball.AddSnowRadial(explosion.Position, explosion.Map, explosion.radius,
                            explosion.radius);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (explosion.damType == TMDamageDefOf.DamageDefOf.TM_BloodBurn)
                        {
                            if (i < 1)
                            {
                                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist,
                                    explosion.Position.ToVector3Shifted() +
                                    Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map,
                                    Rand.Range(1f, 1.5f), .2f, 0.6f, 2f, Rand.Range(-30, 30),
                                    Rand.Range(.5f, .7f), Rand.Range(30f, 40f), Rand.Range(0, 360));
                            }
                        }
                        else
                        {
                            FleckMaker.ThrowSmoke(
                                explosion.Position.ToVector3Shifted() +
                                Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map,
                                explosion.radius * 0.6f);
                        }
                    }

                    if (__instance.def.explosionInteriorMote != null)
                    {
                        int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
                        for (int j = 0; j < num; j++)
                        {
                            MoteMaker.ThrowExplosionInteriorMote(
                                explosion.Position.ToVector3Shifted() +
                                Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map,
                                __instance.def.explosionInteriorMote);
                        }
                    }

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(DamageWorker_AddInjury), "Apply", null)]
        public static class DamageWorker_ApplyEnchantmentAction_Patch
        {
            public static void Postfix(DamageWorker_AddInjury __instance, DamageInfo dinfo, Thing thing,
                DamageWorker.DamageResult __result)
            {
                if (dinfo.Instigator != null && dinfo.Instigator is Pawn && dinfo.Amount != 0 &&
                    dinfo.Weapon != null &&
                    dinfo.Weapon.HasComp(typeof(Enchantment.CompEnchantedItem)))
                {
                    Pawn instigator = dinfo.Instigator as Pawn;
                    if (instigator.equipment != null && instigator.equipment.Primary != null)
                    {
                        ThingWithComps eq = instigator.equipment.Primary;
                        Enchantment.CompEnchantedItem enchantment =
                            eq.TryGetComp<Enchantment.CompEnchantedItem>();
                        if (enchantment != null && enchantment.enchantmentAction != null)
                        {
                            if (enchantment.enchantmentAction.type ==
                                Enchantment.EnchantmentActionType.ApplyHediff &&
                                enchantment.enchantmentAction.hediffDef != null)
                            {
                                if (Rand.Chance(enchantment.enchantmentAction.hediffChance))
                                {
                                    if (enchantment.enchantmentAction.onSelf)
                                    {
                                        List<Pawn> plist = TM_Calc.FindPawnsNearTarget(instigator,
                                            Mathf.RoundToInt(enchantment.enchantmentAction.splashRadius),
                                            instigator.Position, enchantment.enchantmentAction.friendlyFire);
                                        if (plist != null && plist.Count > 0)
                                        {
                                            for (int i = 0; i < plist.Count; i++)
                                            {
                                                HealthUtility.AdjustSeverity(plist[i],
                                                    enchantment.enchantmentAction.hediffDef,
                                                    enchantment.enchantmentAction.hediffSeverity);
                                                if (enchantment.enchantmentAction.hediffDurationTicks != 0)
                                                {
                                                    HediffComp_Disappears hdc = plist[i].health.hediffSet
                                                        .GetFirstHediffOfDef(enchantment.enchantmentAction
                                                            .hediffDef).TryGetComp<HediffComp_Disappears>();
                                                    hdc.ticksToDisappear = enchantment.enchantmentAction
                                                        .hediffDurationTicks;
                                                }
                                            }
                                        }

                                        HealthUtility.AdjustSeverity(instigator,
                                            enchantment.enchantmentAction.hediffDef,
                                            enchantment.enchantmentAction.hediffSeverity);
                                        if (enchantment.enchantmentAction.hediffDurationTicks != 0)
                                        {
                                            HediffComp_Disappears hdc = instigator.health.hediffSet
                                                .GetFirstHediffOfDef(enchantment.enchantmentAction.hediffDef)
                                                .TryGetComp<HediffComp_Disappears>();
                                            hdc.ticksToDisappear = enchantment.enchantmentAction
                                                .hediffDurationTicks;
                                        }
                                    }
                                    else if (thing is Pawn)
                                    {
                                        Pawn p = thing as Pawn;
                                        if (enchantment.enchantmentAction.splashRadius > 0)
                                        {
                                            List<Pawn> plist = TM_Calc.FindPawnsNearTarget(p,
                                                Mathf.RoundToInt(enchantment.enchantmentAction.splashRadius),
                                                p.Position, enchantment.enchantmentAction.friendlyFire);
                                            if (plist != null && plist.Count > 0)
                                            {
                                                for (int i = 0; i < plist.Count; i++)
                                                {
                                                    HealthUtility.AdjustSeverity(plist[i],
                                                        enchantment.enchantmentAction.hediffDef,
                                                        enchantment.enchantmentAction.hediffSeverity);
                                                    if (enchantment.enchantmentAction.hediffDurationTicks !=
                                                        0)
                                                    {
                                                        HediffComp_Disappears hdc = plist[i].health.hediffSet
                                                            .GetFirstHediffOfDef(enchantment.enchantmentAction
                                                                .hediffDef)
                                                            .TryGetComp<HediffComp_Disappears>();
                                                        hdc.ticksToDisappear = enchantment.enchantmentAction
                                                            .hediffDurationTicks;
                                                    }
                                                }
                                            }
                                        }

                                        HealthUtility.AdjustSeverity(p,
                                            enchantment.enchantmentAction.hediffDef,
                                            enchantment.enchantmentAction.hediffSeverity);
                                        if (enchantment.enchantmentAction.hediffDurationTicks != 0)
                                        {
                                            HediffComp_Disappears hdc = instigator.health.hediffSet
                                                .GetFirstHediffOfDef(enchantment.enchantmentAction.hediffDef)
                                                .TryGetComp<HediffComp_Disappears>();
                                            hdc.ticksToDisappear = enchantment.enchantmentAction
                                                .hediffDurationTicks;
                                        }
                                    }
                                }
                            }

                            if (enchantment.enchantmentAction.type ==
                                Enchantment.EnchantmentActionType.ApplyDamage &&
                                enchantment.enchantmentAction.damageDef != null &&
                                dinfo.Def != enchantment.enchantmentAction.damageDef)
                            {
                                if (Rand.Chance(enchantment.enchantmentAction.damageChance))
                                {
                                    DamageInfo dinfo2 = new DamageInfo(
                                        enchantment.enchantmentAction.damageDef,
                                        Rand.Range(
                                            enchantment.enchantmentAction.damageAmount -
                                            enchantment.enchantmentAction.damageVariation,
                                            enchantment.enchantmentAction.damageAmount +
                                            enchantment.enchantmentAction.damageVariation),
                                        enchantment.enchantmentAction.armorPenetration, -1f, instigator, null,
                                        dinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown);

                                    if (enchantment.enchantmentAction.onSelf)
                                    {
                                        List<Pawn> plist = TM_Calc.FindPawnsNearTarget(instigator,
                                            Mathf.RoundToInt(enchantment.enchantmentAction.splashRadius),
                                            instigator.Position, enchantment.enchantmentAction.friendlyFire);
                                        if (plist != null && plist.Count > 0)
                                        {
                                            for (int i = 0; i < plist.Count; i++)
                                            {
                                                plist[i].TakeDamage(dinfo2);
                                            }
                                        }

                                        instigator.TakeDamage(dinfo2);
                                    }
                                    else if (thing is Pawn)
                                    {
                                        Pawn p = thing as Pawn;
                                        List<Pawn> plist = TM_Calc.FindPawnsNearTarget(p,
                                            Mathf.RoundToInt(enchantment.enchantmentAction.splashRadius),
                                            p.Position, enchantment.enchantmentAction.friendlyFire);
                                        if (plist != null && plist.Count > 0)
                                        {
                                            for (int i = 0; i < plist.Count; i++)
                                            {
                                                plist[i].TakeDamage(dinfo2);
                                            }
                                        }

                                        p.TakeDamage(dinfo2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AbilityAIDef), "CanPawnUseThisAbility", null)]
        public static class CanPawnUseThisAbility_Patch
        {
            private static bool Prefix(AbilityAIDef __instance, Pawn caster, LocalTargetInfo target,
                ref bool __result)
            {
                if (__instance.appliedHediffs.Count > 0 &&
                    __instance.appliedHediffs.Any((HediffDef hediffDef) =>
                        caster.health.hediffSet.HasHediff(hediffDef, false)))
                {
                    __result = false;
                }
                else
                {
                    if (!__instance.Worker.CanPawnUseThisAbility(__instance, caster, target))
                    {
                        __result = false;
                    }
                    else
                    {
                        if (!__instance.needEnemyTarget)
                        {
                            __result = true;
                        }
                        else
                        {
                            if (!__instance.usedOnCaster && target.IsValid)
                            {
                                float num = Math.Abs(caster.Position.DistanceTo(target.Cell));
                                if (num < __instance.minRange || num > __instance.maxRange)
                                {
                                    __result = false;
                                    return false;
                                }

                                if (__instance.needSeeingTarget &&
                                    !AbilityUserAI.AbilityUtility.LineOfSightLocalTarget(caster, target, true,
                                        null))
                                {
                                    __result = false;
                                    return false;
                                }
                            }

                            //Log.Message("caster " + caster.LabelShort + " attempting to case " + __instance.ability.defName + " on target " + target.Thing.LabelShort);
                            if (__instance.ability.defName == "TM_ArrowStorm" &&
                                !caster.equipment.Primary.def.weaponTags.Contains("Neolithic"))
                            {
                                __result = false;
                                return false;
                            }

                            if (__instance.ability.defName == "TM_DisablingShot" ||
                                __instance.ability.defName == "TM_Headshot" &&
                                caster.equipment.Primary.def.weaponTags.Contains("Neolithic"))
                            {
                                __result = false;
                                return false;
                            }

                            if (target.IsValid && !target.Thing.Destroyed && target.Thing.Map == caster.Map &&
                                target.Thing.Spawned)
                            {
                                Pawn targetPawn = target.Thing as Pawn;
                                if (targetPawn != null && targetPawn.Dead)
                                {
                                    __result = false;
                                    return false;
                                }
                                else
                                {
                                    __result = true;
                                }
                            }
                            else
                            {
                                __result = false;
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(FloatMenuOptionProvider), "GetOptionsFor",
            new[] { typeof(Pawn), typeof(FloatMenuContext) })]
        [HarmonyPriority(100)]
        public static class FloatMenuMakerMap_ROMV_Undead_Patch
        {
            public static void Postfix(Pawn clickedPawn,
                FloatMenuContext context,
                ref IEnumerable<FloatMenuOption> __result)
            {
                // Materialize __result to a list, which we can edit
                var options = new List<FloatMenuOption>(__result);

                // Get the pawn at the context's ClickedCell
                Pawn target = context.ClickedCell.GetFirstPawn(context.map);

                if (target != null)
                {
                    if (target.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadHD")) ||
                        target.health.hediffSet.HasHediff(HediffDef.Named("TM_UndeadAnimalHD")) ||
                        target.health.hediffSet.HasHediff(HediffDef.Named("TM_LichHD")))
                    {
                        string name = target.LabelShort;
                        for (int i = options.Count - 1; i >= 0; i--)
                        {
                            var label = options[i].Label;
                            if (label.Contains("Feed on") ||
                                label.Contains("Sip") ||
                                label.Contains("Embrace") ||
                                label.Contains("Give vampirism") ||
                                label.Contains("Create Ghoul") ||
                                label.Contains("Give vitae") ||
                                label == "Embrace " + name + " (Give vampirism)")
                            {
                                options.RemoveAt(i);
                            }
                        }
                    }
                }

                // Assign the modified list back
                __result = options;
            }
        }

        [HarmonyPatch(typeof(AbilityWorker), "CanPawnUseThisAbility", null)]
        public static class AbilityWorker_CanPawnUseThisAbility_Patch
        {
            public static bool Prefix(AbilityAIDef abilityDef, Pawn pawn, LocalTargetInfo target,
                ref bool __result)
            {
                if (!TM_Calc.HasMightOrMagicTrait(pawn)) return true;
                if (!ModOptions.Settings.Instance.AICasting)
                {
                    __result = false;
                    return false;
                }

                if (pawn.IsPrisoner || pawn.Downed)
                {
                    __result = false;
                    return false;
                }

                bool hasThing = target != null && target.HasThing;
                if (hasThing)
                {
                    if (abilityDef.needSeeingTarget && !TM_Calc.HasLoSFromTo(pawn.Position, target, pawn,
                            abilityDef.minRange, abilityDef.maxRange))
                    {
                        __result = false;
                        return false;
                    }

                    Pawn pawn2 = target.Thing as Pawn;
                    if (pawn2 != null)
                    {
                        if (abilityDef.ability == TorannMagicDefOf.TM_Possess && pawn2.RaceProps.Animal)
                        {
                            __result = false;
                            return false;
                        }

                        bool flag = !abilityDef.canTargetAlly;
                        if (flag)
                        {
                            __result = !pawn2.Downed;
                            return false;
                        }
                    }

                    Building bldg2 = target.Thing as Building;
                    if (bldg2 != null)
                    {
                        if (pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Empath) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Apothecary) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Shaman) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Commander) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Chronomancer) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Monk) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.DeathKnight) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.BloodMage) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Enchanter) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Necromancer) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Ranger) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Priest) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Bard) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Geomancer) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Gladiator) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Bladedancer) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Druid) ||
                            pawn.story.traits.HasTrait(TorannMagicDefOf.Summoner))
                        {
                            __result = false;
                            return false;
                        }

                        __result = !bldg2.Destroyed;
                        return false;
                    }

                    Corpse corpse2 = target.Thing as Corpse;
                    if (corpse2 != null)
                    {
                        __result = true; //!corpse2.IsNotFresh();
                        return false;
                    }
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(FertilityGrid), "CalculateFertilityAt", null)]
        public static class FertilityGrid_Patch
        {
            private static void Postfix(Map ___map, IntVec3 loc, ref float __result)
            {
                if (ModOptions.Constants.GetGrowthCells().Count > 0)
                {
                    List<IntVec3> growthCells = ModOptions.Constants.GetGrowthCells();
                    for (int i = 0; i < growthCells.Count; i++)
                    {
                        if (loc != growthCells[i]) continue;

                        __result *= 2f;
                        if (Rand.Chance(.6f) && (ModOptions.Constants.GetLastGrowthMoteTick() + 5) <
                            Find.TickManager.TicksGame)
                        {
                            TM_MoteMaker.ThrowTwinkle(growthCells[i].ToVector3Shifted(), ___map,
                                Rand.Range(.3f, .7f), Rand.Range(100, 300), Rand.Range(.5f, 1.5f),
                                Rand.Range(.1f, .5f), .05f, Rand.Range(.8f, 1.8f));
                            ModOptions.Constants.SetLastGrowthMoteTick(Find.TickManager.TicksGame);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AbilityWorker), "TargetAbilityFor", null)]
        public static class AbilityWorker_TargetAbilityFor_Patch
        {
            public static bool Prefix(AbilityAIDef abilityDef, Pawn pawn, ref LocalTargetInfo __result)
            {
                if (!TM_Calc.HasMightOrMagicTrait(pawn)) return true;


                if (abilityDef.usedOnCaster)
                {
                    __result = pawn;
                }
                else
                {
                    bool canTargetAlly = abilityDef.canTargetAlly;
                    if (canTargetAlly)
                    {
                        __result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                            ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell,
                            TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false),
                            abilityDef.maxRange,
                            (Thing thing) => AbilityUserAI.AbilityUtility.AreAllies(pawn, thing), null, 0, -1,
                            false, RegionType.Set_Passable, false);
                    }
                    else
                    {
                        Pawn pawn2 = pawn.mindState.enemyTarget as Pawn;
                        Building bldg = pawn.mindState.enemyTarget as Building;
                        Corpse corpse = pawn.mindState.enemyTarget as Corpse;
                        if (pawn.mindState.enemyTarget != null && pawn2 != null)
                        {
                            if (!pawn2.Dead)
                            {
                                __result = pawn.mindState.enemyTarget;
                                return false;
                            }
                        }
                        else if (pawn.mindState.enemyTarget != null && bldg != null)
                        {
                            if (!bldg.Destroyed)
                            {
                                __result = pawn.mindState.enemyTarget;
                                return false;
                            }
                        }
                        else if (pawn.mindState.enemyTarget != null && corpse != null)
                        {
                            if (!corpse.IsNotFresh())
                            {
                                __result = pawn.mindState.enemyTarget;
                                return false;
                            }
                        }
                        else
                        {
                            if (pawn.mindState.enemyTarget != null && !(pawn.mindState.enemyTarget is Corpse))
                            {
                                __result = pawn.mindState.enemyTarget;
                                return false;
                            }
                        }

                        __result = null;
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo", null)]
        public static class TryFindShootLineFromTo_Base_Patch
        {
            public static bool Prefix(Verb __instance, IntVec3 root, LocalTargetInfo targ,
                out ShootLine resultingLine, ref bool __result)
            {
                if (__instance.verbProps.IsMeleeAttack)
                {
                    resultingLine = new ShootLine(root, targ.Cell);
                    __result = ReachabilityImmediate.CanReachImmediate(root, targ, __instance.caster.Map,
                        PathEndMode.Touch, null);
                    return false;
                }

                if (__instance.verbProps.range == 0 && __instance.CasterPawn != null &&
                    !__instance.CasterPawn.IsColonist) // allows ai to autocast on themselves
                {
                    resultingLine = default(ShootLine);
                    __result = true;
                    return false;
                }

                if (__instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Blink" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Summon" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_PhaseStrike" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_BLOS" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_LightSkip" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_SootheAnimal" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Effect_EyeOfTheStorm" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Effect_Flight" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Regenerate" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_SpellMending" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_CauterizeWound" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Transpose" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Disguise" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_SoulBond" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_SummonDemon" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_EarthSprites" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Overdrive" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_BlankMind" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_MechaniteReprogramming" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_ShadowWalk" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_SoL_CreateLight" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Enrage" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Hex" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Discord" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_AdvancedHeal" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_ControlSpiritStorm" ||
                    __instance.verbProps.verbClass.ToString() == "TorannMagic.Verb_Scorn" ||
                    !__instance.verbProps.requireLineOfSight)
                {
                    //Ignores line of sight
                    //                    
                    if (__instance.CasterPawn != null && __instance.CasterPawn.RaceProps.Humanlike)
                    {
                        Pawn pawn = __instance.CasterPawn;
                        CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
                        if (comp != null && (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless) ||
                                             TM_ClassUtility.ClassHasAbility(TorannMagicDefOf.TM_Transpose,
                                                 null, comp)))
                        {
                            MightPowerSkill ver =
                                comp.MightData.MightPowerSkill_Transpose.FirstOrDefault((MightPowerSkill x) =>
                                    x.label == "TM_Transpose_ver");
                            if (ver.level < 3)
                            {
                                __result = true;
                                resultingLine = default(ShootLine);
                                return true;
                            }
                        }
                    }

                    resultingLine = new ShootLine(root, targ.Cell);
                    __result = true;
                    return false;
                }

                resultingLine = default(ShootLine);
                __result = true;
                return true;
            }
        }

        [HarmonyPatch(typeof(CastPositionFinder), "TryFindCastPosition", null)]
        public static class TryFindCastPosition_Base_Patch
        {
            private static bool Prefix(CastPositionRequest newReq, out IntVec3 dest) //, ref IntVec3 __result)
            {
                CastPositionRequest req = newReq;
                IntVec3 casterLoc = req.caster.Position;
                Verb verb = req.verb;
                dest = IntVec3.Invalid;
                bool isTMAbility = verb.verbProps.verbClass.ToString().Contains("TorannMagic") ||
                                   verb.verbProps.verbClass.ToString().Contains("AbilityUser");


                if (verb.CanHitTargetFrom(casterLoc, req.target) &&
                    (req.caster.Position - req.target.Position).LengthHorizontal < verb.verbProps.range &&
                    isTMAbility)
                {
                    //If in range and in los, cast immediately
                    dest = casterLoc;
                    //__result = dest;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_SkillTracker), "Learn", null)]
        public static class Pawn_SkillTracker_Base_Patch
        {
            private static bool Prefix(Pawn_SkillTracker __instance, Pawn ___pawn)
            {
                if (___pawn != null)
                {
                    if (___pawn.story.traits.HasTrait(TorannMagicDefOf.Undead))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SkillRecord), "Learn", null)]
        public static class SkillRecord_Patch
        {
            private static bool Prefix(SkillRecord __instance, Pawn ___pawn)
            {
                if (___pawn != null)
                {
                    if (___pawn.story.traits.HasTrait(TorannMagicDefOf.Undead))
                    {
                        return false;
                    }

                    if (___pawn is TMPawnGolem)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}