using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using AbilityUser;

namespace TorannMagic
{
    public class CompSentinel : ThingComp
	{
        private bool initialized;
        private List<Pawn> threatList = new List<Pawn>();
        public LocalTargetInfo target = null;
        private int age = -1;
        private bool shouldDespawn;
        public IntVec3 sentinelLoc;
        public Rot4 rotation;
        public Pawn sustainerPawn;

        private int killNow;

        private int threatRange = 40;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<Rot4>(ref rotation, "rotation", Rot4.South, false);
            Scribe_Values.Look<IntVec3>(ref sentinelLoc, "sentinelLoc", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref sustainerPawn, "sustainerPawn", false);
        }

        private Pawn Pawn
        {
            get
            {
                Pawn pawn = parent as Pawn;
                bool flag = pawn == null;
                if (flag)
                {
                    Log.Error("pawn is null");
                }
                return pawn;
            }
        }

        private bool ShouldDespawn
        {
            get
            {
                return target == null && shouldDespawn;
            }
        }

        public override void CompTick()
        {
            if (age > 0)
            {
                if (!initialized)
                {
                    initialized = true;
                }

                if (Pawn.Spawned)
                {
                    if (!Pawn.Downed)
                    {
                        if (Find.TickManager.TicksGame % 300 == 0)
                        {
                            if (sustainerPawn != null)
                            {
                                DetermineThreats();
                                DetermineSustainerPawn();

                                if (ShouldDespawn && Pawn.Position != sentinelLoc)
                                {
                                    if (sentinelLoc.Walkable(Pawn.Map))
                                    {
                                        //this.Pawn.jobs.ClearQueuedJobs();
                                        //this.Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                                        Job job = new Job(JobDefOf.Goto, sentinelLoc);
                                        Pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                                    }
                                    else
                                    {
                                        Messages.Message("TM_SentinelCannotReturn".Translate(
                                        sustainerPawn.LabelShort
                                        ), MessageTypeDefOf.RejectInput, false);
                                        Pawn.Destroy(DestroyMode.Vanish);
                                    }

                                }

                                if(target.Thing is Pawn prisonerPawn)
                                {
                                    if(prisonerPawn.IsPrisoner)
                                    {
                                        Job job = new Job(JobDefOf.AttackMelee, prisonerPawn);
                                        Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                    }
                                }
                            }
                            else
                            {
                                Log.Message("Sentinel has despawned due to lack of mana to sustain it.");
                                Pawn.Destroy(DestroyMode.Vanish);
                            }
                        }
                    }
                    else
                    { 
                        if (killNow > 100)
                        {
                            DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, 100, 0, (float)-1, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                            Pawn.TakeDamage(dinfo);
                        }
                        killNow++;
                    }

                    if(ShouldDespawn && Pawn.Position == sentinelLoc)
                    {
                        SingleSpawnLoop();
                        Pawn.Destroy(DestroyMode.Vanish);
                    }
                    
                }                
            }
            age++;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if(dinfo.Instigator != null)
            {
                Thing instigatorThing = dinfo.Instigator;
                if(instigatorThing is Building)
                {
                    if (instigatorThing.Faction != null && instigatorThing.Faction != Pawn.Faction)
                    {
                        
                    }
                }
            }
        }

        private void DetermineSustainerPawn()
        {
            if(sustainerPawn.DestroyedOrNull() || sustainerPawn.Dead)
            {
                Pawn.Kill(null, null);
            }
        }

        private void DetermineThreats()
        {
            target = null;
            try
            {                
                List<Pawn> allPawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
                for (int i = 0; i < allPawns.Count(); i++)
                {
                    if (!allPawns[i].DestroyedOrNull() && allPawns[i] != Pawn)
                    {
                        if (!allPawns[i].Dead && !allPawns[i].Downed && !allPawns[i].IsPrisonerInPrisonCell())
                        {
                            if ((allPawns[i].Position - Pawn.Position).LengthHorizontal <= threatRange)
                            {
                                if (allPawns[i].Faction != null && allPawns[i].Faction != Pawn.Faction)
                                {
                                    if (FactionUtility.HostileTo(Pawn.Faction, allPawns[i].Faction))
                                    {
                                        if(ModCheck.Validate.PrisonLabor.IsInitialized())
                                        {
                                            if(!allPawns[i].IsPrisoner)
                                            {
                                                target = allPawns[i];
                                                break;
                                            }                                            
                                        }
                                        else
                                        {
                                            target = allPawns[i];
                                            break;
                                        }                                      
                                    }
                                }
                            }
                        }
                    }
                }

                if (target != null && Pawn.meleeVerbs.TryGetMeleeVerb(target.Thing) != null)
                {
                    Thing currentTargetThing = Pawn.CurJob.targetA.Thing;
                    if (currentTargetThing == null)
                    {
                        Pawn.TryStartAttack(target);
                    }
                }
                else
                {
                    shouldDespawn = true;
                }
            }
            catch(NullReferenceException ex)
            {
                //Log.Message("Error processing threats" + ex);
            }
        }

        public void DamageEntities(Thing e, float d, DamageDef type, Thing instigator)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, Rand.Range(0,amt), (float)-1, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }

        public bool TargetIsValid(Thing target)
        {
            if(target.DestroyedOrNull())
            {
                return false;
            }
            if(!target.Spawned)
            {
                return false;
            }
            if(target is Pawn targetPawn)
            {
                return !targetPawn.Downed;
            }
            if(target.Position.DistanceToEdge(Pawn.Map) < 8)
            {
                return false;
            }
            if(target.Faction != null)
            {
                return target.Faction != Pawn.Faction;
            }
            return true;
        }

        public void SingleSpawnLoop()
        {
            Thing spawnedThing = null;
            SpawnThings spawnables = new SpawnThings();
            spawnables.def = ThingDef.Named("TM_Sentinel");
            spawnables.spawnCount = 1;
            if (spawnables.def != null)
            {
                ThingDef def = spawnables.def;
                ThingDef stuff = null;
                bool madeFromStuff = def.MadeFromStuff;
                if (madeFromStuff)
                {
                    stuff = ThingDef.Named("BlocksGranite");
                }
                spawnedThing = ThingMaker.MakeThing(def, stuff);
                GenSpawn.Spawn(spawnedThing, sentinelLoc, Pawn.Map, rotation, WipeMode.Vanish, false);

                float healthDeficit = Pawn.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>()
                    .Sum(injury => injury.Severity);

                CompAbilityUserMagic comp = sustainerPawn.GetCompAbilityUserMagic();
                comp.summonedSentinels.Remove(Pawn);
                comp.summonedSentinels.Add(spawnedThing);
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, 10*healthDeficit, 0, (float)-1, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                spawnedThing.TakeDamage(dinfo);

            }
        }
    }
}