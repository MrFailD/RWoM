using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    public class TMPawnSummoned : Pawn
    {
        private Effecter effecter;
        private bool initialized;
        private bool temporary;
        private int ticksLeft;
        private int ticksToDestroy = 1800;
        public bool validSummoning;

        private CompAbilityUserMagic compSummoner;
        private Pawn spawner;
        private Pawn original;

        private List<float> bodypartDamage = new List<float>();
        private List<DamageDef> bodypartDamageType = new List<DamageDef>();

        private List<Hediff_Injury> injuries = new List<Hediff_Injury>();

        public Pawn Original
        {
            get => original;
            set => original = value;
        }

        public Pawn Spawner
        {
            get => spawner;
            set => spawner = value;
        }

        public CompAbilityUserMagic CompSummoner
        {
            get
            {
                return spawner.GetCompAbilityUserMagic();
            }
        }

        public bool Temporary
        {
            get
            {
                return temporary;
            }
            set
            {
                temporary = value;
            }
        }

        public int TicksToDestroy
        {
            get
            {
                return ticksToDestroy;
            }
            set
            {
                ticksToDestroy = value;
            }
        }

        public int TicksLeft
        {
            get
            {
                return ticksLeft;
            }
            set
            {
                ticksLeft = value;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            ticksLeft = ticksToDestroy;
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public virtual void PostSummonSetup()
        {
            if (!validSummoning)
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        public void CheckPawnState()
        {
            if (def.race.body.ToString() == "Minion")
            {
                try
                {
                    if (Downed && !Destroyed && this != null && Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MinionFled".Translate(), MessageTypeDefOf.NeutralEvent);
                        FleckMaker.ThrowSmoke(Position.ToVector3(), Map, 1);
                        FleckMaker.ThrowHeatGlow(Position, Map, 1);
                        if (CompSummoner != null)
                        {
                            CompSummoner.summonedMinions.Remove(this);
                        }
                        Destroy(DestroyMode.Vanish);
                    }
                }
                catch
                {
                    Log.Message("TM_ExceptionTick".Translate(
                        def.defName
                    ));
                    Destroy(DestroyMode.Vanish);
                }
            }
        }

        protected override void Tick()
        {
            if (Spawned && Map != null && Position.InBounds(Map))
            {
                base.Tick();
                if (Find.TickManager.TicksGame % 10 == 0)
                {
                    if (!initialized)
                    {
                        initialized = true;
                        PostSummonSetup();
                    }
                    bool flag2 = temporary;
                    if (flag2)
                    {
                        ticksLeft -= 10;
                        bool flag3 = ticksLeft <= 0;
                        if (flag3)
                        {
                            PreDestroy();
                            if (!Destroyed)
                            {
                                Destroy(DestroyMode.Vanish);
                            }
                        }
                        CheckPawnState();

                        bool flag4 = effecter == null;
                        if (flag4)
                        {
                            EffecterDef progressBar = EffecterDefOf.ProgressBar;
                            effecter = progressBar.Spawn();
                        }
                        else
                        {
                            LocalTargetInfo localTargetInfo = this;
                            bool spawned2 = Spawned;
                            if (spawned2)
                            {
                                effecter.EffectTick(this, TargetInfo.Invalid);
                            }
                            MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                            bool flag5 = mote != null;
                            if (flag5)
                            {
                                float value = 1f - (float)(TicksToDestroy - ticksLeft) / (float)TicksToDestroy;
                                mote.progress = Mathf.Clamp01(value);
                                mote.offsetZ = -0.5f;
                            }
                        }
                        if (Find.TickManager.TicksGame % 120 == 0)
                        {
                            CheckAndTrain();
                        }
                    }
                }
            }
        }

        public void PreDestroy()
        {
            Thing resultThing = null;
            if (carryTracker != null && carryTracker.CarriedThing != null)
            {
                carryTracker.TryDropCarriedThing(Position, ThingPlaceMode.Near, out resultThing);
            }
            if (def.defName == "TM_MinionR" || def.defName == "TM_GreaterMinionR")
            {
                try
                {
                    if (Map != null)
                    {
                        FleckMaker.ThrowSmoke(Position.ToVector3(), Map, 3); 
                    }
                    else
                    {
                        holdingOwner.Remove(this);
                    }
                    if (CompSummoner != null)
                    {
                        CompSummoner.summonedMinions.Remove(this);
                    }                    
                }
                catch
                {
                    Log.Message("TM_ExceptionClose".Translate(
                            def.defName
                    ));
                }
            }
            if(def == TorannMagicDefOf.TM_SpiritWolfR || def == TorannMagicDefOf.TM_SpiritBearR || def == TorannMagicDefOf.TM_SpiritCrowR || def == TorannMagicDefOf.TM_SpiritMongooseR)
            {
                try
                {
                    if(Map != null)
                    {
                        FleckMaker.ThrowSmoke(DrawPos, Map, Rand.Range(1f, 3f));
                        FleckMaker.ThrowSmoke(DrawPos, Map, Rand.Range(1f, 2f));
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Ghost, DrawPos, Map, 1f, .25f, 0f, .25f, 0, Rand.Range(1f, 2f), 0, 0);
                    }
                    else if(holdingOwner != null && holdingOwner.Contains(this))
                    {
                        holdingOwner.Remove(this);
                    }                
                }
                catch
                {
                    Log.Message("TM_ExceptionClose".Translate(
                            def.defName
                    ));
                }
            }
            if(original != null)
            {
                //Log.Message("pre destroy");
                CopyDamage(this);
                SpawnOriginal();
                ApplyDamage(original);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {            
            bool flag = effecter != null;
            if (flag)
            {
                effecter.Cleanup();
            }
            base.DeSpawn(mode);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref temporary, "temporary", false, false);
            Scribe_Values.Look<bool>(ref validSummoning, "validSummoning", true, false);
            Scribe_Values.Look<int>(ref ticksLeft, "ticksLeft", 0, false);
            Scribe_Values.Look<int>(ref ticksToDestroy, "ticksToDestroy", 1800, false);
            Scribe_Values.Look<CompAbilityUserMagic>(ref compSummoner, "compSummoner", null, false);
            Scribe_References.Look<Pawn>(ref spawner, "spawner", false);
            Scribe_References.Look<Pawn>(ref original, "original", false);
        }

        public TMPawnSummoned()
        {

        }

        public void CopyDamage(Pawn pawn)
        {
            IEnumerable<Hediff_Injury> injuriesToCopy = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury => injury.CanHealNaturally());
            injuries.AddRange(injuriesToCopy);
        }

        public void SpawnOriginal()
        {
            GenSpawn.Spawn(original, Position, Map, WipeMode.Vanish);
        }

        public void ApplyDamage(Pawn pawn)
        {
            List<BodyPartRecord> bodyparts = pawn.health.hediffSet.GetNotMissingParts().ToList();
            for(int i =0; i < injuries.Count; i++)
            {
                pawn.health.AddHediff(injuries[i], bodyparts.RandomElement());
            }
        }

        public void CheckAndTrain()
        {
            if (training != null && training.CanBeTrained(TrainableDefOf.Tameness))
            {
                while (!training.HasLearned(TrainableDefOf.Tameness))
                {
                    training.Train(TrainableDefOf.Tameness, null);
                }
            }
        }
    }
}
