using System;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class TM_MeshBolt : Thing
    {
        public static readonly Material Lightning = MatLoader.LoadMat("Weather/LightningBolt", -1);

        private IntVec3 hitThing;

        private Vector3 origin;

        private Mesh boltMesh;

        private Quaternion direction;

        private Material mat;

        public TM_MeshBolt(IntVec3 hitThing, Vector3 origin, Material _mat)
        {
            this.hitThing = hitThing;
            this.origin = origin;
            mat = _mat;
        }

        public void CreateBolt()
        {
            Vector3 vector;
            vector.x = (float)hitThing.x;
            vector.y = (float)hitThing.y;
            vector.z = (float)hitThing.z;
            direction = Quaternion.LookRotation((vector - origin).normalized);
            float distance = Vector3.Distance(origin, vector);
            boltMesh = TM_MeshMaker.NewBoltMesh(distance, 6f);
            Graphics.DrawMesh(boltMesh, origin, direction, mat, 0);
        }

        public void CreateFadedBolt(int magnitude)
        {
            Vector3 vector;
            vector.x = (float)hitThing.x;
            vector.y = (float)hitThing.y;
            vector.z = (float)hitThing.z;
            direction = Quaternion.LookRotation((vector - origin).normalized);
            float distance = Vector3.Distance(origin, vector);
            boltMesh = TM_MeshMaker.NewBoltMesh(distance, 6f);
            //Graphics.DrawMesh(this.boltMesh, this.origin, this.direction, this.mat, 0);
            Graphics.DrawMesh(boltMesh, origin, direction, FadedMaterialPool.FadedVersionOf(mat, (float)magnitude), 0);
        }
    }
}
