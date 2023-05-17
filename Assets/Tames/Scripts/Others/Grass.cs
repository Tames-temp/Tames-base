using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Others
{
    public class Grass
    {
        public GameObject ground;
        public float density = 0.04f;
        public float displacement = 0.2f;
        public float maxHeight = 0.2f;
        public float minHeight = 0.1f;
        public float maxBase = 0.02f;
        public float minBase = 0.05f;
        public bool relativeThickness = false;
        public int segmentCount = 1;
        public float minBow = 0;
        public float maxBow = 0.2f;
        public bool rotated = false;
        public Material material;
        public int variantCount = 1;
        public GameObject[] plants;
        private List<Vector3> roots = new List<Vector3>();
        public Grass(GameObject g)
        {
            ground = g;
            Markers.MarkerGrass mg = ground.GetComponent<Markers.MarkerGrass>();

            density = mg.density;
            displacement = mg.randomness;
            maxHeight = mg.maxHeight;
            minHeight = mg.minHeight;
            minBase = mg.minBase;
            maxBase = mg.maxBase;
            minBow = mg.minBow;
            maxBow = mg.maxBow;
            material = mg.material;
            relativeThickness = mg.relative;
            variantCount = mg.variantCount;
            segmentCount = mg.segmentCount;
            MeshFilter mf = g.GetComponent<MeshFilter>();
            Walking.WalkObject wo = new Walking.WalkObject(g);
            if (mf != null)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3[] vg = Global(mesh.vertices);
                Vector3[] v = mesh.vertices;
                int[] t = mesh.triangles;
                Walking.WalkFace[] wf = new Walking.WalkFace[t.Length / 3];
                for (int i = 0; i < t.Length; i += 3)
                    wf[i / 3] = new Walking.WalkFace(new Vector3[] { v[t[i]], v[t[i + 1]], v[t[i + 2]] }) { control = wo };
           //     Debug.Log("GRS: " + wf.Length);
                Vector2[] mm = MinMax(vg);
                float dx, dz;
                float y;
                for (float x = mm[0].x; x <= mm[1].x; x += density)
                    for (float z = mm[0].y; z <= mm[1].y; z += density)
                    {
                        dx = (displacement * (0.5f - Random.value) * 2) * density;
                        dz = (displacement * (0.5f - Random.value) * 2) * density;
                        if ((y = On(x + dx, z + dz, wf)) != float.NegativeInfinity)
                            roots.Add(new Vector3(x + dx, y + dz, z));
                    }
            }
            CreateGrass();
        }
        public void CreateGrass()
        {
            int pc, n = roots.Count / 1000 + (roots.Count % 1000 == 0 ? 0 : 1);
            plants = new GameObject[n];
   //         Debug.Log("GRS: pl " + roots.Count + " " + plants.Length);
            for (int i = 0; i < n; i++)
            {
                pc = i == n - 1 ? roots.Count % 1000 : 1000;
                Vector3[] vs = new Vector3[pc * (segmentCount * 2 + 1)];
                Vector3[] ns = new Vector3[pc * (segmentCount * 2 + 1)];
                Vector2[] uv = new Vector2[pc * (segmentCount * 2 + 1)];
                int[] ts = new int[pc * (segmentCount * 2 - 1) * 3];
                for (int j = 0; j < pc; j++)
                    CreatePlant(roots[i * 1000 + j], j, vs, ns, uv, ts);
                plants[i] = new GameObject(ground.name + " plant " + i);
                plants[i].transform.parent = ground.transform;
                plants[i].transform.localPosition = Vector3.zero;
                plants[i].transform.localRotation = Quaternion.identity;
                MeshFilter mf = plants[i].AddComponent<MeshFilter>();
                mf.mesh = new Mesh();
                mf.mesh.vertices = Local(plants[i].transform, vs);
                mf.mesh.normals = LocalVector(plants[i].transform, ns);
                mf.mesh.uv = uv;
                mf.mesh.triangles = ts;
                mf.mesh.RecalculateBounds();
                MeshRenderer mr = plants[i].AddComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

        }
        private void CreatePlant(Vector3 root, int n, Vector3[] vs, Vector3[] ns, Vector2[] uv, int[] ts)
        {
            float angle = Random.value * Mathf.PI * 2;
            Vector3 u = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 forward = new Vector3(-u.y, 0, u.x);
            float h = Random.value * (maxHeight - minHeight) + minHeight;
            float thick = Random.value * (maxBase - minBase) + minBase;
            if (relativeThickness) thick *= h / maxHeight;
            float bow = Random.value * (maxBow - minBow) + minBow;
            float w = h * bow;
            float r = bow > 0.01 ? (h * h + w * w) / (2 * w) : -1;
            Vector3[] mid = new Vector3[segmentCount + 1];
            int k = n * (segmentCount * 2 + 1);
            int variant = Random.Range(0, variantCount);
            float uvstart = variantCount <= 1 ? 0 : variant / (float)variantCount;
            float uvend = variantCount <= 1 ? 1 : (variant + 1) / (float)variantCount;
            Vector3 center = Vector3.zero;
            //     Debug.Log("GRS: " + n + " > " + root.ToString());

            if (r > 0)
            {
                angle = w < h ? Mathf.Asin(h / r) : Mathf.PI - Mathf.Asin(h / r);
                angle /= segmentCount;
                center = root + r * forward;
                for (int i = 0; i < mid.Length; i++)
                {
                    mid[i] = -Mathf.Cos(i * angle) * r * forward + Mathf.Sin(i * angle) * r * Vector3.up + center;
                    //          Debug.Log("GRS: " + n + " > " + angle * Mathf.Rad2Deg + " " + r + " " + mid[i].ToString());
                }
            }
            else
                for (int i = 0; i < mid.Length; i++)
                    mid[i] = root + Vector3.up * i / (float)segmentCount * h;
            int ki,ti;
            for (int i = 0; i <= segmentCount; i++)
            {
                ki = k + i * 2;
                vs[ki] = mid[i] - thick * (float)(segmentCount - i) / segmentCount * u;
                //  Debug.Log("GRS: " + vs[k + i * 2].ToString());
                if (r <= 0)
                    ns[ki] = forward;
                else
                    ns[ki] = (mid[i] - center).normalized;
                uv[ki] = new Vector2(uvstart, i / (float)segmentCount);
                if (i != segmentCount)
                {
                    vs[ki+ 1] = mid[i] + thick * (float)(segmentCount - i) / segmentCount * u;
                    ns[ki + 1] = ns[k + i * 2];
                    uv[ki + 1] = new Vector2(uvend, i / (float)segmentCount);
                }
            }
            int t = n * (segmentCount * 2 - 1) * 3;
            for (int i = 0; i < segmentCount; i++)
            {
                ki = k + i * 2;
                ti = t + i * 6;
                ts[ti] = ki;
                ts[ti + 1] = ki+ 1;
                ts[ti + 2] = ki + 2;
                if (i < segmentCount - 1)
                {
                    ts[ti + 3] = ki + 1;
                    ts[ti + 4] = ki + 3;
                    ts[ti + 5] = ki + 2;
                }
            }
        }
        private float On(float x, float z, Walking.WalkFace[] wf)
        {
            Vector3 p = new Vector3(x, 0, z);
            float dy;
            for (int i = 0; i < wf.Length; i++)
            {
                if (wf[i].On(p, out dy))
                {
                    //      Debug.Log("GRS: on " + dy + " " + p.y); 
                    return p.y - dy;
                }
            }
            return float.NegativeInfinity;
        }
        private Vector3[] Global(Vector3[] v)
        {
            Vector3[] p = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
                p[i] = ground.transform.TransformPoint(v[i]);
            return p;
        }
        private Vector3[] Local(Transform t, Vector3[] v)
        {
            Vector3[] p = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
                p[i] = t.transform.InverseTransformPoint(v[i]);
            return p;
        }
        private Vector3[] LocalVector(Transform t, Vector3[] v)
        {
            Vector3[] p = new Vector3[v.Length];
            Vector3 q;
            for (int i = 0; i < v.Length; i++)
            {
                q = t.transform.InverseTransformPoint(Vector3.zero);
                p[i] = t.transform.InverseTransformPoint(v[i]);
                p[i] -= q;
            }
            return p;
        }
        private Vector2[] MinMax(Vector3[] v)
        {
            Vector2[] r = new Vector2[] { Vector2.positiveInfinity, Vector2.negativeInfinity };
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i].x < r[0].x) r[0].x = v[i].x;
                if (v[i].z < r[0].y) r[0].y = v[i].z;
                if (v[i].x > r[1].x) r[1].x = v[i].x;
                if (v[i].z > r[1].y) r[1].y = v[i].z;
            }
            return r;
        }
    }
}