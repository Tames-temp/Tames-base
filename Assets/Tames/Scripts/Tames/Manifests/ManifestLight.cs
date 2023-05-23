using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class ManifestLight : ManifestBase
    {
        //     public List<TameChanger> properties = new List<TameChanger>();
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float f;
            float[] f2;
            TameChanger tc;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                if (mh.key == TameKeys.None)
                {
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Update:
                            //            Debug.Log("update " + mh.header);
                            updates = mh;
                            updateType = TrackBasis.Tame;
                            break;
                        case ManifestKeys.Color:
                        case ManifestKeys.Glow:
                            tc = TameColor.Read(mh, true);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Glow;
                                properties.Add(tc);
                            }
                            break;

                        case ManifestKeys.Bright:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Bright;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.Focus:

                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Focus;
                                properties.Add(tc);
                            }
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
        public void ExternalChanger(Markers.MarkerChanger[] chs)
        {
            TameChanger tch;
            TameColor tco;
            bool found;
            MaterialProperty mp;
            int pcount = properties.Count;
            if (chs != null)
                foreach (Markers.MarkerChanger ch in chs)
                {
                    mp = ch.GetProperty();
                    switch (mp)
                    {
                        case MaterialProperty.Bright:
                        case MaterialProperty.MapY:
                        case MaterialProperty.LightY:
                        case MaterialProperty.MapX:
                        case MaterialProperty.LightX:
                        case MaterialProperty.Focus:
                            if ((tch = TameChanger.ReadStepsOnly(ch.steps, ch.GetToggle(), ch.switchValue, 1)) != null)
                                tch.property = mp;
                            break;
                        default:
                            if (ch.colorSteps.Length > 0)
                                tch = tco = TameColor.ReadStepsOnly(ch.colorSteps, ch.GetToggle(), ch.switchValue, false);
                            else
                                tch = tco = TameColor.ReadStepsOnly(ch.steps, ch.GetToggle(), ch.switchValue, false);
                            if (tch != null) tco.property = mp;
                            break;
                    }
                    if (tch != null)
                    {
                        tch.marker = ch;
                        found = false;
                        for (int i = 0; i < pcount; i++)
                            if (mp == properties[i].property)
                            {
                                if (tch.count == 1)
                                    properties[i].From(tch);
                                else
                                    ((TameColor)properties[i]).From((TameColor)tch);
                                found = true;
                            }
                        if (!found)
                            properties.Add(tch);
                    }
                }

        }
    }
}
