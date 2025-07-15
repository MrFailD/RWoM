using Verse;
using RimWorld;
using System.Linq;
using System.Text;

namespace TorannMagic.Enchantment
{
    internal class HediffComp_Enchantment : HediffComp
    {
        private bool initializing = true;
        private bool removeNow;

        private string enchantment ="";

        private CompAbilityUserMagic compMagic;
        private CompAbilityUserMight compMight;

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

        public bool IsMagicUser
        {
            get
            {
                if(compMagic != null && compMagic.IsMagicUser)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsMightUser
        {
            get
            {
                if (compMight != null && compMight.IsMightUser)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsDualClass
        {
            get
            {
                if (IsMagicUser && IsMightUser)
                {
                    return true;
                }
                return false;
            }
        }

        public override string CompLabelInBracketsExtra => enchantment;

        private void Initialize()
        {
            bool spawned = Pawn.Spawned;
            if (spawned)
            {
                //FleckMaker.ThrowLightningGlow(base.Pawn.TrueCenter(), base.Pawn.Map, 3f);
            }
        }

        public override bool CompShouldRemove => base.CompShouldRemove || removeNow;

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
            if(Find.TickManager.TicksGame % 120 == 0)
            {
                compMagic = Pawn.GetCompAbilityUserMagic();
                compMight = Pawn.GetCompAbilityUserMight();
                DetermineEnchantments();
            }
            if(Find.TickManager.TicksGame % 480 == 0 && enchantment == "unknown")
            {
                removeNow = true;
            }
        }

        private void DetermineEnchantments()
        {
            if (parent.def.defName == "TM_HediffEnchantment_maxEnergy")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.maxMP, compMight.maxSP);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.maxMP, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.maxSP);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_coolDown")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.coolDown, compMight.coolDown);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.coolDown, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.coolDown);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_energyCost")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.mpCost, compMight.spCost);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.mpCost, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.spCost);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_energyRegen")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.mpRegenRate, compMight.spRegenRate);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.mpRegenRate, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.spRegenRate);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_xpGain")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.xpGain, compMight.xpGain);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.xpGain, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.xpGain);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_dmgResistance")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.arcaneRes, compMight.arcaneRes);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.arcaneRes, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.arcaneRes);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_dmgBonus")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.arcaneDmg, compMight.mightPwr);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.arcaneDmg, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.mightPwr);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_arcalleumCooldown")
            {
                if (IsDualClass)
                {
                    DisplayEnchantments(compMagic.arcalleumCooldown, compMight.arcalleumCooldown);
                }
                else if (IsMagicUser)
                {
                    DisplayEnchantments(compMagic.arcalleumCooldown, 1f);
                }
                else if (IsMightUser)
                {
                    DisplayEnchantments(1f, compMight.arcalleumCooldown);
                }
                else
                {
                    removeNow = true;
                }
            }
            else if (parent.def.defName == "TM_HediffEnchantment_arcaneSpectre")
            {
                enchantment = "TM_ArcaneSpectre".Translate();
            }
            else if (parent.def.defName == "TM_HediffEnchantment_phantomShift")
            {
                enchantment = "TM_PhantomShift".Translate();
            }
            else
            {
                Log.Message("enchantment unknkown");
                enchantment = "unknown";
            }           

        }

        private void DisplayEnchantments(float magVal = 1f, float mitVal = 1f)
        {
            string txtMagic = "";
            string txtMight = "";

            if (magVal != 1f)
            {
                txtMagic = (magVal * 100).ToString("0.##") + "%";
            }
            if (mitVal != 1f)
            {
                txtMight = (mitVal * 100).ToString("0.##") + "%";
            }

            if (txtMagic != "" && txtMight != "")
            {
                if (magVal != mitVal)
                { 
                    enchantment = txtMagic + " | " + txtMight;
                }
                else
                {
                    enchantment = txtMagic;
                }
            }
            else
            {
                enchantment = txtMagic + txtMight;
            }
            
            if(enchantment == "")
            {
                removeNow = true;
            }
        }
    }
}
