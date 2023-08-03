using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerOrigin : MonoBehaviour
    {
        public enum Origin
        {
            Blender,
            Max,
            Rhino
        }
        public Origin origin;
        public int GetOrigin()
        {
            switch (origin)
            {
                case Origin.Blender: return Tames.TameManager.Blender;
                case Origin.Max: return Tames.TameManager.Max3DS;
                case Origin.Rhino: return Tames.TameManager.Rhino;
            }
            return -1;
        }
    }
}
