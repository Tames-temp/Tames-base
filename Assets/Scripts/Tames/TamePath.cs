using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameRotator
    {
        public Vector3[] axis = new Vector3[] { Vector3.negativeInfinity, Vector3.negativeInfinity };
        public float[] angle = new float[] { 0, 0 };
        public static void Rotate(Transform t, Quaternion initial, TameRotator r)
        {
            if (r == null) return;
            t.localRotation = initial;
            if (r.axis[0].x != float.NegativeInfinity) t.Rotate(r.axis[0], r.angle[0], Space.World);
            if (r.axis[1].x != float.NegativeInfinity) t.Rotate(r.axis[1], r.angle[1], Space.World);
        }
    }    /// <summary>
         /// this class manages the movement paths for <see cref="TameHandle"/> in the form of a polygon but effect of a curve. The class is instantiated when the keyword <see cref="TameHandles.KeyPath"/> exists in the locality of the <see cref="TameElement"/>'s gameobject.
         /// </summary>
    public class TamePath
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
            public Vector3 normal;
            /// <summary>
            /// index of the segment
            /// </summary>
            public int index;
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
            public IndexDelta() { }
            public IndexDelta(int i, float d) { index = i; delta = d; }
        }

        private Transform parent;
        /// <summary>
        /// the game object representing the path
        /// </summary>
        private GameObject gameObject;
        /// <summary>
        /// the mover object in the handle
        /// </summary>
        private GameObject mover;
        /// <summary>
        /// the handle
        /// </summary>
        public TameHandles handle;
        /// <summary>
        /// points on the path, local to element's transform (mover's parent)
        /// </summary>
        private Vector3[] point;
        /// <summary>
        /// vectors between <see cref="point"/>s 
        /// </summary>
        private Vector3[] vector;
        /// <summary>
        /// vector perpendicular to the path on each <see cref="point"/> (local to mover's transform)
        /// </summary>
        private Vector3[] normal;
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
        private const int Division = 128;
        private float delta;
        private Transform auxParent, auxChild;
        public bool valid = false;
        /// <summary>
        /// creates a path from a game object.
        /// </summary>
        /// <param name="g">the path's game object</param>
        /// <param name="mover">the mover game object</param>
        /// <param name="from">a point that is closest to the beginning of a closed path, or the firts point of the path's direction</param>
        /// <param name="to">the second point of the path's direction vector</param>
        public TamePath(GameObject g, GameObject mover, Vector3 from, Vector3 to)
        {
            try
            {
                gameObject = g;
                parent = g.transform.parent;
                this.mover = mover;
                MeshFilter mf = g.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    Mesh mesh = mf.sharedMesh;
                    Vector3[] v = mesh.vertices;
                    Vector3[] n = mesh.normals;
                    T = mesh.triangles;
                    oT = mesh.triangles;
                    replace = new int[v.Length];
                    for (int i = 0; i < v.Length; i++)
                    {
                        v[i] = Utils.LocalizePoint(v[i], g.transform, parent);
                        n[i] = Utils.LocalizeVector(n[i], g.transform, parent).normalized;
                    }
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
                                        normal = (n[oT[ms[0]]] + n[oT[ms[1]]] + n[oT[ms[2]]] + n[oT[ms[3]]]) / 4,
                                        face = new int[] { i, j },
                                        index = segments.Count,
                                    });
                                }
                    CreateNeighbors(segments);
                    AddEnds(segments, n);
                    int index = Closest(from, segments);
                    segments = Clean(segments, index, out int ni);
                    index = ni;
                    Order(segments, index, to);
                    CreateSteps();
                    LocalizeToMover();
                    GameObject gaux = new GameObject();
                    auxParent = gaux.transform;
                    auxParent.parent = g.transform.parent;
                    // auxParent.parent = mover.transform;
                    gaux = new GameObject();
                    auxChild = gaux.transform;
                    auxChild.parent = auxParent;
                    valid = true;
                }
            }
            catch { }
        }
        private Vector3 Vector(IndexDelta id)
        {
            return id.index < 0 ? vector[0] : (1 - id.delta) * vector[id.index] + id.delta * vector[(id.index + 1) % point.Length];
        }
        private Vector3 Normal(IndexDelta id)
        {
            return id.index < 0 ? normal[0] : (1 - id.delta) * normal[id.index] + id.delta * normal[(id.index + 1) % point.Length];
        }
        private Vector3 Local(IndexDelta id)
        {
            return id.index < 0 ? point[0] : point[id.index] + id.delta * (point[(id.index + 1) % point.Length] - point[id.index]);
        }
        public void PR(float m, out Vector3 position, out TameRotator rotator)
        {
            IndexDelta id = GetID(m);
            position = Position(id);
            rotator = Rotator(IndexDelta.Initial, id, handle.up);

        }
        public void PR(float m, float initial, out Vector3 position, out TameRotator rotator)
        {
            IndexDelta id1 = GetID(m);
            IndexDelta id0 = initial == 0 ? IndexDelta.Initial : GetID(initial);
            position = Position(id1);
            rotator = Rotator(id0, id1, handle.up);
        }

        public float PR(Vector3 p, out Vector3 position)
        {
            IndexDelta id = Closest(p);
            position = Position(id);
            return (id.index + id.delta) / (closed ? point.Length + 1 : point.Length);
        }
        /// <summary>
        /// outputs the position and rotation route <see cref="GetRotation"/> at a specified progress, relative to the beginning of the path
        /// </summary>
        /// <param name="m">the progress</param>
        /// <param name="position">position output, relative to element's transform</param>
        /// <param name="rotation">rotation route</param>
        public void PandR(float m, out Vector3 position, out Vector3[] rotation)
        {
            IndexDelta id = GetID(m);
            position = Position(id);
            rotation = Rotation(IndexDelta.Initial, id);
        }
        /// <summary>
        /// outputs the position and rotation route <see cref="GetRotation"/> at a specified progress, relative to the initial progress
        /// </summary>
        /// <param name="m">the progress</param>
        /// <param name="initial">the initial progress</param>
        /// <param name="position">position output, relative to element's transform</param>
        /// <param name="rotation">rotation route</param>
        public void PandR(float m, float initial, out Vector3 position, out Vector3[] rotation)
        {
            IndexDelta id1 = GetID(m);
            IndexDelta id0 = initial == 0 ? IndexDelta.Initial : GetID(initial);
            position = Position(id1);
            rotation = Rotation(id0, id1);
        }

        public float PandR(Vector3 p, out Vector3 position)
        {
            IndexDelta id = Closest(p);
            position = Position(id);
            return (id.index + id.delta) / (closed ? point.Length + 1 : point.Length);
        }
        private IndexDelta GetID(float m)
        {
            float ipd = m * (closed ? point.Length + 1 : point.Length);
            int i = (int)ipd;
            if (i < 0) i = 0;
            if (closed && (i > point.Length - 1)) i = point.Length - 1;
            if ((!closed) && (i > point.Length - 2)) i = point.Length - 2;
            float d = ipd - i;
            if (d > 1) d = 1;
            return new IndexDelta(i, d);
        }
        private Vector3 Position(IndexDelta id)
        {
            return Local(id);
        }
        private bool IsRotationValid(Vector3 a, Vector3 b, Vector3 u)
        {
            float A = Mathf.Abs(Vector3.Angle(a, u) - 90);
            float B = Mathf.Abs(Vector3.Angle(b, u) - 90);
            return (A > 1) && (B > 1);
        }
        private bool IsRotationValid(Vector3 a, Vector3 u, Vector3 b, Vector3 v)
        {
            float A = Mathf.Abs(Vector3.Angle(a, u) - 90);
            float B = Mathf.Abs(Vector3.Angle(b, v) - 90);
            return (A > 1) && (B > 1);
        }

        private TameRotator Rotator(IndexDelta initial, IndexDelta id, Vector3 up)
        {
            Vector3 v0, n0, v1, n1, w0, w1, f, p, q, u;
            bool hasUp = up.x != float.NegativeInfinity;
            v0 = Vector(initial);
            v1 = Vector(id);
            n0 = Normal(initial);
            n1 = Normal(id);
            v0 = Utils.LocalizeVector(v0, parent, null);
            v1 = Utils.LocalizeVector(v1, parent, null);
            n0 = Utils.LocalizeVector(n0, parent, null);
            n1 = Utils.LocalizeVector(n1, parent, null);
            w0 = Vector3.Cross(v0, n0).normalized;

            TameRotator r = new TameRotator();
            if (hasUp)
            {
                u = Utils.LocalizeVector(up, parent, null);
                v0 = Utils.On(v0, Vector3.zero, u);
                v1 = Utils.On(v1, Vector3.zero, u);
                v0.Normalize();
                v1.Normalize();
            }
            else
                u = w0;
        //    Debug.Log("hasup: " + hasUp +" "+handle.up.ToString()+ " " + r.angle[0] + " " + r.axis[0].ToString());


            float a0;
            if ((v0.magnitude > 0.001f) && (v1.magnitude > 0.001f))
            {
                //      w1 = Vector3.Cross(v1, n1).normalized;
                a0 = Vector3.Angle(v0, v1);
                if (a0 > 0.1)
                {
                    if (a0 < 179.9)
                    {
                        r.axis[0] = hasUp ? u : Vector3.Cross(v0, v1).normalized;
                        r.angle[0] = Utils.SignedAngle(v1, Vector3.zero, v0, r.axis[0]);
                     }
                    else
                    {
                        r.axis[0] = u;
                        r.angle[0] = 180;
                    }
                }
            }
            if (r.axis[0].x != float.NegativeInfinity)
            {
                n0 = Utils.Rotate(n0, Vector3.zero, r.axis[0], r.angle[0]);
                if (hasUp)
                {
                    n0 = Utils.On(n0, Vector3.zero, u);
                    n1 = Utils.On(n1, Vector3.zero, u);
                }
                else
                    u = Utils.Rotate(u, Vector3.zero, r.axis[0], r.angle[0]);
            }
            a0 = Vector3.Angle(n0, n1);
            if (a0 > 0.1)
            {
                if (a0 < 179.9)
                {
                    r.axis[1] = hasUp ? u : Vector3.Cross(n0, n1).normalized;
                    r.angle[1] = Utils.SignedAngle(n1, Vector3.zero, n0, r.axis[1]);
                }
                else
                {
                    r.axis[1] = u;
                    r.angle[1] = 180;
                }
            }

            return r;
        }

        private Vector3[] Rotation(IndexDelta initial, IndexDelta id)
        {
            if ((handle.facing == FacingLogic.Fixed) || handle.facesToward)
                return null;
            Vector3 v0, n0, v1, n1, w0, w1, f, p, q, up;
            v0 = Vector(initial);
            v1 = Vector(id);
            n0 = Normal(initial);
            n1 = Normal(id);

            w0 = Vector3.Cross(v0, n0).normalized;
            w1 = Vector3.Cross(v1, n1).normalized;
            if (handle.facing == FacingLogic.Axis)
            {
                up = handle.up;
                up.Normalize();
                if (IsRotationValid(v0, v1, up))
                {
                    w0 = Vector3.Cross(v0, up).normalized;
                    w1 = Vector3.Cross(v1, up).normalized;
                }
                else if (IsRotationValid(n0, n1, up))
                {
                    w0 = Vector3.Cross(n0, up).normalized;
                    w1 = Vector3.Cross(n1, up).normalized;
                }
                else
                {
                    w0 = Vector3.Cross(w0, up).normalized;
                    w1 = Vector3.Cross(w1, up).normalized;
                }
                n0 = n1 = up;
            }
            return new Vector3[] { v0, n0, w0, v1, n1, w1 };
        }
        /// <summary>
        /// finding the new forward and up vectors based on change from one axis triad to another
        /// </summary>
        /// <param name="initialRotation">the initial rotation of the mover</param>
        /// <param name="r">a 6 element array of Vector3 containing the two axis triads</param>
        /// <returns>returns a Vector3 array containing forward and up vectors</returns>
        public Vector3[] GetRotation(Quaternion initialRotation, Vector3[] r)
        {
            auxParent.localRotation = initialRotation;
            Vector3 fwd = Utils.LocalizeVector(auxParent.forward, null, auxParent.parent);
            Vector3 up = Utils.LocalizeVector(auxParent.up, null, auxParent.parent);
            float ax = Vector3.Angle(r[0], r[3]);
            if (ax < 0.1f) ax = 0f; else if (ax > 179.9f) ax = 180f;
            Vector3 vx = ax == 180f ? r[1] : Vector3.Cross(r[0], r[3]);
            if (ax != 0f) for (int i = 0; i < 3; i++) r[i] = Utils.Rotate(r[i], Vector3.zero, vx, ax);
            float ay = Vector3.Angle(r[1], r[4]);
            if (ay < 0.1f) ay = 0f; else if (ay > 179.9f) ay = 180f;
            Vector3 vy = ay == 180f ? r[0] : Vector3.Cross(r[1], r[4]);
            Vector3 nf = Utils.Rotate(fwd, Vector3.zero, vx, ax);
            Vector3 nu = Utils.Rotate(up, Vector3.zero, vx, ax);
            nf = Utils.Rotate(nf, Vector3.zero, vy, ay).normalized;
            nu = Utils.Rotate(nu, Vector3.zero, vy, ay).normalized;
            return new Vector3[] { nf, nu };
        }
        /// <summary>
        /// returns the progress on the path, closest ot a point 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float Progress(Vector3 p)
        {
            IndexDelta id = Closest(p);
            return (id.index + id.delta) / (closed ? point.Length + 1 : point.Length);
        }
        private IndexDelta Closest(Vector3 p)
        {
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
            Vector3 currentP;
            List<Vector3> p = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            for (int i = 0; i < segment.Length; i++)
                if (i < segment.Length - 1)
                    l += Vector3.Distance(segment[i].center, segment[i + 1].center);
                else if (closed)
                    l += Vector3.Distance(segment[i].center, segment[0].center);
            delta = l / Division;
            p.Add(segment[0].center);
            normals.Add(segment[0].normal);
            Vector3 v;
            float waiting = delta;
            float dl = 0;
            for (int i = 0; i < segment.Length; i++)
                if ((i < segment.Length - 1) || closed)
                {
                    v = segment[(i + 1) % segment.Length].center - segment[i].center;
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
                            p.Add(currentP);
                            normals.Add((1 - r) * segment[i].normal + r * segment[(i + 1) % segment.Length].normal);
                        }
                    }
                }
            if ((!closed) && (n <= Division))
            {
                p.Add(segment[segment.Length - 1].center);
                normals.Add(segment[segment.Length - 1].normal);
            }

            point = p.ToArray();
            normal = normals.ToArray();
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
        private void AddEnds(List<Segment> a, Vector3[] n)
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
                    normal = (ends[i].normal + n[oT[ends[i].face[Tend] + 3 - jj]]) / 2,
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
        private void LocalizeToMover()
        {
            Vector3 p;
            vector = new Vector3[point.Length];
            for (int i = 0; i < point.Length; i++)
            {
                if (closed)
                    vector[i] = point[(i + 1) % point.Length] - point[(i - 1 + point.Length) % point.Length];
                else
                {
                    if (i == 0) vector[i] = point[1] - point[0];
                    else if (i == point.Length - 1) vector[i] = point[i] - point[i - 1];
                    else vector[i] = point[i + 1] - point[i - 1];
                }
                vector[i].Normalize();
            }
        }
    }
}
