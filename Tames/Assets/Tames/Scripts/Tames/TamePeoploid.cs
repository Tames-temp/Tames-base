using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TamePeoploid
    {
        public GameObject person;
        public Vector3 LastPosition { get { return pos[0]; } }
        public Vector3 Position { get { return pos[1]; } }
        public Transform transform;
        //    private int counter = 0;
        private Vector3[] pos = new Vector3[2];
        public TamePeoploid(GameObject go)
        {
            person = go;
            transform = go.transform;
            pos[0] = pos[1] = transform.position;
        }
        public void ChangeFrame()
        {
            Vector3 p = pos[1];
            pos[1] = transform.position;
            pos[0] = p;
        }
    }
}
