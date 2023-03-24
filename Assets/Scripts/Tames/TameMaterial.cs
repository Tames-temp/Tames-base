using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameMaterial : TameElement
    {
        /// <summary>
        /// the original material in the scene. The reason for having the original and the <see cref="clones"/> is for anticipating stencil buffer usage. 
        /// </summary>
        public Material original;
        public Markers.MarkerChanger[] changers = null;
        public Color initialSpectrum;
        public float initialIntensity;
        /// <summary>
        /// instances of the original material, dictated by the type of stencil buffer used
        /// </summary>
        public List<Material> clones = new List<Material>();
        /// <summary>
        /// the initial offset of the material main texture
        /// </summary>
        private Vector2 offsetBase = Vector2.zero;
        /// <summary>
        /// the initial offset of the emission texture map 
        /// </summary>
        public Vector2 offsetLight = Vector2.zero;
        /// <summary>
        /// keywords for the material property names
        /// </summary>
        public const int BaseColor = 0;
        public const int EmissionColor = 1;
        public const int MainTex = 2;
        public const int EmissionMap = 3;
      
        public TameMaterial()
        {
            tameType = TameKeys.Material;
        }
        /// <summary>
        /// overrides the <see cref="TameElement.GetParent"/>.
        /// </summary>
        /// <returns>the update parent of the material, hence only the first element of the returned array is assigned</returns>
        override public TameEffect GetParent()
        {
            TameEffect r = null;
            //      if (name.Equals("illum"))
            //          Debug.Log("mix: assign " + basis[0] + " " + updateParents.Count);
            if (basis == TrackBasis.Time)
                r = TameEffect.Time();
            else if (parents.Count > 0)
                r = parents[0];
            if (r != null) r.child = this;
            return r;
        }
        /// <summary>
        /// ovverrides <see cref="TameElement.AssignParent"/>
        /// </summary>
        /// <param name="all"></param>
        /// <param name="index"></param>
        public override void AssignParent(TameEffect[] all, int index)
        {
            TameEffect ps = GetParent();
            //        if (name.Equals("blink-blue"))
            //        Debug.Log("custom: blink " + index+ " "+all.Length);
           // for (int i = 0; i < 3; i++)
                all[index ] = ps;
        }
        /// <summary>
        /// swaps a specific material in a gameobject with its clone and returns the latter (or null if that material is not on the gameobject)
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Material SwitchMaterial(GameObject gameObject, Material original)
        {
            Material clone = null;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] ms = renderer.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                    if (ms[i] == original)
                    {
                        if (clone == null)
                        {
                            clone = new Material(original.shader);
                            clone.name = original.name;
                            clone.CopyPropertiesFromMaterial(original);
                        }
                        ms[i] = clone;
                    }
                if (clone != null)
                    renderer.sharedMaterials = ms;
            }
            return clone;
        }
        /// <summary>
        /// applies updates after progress is set by other update methods: <see cref="Update()"/> and <see cref="Update(TameProgress)"/>. This uses information read from the manifest file and stored in the manifest headers of the <see cref="manifest"/> field.
        /// </summary>
        private void ApplyUpdate()
        {
            float[] f;
            ManifestMaterial m = (ManifestMaterial)manifest;
            float[] emis = new float[] { 0, 0, 0 };
            float ins = 0;
            bool emC = false;
            bool inC = false;
            //      if (name == "coolermat")
            //        Debug.Log("input: apply: " + progress[0].progress);
         //   Debug.Log("changer before " + name);
            if (progress != null)
            {
                foreach (TameChanger tc in m.properties)
                {
                    f = tc.On(progress.progress);
                    switch (tc.property)
                    {
                        case MaterialProperty.Color:
                            //    Debug.Log("mix: " + progress[0].progress + " " + f[1] + ", " + clones.Count);
                            if (clones.Count == 0) original.SetColor(Utils.ProperyKeywords[BaseColor], TameColor.ToColor(f));
                            else foreach (Material mat in clones) mat.SetColor(Utils.ProperyKeywords[BaseColor], TameColor.ToColor(f));
                            break;
                        case MaterialProperty.Glow:
                             emis = f;
                            emC = true;
                            ins = f[3];
                            inC = true;
                        //    Debug.Log("emic: " + f[0] + "," + f[1] + "," + f[2]);
                            break;
                        case MaterialProperty.Bright:
                            ins = f[0];
                            inC = true;
                             break;
                        case MaterialProperty.MapX:
                            offsetBase.x = f[0];
                   //         Debug.Log("changer " + name);
                            if (clones.Count == 0) original.SetTextureOffset(Utils.ProperyKeywords[MainTex], offsetBase);
                            else foreach (Material mat in clones) mat.SetTextureOffset(Utils.ProperyKeywords[MainTex], offsetBase);
                            break;
                        case MaterialProperty.MapY:
                            offsetBase.y = f[0];
                            if (clones.Count == 0) original.SetTextureOffset(Utils.ProperyKeywords[MainTex], offsetBase);
                            else foreach (Material mat in clones) mat.SetTextureOffset(Utils.ProperyKeywords[MainTex], offsetBase);
                            break;
                        case MaterialProperty.LightX:
                            offsetLight.x = f[0];
                            if (clones.Count == 0) original.SetTextureOffset(Utils.ProperyKeywords[EmissionMap], offsetLight);
                            else foreach (Material mat in clones) mat.SetTextureOffset(Utils.ProperyKeywords[EmissionMap], offsetLight);
                            break;
                        case MaterialProperty.LightY:
                            offsetLight.y = f[0];
                            //       Debug.Log("cyclingp " + progress[0].progress + " " + offsetLight.ToString("0.00"));
                            if (clones.Count == 0) original.SetTextureOffset(Utils.ProperyKeywords[EmissionMap], offsetLight);
                            else foreach (Material mat in clones) mat.SetTextureOffset(Utils.ProperyKeywords[EmissionMap], offsetLight);
                            break;
                    }
                }
                if ((!emC) && inC)
                    original.SetColor(Utils.ProperyKeywords[EmissionColor], initialSpectrum * Mathf.Pow(2, ins));
                if (emC)
                    original.SetColor(Utils.ProperyKeywords[EmissionColor], new Color(emis[0], emis[1], emis[2]) * Mathf.Pow(2, ins));
            }
        }
        /// <summary>
        /// stores the initial value of the emission color in <see cref="initialSpectrum"/>.
        /// </summary>
        public void GetInitial()
        {
            initialSpectrum = original.GetColor(Utils.ProperyKeywords[EmissionColor]);
            if (initialSpectrum == null)
                initialSpectrum = Color.white;
        }
        /// <summary>
        /// updates the matrial element as child of another <see cref="TameElement"/>, overriding <see cref="TameElement.Update(TameProgress)"/>
        /// </summary>
        /// <param name="p"></param>
        override public void Update(float p)
        {
            if(progress!=null)progress.SetProgress(p);
            ApplyUpdate();
        }

        override public void Update(TameProgress p)
        {
            SetByParent(p);
            ApplyUpdate();
        }
        /// <summary>
        /// updates the material by time, overriding <see cref="TameElement.Update"/>
        /// </summary>
        override public void Update()
        {
        
            SetByTime();
            ApplyUpdate();
     //       Debug.Log("material time " + name+ " "+progress.progress+" "+markerProgress.cycleType);
        }
        /// <summary>
        /// sets the inintial offsets of the maps
        /// </summary>
        public void SetProperties()
        {
            offsetBase = original.GetTextureOffset(Utils.ProperyKeywords[MainTex]);
            offsetLight = original.GetTextureOffset(Utils.ProperyKeywords[EmissionMap]);
        }

        /// <summary>
        /// identifies the materials that are deemed as interactive in the <see cref="TameManager"/> argument"/>. Only the first material matching a name in the manifest is selected.
        /// </summary>
        /// <param name="man">the <see cref="TameManager"/> manifest</param>
        /// <param name="tgos">list of all children and descendants of the interactive root, created by <see cref="TameManager.SurveyInteractives(GameObject[])"/></param>
        /// <returns>a list of <see cref="TameElement"/> that includes <see cref="TameMaterial"/> objects made by each material found</returns>
        public static List<TameElement> FindMaterials(TameManager man, List<TameGameObject> tgos)
        {
            /*
             * first we find all unique materials in tgos and store them in materials list. Then for each material in the list, we check if the name matches one on the manifest material headers and create TameMaterials by them 
             */
            List<Material> materials = new List<Material>();
            Material[] sm;
            bool f;
            foreach (TameGameObject go in tgos)
            {
                Renderer ren = go.gameObject.GetComponent<Renderer>();
                if (ren != null)
                {
                    sm = ren.sharedMaterials;
                    for (int i = 0; i < sm.Length; i++)
                    {
                        f = false;
                        for (int j = 0; j < materials.Count; j++)
                            if (materials[j] == sm[i])
                            {
                                f = true;
                                break;
                            }
                        if (!f)
                            materials.Add(sm[i]);
                    }
                }
            }
            List<TameElement> r = new List<TameElement>();
            List<Material> mit = new List<Material>();
            for (int mi = 0; mi < man.manifests.Count; mi++)
                if (man.manifests[mi].header.key == TameKeys.Material)
                {
                    mit.Clear();
                    foreach (string name in man.manifests[mi].header.items)
                    {
                        bool pstart = name[name.Length - 1] == '*';
                        string nameL = pstart ? name.Substring(0, name.Length - 1) : name;
                        nameL = nameL.ToLower();
                        f = false;
                        foreach (Material m in materials)
                        {
                            if (pstart)
                            {
                                if (m.name.ToLower().StartsWith(nameL))
                                    mit.Add(m);
                            }
                            else
                            if (m.name.ToLower().Equals(nameL))
                                mit.Add(m);
                        }
                    }
                    foreach (Material m in mit)
                    {
                        f = false;
                        foreach (TameMaterial tm in r)
                        {
                            if (tm.original == m)
                            {
                                tm.manifest = man.manifests[mi];
                                f = true;
                            }
                        }
                        if (!f)
                        {
                            r.Add(new TameMaterial() { name = m.name, manifest = man.manifests[mi], original = m });
                            ((TameMaterial)r[r.Count - 1]).SetProperties();
                        }
                    }
                }
            return r;
        }
        /// <summary>
        /// checks if the material has an emission property and if it does enables the keyword
        /// </summary>
        public void CheckEmission()
        {
            ManifestMaterial tmm = (ManifestMaterial)manifest;
            foreach (TameChanger tc in tmm.properties)
                if ((tc.property == MaterialProperty.Glow) || (tc.property == MaterialProperty.LightX) || (tc.property == MaterialProperty.LightY))
                    if (!original.IsKeywordEnabled("_EmissiveColor"))
                        original.EnableKeyword("_EmissiveColor");
            GetInitial();
        }
        public static List<Material> LocalizeMaterials(List<GameObject> gos, List<float> initial)
        {
            List<Material> materials = new List<Material>();
            List<Material> originals = new List<Material>();
            Renderer r;
            Material[] sm;
            Material m;
            int ii;
            foreach (GameObject go in gos)
            {
                r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    sm = r.sharedMaterials;
                    for(int i = 0; i < sm.Length; i++)
                    {
                        if ((ii = originals.IndexOf(sm[i])) >= 0)
                            sm[i] = materials[ii];
                        else
                        {
                            if (sm[i].GetTexture(Utils.ProperyKeywords[TameMaterial.MainTex]) != null)
                            {
                                m = new Material(sm[i]);
                                originals.Add(sm[i]);
                                materials.Add(m);
                                try { initial.Add(m.GetTextureScale(Utils.ProperyKeywords[TameMaterial.MainTex]).x); } catch { initial.Add(1); }
                                sm[i] = m;
                            }
                        }
                    }
                    r.sharedMaterials = sm;
                }
            }
            return materials;
        }
    }
}