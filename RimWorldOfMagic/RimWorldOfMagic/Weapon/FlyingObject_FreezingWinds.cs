using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using AbilityUser;

namespace TorannMagic.Weapon
{
    [StaticConstructorOnStartup]
    public class FlyingObject_FreezingWinds : FlyingObject_Advanced
    {
        private int actionTick = 10;
        private int age;
        private int rotationOffset;
        private float rotationRate = 1f;
        private float moteAngle;
        private Thing hitThing;

        public override void PreInitialize()
        {
            actionTick = Rand.Range(0, 15);
            rotationOffset = Rand.RangeInclusive(0, 360);
            if (Rand.Chance(.5f))
            {
                rotationRate = Rand.Range(-15f, -8f);
            }
            else
            {
                rotationRate = Rand.Range(8, 15f);
            }
            moteAngle = (Quaternion.AngleAxis(90, Vector3.up) * travelVector).ToAngleFlat();
            base.PreInitialize();
        }

        public override void PreTick()
        {
            age++;
            if(age >= actionTick)
            {
                actionTick = age + Rand.Range(15, 25);                
                DamageThingsAtPosition();
                WeatherBuildupUtility.AddSnowRadial(ExactPosition.ToIntVec3(), Map, .4f, .2f);                
            }           
        }

        public override void PostTick()
        {
            if (hitThing != null)
            {
                Impact(hitThing);
            }
        }

        public void DamageThingsAtPosition()
        {
            IntVec3 curCell = ExactPosition.ToIntVec3();
            List<Thing> hitList = new List<Thing>();
            hitList = curCell.GetThingList(Map);
            List<Fire> destroyFires = new List<Fire>();
            destroyFires.Clear();            
            for (int j = 0; j < hitList.Count; j++)
            {
                if (hitList[j] is Pawn && hitList[j] != launcher)
                {
                    DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, def.projectile.GetDamageAmount(this), 1, -1, launcher);                    
                    hitList[j].TakeDamage(dinfo);
                    hitThing = hitList[j];
                }
                if(hitList[j] is Fire)
                {
                    Fire hitFire = hitList[j] as Fire;
                    hitFire.fireSize -= .25f;
                    if(hitFire.fireSize <= .1f)
                    {
                        destroyFires.Add(hitFire);
                    }
                }
            }
            for(int i = 0; i < destroyFires.Count; i++)
            {
                destroyFires[i].Destroy(DestroyMode.Vanish);
            }            
        }

        public override void DrawEffects(Vector3 effectVec)
        {
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);            
            TM_MoteMaker.ThrowGenericMote(moteDef, effectVec, Map, Rand.Range(.15f, .45f), Rand.Range(.05f, .1f), .03f, Rand.Range(.2f, .3f), Rand.Range(-200, 200), Rand.Range(1f, 6f), moteAngle + Rand.Range(-20,20), Rand.Range(0, 360));
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = flyingThing != null;
            if (flag)
            {                
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, DrawRotation, flyingThing.def.DrawMatSingle, 0);                
            }
            else
            {                
                Graphics.DrawMesh(MeshPool.plane10, DrawPos, DrawRotation, def.DrawMatSingle, 0);
            }
            Comps_PostDraw();
        }

        public Quaternion DrawRotation
        {
            get
            {                
                return Quaternion.LookRotation(Quaternion.AngleAxis((age * rotationRate) + rotationOffset, Vector3.up) * travelVector);
            }
        }
    }
}
