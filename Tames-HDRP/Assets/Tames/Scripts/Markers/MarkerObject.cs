using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Markers
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
        public GameObject headTracker = null;
        //    public List<GameObject> areas = new List<GameObject> ();
        private GameObject[] all;
        public string manifestLines = "";
        public GameObject GetObject(int i)
        {
            return all[i];
        }
        public Transform GetTransform(int i)
        {
            return all[i] == null ? null : all[i].transform;
        }
        public void Set()
        {
            all = new GameObject[10];
            all[0] = start;
            all[1] = end;
            all[2] = middle;
            all[3] = axis;
            all[4] = pivot;
            all[5] = up;
            all[6] = tracker;
            all[7] = mover;
            all[8] = path;
            all[9] = headTracker;
        }
    }
}