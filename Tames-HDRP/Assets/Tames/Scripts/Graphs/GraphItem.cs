using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Graphs
{
    public class GraphItem
    {
        public float[] values;
        public int index;
        public GameObject gameObject, label;
        public Vector3 initialPosition;
        public Markers.MarkerGraph marker;
        public Transform transform;
        public virtual void Scale(float s, float total = 0)
        {

        }
    }
    public class BarGraphItem : GraphItem
    {
        public Vector3 basePoint, topPoint, labelInitialPosition;
        public int axis, side;
        public float initialScale;
        public void Initialize(Markers.InputSetting.Axis yAx)
        {
            transform = gameObject.transform;
            initialPosition = transform.localPosition;
            axis = yAx switch
            {
                Markers.InputSetting.Axis.X => 1,
                Markers.InputSetting.Axis.Y => 2,
                Markers.InputSetting.Axis.Z => 3,
                Markers.InputSetting.Axis.NegX => -1,
                Markers.InputSetting.Axis.NegY => -2,
                Markers.InputSetting.Axis.NegZ => -3,
                _ => 2
            };
            side = axis < 0 ? -1 : 1;
            axis = axis < 0 ? -axis - 1 : axis - 1;
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3[] vs = mesh.vertices;
                Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
                for (int i = 0; i < vs.Length; i++)
                {
                    min = Vector3.Min(min, vs[i]);
                    max = Vector3.Max(max, vs[i]);
                }

                Vector3 ax = axis == 0 ? Vector3.right : (axis == 1 ? Vector3.up : Vector3.forward);
                Vector3 top = side == 1 ? Vector3.Scale(ax, max) - Vector3.Scale(ax, transform.localPosition) : Vector3.Scale(ax, min) - Vector3.Scale(ax, transform.localPosition);
                Vector3 bot = side == 1 ? Vector3.Scale(ax, min) - Vector3.Scale(ax, transform.localPosition) : Vector3.Scale(ax, max) - Vector3.Scale(ax, transform.localPosition);
                top += transform.localPosition;
                bot += transform.localPosition;
                labelInitialPosition = label.transform.position;
                basePoint = transform.TransformPoint(bot);
                topPoint = transform.TransformPoint(top);
            }
        }
        override public void Scale(float s, float total = 0)
        {
            Vector3 scale = transform.localScale;
            scale[axis] = initialScale * s;
            transform.localScale = scale;
            transform.localPosition = initialPosition + (1 - s) * (basePoint - initialPosition);
            Vector3 p = initialPosition + s * (topPoint - initialPosition);
            label.transform.position = p + labelInitialPosition - topPoint;
        }
    }
    public class PieGraphItem : GraphItem
    {
        Vector3[] peri;
        MeshFilter mf;
        public PieGraphItem(Vector3[] p, Material m)
        {
            peri = p;
            mf = gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m;
        }       
        public override void Scale(float s, float end = 0)
        {
            float d = 100f / PieGraph.Count;
            int first = Mathf.CeilToInt(s / d);
            int last = Mathf.FloorToInt(end / d);
            int before = s % d == 0 ? -1 : (first + PieGraph.Count - 1) % PieGraph.Count;
            int after = end % d == 0 ? -1 : (last + 1) % PieGraph.Count;
            int n = last - first + 1;
            int bc = before < 0 ? 0 : 1;
            int ac = after < 0 ? 0 : 1;
            Vector3[] side = new Vector3[ac + bc + n];
            if (bc == 1)
                side[0] = peri[before] + (peri[first] - peri[before]) * (s % d);
            if (ac == 1)
                side[^1] = peri[last] + (peri[after] - peri[last]) * (end % d);
            for (int i = 0; i < n; i++)
                side[i + ac] = peri[first + i];

            Vector3[] vs = new Vector3[side.Length * 4 + 2];
            Vector3[] ns = new Vector3[vs.Length];
            for (int i = 0; i < side.Length; i++)
            {
                ns[i + side.Length] = ns[i + side.Length * 2] = vs[i] = vs[i + side.Length] = side[i];
                vs[i + side.Length * 2] = vs[i + side.Length * 3] = side[i] + 0.1f * Vector3.forward;
                ns[i] = -Vector3.forward;
                ns[i + side.Length * 3] = Vector3.forward;
            }
            vs[^2] = Vector3.zero;
            vs[^1] = 0.1f * Vector3.forward;
            ns[^1] = Vector3.forward;
            ns[^2] = -Vector3.forward;

            // top, side 1, side 2, bottom
            int[] ts = new int[side.Length * 12];
            int k, tk;
            for (int i = 0; i < side.Length; i++)
            {
                k = i * 3;
                ts[k] = i;
                ts[k + 1] = i + 1;
                ts[k + 2] = vs.Length - 2;
                tk = side.Length * 3;
                ts[k + tk * 3] = i + tk;
                ts[k + tk * 3 + 1] = i + tk + 1;
                ts[k + tk * 3 + 2] = vs.Length - 1;
                tk = side.Length;
                ts[k + tk * 3] = i + tk;
                ts[k + tk * 3 + 1] = i + tk + 1;
                ts[k + tk * 3 + 2] = i + tk * 2;
                ts[k + tk * 6] = i + tk + 1;
                ts[k + tk * 6 + 1] = i + tk * 2 + 1;
                ts[k + tk * 6 + 2] = i + tk * 2;
            }
            Mesh m = mf.sharedMesh;
            m.vertices = vs;
            m.triangles = ts;
            m.normals = ns;
            mf.sharedMesh = m;
        }
    }

}