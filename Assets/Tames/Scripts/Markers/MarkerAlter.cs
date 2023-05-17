using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerAlter : MonoBehaviour
    {
        public string label;
        public GameObject syncAlternative;
        public string back;
        public string forward;
        public bool initial = false;
        public string[] ToLines()
        {
            return new string[]
            {
                ":alter",
                MarkerSettings.ObjectToLine(gameObject),
                label,
                 MarkerSettings.ObjectToLine(syncAlternative),
                back,
                forward,
                initial?"1":"0"
            };
        }
        public static int FromLines(string[] line, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(line[index]);
            MarkerAlter ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((ma = go.AddComponent<MarkerAlter>()) == null) ma = go.AddComponent<MarkerAlter>();
                        ma.label = line[index + 1];
                        ma.syncAlternative = MarkerSettings.LineToObject(line[index + 2]);
                        ma.back = line[index + 3];
                        ma.forward = line[index + 4];
                        ma.initial = line[index + 5] == "1";
                        return index + 5;
                }
            return index;
        }
    }
}
