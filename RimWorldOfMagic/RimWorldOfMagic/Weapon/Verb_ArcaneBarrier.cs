using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;


namespace TorannMagic.Weapon
{
    public class Verb_ArcaneBarrier : Verb_UseAbility
    {
        private float xProbL;
        private float xProbR;

        private IntVec3 currentPosL = IntVec3.Invalid;
        private IntVec3 currentPosR = IntVec3.Invalid;

        private bool validTarg;
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
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
            Map map = base.CasterPawn.Map;

            IntVec3 destinationRPos = currentTarget.Cell;
            IntVec3 destinationLPos = currentTarget.Cell;
            currentPosL = currentTarget.Cell;
            currentPosR = currentTarget.Cell;
            IntVec3 angleVec = (currentTarget.Cell - CasterPawn.Position).RotatedBy(Rot4.FromAngleFlat(90));
            destinationRPos.x += angleVec.x;
            destinationRPos.z += angleVec.z;
            xProbR = CalculateAngles(currentTarget.Cell, destinationRPos);
            angleVec = (currentTarget.Cell - CasterPawn.Position).RotatedBy(Rot4.FromAngleFlat(-90));
            destinationLPos.x += angleVec.x;
            destinationLPos.z += angleVec.z;
            xProbL = CalculateAngles(currentTarget.Cell, destinationLPos);

            SpawnThings tempPod = new SpawnThings();
            tempPod.def = ThingDef.Named("TM_ArcaneBarrier");
            tempPod.spawnCount = 1;

            SingleSpawnLoop(tempPod, currentTarget.Cell, CasterPawn.Map);
            FleckMaker.ThrowHeatGlow(currentTarget.Cell, map, 1f);

            for (int i = 0; i < 5; i++)
            {
                currentPosR = GetNewPos(currentPosR, currentTarget.Cell.x <= destinationRPos.x, currentTarget.Cell.z <= destinationRPos.z, false, 0, 0, xProbR, 1 - xProbR);
                if(currentPosR.IsValid && currentPosR.InBoundsWithNullCheck(CasterPawn.Map) && !currentPosR.Impassable(CasterPawn.Map) && currentPosR.Walkable(CasterPawn.Map))
                {
                    bool flag = true;
                    foreach (Thing current in currentPosR.GetThingList(CasterPawn.Map))
                    {
                        if(current.def.altitudeLayer == AltitudeLayer.Building || current.def.altitudeLayer == AltitudeLayer.Item || current.def.altitudeLayer == AltitudeLayer.ItemImportant)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        SingleSpawnLoop(tempPod, currentPosR, CasterPawn.Map);
                        FleckMaker.ThrowHeatGlow(currentPosR, map, .6f);
                    }
                }
                currentPosL = GetNewPos(currentPosL, currentTarget.Cell.x <= destinationLPos.x, currentTarget.Cell.z <= destinationLPos.z, false, 0, 0, xProbL, 1 - xProbL);
                if (currentPosL.IsValid && currentPosL.InBoundsWithNullCheck(CasterPawn.Map) && !currentPosL.Impassable(CasterPawn.Map) && currentPosL.Walkable(CasterPawn.Map))
                {
                    bool flag = true;
                    foreach (Thing current in currentPosL.GetThingList(CasterPawn.Map))
                    {
                        if (current.def.altitudeLayer == AltitudeLayer.Building || current.def.altitudeLayer == AltitudeLayer.Item || current.def.altitudeLayer == AltitudeLayer.ItemImportant)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    { 
                        SingleSpawnLoop(tempPod, currentPosL, CasterPawn.Map);
                        FleckMaker.ThrowHeatGlow(currentPosL, map, .6f);
                    }
                }
            }
            burstShotsLeft = 0;
            return true;
        }

        private float CalculateAngles(IntVec3 originPos, IntVec3 destPos)
        {
            float hyp = Mathf.Sqrt((Mathf.Pow(originPos.x - destPos.x, 2)) + (Mathf.Pow(originPos.z - destPos.z, 2)));
            float angleRad = Mathf.Asin(Mathf.Abs(originPos.x - destPos.x) / hyp);
            float angleDeg = Mathf.Rad2Deg * angleRad;
            return angleDeg / 90;
        }

        public void SingleSpawnLoop(SpawnThings spawnables, IntVec3 position, Map map)
        {
            bool flag = spawnables.def != null;
            if (flag)
            {
                Faction faction = CasterPawn.Faction;
                ThingDef def = spawnables.def;
                ThingDef stuff = null;
                bool madeFromStuff = def.MadeFromStuff;
                if (madeFromStuff)
                {
                    stuff = ThingDefOf.BlocksGranite;
                }
                Thing thing = ThingMaker.MakeThing(def, stuff);
                GenSpawn.Spawn(thing, position, map, Rot4.North, WipeMode.Vanish, false);                
            }
        }

        private IntVec3 GetNewPos(IntVec3 curPos, bool xdir, bool zdir, bool halfway, float zvar, float xvar, float xguide, float zguide)
        {
            float rand = (float)Rand.Range(0, 100);
            bool flagx = rand <= ((xguide + Mathf.Abs(xvar)) * 100);

            bool flagy = rand <= ((zguide + Mathf.Abs(zvar)) * 100);
            if (halfway)
            {
                xvar = (-1 * xvar);
                zvar = (-1 * zvar);
            }

            if (xdir && zdir)
            {
                //top right
                if (flagx)
                {
                    if (xguide + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagy)
                {
                    if (zguide + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (xdir && !zdir)
            {
                //bottom right
                if (flagx)
                {
                    if (xguide + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagy)
                {
                    if ((-1 * zguide) + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (!xdir && zdir)
            {
                //top left
                if (flagx)
                {
                    if ((-1 * xguide) + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagy)
                {
                    if (zguide + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            if (!xdir && !zdir)
            {
                //bottom left
                if (flagx)
                {
                    if ((-1 * xguide) + xvar >= 0) { curPos.x++; }
                    else { curPos.x--; }
                }
                if (flagy)
                {
                    if ((-1 * zguide) + zvar >= 0) { curPos.z++; }
                    else { curPos.z--; }
                }
            }
            else
            {
                //no direction identified
            }
            return curPos;
        }
    }
}
