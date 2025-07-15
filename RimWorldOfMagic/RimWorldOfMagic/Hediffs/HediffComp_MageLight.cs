using RimWorld;
using Verse;
using System.Linq;

namespace TorannMagic
{
    public class HediffComp_MageLight : HediffComp
    {
        private bool initializing = true;
        private CompProperties_Glower gProps = new CompProperties_Glower();
        private CompGlower glower = new CompGlower();
        private IntVec3 oldPos = default(IntVec3);
        private CompAbilityUserMagic comp;
        private bool canCastLightning;
        private int nextLightningTick;

        public ColorInt glowColor = new ColorInt(255, 255, 204, 1);

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
            if (spawned && Pawn.Map != null)
            {
                FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
                glower = new CompGlower();
                gProps.glowColor = glowColor;
                gProps.glowRadius = 7f;
                glower.parent = Pawn;
                glower.Initialize(gProps);
                comp = Pawn.GetCompAbilityUserMagic();
                nextLightningTick = Find.TickManager.TicksGame + Rand.Range(400, 800);
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

            if (Find.TickManager.TicksGame >= nextLightningTick && comp != null)
            {
                if (canCastLightning || comp?.MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_ver").level >= 13)
                {
                    canCastLightning = true;
                    nextLightningTick = Find.TickManager.TicksGame + Rand.Range(400, 800);
                    if(Pawn.Drafted && !Pawn.Downed && Pawn.Map != null && Pawn.Spawned)
                    {
                        Pawn e = TM_Calc.FindNearbyEnemy(Pawn.Position, Pawn.Map, Pawn.Faction, 24, 0);
                        if (e != null && TM_Calc.HasLoSFromTo(Pawn.Position, e, Pawn, 0, 25))
                        {
                            Projectile lightning = ThingMaker.MakeThing(ThingDef.Named("Laser_LightningBolt"), null) as Projectile;
                            TM_CopyAndLaunchProjectile.CopyAndLaunchProjectile(lightning, Pawn, e, e, ProjectileHitFlags.All, null);
                        }
                    }
                }
            }

            if (glower != null && glower.parent != null && comp != null)
            {
                if (oldPos != Pawn.Position)
                {
                    if (Pawn != null && Pawn.Map != null)
                    {
                        if (oldPos != default(IntVec3))
                        {
                            Pawn.Map.mapDrawer.MapMeshDirty(oldPos, MapMeshFlagDefOf.Things);
                            Pawn.Map.glowGrid.DeRegisterGlower(glower);
                        }
                        if ((Pawn.Map.skyManager.CurSkyGlow < 0.7f || Pawn.Position.Roofed(Pawn.Map)) && !comp.mageLightSet)
                        {
                            Pawn.Map.mapDrawer.MapMeshDirty(Pawn.Position, MapMeshFlagDefOf.Things);
                            oldPos = Pawn.Position;
                            Pawn.Map.glowGrid.RegisterGlower(glower);
                        }
                    }
                }
            }
            else
            {
                initializing = false;
                Initialize();
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