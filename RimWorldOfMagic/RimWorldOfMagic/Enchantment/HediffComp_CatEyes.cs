using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TorannMagic.Enchantment
{
    public class HediffComp_CatEyes : HediffComp_EnchantedItem
    {
        private CompProperties_Glower gProps = new CompProperties_Glower();
        private CompGlower glower = new CompGlower();
        private IntVec3 oldPos = default(IntVec3);

        public ColorInt glowColor = new ColorInt(0, 205, 102, 1);

        public override void CompExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && oldPos != default(IntVec3))
            {
                Pawn.Map.mapDrawer.MapMeshDirty(oldPos, MapMeshFlagDefOf.Things);
                Pawn.Map.glowGrid.DeRegisterGlower(glower);
                glower = null;
            }
            base.CompExposeData();
            
        }

        public override void PostInitialize()
        {
            glower = new CompGlower();
            hediffActionRate = 10;
            gProps.glowColor = glowColor;
            gProps.glowRadius = 1.4f;
            glower.parent = Pawn;
            glower.Initialize(gProps);
        }

        public override void HediffActionTick()
        {
            if (glower != null && glower.parent != null)
            {
                if (Pawn != null && Pawn.Map != null && Pawn.Position != oldPos)
                {
                    if (oldPos != default(IntVec3))
                    {
                        Pawn.Map.mapDrawer.MapMeshDirty(oldPos, MapMeshFlagDefOf.Things);
                        Pawn.Map.glowGrid.DeRegisterGlower(glower);
                    }
                    if (Pawn.Map.skyManager.CurSkyGlow < 0.4f)
                    {
                        Pawn.Map.mapDrawer.MapMeshDirty(Pawn.Position, MapMeshFlagDefOf.Things);
                        Pawn.Map.glowGrid.RegisterGlower(glower);
                        oldPos = Pawn.Position;
                    }
                }
            }
            else
            {
                PostInitialize();
            }

        }

        public override void CompPostPostRemoved()
        {
            if (oldPos != default(IntVec3))
            {
                Pawn.Map.mapDrawer.MapMeshDirty(oldPos, MapMeshFlagDefOf.Things);
                Pawn.Map.glowGrid.DeRegisterGlower(glower);
            }
            base.CompPostPostRemoved();
        }
    }
}
