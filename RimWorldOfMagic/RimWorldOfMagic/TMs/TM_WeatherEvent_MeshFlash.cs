using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using TorannMagic.Weapon;

namespace TorannMagic
{
    public class TM_WeatherEvent_MeshFlash : WeatherEvent
    {
        private static List<Mesh> boltMeshes = new List<Mesh>();
        private const int NumBoltMeshesMax = 20;

        private IntVec3 strikeLoc = IntVec3.Invalid;
        private Mesh boltMesh;

        private Material weatherMeshMat;
        //SkyColorSet weatherSkyColors;
        //private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);

        private Vector2 shadowVector;
        public int duration;
        private int age;
        private DamageDef damageType = TMDamageDefOf.DamageDefOf.TM_Shadow;
        private float averageRadius = 1.75f;
        private int damageAmount = -1;
        private float soundVolume = 1f;
        private float soundPitch = 1f;
        private Thing instigator;

        private const int FlashFadeInTicks = 3;
        private const int MinFlashDuration = 15;
        private const int MaxFlashDuration = 60;
        private const float FlashShadowDistance = 5f;

        private static readonly SkyColorSet MeshFlashColors = new SkyColorSet(new Color(1.2f, 0.8f, 1.2f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.8f, 0.8f), 1.15f);

        public override bool Expired
        {
            get
            {
                return age > duration;
            }
        }

        public override SkyTarget SkyTarget
        {
            get
            {
                return new SkyTarget(1f, MeshFlashColors, 1f, 1f);
            }
        }

        public override Vector2? OverrideShadowVector
        {
            get
            {
                return new Vector2?(shadowVector);
            }
        }

        public override float SkyTargetLerpFactor
        {
            get
            {
                return LightningBrightness;
            }
        }

        protected float LightningBrightness
        {
            get
            {
                float result;
                if (age <= 3)
                {
                    result = (float)age / 3f;
                }
                else
                {
                    result = 1f - (float)age / (float)duration;
                }
                return result;
            }
        }

        public TM_WeatherEvent_MeshFlash(Map map, IntVec3 forcedStrikeLoc, Material meshMat) : base(map)
		{
            weatherMeshMat = meshMat;
            strikeLoc = forcedStrikeLoc;
            duration = Rand.Range(15, 60);
            shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));            
        }

        public TM_WeatherEvent_MeshFlash(Map map, IntVec3 forcedStrikeLoc, Material meshMat, DamageDef dmgType, Thing instigator, int dmgAmt, float averageRad, float _soundVol = 1f, float _soundPitch = 1f) : base(map)
        {
            weatherMeshMat = meshMat;
            strikeLoc = forcedStrikeLoc;
            duration = Rand.Range(15, 60);
            shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
            damageType = dmgType;
            damageAmount = dmgAmt;
            averageRadius = averageRad;
            this.instigator = instigator;
            soundVolume = _soundVol;
            soundPitch = _soundPitch;
        }

        public override void FireEvent()
        {
            //SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(this.map);
            if (!strikeLoc.IsValid)
            {
                strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
            }
            boltMesh = RandomBoltMesh;
            if (!strikeLoc.Fogged(map))
            {
                SoundDef exp = TorannMagicDefOf.TM_FireBombSD;
                ExplosionHelper.Explode(strikeLoc, map, (Rand.Range(.8f, 1.2f) * averageRadius), damageType, instigator, damageAmount, -1f, exp, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                Vector3 loc = strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    FleckMaker.ThrowSmoke(loc, map, 1.3f);
                    FleckMaker.ThrowMicroSparks(loc, map);
                    FleckMaker.ThrowLightningGlow(loc, map, 1.2f);
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map, false), MaintenanceType.None);
            info.volumeFactor = soundVolume;
            info.pitchFactor = soundPitch;
            TorannMagicDefOf.TM_Thunder_OnMap.PlayOneShot(info);
        }

        public override void WeatherEventDraw()
        {
            Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(weatherMeshMat, LightningBrightness), 0);
        }

        public override void WeatherEventTick()
        {
            age++;
        }

        public static Mesh RandomBoltMesh
        {
            get
            {
                Mesh result;
                if (boltMeshes.Count < 20)
                {
                    Mesh mesh = TM_MeshMaker.NewBoltMesh(200f, 20);
                    boltMeshes.Add(mesh);
                    result = mesh;
                }
                else
                {
                    result = boltMeshes.RandomElement<Mesh>();
                }
                return result;
            }
        }
    }
}
