using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using TorannMagic.Weapon;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Projectile_Resurrection : Projectile_AbilityBase
    {
        private bool initialized;
        private bool validTarget;
        private int verVal;
        private int pwrVal;
        private int timeToRaise = 1200;
        private int age = -1;
        private IntVec3 deadPawnPosition = default(IntVec3);
        private Thing corpseThing;
        private Pawn deadPawn;
        private Pawn caster;

        private ColorInt colorInt = new ColorInt(255, 255, 140);
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
            Scribe_Values.Look<bool>(ref validTarget, "validTarget", false, false);
            Scribe_Values.Look<int>(ref age, "age", -1, false);
            Scribe_Values.Look<int>(ref timeToRaise, "timeToRaise", 1800, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<IntVec3>(ref deadPawnPosition, "deadPawnPosition", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref deadPawn, "deadPawn", false);
            Scribe_References.Look<Thing>(ref corpseThing, "corpseThing", false);
        }

        private int TicksLeft
        {
            get
            {
                return timeToRaise - age;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;

            if (!initialized)
            {
                if (launcher is Pawn)
                {
                    caster = launcher as Pawn;
                    CompAbilityUserMagic comp = caster.GetCompAbilityUserMagic();
                    if (comp != null && comp.MagicData != null)
                    {
                        MagicPowerSkill ver = comp.MagicData.MagicPowerSkill_Resurrection.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Resurrection_ver");
                        MagicPowerSkill pwr = comp.MagicData.MagicPowerSkill_Resurrection.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Resurrection_eff");
                        verVal = ver.level;
                        pwrVal = pwr.level;
                    }
                }
                angle = Rand.Range(-12f, 12f);               
                
                IntVec3 curCell = Position;

                CheckSpawnSustainer();

                if (curCell.InBoundsWithNullCheck(map))
                {
                    Corpse corpse = null;
                    List<Thing> thingList = new List<Thing>();
                    thingList = curCell.GetThingList(map);
                    int z = 0;
                    while (z < thingList.Count)
                    {
                        corpseThing = thingList[z];
                        if (corpseThing != null)
                        {
                            bool validator = corpseThing is Corpse;
                            if (corpseThing is Corpse)
                            {
                                corpse = corpseThing as Corpse;
                                CompRottable compRot = corpse.GetComp<CompRottable>();
                                deadPawn = corpse.InnerPawn;
                                deadPawnPosition = corpse.Position;
                                if (deadPawn != null && deadPawn.RaceProps.IsFlesh && !TM_Calc.IsUndead(deadPawn) && compRot != null)
                                {
                                    if (!corpse.IsNotFresh())
                                    {                                        
                                        validTarget = true;
                                        corpse.SetForbidden(true);
                                        break;
                                    }
                                    else
                                    {
                                        Messages.Message("TM_ResurrectionTargetExpired".Translate(), MessageTypeDefOf.RejectInput);
                                        break;
                                    }
                                }
                                if(TM_Calc.IsUndead(deadPawn))
                                {
                                    z = thingList.Count;
                                    validTarget = true;
                                }
                            }
                        }
                        z++;
                    }
                }
                initialized = true;
            }

            if (validTarget && corpseThing != null && (corpseThing.Position != deadPawnPosition || corpseThing.Map == null) && deadPawn.Dead)
            {
                Log.Message("Corpse was moved or destroyed during resurrection process.");
                age = timeToRaise;
            }

            if (validTarget)
            {
                if (sustainer != null)
                {
                    sustainer.info.volumeFactor = age / timeToRaise;
                    sustainer.Maintain();
                    if (TicksLeft <= 0)
                    {
                        sustainer.End();
                        sustainer = null;
                    }
                }
                if (age+1 == timeToRaise)
                {
                    TM_MoteMaker.MakePowerBeamMoteColor(Position, Map, radius * 3f, 2f, 2f, .1f, 1.5f, colorInt.ToColor);
                    if (deadPawn == null)
                    {
                        if (corpseThing != null)
                        {
                            Corpse corpse = corpseThing as Corpse;
                            if (corpse != null)
                            {
                                deadPawn = corpse.InnerPawn;
                            }
                        }
                    }
                    if (deadPawn != null && deadPawn.RaceProps != null && deadPawn.kindDef != null)
                    {
                        if (TM_Calc.IsUndead(deadPawn))
                        {
                            if(deadPawn.RaceProps.Humanlike)
                            {
                                ExplosionHelper.Explode(Position, Map, Rand.Range(10, 16), TMDamageDefOf.DamageDefOf.TM_Holy, launcher, Mathf.RoundToInt(Rand.Range(20, 32)), 6, TMDamageDefOf.DamageDefOf.TM_Holy.soundExplosion);
                            }
                            else
                            {
                                ExplosionHelper.Explode(Position, Map, Rand.Range(10, 16), TMDamageDefOf.DamageDefOf.TM_Holy, launcher, Mathf.RoundToInt(Rand.Range(16, 24)), 3, TMDamageDefOf.DamageDefOf.TM_Holy.soundExplosion);
                            }
                        }
                        else
                        {
                            try
                            {
                                if (!deadPawn.kindDef.RaceProps.Animal && deadPawn.kindDef.RaceProps.Humanlike)
                                {

                                    ResurrectionUtility.TryResurrectWithSideEffects(deadPawn);
                                    SoundDef.Named("Thunder_OffMap").PlayOneShot(null);
                                    SoundDef.Named("Thunder_OffMap").PlayOneShot(null);
                                    Hediff rec = deadPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ResurrectionPsychosis);
                                    if (rec != null && Rand.Chance(verVal * .33f))
                                    {
                                        deadPawn.health.RemoveHediff(rec);
                                    }
                                    HealthUtility.AdjustSeverity(deadPawn, HediffDef.Named("TM_ResurrectionHD"), 1f);
                                    ReduceSkillsOfPawn(deadPawn, (.35f - .035f * pwrVal));
                                    ApplyHealthDefects(deadPawn, .6f - (.06f * verVal), .3f - .03f * verVal);
                                }

                                if (deadPawn.kindDef.RaceProps.Animal)
                                {
                                    ResurrectionUtility.TryResurrect(deadPawn);
                                    HealthUtility.AdjustSeverity(deadPawn, HediffDef.Named("TM_ResurrectionHD"), 1f);
                                }
                            }
                            catch(NullReferenceException ex)
                            {
                                Log.Warning("Resurrection spell failed or incomplete due to " + ex);
                            }
                        }
                    }                    
                }
            }
            else
            {
                Messages.Message("TM_InvalidResurrection".Translate(
                    caster.LabelShort
                ), MessageTypeDefOf.RejectInput);
                age = timeToRaise;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Vector3 drawPos = Position.ToVector3Shifted(); // this.parent.DrawPos;
            drawPos.z = drawPos.z - 1.5f;
            float num = ((float)Map.Size.z - drawPos.z) * 1.41421354f;
            Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);
            Vector3 a2 = drawPos + a * num * 0.5f;
            a2.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            float num2 = Mathf.Min((float)age / 10f, 1f);
            Vector3 b = a * ((1f - num2) * num);
            float num3 = 0.975f + Mathf.Sin((float)age * 0.3f) * 0.025f;
            if (TicksLeft > (timeToRaise * .2f))
            {
                num3 *= (float)age / (timeToRaise * .8f);
            }
            Color arg_50_0 = colorInt.ToColor;
            Color color = arg_50_0;
            color.a *= num3;
            MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(a2 + a * radius * 0.5f + b, Quaternion.Euler(0f, angle, 0f), new Vector3(radius, 1f, num));
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
            Vector3 pos = drawPos + b;
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix2 = default(Matrix4x4);
            matrix2.SetTRS(pos, Quaternion.Euler(0f, angle, 0f), new Vector3(radius, 1f, radius));
            Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age < timeToRaise;
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

        public static void ReduceSkillsOfPawn(Pawn p, float percent)
        {
            if (p.skills != null)
            {
                //p.skills.Learn(SkillDefOf.Shooting, -(p.skills.GetSkill(SkillDefOf.Shooting).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Animals, -(p.skills.GetSkill(SkillDefOf.Animals).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Artistic, -(p.skills.GetSkill(SkillDefOf.Artistic).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Cooking, -(p.skills.GetSkill(SkillDefOf.Cooking).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Crafting, -(p.skills.GetSkill(SkillDefOf.Crafting).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Plants, -(p.skills.GetSkill(SkillDefOf.Plants).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Intellectual, -(p.skills.GetSkill(SkillDefOf.Intellectual).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Medicine, -(p.skills.GetSkill(SkillDefOf.Medicine).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Melee, -(p.skills.GetSkill(SkillDefOf.Melee).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Mining, -(p.skills.GetSkill(SkillDefOf.Mining).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Social, -(p.skills.GetSkill(SkillDefOf.Social).XpTotalEarned * percent), true);
                //p.skills.Learn(SkillDefOf.Construction, -(p.skills.GetSkill(SkillDefOf.Construction).XpTotalEarned * percent), true);
                foreach(SkillRecord skill in p.skills.skills)
                {
                    p.skills.Learn(skill.def, -(p.skills.GetSkill(skill.def).XpTotalEarned * percent), true);
                    if(skill.xpSinceLastLevel <= -1000)
                    {
                        skill.levelInt--;
                        skill.xpSinceLastLevel += skill.XpRequiredForLevelUp;
                        if (skill.levelInt <= 0)
                        {
                            skill.levelInt = 0;
                            skill.xpSinceLastLevel = 0f;
                            break;
                        }
                    }
                }
            }
        }

        public static void ApplyHealthDefects(Pawn p, float chanceMinor, float chanceMajor)
        {
            List<BodyPartRecord> parts = p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).InRandomOrder().ToList();
            for (int k = 0; k < 2; k++)
            {
                if (Rand.Chance(chanceMinor))
                {
                    List<HediffDef> minorHealthDefects = new List<HediffDef>();
                    minorHealthDefects.Add(HediffDef.Named("BadBack"));
                    minorHealthDefects.Add(HediffDef.Named("HearingLoss"));
                    minorHealthDefects.Add(HediffDefOf.Carcinoma);
                    minorHealthDefects.Add(HediffDef.Named("ChemicalDamageModerate"));

                    HediffDef hdDef = minorHealthDefects.RandomElement();

                    if (hdDef == HediffDef.Named("BadBack"))
                    {
                        for (int i = 0; i < parts.Count; i++)
                        {
                            if (parts[i].def.defName == "Spine")
                            {
                                p.health.AddHediff(hdDef, parts[i]);
                                break;
                            }
                        }
                    }
                    else if (hdDef == HediffDef.Named("HearingLoss"))
                    {
                        for (int i = 0; i < parts.Count; i++)
                        {
                            if (parts[i].def.tags.Contains(BodyPartTagDefOf.HearingSource))
                            {
                                p.health.AddHediff(hdDef, parts[i]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        p.health.AddHediff(hdDef, parts.RandomElement());
                        Hediff hd = p.health.hediffSet.GetFirstHediffOfDef(hdDef);
                        hd.Severity = Rand.Range(.2f, .6f);
                    }
                }
            }

            if (Rand.Chance(chanceMajor))
            {
                List<HediffDef> majorHealthDefects = new List<HediffDef>();
                majorHealthDefects.Add(HediffDef.Named("Frail"));
                majorHealthDefects.Add(HediffDefOf.Dementia);
                majorHealthDefects.Add(HediffDefOf.Carcinoma);
                majorHealthDefects.Add(HediffDef.Named("HeartArteryBlockage"));
                majorHealthDefects.Add(HediffDef.Named("ChemicalDamageSevere"));

                HediffDef hdDef = majorHealthDefects.RandomElement();
                if (hdDef == HediffDefOf.Dementia)
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        if (parts[i].def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
                        {
                            p.health.AddHediff(hdDef, parts[i]);
                            break;
                        }
                    }
                }
                else if (hdDef == HediffDef.Named("Frail"))
                {
                    HealthUtility.AdjustSeverity(p, hdDef, 1f);
                }
                else if(hdDef == HediffDef.Named("HeartArteryBlockage"))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        if (parts[i].def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource))
                        {
                            p.health.AddHediff(hdDef, parts[i]);
                            break;
                        }
                    }
                }
                else
                {
                    p.health.AddHediff(hdDef, parts.RandomElement());
                    Hediff hd = p.health.hediffSet.GetFirstHediffOfDef(hdDef);
                    hd.Severity = Rand.Range(.25f, .8f);
                }
            }
        }
    }
}


