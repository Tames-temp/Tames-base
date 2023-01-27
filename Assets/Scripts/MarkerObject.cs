using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tames
{
    public class MarkerObject : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject path = null;
        public GameObject start = null;
        public GameObject end = null;
        public GameObject middle = null;
        public GameObject axis = null;
        public GameObject pivot = null;
        public GameObject up = null;
        public GameObject tracker = null;
        public GameObject mover = null;
        public GameObject[] all;
        void Start()
        {
            all = new GameObject[9];
            all[0] = start;
            all[1] = end;   
            all[2] = middle;
            all[3] = axis;
            all[4] = pivot;
            all[5] = up;
            all[6] = tracker;
            all[7] = mover;
            all[8] = path;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}