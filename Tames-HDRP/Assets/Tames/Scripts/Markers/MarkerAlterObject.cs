using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerAlterObject : MonoBehaviour
    {
        public GameObject syncWith;
        public GameObject initial;
        public InputSetting control;
        public GameObject[] alternatives;
        public string[] ToLines()
        {
            string[] r = new string[6 + alternatives.Length];
            r[0] = ":alter";
            r[1] = MarkerSettings.ObjectToLine(gameObject);
            r[2] = MarkerSettings.ObjectToLine(syncWith);
            r[3] = MarkerSettings.ObjectToLine(initial);
            r[4] = control.ToString();
            for (int i = 0; i < alternatives.Length; i++)
                r[i + 5] = MarkerSettings.ObjectToLine(alternatives[i]);
            return r;
        }
        public static int FromLines(string[] line, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(line[index]);
            MarkerAlterObject ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((ma = go.AddComponent<MarkerAlterObject>()) == null) ma = go.AddComponent<MarkerAlterObject>();
                        ma.syncWith = MarkerSettings.LineToObject(line[index + 1]);
                        ma.initial = MarkerSettings.LineToObject(line[index + 2]);
                        ma.control = InputSetting.FromString(line[index + 3]);
                        int l = int.Parse(line[index + 4]);
                        ma.alternatives=new GameObject[l];
                        for(int i = 0; i < l; i++)
                            ma.alternatives[i] = MarkerSettings.LineToObject(line[index + 5 + i]);
                        return index + 5 + l - 1;
                }
            return index;
        }
    }
}
