using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameFreeRotator : TamePath
    {
        public Vector3 pivot;
        public Vector3 axis;
        public Vector3 up;
        public float span;
        //    public Transform[] rot;
        Vector3 U, V, W;
        public TameFreeRotator()
        {
            freeRotator = true;
        }
        override public void AssignMovers(GameObject[] g, bool def = false)
        {
            bases = new Transform[1];
            attached = new Transform[1];
            for (int i = 0; i < 1; i++)
                if (i < g.Length)
                {
                    attached[i] = g[i].transform;
                    bases[i] = new GameObject(mover.parent.name + "-" + i).transform;
                    bases[i].parent = mover.parent;
                    bases[i].localRotation = Quaternion.identity;
                    bases[i].localPosition = start;
                    W = Vector3.forward;
                    V = Vector3.up;
                     Vector3 p = g[i].transform.position;
                    Quaternion q = g[i].transform.rotation;
                    g[i].transform.parent = bases[i];
                    g[i].transform.position = p;
                    g[i].transform.rotation = q;
                }
        }

        public float Move(Vector3 global)
        {
      //     if (this.element.name == "arm")                Debug.Log("arm : " + global.ToString());
            Vector3 p = parent.InverseTransformPoint(global);
            Vector3 u = p - pivot;
            Vector3 v = start - pivot;
            if (Vector3.Angle(u, v) > span)
                u = Utils.On(u, Vector3.zero, v);
            u = v.magnitude * u.normalized;
            float a = Vector3.Angle(u, v);
            if (a == 0)
            {
                //    bases[0].localRotation = Quaternion.LookRotation(W, U);
                bases[0].localRotation = Quaternion.identity;
                bases[0].localPosition = start;
            }
            else
            {
                bases[0].localRotation = Quaternion.identity;
                bases[0].localPosition = start;
                Vector3 w = Vector3.Cross(v, u);
                a = Utils.Angle(u, Vector3.zero, v, w, true);
                //    u = Utils.Rotate(W, Vector3.zero, w, a);
                //     v = Utils.Rotate(V, Vector3.zero, w, a);
                //     p = Utils.Rotate(start - pivot, Vector3.zero, w, a);
                //   bases[0].localRotation = Quaternion.LookRotation(u, v);
                //      bases[0].localPosition = pivot + p;
                bases[0].Rotate(w, a);
                bases[0].localPosition = Utils.Rotate(start, pivot, w, a);
            }
            return a / span;
        }


        public override TamePath Clone(GameObject owner, GameObject mover, LinkedKeys lt)
        {
            TameOrbit ts = new()
            {
                start = start,
                pivot = pivot,
                span = span,
                end = end,
                axis = axis,
                up = up,
                //    rot = rot,
                attached = new Transform[attached.Length],
                bases = new Transform[bases.Length],
                facing = facing,
            };
            ts.parent = ts.self = owner.transform;
            //   to.mover = mover.transform;
            if (lt == LinkedKeys.None)
            {
                ts.bases[0] = new GameObject().transform;
                ts.bases[0].parent = owner.transform;
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
                    ts.bases[i].parent = owner.transform;
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
