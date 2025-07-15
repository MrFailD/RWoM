using RimWorld;
using System;
using Verse;
using AbilityUser;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TorannMagic
{
    internal class Verb_Blink : Verb_BLOS 
    {
        private bool arg_41_0;
        private bool arg_42_0;
        private bool validTarg = false;
        

        protected override bool TryCastShot()
        {
            bool result = false;
            bool arg_40_0;
            
            if (currentTarget != null && base.CasterPawn != null)
            {
                IntVec3 arg_29_0 = currentTarget.Cell;
                Vector3 vector = currentTarget.CenterVector3;
                arg_40_0 = currentTarget.Cell.IsValid;
                arg_41_0 = vector.InBoundsWithNullCheck(base.CasterPawn.Map);
                arg_42_0 = true; // vector.ToIntVec3().Standable(base.CasterPawn.Map);
            }
            else
            {
                arg_40_0 = false;
            }
            bool flag = arg_40_0;
            bool flag2 = arg_41_0;
            bool flag3 = arg_42_0;
            if (flag)
            {
                if (flag2 & flag3)
                {
                    Pawn p = CasterPawn;
                    Map map = CasterPawn.Map;
                    IntVec3 cell = CasterPawn.Position;
                    bool draftFlag = CasterPawn.Drafted;
                    try
                    {
                        if (CasterPawn.IsColonist)
                        {
                            ModOptions.Constants.SetPawnInFlight(true);
                            ThingSelectionUtility.SelectNextColonist();
                            CasterPawn.DeSpawn();
                            //p.SetPositionDirect(this.currentTarget.Cell);
                            GenSpawn.Spawn(p, currentTarget.Cell, map);
                            p.drafter.Drafted = draftFlag;
                            if (base.CasterPawn.GetCompAbilityUserMagic().MagicData.MagicPowerSkill_Cantrips.FirstOrDefault((MagicPowerSkill x) => x.label == "TM_Cantrips_ver").level >= 1)
                            {
                                List<Pawn> eList = TM_Calc.FindPawnsNearTarget(p, 2, currentTarget.Cell, true);
                                if(eList != null && eList.Count > 0)
                                {
                                    for(int i = 0; i < eList.Count; i++)
                                    {
                                        Pawn e = eList[i];
                                        TM_Action.DamageEntities(e, null, 5f, DamageDefOf.Stun, p);
                                    }
                                }
                            }
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
                    }
                    catch
                    {
                        if(!CasterPawn.Spawned)
                        {
                            GenSpawn.Spawn(p, cell, map);
                            Log.Message("Exception occured when trying to blink - recovered pawn at position ability was used from.");
                        }
                    }
                    //this.Ability.PostAbilityAttempt();
                    //this.CasterPawn.SetPositionDirect(this.currentTarget.Cell);
                    //base.CasterPawn.SetPositionDirect(this.currentTarget.Cell);
                    //this.CasterPawn.pather.ResetToCurrentPosition();
                    result = true;
                }
                else
                {
                    
                    Messages.Message("InvalidTargetLocation".Translate(), MessageTypeDefOf.RejectInput);
                }
            }
            else
            {
                Log.Warning("failed to TryCastShot");
            }
            burstShotsLeft = 0;
            //this.ability.TicksUntilCasting = (int)base.UseAbilityProps.SecondsToRecharge * 60;
            return result;
        }
    }
}
