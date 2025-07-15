using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    public class CompLeaper : ThingComp
    {
        private bool initialized = true;
        public float explosionRadius = 2f;
        private int nextLeap;

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

        public override void CompTick()
        {
            base.CompTick();
            if (Pawn.Spawned)
            {                
                if (Find.TickManager.TicksGame % nextLeap == 0 && !Pawn.Downed && !Pawn.Dead)
                {
                    LocalTargetInfo lti = null;
                    if (Pawn.CurJob != null && Pawn.CurJob.targetA != null)
                    {
                        lti = Pawn.jobs.curJob.targetA.Thing;
                    }
                    if (lti != null && lti.Thing != null)
                    {
                        Thing target = lti.Thing;
                        if (target is Pawn && target.Spawned)
                        {
                            float targetRange = (target.Position - Pawn.Position).LengthHorizontal;
                            if (targetRange <= Props.leapRangeMax && targetRange > Props.leapRangeMin)
                            {
                                if (Rand.Chance(Props.GetLeapChance))
                                {
                                    if (CanHitTargetFrom(Pawn.Position, target))
                                    {
                                        LeapAttack(target);
                                    }
                                }
                                else
                                {
                                    if (Props.textMotes)
                                    {
                                        if (Rand.Chance(.5f))
                                        {
                                            MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "grrr", -1);
                                        }
                                        else
                                        {
                                            MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "hsss", -1);
                                        }
                                    }
                                }
                            }
                            else if (Props.bouncingLeaper)
                            {
                                Faction targetFaction = null;
                                if (target != null && target.Faction != null)
                                {
                                    targetFaction = target.Faction;
                                }

                                List<Pawn> list = new List<Pawn>();
                                list.Clear();
                                list = (from x in Pawn.Map.mapPawns.AllPawnsSpawned
                                        where x.Position.InHorDistOf(Pawn.Position, (float)Props.leapRangeMax) && x.Faction == targetFaction && !x.DestroyedOrNull() && !x.Downed
                                        select x).ToList<Pawn>();

                                if (list.Count > 0)
                                {
                                    Pawn bounceTarget = list.RandomElement();
                                    if (Rand.Chance(1 - Props.leapChance))
                                    {
                                        if (CanHitTargetFrom(Pawn.Position, target))
                                        {
                                            LeapAttack(bounceTarget);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Find.TickManager.TicksGame % 10 == 0)
                {
                    if (Pawn.Downed && !Pawn.Dead)
                    {
                        ExplosionHelper.Explode(Pawn.Position, Pawn.Map, Rand.Range(explosionRadius * .5f, explosionRadius * 1.5f), DamageDefOf.Burn, Pawn, Rand.Range(6, 10), 0, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                        Pawn.Kill(null, null);
                    }
                }
            }
        }

        public void LeapAttack(LocalTargetInfo target)
        {                
            bool flag = target != null && target.Cell != default(IntVec3);
            if (flag)
            {
                if (Pawn != null && Pawn.Position.IsValid && Pawn.Spawned && Pawn.Map != null && !Pawn.Downed && !Pawn.Dead && !target.Thing.DestroyedOrNull())
                {
                    Pawn.jobs.StopAll();
                    FlyingObject_Leap flyingObject = (FlyingObject_Leap)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Leap"), Pawn.Position, Pawn.Map);
                    flyingObject.Launch(Pawn, target.Cell, Pawn);
                }
            }            
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            initialized = true;
            Pawn pawn = parent as Pawn;
            nextLeap = Mathf.RoundToInt(Rand.Range(Props.ticksBetweenLeapChance * .75f, 1.25f * Props.ticksBetweenLeapChance));
            explosionRadius = Props.explodingLeaperRadius * Rand.Range(.8f, 1.25f);
        }

        public CompProperties_Leaper Props
        {
            get
            {
                return (CompProperties_Leaper)props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
        }

        private bool CanHitTargetFrom(IntVec3 pawn, LocalTargetInfo target)
        {
            bool result = false;
            if (target.IsValid && target.CenterVector3.InBoundsWithNullCheck(Pawn.Map) && !target.Cell.Fogged(Pawn.Map) && target.Cell.Walkable(Pawn.Map))
            {
                ShootLine shootLine;
                result = TryFindShootLineFromTo(pawn, target, out shootLine);                
            }
            else
            {
                result = false;
            }
            
            return result;
        }

        public bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
        {
            if (targ.HasThing && targ.Thing.Map != Pawn.Map)
            {
                resultingLine = default(ShootLine);
                return false;
            }
            resultingLine = new ShootLine(root, targ.Cell);
            if (!GenSight.LineOfSightToEdges(root, targ.Cell, Pawn.Map, true, null))
            {
                return false;
            }
            return true;
        }
    }
}
