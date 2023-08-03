using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameCorrespond
    {
        public GameObject element = null;
        public Transform target;
        public Markers.MarkerCorrespond marker;
        private GameObject root;
        private Transform[] ts;
        public TameCorrespond(Markers.MarkerCorrespond mc)
        {
            marker = mc;
            root = mc.root;
            if ((ts = TameHandles.ValidObject(mc.gameObject, out int f, out int t)) != null)
                element = mc.gameObject;
        }
        private Transform FindClosest(Transform root, float dist, Transform last)
        {
            Transform t = ts[7];
            float f;
            float min = dist;
            Transform gi, current = last;
            if ((f = Vector3.Distance(root.position, t.position)) < min)
            {
                min = f;
                current = root;
            }
            for (int i = 0; i < root.childCount; i++)
                if ((gi = FindClosest(root.GetChild(i), min, current)) != last)
                {
                    current = gi;
                    min = Vector3.Distance(gi.position, t.position);
                }
            return current;
        }
        public void Match()
        {
            if ((root == null) || (element == null)) return;
            target = FindClosest(root.transform, float.PositiveInfinity, null);
       //     Debug.Log("CSP: " + target.name);
            Vector3 u, targetPos;
            Quaternion targetRot;

            if (target.parent != null)
                element.transform.parent = target.parent;

            targetPos = target.position;
            targetRot = target.rotation;
            element.transform.position = target.position;
            target.parent = element.transform;
            target.rotation = targetRot;
            target.position = targetPos;
            target.name = ts[7].name;
            ts[7].name = "_replaced";
            u = target.position - ts[7].position;
            for (int i = 0; i < element.transform.childCount; i++)
                element.transform.GetChild(i).position += u;
        }
    }
}

