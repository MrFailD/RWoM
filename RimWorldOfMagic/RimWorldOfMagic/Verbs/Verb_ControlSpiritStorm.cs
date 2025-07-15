using RimWorld;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Verse.Sound;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class Verb_ControlSpiritStorm : Verb_BLOS 
    {
        public Map map;
        protected override bool TryCastShot()
        {
            Pawn p = CasterPawn;
            map = CasterPawn.Map;
            CompAbilityUserMagic comp = CasterPawn.GetCompAbilityUserMagic();
            List<FlyingObject_SpiritStorm> storms = GetActiveStorms();
            if(storms != null)
            {
                foreach(FlyingObject_SpiritStorm ss in storms)
                {
                    ss.PlayerTargetSet = true;
                    ss.ManualDestination = currentTarget.CenterVector3;
                }
            }
            burstShotsLeft = 0;
            return true;
        }


        private List<FlyingObject_SpiritStorm> GetActiveStorms()
        {
            if (map != null)
            {
                IEnumerable<FlyingObject_SpiritStorm> storm = from def in map.listerThings.AllThings
                                                              where (def.Spawned && def is FlyingObject_SpiritStorm)
                                                              select def as FlyingObject_SpiritStorm;
                if(storm != null && storm.Count() > 0)
                {
                    return storm.ToList(); ;
                }
            }
            return null;                                 
        }
    }
}
