using Verse;
using AbilityUser;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace TorannMagic
{
    internal class Projectile_LightSkip : Projectile_AbilityBase
    {
        private int age = -1;
        private int duration = 45;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private int strikeDelay = 4;
        private int strikeNum = 1;
        private float radius = 5;
        private bool initialized;
        private float angle = 0;
        private List<IntVec3> cellList;
        private Pawn pawn;
        private IEnumerable<IntVec3> targets;
        private Skyfaller skyfaller2;
        private Skyfaller skyfaller;
        private Map map;
        private IntVec3 safePos = default(IntVec3);

        private bool launchedFlag;
        private bool pivotFlag;
        private bool landedFlag;
        private bool draftFlag;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref launchedFlag, "launchedFlag", false, false);
            Scribe_Values.Look<bool>(ref landedFlag, "landedFlag", false, false);
            Scribe_Values.Look<bool>(ref pivotFlag, "pivotFlag", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1800, false);
            Scribe_Values.Look<int>(ref strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<int>(ref strikeNum, "strikeNum", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<IntVec3>(ref safePos, "safePos", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            Scribe_Collections.Look<IntVec3>(ref cellList, "cellList", LookMode.Value);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                ModOptions.Constants.SetPawnInFlight(false);
                base.Destroy(mode);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {            
            ThingDef def = this.def;
            if (!initialized)
            {
                pawn = launcher as Pawn;
                map = pawn.Map;
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_LightSkip.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_LightSkip_pwr");
                
                pwrVal = pwr.level;
                arcaneDmg = comp.arcaneDmg;
                if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
                {
                    pwrVal = 1;
                    verVal = 1;
                }
                draftFlag = pawn.drafter != null ? pawn.Drafted : false;
                initialized = true;
            }

            if (!launchedFlag)
            {
                Pawn pawnToSkip = pawn;
                Pawn mount = null;
                ModOptions.Constants.SetPawnInFlight(true);
                if (ModCheck.Validate.GiddyUp.Core_IsInitialized())
                {
                    mount = ModCheck.GiddyUp.GetMount(pawn);
                    ModCheck.GiddyUp.ForceDismount(pawn);                    
                }
                if(pawnToSkip.carryTracker != null && pawnToSkip.carryTracker.CarriedThing != null)
                {
                    pawnToSkip.carryTracker.TryDropCarriedThing(pawnToSkip.Position, ThingPlaceMode.Near, out Thing _);
                }
                Thing pod = ThingMaker.MakeThing(TorannMagicDefOf.TM_LightPod, null);
                CompLaunchable podL = pod.TryGetComp<CompLaunchable>();
                CompTransporter podT = podL.parent.GetComp<CompTransporter>();
                GenPlace.TryPlaceThing(pod, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                podT.groupID = 11;
                pawnToSkip.DeSpawn();
                pawn.teleporting = true;
                if(mount != null)
                {
                    mount.DeSpawn();
                    podT.innerContainer.TryAddOrTransfer(mount);
                }
                podT.innerContainer.TryAddOrTransfer(pawnToSkip);                
                GlobalTargetInfo gti = new GlobalTargetInfo(Position, Map, false);
                LaunchLightPod(pod, podT, gti.Tile, gti.Cell);
                launchedFlag = true;
            }
            
            if (launchedFlag)
            {
                age++;
                Destroy(DestroyMode.Vanish);
            }
        }

        public void LaunchLightPod(Thing pod, CompTransporter compTransporter, int destinationTile, IntVec3 destinationCell)
        {
            Map map = pod.Map;
            int groupID = compTransporter.groupID;
            ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
            ActiveTransporter activeDropPod = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeDropPod.Contents = new ActiveTransporterInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);          
            WorldTransport.TM_DropPodLeaving obj = (WorldTransport.TM_DropPodLeaving)SkyfallerMaker.MakeSkyfaller(TorannMagicDefOf.TM_LightPodLeaving, activeDropPod);
            obj.groupID = groupID;
            obj.destinationTile = destinationTile;
            obj.arrivalAction = null;
            obj.arrivalCell = destinationCell;
            obj.draftFlag = draftFlag;
            compTransporter.CleanUpLoadingVars(map);
            compTransporter.parent.Destroy();
            GenSpawn.Spawn(obj, compTransporter.parent.Position, map);
        }

    }    
}