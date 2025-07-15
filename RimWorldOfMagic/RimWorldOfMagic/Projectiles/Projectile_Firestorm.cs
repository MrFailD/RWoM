using RimWorld;
using Verse;
using AbilityUser;
using TorannMagic.Weapon;

namespace TorannMagic
{
	public class Projectile_Firestorm : Projectile_AbilityBase
	{
        private int age;
        private int duration = 420;
        private int lastStrikeTiny;
        private int lastStrikeSmall;
        private int lastStrikeLarge;
        private int[] ticksTillHeavy = new int[200];
        private IntVec3[] shrapnelPos = new IntVec3[200];
        private int heavyCount;
        private bool initialized;
        private CellRect cellRect;
        private Pawn pawn;
        private int verVal;
        private int pwrVal;
        private MagicPowerSkill pwr;
        private MagicPowerSkill ver;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref duration, "duration", 420, false);
            Scribe_Values.Look<int>(ref lastStrikeTiny, "lastStrikeTiny", 0, false);
            Scribe_Values.Look<int>(ref lastStrikeSmall, "lastStrikeSmall", 0, false);
            Scribe_Values.Look<int>(ref lastStrikeLarge, "lastStrikeLarge", 0, false);
            Scribe_Values.Look<int>(ref heavyCount, "heavyCount", 0, false);

        }

        public void Initialize(Map map)
        {
            
            pawn = launcher as Pawn;
            CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
            pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Firestorm.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Firestorm_pwr");
            ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Firestorm.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Firestorm_ver");
            
            pwrVal = pwr.level;
            verVal = ver.level;
            if (ModOptions.Settings.Instance.AIHardMode && !pawn.IsColonist)
            {
                pwrVal = 1;
                verVal = 1;
            }
            duration = (int)((duration + (60 * verVal)) * comp.arcaneDmg);
            cellRect = CellRect.CenteredOn(Position, (int)(def.projectile.explosionRadius + .5*(pwrVal + verVal)));
            cellRect.ClipInsideMap(map);
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
            if (age > lastStrikeLarge + Rand.Range((200/(1+pwrVal))+20, (duration/(1+pwrVal))+40) && impactPos.InBoundsWithNullCheck(map) && impactPos.Standable(map))
            {
                lastStrikeLarge = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Firestorm_Large, impactPos, map);
                CellRect cellRectSec = CellRect.CenteredOn(impactPos, (int)(TorannMagicDefOf.TM_Firestorm_Large.skyfaller.explosionRadius + 2));
                for (int j = 0; j < (int)Rand.Range(1 + verVal, 5 + verVal); j++)
                {
                    shrapnelPos[heavyCount] = cellRectSec.RandomCell;
                    ticksTillHeavy[heavyCount] = TorannMagicDefOf.TM_Firestorm_Large.skyfaller.ticksToImpactRange.RandomInRange + 8;
                    heavyCount++;
                }                
            }
            impactPos = cellRect.RandomCell;
            if (age > lastStrikeTiny + Rand.Range(7-pwrVal, 20-pwrVal) && impactPos.InBoundsWithNullCheck(map) && impactPos.Standable(map))
            {
                lastStrikeTiny = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Firestorm_Tiny, impactPos, map);
            }
            impactPos = cellRect.RandomCell;
            if ( age > lastStrikeSmall + Rand.Range(18-(2*pwrVal), 42-(2*pwrVal)) && impactPos.InBoundsWithNullCheck(map) && impactPos.Standable(map))
            {
                lastStrikeSmall = age;
                SkyfallerMaker.SpawnSkyfaller(TorannMagicDefOf.TM_Firestorm_Small, impactPos, map);
            }

            for (int i = 0; i <= heavyCount; i++)
            {
                if (ticksTillHeavy[i] == 0)
                {
                    ExplosionHelper.Explode(shrapnelPos[heavyCount], map, .4f, TMDamageDefOf.DamageDefOf.TM_Firestorm_Small, launcher, Rand.Range(5, this.def.projectile.GetDamageAmount(1,null)), 0, SoundDefOf.BulletImpact_Ground, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0.2f, false);
                    ticksTillHeavy[i]--;
                }
                else
                {
                    ticksTillHeavy[i]--;
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

    }
}
