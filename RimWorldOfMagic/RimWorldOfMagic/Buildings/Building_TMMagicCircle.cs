using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using System.Diagnostics;
using UnityEngine;
using RimWorld;
using AbilityUser;


namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class Building_TMMagicCircle : Building_TMMagicCircleBase
    {
        private float matMagnitude = 5.2f;

        public override IntVec3 GetCircleCenter
        {
            get
            {
                IntVec3 center = InteractionCell;
                if (Rotation == Rot4.North)
                {
                    center.z -= 2;
                }
                else if (Rotation == Rot4.South)
                {
                    center.z += 2;
                }
                else if (Rotation == Rot4.West)
                {
                    center.x += 2;
                }
                else
                {
                    center.x -= 2;
                }
                return center;
            }
        }

        public Job ActiveJob
        {
            get
            {
                if(MageList.Count > 0)
                {
                    return MageList[0].CurJob;
                }
                else
                {
                    return null;
                }
            }
        }

        public override float SpellSuccessModifier
        {
            get
            {
                if(Stuff != null)
                {
                    if (Stuff == ThingDef.Named("Jade"))
                    {
                        return .3f;
                    }
                    else if (Stuff == ThingDef.Named("Uranium"))
                    {
                        return .2f;
                    }      
                    else if(Stuff == TorannMagicDefOf.TM_Arcalleum)
                    {
                        return .15f;
                    }
                }
                return 0f;
            }
        }

        public override float MaterialCostModifier
        {
            get
            {
                if (Stuff != null)
                {
                    if (Stuff == ThingDef.Named("Silver"))
                    {
                        return .25f;
                    }
                    else if (Stuff == ThingDef.Named("Gold"))
                    {
                        return .15f;
                    }
                }
                return 0f;
            }
        }

        public override float DurationModifier
        {
            get
            {
                if (Stuff != null)
                {
                    if (Stuff == ThingDef.Named("Gold"))
                    {
                        return .2f;
                    }
                    else if(Stuff == ThingDef.Named("Uranium"))
                    {
                        return .2f;
                    }
                    else if (Stuff == ThingDefOf.Plasteel)
                    {
                        return .10f;
                    }
                }
                return 0f;
            }
        }

        public override float ManaCostModifer
        {
            get
            {
                if (Stuff != null)
                {
                    if (Stuff == TorannMagicDefOf.TM_Arcalleum)
                    {
                        return .25f;
                    }
                }
                return 0f;
            }
        }

        public override float PointModifer
        {
            get
            {
                if (Stuff != null)
                {
                    if (Stuff == ThingDef.Named("Gold"))
                    {
                        return .2f;
                    }
                    else if(Stuff == ThingDefOf.Plasteel)
                    {
                        return .15f;
                    }
                }
                return 0f;
            }
        }

        public override void DoActiveEffecter()
        {
            Effecter CircleED = TorannMagicDefOf.TM_MagicCircleED.Spawn();
            CircleED.Trigger(new TargetInfo(GetCircleCenter, Map, false), new TargetInfo(GetCircleCenter, Map, false));
            CircleED.Cleanup();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {           
            if (IsActive)
            {
                Vector3 vector = base.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.Blueprint);
                Vector3 s = new Vector3(matMagnitude, 2*matMagnitude, matMagnitude);
                Quaternion quaternion = Quaternion.AngleAxis(Rotation.AsAngle, Vector3.up);
                Matrix4x4 matrix = default(Matrix4x4);
                float angle = ExactRotation;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                if (Rotation == Rot4.North)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mc_north, 0);
                }
                else if (Rotation == Rot4.South)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mc_south, 0);
                }
                else if (Rotation == Rot4.East)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mc_east, 0);
                }
                else if (Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mc_west, 0);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mc_north, 0);
                }
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        public override IntVec3 GetMageIndexPosition(List<Pawn> allMages, Pawn mage)
        {
            IntVec3 magePosition = default(IntVec3);
            for (int i = 0; i < allMages.Count; i++)
            {
                if (allMages[i] == mage)
                {
                    IntVec3 ic = InteractionCell;
                    if (i == 0)
                    {
                        //always interaction cell
                    }
                    else if (i == 1)
                    {
                        if (Rotation == Rot4.North)
                        {
                            ic.x -= 2;
                            ic.z -= 3;
                        }
                        else if (Rotation == Rot4.South)
                        {
                            ic.x += 2;
                            ic.z += 3;
                        }
                        else if (Rotation == Rot4.West)
                        {
                            ic.x += 3;
                            ic.z -= 2;
                        }
                        else
                        {
                            ic.x -= 3;
                            ic.z += 2;
                        }
                    }
                    else if (i == 2)
                    {
                        if (Rotation == Rot4.North)
                        {
                            ic.x += 2;
                            ic.z -= 3;
                        }
                        else if (Rotation == Rot4.South)
                        {
                            ic.x -= 2;
                            ic.z += 3;
                        }
                        else if (Rotation == Rot4.West)
                        {
                            ic.x += 3;
                            ic.z += 2;
                        }
                        else
                        {
                            ic.x -= 3;
                            ic.z -= 2;
                        }
                    }
                    else if (i == 3)
                    {
                        if (Rotation == Rot4.North)
                        {
                            ic.x += 0;
                            ic.z -= 4;
                        }
                        else if (Rotation == Rot4.South)
                        {
                            ic.x -= 0;
                            ic.z += 4;
                        }
                        else if (Rotation == Rot4.West)
                        {
                            ic.x += 4;
                            ic.z += 0;
                        }
                        else
                        {
                            ic.x -= 4;
                            ic.z -= 0;
                        }
                    }
                    else if (i == 4)
                    {
                        if (Rotation == Rot4.North)
                        {
                            ic.x -= 2;
                            ic.z -= 1;
                        }
                        else if (Rotation == Rot4.South)
                        {
                            ic.x += 2;
                            ic.z += 1;
                        }
                        else if (Rotation == Rot4.West)
                        {
                            ic.x += 1;
                            ic.z -= 2;
                        }
                        else
                        {
                            ic.x -= 1;
                            ic.z += 2;
                        }
                    }
                    else if (i == 5)
                    {
                        if (Rotation == Rot4.North)
                        {
                            ic.x += 2;
                            ic.z -= 1;
                        }
                        else if (Rotation == Rot4.South)
                        {
                            ic.x -= 2;
                            ic.z += 1;
                        }
                        else if (Rotation == Rot4.West)
                        {
                            ic.x += 1;
                            ic.z += 2;
                        }
                        else
                        {
                            ic.x -= 1;
                            ic.z -= 2;
                        }
                    }
                    else
                    {
                        ic = GetRandomStaticIndexPositionFor(mage);
                    }
                    magePosition = ic;
                }
            }
            return magePosition;
        }
    }
}
