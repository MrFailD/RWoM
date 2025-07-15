using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using RimWorld;

namespace TorannMagic.Conditions
{
    internal class GameCondition_ElementalAssault : GameCondition
    {
        public IntVec2 centerLocation;
        private int areaRadius = 2;
        private bool initialized;
        private bool disabled;
        public Thing thing;

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if(Find.TickManager.TicksGame % 60 == 0)
            {
                if(thing.DestroyedOrNull())
                {
                    End();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref thing, "thing", false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<IntVec2>(ref centerLocation, "centerLocation", default(IntVec2), false);
            Scribe_Values.Look<bool>(ref disabled, "disabled", false, false);
        }

        public override void Init()
        {           
                        
            if (ModOptions.Settings.Instance.riftChallenge > 0)
            {
                base.Init();
                disabled = false;
                FindGoodCenterLocation();
                thing = ThingMaker.MakeThing(ThingDef.Named("TM_ElementalRift"), ThingDefOf.BlocksGranite);
                GenSpawn.Spawn(thing, centerLocation.ToIntVec3, SingleMap, Rot4.North, WipeMode.Vanish, false);
                Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("TM_ElementalFaction"));
                if (!faction.HostileTo(Faction.OfPlayer))
                {
                    thing.SetFaction(Faction.OfMechanoids, null);
                }
                else
                {
                    thing.SetFaction(faction, null);
                }
            }
            else
            {
                disabled = true;
                Log.Message("Rift spawning disabled.");
            }
        }

        public override void End()
        {
            IntVec3 thingLoc = centerLocation.ToIntVec3;
            List<Thing> thingList;
            Thing destroyable = null;
            thingList = thingLoc.GetThingList(SingleMap);
            int z = 0;
            while (z < thingList.Count)
            {
                destroyable = thingList[z];
                if (destroyable != null)
                {
                    bool validator = destroyable is Building;
                    if (validator)
                    {
                        if(destroyable.def == ThingDef.Named("TM_ElementalRift"))
                        {
                            destroyable.Destroy(DestroyMode.Vanish);
                        }
                    }
                }
                z++;
            }
            
            if (!disabled)
            {
                Thing thing = null;
                thing = ThingMaker.MakeThing(ThingDef.Named("Jade"));
                thing.stackCount = Rand.Range(35 * (int)ModOptions.Settings.Instance.riftChallenge, 60 * (int)ModOptions.Settings.Instance.riftChallenge);
                if (thing != null)
                {
                    GenPlace.TryPlaceThing(thing, thingLoc, SingleMap, ThingPlaceMode.Near, null);
                }

                int totalMarketValue = Mathf.RoundToInt(1000f * (ModOptions.Settings.Instance.riftChallenge * ModOptions.Settings.Instance.riftChallenge));
                List<Thing> list = new List<Thing>();
                ItemCollectionGenerator_Gemstones itc_g = new ItemCollectionGenerator_Gemstones();
                list = itc_g.Generate(totalMarketValue, list);
                for (int i = 0; i < list.Count; i++)
                {
                    thing = list[i];
                    thing.stackCount = list[i].stackCount;
                    GenPlace.TryPlaceThing(thing, thingLoc, SingleMap, ThingPlaceMode.Near, null);
                }
            }
            base.End();
        }

        private void FindGoodCenterLocation()
        {
            if (SingleMap.Size.x <= 16 || SingleMap.Size.z <= 16)
            {
                throw new Exception("Map too small for elemental assault");
            }
            for (int i = 0; i < 16; i++)
            {
                centerLocation = new IntVec2(Rand.Range(8, SingleMap.Size.x - 8), Rand.Range(8, SingleMap.Size.z - 8));
                if (IsGoodCenterLocation(centerLocation))
                {
                    break;
                }
            }
        }

        private bool IsGoodLocationForSpawn(IntVec3 loc)
        {
            return loc.InBoundsWithNullCheck(SingleMap) && !loc.Roofed(SingleMap) && loc.Standable(SingleMap) && loc.IsValid && !loc.Fogged(SingleMap) && loc.Walkable(SingleMap);
        }

        private bool IsGoodCenterLocation(IntVec2 loc)
        {
            int num = 0;
            int num2 = (int)(3.14159274f * (float)areaRadius * (float)areaRadius / 2f);
            foreach (IntVec3 current in GetPotentiallyAffectedCells(loc))
            {
                if (IsGoodLocationForSpawn(current))
                {
                    num++;
                }
                if (num >= num2)
                {
                    break;
                }
            }
            
            return num >= num2 && (IsGoodLocationForSpawn(loc.ToIntVec3));
        }

        [DebuggerHidden]
        private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
        {
            for (int x = center.x - areaRadius; x <= center.x + areaRadius; x++)
            {
                for (int z = center.z - areaRadius; z <= center.z + areaRadius; z++)
                {
                    if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= areaRadius * areaRadius)
                    {
                        yield return new IntVec3(x, 0, z);
                    }
                }
            }
        }
    }
}
