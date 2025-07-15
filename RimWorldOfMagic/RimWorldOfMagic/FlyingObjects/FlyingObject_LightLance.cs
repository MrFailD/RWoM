using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class FlyingObject_LightLance : Projectile
    {

        private static readonly Color lightningColor = new Color(160f, 160f, 160f);
        private static readonly Material OrbMat = MaterialPool.MatFrom("Spells/deathbolt", false);

        protected new Vector3 origin;
        protected new Vector3 destination;

        private int age = -1;
        private float arcaneDmg = 1;
        private bool powered;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;

        private int pwrVal;
        private int verVal;
        private float radius = 1.4f;
        private int scanFrequency = 1;
        private float lightPotency = .5f;

        protected float speed = 30f;
        protected new int ticksToImpact;
        private bool impacted;

        protected Thing assignedTarget;
        protected Thing flyingThing;
        private Pawn pawn;
        private List<Pawn> filteredTargets = new List<Pawn>();

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion;

        private bool initialized = true;        

        protected new int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((origin - destination).magnitude / (speed / 100f));
                bool flag = num < 1;
                if (flag)
                {
                    num = 1;
                }
                return num;
            }
        }

        protected new IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(destination);
            }
        }

        public new Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (destination - origin) * (1f - (float)ticksToImpact / (float)StartingTicksToImpact);
                return origin + b + Vector3.up * def.Altitude;
            }
        }

        public new Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(destination - origin);
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return ExactPosition;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref pwrVal, "pwrVal", 0, false);
            Scribe_Values.Look<int>(ref verVal, "verVal", 0, false);
            Scribe_Values.Look<float>(ref radius, "radius", 1.4f, false);
            Scribe_Values.Look<bool>(ref damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref explosion, "explosion", false, false);
            Scribe_Values.Look<bool>(ref initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref impacted, "impacted", false, false);
            Scribe_Values.Look<bool>(ref powered, "powered", false, false);
            Scribe_References.Look<Thing>(ref assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Pawn>(ref pawn, "pawn", false);
            //Scribe_References.Look<Thing>(ref this.flyingThing, "flyingThing", false);
            Scribe_Deep.Look<Thing>(ref flyingThing, "flyingThing", new object[0]);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                GetFilteredList();
                CompAbilityUserMagic comp = pawn.GetCompAbilityUserMagic();
                if(comp != null)
                {
                    //pwrVal = comp.MagicData.MagicPowerSkill_LightLance.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_LightLance_pwr").level;
                    //verVal = comp.MagicData.MagicPowerSkill_LightLance.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_LightLance_ver").level;
                    pwrVal = TM_Calc.GetSkillPowerLevel(pawn, TorannMagicDefOf.TM_LightLance, true);
                    verVal = TM_Calc.GetSkillVersatilityLevel(pawn, TorannMagicDefOf.TM_LightLance, true);
                    radius += (verVal * .2f);
                    if (pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_LightCapacitanceHD))
                    {
                        HediffComp_LightCapacitance hd = pawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_LightCapacitanceHD).TryGetComp<HediffComp_LightCapacitance>();
                        lightPotency = hd.LightPotency;
                    }

                }
                for (int i = 0; i < 3; i++)
                {
                    Vector3 rndPos = pawn.DrawPos;
                    rndPos.x += Rand.Range(-.75f, .75f);
                    rndPos.z += Rand.Range(-.75f, .75f);
                    FleckMaker.Static(rndPos, pawn.Map, FleckDefOf.FireGlow, 1f);
                }
            }            
            flyingThing.ThingID += Rand.Range(0, 214).ToString();            
        }

        private void GetFilteredList()
        {
            filteredTargets = new List<Pawn>();
            filteredTargets.Clear();
            IEnumerable<Pawn> pList = from p in Map.mapPawns.AllPawnsSpawned
                                      where (!p.DestroyedOrNull() && p != pawn && (p.Position - pawn.Position).LengthHorizontal < ((destination - origin).magnitude * 1.5f))
                                      select p;
            filteredTargets = pList.ToList();
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.origin = origin;
            impactDamage = newDamageInfo;
            speed = def.projectile.speed;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                assignedTarget = targ.Thing;
            }
            destination = targ.Cell.ToVector3Shifted();
            ticksToImpact = StartingTicksToImpact;
            Initialize();
        }

        protected override void Tick()
        {
            //base.Tick();
            age++;
            if (ticksToImpact >= 0)
            {
                DrawEffects(ExactPosition, Map);
            }
            if(Find.TickManager.TicksGame % scanFrequency == 0)
            {
                DamageScan();
            }
            ticksToImpact--;            
            Position = ExactPosition.ToIntVec3();
            bool flag = !ExactPosition.InBoundsWithNullCheck(Map);
            if (flag)
            {
                ticksToImpact++;
                Destroy(DestroyMode.Vanish);
            }
            else
            {                                           
                bool flag2 = ticksToImpact <= 0 && !impacted;
                if (flag2)
                {
                    bool flag3 = DestinationCell.InBoundsWithNullCheck(Map);
                    if (flag3)
                    {
                        Position = DestinationCell;
                    }
                    ImpactSomething();
                }
            }
        }

        public void DamageScan()
        {
            if(filteredTargets == null || filteredTargets.Count <= 0)
            {
                GetFilteredList();
            }
            int num = filteredTargets.Count;
            List<Pawn> targets = new List<Pawn>();
            targets.Clear();
            for (int i = 0; i < num; i++)
            {
                if ((filteredTargets[i].DrawPos - ExactPosition).magnitude < radius)
                {
                    if (!(filteredTargets[i].Faction == Faction.OfPlayer && (filteredTargets[i].DrawPos - origin).magnitude < 2f))
                    {
                        targets.Add(filteredTargets[i]);
                    }
                }
            }
            if(targets.Count > 0)
            {
                for(int k =0; k < targets.Count; k++)
                {
                    TM_Action.DamageEntities(targets[k], null, (4 + (.6f*pwrVal)) * arcaneDmg * lightPotency, TMDamageDefOf.DamageDefOf.TM_BurningLight, pawn);
                }           
            }
        }

        public void DrawEffects(Vector3 effectVec, Map map)
        {
            //effectVec.x += Rand.Range(-0.4f, 0.4f);
            //effectVec.z += Rand.Range(-0.4f, 0.4f);
            //TM_MoteMaker.ThrowDiseaseMote(effectVec, map, 0.4f, 0.1f, .01f, 0.35f);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null && !impacted;
            if (flag)
            {
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                Comps_PostDraw();
            }
        }

        private void ImpactSomething()
        {
            bool flag = assignedTarget != null;
            if (flag)
            {
                Pawn pawn = assignedTarget as Pawn;
                bool flag2 = pawn != null && pawn.GetPosture() != PawnPosture.Standing && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f;
                if (flag2)
                {
                    Impact(null);
                }
                else
                {
                    Impact(assignedTarget);
                }
            }
            else
            {
                Impact(null);
            }
        }

        protected new void Impact(Thing hitThing)
        {
            Destroy(DestroyMode.Vanish);
        }        
    }
}
