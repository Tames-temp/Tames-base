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

        override public void AssignMovers(GameObject[] g, bool def = false)
        {
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
                Vector3 p = g[i].transform.position;
                Quaternion q = g[i].transform.rotation;
                g[i].transform.parent = bases[i];
                g[i].transform.position = p;
                g[i].transform.rotation = q;
            }
        }
        public void AssignMovers2(GameObject[] g, bool def = false)
        {
            bases = new Transform[g.Length];
            rot = new Transform[g.Length];
            attached = new Transform[g.Length];
            for (int i = 0; i < g.Length; i++)
            {
                attached[i] = g[i].transform;
                float m = GetM(g[i].transform.position);
                bases[i] = new GameObject(mover.parent.name + "-b" + i).transform;
                rot[i] = new GameObject(mover.parent.name + "-r" + i).transform;
                bases[i].parent = mover.parent;
                rot[i].parent = mover.parent;
                bases[i].localPosition = pivot;
                bases[i].LookAt(mover.parent.TransformPoint(start), mover.parent.TransformPoint(pivot + axis) - mover.parent.TransformPoint(pivot));
                bases[i].Rotate(bases[i].up, m * span);
                rot[i].localPosition = attached[i].localPosition;
                rot[i].localRotation = Quaternion.identity;
                bases[i].localRotation = facing == FacingLogic.Free ? Rotation(bases[i].localPosition - pivot) : Quaternion.identity;
                Vector3 p = g[i].transform.position;
                Quaternion q = g[i].transform.rotation;
                g[i].transform.parent = bases[i];
                g[i].transform.position = p;
                g[i].transform.rotation = q;
            }
        }
        override public void AssignMoverBasis(GameObject g)
        {
            mover = g.transform;
            float m = GetM(g.transform.position);
            moverBase = new GameObject(mover.parent.name + "-base").transform;
            moverBase.parent = mover.parent;
            moverBase.localPosition = Position(m);
            moverBase.localRotation = facing == FacingLogic.Free ? Rotation(moverBase.localPosition - pivot) : Quaternion.identity;
            Vector3 p = g.transform.position;
            Quaternion q = g.transform.rotation;
            g.transform.parent = moverBase.transform;
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
        private Vector3 Position(float m)
        {
            return Utils.Rotate(start, pivot, span>0?-axis:axis, m * span);
        }
        private Quaternion Rotation(Vector3 pos)
        {
            return Quaternion.LookRotation(pos, axis);
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
    }
}
