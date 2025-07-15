using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using AbilityUser;
using Verse;
using UnityEngine;


namespace TorannMagic
{
    public class Verb_RegrowLimb : Verb_UseAbility
    {

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            bool validTarg = false;
            if (targ.Thing != null && targ.Thing == caster)
            {
                return verbProps.targetParams.canTargetSelf;
            }
            if (targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    ShootLine shootLine;
                    validTarg = TryFindShootLineFromTo(root, targ, out shootLine);
                }
                else
                {
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        protected override bool TryCastShot()
        {

            CellRect cellRect = CellRect.CenteredOn(currentTarget.Cell, 1);
            cellRect.ClipInsideMap(CasterPawn.Map);
            IntVec3 centerCell = cellRect.CenterCell;
            Map map = CasterPawn.Map;

            Pawn caster = base.CasterPawn;

            if ((centerCell.IsValid && centerCell.Standable(map)))
            {
                SpawnThings tempThing = new SpawnThings();
                tempThing.def = ThingDef.Named("SeedofRegrowth");
                SingleSpawnLoop(tempThing, centerCell, map);
            }
            else
            {
                Messages.Message("InvalidSummon".Translate(), MessageTypeDefOf.RejectInput);
            }
            return false;
        }

        public static void SingleSpawnLoop(SpawnThings spawnables, IntVec3 position, Map map)
        {
            bool flag = spawnables.def != null;
            if (flag)
            {
                ThingDef def = spawnables.def;
                ThingDef stuff = null;
                bool madeFromStuff = def.MadeFromStuff;
                if (madeFromStuff)
                {
                    stuff = ThingDefOf.WoodLog;
                }
                Thing thing = ThingMaker.MakeThing(def, stuff);
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near);
                //GenSpawn.Spawn(thing, position, map, Rot4.North, WipeMode.Vanish, false);                
            }
        }
    }
}
