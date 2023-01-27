using HandAsset;
using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script
{
    public class Identifier
    {
        public static Material HandMats(GameObject[] root)
        {
            int i;
            //  Material[] handmats = new Material[4];
            foreach (GameObject g in root)
                if (g.name.StartsWith("handmat"))
                    return g.GetComponent<MeshRenderer>().sharedMaterial;
            return null;
        }
        public static string[] LoadLines(string path)
        {
            TextAsset ta = (TextAsset)Resources.Load(path);
            string s = ta.text.Replace('\r', '\n');
            s = Utils.RemoveDuplicate(s, "\n");
            return s.Split('\n');

        }
        public static HandModel[] Inputs(GameObject[] root, string fingerHeader)
        {
            HandModel[] r = new HandModel[2];
            GameObject[] hs = new GameObject[2];
            foreach (GameObject g in root)
                if (g.name.Equals("XRRig"))
                {
                    GameObject co = Child(g, "Camera Offset");
                    hs[0] = Child(co, "Left");
                    hs[1] = Child(co, "Right");
                    Debug.Log(hs[0].name);
                }
            foreach (GameObject g in root)
            {
                if (g.name.Equals("leftHand"))
                { r[0] = new HandModel(hs[0], g, 0); r[0].GetFingers(fingerHeader); }
                if (g.name.Equals("rightHand"))
                { r[1] = new HandModel(hs[1], g, 1); r[1].GetFingers(fingerHeader); r[1].gripDirection = -1; }
                if (g.name.Equals("HeadObject"))
                    MainScript.HeadObject = g;
            }
            Debug.Log("ID: " + r[0] + " / " + r[1]);
            return r;
        }
        public static Material FindMaterial(GameObject g, string name)
        {
            MeshRenderer mr = g.GetComponent<MeshRenderer>();
            Material m;
            Material[] ms;
            if (mr != null)
            {
                ms = mr.sharedMaterials;
                foreach (Material mi in ms)
                    if (mi.name.Equals(name))
                        return mi;
            }
            for (int i = 0; i < g.transform.childCount; i++)
            {
                m = FindMaterial(g.transform.GetChild(i).gameObject, name);
                if (m != null)
                    return m;
            }
            return null;

        }
        public static GameObject Child(GameObject a, string s)
        {
            for (int i = 0; i < a.transform.childCount; i++)
                if (a.transform.GetChild(i).gameObject.name.Equals(s))
                    return a.transform.GetChild(i).gameObject;
            return null;
        }
        public static GameObject Descendent(GameObject a, string name)
        {
            GameObject g;
            for (int i = 0; i < a.transform.childCount; i++)
                if (a.transform.GetChild(i).gameObject.name.Equals(name))
                    return a.transform.GetChild(i).gameObject;
                else
                {
                    g = Descendent(a.transform.GetChild(i).gameObject, name);
                    if (g != null)
                        return g;
                }
            return null;
        }
        public static GameObject DescendentStartsWith(GameObject a, string name)
        {
            GameObject g;
            for (int i = 0; i < a.transform.childCount; i++)
                if (a.transform.GetChild(i).gameObject.name.StartsWith(name))
                    return a.transform.GetChild(i).gameObject;
                else
                {
                    g = DescendentStartsWith(a.transform.GetChild(i).gameObject, name);
                    if (g != null)
                        return g;
                }
            return null;
        }
    }
}
