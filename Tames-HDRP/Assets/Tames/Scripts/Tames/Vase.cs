using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Vases
{
    public class VaseRing
    {
        public int sideCount = 1;
        public float filletPortion = 0;
        public Vector2 scale = Vector2.one;
        public float thickness = 0.1f;
        public float elevation = 0;
        public float sharpness = 0.5f;
        public float rotation = 0;
        public Vector3[] outer, inner;
    }
    public class Vase
    {
        public List<VaseRing> rings = new();
        public int uvMaster = -1;
        public float maxEdgeSize = 0.1f;
        public int ringEdgeCount = 10;
        public enum SizeType { ByMaxEdge, ByEdgeCount}
        public SizeType sizeType = SizeType.ByEdgeCount;
        public GameObject model { get { return gameObject; } }
        private GameObject gameObject = null;
        public void Generate()
        {
            for(int i = 0; i < rings.Count; i++)
            {

            }
        }
    }
}
