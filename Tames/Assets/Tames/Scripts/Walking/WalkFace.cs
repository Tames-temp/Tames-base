using System.Collections.Generic;
using UnityEngine;
namespace Walking
{

    /// <summary>
    /// this class provides a dynamic basis for walking. Each instance of this class is associated with a gameobject that would update the face if it changes.  
    /// </summary>
    public class WalkFace
    {
        /// <summary>
        /// the 3 vertexes of the face, based on a triangle in the <see cref="control"/>'s mesh
        /// </summary>
        public Vector3[] point = new Vector3[3];
        /// <summary>
        /// the normal of the face
        /// </summary>
        public Vector3 normal;
        /// <summary>
        /// the center of the face
        /// </summary>
        public Vector3 center;
        /// <summary>
        /// the owner of the mesh
        /// </summary>
        public WalkObject control;
        public ForceType forceType = ForceType.None;
        public float forcePush = 0;
        public Vector3 forceVector = Vector3.zero;
        private Vector3[] global = new Vector3[3];
        private Vector2[] P, U, V;
        private float[] C;
        private float D;
        public WalkFace(Vector3[] p)
        {
            point = p;
            //    center = (p[0] + p[1] + p[2]) / 3;
            //    P = new Vector2[3];
            P = new Vector2[3];
            U = new Vector2[3];
            V = new Vector2[3];
            C = new float[3];
            normal = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
            //   D = -Vector3.Dot(normal, point[0]);
            center = (p[0] + p[1] + p[2]) / 3;
        }
        /// <summary>
        /// checks if a specific point lands on the face via Y axis and returns the height different between the point and its landing. 
        /// </summary>
        /// <param name="p">the specified point in world space</param>
        /// <param name="dy">the output height difference</param>
        /// <returns>returns true if p can land on this face</returns>
        public bool On(Vector3 p, out float dy)
        {
            Transform t = control.owner.transform;
            Vector2 p2 = new Vector2(p.x, p.z);
            Vector3 n = (t.TransformPoint(normal) - t.TransformPoint(Vector3.zero)).normalized;
            for (int i = 0; i < 3; i++)
            {
                global[i] = t.TransformPoint(point[i]);
                P[i] = new Vector2(global[i].x, global[i].z);
            }
            D = -Vector3.Dot(n, global[0]);
            for (int i = 0; i < 3; i++)
            {
                U[i] = P[(i + 1) % 3] - P[i];
                V[i] = new Vector2(-U[i].y, U[i].x);
                C[i] = -Vector2.Dot(V[i], P[i]);
            }
            Vector2 inside = (P[0] + P[1] + P[2]) / 3;
            int sign = Vector2.Dot(V[0], inside) + C[0] > 0 ? 1 : -1;
            int plus = 0;
            for (int i = 0; i < 3; i++)
                plus += SameSign(Vector2.Dot(V[i], p2) + C[i], sign) ? 1 : 0;
            if (plus == 3)
                try
                {
                    dy = (Vector3.Dot(n, p) + D) / n.y;
                }
                catch
                {
                    dy = 0;
                }
            else dy = 0;
            return plus == 3;
        }
        private bool SameSign(float a, int sign)
        {
            if (a == 0) return true;
            else if (a < 0) return sign == -1;
            else return sign == 1;
        }
        public Vector3 Pushing(Vector3 p, float dT)
        {
            Vector3 r = Vector3.zero;
            switch (forceType)
            {
                case ForceType.Slide: return dT * forcePush * forceVector;
                case ForceType.Rotate:
                    Vector3 q = Utils.Rotate(p, control.owner.transform.TransformPoint(forceVector), Vector3.up, dT * forcePush);
                    return q - p;
                case ForceType.Path:
                    Debug.Log(forceVector.ToString() + (control.owner.transform.TransformPoint(forceVector) - control.owner.transform.position));
                    r = control.owner.transform.TransformPoint(forceVector) - control.owner.transform.position;
                    r = dT * forcePush * r.normalized;
                    return r;
                default: return r;
            }
        }
        /// <summary>
        /// converts faces in a game object's mesh into <see cref="WalkFace"/>s
        /// </summary>
        /// <param name="g">the game object</param>
        /// <param name="onlyUpward">determines if only upward faces are considered or all faces</param>
        /// <returns>a list of created faces</returns>
        public static List<WalkFace> GetFaces(WalkObject wo, bool onlyUpward)
        {
            WalkFace wf;
            Vector3 gn;
            GameObject g = wo.owner;
            ForceType ft = GetForce(g, out Vector3 vector, out float push, out Tames.TameSlider slider, out GameObject path);
            List<WalkFace> r = new List<WalkFace>();
            MeshFilter mf;
            GameObject owner;
            if (ft == ForceType.Path)
                owner = path;
            else
                owner = g;
            wo.owner = owner;
            mf = owner.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3[] v = mesh.vertices;
                int[] t = mesh.triangles;
                for (int i = 0; i < t.Length; i += 3)
                {
                    wf = new WalkFace(new Vector3[] { v[t[i]], v[t[i + 1]], v[t[i + 2]] }) { control = wo };
                    gn = g.transform.TransformPoint(wf.normal) - owner.transform.TransformPoint(Vector3.zero);
                    if ((Vector3.Angle(gn, Vector3.up) < 90) || (!onlyUpward))
                    {
                        r.Add(wf);
                        wf.forceType = ft;
                        if (ft == ForceType.Slide)
                        {
                            wf.forceVector = vector;
                            wf.forcePush = push;
                        }
                        if (ft == ForceType.Rotate)
                        {
                            wf.forceVector = vector;
                            wf.forcePush = push * 360;
                        }
                        if (ft == ForceType.Path)
                        {
                            wf.forceVector = slider.VectorOn(owner.transform.TransformPoint(wf.center)).normalized;
                            wf.forcePush = push;
                         }
                    }
                }
            }
            return r;
        }
        private static string KeyFrom = "_fstart";
        private static string KeyTo = "_fend";
        private static string KeyPivot = "_fpivot";
        private static string KeyAxis = "_faxis";
        private static string KeyPath = "_fpath";
        private Tames.TameSlider slider = null;
        private static ForceType GetForce(GameObject g, out Vector3 vector, out float m, out Tames.TameSlider slider, out GameObject pathObject)
        {
            Transform pivot = Utils.FindStartsWith(g.transform, KeyPivot);
            Transform axis = Utils.FindStartsWith(g.transform, KeyAxis);
            Transform from = Utils.FindStartsWith(g.transform, KeyFrom);
            Transform to = Utils.FindStartsWith(g.transform, KeyTo);
            Transform path = Utils.FindStartsWith(g.transform, KeyPath);
            m = 0;
            slider = null;
            vector = Vector3.zero;
            if ((pivot != null) && (axis != null))
            {
                m = Utils.M(pivot.position, axis.position - pivot.position, Vector3.up);
                vector = pivot.localPosition;
                pathObject = null;
                return ForceType.Rotate;
            }
            else if ((to != null) && (from != null))
            {
                m = (to.position - from.position).magnitude;
                vector = (to.position - from.position).normalized;
                if (path != null)
                {
                    slider = new Tames.TameSlider(path.gameObject, null, from.localPosition, to.localPosition);
                    pathObject = path.gameObject;
                }
                else
                    pathObject = null;
                //       Debug.Log("walk " + vector.ToString("0.00"));
                return path == null ? ForceType.Slide : ForceType.Path;
            }
            pathObject = null;
            return ForceType.None;
        }
        public override string ToString()
        {
            return forceType + " " + point[0].ToString("0.00") + point[1].ToString("0.00") + point[2].ToString("0.00");

        }
    }
}