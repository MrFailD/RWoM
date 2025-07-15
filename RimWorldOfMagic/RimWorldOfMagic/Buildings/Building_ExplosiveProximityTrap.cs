using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TorannMagic.Buildings
{
    public class Building_ExplosiveProximityTrap : Building_Trap
    {
        protected List<Pawn> TouchingPawns = new List<Pawn>();

        private int trapSpringDelay = 30;
        private bool trapSprung;

        private Pawn trapPawn = new Pawn();

        protected bool Armed => !trapSprung;


        protected override void SpringSub(Pawn p)
        {
            foreach (ThingComp thingComp in AllComps)
            {
                if (!(thingComp is CompExplosive comp)) continue;
                comp.StartWick();
                return;
            }
        }

        private void CheckPawn(IntVec3 position)
        {
            List<Thing> thingList = position.GetThingList(Map);
            foreach (Thing thingComp in thingList)
            {
                Pawn pawn = thingComp as Pawn;
                if (pawn == null
                    || pawn.Faction == Faction
                    || !pawn.HostileTo(Faction)
                    || TouchingPawns.Contains(pawn)) continue;

                TouchingPawns.Add(pawn);
                CheckSpring(pawn);
            }
        }

        protected override void Tick()
        {
            if (Armed)
            {
                CheckPawn(Position);
                for (int j = 0; j < 8; j++)
                {
                    IntVec3 intVec = Position + GenAdj.AdjacentCells[j];
                    CheckPawn(intVec);
                }
                for (int j = 0; j < TouchingPawns.Count; j++)
                {
                    Pawn pawn2 = TouchingPawns[j];
                    if (!pawn2.Spawned || pawn2.Position != Position)
                    {
                        TouchingPawns.Remove(pawn2);
                    }
                }
            }
            else
            {
                trapSpringDelay--;
                if (trapSpringDelay <= 0)
                {
                    SpringSub(trapPawn);
                }
            }

            base.Tick();
        }

        protected virtual void CheckSpring(Pawn p)
        {
            if (!(Rand.Value < SpringChance(p))) return;

            Spring(p);
            if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
            {
                Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(
                    p.LabelShort
                ), "LetterFriendlyTrapSprung".Translate(
                    p.LabelShort
                ), LetterDefOf.NegativeEvent, new TargetInfo(Position, Map));
            }            
        }

        protected new virtual void Spring(Pawn p)
        {
            
            TorannMagicDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Position, Map, false));
            trapPawn = p;
            trapSprung = true;
        }

        protected virtual float UnclampedSpringChance(Pawn p)
        {
            float num = KnowsOfTrap(p) ? 0.8f : this.GetStatValue(StatDefOf.TrapSpringChance, true);
            num *= GenMath.LerpDouble(0.4f, 0.8f, 0f, 1f, p.BodySize);
            return num;
        }

        protected override float SpringChance(Pawn p)
        {            
            return Mathf.Clamp01(UnclampedSpringChance(p));
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            if (!Armed && KnowsOfTrap(p)) return 50;
            return 0;
        }

        public override bool IsDangerousFor(Pawn p)
        {
            return Armed && KnowsOfTrap(p) && p.Faction != Faction;
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            if (Armed)
            {
                text += "Trap Armed";
            }
            else
            {
                text += "Trap Not Armed";
            }
            return text;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            InstallBlueprintUtility.CancelBlueprintsFor(this);
            if (mode == DestroyMode.Deconstruct)
            {
                SoundDef.Named("Building_Deconstructed").PlayOneShot(new TargetInfo(Position, Map, false));
            }
        }
    }
}
