using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerScale:MonoBehaviour
    {
        public GameObject byObject = null;
        public string byName = "";
        public GameObject childrenOf = null;
        public enum ScaleAxis { X, Y, Z }
        public ScaleAxis axis = ScaleAxis.X;
        public float from;
        public float to;
        public enum AffectUV { U , V }
        public AffectUV affectedUV = AffectUV.U;
    }
}
