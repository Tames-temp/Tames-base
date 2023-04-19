using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerProgress : MonoBehaviour
    {
        public CycleTypes continuity = CycleTypes.Stop;
        public float initialStatus = 0;
        public float setAt = 0;
        public float duration = 1;
        public string slerp = "";
        public string trigger = "";
        public GameObject byElement = null;
        public Material byMaterial = null;
        public bool manual = false;
        public string update = "";
        public string showBy = "";
        public bool active = true;
        public string activateBy = "";

        public static void PopulateAll(List<Tames.TameGameObject> tgos)
        {
        }
    }
}
