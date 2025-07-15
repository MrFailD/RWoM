using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Reversal : HediffComp
    {

        private bool initialized;
        private int age;

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
                age = 60;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn != null;
            if (flag)
            {
                if (!initialized)
                {
                    initialized = true;
                    Initialize();
                }
                if(age <=0)
                {
                    severityAdjustment--;
                    age = 60;
                }
                age--;
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || parent.Severity < .1f;
            }
        }
    }
}
