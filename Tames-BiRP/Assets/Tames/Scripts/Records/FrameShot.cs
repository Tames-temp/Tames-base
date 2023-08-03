using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Records
{
    public class FrameShot
    {
        public float time;
        public Vector3 cpos;
        public Quaternion crot;
        public Vector3[] hpos;
        public Quaternion[] hrot;
        public float[] grip;
        public ulong GPHold, KBHold, GPPressed, KBPressed;
        public uint VRPressed, VRHold;
        public uint mouse;
    }
}
