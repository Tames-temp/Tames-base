using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerAlterMaterial : MonoBehaviour
    {
        public Material applyTo = null;
        public string back = "";
        public string forward = "";
        public Material initial = null;
        public Material[] alternatives;
        public string[] ToLines()
        {
            string[] r = new string[7 + alternatives.Length];
            r[0] = ":matalt";
            r[1] = MarkerSettings.ObjectToLine(gameObject);
            r[2] = MarkerSettings.FindMaterial(applyTo);
            r[3] = back;
            r[4] = forward;
            r[5] = MarkerSettings.FindMaterial(initial);
            r[6] = alternatives.Length + "";
            for (int i = 0; i < alternatives.Length; i++)
                r[i + 7] = MarkerSettings.FindMaterial(alternatives[i]);
            return r;
        }
        public static int FromLines(string[] line, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(line[index]);
            MarkerAlterMaterial ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((ma = go.AddComponent<MarkerAlterMaterial>()) == null) ma = go.AddComponent<MarkerAlterMaterial>();
                        ma.applyTo = MarkerSettings.FindMaterial(line[index + 1]);
                        ma.back = line[index + 2];
                        ma.forward = line[index + 3];
                        ma.initial = MarkerSettings.FindMaterial(line[index + 4]);
                        int l = int.Parse(line[index + 5]);
                        ma.alternatives = new Material[l];
                        for (int i = 0; i < l; i++)
                            ma.alternatives[i] = MarkerSettings.FindMaterial(line[index + i + 6]);
                        return index + 6 + l;
                }
            return index;
        }
    }
}
