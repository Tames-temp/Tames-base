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
        public string back;
        public string forward;
        public float activationDistance = -1;
        public float activationAngle = 30;
        public GameObject[] alternatives;
        public string[] ToLines()
        {
            string[] r = new string[6 + alternatives.Length];
            r[0] = ":alter";
            r[1] = MarkerSettings.ObjectToLine(gameObject);
            r[2] = MarkerSettings.ObjectToLine(syncWith);
            r[3] = MarkerSettings.ObjectToLine(initial);
            r[4] = back;
            r[5] = forward;
            r[6] = activationDistance + "";
            r[7] = activationAngle + "";
            r[8] = alternatives.Length + "";
            for (int i = 0; i < alternatives.Length; i++)
                r[i + 9] = MarkerSettings.ObjectToLine(alternatives[i]);
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
                        ma.back = line[index + 3];
                        ma.forward = line[index + 4];
                        ma.activationDistance = float.Parse(line[index + 5]);
                        ma.activationAngle = float.Parse(line[index + 6]);
                        int l = int.Parse(line[index + 7]);
                        ma.alternatives=new GameObject[l];
                        for(int i = 0; i < l; i++)
                            ma.alternatives[i] = MarkerSettings.LineToObject(line[index + 8 + i]);
                        return index + 8 + l - 1;
                }
            return index;
        }
    }
}
