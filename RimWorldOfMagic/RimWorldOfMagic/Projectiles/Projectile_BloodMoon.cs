using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_BloodMoon : Projectile_AbilityBase
    {
        private bool initialized;
        private bool validTarget = false;
        private int verVal;
        private int pwrVal;
        private int effVal;
        private float arcaneDmg = 1;
        private int duration = 1200;
        private int age = -1;
        private int bloodFrequency = 8;
        private float attackFrequency = 30;
        private List<IntVec3> bloodCircleCells = new List<IntVec3>();
        private List<IntVec3> bloodCircleOuterCells = new List<IntVec3>();

        private Pawn caster;
        private List<Pawn> victims = new List<Pawn>();
        private List<int> victimHitTick = new List<int>();
        private List<float> wolfDmg = new List<float>();

        private int delayTicks = 25;
        private int nextAttack;

        private ColorInt colorInt = new ColorInt(45, 0, 4, 250);
        private Sustainer sustainer;

        private float angle;
        private float radius = 5;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref duration, "duration", 1200, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1f, false);
            Scribe_Values.Look<float>(ref radius, "radius", 6f, false);
            Scribe_Values.Look<float>(ref attackFrequency, "attackFrequency", 30f, false);
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
            Scribe_Collections.Look<Pawn>(ref victims, "victims", LookMode.Reference);
            Scribe_Collections.Look<int>(ref victimHitTick, "victimHitTick", LookMode.Value);
            Scribe_Collections.Look<float>(ref wolfDmg, "wolfDmg", LookMode.Value);
            Scribe_Collections.Look<IntVec3>(ref bloodCircleOuterCells, "bloodCircleOuterCells", LookMode.Value);
            Scribe_Collections.Look<IntVec3>(ref bloodCircleCells, "bloodCircleCells", LookMode.Value);
        }

        private int TicksLeft
        {
            get
            {
                return duration - age;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;

            if (!initialized)
            {

                bloodCircleOuterCells = new List<IntVec3>();
                bloodCircleOuterCells.Clear();
                victimHitTick = new List<int>();
                victimHitTick.Clear();
                victims = new List<Pawn>();
                victims.Clear();
                wolfDmg = new List<float>();
                wolfDmg.Clear();

                caster = launcher as Pawn;
                CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                MagicPowerSkill bpwr = comp.MagicData.MagicPowerSkill_BloodGift.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodGift_pwr");
                pwrVal = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_BloodMoon.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodMoon_pwr").level;
                verVal = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_BloodMoon.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodMoon_ver").level;
                effVal = caster.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_BloodMoon.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodMoon_eff").level;
                arcaneDmg = comp.arcaneDmg;
                arcaneDmg *= (1f + (.1f * bpwr.level));
                attackFrequency *= (1 - (.05f * effVal));
                duration = Mathf.RoundToInt(duration + (duration * .1f * verVal));

                angle = Rand.Range(-2f, 2f);
                radius = this.def.projectile.explosionRadius;
                
                IntVec3 curCell = Position;

                CheckSpawnSustainer();

                if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                {
                    List<IntVec3> cellList = GenRadial.RadialCellsAround(Position, radius, true).ToList();
                    for (int i = 0; i < cellList.Count; i++)
                    {
                        curCell = cellList[i];
                        if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                        {
                            bloodCircleCells.Add(curCell);
                        }
                    }
                    cellList.Clear();
                    cellList = GenRadial.RadialCellsAround(Position, radius+1, true).ToList();
                    List<IntVec3> outerRing = new List<IntVec3>();
                    for (int i = 0; i < cellList.Count; i++)
                    {
                        curCell = cellList[i];
                        if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                        {
                            outerRing.Add(curCell);
                        }
                    }
                    bloodCircleOuterCells = outerRing.Except(bloodCircleCells).ToList();
                }

                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodCircle, Position.ToVector3Shifted(), caster.Map, radius + 2, (duration/60) *.9f, (duration / 60) * .06f, (duration / 60) * .08f, Rand.Range(-50, -50), 0, 0, Rand.Range(0, 360));
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodCircle, Position.ToVector3Shifted(), caster.Map, radius + 2, (duration / 60) * .9f, (duration / 60) * .06f, (duration / 60) * .08f, Rand.Range(50, 50), 0, 0, Rand.Range(0, 360));
                TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodCircle, Position.ToVector3Shifted(), caster.Map, radius + 2, (duration / 60) * .9f, (duration / 60) * .06f, (duration / 60) * .08f, Rand.Range(-50,50), 0, 0, Rand.Range(0, 360));
                caster.Map.weatherManager.eventHandler.AddEvent(new TM_WeatherEvent_BloodMoon(caster.Map, duration, 2f - (pwrVal * .1f)));
                initialized = true;
            }            

            if (initialized && Map != null && age > 15)
            {
                if (victims.Count > 0)
                {
                    for (int i = 0; i < victims.Count; i++)
                    {
                        if (victimHitTick[i] < age)
                        {
                            TM_Action.DamageEntities(victims[i], null, Mathf.RoundToInt((Rand.Range(5, 8) * wolfDmg[i])*arcaneDmg), DamageDefOf.Bite, launcher);
                            TM_MoteMaker.ThrowBloodSquirt(victims[i].DrawPos, victims[i].Map, Rand.Range(.6f, 1f));
                            victims.Remove(victims[i]);
                            victimHitTick.Remove(victimHitTick[i]);
                            wolfDmg.Remove(wolfDmg[i]);
                        }
                    }
                }

                if (Find.TickManager.TicksGame % bloodFrequency == 0)
                {
                    Filth filth = (Filth)ThingMaker.MakeThing(ThingDefOf.Filth_Blood);
                    GenSpawn.Spawn(filth, bloodCircleOuterCells.RandomElement(), Map);
                }

                if(nextAttack < age && !caster.DestroyedOrNull() && !caster.Dead)
                {

                    Pawn victim = TM_Calc.FindNearbyEnemy(Position, Map, caster.Faction, radius, 0);
                    if (victim != null)
                    {
                        IntVec3 rndPos = victim.Position;
                        while (rndPos == victim.Position)
                        {
                            rndPos = bloodCircleCells.RandomElement();
                        }
                        Vector3 wolf = rndPos.ToVector3Shifted();
                        Vector3 direction = TM_Calc.GetVector(wolf, victim.DrawPos);
                        float angle = direction.ToAngleFlat();
                        float fadeIn = .1f;
                        float fadeOut = .25f;
                        float solidTime = .10f;
                        float drawSize = Rand.Range(.7f, 1.2f)+(pwrVal *.1f);
                        float velocity = (victim.DrawPos - wolf).MagnitudeHorizontal();
                        if (angle >= -135 && angle < -45) //north
                        {
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodWolfNorth, wolf, Map, drawSize, solidTime, fadeIn, fadeOut, 0, 2*velocity, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 0);
                        }
                        else if (angle >= 45 && angle < 135) //south
                        {
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodWolfSouth, wolf, Map, drawSize, solidTime, fadeIn, fadeOut, 0, 2 * velocity, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 0);
                        }
                        else if (angle >= -45 && angle < 45) //east
                        {
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodWolfEast, wolf, Map, drawSize, solidTime, fadeIn, fadeOut, 0, 2 * velocity, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 0);
                        }
                        else //west
                        {
                            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodWolfWest, wolf, Map, drawSize, solidTime, fadeIn, fadeOut, 0, 2 * velocity, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 0);
                        }
                        int hitDelay = age + delayTicks;
                        Effecter BloodShieldEffect = TorannMagicDefOf.TM_BloodShieldEffecter.Spawn();
                        BloodShieldEffect.Trigger(new TargetInfo(wolf.ToIntVec3(), Map, false), new TargetInfo(wolf.ToIntVec3(), Map, false));
                        BloodShieldEffect.Cleanup();
                        victims.Add(victim);
                        victimHitTick.Add(hitDelay);
                        wolfDmg.Add(drawSize);
                        if (Rand.Chance(.1f))
                        {
                            if (Rand.Chance(.65f))
                            {
                                SoundInfo info = SoundInfo.InMap(new TargetInfo(wolf.ToIntVec3(), Map, false), MaintenanceType.None);
                                SoundDef.Named("TM_DemonCallHigh").PlayOneShot(info);
                            }
                            else
                            {
                                SoundInfo info = SoundInfo.InMap(new TargetInfo(wolf.ToIntVec3(), Map, false), MaintenanceType.None);
                                info.pitchFactor = .8f;
                                info.volumeFactor = .8f;
                                SoundDef.Named("TM_DemonPain").PlayOneShot(info);
                            }
                        }
                    }
                    nextAttack = age + Mathf.RoundToInt(Rand.Range(.4f * (float)attackFrequency, .8f * (float)attackFrequency));
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float beamSize = 8f;
            Vector3 drawPos = Position.ToVector3Shifted(); // this.parent.DrawPos;
            drawPos.z = drawPos.z - ((.5f * beamSize)*radius);
            float num = ((float)Map.Size.z - drawPos.z) * 1.4f;
            Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);  //angle of beam
            Vector3 a2 = drawPos + a * num * 0.5f;                      //
            a2.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays); //mote depth
            float num2 = Mathf.Min((float)age / 10f, 1f);          //
            Vector3 b = a * ((1f - num2) * num);
            float num3 = 0.975f + (.15f) * 0.025f;       //color
            if (age < (duration * .1f))                          //color
            {
                num3 *= (float)(age) / (duration * .1f);
            }
            if(age > (.9f * duration))
            {
                num3 *= (float)(duration - age) / (duration * .1f);
            }
            Color arg_50_0 = colorInt.ToColor;
            Color color = arg_50_0;
            color.a *= num3;
            MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(a2 + a * (radius*beamSize) * 0.5f + b, Quaternion.Euler(0f, angle, 0f), new Vector3(radius*beamSize, 1f, num));   //drawer for beam
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
            Vector3 vectorPos = drawPos;
            //vectorPos.z -= (this.radius * (.5f * beamSize));
            vectorPos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(vectorPos, Quaternion.Euler(0f, angle, 0f), new Vector3(radius * beamSize, 1f, radius * beamSize));                 //drawer for beam end
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
        }

        public override void Tick()
        {
            base.Tick();
            age++;            
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < duration;
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


