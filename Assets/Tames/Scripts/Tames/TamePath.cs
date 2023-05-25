using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TamePath
    {
        public Vector3 start, end;
        public Transform parent;
        public Transform self;
        public Transform mover;
        public Transform virtualMover;
        public Transform[] bases;
        public Transform[] attached;
        public Transform[] linked = null;
        public float[] linkOffset = null;
        public FacingLogic facing = FacingLogic.Free;
        public TameElement element;
        public bool valid = false;
        public bool freeRotator = false;
        public float length;
        public virtual float GetM(Vector3 global)
        {
            return 0;
        }
        public virtual Vector3 Normal(float m) { return Vector3.up; }
        public virtual void MoveVirtual(float m) { }
        public virtual Vector3 Position(float m) { return Vector3.zero; }
        public virtual Quaternion Rotation(float m) { return Quaternion.identity; }
        public virtual void Move(int index, float m) { }
        public virtual void MoveLinked(float m) { }
        public virtual void SetInitial(float mold, float mnew) { }
        public virtual void AssignMovers(GameObject[] g, bool def = false) { }
        public virtual void AssignMoverBasis(GameObject g) { }
        public virtual GameObject Clone(int i, float m)
        {
            return null;
        }
        public virtual TamePath Clone(GameObject owner, GameObject mover, LinkedKeys lt)
        {
            return null;
        }
    }
}
