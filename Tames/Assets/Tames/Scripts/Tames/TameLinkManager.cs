using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markers;
using UnityEngine;
namespace Tames
{
    public class TameLinkManager
    {
        public TameObject element;
        public void Populate(List<TameGameObject> tgos, List<TameElement> tes)
        {
            if (element.markerCycle != null)
                PopulateCycle(tgos, tes, element.markerCycle);
            if (element.markerQueue != null)
                PopulateQueue(tgos, tes, element.markerQueue);
        }
        private void PopulateCycle(List<TameGameObject> tgos, List<TameElement> tes, MarkerCycle mc)
        {
            TameFinder finder = new TameFinder();
            finder.objectList.Clear();
            finder.owner = element;
            List<string> linked = new List<string>();
            string[] a;
            if (mc != null)
            {
                if (mc.itemNames != "")
                {
                    a = mc.itemNames.Split(',');
                    for (int i = 0; i < a.Length; i++)
                        linked.Add(Utils.Clean(a[i]));
                    finder.header = new ManifestHeader() { items = linked };
                    finder.PopulateObjects(tgos);
                }
                if (mc.childrenOf.Length > 0)
                {
                    element.handle.childrenParent = mc.childrenOf;
                    for (int j = 0; j < mc.childrenOf.Length; j++)
                        for (int i = 0; i < mc.childrenOf[j].transform.childCount; i++)
                            finder.objectList.Add(TameGameObject.Find(mc.childrenOf[j].transform.GetChild(i).gameObject, tgos));
                }
                Debug.Log("children " + mc.name + " " + finder.objectList.Count);
                element.handle.AlignLinked(LinkedKeys.Cycle, null, finder.objectList);
            }
        }
        private int StringToUV(string s)
        {
            switch (s.ToLower())
            {
                case "u":
                case "x":
                    return 0;
                case "v":
                case "y":
                    return 1;
                default:
                    return -1;
            }
        }
        private void PopulateQueue(List<TameGameObject> tgos, List<TameElement> tes, MarkerQueue mq)
        {
            element.handle.AlignQueued(mq.startAt, mq.byInterval ? -1 : (int)mq.countOrInterval, mq.byInterval ? mq.countOrInterval : -1, StringToUV(mq.randomizeUOrV));
        }


    }
}
