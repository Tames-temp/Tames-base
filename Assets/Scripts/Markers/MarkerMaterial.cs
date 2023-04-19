using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerMaterial : MonoBehaviour
    {
        public Material material;
        public bool unique;
      


        public MarkerChanger[] MatchMaterial(Material tm)
        {
            MarkerChanger[] changers = gameObject.GetComponents<MarkerChanger>();
            if (changers != null)
                if (changers.Length > 0)
                    if (tm == material)
                        return changers;
            return null;
        }
        public static MarkerChanger[] FirstMatch(List<MarkerMaterial> mms, Material tm, out int index)
        {
            MarkerChanger[] mcs;
            index = -1;
            for (int i = 0; i < mms.Count; i++)
                if ((mcs = mms[i].MatchMaterial(tm)) != null)
                {
                    index = i;
                    return mcs;
                }
            return null;
        }
        public static MarkerChanger[] LightMatch(List<MarkerMaterial> mms, Light l, out int index)
        {
            MarkerChanger[] mcs;
            index = -1;
            for (int i = 0; i < mms.Count; i++)
            {
                Light light = mms[i].gameObject.GetComponent<Light>();
                if (light == l)
                {
                    index = i;
                    return mms[i].gameObject.GetComponents<MarkerChanger>();
                }
            }
            return null;
        }
    }

}