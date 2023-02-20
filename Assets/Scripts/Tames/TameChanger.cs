using System.Collections.Generic;
using UnityEngine;
namespace Tames
{
    /// <summary>
    /// hosts a float array that contains the values of each step of a changer
    /// </summary>
    public class TameNumericStep
    {
        public float[] value;
    }
    /// <summary>
    /// the class is used for changing the material and light properties for a given progress value
    /// </summary>
    public class TameChanger
    {
        /// <summary>
        /// the list of stops, each stop contains a value relative to 0..1 progress (stop[i] is for progress = i / (float)(stop.Count -1)
        /// /// </summary>
        public List<TameNumericStep> steps;
        /// <summary>
        /// defines the behaviour of changing based on the parent's progress. If the trigger in null, the element's trigger is used. Otherwise, for each property the trigger 
        /// </summary>
        public float toggle = 0.5f;
        public ToggleType toggleType;
        /// <summary>
        /// the material or light property to be changed
        /// </summary>
        public MaterialProperty property;
        /// <summary>
        /// count of the values in each stop (1: float, 3: color)
        /// </summary>
        public int count;
        /// <summary>
        /// returns the value(s) that the property should have on the progress = p 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float[] On(float p)
        {
            int index, sc = steps.Count;
            float k, d;
            if (p >= 1) return steps[steps.Count - 1].value;
            if (p <= 0) return steps[0].value;
            switch (toggleType)
            {
                case ToggleType.Stepped:
                    index = (int)System.Math.Round(p * (steps.Count - 1));
                    return steps[index].value;
                case ToggleType.Gradient:
                    k = 1f / (sc - 1);
                    d = (p % k) / k;
                    index = (int)(p / k);
                    float[] r = new float[count];
                    for (int i = 0; i < count; i++)
                        r[i] = steps[index].value[i] + (steps[index + 1].value[i] - steps[index].value[i]) * d;
                    return r;
                default:
                    if (p >= toggle) return steps[steps.Count - 1].value; else return steps[0].value;
            }
        }
        /// <summary>
        /// creates a changer from a line as translated to a <see cref="ManifestHeader"/>. 
        /// </summary>
        /// <param name="mh">the manifest header</param>
        /// <param name="n">the number of expected value <see cref="count"/> in each stop. Every stop should have this exact amount of float values</param>
        /// <returns>returnes a TameChanger object if successful, or null if an error occurred</returns>
        public static TameChanger Read(ManifestHeader mh, int n)
        {
            ToggleType st = ToggleType.Gradient;
            if (mh.items.Count <= 2) return null;
            List<string> s;
            float sv = 0, f;
            s = Utils.Split(mh.items[0], ",");
            if (mh.items[0].StartsWith("grad")) st = ToggleType.Gradient;
            else if (mh.items[0].StartsWith("step")) st = ToggleType.Stepped;
            else if (Utils.SafeParse(mh.items[0], out sv)) st = ToggleType.Switch;
            else return null;
            List<TameNumericStep> steps = new List<TameNumericStep>();
            float[] value;
            for (int i = 1; i < mh.items.Count; i++)
            {
                Debug.Log("read-y: " + mh.items[i]);
                s = Utils.Split(mh.items[i], ",");
                if (s.Count < n) return null;
                value = new float[n];
                for (int j = 0; j < n; j++)
                    if (Utils.SafeParse(s[j], out f))
                        value[j] = f;
                    else
                        return null;
                steps.Add(new TameNumericStep() { value = value });
            }
            return new TameChanger()
            {
                steps = steps,
                toggleType = st,
                toggle = sv,
                count = n
            };
        }
        public static TameChanger ReadStepsOnly(string line, ToggleType st, float sv, int n)
        {
           string clean= Utils.Clean(line);
            List<string> si, s = Utils.Split(clean, " ");
            float  f;
            //  Debug.Log("read-y: " + s.Count);
            List<TameNumericStep> steps = new List<TameNumericStep>();
            float[] value;
         //   int n;
            for (int i = 0; i < s.Count; i++)
            {
                si = Utils.Split(s[i], ",");
                value = new float[n];
                if (s.Count < n) return null;
                for (int j = 0; j < n; j++)
                    if (Utils.SafeParse(si[j], out f))
                        value[j] = f;
                    else
                        return null;
                steps.Add(new TameNumericStep() { value = value });
            }
            return new TameChanger()
            {
                steps = steps,
                toggleType = st,
                toggle = sv,
                count = n
            };
        }
        public void From(TameChanger tch)
        {
            count= tch.count;
            property = tch.property;
            steps = tch.steps;
            toggle = tch.toggle;
            toggleType = tch.toggleType;
        }
    }
}