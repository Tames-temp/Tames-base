using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerGrass : MonoBehaviour
    {
        public Material material;
        public float density = 100;
        public float randomness = 0.2f;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minBase = 0.05f;
        public float maxBase = 0.02f;
        public bool relative = false;
        public int segmentCount = 3;
        public float minBow = 0;
        public float maxBow = 0.2f;
        public int variantCount = 1;
    }
}
