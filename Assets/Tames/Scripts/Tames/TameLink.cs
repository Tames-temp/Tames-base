using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameLink
    {
        public Markers.MarkerLink.CloneTypes type;
        public GameObject gameObject;
        public float factor = 1;
        public float offset = 0;
        public Markers.MarkerLink.LinkTypes offsetBase;
        public Markers.MarkerLink.LinkTypes speedBase;
        public TameLink(Markers.MarkerLink ml)
        {
            type= ml.type;
            gameObject = ml.gameObject;
            factor = ml.factor;
            offset = ml.offset;
            offsetBase = ml.offsetBase;
            speedBase = ml.speedBase;
        }
        public TameLink(GameObject go, Markers.MarkerLink ml)
        {
            type = ml.type;
            gameObject = go;
            factor = ml.factor;
            offset = ml.offset;
            offsetBase = ml.offsetBase;
            speedBase = ml.speedBase;

        }
    }
}
