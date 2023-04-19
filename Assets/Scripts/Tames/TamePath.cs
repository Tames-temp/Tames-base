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
        public Transform mover;
        public Transform moverBase;
        public Transform[] bases;
        public Transform[] attached;
        public FacingLogic facing = FacingLogic.Free;
        public bool valid = false;
        public virtual float GetM(Vector3 global)
        {
            return 0;
        }
        public virtual void Move(int index, float m) { }
        public virtual void SetInitial(float mold, float mnew) { }
        public virtual void AssignMovers(GameObject[] g, bool def = false) { }
        public virtual void AssignMoverBasis(GameObject g) { }
        public virtual GameObject Clone(int i, float m)
        {
            return null;
        }
    }
}
