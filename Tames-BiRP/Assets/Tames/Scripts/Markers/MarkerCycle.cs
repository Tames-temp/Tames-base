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
        public GameObject[] childrenOf;
        public string[] ToLines()
        {
            List<string> r = new List<string>()
            {
                ":cycle",
                MarkerSettings.ObjectToLine(gameObject),
                offset+"",
                itemNames,
                childrenOf.Length+"",
            };
            for (int i = 0; i < childrenOf.Length; i++)
                r.Add(MarkerSettings.ObjectToLine(childrenOf[i]));
            return r.ToArray();
        }
        public static int FromLines(string[] lines, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(lines[index]);
            MarkerCycle ma;
            if (go == null)
            {
                if ((ma = go.GetComponent<MarkerCycle>()) == null) ma = go.AddComponent<MarkerCycle>();
                ma.offset = float.Parse(lines[index + 1]);
                ma.itemNames = lines[index + 2];
                int n = int.Parse(lines[index + 3]);
                ma.childrenOf = new GameObject[n];
                for (int i = 0; i < n; i++) ma.childrenOf[i] = MarkerSettings.LineToObject(lines[index + 4]);
                return index + 3;
            }
            return index;
        }
    }
}
