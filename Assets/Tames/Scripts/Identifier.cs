using HandAsset;
using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class Identifier
{
    public static GameObject left, right, head, rig;
    public static string[] LoadLines(string path)
    {
        TextAsset ta = (TextAsset)Resources.Load(path);
        string s = ta.text.Replace('\r', '\n');
        s = Utils.RemoveDuplicate(s, "\n");
        return s.Split(new char[] { '\n', ';', ':' });

    }
    public static HandModel[] Inputs(XRController lx, XRController rx, string fingerHeader)
    {

        HandModel[] r = new HandModel[2];
        GameObject[] hs = new GameObject[2];
        r[0] = new HandModel(lx.gameObject, left, 0);
        r[0].GetFingers(fingerHeader);
        r[1] = new HandModel(rx.gameObject, right, 1);
        r[1].GetFingers(fingerHeader);
        r[1].gripDirection = -1;
        //    Debug.Log("ID: " + r[0] + " / " + r[1]);
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