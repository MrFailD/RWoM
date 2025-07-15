using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    public class MagicMapComponent: MapComponent
    {
        public float windSpeed;
        public int windSpeedEndTick;
        public bool allowAllIncidents;
        public int weatherControlExpiration;

        public MagicMapComponent(Map map): base(map)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref windSpeed, "windSpeed", 0f, false);
            Scribe_Values.Look<int>(ref windSpeedEndTick, "windSpeedEndTick", 0, false);
            Scribe_Values.Look<bool>(ref allowAllIncidents, "allowAllIncidents", false, false);
            Scribe_Values.Look<int>(ref weatherControlExpiration, "weatherControlExpiration", 0, false);
        }

        public void ApplyComponentConditions(string condition, float value = 0f)
        {
            if(condition == "NameOfTheWind")
            {
                windSpeed = 2f;
                windSpeedEndTick = Find.TickManager.TicksGame + Rand.Range(160000, 240000);
            }
            if(condition == "ArcaneInspiration")
            {
                List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned.InRandomOrder().ToList();
                int count = Mathf.Clamp(Rand.RangeInclusive(1, 3), 1, colonists.Count);
                for(int i =0; i < count; i++)
                {
                    InspirationDef id = TM_Calc.GetRandomAvailableInspirationDef(colonists[i]);
                    colonists[i].mindState.inspirationHandler.TryStartInspiration(id);
                }
            }
            if(condition == "AllowAllIncidents")
            {
                allowAllIncidents = true;
            }
        }
    }
}
