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
        public TameChanger flickerParent = null;
        public bool flickering = false;
        /// <summary>
        /// the list of stops, each stop contains a value relative to 0..1 progress (stop[i] is for progress = i / (float)(stop.Count -1)
        /// /// </summary>
        public List<TameNumericStep> steps;
        protected bool[] flickerPlan;
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
        public float[] On(float p, float tp)
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
                case ToggleType.Switch:
                    if (p >= toggle) return steps[^1].value; else return steps[0].value;
                default:
                    if (flickerParent != null)
                    {
                        flickering = flickerParent.flickering;
                        Debug.Log("flicker mat " + tp + " " + flickering);
                    }
                    else
                    {
                        sc = (int)(tp % FlickPlanCount);
                        index = (int)((tp % 1f) * 100);
                        flickering = flickerPlan[sc * FlickPlanCount + index];
                  //      Debug.Log("flicker lig " + tp + " " + flickering);
                    }
                    return flickering ? steps[^1].value : steps[0].value;
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
                //             Debug.Log("read-y: " + mh.items[i]);
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
            string clean = Utils.Clean(line);
            List<string> si, s = Utils.Split(clean, " ");
            float f;
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
        private const int FlickPlanCount = 3;
        private void Randomize(int[] a, int n, int spare)
        {
            int e, j;
            for (int i = 0; i < n; i++)
            {
                j = a.Length - i - 1;
                int r = UnityEngine.Random.Range(0, a.Length - n - spare);
                e = a[r];
                a[r] = a[j];
                a[j] = e;
                Debug.Log("flicker " + i + " > " + r);
            }
        }
        public void SetFlickerPlan(Markers.MarkerFlicker mf)
        {
            if ((mf.byLight != null) || (mf.byMaterial != null)) return;
            flickerPlan = new bool[FlickPlanCount * 100];
            //   float f0 = mf.minFlicker, f2 = mf.maxFlicker;
            float df;
            float[] fs = new float[mf.flickerCount];
            float left = (1 - mf.flickerCount * mf.minFlicker) / 2;
            float spare = left;
            float max = Mathf.Min(mf.maxFlicker - mf.minFlicker, left);
            Debug.Log("flicker " + max);
            for (int i = 0; i < mf.flickerCount; i++)
            {
                df = UnityEngine.Random.Range(0, max);
                fs[i] = mf.minFlicker + df;
                max -= df;
                left -= df;
                Debug.Log("flicker " + i + " " + fs[i] + " " + left + " " + df);
            }
            int spareLen = (int)(spare * 100);
            int dl, len = spareLen + (int)(left * 100);
            Debug.Log("flicker len: " + len);
            int[] plan = new int[len + mf.flickerCount];
            int k;
            for (int i = 0; i < FlickPlanCount; i++)
            {
                for (int j = 0; j < plan.Length; j++)
                    plan[j] = j < len ? -1 : j - len;
                Randomize(plan, mf.flickerCount, mf.steadyPortion ? spareLen : 0);
                //  Debug.Log("flicker check: "+plan[plan.Length-1]);
                k = 0;
                for (int j = 0; j < plan.Length; j++)
                    if (k >= 100) break;
                    else if (plan[j] < 0)
                    {
                        flickerPlan[i * 100 + k] = false;
                        k++;
                    }
                    else
                    {
                        dl = (int)(fs[plan[j]] * 100);
                        for (int t = 0; t < dl; t++)
                        {
                            flickerPlan[i * 100 + k] = true;
                            k++;
                            if (k >= 100) break;
                        }
                    }
            }
        }
        public void From(TameChanger tch)
        {
            count = tch.count;
            property = tch.property;
            steps = tch.steps;
            toggle = tch.toggle;
            toggleType = tch.toggleType;
        }
    }
}