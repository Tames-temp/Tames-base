using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerCarrier : MonoBehaviour
    {
        public bool position = true;
        public bool rotation = true;
        public string[] ToLines()
        {
            return new string[]
            {
                ":carrier",
                MarkerSettings.ObjectToLine(gameObject),
                position?"1":"0",
                rotation?"1":"0"
            };
        }
        public static int FromLines(string[] lines, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(lines[index]);
            MarkerCarrier ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((ma = go.AddComponent<MarkerCarrier>()) == null) ma = go.AddComponent<MarkerCarrier>();
                        ma.position = lines[index + 1] == "1";
                        ma.rotation = lines[index + 2] == "1";
                        return index + 3;
                }
            return index;
        }
    }
}
