using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using RimWorld;
using System.Collections.Generic;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_ThunderStrike : Projectile_AbilityBase
    {
        private Vector3 origin;
        private Vector3 destination;
        private Vector3 direction;
        private Vector3 directionOffsetRight;
        private Vector3 directionOffsetLeft;

        private int iteration;
        private int maxIteration = 4;
        private float directionMagnitudeOffset = 1.5f;
        private bool initialized;

        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;

        private int nextEventTick;
        private int nextRightEventTick;
        private int nextLeftEventTick;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref destination, "destination", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref direction, "direction", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref directionOffsetRight, "directionOffsetRight", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref directionOffsetLeft, "directionOffsetLeft", default(Vector3), false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref iteration, "iteration", 0, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing);

            if(!initialized)
            {
                Initialize(Position, launcher as Pawn);
            }
            Vector3 directionOffset = default(Vector3);

            if(initialized && nextLeftEventTick < Find.TickManager.TicksGame && nextLeftEventTick != 0)
            {
                directionOffset = directionOffsetLeft * (directionMagnitudeOffset * iteration);
                DoThunderStrike(directionOffset);
                nextLeftEventTick = 0;
            }
            if(initialized && nextRightEventTick < Find.TickManager.TicksGame && nextRightEventTick != 0)
            {
                directionOffset = directionOffsetRight * (directionMagnitudeOffset * iteration);
                DoThunderStrike(directionOffset);
                nextRightEventTick = 0;
            }
            if (initialized && nextEventTick < Find.TickManager.TicksGame)
            {               
                
                if(iteration == 1 && verVal > 0)
                {
                    nextRightEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);
                    nextLeftEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);                    
                }
                if (iteration == 3 && verVal > 1)
                {
                    nextRightEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);
                    nextLeftEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);
                }
                if (iteration == 5 && verVal > 2)
                {
                    nextRightEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);
                    nextLeftEventTick = Find.TickManager.TicksGame + Rand.Range(2, 6);
                }
                iteration++;
                directionOffset = direction * (directionMagnitudeOffset * iteration);
                DoThunderStrike(directionOffset);

                nextEventTick = Find.TickManager.TicksGame + Rand.Range(2,5);                
            }                       

        }

        private void DoThunderStrike(Vector3 directionOffset)
        {
            IntVec3 currentPos = default(IntVec3);
            if (directionOffset != default(Vector3))
            {
                currentPos = (origin + directionOffset).ToIntVec3();
                if (currentPos != default(IntVec3) && currentPos.IsValid && currentPos.InBoundsWithNullCheck(Map) && currentPos.Walkable(Map) && currentPos.DistanceToEdge(Map) > 3)
                {
                    CellRect cellRect = CellRect.CenteredOn(currentPos, 1);
                    //cellRect.ClipInsideMap(base.Map);
                    IntVec3 rndCell = cellRect.RandomCell;
                    if (rndCell != IntVec3.Invalid && rndCell != default(IntVec3) && rndCell.IsValid && rndCell.InBoundsWithNullCheck(Map) && rndCell.Walkable(Map) && rndCell.DistanceToEdge(Map) > 3)
                    {
                        Map.weatherManager.eventHandler.AddEvent(new TM_WeatherEvent_MeshFlash(Map, rndCell, TM_MatPool.chiLightning, TMDamageDefOf.DamageDefOf.TM_ChiBurn, launcher, Mathf.RoundToInt(Rand.Range(8, 14) * (1 +(.12f * pwrVal)) * arcaneDmg), Rand.Range(1.5f, 2f)));
                    }
                }
            }
        }

        private void Initialize(IntVec3 target, Pawn pawn)
        {
            if (target != IntVec3.Invalid && pawn != null)
            {
                verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_ThunderStrike, false);
                pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_ThunderStrike, false);
                //verVal = TM_Calc.GetMightSkillLevel(pawn, pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_ThunderStrike, "TM_ThunderStrike", "_ver", true);
                //pwrVal = TM_Calc.GetMightSkillLevel(pawn, pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_ThunderStrike, "TM_ThunderStrike", "_pwr", true);
                //this.verVal = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_ThunderStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ThunderStrike_ver").level;
                //this.pwrVal = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_ThunderStrike.FirstOrDefault((MightPowerSkill x) => x.label == "TM_ThunderStrike_pwr").level;
                //if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                //{
                //    MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                //    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                //    verVal = mver.level;
                //    pwrVal = mpwr.level;
                //}
                arcaneDmg = pawn.GetCompAbilityUserMight().mightPwr;
                origin = pawn.Position.ToVector3Shifted();
                destination = target.ToVector3Shifted();
                direction = TM_Calc.GetVector(origin, destination);
                directionOffsetRight = Quaternion.AngleAxis(30, Vector3.up) * direction;
                directionOffsetLeft = Quaternion.AngleAxis(-30, Vector3.up) * direction;
                //Log.Message("origin: " + this.origin + " destination: " + this.destination + " direction: " + this.direction + " directionRight: " + this.directionOffsetRight);
                maxIteration = maxIteration + verVal;
                initialized = true;
                HealthUtility.AdjustSeverity(pawn, TorannMagicDefOf.TM_HediffInvulnerable, .05f);
            }
            else
            {
                Log.Warning("Failed to initialize " + def.defName);
                iteration = maxIteration;
            }            
        }        

        public override void Tick()
        {
            base.Tick();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (iteration >= maxIteration)
            {
                Pawn pawn = launcher as Pawn;
                if(!pawn.DestroyedOrNull() && !pawn.Dead && pawn.Spawned)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_HediffInvulnerable, false);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
                base.Destroy(mode);
            }
        }
    }
}
