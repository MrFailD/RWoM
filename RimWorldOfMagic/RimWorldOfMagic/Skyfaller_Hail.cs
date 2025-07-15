using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace TorannMagic
{
    public class Skyfaller_Hail : Skyfaller
    {
        protected override void HitRoof()
        {
            if (def.skyfaller.hitRoof)
            {
                CellRect cr = this.OccupiedRect();
                if (cr.Cells.Any((IntVec3 x) => x.Roofed(Map)))
                {
                    RoofDef roof = cr.Cells.First((IntVec3 x) => x.Roofed(Map)).GetRoof(Map);
                    if (!roof.soundPunchThrough.NullOrUndefined())
                    {
                        roof.soundPunchThrough.PlayOneShot(new TargetInfo(Position, Map));
                    }
                    CellRect cellRect = this.OccupiedRect();
                    for (int i = 0; i < cellRect.Area * def.skyfaller.motesPerCell; i++)
                    {
                        FleckMaker.ThrowDustPuff(cellRect.RandomVector3, Map, 2f);
                    }
                    Destroy();
                }
            }
        }
    }
}
