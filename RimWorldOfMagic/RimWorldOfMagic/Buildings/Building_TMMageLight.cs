using UnityEngine;
using Verse;

namespace TorannMagic.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_TMMageLight : Building
    {

        private static readonly Material sunlightMat_1 = MaterialPool.MatFrom("Other/sunlight1", false);
        private static readonly Material sunlightMat_2 = MaterialPool.MatFrom("Other/sunlight2", false);
        private static readonly Material sunlightMat_3 = MaterialPool.MatFrom("Other/sunlight3", false);

        private int matRng = 0;
        private float matMagnitude = 1;
        private bool objectFloatingDown;
        private Vector3 objectPosition = default(Vector3);
        private float objectOffset = .5f;

        private bool initialized;
                
        protected override void Tick()
        {
            if(!initialized)
            {
                objectPosition = DrawPos;
                initialized = true;
            }
            base.Tick();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (objectFloatingDown)
            {
                if (objectOffset < .35f)
                {
                    objectFloatingDown = false;
                }
                objectOffset -= .001f;
            }
            else
            {
                if (objectOffset > .6f)
                {
                    objectFloatingDown = true;
                }
                objectOffset += .001f;
            }

            objectPosition = DrawPos;
            objectPosition.z += objectOffset;
            objectPosition.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float angle = Rand.Range(0, 360);
            Vector3 s = new Vector3(.35f, 1f, .35f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(objectPosition, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mageLightMat, 0);
        }
    }
}
