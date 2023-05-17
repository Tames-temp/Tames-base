using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerMaster:MonoBehaviour
    {
        public enum Types
        {
            Object, Material, Light
        }
        public Types type;
        public TameKeys Type { get { return type switch { Types.Object => TameKeys.Object, Types.Material => TameKeys.Material, _ => TameKeys.Light }; } }
        public string elements = "";
        public bool updates = true;
        public bool durations = true;
        public bool showKeys = true;
        public bool activeKeys = true;
    }
}
