using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace TorannMagic
{
    public class Need_Stamina : Need  //Original code by Jecrell
    {
        public const float BaseGainPerTickRate = 150f;

        public const float BaseFallPerTick = 1E-05f;

        public const float ThreshVeryLow = 0.1f;

        public const float ThreshLow = 0.3f;

        public const float ThreshSatisfied = 0.5f;

        public const float ThreshHigh = 0.7f;

        public const float ThreshVeryHigh = 0.9f;

        public int ticksUntilBaseSet = 500;

        public float lastGainPct;

        private int lastGainTick;

        public float baseStaminaGain;
        public float modifiedStaminaGain;
        public float drainSkillUpkeep;

        public StaminaPoolCategory CurCategory
        {
            get
            {
                bool flag = CurLevel < 0.1f;
                StaminaPoolCategory result;
                if (flag)
                {
                    result = StaminaPoolCategory.Fatigued;
                }
                else
                {
                    bool flag2 = CurLevel < 0.3f;
                    if (flag2)
                    {
                        result = StaminaPoolCategory.Weakened;
                    }
                    else
                    {
                        bool flag3 = CurLevel < 0.5f;
                        if (flag3)
                        {
                            result = StaminaPoolCategory.Steady;
                        }
                        else
                        {
                            bool flag4 = CurLevel < 0.7f;
                            if (flag4)
                            {
                                result = StaminaPoolCategory.Energetic;
                            }
                            else
                            {
                                result = StaminaPoolCategory.Surging;
                            }
                        }
                    }
                }
                return result;
            }
        }

        public override float CurLevel
        {
            get => base.CurLevel;
            set => base.CurLevel = Mathf.Clamp(value, 0f, pawn.GetCompAbilityUserMight().maxSP);
        }

        public override float MaxLevel => pawn.ageTracker.AgeBiologicalYears < 13 ? pawn.GetCompAbilityUserMight().maxSP - ((12f - pawn.ageTracker.AgeBiologicalYearsFloat)/10f) : pawn.GetCompAbilityUserMight().maxSP;

        public override int GUIChangeArrow
        {
            get
            {
                return GainingNeed ? 1 : -1;
            }
        }

        public override float CurInstantLevel
        {
            get
            {
                return CurLevel;
            }
        }

        private bool GainingNeed
        {
            get
            {
                return Find.TickManager.TicksGame < lastGainTick + 10;
            }
        }

        static Need_Stamina()
        {
        }

        public Need_Stamina(Pawn pawn) : base(pawn)
		    {
            lastGainTick = -999;
            threshPercents = new List<float>();
            threshPercents.Add((0.25f / MaxLevel));
            threshPercents.Add((0.5f / MaxLevel));
            threshPercents.Add((0.75f / MaxLevel));
        }

        private void AdjustThresh()
        {
            threshPercents.Clear();
            threshPercents.Add((0.25f / MaxLevel));
            threshPercents.Add((0.5f / MaxLevel));
            threshPercents.Add((0.75f / MaxLevel));
            if (MaxLevel > 1)
            {
                threshPercents.Add((1f / MaxLevel));
            }
            if (MaxLevel > 1.25f)
            {
                threshPercents.Add((1.25f / MaxLevel));
            }
            if (MaxLevel > 1.5f)
            {
                threshPercents.Add((1.5f / MaxLevel));
            }
            if (MaxLevel > 1.75f)
            {
                threshPercents.Add((1.75f / MaxLevel));
            }
            if (MaxLevel > 2f)
            {
                threshPercents.Add((2f / MaxLevel));
            }
            if (MaxLevel > 2.5f)
            {
                threshPercents.Add((2.5f / MaxLevel));
            }
            if (MaxLevel > 3f)
            {
                threshPercents.Add((3f / MaxLevel));
            }
            if (MaxLevel > 4f)
            {
                threshPercents.Add((4f / MaxLevel));
            }
            if (MaxLevel > 5f)
            {
                threshPercents.Add((5f / MaxLevel));
            }
        }

        public override void SetInitialLevel()
        {
            CurLevel = 0.8f;
        }

        public void GainNeed(float amount)
        {
            if (!this.pawn.DestroyedOrNull() && !this.pawn.Dead && this.pawn.story != null && this.pawn.story.traits != null)
            {
                if (!this.pawn.NonHumanlikeOrWildMan())
                {
                    Pawn pawn = this.pawn;
                    CompAbilityUserMight comp = pawn.GetCompAbilityUserMight();
                    
                    amount = amount * (0.015f);
                    baseStaminaGain = amount * ModOptions.Settings.Instance.needMultiplier;
                    amount *= comp.spRegenRate;
                    if (pawn.health != null && pawn.health.hediffSet != null)
                    {
                        Hediff hdRegen = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_EnergyRegenHD);
                        if (hdRegen != null)
                        {
                            amount *= hdRegen.Severity;
                        }
                    }
                    modifiedStaminaGain = amount - baseStaminaGain;
                    amount = Mathf.Min(amount, MaxLevel - CurLevel);
                    curLevelInt += amount;
                    lastGainPct = amount;
                    lastGainTick = Find.TickManager.TicksGame;
                    comp.Stamina.curLevelInt = Mathf.Clamp(comp.Stamina.curLevelInt += amount, 0f, MaxLevel);

                }
                AdjustThresh();
            }
        }

        public void UseMightPower(float amount)
        {
            curLevelInt = Mathf.Clamp(curLevelInt - amount, 0f, pawn.GetCompAbilityUserMight().maxSP); //change for max sp
        }

        public override void NeedInterval()
        {
            GainNeed(1f);
        }

        public override string GetTipString()
        {
            //return base.GetTipString();
            return string.Concat(new string[]
            {
                LabelCap,
                ": ",
                (CurLevel / .01f).ToString("n2"),
                "\n",
                def.description
            });
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
        {
            bool flag = rect.height > 70f;
            if (flag)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            Rect rect2 = rectForTooltip ?? rect;
            if (Mouse.IsOver(rect2))
            {
                Widgets.DrawHighlight(rect2);
            }
            if (doTooltip && Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, new TipSignal(() => GetTipString(), rect2.GetHashCode()));
            }
            float num2 = 14f;
            float num3 = num2 + 15f;
            bool flag3 = rect.height < 50f;
            if (flag3)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect _rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(_rect2, LabelCap);
            GUI.color = Color.yellow;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Widgets.FillableBar(rect3, CurLevelPercentage);
            bool flag4 = threshPercents != null;
            if (flag4)
            {
                for (int i = 0; i < threshPercents.Count; i++)
                {
                    DrawBarThreshold(rect3, threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = Mathf.Clamp(CurLevel / MaxLevel, 0f, 1f);
            bool flag5 = curInstantLevelPercentage >= 0f;
            if (flag5)
            {
                DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            bool flag6 = !def.tutorHighlightTag.NullOrEmpty();
            if (flag6)
            {
                UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            bool flag = threshPct < CurLevelPercentage;
            Texture2D image;
            if (flag)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }
    }
}
