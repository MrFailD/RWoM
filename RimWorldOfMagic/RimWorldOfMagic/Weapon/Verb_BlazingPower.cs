using Verse;
using RimWorld;
using TorannMagic.Golems;

namespace TorannMagic.Weapon
{
    public class Verb_BlazingPower : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
            if (comp != null && comp.IsMagicUser)
            {
                return base.TryCastShot();
            }
            else if (CasterPawn is TMHollowGolem)
            {
                return base.TryCastShot();
            }
            else
            {                
                MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, "Failed", -1);                
                return false;
            }          
            
        }
    }
}
