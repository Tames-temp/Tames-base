using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Labyrinth
{
    public class Labyrinth
    {
        public Vector2Int size = new Vector2Int(6, 6);
        public Vector2 cellSize = Vector2.one * 3;
        public float wallThickness = 0.1f;
        public float wallHeight = 2.5f;
        public bool uvLocal = true;
        public Material floorMaterial, wallMaterial;
        public GameObject walk, floor, wall;
        private int[,] connect;
        private Transform parent;
        public void Create(GameObject parent)
        {
            this.parent = parent.transform;
            wall = new GameObject();
            wall.transform.parent = parent.transform;

            connect = new int[size.x, size.y];
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                {
                    connect[i, j] = 0;
                }
            for (int i = 0; i < size.x - 1; i++)
                for (int j = 0; j < size.y - 1; j++)
                {
                    float p = UnityEngine.Random.value * 3.1f;
                    int pi = (int)p;
                    if ((pi & 1) > 0)
                    {
                        connect[i, j] += 1;
                        connect[i + 1, j] += 4;
                    }
                    if ((pi & 2) > 0)
                    {
                        connect[i, j] += 2;
                        connect[i + 1, j] += 8;
                    }
                }
            CreateFloor();
            CreateWalls();
        }
        void CreateFloor()
        {
            bool go;
            List<Vector3> vs = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> ts = new List<int>();
            Vector3[] minMax = new Vector3[] { Vector3.zero, Vector3.zero };
            Vector2 totalSize = Vector2.Scale(cellSize, size);
            int tn = 0;
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        go = (connect[i, j] & (1 << k)) > 0;
                        switch (k)
                        {
                            case 0: minMax[1].x = (i + 1) * cellSize.x + (go ? 0 : -wallThickness / 2); break;
                            case 1: minMax[1].z = (j + 1) * cellSize.y + (go ? 0 : -wallThickness / 2); break;
                            case 2: minMax[0].x = i * cellSize.x + (go ? 0 : wallThickness / 2); break;
                            case 3: minMax[0].z = j * cellSize.y + (go ? 0 : wallThickness / 2); break;
                        }
                    }
                    vs.Add(minMax[0]);
                    vs.Add(new Vector3(minMax[1].x, 0, minMax[0].z));
                    vs.Add(minMax[1]);
                    vs.Add(new Vector3(minMax[0].x, 0, minMax[1].z));
                    uvs.Add(uvLocal ? Vector2.zero : new Vector2(minMax[0].x / totalSize.x, minMax[0].z / totalSize.y));
                    uvs.Add(uvLocal ? Vector2.right : new Vector2(minMax[1].x / totalSize.x, minMax[0].z / totalSize.y));
                    uvs.Add(uvLocal ? Vector2.one : new Vector2(minMax[1].x / totalSize.x, minMax[1].z / totalSize.y));
                    uvs.Add(uvLocal ? Vector2.up : new Vector2(minMax[0].x / totalSize.x, minMax[1].z / totalSize.y));
                    ts.AddRange(new int[] { tn, tn + 1, tn + 2, tn, tn + 2, tn + 3 });
                    tn += 4;
                }
            Vector3[] v2a = vs.ToArray();
            Vector2[] uv2a = uvs.ToArray();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            int[] rep = new int[v2a.Length];
            for (int i = 0; i < v2a.Length; i++)
                rep[i] = i;
            bool found;
            for (int i = 0; i < v2a.Length; i++)
            {
                found = false;
                for (int j = 0; j < vertices.Count; j++)
                    if (Vector3.Distance(v2a[i], v2a[j]) < wallThickness / 2)
                    { rep[i] = j; found = true; break; }
                if (!found)
                {
                    rep[i] = vertices.Count;
                    vertices.Add(v2a[i]);
                    uv.Add(uv2a[i]);
                }
            }
            int[] t2a = new int[ts.Count];
            for (int i = 0; i < ts.Count; i++)
                t2a[i] = rep[ts[i]];
            Vector3[] ns = new Vector3[vertices.Count];
            for (int i = 0; i < ns.Length; i++)
                ns[i] = Vector3.up;
            floor = new GameObject("floor");
            floor.transform.parent = parent;
            MeshFilter mf = floor.AddComponent<MeshFilter>();
            Mesh m = new Mesh();
            m.vertices = vertices.ToArray();
            m.uv = uv.ToArray();
            m.triangles = t2a;
            m.normals = ns;
            m.RecalculateBounds();
            mf.sharedMesh = m;
            MeshRenderer mr = floor.AddComponent<MeshRenderer>();
            mr.sharedMaterial = floorMaterial;
        }
        void CreateWalls()
        {
            bool right, top;
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                {
                    right = (connect[i, j] & 1) == 0;
                    top = (connect[i, j] & 2) == 0;

                }
        }

    }
}
