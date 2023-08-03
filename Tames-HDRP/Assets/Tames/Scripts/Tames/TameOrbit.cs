using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameOrbit : TamePath
    {
        public Vector3 pivot;
        public Vector3 axis;
        public Vector3 up;
        public float span;
        public Transform[] rot;
        public void SetLength()
        {
            length = Mathf.Abs(span) * Mathf.Deg2Rad;
        }
        override public void AssignMovers(GameObject[] g, bool def = false)
        {
            Vector3 p;
            Quaternion q;
            bases = new Transform[g.Length];
            attached = new Transform[g.Length];
            for (int i = 0; i < g.Length; i++)
            {
                attached[i] = g[i].transform;
                float m = def ? 0 : GetM(g[i].transform.position);
                bases[i] = new GameObject(mover.parent.name + "-" + i).transform;
                bases[i].parent = mover.parent;
                bases[i].localPosition = Position(m);
                bases[i].localRotation = facing == FacingLogic.Free ? Rotation(bases[i].localPosition - pivot) : Quaternion.identity;
                p = g[i].transform.position;
                q = g[i].transform.rotation;
                g[i].transform.parent = bases[i];
                g[i].transform.position = p;
                g[i].transform.rotation = q;
            }
            virtualMover = new GameObject(bases[0].parent.name + "-virtual").transform;
            virtualMover.parent = bases[0].parent;
            virtualMover.localPosition = Position(0);
            virtualMover.localRotation = facing == FacingLogic.Free ? Rotation(virtualMover.localPosition - pivot) : Quaternion.identity;
        }

        override public void AssignMoverBasis(GameObject g)
        {
            mover = g.transform;
            float m = GetM(g.transform.position);
            virtualMover = new GameObject(mover.parent.name + "-base").transform;
            virtualMover.parent = mover.parent;
            virtualMover.localPosition = Position(m);
            virtualMover.localRotation = facing == FacingLogic.Free ? Rotation(virtualMover.localPosition - pivot) : Quaternion.identity;
            Vector3 p = g.transform.position;
            Quaternion q = g.transform.rotation;
            g.transform.parent = virtualMover.transform;
            g.transform.position = p;
            g.transform.rotation = q;
        }
        override public GameObject Clone(int i, float m)
        {
            bases[i] = new GameObject(mover.parent.name + "-" + i).transform;
            bases[i].parent = mover.parent;

            bases[i].localPosition = Position(m);
            bases[i].localRotation = facing == FacingLogic.Free ? Rotation(bases[i].localPosition - pivot) : Quaternion.identity;
            attached[i] = GameObject.Instantiate(mover, bases[i]);
            attached[i].localPosition = mover.localPosition;
            attached[i].localRotation = mover.localRotation;
            return attached[i].gameObject;
        }
        override public void SetInitial(float mold, float mnew)
        {
            Vector3 p;
            Quaternion q;
            for (int i = 0; i < bases.Length; i++)
            {
                p = attached[i].transform.position;
                q = attached[i].transform.rotation;
                bases[i].localPosition = Position(mnew);
                if (facing == FacingLogic.Free)
                    bases[i].localRotation = Rotation(bases[i].localPosition - pivot);
                attached[i].position = p;
                attached[i].rotation = q;
            }
        }
        public override Vector3 Normal(float m)
        {
            return Utils.Rotate(start, pivot, axis, m * span).normalized;
        }
        public override  Vector3 Position(float m)
        {
            return Utils.Rotate(start, pivot, axis, m * span);
        }
        public override Quaternion Rotation(float m)
        {
            return Quaternion.LookRotation(Position(m), axis);
        }
        private Quaternion Rotation(Vector3 pos)
        {
            return Quaternion.LookRotation(pos, axis);
        }
        override public  void MoveVirtual(float m)
        {
            Vector3 p = Position(m);
            virtualMover.localPosition = p;
            if (facing == FacingLogic.Free)
                virtualMover.localRotation = Rotation(p - pivot);

        }
        override public  void MoveLinked(float m)
        {
            float mi;
            if (linked != null)
                for (int i = 0; i < linked.Length; i++)
                {
                    mi = element.progress.FakeByOffset(linkOffset[i]);
                    Vector3 p = Position(mi);
                    linked[i].localPosition = p;
                    if (facing == FacingLogic.Free)
                        linked[i].localRotation = Rotation(p - pivot);
                }
        }
        override public void Move(int index, float m)
        {
            Vector3 p = Position(m);
            bases[index].localPosition = p;
            if (facing == FacingLogic.Free)
                bases[index].localRotation = Rotation(p - pivot);
        }
        override public float GetM(Vector3 global)
        {
            Vector3 p = parent.InverseTransformPoint(global);
            float ang = Utils.Angle(p, pivot, start, axis, true);
            if (ang == 0f) return 0f;
            if (span == 360)
                return ang > 0 ? ang / span : (360 + ang) / span;
            float s = Math.Abs(span);
            float a = span > 0 ? ang : -ang;
            if (s <= 180)
            {
                if (a > 0)
                    return a < s ? a / s : 1;
                else
                {
                    if (Mathf.Abs(a) < (360 - s) / 2) return 0;
                    else return 1;
                }
            }
            else
            {
                if (a > 0) return a / span;
                else
                {
                    if (360 + a <= s) return (360 + a) / s;
                    else return Mathf.Abs(a) < (360 - s) / 2 ? 0 : 1;
                }
            }
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
                rot = rot,
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
