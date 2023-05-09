using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Markers;

namespace Tames
{
     public class TameAlternative
    {
        internal class Alternative
        {
            public List<GameObject> gameObject = new List<GameObject>();
        }
        private List<Alternative> alternatives = new List<Alternative>();
        public int prev = -1, next = -1;
        public int current = -1;
        public List<TameInputControl> back = new List<TameInputControl>();
        public List<TameInputControl> forth = new List<TameInputControl>();
        public void GoNext()
        {
            if (current >= 0)
            {
                if (alternatives.Count > 0)
                    current = (current + 1) % alternatives.Count;
            }
            else if (alternatives.Count > 0) current = 0;
            Progress();
        }
        public void GoPrevious()
        {
            if (current >= 0)
            {
                if (alternatives.Count > 0)
                    current = (current + alternatives.Count - 1) % alternatives.Count;
            }
            else if (alternatives.Count > 0) current = 0;
            Progress();
        }
        public void SetInitial(int i)
        {
            if (alternatives.Count > 0)
                current = i;
            Progress();
        }
        public void Progress()
        {
            if (current >= 0)
            {
                for (int i = 0; i < alternatives.Count; i++)
                    foreach (GameObject go in alternatives[i].gameObject)
                        go.SetActive(i == current);
            }
        }
        public void Update()
        {
            foreach (TameInputControl tci in back)
                if (tci.Pressed()) { GoPrevious(); Debug.Log("ALTER: back " + current + " "+ alternatives.Count); return; }
            foreach (TameInputControl tci in forth)
                if (tci.Pressed()) { GoNext(); Debug.Log("ALTER: forth " + current + " " + alternatives.Count); return; }
        }
        public void SetKeys(string keys, bool backOrFoth)
        {
            List<TameInputControl> tcs = backOrFoth ? back : forth;
            string[] ks = keys.Split(' ');
            for (int i = 0; i < ks.Length; i++)
            {
                TameInputControl tc = TameInputControl.ByStringMono(ks[i]);
                if (tc != null) tcs.Add(tc);
            }
        }
        public static List<TameAlternative> GetAlternatives(List<TameGameObject> tgos)
        {
            List<TameAlternative> tas = new List<TameAlternative>();
            TameAlternative ta;
            MarkerAlter ma;
            List<MarkerAlter> markers = new List<MarkerAlter>();
            List<MarkerAlter> syncMarkers = new List<MarkerAlter>();
            Alternative alt;
            for (int i = 0; i < tgos.Count; i++)
                if ((ma = tgos[i].gameObject.GetComponent<MarkerAlter>()) != null)
                {
           //         Debug.Log("ALTERX " + tgos[i].gameObject.name);
                    if (ma.syncAlternative == null)
                        markers.Add(ma);
                    else
                        syncMarkers.Add(ma);
                }
            for (int i = 0; i < markers.Count; i++)
            {
         //       Debug.Log("ALTER " + markers[i].gameObject.name);
                ta = new TameAlternative();
                ta.SetKeys(markers[i].back, true);
                ta.SetKeys(markers[i].forward, false);
                alt = new Alternative();
                alt.gameObject.Add(markers[i].gameObject);
                for (int k = syncMarkers.Count - 1; k >= 0; k--)
                    if (syncMarkers[k].gameObject == markers[i].gameObject)
                    {
                        alt.gameObject.Add(syncMarkers[k].gameObject);
                        syncMarkers.RemoveAt(k);
                    }
                ta.alternatives.Add(alt);
                int initial = 0;
                bool initialized = false;
                for (int j = markers.Count - 1; j > i; j--)
                    if (markers[j].label.ToLower() == markers[i].label.ToLower())
                    {
                    //    Debug.Log("ALTER ADDED " + markers[j].gameObject.name);
                        alt = new Alternative();
                        alt.gameObject.Add(markers[j].gameObject);
                        if (!initialized)
                            if (markers[j].initial)
                            {
                                initial = j;
                                initialized = true;
                            }
                        for (int k = syncMarkers.Count - 1; k >= 0; k--)
                            if (syncMarkers[k].gameObject == markers[j].gameObject)
                            {
                                alt.gameObject.Add(syncMarkers[k].gameObject);
                                syncMarkers.RemoveAt(k);
                            }
                        markers.RemoveAt(j);
                        ta.alternatives.Add(alt);
                    }
           //     Debug.Log("ALTER INIT " + initial);
                ta.SetInitial(initial);
                tas.Add(ta);
            }
            return tas;
        }
    }
}
