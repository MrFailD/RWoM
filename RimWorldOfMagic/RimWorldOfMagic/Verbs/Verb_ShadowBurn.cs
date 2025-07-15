using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;


namespace TorannMagic
{

    public class Verb_ShadowBurn : Verb_MeleeAttack
    {

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            DamageInfo dinfo = new DamageInfo(maneuver.verb.meleeDamageDef, (int)(tool.power), 2, (float)-1, CasterPawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            damageResult.hitThing = target.Thing;
            damageResult.totalDamageDealt = Mathf.Min((float)target.Thing.HitPoints, dinfo.Amount);
            float angle = (Quaternion.AngleAxis(90, Vector3.up)*GetVector(CasterPawn.Position, target.Thing.Position)).ToAngleFlat();
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Shadow, CasterPawn.DrawPos, target.Thing.Map, Rand.Range(.8f, 1f), .15f, .05f, .1f, 0, 5, (angle + Rand.Range(-20,20)), Rand.Range(0,360));
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Shadow, CasterPawn.DrawPos, target.Thing.Map, Rand.Range(.8f, 1f), .15f, .05f, .1f, 0, 5, (angle + Rand.Range(-20, 20)), Rand.Range(0, 360));
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Shadow, CasterPawn.DrawPos, target.Thing.Map, Rand.Range(.8f, 1f), .15f, .05f, .1f, 0, 5, (angle + Rand.Range(-20, 20)), Rand.Range(0, 360));
            target.Thing.TakeDamage(dinfo);
            return damageResult;  
        }

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }
    }
}
