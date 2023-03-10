using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tames
{
    public class ManifestMaterial : ManifestBase
    {
        public bool unique = false;
        public List<TameChanger> properties = new List<TameChanger>();
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
                            updateType = TrackBasis.Tame;
                            updates = mh;
                            break;
                        case ManifestKeys.Color:
                            tc = TameColor.Read(mh, false);
                            //     Debug.Log("chor "+mh.items[1] + (tc==null?" null":" not"));
                            if (tc != null)
                                properties.Add(tc);
                            break;

                        case ManifestKeys.Glow:
                            tc = TameColor.Read(mh, true);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Glow;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.MapX:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.MapX;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.MapY:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.MapY;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.LightX:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.LightX;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.LightY:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.LightY;
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
                        case ManifestKeys.Unique:
                            unique = true;
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
                            tch = TameChanger.ReadStepsOnly(ch.steps, ch.GetToggle(), ch.switchValue, 1);
                            tch.property = mp;
                            break;
                        default:
                            tch = tco = TameColor.ReadStepsOnly(ch.steps, ch.GetToggle(), ch.switchValue, mp == MaterialProperty.Glow);
                            tco.property = mp;
                            break;
                    }
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
