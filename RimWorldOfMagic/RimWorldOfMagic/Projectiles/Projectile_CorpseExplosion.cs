using System.Linq;
using Verse;
using AbilityUser;
using UnityEngine;
using RimWorld;
using TorannMagic.Weapon;

namespace TorannMagic
{
    internal class Projectile_CorpseExplosion : Projectile_AbilityBase
    {
        private int age = 300;
        private int verVal;
        private int pwrVal;
        private float arcaneDmg = 1;
        private float radius = 3f;
        private bool initialized;
        private Corpse targetCorpse;
        private Pawn targetPawn;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = age > 0;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            age--;
        }

        public void Initialize()
        {
            radius = 3f + (.3f * (verVal+pwrVal));
            age = age - (60 * verVal);
            initialized = true;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            Pawn pawn = launcher as Pawn;

            if (!initialized)
            {
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                MagicPowerSkill pwr = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_CorpseExplosion.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_CorpseExplosion_pwr");
                MagicPowerSkill ver = pawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_CorpseExplosion.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_CorpseExplosion_ver");
                arcaneDmg = comp.arcaneDmg;
                pwrVal = pwr.level;
                verVal = ver.level;
                if (pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    MightPowerSkill mpwr = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_pwr");
                    MightPowerSkill mver = pawn.GetCompAbilityUserMight().MightData.MightPowerSkill_Mimic.FirstOrDefault((MightPowerSkill x) => x.label == "TM_Mimic_ver");
                    pwrVal = mpwr.level;
                    verVal = mver.level;
                }
                Initialize();

                CellRect cellRect = CellRect.CenteredOn(Position, 1);
                cellRect.ClipInsideMap(map);
                IntVec3 curCell = cellRect.CenterCell;
                if (curCell.InBoundsWithNullCheck(map) && curCell.IsValid)
                {
                    Pawn undead = curCell.GetFirstPawn(map);
                    bool flag = undead != null && !undead.Dead;
                    if (flag)
                    {
                        if(TM_Calc.IsUndead(undead))
                        {
                            targetPawn = undead;
                        }
                        //if (undead.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadHD))
                        //{
                        //    this.targetPawn = undead;
                        //}
                        //if (undead.health.hediffSet.HasHediff(TorannMagicDefOf.TM_UndeadAnimalHD))
                        //{
                        //    this.targetPawn = undead;
                        //}
                    }

                    Thing corpseThing = curCell.GetFirstItem(map);
                    Corpse corpse = null;
                    if (corpseThing != null)
                    {
                        bool validator = corpseThing is Corpse;
                        if (validator)
                        {                            
                            corpse = corpseThing as Corpse;
                            Pawn corpsePawn = corpse.InnerPawn;
                            if (corpsePawn.RaceProps.IsFlesh)
                            {
                                if (corpsePawn.RaceProps.Humanlike || corpsePawn.RaceProps.Animal || TM_Calc.IsUndead(corpsePawn))
                                {
                                    targetCorpse = corpse;
                                }
                            }
                        }
                    }
                }
            }     
            
            if(targetPawn != null && !targetPawn.Destroyed)
            {
                if (age == 360)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "6", -.5f);
                }
                if (age == 300)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "5", -.5f);
                }
                if (age == 240)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "4", -.5f);
                }
                if (age == 180)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "3", -.5f);
                }
                if (age == 120)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "2", -.5f);
                }
                if (age == 60)
                {
                    MoteMaker.ThrowText(targetPawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "1", -.5f);
                }
                if(age == 1)
                {
                    //explode
                    TM_MoteMaker.ThrowBloodSquirt(targetPawn.Position.ToVector3Shifted(), map, 1.2f);
                    TM_MoteMaker.ThrowBloodSquirt(targetPawn.Position.ToVector3Shifted(), map, .6f);
                    TM_MoteMaker.ThrowBloodSquirt(targetPawn.Position.ToVector3Shifted(), map, .8f);
                    if (targetPawn.def != TorannMagicDefOf.TM_SkeletonLichR && targetPawn.def != TorannMagicDefOf.TM_GiantSkeletonR)
                    {
                        if (targetPawn.RaceProps.Humanlike)
                        {
                            targetPawn.inventory.DropAllNearPawn(targetPawn.Position, false, true);
                            targetPawn.equipment.DropAllEquipment(targetPawn.Position, false);
                            targetPawn.apparel.DropAll(targetPawn.Position, false);
                        }
                        if (!targetPawn.Destroyed)
                        {
                            targetPawn.Destroy();
                        }
                    }
                    ExplosionHelper.Explode(targetPawn.Position, map, radius, TMDamageDefOf.DamageDefOf.TM_CorpseExplosion, launcher, Mathf.RoundToInt((Rand.Range(22f, 36f) + (5f * pwrVal)) * arcaneDmg), 0, this.def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 01, null, false, null, 0f, 0, 0.0f, true);

                }
            }

            if (targetCorpse != null && !targetCorpse.Destroyed)
            {
                if (age == 360)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "6", -.5f);
                }
                if (age == 300)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "5", -.5f);
                }
                if (age == 240)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "4", -.5f);
                }
                if (age == 180)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "3", -.5f);
                }
                if (age == 120)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "2", -.5f);
                }
                if (age == 60)
                {
                    MoteMaker.ThrowText(targetCorpse.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, "1", -.5f);
                }
                if (age == 1)
                {
                    //explode
                    TM_MoteMaker.ThrowBloodSquirt(targetCorpse.Position.ToVector3Shifted(), map, 1.2f);
                    TM_MoteMaker.ThrowBloodSquirt(targetCorpse.Position.ToVector3Shifted(), map, .6f);
                    TM_MoteMaker.ThrowBloodSquirt(targetCorpse.Position.ToVector3Shifted(), map, .8f);
                    Pawn corpsePawn = targetCorpse.InnerPawn;
                    if (corpsePawn.RaceProps.Humanlike)
                    {
                        corpsePawn.inventory.DropAllNearPawn(targetCorpse.Position, false, true);
                        corpsePawn.equipment.DropAllEquipment(targetCorpse.Position, false);
                        corpsePawn.apparel.DropAll(targetCorpse.Position, false);
                    }
                    ExplosionHelper.Explode(targetCorpse.Position, map, radius, TMDamageDefOf.DamageDefOf.TM_CorpseExplosion, launcher, Mathf.RoundToInt((Rand.Range(18f, 30f) + (5f * pwrVal))*arcaneDmg), 0, this.def.projectile.soundExplode, def, equipmentDef, null, null, 0f, 01, null, false, null, 0f, 0, 0.0f, true);
                    if (!targetCorpse.Destroyed)
                    {
                        targetCorpse.Destroy();
                    }
                }
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref age, "age", 360, false);
            Scribe_Values.Look<float>(ref radius, "radius", 2.4f, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
        }

    }    
}


