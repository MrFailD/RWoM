using Verse;
using RimWorld;
using UnityEngine;

namespace TorannMagic
{
    public class CompSummoned : ThingComp
    {
        public int ticksToDestroy = 3600;

        private Effecter effecter;

        private bool initialized;

        private bool temporary;
        public bool sustained;

        private int ticksLeft;

        private CompAbilityUserMagic compSummoner;
        private Pawn spawner;

        public CompProperties_Summoned Props
        {
            get
            {
                return (CompProperties_Summoned)props;
            }
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
        }

        private Thing Thing
        {
            get
            {
                Thing thing = parent as Thing;
                bool flag = thing == null;
                if (flag)
                {
                    Log.Error("pawn is null");
                }
                return thing;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Pawn pawn = parent as Pawn;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref temporary, "temporary", false, false);
            Scribe_Values.Look<bool>(ref sustained, "sustained", false, false);
            Scribe_Values.Look<int>(ref ticksLeft, "ticksLeft", 0, false);
            Scribe_Values.Look<int>(ref ticksToDestroy, "ticksToDestroy", 3600, false);
            Scribe_Values.Look<CompAbilityUserMagic>(ref compSummoner, "compSummoner", null, false);
            Scribe_References.Look<Pawn>(ref spawner, "spawner", false);
        }       

        public void SpawnSetup()
        {
            ticksLeft = ticksToDestroy;
        }

        public override void CompTick()
        {
            base.CompTick();
            if(!initialized)
            {
                SpawnSetup();
                initialized = true;
            }
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                bool flag2 = temporary;
                if (flag2)
                {
                    ticksLeft -= 10;
                    bool flag3 = ticksLeft <= 0;
                    if (flag3)
                    {
                        PreDestroy();
                        parent.Destroy(DestroyMode.Vanish);
                    }
                    bool spawned = parent.Spawned;
                    if (spawned)
                    {
                        bool flag4 = effecter == null;
                        if (flag4)
                        {
                            EffecterDef progressBar = EffecterDefOf.ProgressBar;
                            effecter = progressBar.Spawn();
                        }
                        else
                        {
                            LocalTargetInfo localTargetInfo = parent;
                            bool spawned2 = parent.Spawned;
                            if (spawned2)
                            {
                                effecter.EffectTick(parent, TargetInfo.Invalid);
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
                    }
                }
                if(sustained)
                {
                    if (spawner != null && !spawner.Destroyed)
                    {
                        if (spawner.Dead)
                        {
                            if (parent != null && !parent.Destroyed)
                            {
                                Messages.Message("TM_SunlightCollapse".Translate(
                                    spawner.LabelShort
                                ), MessageTypeDefOf.NeutralEvent);
                                parent.Destroy();                                
                            }
                        }
                    }
                    else
                    {
                        //spawner shouldn't be null if it's sustained, destroy object if that's the case
                        if (parent != null && !parent.Destroyed)
                        {
                            Log.Message("A sunlight has been destroyed, spawner found to be null.");
                            parent.Destroy();                            
                        }
                    }
                }
            }
        }

        public void PreDestroy()
        {
            try
            {
                //bool flag = this.effecter != null;
                //if (flag)
                //{
                //    this.effecter.Cleanup();
                //}     

                FleckMaker.ThrowSmoke(parent.Position.ToVector3(), parent.Map, 1);
                FleckMaker.ThrowHeatGlow(parent.Position, parent.Map, 1);
                
                if (parent.def.defName.Contains("TM_ManaMine"))
                {
                    Messages.Message("MineDeSpawn".Translate(), MessageTypeDefOf.SilentInput);
                }
                if (parent.def.defName.Contains("DefensePylon"))
                {
                    Messages.Message("PylonDeSpawn".Translate(), MessageTypeDefOf.SilentInput);
                }
            }
            catch
            {
                Log.Message("TM_ExceptionClose".Translate(
                            parent.def.defName
                ));
            }
        }

        
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = effecter != null && effecter.children != null;
            if (flag)
            {
                effecter.Cleanup();
            }
            base.PostDeSpawn(map, mode);
        }

    }
}
