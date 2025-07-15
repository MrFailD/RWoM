using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using RimWorld;

namespace TorannMagic
{
    internal class Projectile_ChronostaticField : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 20;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 4;
        private int strikeNum = 1;
        private float radius = 5;
        private bool initialized;
        private List<IntVec3> cellList;
        private Pawn casterPawn;
        private IEnumerable<IntVec3> targets;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1800, false);
            Scribe_Values.Look<int>(ref strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<int>(ref strikeNum, "strikeNum", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_References.Look<Pawn>(ref casterPawn, "casterPawn", false);
            Scribe_Collections.Look<IntVec3>(ref cellList, "cellList", LookMode.Value);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {            
            base.Impact(hitThing);           
            ThingDef def = this.def;
            if (!initialized)
            {
                casterPawn = launcher as Pawn;
                CompAbilityUserMagic comp = casterPawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = comp.MagicData.MagicPowerSkill_ChronostaticField.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ChronostaticField_pwr");
                MagicPowerSkill ver = comp.MagicData.MagicPowerSkill_ChronostaticField.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ChronostaticField_ver");
                
                pwrVal = pwr.level;
                verVal = ver.level;
                if (casterPawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = casterPawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = casterPawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                arcaneDmg = comp.arcaneDmg;
                if (ModOptions.Settings.Instance.AIHardMode && !casterPawn.IsColonist)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                strikeDelay = strikeDelay - verVal;
                radius = this.def.projectile.explosionRadius;
                duration = Mathf.RoundToInt(radius * strikeDelay);
                initialized = true;
                //this.targets = GenRadial.RadialCellsAround(base.Position, this.radius, true);
                //cellList = targets.ToList<IntVec3>();
            }

            cellList = new List<IntVec3>();
            cellList.Clear();
            cellList = GenRadial.RadialCellsAround(Position, radius, true).ToList(); //this.radius instead of 2
            for (int i = 0; i < cellList.Count; i++)
            {
                if (cellList[i].IsValid && cellList[i].InBoundsWithNullCheck(Map))
                {
                    List<Thing> thingList = cellList[i].GetThingList(Map);
                    if (thingList != null && thingList.Count > 0)
                    {
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            Pawn pawn = thingList[j] as Pawn;
                            if (pawn != null)
                            {
                                RemoveFireAt(thingList[j].Position);
                                if (Rand.Chance(TM_Calc.GetSpellSuccessChance(casterPawn, pawn, false) * (.6f + (.1f * verVal))))
                                {
                                    IntVec3 targetCell = pawn.Position;
                                    targetCell.z++;
                                    LaunchFlyingObect(targetCell, pawn, 1, Mathf.RoundToInt(Rand.Range(1400, 1800) * (1f + (.2f * pwrVal)) * arcaneDmg));
                                }
                                else
                                {
                                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TM_ResistedSpell".Translate(), -1);
                                }
                            }
                        }
                    }
                }
            }
            age = duration;
            Destroy(DestroyMode.Vanish);
        }

        public void LaunchFlyingObect(IntVec3 targetCell, Pawn pawn, int force, int duration)
        {
            bool flag = targetCell != IntVec3.Invalid && targetCell != default(IntVec3);
            if (flag)
            {
                if (pawn != null && pawn.Position.IsValid && pawn.Spawned && pawn.Map != null && !pawn.Downed && !pawn.Dead)
                {
                    if (ModCheck.Validate.GiddyUp.Core_IsInitialized())
                    {
                        ModCheck.GiddyUp.ForceDismount(pawn);
                    }
                    FlyingObject_TimeDelay flyingObject = (FlyingObject_TimeDelay)GenSpawn.Spawn(ThingDef.Named("FlyingObject_TimeDelay"), pawn.Position, pawn.Map);
                    flyingObject.speed = .01f;
                    flyingObject.duration = duration;
                    flyingObject.Launch(casterPawn, targetCell, pawn);
                }
            }
        }
        
        private void RemoveFireAt(IntVec3 position)
        {            
            List<Thing> thingList = position.GetThingList(Map);
            if (thingList != null && thingList.Count > 0)
            {
                for (int i = 0; i < thingList.Count; i++)
                {
                    if(thingList[i].def == ThingDefOf.Fire)
                    {
                        //Log.Message("removing fire at " + position);
                        FleckMaker.ThrowHeatGlow(position, Map, .6f);
                        thingList[i].Destroy(DestroyMode.Vanish);
                        i--;
                    }
                }
            }
        }
    }    
}