using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class ManifestObject : ManifestBase
    {
        public List<string> cues = new List<string>();
        //     7/25 12:40
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float f;
            float[] f2;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                // Debug.Log(mh.key+" object prop: " + mh.header + " >> " + lines[i]);
                if (mh.key == TameKeys.None)
                {
            //            Debug.Log(" subkey: " + mh.header +" "+ mh.subKey);
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Update: updates = mh; updateType = TrackBasis.Tame; break;
                        case ManifestKeys.Follow: updates = mh; updateType = TrackBasis.Mover; break;
                        case ManifestKeys.Track:
                            updates = mh; updateType = TrackBasis.Object;
                            //      Debug.Log("tracked");
                            break;
                        case ManifestKeys.Area:
                            cues.AddRange(mh.items);
                            break;
                        case ManifestKeys.Queue: ReadQueue(mh); break;
                        case ManifestKeys.Linked:
                        case ManifestKeys.Clone: ReadLink(mh); break;
                        case ManifestKeys.Scale: ReadScale(mh); break;
                        default: ReadShared(mh); break;
                    }
                }
                else
                {
                    i--;
                    break;
                }
                i++;
            }
            //  Debug.Log("update " + updateType);
            return i;
        }
        private void ReadQueue(ManifestHeader mh)
        {
            float start;
            float by;
            int count = 10;
            bool isCount;
            //    Debug.Log("queue: " + mh.header + ManifestKeys.keys[ManifestKeys.By - 1].alias[0]);

            if (mh.items.Count < 3)
                return;
            if (Utils.SafeParse(mh.items[0], out start))
            {
                if (Utils.SafeParse(mh.items[2], out by))
                {
                    if (isCount = ManifestKeys.keys[ManifestKeys.Count - 1].Has(mh.items[1]))
                        count = (int)by;
                    if (isCount || ManifestKeys.keys[ManifestKeys.By - 1].Has(mh.items[1]))
                    {
                        queued = true;
                        queueCount = isCount ? count : -1;
                        queueInterval = by;
                        queueStart = start;
                        if (mh.items.Count > 3)
                            if ("uxUX".IndexOf(mh.items[3]) >= 0) queueUV = 0; else if ("vyVY".IndexOf(mh.items[3]) >= 0) queueUV = 1;
                    }
                }
            }
        }
        private void ReadScale(ManifestHeader mh)
        {
            if (mh.items.Count < 4)
                return;
            int axis = mh.items[0].ToLower() switch
            {
                "x" => 0,
                "y" => 1,
                "z" => 2,
                _ => -1
            };
            if (axis < 0) return;

            string[] sp = mh.items[1].Split(',');
            if (sp.Length < 2) return;
            if (!(Utils.SafeParse(sp[0], out scaleFrom) && Utils.SafeParse(sp[1], out scaleTo))) return;
            if (mh.items[2].ToLower().Equals("x")) scaleUV = 0; else scaleUV = 1;
            string s = "";
            for (int i = 3; i < mh.items.Count; i++)
                s += mh.items[i] + " ";
            string[] so = s.Split(',');
            for (int i = 0; i < so.Length; i++)
                scaledObjects.Add(Utils.Clean(so[i]));
            scaleAxis = axis;
            scales = true;
        }
        private void ReadLink(ManifestHeader mh)
        {
            float f = 0;
            int start = 2;
            int k;
            linkType = LinkedKeys.None;
            if (mh.subKey == ManifestKeys.Clone)
            {
                linkType = LinkedKeys.Clone;
                start = 0;
            }
            else if (mh.items.Count > 1)
            {
                //    Debug.Log("item0 = " + mh.items[0]);
                k = ManifestKeys.GetKey(mh.items[0].ToLower());
                switch (k)
                {
                    case ManifestKeys.Local:
                        start = 1;
                        linkType = LinkedKeys.Local;
                        break;
                    case ManifestKeys.Ratio:
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Progress;
                        break;
                    case ManifestKeys.Stack:
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Stack;
              //          Debug.Log("is stack " + progressedDistance);
                        break;
                    case ManifestKeys.Cycle:
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Cycle;
                        break;
                }
            }
            if (linkType != LinkedKeys.None)
            {
                string s = "";
                for (int i = start; i < mh.items.Count; i++)
                    s += " " + mh.items[i];
                s = Utils.Clean(s);
                string[] a = s.Split(',');
                int added = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    linked.Add(Utils.Clean(a[i]));
                    linkedOffset.Add(f);
                    linkedTypes.Add(linkType);
                    added++;
                }
                if (linked.Count == 0)
                    linkType = LinkedKeys.None;
            }
        }

    }
}
