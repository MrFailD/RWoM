using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace TorannMagic.Golems
{
    public class GolemDrawClass
    {
        public Material mat;
        public Mesh mesh;
        public IntVec3 startPos = default(IntVec3);
        public float angle;
        public int startTick;
        public int duration = 60;

        public GolemDrawClass(Material _mat, Mesh _mesh, IntVec3 _startPos, float _angle, int _startTick, int _endTick)
        {
            mat = _mat;
            mesh = _mesh;
            startPos = _startPos;
            angle = _angle;
            startTick = _startTick;
            duration = _endTick;
        }
    }
}
