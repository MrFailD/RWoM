using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TorannMagic.Weapon;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_PoisonTrap : Building_ExplosiveProximityTrap
    {
        private int age = -1;
        private int duration = 480;
        private int strikeDelay = 40;
        private int lastStrike;
        private bool triggered;
        private const float Radius = 3f;
        private const int TicksTillReArm = 15000;
        private bool rearming;
        private ThingDef fog;

        private bool destroyAfterUse;

        private static readonly Material trap_rearming = MaterialPool.MatFrom("Other/PoisonTrap_rearming");
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref TouchingPawns, "testees", LookMode.Reference);
            Scribe_Values.Look(ref triggered, "triggered");
            Scribe_Values.Look(ref rearming, "rearming");
            Scribe_Values.Look(ref destroyAfterUse, "destroyAfterUse");
            Scribe_Values.Look(ref age, "age", -1);
            Scribe_Values.Look(ref duration, "duration", 600);
            Scribe_Values.Look(ref strikeDelay, "strikeDelay");
            Scribe_Values.Look(ref lastStrike, "lastStrike");
            Scribe_Defs.Look(ref fog, "fog");
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (rearming)
            {
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(DrawPos, Quaternion.identity, new Vector3(1f, 1f, 1f)); //drawer for beam
                Graphics.DrawMesh(MeshPool.plane10, matrix, trap_rearming, 0, null, 0, MatPropertyBlock);
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        protected override void Tick()
        {
            if (triggered)
            {
                HandleTriggered();
            }
            else if (rearming)
            {
                HandleRearming();
            }
            else
            {
                try
                {
                    ProcessArmedState();
                }
                catch
                {
                    Log.Message("Debug: poison trap failed to process armed event - terminating poison trap");
                    Destroy();
                }
            }

            foreach (ThingComp t in AllComps)
            {
                t.CompTick();
            }
        }

        private void HandleTriggered()
        {
            if (age >= lastStrike + strikeDelay)
            {
                try
                {
                    ProcessPoisonArea(Position, Radius);
                }
                catch
                {
                    Log.Message(
                        "Debug: poison trap failed to process triggered event - terminating poison trap");
                    Destroy();
                    return;
                }

                lastStrike = age;
            }

            age++;
            if (age > duration)
            {
                CheckForAgent();
                if (destroyAfterUse)
                {
                    Destroy();
                }
                else
                {
                    age = 0;
                    triggered = false;
                    rearming = true;
                    lastStrike = 0;
                }
            }
        }

        private void HandleRearming()
        {
            age++;
            if (age > TicksTillReArm)
            {
                age = 0;
                rearming = false;
                triggered = false;
            }
        }

        private void ProcessArmedState()
        {
            if (!Armed)
            {
                RemoveNonTouchingPawns();
                return;
            }

            foreach (var cell in GenRadial.RadialCellsAround(Position, 2, true))
            {
                ProcessCellForTouchingPawns(cell);
            }

            RemoveNonTouchingPawns();
        }

        private void ProcessCellForTouchingPawns(IntVec3 cell)
        {
            List<Thing> thingList = cell.GetThingList(Map);
            foreach (Thing thing in thingList)
            {
                Pawn pawn = thing as Pawn;
                if (pawn == null
                    || pawn.RaceProps.Animal
                    || pawn.Faction == null
                    || pawn.Faction == Faction
                    || !pawn.HostileTo(Faction)
                    || TouchingPawns.Contains(pawn))
                {
                    continue;
                }

                TouchingPawns.Add(pawn);
                CheckSpring(pawn);
            }
        }


        private void RemoveNonTouchingPawns()
        {
            for (int i = TouchingPawns.Count - 1; i >= 0; i--)
            {
                var pawn = TouchingPawns[i];
                if (!pawn.Spawned || pawn.Position != Position)
                {
                    TouchingPawns.RemoveAt(i);
                }
            }
        }

        private void ProcessPoisonArea(IntVec3 center, float radius)
        {
            var targets = GenRadial.RadialCellsAround(center, radius, true);
            foreach (var cell in targets)
            {
                if (!cell.InBounds(Map) || !cell.IsValid) continue;
                var victim = cell.GetFirstPawn(Map);
                if (victim != null && !victim.Dead && victim.RaceProps.IsFlesh)
                {
                    var bodyPart = victim.def.race.body.AllParts
                        .InRandomOrder()
                        .FirstOrDefault(part => part.def.tags.Contains(BodyPartTagDefOf.BreathingSource));
                    TM_Action.DamageEntities(
                        victim,
                        bodyPart,
                        Rand.Range(1f, 2f),
                        2f,
                        TMDamageDefOf.DamageDefOf.TM_Poison,
                        this
                    );
                }
            }
        }


        private void CheckForAgent()
        {
            destroyAfterUse = true;
            List<Pawn> pList = Map.mapPawns.AllPawnsSpawned.ToList();
            if (pList.Count <= 0) return;

            foreach (Pawn pawn in pList)
            {
                CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
                if (comp?.combatItems == null || comp.combatItems.Count <= 0) continue;

                if (comp.combatItems.Contains(this))
                {
                    destroyAfterUse = false;
                }
            }
        }

        protected override void CheckSpring(Pawn p)
        {
            if (Rand.Value < SpringChance(p))
            {
                Spring(p);
            }
        }

        protected override void Spring(Pawn p)
        {
            SoundDef.Named("DeadfallSpring").PlayOneShot(new TargetInfo(Position, Map));
            fog = TorannMagicDefOf.Fog_Poison;
            float expiration = duration / 60f;
            fog.gas.expireSeconds.min = expiration;
            fog.gas.expireSeconds.max = expiration;
            ExplosionHelper.Explode(Position, Map, Radius, TMDamageDefOf.DamageDefOf.TM_Poison, this, 0, 0,
                SoundDef.Named("TinyBell"), def, null, null, fog, 1f, 1, null, false, null, 0f, 0);
            triggered = true;
        }

        protected override float SpringChance(Pawn p)
        {
            float num = UnclampedSpringChance(p);
            if (p.RaceProps.Animal)
            {
                num *= 0.1f;
            }

            return Mathf.Clamp01(num);
        }

        public new bool KnowsOfTrap(Pawn p)
        {
            if (p.Faction != null && !p.Faction.HostileTo(Faction))
            {
                return true;
            }

            if (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState)
            {
                return true;
            }

            if (p.guest != null && p.guest.Released)
            {
                return true;
            }

            Lord lord = p.GetLord();
            return p.RaceProps.Humanlike && lord != null && lord.LordJob is LordJob_FormAndSendCaravan;
        }


        public override bool IsDangerousFor(Pawn p)
        {
            return Armed && KnowsOfTrap(p);
        }
    }
}