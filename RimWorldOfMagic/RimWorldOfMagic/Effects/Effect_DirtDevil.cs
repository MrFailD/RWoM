using Verse;
using AbilityUser;
using System.Collections.Generic;
using RimWorld;

namespace TorannMagic
{    
    public class Effect_DirtDevil : Verb_UseAbility
    {
        private bool validTarg;

        public virtual void Effect()
        {
            LocalTargetInfo t = TargetsAoE[0];
            bool flag = t.Cell != default(IntVec3);
            if (flag)
            {
                Thing dirtDevil = new Thing();
                dirtDevil.def = TorannMagicDefOf.FlyingObject_DirtDevil;
                Pawn casterPawn = base.CasterPawn;
                FlyingObject_DirtDevil flyingObject = (FlyingObject_DirtDevil)GenSpawn.Spawn(ThingDef.Named("FlyingObject_DirtDevil"), CasterPawn.Position, CasterPawn.Map);
                flyingObject.Launch(CasterPawn, t.Cell, dirtDevil);
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult)
            {
                Effect();
                outResult = true;
            }
            outResult = inResult;
        }
    }    
}
