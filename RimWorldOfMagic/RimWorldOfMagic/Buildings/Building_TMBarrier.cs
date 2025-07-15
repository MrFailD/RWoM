using UnityEngine;
using Verse;

namespace TorannMagic.Buildings
{
    public class Building_TMBarrier : Building
    {

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 vector = base.DrawPos;
            float size = Rand.Range(1.60f, 1.70f);
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Vector3 s = new Vector3(size,size, size);
            Matrix4x4 matrix = default(Matrix4x4);
            float angle = Rand.Range(0, 360);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TM_MatPool.barrier_Mote_Mat, 0);            
        }
    }
}
