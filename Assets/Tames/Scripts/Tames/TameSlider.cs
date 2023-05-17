using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    /// <summary>
    /// this class manages the movement paths for <see cref="TameHandle"/> in the form of a polygon but effect of a curve. The class is instantiated when the keyword <see cref="TameHandles.KeyPath"/> exists in the locality of the <see cref="TameElement"/>'s gameobject.
    /// </summary>
    public class TameSlider : TamePath
    {
        /// <summary>
        /// the segments on the initial geometry of the path, including the rotation vectors
        /// </summary>
        internal class Segment
        {
            /// <summary>
            /// the unique vertexes at the end of the segment
            /// </summary>
            public Vector3[] end;
            /// <summary>
            /// center of the segment
            /// </summary>
            public Vector3 center;
            /// <summary>
            /// the segments on the sides of this segment
            /// </summary>
            public Segment[] side = new Segment[] { null, null };
            /// <summary>
            /// the starting index of the triangles at the sides of this segment
            /// </summary>
            public int[] face;
            /// <summary>
            /// the average normal of the faces at the center of the segment, relative to the mover's transfrom
            /// </summary>
       //     public Vector3 normal;
            /// <summary>
            /// index of the segment
            /// </summary>
            public int index;
            public void Reverse()
            {
                Utils.Swap(end, 0, 1);
            }
        }
        /// <summary>
        /// a pointer to a specific spot on the path
        /// </summary>
        internal class IndexDelta
        {
            /// <summary>
            /// index of the local point immediately before the spot 
            /// </summary>
            public int index;
            /// <summary>
            /// the percentage distance of the spot between the index point and the next point
            /// </summary>
            public float delta;
            /// <summary>
            /// containing the initial index-delta
            /// </summary>
            public static IndexDelta Initial = new IndexDelta() { index = -1 };
            public static IndexDelta Zero = new IndexDelta() { index = 0, delta = 0 };
            public IndexDelta() { }
            public IndexDelta(int i, float d) { index = i; delta = d; }
        }

        /// <summary>
        /// the game object representing the path
        /// </summary>
        public GameObject gameObject;
        /// <summary>
        /// points on the path, local to element's transform (mover's parent)
        /// </summary>
        private Vector3[] point;
        private Vector3[] toEdge;
        /// <summary>
        /// vectors between <see cref="point"/>s 
        /// </summary>
        private Vector3[] vector;
        /// <summary>
        /// whether the path is closed or not.
        /// </summary>
        private bool closed = false;
        /// <summary>
        /// segments of the path, fron which the points and vectors are derived
        /// </summary>
        private Segment[] segment;
        /// <summary>
        /// unique vertices, used to make sure triangles refer to the same vertex
        /// </summary>
        private Vector3[] U;

        private int[] replace, T, oT;
        private int division = 128;
        private float delta;
        //     private Transform auxParent, auxChild;
         public Mesh mesh;
        public TameSlider(Transform mover, Vector3 from, Vector3 to)
        {
            this.mover = mover;
            closed = false;
            gameObject = new GameObject("_path");
            self = gameObject.transform;
            gameObject.transform.parent = mover.transform.parent;
            gameObject.transform.position = mover.transform.parent.position;
            gameObject.transform.rotation = Quaternion.identity;
            vector = new Vector3[2];
            Vector3 a = gameObject.transform.InverseTransformPoint(mover.parent.TransformPoint(from));
            Vector3 b = gameObject.transform.InverseTransformPoint(mover.parent.TransformPoint(to));
            vector[0] = vector[1] = b - a;
            point = new Vector3[2];
            point[0] = a;
            point[1] = b;
            Vector3 u = Utils.Perp(vector[0]).normalized;
            toEdge = new Vector3[] { u, u };
        }
        /// <summary>
        /// creates a path from a game object.
        /// </summary>
        /// <param name="pathObject">the path's game object</param>
        /// <param name="mover">the mover game object</param>
        /// <param name="from">a point that is closest to the beginning of a closed path, or the firts point of the path's direction</param>
        /// <param name="to">the second point of the path's direction vector</param>

        public TameSlider(GameObject pathObject, GameObject mover, Vector3 from, Vector3 to)
        {
            try
            {
                gameObject = new GameObject(pathObject.name + "-clone");
                self = gameObject.transform;
                pathObject.SetActive(false);
                if (mover != null)
                {
                    this.mover = mover.transform;
                    gameObject.transform.parent = mover.transform.parent;
                }
                else
                    gameObject.transform.parent = pathObject.transform.parent;
                gameObject.transform.position = pathObject.transform.position;
                gameObject.transform.rotation = pathObject.transform.rotation;
                //     parent = g.transform.parent;
                MeshFilter mf = pathObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mesh = mf.sharedMesh;
                    Vector3[] v = mesh.vertices;
                    //  Vector3[] n = mesh.normals;
                    T = mesh.triangles;
                    oT = mesh.triangles;
                    replace = new int[v.Length];
                    U = Utils.GetUnique(v, replace).ToArray();
                    List<Segment> segments = new List<Segment>();
                    for (int i = 0; i < T.Length; i++)
                        T[i] = replace[T[i]];
                    int[] ms;
                    for (int i = 0; i < T.Length - 3; i += 3)
                        for (int j = i + 3; j < T.Length; j += 3)
                            if (i != j)
                                if ((ms = Shared(T, i, j)) != null)
                                {
                                    segments.Add(new Segment()
                                    {
                                        end = new Vector3[] { U[T[ms[0]]], U[T[ms[1]]] },
                                        center = (U[T[ms[0]]] + U[T[ms[1]]]) / 2,
                                        //                       normal = (n[oT[ms[0]]] + n[oT[ms[1]]] + n[oT[ms[2]]] + n[oT[ms[3]]]) / 4,
                                        face = new int[] { i, j },
                                        index = segments.Count,
                                    });
                                }
                    CreateNeighbors(segments);
                    AddEnds(segments);
                    int index = Closest(from, segments);
                    segments = Clean(segments, index, out int ni);
                    index = ni;
                    Order(segments, index, to);
                    CreateSteps();
           
                         valid = true;
                }
            }
            catch (Exception)
            {
                Debug.Log("error ");
            }
        }
        public Vector3 VectorOn(Vector3 p)
        {
            IndexDelta id = Closest(p);
            Vector3 v = Vector(id);
             return v.normalized;
        }
        override public void AssignMovers(GameObject[] g, bool def = false)
        {
            bases = new Transform[g.Length];
            attached = new Transform[g.Length];
            for (int i = 0; i < g.Length; i++)
            {
                attached[i] = g[i].transform;
                bases[i] = new GameObject(gameObject.name + "-" + i).transform;
                bases[i].parent = gameObject.transform;
                IndexDelta id = def ? IndexDelta.Zero : Closest(g[i].transform.position);
                bases[i].localPosition = Position(id);
                bases[i].localRotation = facing == FacingLogic.Free ? Rotation(id) : Quaternion.identity;
                Vector3 p = g[i].transform.position;
                Quaternion q = g[i].transform.rotation;
                g[i].transform.parent = bases[i].transform;
                g[i].transform.position = p;
                g[i].transform.rotation = q;
            }
        }

        override public void AssignMoverBasis(GameObject g)
        {
            mover = g.transform;
            IndexDelta id = Closest(g.transform.position);
            moverBase = new GameObject(gameObject.name + "-base").transform;
            moverBase.transform.parent = gameObject.transform;
            moverBase.transform.localPosition = Position(id);
            moverBase.transform.localRotation = facing == FacingLogic.Free ? Rotation(id) : Quaternion.identity;
            Vector3 p = g.transform.position;
            Quaternion q = g.transform.rotation;
            g.transform.parent = moverBase.transform;
            g.transform.position = p;
            g.transform.rotation = q;
        }
        override public GameObject Clone(int i, float m)
        {
            IndexDelta id = GetID(m);
            bases[i] = new GameObject(gameObject.name + "-" + i).transform;
            bases[i].transform.parent = gameObject.transform;

            bases[i].transform.localPosition = Position(id);
            bases[i].transform.localRotation = facing == FacingLogic.Free ? Rotation(id) : Quaternion.identity;
            attached[i] = GameObject.Instantiate(mover, bases[i].transform);
            attached[i].transform.localPosition = mover.transform.localPosition;
            attached[i].transform.localRotation = mover.transform.localRotation;
            return attached[i].gameObject;
        }
        override public void SetInitial(float mold, float mnew)
        {
            for (int i = 0; i < bases.Length; i++)
            {
                IndexDelta nid = GetID(mnew);
                bases[i].transform.localPosition = Position(nid);
                if (facing == FacingLogic.Free)
                    bases[i].transform.localRotation = Rotation(nid);
            }
        }
        public override void MoveLinked(float m)
        {
            float mi;
            if (linked != null)
                for (int i = 0; i < linked.Length; i++)
                {
                    mi = element.progress.FakeByOffset(linkOffset[i]);
                    IndexDelta id = GetID(mi);
                    Vector3 p = Position(id);
                    linked[i].transform.localPosition = p;
                    if (facing == FacingLogic.Free)
                        linked[i].transform.localRotation = Rotation(id);
                }
        }
        override public void Move(int index, float m)
        {
            IndexDelta id = GetID(m);
            Vector3 p = Position(id);
            bases[index].transform.localPosition = p;
            if (facing == FacingLogic.Free)
                bases[index].transform.localRotation = Rotation(id);
        }
        private Vector3 Vector(IndexDelta id)
        {
            return id.index < 0 ? vector[0] : (1 - id.delta) * vector[id.index] + id.delta * vector[(id.index + 1) % point.Length];
        }
        private Vector3 Segtor(IndexDelta id)
        {
            //  Debug.Log(mover.parent.name+ " "+id.index + " " + id.delta+ " ");
            Vector3 a = id.index < 0 ? toEdge[0] : (1 - id.delta) * toEdge[id.index] + id.delta * toEdge[(id.index + 1) % point.Length];
            return a.normalized;

        }

        private IndexDelta GetID(float m)
        {
            float ipd = m * (closed ? point.Length : point.Length - 1);
            int i = (int)ipd;
            if (i < 0) i = 0;
            if (closed && (i > point.Length - 1)) i = point.Length - 1;
            if ((!closed) && (i > point.Length - 2)) i = point.Length - 2;
            float d = ipd - i;
            if (d > 1) d = 1;
            return new IndexDelta(i, d);
        }
        public override Vector3 Position(float m)
        {
            return Position(GetID(m));
        }
        public override Quaternion Rotation(float m)
        {
            return Rotation(GetID(m));
        }
        private Quaternion Rotation(IndexDelta id)
        {
            //      Vector3 p = Position(id);
            Vector3 w = Vector(id);
            Vector3 u = Segtor(id);
            Vector3 v = Vector3.Cross(w, u);
            return Quaternion.LookRotation(w, v);
        }
        private Vector3 Position(IndexDelta id)
        {
            return id.index < 0 ? point[0] : point[id.index] + id.delta * (point[(id.index + 1) % point.Length] - point[id.index]);
        }
        override public float GetM(Vector3 global)
        {
            IndexDelta id = Closest(global);
            return (id.index + id.delta) / (closed ? point.Length : point.Length - 1);
        }
        private IndexDelta Closest(Vector3 global)
        {
            Vector3 p = gameObject.transform.InverseTransformPoint(global);
            int index = 0;
            float d = 0;
            int k;
            float dist, min = Vector3.Distance(point[0], p);
            for (int i = 1; i < point.Length; i++)
                if ((dist = Vector3.Distance(point[i], p)) < min)
                { min = dist; index = i; }
            float m, n;
            Vector3[] ps = new Vector3[] { point[index], Vector3.positiveInfinity, Vector3.positiveInfinity };
            if (closed)
            {
                m = Utils.M(point[index], p, -vector[(index + point.Length - 1) % point.Length]);
                n = Utils.M(point[index], p, vector[index]);
                k = 0;
                if (m >= 0) ps[1] = point[index] - m * vector[(index + point.Length - 1) % point.Length];
                if (n >= 0) ps[2] = point[index] + n * vector[index];
                for (int i = 1; i < 3; i++)
                    if ((dist = Vector3.Distance(ps[i], p)) < min)
                    { k = i; min = dist; }
                switch (k)
                {
                    case 1: index = (index + point.Length - 1) % point.Length; d = 1 - m; break;
                    case 2: d = n; break;
                }
            }
            else
            {
                if (index == 0)
                {
                    n = Utils.M(point[index], p, vector[index]);
                    if (n >= 0) ps[1] = point[index] + n * vector[index];
                    if (Vector3.Distance(ps[1], p) < min)
                        d = n;

                }
                else if (index == point.Length - 1)
                {
                    m = Utils.M(point[index], p, -vector[index - 1]);
                    if (m >= 0) ps[1] = point[index] - m * vector[index];
                    if (Vector3.Distance(ps[1], p) < min)
                    { d = 1 - m; index--; }
                }
                else
                {
                    n = Utils.M(point[index], p, vector[index]);
                    m = Utils.M(point[index], p, vector[index - 1]);
                    k = 0;
                    if (m >= 0) ps[1] = point[index] + m * vector[index - 1];
                    if (n >= 0) ps[2] = point[index] + n * vector[index];
                    for (int i = 1; i < 3; i++)
                        if ((dist = Vector3.Distance(ps[i], p)) < min)
                        { k = i; min = dist; }
                    switch (k)
                    {
                        case 1: index--; d = 1 - m; break;
                        case 2: d = n; break;
                    }
                }
            }
            return new IndexDelta(index, d);
        }
        private void CreateSteps()
        {
            float l = 0;
            float passed = 0, r;
            int n = 1;
            Vector3 currentP, currentE;
            List<Vector3> p = new List<Vector3>();
            List<Vector3> e = new List<Vector3>();
            //    List<Vector3> normals = new List<Vector3>();
            for (int i = 0; i < segment.Length - 1; i++)
                for (int j = i + 1; j < segment.Length; j++)
                    if ((segment[i].end[0] == segment[j].end[1]) || (segment[i].end[1] == segment[j].end[0]))
                        segment[j].Reverse();
            float li, minL = float.PositiveInfinity;
            for (int i = 0; i < segment.Length; i++)
                if (i < segment.Length - 1)
                {
                    l += li = Vector3.Distance(segment[i].center, segment[i + 1].center);
                    if ((li < minL) && (li > 0)) minL = li;
                }
                else if (closed)
                {
                    l += li = Vector3.Distance(segment[i].center, segment[0].center);
                    if ((li < minL) && (li > 0)) minL = li;
                }
            division = (int)(l / minL);
            if (division > 1024) division = 1024;
            delta = l / division;
            p.Add(segment[0].center);
            e.Add(segment[0].end[0]);
            //   normals.Add(segment[0].normal);
            Vector3 v, ve;
            float waiting = delta;
            float dl = 0;
            for (int i = 0; i < segment.Length; i++)
                if ((i < segment.Length - 1) || closed)
                {
                    v = segment[(i + 1) % segment.Length].center - segment[i].center;
                    ve = segment[(i + 1) % segment.Length].end[0] - segment[i].end[0];
                    while (true)
                    {

                        if (waiting > v.magnitude - passed)
                        {
                            dl += v.magnitude - passed;
                            waiting -= v.magnitude - passed;
                            passed = 0;
                            break;
                        }
                        else
                        {
                            passed += waiting;
                            dl += waiting;
                            waiting = delta;
                            n++;
                            r = passed / v.magnitude;
                            currentP = segment[i].center + r * v;
                            currentE = segment[i].end[0] + r * ve;
                            p.Add(currentP);
                            e.Add(currentE);
                            //             normals.Add((1 - r) * segment[i].normal + r * segment[(i + 1) % segment.Length].normal);
                        }
                    }
                }
            if ((!closed) && (n <= division))
            {
                p.Add(segment[segment.Length - 1].center);
                e.Add(segment[segment.Length - 1].end[0]);
                //        normals.Add(segment[segment.Length - 1].normal);
            }
            point = p.ToArray();
            toEdge = e.ToArray();
            for (int i = 0; i < point.Length; i++)
                toEdge[i] -= point[i];
            //    normal = normals.ToArray();
            vector = new Vector3[point.Length];
            for (int i = 0; i < point.Length; i++)
                if (closed)
                    vector[i] = point[(i + 1) % point.Length] - point[(i - 1 + point.Length) % point.Length];
                else
                {
                    if (i == 0) vector[i] = point[1] - point[0];
                    else if (i == point.Length - 1) vector[i] = point[i] - point[i - 1];
                    else vector[i] = point[i + 1] - point[i - 1];
                }
        }

        private int Closest(Vector3 p, List<Segment> s)
        {
            int r = 0;
            float d, min = Vector3.Distance(s[0].center, p);
            for (int i = 1; i < s.Count; i++)
                if ((d = Vector3.Distance(s[i].center, p)) < min)
                { min = d; r = i; }
            return r;
        }
        private void CreateNeighbors(List<Segment> seg)
        {
            for (int i = 0; i < seg.Count - 1; i++)
                for (int j = i + 1; j < seg.Count; j++)
                    for (int k = 0; k < 4; k++)
                        if (seg[i].face[k / 2] == seg[j].face[k % 2])
                        {
                            seg[i].side[k / 2] = seg[j];
                            seg[j].side[k % 2] = seg[i];
                            break;
                        }
        }

        private int[] Shared(int[] t, int i, int j)
        {
            for (int m = 0; m < 3; m++)
                for (int n = 0; n < 3; n++)
                    if (((t[i + m] == t[j + n]) && (t[i + (m + 1) % 3] == t[j + (n + 1) % 3]))
                        || ((t[i + (m + 1) % 3] == t[j + n]) && (t[i + m] == t[j + (n + 1) % 3])))
                        return new int[] { i + m, i + (m + 1) % 3, j + n, j + (n + 1) % 3 };
            return null;
        }
        private List<Segment> Clean(List<Segment> a, int index, out int o)
        {
            List<Segment> r = new List<Segment>() { a[index] };
            Segment tmp, current = a[index];
            int next = 1;
            o = 0;
            for (int i = 0; i < a.Count; i++)
                if (current != null)
                {
                    if (current.side[next] != null)
                    {
                        tmp = current.side[next];
                        r.Add(tmp);
                        if (tmp.side[0] == current) next = 1; else next = 0;
                        current = tmp;
                        if (current == a[index])
                        {
                            closed = true;
                            return r;
                        }
                    }
                }
                else break;
            current = a[index];
            next = 0;
            for (int i = 0; i < a.Count; i++)
                if (current != null)
                {
                    if (current.side[next] != null)
                    {
                        tmp = current.side[next];
                        r.Insert(0, tmp);
                        o++;
                        if (tmp.side[0] == current) next = 1; else next = 0;
                        current = tmp;
                        if (current == a[index])
                            return r;
                    }
                }
                else break;
            return r;
        }
        private void AddEnds(List<Segment> a)
        {
            int[] tcount = new int[T.Length / 3];
            for (int i = 0; i < tcount.Length; i++)
                tcount[i] = 0;
            for (int i = 0; i < a.Count; i++)
            {
                tcount[a[i].face[0] / 3]++;
                tcount[a[i].face[1] / 3]++;
            }
            List<Segment> ends = new List<Segment>();
            for (int i = 0; i < a.Count; i++)
                if ((tcount[a[i].face[0] / 3] == 1) || (tcount[a[i].face[1] / 3] == 1))
                {
                    ends.Add(a[i]);
                }
            int Tend, jj, ts;
            Vector3 u, w, p, q;
            Segment s;
            for (int i = 0; i < ends.Count; i++)
            {
                Tend = tcount[ends[i].face[0] / 3] == 1 ? 0 : 1;
                u = ends[i].center - ends[i].side[1 - Tend].center;
                jj = 0;
                for (int j = 0; j < 3; j++) if (U[T[ends[i].face[Tend] + j]] == ends[i].end[0]) jj += j;
                for (int j = 0; j < 3; j++) if (U[T[ends[i].face[Tend] + j]] == ends[i].end[1]) jj += j;
                w = U[T[ends[i].face[Tend] + 3 - jj]];
                p = (w + ends[i].end[0]) / 2;
                q = (w + ends[i].end[1]) / 2;
                if (Vector3.Angle(u, p - ends[i].center) < Vector3.Angle(u, q - ends[i].center)) ts = 0; else ts = 1;
                s = new Segment()
                {
                    end = new Vector3[] { ends[i].end[ts], w },
                    face = new int[] { ends[i].face[Tend], -1 },
                    center = (ends[i].end[ts] + w) / 2,
                    side = new Segment[] { ends[i], null },
                    index = a.Count
                };
                ends[i].side[Tend] = s;
                a.Add(s);
            }
        }
        private void Order(List<Segment> a, int index, Vector3 to)
        {
            segment = new Segment[a.Count];
            int tindex = Closest(to, a);
            if (closed)
            {
                if (Mathf.Abs(tindex - index) <= a.Count / 2)
                    for (int i = 0; i < a.Count; i++)
                        segment[i] = a[i];
                else
                    for (int i = a.Count; i > 0; i--)
                        segment[a.Count - i] = a[i % a.Count];
            }
            else
            {
                if (tindex >= index)
                    for (int i = 0; i < a.Count; i++)
                        segment[i] = a[i];
                else
                    for (int i = a.Count - 1; i >= 0; i--)
                        segment[a.Count - i - 1] = a[i];

            }
        }
        public override TamePath Clone(GameObject owner, GameObject mover, LinkedKeys lt)
        {
            TameSlider ts;
            if (segment.Length == 2)
                ts = new TameSlider(mover.transform, start, end);
            else
            {
                //      Debug.Log("before: " + owner.name);
                GameObject path = new GameObject();
                path.transform.parent = owner.transform;
                path.transform.localPosition = gameObject.transform.localPosition;
                path.transform.localRotation = gameObject.transform.localRotation;
                MeshFilter mf = path.AddComponent<MeshFilter>();
                Mesh m = new Mesh()
                {
                    vertices = mesh.vertices,
                    triangles = mesh.triangles
                };
                mf.mesh = m;
                ts = new TameSlider(path, mover, start, end);
                //        Debug.Log("after " + owner.name);
            }
            //       Debug.Log("after " + (ts.point == null));
            ts.attached = new Transform[attached.Length];
            ts.bases = new Transform[bases.Length];
            ts.facing = facing;
            ts.parent = owner.transform;
            if (lt == LinkedKeys.None)
            {
                ts.bases[0] = new GameObject().transform;
                ts.bases[0].parent = ts.gameObject.transform;
                ts.bases[0].localPosition = bases[0].localPosition;
                ts.bases[0].localRotation = bases[0].localRotation;
                ts.attached[0] = mover.transform;
                ts.attached[0].parent = ts.bases[0];
                ts.attached[0].localPosition = attached[0].localPosition;
                ts.attached[0].localRotation = attached[0].localRotation;

            }
            else
                for (int i = 0; i < bases.Length; i++)
                {
                    ts.bases[i] = new GameObject().transform;
                    ts.bases[i].parent = ts.gameObject.transform;
                    ts.bases[i].localPosition = bases[i].localPosition;
                    ts.bases[i].localRotation = bases[i].localRotation;
                    ts.attached[i] = GameObject.Instantiate(attached[i]);
                    ts.attached[i].parent = ts.bases[i];
                    ts.attached[i].localPosition = attached[i].localPosition;
                    ts.attached[i].localRotation = attached[i].localRotation;
                }
            return ts;
        }
    }
}
