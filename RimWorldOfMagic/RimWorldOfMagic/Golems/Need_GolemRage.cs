using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TorannMagic.Golems
{
    public class Need_GolemRage : Need  
    {
        public int lastTick = -999;

        private float needGain = 1f;
        private float needLoss = 1f;

        public float maxEnergy = 100f;
        public float energyEfficiency = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref maxEnergy, "maxEnergy", 100);
            Scribe_Values.Look<float>(ref energyEfficiency, "energyEfficiency", 1f);
        }

        public float NeedLoss => needLoss;

        public override float MaxLevel
        {
            get
            {
                return maxEnergy; 
            }
        }

        public void AddEnergy(float amount)
        {
            CurLevel += (amount * energyEfficiency);
        }

        public float ActualNeedCost(float amount)
        {
            return amount / energyEfficiency;
        }

        public float CurEnergyPercent => Mathf.Clamp01(CurLevel / MaxLevel);

        public GolemRageCategory CurCategory
        {
            get
            {
                bool flag = CurLevel < 10f;
                GolemRageCategory result;
                if (flag)
                {
                    result = GolemRageCategory.None;
                }
                else
                {
                    bool flag2 = CurLevel < 25f;
                    if (flag2)
                    {
                        result = GolemRageCategory.Low;
                    }
                    else if(CurLevel < 75f)
                    {
                        result = GolemRageCategory.Medium;
                    }
                    else
                    {
                        result = GolemRageCategory.High;
                    }
                }
                return result;
            }
        }

        static Need_GolemRage()
        {
        }

        public Need_GolemRage(Pawn pawn) : base(pawn)
		{
            lastTick = -999;
            threshPercents = new List<float>();
            threshPercents.Add((10f / MaxLevel));
            threshPercents.Add((25f / MaxLevel));
            threshPercents.Add((75f / MaxLevel));
        }

        public override void SetInitialLevel()
        {
            CurLevel = 0f;
        }

        public override void NeedInterval()
        {
            if(!base.IsFrozen)
            {
                CurLevel -= ActualNeedCost(NeedLoss);
            }
        }

        public bool InCaravan(Pawn p)
        {
            if(p.Map == null)
            {
                if (p.ParentHolder.ToString().Contains("Caravan"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public enum GolemRageCategory
    {
        None,
        Low,
        Medium,
        High
    }
}
