using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffComp_SpellMending : HediffComp
    {

        private bool initializing = true;
        private int ticksTillNextMend;
        private int mendTick;

        public int mendTickTimer = 80;

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
            if(mendTick > ticksTillNextMend)
            {
                TickAction();
            }
            mendTick++;
        }

        public void TickAction()
        {
            List<Apparel> gear = Pawn.apparel.WornApparel;
            int tmpDmgItems = 0;

            for (int i = 0; i < gear.Count; i++)
            {
                if (gear[i].HitPoints < gear[i].MaxHitPoints)
                {
                    gear[i].HitPoints++;
                    for (int j = 0; j < Rand.Range(1, 3); j++)
                    {
                        TM_MoteMaker.ThrowTwinkle(Pawn.DrawPos, Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(100, 500), Rand.Range(.4f, 1f), Rand.Range(.05f, .2f), .05f, Rand.Range(.4f, .85f));
                    }
                    tmpDmgItems++;
                }
                if (gear[i].WornByCorpse && Rand.Chance(.1f))
                {
                    Traverse.Create(root: gear[i]).Field(name: "wornByCorpseInt").SetValue(false);
                }
            }
            Thing weapon = Pawn.equipment.Primary;
            if (weapon != null && (weapon.def.IsRangedWeapon || weapon.def.IsMeleeWeapon))
            {
                if (weapon.HitPoints < weapon.MaxHitPoints)
                {
                    weapon.HitPoints++;
                    for (int j = 0; j < Rand.Range(1, 3); j++)
                    {
                        TM_MoteMaker.ThrowTwinkle(Pawn.DrawPos, Pawn.Map, Rand.Range(.4f, .7f), Rand.Range(100, 500), Rand.Range(.4f, 1f), Rand.Range(.05f, .2f), .05f, Rand.Range(.4f, .85f));
                    }                    
                    tmpDmgItems++;
                }
            }
            mendTick = 0;
            ticksTillNextMend = mendTickTimer * tmpDmgItems;
        }

    }
}
