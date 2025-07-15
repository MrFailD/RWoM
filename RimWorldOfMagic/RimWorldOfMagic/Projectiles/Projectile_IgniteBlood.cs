using AbilityUser;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    public struct BloodFire : IExposable
    {
        public IntVec3 position;
        public int pulseCount;        

        public BloodFire(IntVec3 pos, int pulse)
        {
            position = pos;
            pulseCount = pulse;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref pulseCount, "pulseCount", 0, false);
            Scribe_Values.Look<IntVec3>(ref position, "position", default(IntVec3), false);
        }
    }

    public class Projectile_IgniteBlood : Projectile_AbilityBase
	{
        private int verVal;
        private int pwrVal;

        private int age = -1;
        private int spreadRate = 12;
        private float arcaneDmg = 1;

        private bool initialized;
        private int duration = 500;
        public List<ThingDef> bloodTypes = new List<ThingDef>();
        private ThingDef pawnBloodDef;

        private List<BloodFire> BF = new List<BloodFire>();

        private Vector3 direction = default(Vector3);
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 500, false);
            Scribe_Values.Look<int>(ref spreadRate, "spreadRate", 12, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<Vector3>(ref direction, "direction", default(Vector3), false);
            Scribe_Collections.Look<ThingDef>(ref bloodTypes, "bloodTypes", LookMode.Def);
            Scribe_Collections.Look<BloodFire>(ref BF, "BF", LookMode.Deep);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {            
            if (!initialized)
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
            if (!initialized)
            {
                base.Impact(hitThing);
                initialized = true;
                BF = new List<BloodFire>();
                BF.Clear();
                
                Pawn pawn = launcher as Pawn;
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill bpwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_pwr");
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_IgniteBlood.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_IgniteBlood_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_IgniteBlood.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_IgniteBlood_ver");
                pwrVal = pwr.level;
                verVal = ver.level;
                if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                arcaneDmg = comp.arcaneDmg;
                arcaneDmg *= (1 + (.1f * bpwr.level));
                spreadRate -= 2 * verVal;
                if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                bloodTypes = new List<ThingDef>();
                bloodTypes.Clear();
                if (ModOptions.Settings.Instance.unrestrictedBloodTypes)
                {
                    pawnBloodDef = pawn.RaceProps.BloodDef;
                    bloodTypes = TM_Calc.GetAllRaceBloodTypes();
                }
                else
                {
                    pawnBloodDef = ThingDefOf.Filth_Blood;
                    bloodTypes.Add(pawnBloodDef);
                }
                
                List<IntVec3> cellList = GenRadial.RadialCellsAround(Position, def.projectile.explosionRadius, true).ToList();

                Filth filth = (Filth)ThingMaker.MakeThing(pawnBloodDef);
                GenSpawn.Spawn(filth, Position, pawn.Map);
                //FilthMaker.MakeFilth(base.Position, this.Map, ThingDefOf.Filth_Blood, 1);
                for (int i = 0; i < 30; i++)
                {
                    IntVec3 randomCell = cellList.RandomElement();
                    if (randomCell.IsValid && randomCell.InBoundsWithNullCheck(pawn.Map) && !randomCell.Fogged(pawn.Map) && randomCell.Walkable(pawn.Map))
                    {
                        //FilthMaker.MakeFilth(randomCell, this.Map, ThingDefOf.Filth_Blood, 1);
                        //Log.Message("creating blood at " + randomCell);
                        Filth filth2 = (Filth)ThingMaker.MakeThing(pawnBloodDef);
                        GenSpawn.Spawn(filth2, randomCell, pawn.Map);
                    }
                }
                BF.Add(new BloodFire(Position, 0));
            }

            if(age > 0 && Find.TickManager.TicksGame % spreadRate == 0)
            {
                BurnBloodAtCell();
                FindNearbyBloodCells();
            }

            if(BF.Count <= 0)
            {
                age = duration;
            }

            if (age >= duration)
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        public void BurnBloodAtCell()
        {
            for (int i = 0; i < BF.Count; i++)
            {
                List<Thing> thingList = BF[i].position.GetThingList(Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (thingList[j] != null && bloodTypes.Contains(thingList[j].def))
                    {
                        thingList[j].Destroy(DestroyMode.Vanish);
                    }
                }
                BF[i] = new BloodFire(BF[i].position, BF[i].pulseCount + 1);
                ExplosionHelper.Explode(BF[i].position, Map, .2f + (.4f * BF[i].pulseCount), TMDamageDefOf.DamageDefOf.TM_BloodBurn, launcher, Mathf.RoundToInt((Rand.Range(2.8f, 4.5f) * (1 + (.12f * pwrVal))) * arcaneDmg), .5f, TorannMagicDefOf.TM_FireWooshSD, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0.0f, false);
                if(BF[i].pulseCount >= 3)
                {                    
                    BF.Remove(BF[i]);
                }                
            }
        }

        public void FindNearbyBloodCells()
        {
            int BFCount = BF.Count;
            for(int i =0; i < BFCount; i++)
            {
                List<IntVec3> cellList = GenRadial.RadialCellsAround(BF[i].position, .4f + BF[i].pulseCount, false).ToList();
                for (int j = 0; j < cellList.Count; j++)
                {
                    if (cellList[j].IsValid && cellList[j].InBoundsWithNullCheck(Map))
                    {
                        List<Thing> thingList = cellList[j].GetThingList(Map);
                        for (int k = 0; k < thingList.Count; k++)
                        {
                            if (thingList[k] != null && bloodTypes.Contains(thingList[k].def))
                            {
                                bool flag = false;
                                for (int z = 0; z < BF.Count; z++)
                                {
                                    if(BF[z].position == cellList[j])
                                    {
                                        flag = true; //already exists as an active blood flame position
                                    }
                                }
                                if (!flag)
                                {
                                    BF.Add(new BloodFire(cellList[j], 0));
                                }
                            }
                        }
                    }
                }
            }
        }
        

        public override void Tick()
        {
            if (!initialized)
            {
                if (age < 2)
                {
                    direction = TM_Calc.GetVector(launcher.DrawPos, Position.ToVector3Shifted());
                }
                Vector3 rndPos = DrawPos;
                rndPos.x += Rand.Range(-.4f, .4f);
                rndPos.z += Rand.Range(-.4f, .4f);
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodSquirt, rndPos, Map, Rand.Range(.9f, 1.2f), .05f, 0f, .25f, Rand.Range(-300, 300), Rand.Range(8f, 12f), (Quaternion.AngleAxis(Rand.Range(60,120), Vector3.up) * direction).ToAngleFlat(), Rand.Range(0, 360));
            }
            else
            {
                age++;
            }
            base.Tick();            
        }

    }	
}


