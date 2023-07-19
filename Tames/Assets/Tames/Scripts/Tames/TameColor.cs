using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    /// <summary>
    /// this class is a specific changer for colors. The reason for a dedicated color changer is to allow names of colors as string inputs for <see cref="TameChanger.steps"/>. The accepted color names are
    /// white, grey, black, yellow, red, lime, green, pink, purple, cyan, blue, teal, magenta, with the possibility of adding light- or dark- modifiers (without space) before them.
    /// </summary>
    public class TameColor : TameChanger
    {
        public static string[] colorNames = new string[] { "white", "grey", "black", "yellow", "red", "lime", "green", "orange", "pink", "purple", "cyan", "blue", "teal", "magenta", "gold" };
        public static Color[] colors = new Color[] { Color.white, Color.gray, Color.black, Color.yellow, Color.red, new Color(0.8f, 1f, 0), Color.green, new Color(1f, 0.5f, 0), new Color(1f, 0.2f, 0.5f), new Color(0.5f, 0.2f, 0.8f), Color.cyan, Color.blue, new Color(0, 0.5f, 0.5f), Color.magenta, new Color(1f, 0.81f, 0) };
        /// <summary>
        /// changes a float array to <see cref="Color"/>. This is the array that is returned by <see cref="TameChanger.On"/>
        /// </summary>
        /// <param name="value">the float array (with length of three, and valued between 0 and 1)</param>
        /// <returns></returns>
        public static Color ToColor(float[] value)
        {
            return new Color(value[0], value[1], value[2], value[3]);
        }
        /// <summary>
        /// finds the index of the color name within the color names array
        /// </summary>
        /// <param name="s">color name</param>
        /// <returns></returns>
        private static int Find(string s)
        {
            for (int i = 0; i < colorNames.Length; i++)
                if (s.Equals(colorNames[i]))
                    return i;
            return -1;
        }
        /// <summary>
        /// converts a string to a 32bit color. The string's format should either be [prefix-]colorname[:alpha] or r,g,b[:alpha] 
        /// </summary>
        /// <param name="S"></param>
        /// <param name="c"></param>
        /// <returns>true if there was no error in parsing the color</returns>
        public static bool GetColor32(string S, out Color c)
        {
            float a = GetGlow(S, out c);
            if (a < 0)
                return false;
            else
                if (a > 1) a = 1;
            c.a = a;
            return true;
        }
        /// <summary>
        /// converts a string to an intensity-based color. The string's format  should either be [prefix-]colorname[:intensity] or r,g,b[:intensity]. The value of the color is passed to c argument while the intensity is returned as a float (negative in case of an error).  
        /// </summary>
        /// <param name="S"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float GetGlow(string S, out Color c)
        {
            bool prefixed = false;
            string lower = S.ToLower();
            int prefix = 0;
            string name = lower;
            int colorIndex = -1;
            int colon = lower.IndexOf(':'), dash = lower.IndexOf('-');
            float r, g, b, a = 1;
            //       Debug.Log("glw dash " + S + " " + dash);
            c = Color.white;
            if (colon > 0)
            {
                if (Utils.SafeParse(lower.Substring(colon + 1), out a))
                    name = lower.Substring(0, colon);
                else
                    return -1;
            }
            if (dash > 0)
            {
                string pre = name.Substring(0, dash);
                if (pre.Equals("light")) { prefix = 1; prefixed = true; }
                if (pre.Equals("dark")) { prefix = -1; prefixed = true; }
                //        Debug.Log("glw prefed " + prefixed + " " + prefix);
                if (prefixed)
                    name = name.Substring(dash + 1);
                else
                    return -1;
            }

            if (name.Length > 0) colorIndex = Find(name); else return -1;
            //     Debug.Log("glw name " + colorIndex);
            if (colorIndex >= 0)
            {
                c = colors[colorIndex];
                if (prefix == 1)
                {
                    r = c.r + (1 - c.r) / 2;
                    g = c.g + (1 - c.g) / 2;
                    b = c.b + (1 - c.b) / 2;
                    c = new Color(r, g, b);
                }
                if (prefix == -1)
                {
                    r = c.r / 2;
                    g = c.g / 2;
                    b = c.b / 2;
                    c = new Color(r, g, b);
                }
                return a;
            }
            else
            {
                List<string> comma = Utils.Split(name, ",");
                //       Debug.Log("color comma " + comma.Count);
                if (comma.Count > 2)
                {
                    try { r = int.Parse(comma[0]) / 255; if ((r > 1) || (r < 0)) return -1; } catch { return -1; }
                    try { g = int.Parse(comma[1]) / 255; if ((g > 1) || (g < 0)) return -1; } catch { return -1; }
                    try { b = int.Parse(comma[2]) / 255; if ((b > 1) || (b < 0)) return -1; } catch { return -1; }
                    c = new Color(r, g, b);
                    return a;
                }
            }
            return -1;

        }
        /// <summary>
        /// converts a color name to a <see cref="Color"/>
        /// </summary>
        /// <param name="S">the color name as it appears in the manifest</param>
        /// <param name="c">the color output</param>
        /// <returns>true if successful</returns>
        public static bool GetColor(string S, out Color c)
        {
            string lower = S.ToLower();
            int prefix = 0;
            string cn;
            int suffix = -1;
            int dash2 = -1, dash = lower.IndexOf('-');
            float r, g, b;
            //    Debug.Log("color dash " +dash);
            c = Color.white;
            if (dash > 0)
            {
                string pre = lower.Substring(0, dash);
                if (pre.Equals("light")) prefix = 1;
                if (pre.Equals("dark")) prefix = -1;
                if (lower.Length > dash + 1)
                {
                    cn = lower.Substring(dash + 1);
                    dash2 = cn.IndexOf('-');
                    suffix = Find(cn);
                }
                if (suffix >= 0)
                {
                    c = colors[suffix];
                    if (prefix == 1)
                    {
                        r = c.r + (1 - c.r) / 2;
                        g = c.g + (1 - c.g) / 2;
                        b = c.b + (1 - c.b) / 2;
                        c = new Color(r, g, b);
                        return true;
                    }
                    if (prefix == -1)
                    {
                        r = c.r / 2;
                        g = c.g / 2;
                        b = c.b / 2;
                        c = new Color(r, g, b);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                List<string> comma = Utils.Split(lower, ",");
                //       Debug.Log("color comma " + comma.Count);
                if (comma.Count > 1)
                {
                    try { r = int.Parse(comma[0]) / 255; if ((r > 1) || (r < 0)) return false; } catch { return false; }
                    try { g = int.Parse(comma[1]) / 255; if ((g > 1) || (g < 0)) return false; } catch { return false; }
                    try { b = int.Parse(comma[2]) / 255; if ((b > 1) || (b < 0)) return false; } catch { return false; }
                    c = new Color(r, g, b);
                    return true;
                }
                else
                {
                    suffix = Find(lower);
                    //      Debug.Log("color " + suffix);

                    if (suffix >= 0)
                    {
                        c = colors[suffix];
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// creates a color changer from a line as translated to a <see cref="ManifestHeader"/>. 
        /// </summary>
        /// <param name="mh">the manifest header</param>
        /// <returns>returnes a TameChanger object if successful, or null if an error occurrs</returns>
        public static TameColor Read(ManifestHeader mh, bool spectrum)
        {
            ToggleType st = ToggleType.Gradual;
            //      Debug.Log("color " + mh.items.Count);
            if (mh.items.Count <= 2) return null;
            List<string> s;
            float sv = 0, f;
            s = Utils.Split(mh.items[0], ",");
            if (mh.items[0].StartsWith("grad")) st = ToggleType.Gradual;
            else if (mh.items[0].StartsWith("step")) st = ToggleType.Stepped;
            else if (Utils.SafeParse(mh.items[0], out sv)) st = ToggleType.Switch;
            else return null;
            //    Debug.Log("color " + st+ " "+sv);
            List<TameNumericStep> stops = new List<TameNumericStep>();
            Color c;
            float a;
            //    Debug.Log("chorr:: " + mh.items.Count);
            for (int i = 1; i < mh.items.Count; i++)
            {
                if (spectrum)
                {
                    // Debug.Log("chor: " + mh.items[i]);
                    if ((a = GetGlow(mh.items[i], out c)) >= 0)
                    {
                        stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, a } });
                    }
                    else
                    {
                        //              Debug.Log("glw:: " + mh.items[i] + " " + a);
                        //      Debug.Log("chor: bad " + mh.items[i]);
                        return null;
                    }
                }
                else
                {
                    if (GetColor32(mh.items[i], out c))
                        stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, c.a } });
                    else
                        return null;
                }
            }
            //        Debug.Log("color return " + stops.Count);
            //        for (int i = 0; i < stops.Count; i++)
            //         Debug.Log("mix: " + stops[i].value[0] + ", " + stops[i].value[1] + ", " + stops[i].value[2]);
            return new TameColor()
            {
                steps = stops,
                toggleType = st,
                toggle = sv,
                count = 4,
                property = MaterialProperty.Color
            };

        }
        public static TameColor ReadStepsOnly(string line, ToggleType st, float sv, bool spectrum)
        {
            string clean = Utils.Clean(line);

            List<string> si, s = Utils.Split(clean, " ");
            float f;
            List<TameNumericStep> stops = new List<TameNumericStep>();
            Color c;
            float a;
            for (int i = 0; i < s.Count; i++)
            {
                if (spectrum)
                {
                    if ((a = GetGlow(s[i], out c)) >= 0)
                        stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, a } });
                    else
                        return null;
                }
                else
                {
                    if (GetColor32(s[i], out c))
                        stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, c.a } });
                    else
                        return null;
                }
            }
             return new TameColor()
            {
                steps = stops,
                toggleType = st,
                toggle = sv,
                count = 4,
                property = MaterialProperty.Color
            };
        }
        public static TameColor ReadStepsOnly(Color[] line, ToggleType st, float sv, bool spectrum)
        {
            List<TameNumericStep> stops = new List<TameNumericStep>();
            Color c;
             for (int i = 0; i < line.Length; i++)
            {
                c = line[i];
                if (spectrum)
                    stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, 1 } });
                else
                    stops.Add(new TameNumericStep() { value = new float[] { c.r, c.g, c.b, c.a } });
            }
               return new TameColor()
            {
                steps = stops,
                toggleType = st,
                toggle = sv,
                count = 4,
                property = MaterialProperty.Color
            };
        }
    }

}