using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_OrbitalStrike : Projectile_AbilityBase
    {
        private bool initialized;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1f;
        private int beamAge;
        private int strikeNum;
        private int age = -1;
        private int beamDuration = 120;
        private int targettingAge = 300;
        private IntVec3 strikePos = default(IntVec3);
        private Pawn caster;

        private ColorInt colorInt = new ColorInt(153, 0, 0);
        private Sustainer sustainer;

        private float angle;
        private float radius = 5;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BombardMat = MaterialPool.MatFrom("Other/Bombardment", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref beamAge, "beamAge", 120, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1f, false);
            Scribe_Values.Look<int>(ref targettingAge, "targettingAge", 300, false);
            Scribe_Values.Look<IntVec3>(ref strikePos, "strikePos", default(IntVec3), false);
        }

        private int TicksLeft
        {
            get
            {
                return (beamDuration + targettingAge) - age;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (!initialized)
            {
                caster = launcher as Pawn;
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_OrbitalStrike.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_OrbitalStrike_pwr");
                MagicPowerSkill ver = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_OrbitalStrike.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_OrbitalStrike_ver");
                verVal = ver.level;
                pwrVal = pwr.level;
                arcaneDmg = comp.arcaneDmg;
                
                if (ModOptions.Settings.Instance.AIHardMode && !caster.IsColonist)
                {
                    pwrVal = 3;
                    verVal = 3;
                }
                angle = Rand.Range(-3f, 3f);
                strikeNum = 1;
                CheckSpawnSustainer();
                strikePos = Position;
                targettingAge = 300 - (50 * verVal);
                beamDuration = 120 - (10 * verVal);
                radius = this.def.projectile.explosionRadius;
                initialized = true;
            }

            if (sustainer != null)
            {
                sustainer.info.volumeFactor = (age) / (beamDuration + targettingAge);
                sustainer.Maintain();
                if (TicksLeft <= 0)
                {
                    sustainer.End();
                    sustainer = null;
                }
            }

            if (age == (targettingAge + beamDuration))
            {
                TM_MoteMaker.MakePowerBeamMoteColor(strikePos, Map, radius * 4f, 2f, .5f, .1f, .5f, colorInt.ToColor);                
                ExplosionHelper.Explode(strikePos, map, this.def.projectile.explosionRadius, DamageDefOf.Bomb, launcher as Pawn, Mathf.RoundToInt((25 + 5*pwrVal) * arcaneDmg), 0, null, def, equipmentDef, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                Effecter OSEffect = TorannMagicDefOf.TM_OSExplosion.Spawn();
                OSEffect.Trigger(new TargetInfo(strikePos, Map, false), new TargetInfo(strikePos, Map, false));
                OSEffect.Cleanup();
            }
            else
            {
                if (Find.TickManager.TicksGame % 3 == 0)
                {
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodSquirt, strikePos.ToVector3Shifted(), Map, .3f, .1f, 0, 0, Rand.Range(-100, 100), 0, 0, Rand.Range(0, 360));
                }
            }
            
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (age >= targettingAge)
            {
                DrawSmiteBeams(strikePos, beamAge);
            }            
        }

        public void DrawSmiteBeams(IntVec3 pos, int wrathAge)
        {
            Vector3 drawPos = pos.ToVector3Shifted(); // this.parent.DrawPos;
            drawPos.z = drawPos.z - 1.5f;
            float num = ((float)Map.Size.z - drawPos.z) * 1.4f;
            Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);  //angle of beam
            Vector3 a2 = drawPos + a * num * 0.5f;                      //
            a2.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays); //mote depth
            float num2 = Mathf.Min((float)beamAge / 10f, 1f);          //
            Vector3 b = a * ((1f - num2) * num);
            float num3 = 0.975f + Mathf.Sin((float)wrathAge * 0.3f) * 0.025f;       //color
            if (TicksLeft > (beamDuration * .2f))                          //color
            {
                num3 *= (float)(age - targettingAge) / (beamDuration * .8f);
            }
            Color arg_50_0 = colorInt.ToColor;
            Color color = arg_50_0;
            color.a *= num3;
            MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(a2 + a * radius * 0.5f + b, Quaternion.Euler(0f, angle, 0f), new Vector3(radius, 1f, num));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
            Vector3 vectorPos = drawPos + b;
            vectorPos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(vectorPos, Quaternion.Euler(0f, angle, 0f), new Vector3(radius, 1f, radius));                 //drawer for beam end
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
            num = num - (num * ((float)wrathAge/(float)(beamDuration/strikeNum)));
            Vector3 a3 = a * num;
            Vector3 vectorOrb = drawPos + a3;
            vectorOrb.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix3 = default(Matrix4x4);
            matrix3.SetTRS(vectorOrb, Quaternion.Euler(0f, angle, 0f), new Vector3(radius*((5*(float)wrathAge)/((float)beamDuration/strikeNum)), 1f, radius * ((5 * (float)wrathAge) / ((float)beamDuration/strikeNum))));                 //drawer for beam end
            Graphics.DrawMesh(MeshPool.plane10, matrix3, BombardMat, 0, null, 0, MatPropertyBlock);
        }

        public override void Tick()
        {
            base.Tick();
            age++;
            if(age >= targettingAge)
            {
                beamAge++;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age <= (targettingAge + beamDuration);
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        private void CheckSpawnSustainer()
        {
            if (TicksLeft >= 0)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    sustainer = SoundDef.Named("OrbitalBeam").TrySpawnSustainer(SoundInfo.InMap(selectedTarget, MaintenanceType.PerTick));
                });
            }
        }
    }
}


