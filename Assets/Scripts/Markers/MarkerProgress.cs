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
        public float speedFactor = 1;
        public float speedOffset = 0;
        public GameObject byElement = null;
        public string byName = "";
        public string trigger = "";
        public bool visible = true;
        public string switchingKey = "";

        public static void PopulateAll(List<Tames.TameGameObject> tgos)
        {
            MarkerProgress mp;
            foreach (Tames.TameGameObject tgo in tgos)
                if ((mp = tgo.gameObject.GetComponent<MarkerProgress>()) != null)
                    tgo.markerProgress = mp;
        }
    }
}
