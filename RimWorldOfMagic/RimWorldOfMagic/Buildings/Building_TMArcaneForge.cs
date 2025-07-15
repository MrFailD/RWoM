using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMArcaneForge : Building_WorkTable
    {
        private Thing targetThing;

        //Saved forge recipe variables
        private bool hasSavedRecipe;
        private ThingDef copiedThingDef;
        private ThingDef copiedStuffDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref hasSavedRecipe, "hasSavedRecipe", false, false);
            Scribe_Defs.Look<ThingDef>(ref copiedThingDef, "copiedThingDef");
            Scribe_Defs.Look<ThingDef>(ref copiedStuffDef, "copiedStuffDef");
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag && hasSavedRecipe)
            {
                RestoreForgeRecipeAfterLoad();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            var gizmoList = base.GetGizmos().ToList();
            if (ResearchProjectDef.Named("TM_ForgeReplication").IsFinished)
            {
                bool canScan = true;
                for (int i = 0; i < BillStack.Count; i++)
                {
                    if (BillStack[i].recipe.defName == "ArcaneForge_Replication")
                    {
                        canScan = false;
                    }
                }

                if (canScan)
                {
                    TargetingParameters newParameters = new TargetingParameters();
                    newParameters.canTargetItems = true;
                    newParameters.canTargetBuildings = true;
                    newParameters.canTargetLocations = true;


                    String label = "TM_Replicate".Translate();
                    String desc = "TM_ReplicateDesc".Translate();

                    Command_LocalTargetInfo item = new Command_LocalTargetInfo
                    {
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = 67,
                        icon = ContentFinder<Texture2D>.Get("UI/replicate", true),
                        targetingParams = newParameters
                    };
                    item.action = delegate(LocalTargetInfo thing)
                    {
                        IntVec3 localCell = thing.Cell;
                        targetThing = thing.Cell.GetFirstItem(Map);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Scan,
                            localCell.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Map, 1.2f, .8f, 0f,
                            .5f, -400, 0, 0, Rand.Range(0, 360));
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Scan,
                            localCell.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Map, 1.2f, .8f, 0f,
                            .5f, 400, 0, 0, Rand.Range(0, 360));
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Scan, DrawPos, Map, 1.2f, .8f, 0f,
                            .5f, 400, 0, 0, Rand.Range(0, 360));
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Scan, DrawPos, Map, 1.2f, .8f, 0f,
                            .5f, -400, 0, 0, Rand.Range(0, 360));
                        SoundInfo info = SoundInfo.InMap(new TargetInfo(thing.Cell, Map, false),
                            MaintenanceType.None);
                        info.pitchFactor = 1.3f;
                        info.volumeFactor = 1.3f;
                        SoundDefOf.TurretAcquireTarget.PlayOneShot(info);
                        if (targetThing != null && targetThing.def.EverHaulable)
                        {
                            ClearReplication();
                            Replicate();
                        }
                        else
                        {
                            Messages.Message("TM_FoundNoReplicateTarget".Translate(),
                                MessageTypeDefOf.CautionInput);
                        }
                    };
                    gizmoList.Add(item);
                }
                else
                {
                    String label = "TM_ReplicateDisabled".Translate();
                    String desc = "TM_ReplicateDisabledDesc".Translate();

                    Command_Action item2 = new Command_Action
                    {
                        defaultLabel = label,
                        defaultDesc = desc,
                        Order = 68,
                        icon = ContentFinder<Texture2D>.Get("UI/replicateDisabled", true),
                        action = delegate { ClearReplication(); }
                    };
                    gizmoList.Add(item2);
                }
            }

            return gizmoList;
        }

        private const string ArcaneForgeReplicationDefName = "ArcaneForge_Replication";

        private void ClearReplication()
        {
            var bills = BillStack.Bills;
            for (int billIndex = bills.Count - 1; billIndex >= 0; billIndex--)
            {
                var bill = bills[billIndex];
                if (bill.recipe.defName == ArcaneForgeReplicationDefName)
                {
                    ClearReplicationJobsForAllColonists(bill.recipe.defName);
                    bills.RemoveAt(billIndex);
                }
            }
        }

        private void ClearReplicationJobsForAllColonists(string replicationRecipeDefName)
        {
            var allColonists = Map.mapPawns.AllPawnsSpawned;
            foreach (var pawn in allColonists)
            {
                if (pawn.IsColonist && pawn.RaceProps.Humanlike && pawn.CurJob != null && pawn.CurJob.bill != null)
                {
                    if (pawn.CurJob.bill.recipe.defName == replicationRecipeDefName)
                    {
                        pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.Incompletable, false);
                    }
                }
            }
        }

        private void Replicate(ThingDef repThingDef = null, ThingDef repStuffDef = null)
        {
            // Extract methods for clarity
            ThingDef replicatedThingDef, replicatedStuffDef;
            GetReplicatedDefs(repThingDef, repStuffDef, out replicatedThingDef, out replicatedStuffDef);

            var forgeRecipe = TorannMagicDefOf.ArcaneForge_Replication;
            RecipeDef replicant = null;
            var potentialRecipes = GetPotentialRecipes(replicatedThingDef);

            // Check for possible gemstone recipe and add it if found
            var gemstoneRecipe = CheckForGemstone(replicatedThingDef);
            if (gemstoneRecipe != null)
                potentialRecipes.Add(gemstoneRecipe);

            if (replicatedThingDef != null && potentialRecipes.Count > 0)
            {
                replicant = potentialRecipes.RandomElement();
                if (!HandleIngredientValidation(replicant, replicatedThingDef))
                    return;

                forgeRecipe.ingredients.Clear();
                AddFixedIngredients(forgeRecipe, replicant);
                AddStuffIngredientIfNeeded(forgeRecipe, replicatedThingDef, replicatedStuffDef);

                CopyRecipeDetails(forgeRecipe, replicant, replicatedThingDef, replicatedStuffDef, true);

                if (forgeRecipe.ingredients.Count == 0)
                {
                    HandleRestoreFallback(forgeRecipe);
                }
            }
            else
            {
                Messages.Message(
                    "TM_FoundNoReplicateRecipe".Translate(targetThing.def.defName),
                    MessageTypeDefOf.CautionInput
                );
            }
        }

// -- Extracted and Renamed Methods --

        private void GetReplicatedDefs(ThingDef repThingDef, ThingDef repStuffDef,
            out ThingDef replicatedThingDef, out ThingDef replicatedStuffDef)
        {
            if (repThingDef == null)
            {
                CheckForUnfinishedThing();
                replicatedThingDef = targetThing.def;
                replicatedStuffDef = targetThing.Stuff;
            }
            else
            {
                replicatedThingDef = repThingDef;
                replicatedStuffDef = repStuffDef;
            }
        }

        private List<RecipeDef> GetPotentialRecipes(ThingDef replicatedThingDef)
        {
            var recipes = new List<RecipeDef>();
            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs)
            {
                if (!(recipeDef is MagicRecipeDef)
                    && recipeDef.defName.Contains(replicatedThingDef.defName)
                    && !recipeDef.defName.Contains("Administer")
                    && !recipeDef.label.Contains("Replicate")
                    && !recipeDef.label.Contains("Install")
                    && !recipeDef.label.Contains("install"))
                {
                    recipes.Add(recipeDef);
                }
            }

            return recipes;
        }

        private bool HandleIngredientValidation(RecipeDef replicant, ThingDef replicatedThingDef)
        {
            if (replicant == null || replicant.ingredients == null) return true;

            foreach (var ingredient in replicant.ingredients)
            {
                if (!ingredient.IsFixedIngredient && !replicatedThingDef.MadeFromStuff)
                {
                    Messages.Message(
                        "TM_ReplicatedUnrecognizedIngredient".Translate(replicatedThingDef.LabelCap),
                        MessageTypeDefOf.RejectInput
                    );
                    return false;
                }
            }

            return true;
        }

        private void AddFixedIngredients(RecipeDef forgeRecipe, RecipeDef replicant)
        {
            if (replicant.ingredients == null) return;

            foreach (var ingredient in replicant.ingredients)
            {
                if (ingredient.filter != null &&
                    ingredient.filter.ToString() != "ingredients" &&
                    ingredient.IsFixedIngredient)
                {
                    forgeRecipe.ingredients.Add(ingredient);
                }
            }
        }

        private void AddStuffIngredientIfNeeded(RecipeDef forgeRecipe, ThingDef replicatedThingDef,
            ThingDef replicatedStuffDef)
        {
            if (replicatedStuffDef != null && replicatedThingDef.MadeFromStuff)
            {
                var stuffIngredient = new IngredientCount();
                stuffIngredient.filter.SetAllow(replicatedStuffDef, true);
                stuffIngredient.SetBaseCount(replicatedThingDef.costStuffCount);
                forgeRecipe.ingredients.Add(stuffIngredient);
            }
        }

        private void CopyRecipeDetails(RecipeDef forgeRecipe, RecipeDef replicant,
            ThingDef replicatedThingDef, ThingDef replicatedStuffDef, bool saved)
        {
            forgeRecipe.workAmount = replicant.workAmount;
            forgeRecipe.description = replicant.description;
            forgeRecipe.label = "Replicate " + replicant.label;
            forgeRecipe.unfinishedThingDef = replicant.unfinishedThingDef;
            forgeRecipe.products = replicant.products;
            copiedThingDef = replicatedThingDef;
            copiedStuffDef = replicatedStuffDef;
            hasSavedRecipe = saved;
        }

        private void HandleRestoreFallback(RecipeDef forgeRecipe)
        {
            forgeRecipe.ingredients.Clear();
            var replicant = TorannMagicDefOf.ArcaneForge_Replication_Restore;
            if (replicant.ingredients.Count > 0)
            {
                foreach (var ingredient in replicant.ingredients)
                {
                    if (ingredient.filter.ToString() != "ingredients")
                    {
                        forgeRecipe.ingredients.Add(ingredient);
                    }
                }

                CopyRecipeDetails(forgeRecipe, replicant, null, null, false);
            }
        }

        private void CheckForUnfinishedThing()
        {
            Thing unfinishedThing = Position.GetFirstItem(Map);
            if (unfinishedThing != null && unfinishedThing.def.isUnfinishedThing)
            {
                unfinishedThing.Destroy(DestroyMode.Cancel);
            }
        }

        private RecipeDef CheckForGemstone(ThingDef td)
        {
            RecipeDef returnedRecipe = null;
            String gemString = "Cut";
            String gemType = "";
            String gemQual = "";
            if (td.defName.Contains("wonder"))
            {
                gemType = "wonder";
            }

            if (td.defName.Contains("maxMP"))
            {
                gemType = "MPGem";
            }

            if (td.defName.Contains("mpRegenRate"))
            {
                gemType = "MPRegenRateGem";
            }

            if (td.defName.Contains("mpCost"))
            {
                gemType = "MPCostGem";
            }

            if (td.defName.Contains("coolDown"))
            {
                gemType = "CoolDownGem";
            }

            if (td.defName.Contains("xpGain"))
            {
                gemType = "XPGainGem";
            }

            if (td.defName.Contains("arcaneRes"))
            {
                gemType = "ArcaneResGem";
            }

            if (td.defName.Contains("arcaneDmg"))
            {
                gemType = "ArcaneDmgGem";
            }

            if (td.defName.Contains("_major"))
            {
                gemQual = "Major";
            }

            if (td.defName.Contains("_minor"))
            {
                gemQual = "Minor";
            }

            gemString += gemQual;
            gemString += gemType;


            IEnumerable<RecipeDef> enumerable = from def in DefDatabase<RecipeDef>.AllDefs
                where (def.defName == gemString)
                select def;

            foreach (RecipeDef current in enumerable)
            {
                returnedRecipe = current;
            }

            return returnedRecipe;
        }

        private void RestoreForgeRecipeAfterLoad()
        {
            if (hasSavedRecipe)
            {
                Replicate(copiedThingDef, copiedStuffDef);
            }
            else
            {
                ClearReplication();
            }
        }
    }
}