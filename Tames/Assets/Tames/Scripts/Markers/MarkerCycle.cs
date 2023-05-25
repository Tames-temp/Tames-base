using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Markers
{
    public class MarkerCycle : MonoBehaviour
    {
        public float offset = 0;
        public string itemNames = "";
        public GameObject childrenOf = null;
        public string[] ToLines()
        {
            return new string[]
            {
                ":cycle",
                MarkerSettings.ObjectToLine(gameObject),
                offset+"",
                itemNames,
                MarkerSettings.ObjectToLine(childrenOf)
            };
        }
        public static int FromLines(string[] lines, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(lines[index]);
            MarkerCycle ma;
            if(go == null)
            {
                if((ma=go.GetComponent<MarkerCycle>())==null)ma=go.AddComponent<MarkerCycle>();
                ma.offset = float.Parse(lines[index+1]);
                ma.itemNames= lines[index+2];
                ma.childrenOf = MarkerSettings.LineToObject(lines[index + 3]);
                return index + 3;
            }
            return index;
        }
    }
}
