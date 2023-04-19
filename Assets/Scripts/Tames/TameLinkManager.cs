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
            else if (element.manifest != null)
                if (element.manifest.linkType == LinkedKeys.Cycle)
                    PopulateCycle(tgos, tes, null);
            if (element.markerQueue != null)
                PopulateQueue(tgos, tes, element.markerQueue);
            else if (element.manifest != null)
                if (element.manifest.queued)
                    PopulateQueue(tgos, tes, null);
            if (element.manifest != null)
            {
                if (element.manifest.linkType != LinkedKeys.None)
                {
                    TameFinder finder = new TameFinder() { owner = element};
                    finder.header = new ManifestHeader() { items = element.manifest.linked };
                    finder.PopulateObjects(tgos);
                    if (finder.objectList.Count > 0)
                        switch (element.manifest.linkType)
                        {
                            case LinkedKeys.Clone:
                                element.CreateClones(finder.objectList, tes);
                                break;
                            case LinkedKeys.Local:
                                element.handle.AlignLinked(LinkedKeys.Local, null, finder.objectList);
                                break;
                            case LinkedKeys.Stack:
                                element.handle.AlignLinked(element.manifest.linkType, null, finder.objectList);
                                element.handle.linkedOffset = element.manifest.progressedDistance;
                                break;
                            case LinkedKeys.Progress:
                                Transform t = Utils.FindStartsWith(finder.objectList[0].transform.parent, TameHandles.KeyLinker);
                                //      Debug.Log("link: " + t.name);                             
                                element.handle.AlignLinked(LinkedKeys.Progress, t != null ? t.gameObject : null, finder.objectList);
                                element.handle.linkedOffset = element.manifest.progressedDistance;
                                break;

                        }
                }
                else if (element.manifest.queued)
                {
                    element.handle.AlignQueued(element.manifest);
                }

            }
        }
        private void PopulateCycle(List<TameGameObject> tgos, List<TameElement> tes, MarkerCycle mc)
        {
            TameFinder finder = new TameFinder();
            finder.objectList.Clear();
            finder.owner = element;
            List<string> linked = new List<string>();
            string[] a;
            if (mc == null)
            {
                if (element.manifest != null)
                    finder.header = new ManifestHeader() { items = element.manifest.linked };
                finder.PopulateObjects(tgos);
                element.handle.AlignLinked(LinkedKeys.Cycle, null, finder.objectList);
                element.handle.linkedOffset = element.manifest.progressedDistance;
            }
            else
            {
                if (mc.itemNames != "")
                {
                    a = mc.itemNames.Split(',');
                    for (int i = 0; i < a.Length; i++)
                        linked.Add(Utils.Clean(a[i]));
                    finder.header = new ManifestHeader() { items = linked };
                    finder.PopulateObjects(tgos);
                }
                if (mc.childrenOf != null)
                {
                    for (int i = 0; i < mc.childrenOf.transform.childCount; i++)
                        finder.objectList.Add(TameGameObject.Find(mc.childrenOf.transform.GetChild(i).gameObject, tgos));
                }
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
