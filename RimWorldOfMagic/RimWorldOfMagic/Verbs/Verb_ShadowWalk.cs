using RimWorld;
using Verse;
using AbilityUser;
using TorannMagic.Weapon;
using UnityEngine;

namespace TorannMagic
{
    internal class Verb_ShadowWalk : Verb_UseAbility  
    {

        private bool validTarg;
        private int verVal;
        private int pwrVal;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {            
            if ( targ.IsValid && targ.CenterVector3.InBoundsWithNullCheck(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < verbProps.range)
                {
                    //out of range
                    validTarg = true;
                }
                else
                {
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        protected override bool TryCastShot()
        {
            bool result = false;
            bool flag = false;
            
            if (currentTarget != null && base.CasterPawn != null)
            {
                IntVec3 arg_29_0 = currentTarget.Cell;
                Vector3 vector = currentTarget.CenterVector3;
                flag = currentTarget.Cell.IsValid && vector.InBoundsWithNullCheck(base.CasterPawn.Map) && currentTarget.Thing != null && currentTarget.Thing is Pawn;
            }

            if (flag)
            {
                Pawn p = CasterPawn;
                Pawn targetPawn = currentTarget.Thing as Pawn;
                Map map = CasterPawn.Map;
                IntVec3 cell = CasterPawn.Position;
                bool draftFlag = CasterPawn.Drafted;
                CompAbilityUserMagic comp = p.GetCompAbilityUserMagic();
                if (comp != null)
                {
                    pwrVal = comp.MagicData.MagicPowerSkill_ShadowWalk.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ShadowWalk_pwr").level;
                    verVal = comp.MagicData.MagicPowerSkill_ShadowWalk.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_ShadowWalk_ver").level;
                }
                try
                {
                    if (CasterPawn.IsColonist)
                    {
                        ModOptions.Constants.SetPawnInFlight(true);                            
                        CasterPawn.DeSpawn();
                        GenSpawn.Spawn(p, currentTarget.Cell, map);
                        p.drafter.Drafted = draftFlag;
                        if (ModOptions.Settings.Instance.cameraSnap)
                        {
                            CameraJumper.TryJumpAndSelect(p);
                        }
                        ModOptions.Constants.SetPawnInFlight(false);                        
                    }
                    else
                    {
                        ModOptions.Constants.SetPawnInFlight(true);
                        CasterPawn.DeSpawn();
                        GenSpawn.Spawn(p, currentTarget.Cell, map);
                        ModOptions.Constants.SetPawnInFlight(false);
                    }
                    if (pwrVal > 0)
                    {
                        HealthUtility.AdjustSeverity(p, TorannMagicDefOf.TM_ShadowCloakHD, .5f);
                        HediffComp_Disappears hdComp = p.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_ShadowCloakHD).TryGetComp<HediffComp_Disappears>();
                        if (hdComp != null)
                        {
                            hdComp.ticksToDisappear = 60 + (60 * pwrVal);
                        }
                    }
                    if (verVal > 0)
                    {
                        TM_Action.DoAction_HealPawn(p, p, 1+verVal, 6 + verVal);
                        if (targetPawn.Faction != null && targetPawn.Faction == p.Faction)
                        {
                            if (verVal > 1)
                            {
                                TM_Action.DoAction_HealPawn(p, targetPawn, verVal, 4 + verVal);
                            }
                            if (verVal > 2)
                            {
                                HealthUtility.AdjustSeverity(targetPawn, TorannMagicDefOf.TM_ShadowCloakHD, .5f);
                                HediffComp_Disappears hdComp = targetPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_ShadowCloakHD).TryGetComp<HediffComp_Disappears>();
                                if (hdComp != null)
                                {
                                    hdComp.ticksToDisappear = 180;
                                }
                                ThingDef fog = TorannMagicDefOf.Fog_Shadows;
                                fog.gas.expireSeconds.min = 4;
                                fog.gas.expireSeconds.max = 4;
                                ExplosionHelper.Explode(p.Position, p.Map, 2, TMDamageDefOf.DamageDefOf.TM_Toxin, caster, 0, 0, TMDamageDefOf.DamageDefOf.TM_Toxin.soundExplosion, null, null, null, fog, 1f, 1, null, false, null, 0f, 0, 0.0f, false);

                            }
                        }
                    }
                    for(int i =0; i < 6; i++)
                    {
                        Vector3 rndPos = p.DrawPos;
                        rndPos.x += Rand.Range(-1.5f, 1.5f);
                        rndPos.z += Rand.Range(-1.5f, 1.5f);
                        TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_ShadowCloud, rndPos, p.Map, Rand.Range(.8f, 1.2f), .6f, .05f, Rand.Range(.7f, 1f), Rand.Range(-40, 40), Rand.Range(0, 1f), Rand.Range(0, 360), Rand.Range(0, 360));
                    }
                }
                catch
                {
                    if(!CasterPawn.Spawned)
                    {
                        GenSpawn.Spawn(p, cell, map);
                        Log.Message("Exception occured when trying to blink - recovered pawn at position ability was used from.");
                    }
                }

                result = true;
            }
            else
            {

                Messages.Message("InvalidTargetLocation".Translate(), MessageTypeDefOf.RejectInput);
            }
            burstShotsLeft = 0;
            return result;
        }
    }
}
