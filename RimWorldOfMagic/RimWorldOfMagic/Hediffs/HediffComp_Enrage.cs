using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Enrage : HediffComp
    {
        public bool consumeJoy;
        public float reductionFactor = 1f;
        private int lastDamageDealt;
        private bool initialized;

        //unsaved
        private float reductionAmount = .04f;
        private bool shouldRemove;

        public override void CompExposeData()
        {
            Scribe_Values.Look<bool>(ref consumeJoy, "consumeJoy", false);
            Scribe_Values.Look<float>(ref reductionFactor, "reductionFactor", 1f);
            base.CompExposeData();
        }
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
            if (spawned)
            {
                if(Pawn.records != null)
                {
                    lastDamageDealt = Pawn.records.GetAsInt(RecordDefOf.DamageDealt);
                }
            }
        }


        public override bool CompShouldRemove => base.CompShouldRemove || shouldRemove;

        public override void CompPostTick(ref float severityAdjustment)
        {
            bool usedEmotions = false;
            base.CompPostTick(ref severityAdjustment);
            bool flag = Pawn.DestroyedOrNull();
            if (!flag)
            {
                if (Pawn.Spawned && Pawn.needs != null)
                {
                    if(!initialized)
                    {
                        initialized = true;
                        Initialize();                        
                    }
                    if(Find.TickManager.TicksGame % 8 == 0)
                    {
                        DrawEffects();
                    }
                    if (Find.TickManager.TicksGame % 155 == 0)
                    {
                        float tickCost = (reductionAmount * reductionFactor);
                        float moodGain = 0f;
                        if (Pawn.records != null)
                        {
                            int currentDamage = Pawn.records.GetAsInt(RecordDefOf.DamageDealt);
                            int damageDiff = Mathf.Clamp(currentDamage - lastDamageDealt, 0, 100);
                            moodGain = Mathf.Clamp(.001f * damageDiff, 0f, 1f);
                            lastDamageDealt = currentDamage;
                        }

                        for (int i = 0; i < Pawn.needs.AllNeeds.Count; i++)
                        {
                            Need n = Pawn.needs.AllNeeds[i];
                            if(consumeJoy && n.def == TorannMagicDefOf.Joy && n.CurLevel >= tickCost)
                            {
                                n.CurLevel -= tickCost;
                                usedEmotions = true;
                                break;
                            }
                            if(n.def.defName == "Mood")
                            {
                                if(moodGain != 0f)
                                {
                                    n.CurLevel += moodGain;
                                }
                                if (n.CurLevel >= tickCost)
                                {
                                    n.CurLevel -= tickCost;
                                    usedEmotions = true;
                                    break;
                                }
                            }
                        }
                        if (!usedEmotions)
                        {
                            if (parent.Severity >= tickCost)
                            {
                                severityAdjustment = (-1f * tickCost);
                            }
                            else
                            {
                                shouldRemove = true;
                            }
                        }
                    }
                    if(Pawn.Dead || Pawn.Downed)
                    {
                        shouldRemove = true;
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            Need n = Pawn.needs.mood;
            if(n != null && n.CurLevel < .4f)
            {
                n.CurLevel = .4f;
            }
            base.CompPostPostRemoved();
        }

        public void DrawEffects()
        {
            Vector3 headOffset = Pawn.DrawPos;
            headOffset.z += .4f;
            float throwAngle = Rand.Range(-20, 20);
            TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_BloodMist, headOffset, Pawn.Map, Rand.Range(.4f, .6f), .12f, .01f, .1f, 0, .5f, throwAngle, Rand.Range(0, 360));
            //if (this.Pawn.Rotation == Rot4.East)
            //{
                
            //}
            //else if (this.Pawn.Rotation == Rot4.West)
            //{

            //    headOffset.z += .65f * sizeOffset;
            //    headOffset.x += -.4f * sizeOffset;
            //    float throwAngle = Rand.Range(5, 20);
            //    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, this.Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, throwAngle, Rand.Range(0, 360));
            //}
            //else if (this.Pawn.Rotation == Rot4.South)
            //{
            //    headOffset.z += .75f * sizeOffset;
            //    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, this.Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, Rand.Range(-30, 30), Rand.Range(0, 360));
            //}
            //else
            //{
            //    headOffset.z += .75f * sizeOffset;
            //    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Demon_Flame, headOffset, this.Pawn.Map, Rand.Range(.5f, .8f) * sizeOffset, Rand.Range(.12f, .18f), .01f, Rand.Range(.1f, .15f), 0, Rand.Range(2.4f, 2.8f) * sizeOffset, Rand.Range(-30, 30), Rand.Range(0, 360));
            //}
        }

    }
}
