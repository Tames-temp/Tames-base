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
        public class Alternative
        {
            public List<GameObject> gameObject = new List<GameObject>();
        }
        public List<Alternative> alternatives = new List<Alternative>();
        public int prev = -1, next = -1;
        public int current = -1;
        public float activeDistance = -1;
        public float activeAngle = 30;
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
            bool possible = true;
            if ((alternatives.Count > 0) && (current >= 0))
                possible = TameCamera.CheckDistanceAndAngle(alternatives[current].gameObject[0], activeDistance, activeAngle);
            
            if (possible)
            {
                foreach (TameInputControl tci in back)
                    if (tci.Pressed()) { GoPrevious(); return; }
                foreach (TameInputControl tci in forth)
                    if (tci.Pressed()) { GoNext(); return; }
            }
        }
        public void SetKeys(string keys)
        {
            TameInputControl[] tcis;
            string[] ks = keys.Split(' ');
            for (int i = 0; i < ks.Length; i++)
            {
                TameInputControl[] tcs = TameInputControl.ByStringTwoMonos(ks[i]);
                if (tcs != null)
                {
                    back.Add(tcs[0]);
                    forth.Add(tcs[1]);
                }
            }
        }
   
        public static List<TameAlternative> GetAlternatives(List<TameGameObject> tgos)
        {
            List<TameAlternative> tas = new();
            TameAlternative ta;
            MarkerAlterObject ma;
            List<MarkerAlterObject> mas = new List<MarkerAlterObject>();
            List<MarkerAlterObject> syncMarkers = new List<MarkerAlterObject>();
            Alternative alt;
            for (int i = 0; i < tgos.Count; i++)
                if ((ma = tgos[i].gameObject.GetComponent<MarkerAlterObject>()) != null)
                {
                    //         Debug.Log("ALTERX " + tgos[i].gameObject.name);
                    if (ma.syncWith == null)
                        mas.Add(ma);
                    else
                        syncMarkers.Add(ma);
                }
            for (int i = 0; i < mas.Count; i++)
            {
                ta = new TameAlternative();
                ta.SetKeys(mas[i].control.pair);
                ta.activeDistance = mas[i].control.maxDistance;
                ta.activeAngle = mas[i].control.maxAngle;
                for (int j = 0; j < mas[i].alternatives.Length; j++)
                    if (mas[i].alternatives[j] != null)
                    {
                        alt = new Alternative();
                        alt.gameObject.Add(mas[i].alternatives[j]);
                        for (int k = syncMarkers.Count - 1; k >= 0; k--)
                            if (syncMarkers[k].syncWith == mas[i].alternatives[j])
                            {
                                //         Debug.Log("ALTER " + syncMarkers[k].gameObject.name);
                                alt.gameObject.Add(syncMarkers[k].gameObject);
                                syncMarkers.RemoveAt(k);
                            }
                        ta.alternatives.Add(alt);
                    }
                int initial = 0;
                if (mas[i].initial != null)
                    for (int j = 0; j < ta.alternatives.Count; j++)
                        if (mas[i].initial == ta.alternatives[j].gameObject[0])
                        {
                            initial = j;
                            break;
                        }
                ta.SetInitial(initial);
                tas.Add(ta);
            }
            return tas;
        }
    }
}
