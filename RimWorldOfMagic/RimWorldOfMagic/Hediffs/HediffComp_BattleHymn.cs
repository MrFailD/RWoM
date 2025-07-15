using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TorannMagic
{
    internal class HediffComp_BattleHymn : HediffComp
    {
        private bool initializing = true;
        private float chantRange = 15f;
        private int chantFrequency = 300;
        private int pwrVal;
        private int verVal;
        private int effVal;

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
            if (spawned)
            {
                CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
                pwrVal = TM_Calc.GetSkillPowerLevel(Pawn, TorannMagicDefOf.TM_BattleHymn);
                verVal = TM_Calc.GetSkillVersatilityLevel(Pawn, TorannMagicDefOf.TM_BattleHymn);
                effVal = TM_Calc.GetSkillEfficiencyLevel(Pawn, TorannMagicDefOf.TM_BattleHymn);
                //MagicPowerSkill pwr = comp.MagicData.MagicPowerSkill_BattleHymn.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BattleHymn_pwr");
                //MagicPowerSkill ver = comp.MagicData.MagicPowerSkill_BattleHymn.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BattleHymn_ver");
                //MagicPowerSkill eff = comp.MagicData.MagicPowerSkill_BattleHymn.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_BattleHymn_eff");
                //this.verVal = ver.level;
                //this.pwrVal = pwr.level;
                //this.effVal = eff.level;
                //
                //if (ModOptions.Settings.Instance.AIHardMode && !this.Pawn.IsColonist)
                //{
                //    pwrVal = 1;
                //    verVal = 1;
                //    effVal = 1;
                //}
                chantRange = chantRange + (verVal * 3f);
                chantFrequency = 300 - (30 * verVal);
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
            Map map = Pawn.Map;

            bool flag4 = Find.TickManager.TicksGame % chantFrequency == 0;
            if (flag4 && map != null)
            {
                CompAbilityUserMagic comp = Pawn.GetCompAbilityUserMagic();
                if (comp.Mana.CurLevel > (.09f - (.009f * effVal)))
                {
                    List<Pawn> pawns = Pawn.Map.mapPawns.AllPawnsSpawned.ToList();
                    for (int i = 0; i < pawns.Count; i++)
                    {
                        if (pawns[i].RaceProps.Humanlike && pawns[i].Faction != null && pawns[i].Faction == Pawn.Faction)
                        {
                            if ((pawns[i].Position - Pawn.Position).LengthHorizontal <= chantRange)
                            {
                                HealthUtility.AdjustSeverity(pawns[i], HediffDef.Named("TM_BattleHymnHD"), Rand.Range(.4f, .7f) + (.15f * pwrVal));
                                TM_MoteMaker.ThrowNoteMote(pawns[i].DrawPos, pawns[i].Map, .3f);
                                TM_MoteMaker.ThrowNoteMote(pawns[i].DrawPos, pawns[i].Map, .2f);
                                if(Rand.Chance(.04f + (.01f * pwrVal)))
                                {
                                    List<InspirationDef> id = new List<InspirationDef>();
                                    id.Add(TorannMagicDefOf.ID_Champion); id.Add(TorannMagicDefOf.ID_ManaRegen); id.Add(TorannMagicDefOf.Frenzy_Go); id.Add(TorannMagicDefOf.Frenzy_Shoot);
                                    pawns[i].mindState.inspirationHandler.TryStartInspiration(id.RandomElement());
                                }
                            }
                        }
                    }
                    comp.Mana.CurLevel -= (.09f - (.009f * effVal));
                    TM_MoteMaker.ThrowSiphonMote(Pawn.DrawPos, Pawn.Map, .5f);
                }
                else
                {
                    Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("TM_SingBattleHymnHD"), false);
                    Pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref effVal, "effVal", 0, false);
            Scribe_Values.Look<int>(ref chantFrequency, "chantFrequency", 300, false);
            Scribe_Values.Look<float>(ref chantRange, "chantRange", 11f, false);
        }
    }
}
