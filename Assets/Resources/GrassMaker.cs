using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class GrassMaker
    {
        public GameObject gameObject;
        public List<GameObject> grass;
        public Material grassMaterial;
        public float dentsity = 100;
        public float height = 0.2f;
        public float width = 0;
        public int segment = 1;
        public GrassMaker(GameObject go, Material m, float h, float d, int s)
        {

            gameObject = go;
            grassMaterial = m;
            dentsity = d;
            segment = s;
            height = h;
            width = height / 10;
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3[] v = mesh.vertices;
                for (int i = 0; i < v.Length; i++)
                    v[i] = go.transform.TransformPoint(v[i]);
                Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
                for (int i = 0; i < v.Length; i++)
                {
                    if (v[i].x < min.x) min.x = v[i].x;
                    if (v[i].z < min.z) min.z = v[i].z;
                    if (v[i].x > max.x) max.x = v[i].x;
                    if (v[i].z > max.z) max.z = v[i].z;
                }
                int[] t = mesh.triangles;   
                float margin = 1 / dentsity * 0.3f;
                int col = (int)((max.x - min.x) * dentsity);
                int row = (int)((max.y - min.y) * dentsity);
                float[,] exist = new float[col, row];
                int c = 0;
                for (int i = 0; i < col; i++)
                    for (int j = 0; j < row; j++)
                    {
                        exist[i, j] = OnFace(v, t, min + new Vector3(i / dentsity, 0, j / dentsity));
                        c++;
                    }
                grass = new List<GameObject>();
                grass.Add(new GameObject("grass " + 0));
                mf = grass[0].AddComponent<MeshFilter>();
                grass[0].transform.parent = gameObject.transform;
                MeshRenderer mr = grass[0].AddComponent<MeshRenderer>();
                c = 0;
                Mesh gm = mf.mesh;
                List<Vector3> vs = new List<Vector3>();
                List<int> ts = new List<int>();
                List<Vector3> ns = new List<Vector3>();
                List<Vector2> us = new List<Vector2>();
                Vector3 p, q, r, dx, dy;
                float maxDisp = width * 2, disp;
                float ang, dk;
                int gi = 0;
                Vector3[] vseg = new Vector3[segment * 2 + 1];
                for (int i = 0; i < col; i++)
                    for (int j = 0; j < row; j++)
                        if (exist[i, j] != float.NegativeInfinity)
                        {
                            ang = UnityEngine.Random.Range(0f, Mathf.PI);
                            disp = UnityEngine.Random.Range(0f, 1f);
                            p = new Vector3(min.x + i / dentsity, height + exist[i,j], min.y + j / dentsity);
                            dx = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                            dy = new Vector3(-Mathf.Sin(ang), 0, Mathf.Cos(ang));
                            us.Add(Vector2.zero);
                            vs.Add(p);
                            vseg[0] = p;
                            for (int k = 0; k < segment; k++)
                            {
                                dk = (k + 1) / (float)segment;
                                q = vseg[k * 2 + 1] = p - dx * dk - dy * disp * dk - Vector3.up * height * dk;
                                r = vseg[k * 2 + 2] = p + dx * dk + dy * disp * dk - Vector3.up * height * dk;
                                vs.Add(q);
                                vs.Add(r);
                                if (k == 0)
                                {
                                    ts.Add(c); ts.Add(c + 1); ts.Add(c + 2);
                                    us.Add(dk * Vector2.up); us.Add(new Vector2(dk, 1));
                                }
                                else
                                {
                                    ts.Add(c - 1); ts.Add(c); ts.Add(c + 1);
                                    ts.Add(c); ts.Add(c + 2); ts.Add(c + 1);
                                    us.Add(dk * Vector2.up); us.Add(new Vector2(dk, 1));
                                }
                            }
                            ns[0] = Vector3.Cross(vseg[0] - p, vseg[1] - p);
                            for (int k = 0; k < segment; k++)
                            {
                                if (k < segment - 1)
                                {
                                    ns[k * 2 + 1] = ns[k * 2 + 2] = Vector3.Cross(vseg[k * 2 + 3] - vseg[k * 2 + 2], vseg[k * 2 + 2] - vseg[k * 2]);
                                }
                                else
                                    ns[k * 2 + 1] = ns[k * 2 + 2] = ns[k * 2];
                            }
                            c += vseg.Length;
                            if (c > 1000)
                            {
                                gm.vertices = vs.ToArray();
                                gm.normals = ns.ToArray();
                                gm.triangles = ts.ToArray();
                                gm.uv = us.ToArray();
                                vs.Clear();
                                ns.Clear();
                                ts.Clear();
                                us.Clear();
                                gm.RecalculateBounds();
                                mr.material = grassMaterial;
                                gi++;
                                grass.Add(new GameObject("grass " + gi));
                                grass[gi].transform.parent = gameObject.transform;
                                mf = grass[gi].AddComponent<MeshFilter>();
                                gm = mf.sharedMesh;
                                mr = grass[gi].GetComponent<MeshRenderer>();
                                c = 0;
                            }
                        }
                if (c > 0)
                {
                    gm.vertices = vs.ToArray();
                    gm.normals = ns.ToArray();
                    gm.triangles = ts.ToArray();
                    gm.uv = us.ToArray();
                    vs.Clear();
                    ns.Clear();
                    ts.Clear();
                    us.Clear();
                    gm.RecalculateBounds();
                    mr.material = grassMaterial;
                    gi++;
                    grass.Add(new GameObject("grass " + gi));
                    mf = grass[gi].AddComponent<MeshFilter>();
                    gm = mf.sharedMesh;
                    mr = grass[gi].GetComponent<MeshRenderer>();
                    c = 0;
                }
            }
        }
        private float OnFace(Vector3[] v, int[] t, Vector3 p)
        {
            return float.NegativeInfinity;
        }
    }
}

