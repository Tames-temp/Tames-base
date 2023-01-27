using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tames
{
    public class TameAltering
    {
        public List<GameObject> gameObject = new List<GameObject>();
        public List<Material> material = new List<Material>();
        public Material replacement = null;
        public List<GameObject> marker = new List<GameObject>();
        public TameKeys type = TameKeys.None;
        public GameObject place;
        private Quaternion rotation;
        private ManifestHeader manifest;
        private int current = -1;
        //   public GameObject Current { get { return current >= 0 ? gameObject[current] : null; } }
        public void Prev()
        {
            if (type == TameKeys.Object)
            {
                if (gameObject.Count == 0) return;
                current = (current + gameObject.Count - 1) % gameObject.Count;
                for (int i = 0; i < gameObject.Count; i++)
                {
                    gameObject[i].SetActive(i == current);
                    marker[i].SetActive(false);
                }
            }
            else
            {
                if (material.Count == 0) return;
                current = (current + material.Count - 1) % material.Count;
                replacement.CopyPropertiesFromMaterial(material[current]);
            }
        }
        public void Next()
        {
            if (type == TameKeys.Object)
            {
                if (gameObject.Count == 0) return;
                current = (current + 1) % gameObject.Count;
                replacement.CopyPropertiesFromMaterial(material[current]);
            }
            else
            {

            }
        }
        public static int Read(ManifestHeader mh, List<TameAltering> tas, int index)
        {
            TameKeys key = TameKeys.None;
            mh.Resplit(',');
            if (mh.items.Count > 1)
            {
                if (mh.items[0].ToLower().StartsWith("mat")) key = TameKeys.Material;
                if (mh.items[0].ToLower().StartsWith("obj")) key = TameKeys.Object;
                mh.items.RemoveAt(0);
                if (key != TameKeys.None)
                {
                    TameAltering ta = new TameAltering() { manifest = mh };
                    tas.Add(ta);
                }
            }
            return index;
        }
        public void Populate(List<TameGameObject> tgos)
        {
            if (type == TameKeys.Object)
            {
                TameFinder finder = new TameFinder() { header = manifest };
                finder.PopulateObjects(tgos);
                GameObject go;
                Transform t;
                foreach (TameGameObject tg in finder.objectList)
                {
                    if (tg.alreadyFound)
                        go = GameObject.Instantiate(tg.gameObject);
                    else
                    {
                        go = tg.gameObject;
                        tg.alreadyFound = true;
                    }
                    gameObject.Add(go);
                    t = Utils.FindStartsWith(go.transform, TameHandles.KeyMarker);
                    if (t != null)
                        marker.Add(t.gameObject);
                    else
                        marker.Add(tg.gameObject);
                }
            }
            else
            {
                MeshRenderer mr;
                Material[] ms;
                bool[] done = new bool[manifest.items.Count];
                bool first = true;
                bool tgAdded;
                for (int i = 0; i < manifest.items.Count; i++) done[i] = false;
                foreach (TameGameObject tg in tgos)
                {
                    tgAdded = false;
                    mr = tg.gameObject.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        ms = mr.sharedMaterials;
                        for (int j = 0; j < manifest.items.Count; j++)
                            if (!done[j])
                                for (int i = 0; i < ms.Length; i++)
                                    if (ms[i].name.ToLower() == manifest.items[j].ToLower())
                                    {
                                        done[j] = true;
                                        material.Add(ms[i]);
                                        if (!tgAdded)
                                        {
                                            gameObject.Add(tg.gameObject);
                                            tgAdded = true;
                                        }
                                        if (first)
                                        {
                                            replacement = ms[i];
                                            first = false;
                                        }
                                        else
                                            ms[i] = replacement;
                                    }
                        mr.sharedMaterials = ms;
                    }
                }
            }
        }
        public static TameAltering[] Detect(Transform camera, List<TameAltering> tas)
        {
            float d, min = float.PositiveInfinity;
            TameAltering[] r = new TameAltering[] { null, null };
            for (int i = 0; i < tas.Count; i++)
                for (int j = 0; j < tas[i].gameObject.Count; j++)
                {
                    d = Hit(camera, tas[i].gameObject[j], tas[i].type == TameKeys.Material, tas[i].replacement);
                    if ((d >= 0) && (d < min))
                    {
                        min = d;
                        if (tas[i].type == TameKeys.Material) r[1] = tas[i];
                        else r[0] = tas[i];
                    }
                }
            return r;
        }
        private static float Hit(Vector3 eye, Vector3 ray, Vector3 n, Vector3 t0, Vector3 t1, Vector3 t2)
        {
            Vector3 nm = Vector3.Cross(t1 - t0, t2 - t0);
            if (Vector3.Angle(nm, n) > 90) nm = -nm;
            if (Vector3.Angle(nm, ray) > 90)
            {
                float m = (Vector3.Dot(t0, nm) - Vector3.Dot(eye, nm)) / Vector3.Dot(ray, nm);
                Vector3 ve = eye + m * ray;
                float at = Vector3.Angle(t0 - ve, t1 - ve) + Vector3.Angle(t0 - ve, t2 - ve) + Vector3.Angle(t2 - ve, t1 - ve);
                if (at > 179)
                    return Vector3.Distance(eye, ve);
            }
            return -1;
        }
        public static float Hit(Transform camera, GameObject go, bool isMat, Material mat)
        {
            try
            {
                MeshFilter mf = go.GetComponent<MeshFilter>();
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                Material[] ms = mr != null ? mr.sharedMaterials : null;
                if (mf != null)
                {
                    Vector3 eye = Utils.LocalizePoint(camera.position, camera, go.transform);
                    Vector3 ray = Utils.LocalizeVector(camera.forward, camera, go.transform);
                    Mesh mesh = mf.sharedMesh;
                    Vector3[] v = mesh.vertices;
                    Vector3[] vt = new Vector3[3];
                    Vector3[] n = mesh.normals;
                    int[] t = mesh.triangles;
                    int smc = mesh.subMeshCount;
                    int i, mi = -1;
                    float d, min = float.PositiveInfinity;
                    if (isMat && (ms != null))
                    {
                        for (int k = 0; k < smc; k++)
                        {
                            SubMeshDescriptor smd = mesh.GetSubMesh(k);
                            for (int j = 0; j < smd.indexCount; j += 3)
                            {
                                i = j + smd.indexStart;
                                d = Hit(eye, ray, n[t[i]], v[t[i]], v[t[i + 1]], v[t[i + 2]]);
                                if ((d >= 0) && (d < min))
                                {
                                    min = d;
                                    mi = k;
                                }
                            }
                        }
                        if (mi >= 0)
                            if (ms[mi] == mat)
                                return min;
                    }
                    else
                    {
                        for (i = 0; i < t.Length; i += 3)
                        {
                            d = Hit(eye, ray, n[t[i]], v[t[i]], v[t[i + 1]], v[t[i + 2]]);
                            if ((d >= 0) && (d < min))
                                min = d;
                        }
                        return min;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public void Place()
        {

        }
    }
}
