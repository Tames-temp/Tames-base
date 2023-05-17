using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Tames
{
    public class ManifestCustom : ManifestBase
    {
        public float[] range = new float[] { 0, 1 };
        public List<TameInputControl> tics = new List<TameInputControl>();
        public static int Create(ManifestHeader header, string[] lines, int index, out TameCustomValue tcv)
        {
            ManifestCustom tcm = new ManifestCustom();
            int i = tcm.Read(lines, index);
            if (tcm.tics.Count > 0)
                tcv = new TameCustomValue()
                {
                    control = tcm.tics,
                    name = header.items[0],
                    range = tcm.range,
                    manifest = tcm
                };
            else
                tcv = null;
            return i;
        }
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float a, b;
            float[] f2;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                if (mh.key == TameKeys.None)
                {
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Input:
                            tics.AddRange(GetControl(mh, 0));
                            break;
                        case ManifestKeys.Factor:
                            if (mh.items.Count == 2)
                                if (Utils.SafeParse(mh.items[0], out a) && Utils.SafeParse(mh.items[1], out b))
                                    range = new float[] { a, b };
                            break;
                        default:
                            ReadShared(mh);
                            break;
                    }
                }
                else
                {
                    i--;
                    break;
                }
                i++;
            }
            return i;
        }
        public static List<TameInputControl> GetControl(ManifestHeader header, int start)
        {
            List<TameInputControl> r = new List<TameInputControl>();
            for (int i = start; i < header.items.Count; i++)
            {
                TameInputControl tci = TameInputControl.ByStringDuo(header.items[i]);
                if (tci != null) r.Add(tci);
            }
            return r;
        }


    }
}
