using RimWorld;
using Verse;

namespace TorannMagic.SihvRMagicScrollScribe
{
    public class CompUseEffect_WriteFullScript : CompUseEffect
    {

        public override void DoEffect(Pawn user)
        {
            ThingDef tempPod = null;
            IntVec3 currentPos = parent.PositionHeld;
            Map map = parent.Map;
            CompAbilityUserMagic comp = user.GetCompAbilityUserMagic();

            if (parent.def != null && comp != null)
            {
                if (user.IsSlave)
                {                    
                    Messages.Message("TM_SlaveScribeFail".Translate(
                        parent.def.label
                    ), MessageTypeDefOf.RejectInput);
                    tempPod = null;
                }
                else
                {
                    if (comp.customClass != null && comp.customClass.fullScript != null)
                    {
                        tempPod = comp.customClass.fullScript;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.InnerFire))
                    {
                        tempPod = ThingDef.Named("BookOfInnerFire");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost))
                    {
                        tempPod = ThingDef.Named("BookOfHeartOfFrost");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.StormBorn))
                    {
                        tempPod = ThingDef.Named("BookOfStormBorn");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.Arcanist))
                    {
                        tempPod = ThingDef.Named("BookOfArcanist");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.Paladin))
                    {
                        tempPod = ThingDef.Named("BookOfValiant");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.Summoner))
                    {
                        tempPod = ThingDef.Named("BookOfSummoner");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.Druid))
                    {
                        tempPod = ThingDef.Named("BookOfDruid");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Necromancer) || user.story.traits.HasTrait(TorannMagicDefOf.Lich)))
                    {
                        tempPod = ThingDef.Named("BookOfNecromancer");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.Priest))
                    {
                        tempPod = ThingDef.Named("BookOfPriest");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && user.story.traits.HasTrait(TorannMagicDefOf.TM_Bard))
                    {
                        tempPod = ThingDef.Named("BookOfBard");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Succubus) || user.story.traits.HasTrait(TorannMagicDefOf.Warlock)))
                    {
                        tempPod = ThingDef.Named("BookOfDemons");
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Geomancer)))
                    {
                        tempPod = TorannMagicDefOf.BookOfEarth;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Technomancer)))
                    {
                        tempPod = TorannMagicDefOf.BookOfMagitech;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.BloodMage)))
                    {
                        tempPod = TorannMagicDefOf.BookOfHemomancy;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Enchanter)))
                    {
                        tempPod = TorannMagicDefOf.BookOfEnchanter;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.Chronomancer)))
                    {
                        tempPod = TorannMagicDefOf.BookOfChronomancer;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.ChaosMage)))
                    {
                        tempPod = TorannMagicDefOf.BookOfChaos;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.TM_Brightmage)))
                    {
                        tempPod = TorannMagicDefOf.BookOfTheSun;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.TM_Shaman)))
                    {
                        tempPod = TorannMagicDefOf.BookOfShamanism;
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else if (parent.def != null && (user.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted) || user.story.traits.HasTrait(TorannMagicDefOf.TM_Wanderer) || TM_Calc.IsMagicUser(user)))
                    {
                        tempPod = TM_Data.MageBookList().RandomElement();
                        parent.SplitOff(1).Destroy(DestroyMode.Vanish);
                    }
                    else
                    {
                        Messages.Message("NotGiftedPawn".Translate(
                                user.LabelShort
                            ), MessageTypeDefOf.RejectInput);
                    }
                }
            }
            if (tempPod != null)
            {                
                SihvSpawnThings.SpawnThingDefOfCountAt(tempPod, 1, new TargetInfo(currentPos, map));
            }

        }
    }
}
