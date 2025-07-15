using RimWorld;
using AbilityUser;
using Verse;
using System.Linq;
using TorannMagic.Buildings;

namespace TorannMagic
{
    public class Projectile_Cooler : Projectile_AbilityBase
    {

        private bool primed;
        private CompAbilityUserMagic comp;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            Pawn victim = hitThing as Pawn;
            Thing item = hitThing as Thing;
            IntVec3 arg_pos_1;

            Pawn pawn = launcher as Pawn;
            comp = pawn.GetCompAbilityUserMagic();

            CellRect cellRect = CellRect.CenteredOn(Position, 1);
            cellRect.ClipInsideMap(map);
            IntVec3 centerCell = cellRect.CenterCell;

            if (!primed)
            {
                arg_pos_1 = centerCell;
                if ((arg_pos_1.IsValid && arg_pos_1.Standable(map)))
                {
                    SpawnThings tempPod = new SpawnThings();
                    IntVec3 shiftPos = centerCell;
                    centerCell.x++;
                    tempPod.def = ThingDef.Named("TM_Cooler");                    
                    tempPod.spawnCount = 1;
                    try
                    {
                        SingleSpawnLoop(tempPod, shiftPos, map);
                    }
                    catch
                    {
                        Log.Message("Attempted to create a cooler but threw an unknown exception - recovering and ending attempt");
                        return;
                    }

                    primed = true;
                }
                else
                {
                    Messages.Message("InvalidSummon".Translate(), MessageTypeDefOf.RejectInput);
                    comp.Mana.GainNeed(comp.ActualManaCost(TorannMagicDefOf.TM_Cooler));
                }
            }
        }

        public void SingleSpawnLoop(SpawnThings spawnables, IntVec3 position, Map map)
        {
            bool flag = spawnables.def != null;
            if (flag)
            {
                Faction faction = TM_Action.ResolveFaction(launcher as Pawn, spawnables, launcher.Faction);
                bool flag2 = spawnables.def.race != null;
                if (flag2)
                {
                    bool flag3 = spawnables.kindDef == null;
                    if (flag3)
                    {
                        Log.Error("Missing kinddef");
                    }
                    else
                    {
                        TM_Action.SpawnPawn(launcher as Pawn, spawnables, faction, position, 0, map);                       
                    }
                }
                else
                {
                    ThingDef def = spawnables.def;
                    ThingDef stuff = null;
                    bool madeFromStuff = def.MadeFromStuff;
                    if (madeFromStuff)
                    {
                        stuff = ThingDefOf.WoodLog;
                    }
                    Thing thing = ThingMaker.MakeThing(def, stuff);
                    if (thing.def.defName != "Portfuel")
                    {
                        thing.SetFaction(faction, null);
                    }
                    CompSummoned bldgComp = thing.TryGetComp<CompSummoned>();
                    bldgComp.Temporary = false;
                    bldgComp.Spawner = launcher as Pawn;
                    bldgComp.sustained = true;
                    GenSpawn.Spawn(thing, position, map, Rot4.North, WipeMode.Vanish, false);
                    comp.summonedCoolers.Add(thing);
                    Building_TMCooler cooler = thing as Building_TMCooler;
                    if (cooler != null)
                    {
                        if (comp.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_pwr").level >= 12)
                        {
                            cooler.Defensive = true;
                        }
                        if (comp.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_ver").level >= 6)
                        {
                            cooler.BuffCool = true;
                        }
                        if (comp.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_ver").level >= 9)
                        {
                            cooler.BuffFresh = true;
                        }
                    }
                }
            }
        }
    }
}
