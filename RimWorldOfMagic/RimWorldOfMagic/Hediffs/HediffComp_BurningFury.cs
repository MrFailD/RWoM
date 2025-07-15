using RimWorld;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using TorannMagic.Golems;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_BurningFury : HediffComp
    {

        private bool initializing = true;
        private int nextAction = 1;
        private int nextSlowAction = 1;
        private bool removeNow;
        private CompAbilityUserMight comp;
        private float intensity = 1f;
        private float drain = 1f;

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
            if (spawned && Pawn.Map != null)
            {
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (initializing)
                {
                    initializing = false;
                    Initialize();
                }
            }
            if (!Pawn.DestroyedOrNull() && Pawn.Spawned && !Pawn.Downed)
            {
                if(comp == null && TM_Calc.IsMightUser(Pawn))
                {
                    comp = Pawn.GetCompAbilityUserMight();
                    int pwrVal = comp.MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_pwr").level;
                    if (pwrVal >= 4)
                    {
                        intensity = 1.5f;
                        if(pwrVal >= 14)
                        {
                            intensity = 2f;
                        }
                    }
                    int verVal = comp.MightData.MightPowerSkill_FieldTraining.FirstOrDefault((MightPowerSkill x) => x.label == "TM_FieldTraining_ver").level;
                    if (pwrVal >= 14)
                    {
                        drain = .65f;
                    }
                }
                if (Find.TickManager.TicksGame % 30 == 0)
                {                    
                    if (comp != null && comp.Stamina != null)
                    {
                        comp.Stamina.CurLevel -= (.02f * drain);
                        if (comp.Stamina.CurLevel <= .001f)
                        {
                            removeNow = true;
                        }
                    }
                    else if(Pawn is TMPawnGolem || Pawn is TMHollowGolem)
                    {
                        severityAdjustment -= .01f;
                        if(parent.Severity <= .01f)
                        {
                            removeNow = true;
                        }
                    }
                    else
                    {
                        removeNow = true;
                    }
                }
                if (!removeNow && Find.TickManager.TicksGame >= nextAction)
                {
                    nextAction = Find.TickManager.TicksGame + Mathf.RoundToInt(Rand.Range(50f/intensity, 80f/intensity));
                    TickAction();
                }
                if (!removeNow && Find.TickManager.TicksGame % nextSlowAction == 0)
                {
                    nextSlowAction = Rand.Range(200, 500);
                    SlowTickAction();
                }
            }
            else
            {
                removeNow = true;
            }
        }

        public void TickAction()
        {
            Pawn victim = TM_Calc.FindNearbyEnemy(Pawn, 2);
            if (victim != null)
            {
                TM_Action.DamageEntities(victim, null, Rand.Range(4, 6), DamageDefOf.Burn, Pawn);
                TM_MoteMaker.ThrowFlames(victim.DrawPos, victim.Map, Rand.Range(.1f, .4f));
            }

            if(Rand.Chance(.2f))
            {
                TM_Action.DamageEntities(Pawn, null, Rand.Range(3, 5), 5f, DamageDefOf.Burn, Pawn);
                TM_MoteMaker.ThrowFlames(Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .2f));
            }

        }

        public void SlowTickAction()
        {
            using (IEnumerator<BodyPartRecord> enumerator = Pawn.health.hediffSet.GetInjuredParts().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord rec = enumerator.Current;

                    IEnumerable<Hediff_Injury> arg_BB_0 = Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>();
                    Func<Hediff_Injury, bool> arg_BB_1;

                    arg_BB_1 = ((Hediff_Injury injury) => injury.Part == rec);

                    foreach (Hediff_Injury current in arg_BB_0.Where(arg_BB_1))
                    {
                        bool flag5 = current.CanHealNaturally() && current.TendableNow();
                        if (flag5)
                        {
                            if (Rand.Chance(.15f))
                            {
                                DamageInfo dinfo;
                                dinfo = new DamageInfo(DamageDefOf.Burn, Mathf.RoundToInt(current.Severity / 2), 0, (float)-1, Pawn, rec, null, DamageInfo.SourceCategory.ThingOrUnknown);
                                dinfo.SetAllowDamagePropagation(false);
                                dinfo.SetInstantPermanentInjury(true);
                                current.Heal(100);
                                TM_MoteMaker.ThrowFlames(Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .2f));
                                Pawn.TakeDamage(dinfo);
                            }
                            else
                            {
                                current.Tended(1, 1);
                                TM_MoteMaker.ThrowFlames(Pawn.DrawPos, Pawn.Map, Rand.Range(.1f, .2f));
                            }
                            goto ExitEnum;
                        }
                    }
                }
            }
            ExitEnum:;
        }

        public override bool CompShouldRemove
        {
            get
            {
                return removeNow || !Pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_BurningFuryHD, false) || base.CompShouldRemove;
            }
        }
    }
}
