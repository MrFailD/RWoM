using RimWorld;
using Verse;
using AbilityUser;
using System.Linq;
using UnityEngine;


namespace TorannMagic
{
	public class Projectile_Blizzard : Projectile_AbilityBase
	{
        private int age;
        private int duration = 720;
        private int lastStrikeTiny;
        private int lastStrikeSmall;
        private int lastStrikeLarge;
        private int snowCount;
        private int[] ticksTillSnow = new int[400];
        private IntVec3[] snowPos = new IntVec3[400];
        private bool initialized;
        private CellRect cellRect;
        private Pawn pawn;
        private MagicPowerSkill pwr;
        private MagicPowerSkill ver;
        private int verVal;
        private int pwrVal;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref duration, "duration", 720, false);
            Scribe_Values.Look<int>(ref lastStrikeTiny, "lastStrikeTiny", 0, false);
            Scribe_Values.Look<int>(ref lastStrikeSmall, "lastStrikeSmall", 0, false);
            Scribe_Values.Look<int>(ref lastStrikeLarge, "lastStrikeLarge", 0, false);
        }

        public void Initialize(Map map)
        {
            pawn = launcher as Pawn;
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Blizzard.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Blizzard_pwr");
            ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Blizzard.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Blizzard_ver");
            
            pwrVal = pwr.level;
            verVal = ver.level;
            if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            {
                pwrVal = 1;
                verVal = 1;
            }
            cellRect = CellRect.CenteredOn(Position, (int)(def.projectile.explosionRadius + (.5 *(verVal + pwrVal))));
            cellRect.ClipInsideMap(map);
            duration = Mathf.RoundToInt(duration + (90 * verVal) * comp.arcaneDmg);
            initialized = true;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            IntVec3 impactPos;
            if (!initialized)
            {
                Initialize(map);
            }
            impactPos = cellRect.RandomCell;
            if (age > lastStrikeLarge + Rand.Range(200 - (pwrVal * 30), duration/(4 + pwrVal)) && impactPos.InBoundsWithNullCheck(map) && impactPos.DistanceToEdge(map) >= 2 && impactPos.Standable(map))
            {
                lastStrikeLarge = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Blizzard_Large, impactPos, map);
                FleckMaker.ThrowSmoke(impactPos.ToVector3(), map, 5f);
                ticksTillSnow[snowCount] = TorannMagicDefOf.TM_Blizzard_Large.skyfaller.ticksToImpactRange.RandomInRange+4;
                snowPos[snowCount] = impactPos;
                snowCount++;
            }
            impactPos = cellRect.RandomCell;
            if (age > lastStrikeTiny + Rand.Range(6-(pwrVal), 18-(2*pwrVal)) && impactPos.InBoundsWithNullCheck(map) && impactPos.Standable(map))
            {
                lastStrikeTiny = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Blizzard_Tiny, impactPos, map);
                FleckMaker.ThrowSmoke(impactPos.ToVector3(), map, 1f);
                ticksTillSnow[snowCount] = TorannMagicDefOf.TM_Blizzard_Tiny.skyfaller.ticksToImpactRange.RandomInRange +2;
                snowPos[snowCount] = impactPos;
                snowCount++;
            }
            impactPos = cellRect.RandomCell;
            if ( age > lastStrikeSmall + Rand.Range(30-(2*pwrVal), 60-(4*pwrVal)) && impactPos.InBoundsWithNullCheck(map) && impactPos.Standable(map))
            {
                lastStrikeSmall = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Blizzard_Small, impactPos, map);
                FleckMaker.ThrowSmoke(impactPos.ToVector3(), map, 3f);
                ticksTillSnow[snowCount] = TorannMagicDefOf.TM_Blizzard_Small.skyfaller.ticksToImpactRange.RandomInRange+2;
                snowPos[snowCount] = impactPos;
                snowCount++;
            }

            for(int i = 0; i <= snowCount; i++)
            {
                if (ticksTillSnow[i] == 0)
                {
                    AddSnowRadial(snowPos[i], map, 2f, 2f);
                    FleckMaker.ThrowSmoke(snowPos[i].ToVector3(), map, 4f);                
                    ticksTillSnow[i]--;
                }
                else
                {
                    ticksTillSnow[i]--;
                }
            }

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

        public static void AddSnowRadial(IntVec3 center, Map map, float radius, float depth)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = center + GenRadial.RadialPattern[i];
                if (intVec.InBoundsWithNullCheck(map))
                {
                    float lengthHorizontal = (center - intVec).LengthHorizontal;
                    float num2 = 1f - lengthHorizontal / radius;
                    map.snowGrid.AddDepth(intVec, num2 * depth);

                }
            }
        }

    }
}
