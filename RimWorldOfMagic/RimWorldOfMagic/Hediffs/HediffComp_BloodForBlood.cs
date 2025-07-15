using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_BloodForBlood : HediffComp
    {

        private bool initialized;
        private int initializeDelay;
        private bool removeNow;

        private int eventFrequency = 120;

        //private int bfbPwr = 0;  //increased amount blood levels affect ability power
        private int bfbVer;  //increased blood per bleed rate and blood gift use
        private float arcaneDmg = 1f;

        private float bleedRate = 0;

        public Pawn linkedPawn;
        private Vector3 directionToLinkedPawn = default(Vector3);

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look<Pawn>(ref linkedPawn, "linkedPawn", false);
            Scribe_Values.Look<int>(ref bfbVer, "bfbVer", 0, false);
            Scribe_Values.Look<float>(ref arcaneDmg, "arcaneDmg", 1f, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
        }

        public string labelCap
        {
            get
            {
                return Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return Def.label;
            }
        }

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            CompAbilityUserMagic comp = linkedPawn.GetCompAbilityUserMagic();
            if(TM_Calc.GetBloodLossTypeDef(Pawn.health.hediffSet.hediffs) == null)
            {
                HealthUtility.AdjustSeverity(Pawn, HediffDefOf.BloodLoss, .05f);
            }
            if (spawned && comp != null && comp.IsMagicUser)
            {
                bfbVer = comp.MagicData.MagicPowerSkill_BloodForBlood.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BloodForBlood_ver").level;
                arcaneDmg = comp.arcaneDmg;
                directionToLinkedPawn = TM_Calc.GetVector(Pawn.DrawPos, linkedPawn.DrawPos);
            }
            else
            {
                removeNow = true;
            }
        }        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null && Pawn.Map != null && initializeDelay > 5;
            if (flag)
            {
                if (!initialized && linkedPawn != null)
                {
                    initialized = true;
                    Initialize();
                }

                if (Find.TickManager.TicksGame % eventFrequency == 0)
                {
                    if(!linkedPawn.DestroyedOrNull() && !linkedPawn.Dead)
                    {
                        directionToLinkedPawn = TM_Calc.GetVector(Pawn.DrawPos, linkedPawn.DrawPos);
                        HealWounds();
                        HealLinkedPawnWounds();
                        AdjustBloodLoss();
                        severityAdjustment -= Rand.Range(.04f, .06f);

                        if (Rand.Chance(.4f) && !Pawn.DestroyedOrNull() && !Pawn.Dead && !linkedPawn.DestroyedOrNull() && !linkedPawn.Dead)
                        {
                            List<Need> needs = linkedPawn.needs.AllNeeds;
                            for (int n = 0; n < needs.Count; n++)
                            {
                                Need need = needs[n];
                                if (need.def.defName == "ROMV_Blood")
                                {
                                    need.CurLevel++;
                                }
                            }
                            needs = Pawn.needs.AllNeeds;
                            for (int n = 0; n < needs.Count; n++)
                            {
                                Need need = needs[n];
                                if (need.def.defName == "ROMV_Blood")
                                {
                                    need.CurLevel--;
                                    if(need.CurLevel <= .5f)
                                    {
                                        Pawn.Kill(null, null);
                                    }
                                }
                            }
                        }

                    }                    
                    else
                    {
                        removeNow = true;
                    }
                }
            }
            else
            {
                initializeDelay++;
            }
        }

        public void AdjustBloodLoss()
        {
            float bloodLoss = 1 + (.25f *bleedRate);
            if(Pawn.Faction == linkedPawn.Faction)
            {
                bloodLoss = (bloodLoss / 2f);
            }
            //Log.Message("adjusting blood loss by " + .03f * bloodLoss +  " bleed rate is " + this.bleedRate);
            HediffDef bloodType = TM_Calc.GetBloodLossTypeDef(Pawn.health.hediffSet.hediffs);
            if (bloodType != null)
            {
                HealthUtility.AdjustSeverity(Pawn, HediffDef.Named(bloodType.ToString()), (.04f * bloodLoss)/Mathf.Clamp(Pawn.BodySize,.25f, 4f));
            }
            bloodType = null;
            bloodType = TM_Calc.GetBloodLossTypeDef(linkedPawn.health.hediffSet.hediffs);
            if (bloodType != null)
            {
                HealthUtility.AdjustSeverity(linkedPawn, HediffDef.Named(bloodType.ToString()), -((.03f * bloodLoss)/Mathf.Clamp(linkedPawn.BodySize, .25f, 4f)));
            }
        }

        public void HealLinkedPawnWounds()
        {
            List<IntVec3> cellList = GenRadial.RadialCellsAround(linkedPawn.Position, .4f + bfbVer, true).ToList();
            for (int i =0; i< cellList.Count; i++)
            {
                Pawn healedPawn = cellList[i].GetFirstPawn(linkedPawn.Map);
                if(healedPawn != null && healedPawn.Faction != null && healedPawn.Faction == linkedPawn.Faction)
                {
                    TM_Action.DoAction_HealPawn(linkedPawn, healedPawn, 1, (1f + .35f * bfbVer) * (1 + bleedRate) * arcaneDmg);
                    if(healedPawn == linkedPawn)
                    {
                        Vector3 revDir = linkedPawn.DrawPos - (2 * directionToLinkedPawn);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist, revDir, Pawn.Map, Rand.Range(.8f, 1f), .2f, 0.05f, 1f, Rand.Range(-50, 50), Rand.Range(2f, 3f), (Quaternion.AngleAxis(90, Vector3.up) * directionToLinkedPawn).ToAngleFlat(), Rand.Range(0, 360));
                    }
                }
            }
            if(bfbVer > 0)
            {
                Effecter BFBEffect = TorannMagicDefOf.TM_BFBEffecter.Spawn();
                BFBEffect.Trigger(new TargetInfo(linkedPawn.Position, linkedPawn.Map, false), new TargetInfo(linkedPawn.Position, linkedPawn.Map, false));
                BFBEffect.Cleanup();
            }

            int num = 1;
            using (IEnumerator<BodyPartRecord> enumerator =linkedPawn.health.hediffSet.GetInjuredParts().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord rec = enumerator.Current;
                    bool flag2 = num > 0;
                    if (flag2)
                    {
                        int num2 = 1;
                        IEnumerable<Hediff_Injury> arg_BB_0 = linkedPawn.health.hediffSet.hediffs
                            .OfType<Hediff_Injury>();
                        Func<Hediff_Injury, bool> arg_BB_1;

                        arg_BB_1 = ((Hediff_Injury injury) => injury.Part == rec);

                        foreach (Hediff_Injury current in arg_BB_0.Where(arg_BB_1))
                        {
                            bool flag4 = num2 > 0;
                            if (flag4)
                            {
                                bool flag5 = !current.CanHealNaturally();
                                if (flag5)
                                {
                                    if (rec.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
                                    {
                                        current.Heal(Rand.Range(.03f, .05f) * arcaneDmg);
                                    }
                                    else
                                    {
                                        current.Heal(Rand.Range(.15f, .25f) * arcaneDmg);
                                        //current.Heal(5.0f + (float)pwrVal * 3f); // power affects how much to heal
                                        num--;
                                        num2--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void HealWounds()
        {
            Hediff_Injury injuries = Pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury => injury.CanHealNaturally())
                .FirstOrDefault();
            if (injuries == null) return;

            injuries.Heal(2f + (.25f * Pawn.health.hediffSet.BleedRateTotal));
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist, Pawn.DrawPos, Pawn.Map, Rand.Range(.5f, .85f), .2f, 0.05f, 1f, Rand.Range(-50, 50), Rand.Range(1f, 1.7f), (Quaternion.AngleAxis(Rand.Range(70f, 110f), Vector3.up) * directionToLinkedPawn).ToAngleFlat(), Rand.Range(0, 360));
                        
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || removeNow;
            }
        }        
    }
}
