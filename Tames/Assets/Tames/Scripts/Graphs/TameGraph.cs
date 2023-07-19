using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Graphs
{
    public class TameGraph
    {
        public Markers.MarkerGraph marker;
        public List<Material> material;
        public List<GraphItem> items;
        public GameObject gameObject;
        internal float[,] values;
        internal int row, col;
        Markers.GraphType type;
        public void ExtractMaterial()
        {
            material = new List<Material>();
            MeshRenderer mr;
            Material m;
            switch (type)
            {
                case Markers.GraphType.Bar:
                    for (int i = 0; i < marker.visual.Length; i++)
                    {
                        mr = marker.visual[i].GetComponent<MeshRenderer>();
                        if (mr != null)
                            material.Add(mr.sharedMaterial);
                        else
                            material.Add(null);
                    }
                    break;
                case Markers.GraphType.Pie:
                    for (int i = 0; i < marker.colors.Length; i++)
                    {
                        m = new Material(Shader.Find("HDRP/Lit"));
                        if (m != null)
                            m.SetColor(Utils.ProperyKeywords[Tames.TameMaterial.BaseColor], marker.colors[i]);
                        material.Add(m);
                    }
                    break;
            }
        }
        public virtual void Update(int from, int to, float p)
        {

        }
        public virtual void Initialize()
        {

        }
    }
    public class BarGraph : TameGraph
    {

    }
    public class PieGraph : TameGraph
    {
        public static int Count = 100;
        override public void Initialize()
        {
            // CreateMaterials();
            ExtractMaterial();
            items = new List<GraphItem>();
            PieGraphItem pgi;
            Vector3[] peri;
            peri = new Vector3[Count];
            for (int i = 0; i < Count; i++)
                peri[i] = new Vector3(Mathf.Cos(Mathf.Deg2Rad * i / Count), Mathf.Sin(Mathf.Deg2Rad * i / Count), 0);
            for (int i = 0; i < col; i++)
            {
                pgi = new PieGraphItem(peri, i < material.Count ? material[i] : material[0]);
                items.Add(pgi);
                pgi.transform.localPosition = Vector3.zero;
                pgi.transform.parent = marker.gameObject.transform;
            }
            values = marker.GetValues(out col, out row);
            for (int j = 0; j < Count; j++)
            {
                float total = 0;
                for (int i = 0; i < col; i++)
                    total += values[i, j] == float.NaN ? 0 : values[i, j];
                for (int i = 0; i < col; i++)
                    values[i, j] = values[i, j] == float.NaN ? 0 : values[i, j] / total;
            }
        }
        override public void Update(int from, int to, float p)
        {
            float[] toV = new float[col];
            for (int i = 0; i < col; i++)
                toV[i] = values[i, from] + p * (values[i, to] - values[i, from]);
            float x = 0;
            for (int i = 0; i < col; i++)
            {
                items[i].Scale(x, i == col - 1 ? 1 : toV[i] + x);
                x += toV[i];
            }
        }
    }
}
