using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tames
{
    /// <summary>
    /// the class contains the whole of the manifest file ("manifest.txt" stored in Resources folder) and the creation and update of the interactives.
    /// </summary>
    public class TameManager
    {
        /// <summary>
        /// the path of mainfest file in the Resources folder
        /// </summary>
        public static string ManifestPath = "manifest";
        /// <summary>
        /// a root <see cref="GameObject"/> named "interactives" that contains all interactive elements 
        /// </summary>
        public static GameObject RootObject = null;
        private string[] lines;
        /// <summary>
        /// this is the list of successfully created manifests for <see cref="TameElement"/>s by blocks.
        /// </summary>
        public List<ManifestBase> manifests;
        /// <summary>
        /// this is the list of successfully created manifests for <see cref="TameArea"/>s by blocks.
        /// </summary>
        public List<TameArea> area;
        /// <summary>
        /// the list of all non-key game objects that have <see cref="RootObject"/> as their parent or ancestor.
        /// </summary>
        public List<TameGameObject> tgos;
        /// <summary>
        /// list of walk manifests
        /// </summary>
        private List<ManifestHeader> walkManifest = new List<ManifestHeader>();
        /// <summary>
        /// the list of <see cref="TameElement"/>s under the <see cref="RootObject"/>. 
        /// </summary>
        public List<TameElement> tes;
        public List<TameAltering> altering = new List<TameAltering>();
        /// <summary>
        /// the walk manager for this manifest
        /// </summary>
        public static Walking.WalkManager walkManager = null;
        public List<TameMatch> matches = new List<TameMatch>();
        public List<Markers.MarkerMaterial> materialMarkers = new List<Markers.MarkerMaterial>();
        public List<Markers.MarkerMaterial> lightMarkers = new List<Markers.MarkerMaterial>();
        public TameManager()
        {
            manifests = new List<ManifestBase>();
            area = new List<TameArea>();
            tgos = new List<TameGameObject>();
            tes = new List<TameElement>();
            //       walkManifest = new List<ManifestHeader>();
        }
        string[] Import(string[] lines, ManifestHeader header)
        {
            List<string> result = new List<string>();
            string[] l;
            result.AddRange(lines);
            foreach (string imn in header.items)
            {
                l = Assets.Script.Identifier.LoadLines(imn);
                Debug.Log("import " + l.Length);
                result.AddRange(l);
            }
            return result.ToArray();
        }
        /// <summary>
        /// reads the manifest file and creates all custom abstract <see cref="TameArea"/>s, <see cref="TameMaterial"/>s and other modifies <see cref="TameElement"/>s. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        bool Read()
        {
            // 7/25 11:55
            if (RootObject == null) return false;
            int i = 0;
            string s;
            ManifestHeader header;
            // i = ManifestKeys.SetLanguage(lines[0]);
            header = ManifestHeader.Read(Utils.Clean(lines[i]));
            if (header.key == TameKeys.Import)
            {
                i++;
                lines = Import(lines, header);
            }
            while (i < lines.Length)
            {
                if (lines[i].Length > 0)
                {
                    s = Utils.Clean(lines[i]);
                    header = ManifestHeader.Read(s);
                    //          Debug.Log(s + " >> " + header.key);
                    //        TameManifestBase.Read(header, lines, i, manifests);
                    switch (header.key)
                    {
                        case TameKeys.Object: i = ManifestBase.Read(header, lines, i, manifests); break;
                        case TameKeys.Material: i = ManifestBase.Read(header, lines, i, manifests); break;
                        case TameKeys.Light: i = ManifestBase.Read(header, lines, i, manifests); break;
                        //   case TameKeys.Area: i = TameArea.GetArea(header, lines, i, area); break;
                        case TameKeys.Custom:
                            i = ManifestCustom.Create(header, lines, i, out TameCustomValue tcv);
                            if (tcv != null)
                            {
                                tes.Add(tcv);
                                //       Debug.Log("custom added " + tcv.name);
                            }
                            break;
                        case TameKeys.Walk: walkManifest.Add(header); break;
                        case TameKeys.Camera: i = TameCamera.ReadCamera(header, lines, i); break;
                        case TameKeys.Eye: i = TameCamera.ReadEye(header, i); break;
                        case TameKeys.Mode: i = InputBasis.ReadMode(header, i); break;
                        case TameKeys.Alter: i = TameAltering.Read(header, altering, i); break;
                        case TameKeys.Match: i = ManifestMatch.Read(lines, i, matches); break;
                    }
                }
                i++;
            }
            return true;
        }
        /// <summary>
        /// loads the manifest
        /// </summary>
        /// <param name="l"></param>
        public void LoadManifest(string[] l)
        {
            // 7/25 11:57
            lines = l;
            Debug.Log("lines = " + l.Length);

            SurveyInteractives();
            if (lines.Length > 0)
                Read();
            AssignMatches();
            TameCamera.AssignCamera(tgos);
            IdentifyWalk();
            if (walkManager != null)
            {
                TameCamera.currentFace = walkManager.InitiatePosition(TameCamera.cameraTransform.position);
            }
            // create all interactors in ints and in objects
            // sort interactors: grip > 
            //     RedefineAreas();
            IdentifyElements();
            RedefineCues();
            IdentifyMaterials();
            IdentifyLights();
            PopulateUpdates();
            SetCustomValues();
            SetProgressProperties();
            PopulateLinked();
            SetScaled();
            SetTo();
            // sort update time > find objects
        }
        /// <summary>
        /// identifies all interactive objects and lights inside the <see cref="RootObject"/> and stores them in <see cref="tes"/>. For this process see <see cref="TameHandles"/>. In addition, it stores all game objects in <see cref="tgos"/>. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        /// <param name="root"></param>
        void SurveyInteractives()
        {
            // 7/25 11:56
            GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObj in root) if (rootObj.name.Equals("defaults"))
                {
                    tgos.AddRange(TameObject.CreateInteractive(TameTime.RootTame, rootObj, tes));
                    //        Debug.Log("import ints: " + tgos.Count);
                    break;
                }
            foreach (GameObject rootObj in root) if (rootObj.name.Equals("interactives")) { RootObject = rootObj; break; }
            if (RootObject != null)
            {
                Markers.MarkerArea.PopulateAll(RootObject);
                tgos.AddRange(TameObject.CreateInteractive(TameTime.RootTame, RootObject, tes));
            }
            Markers.MarkerProgress.PopulateAll(tgos);
            Markers.MarkerMaterial mt;
            foreach (TameGameObject tgo in tgos)
                if ((mt = tgo.gameObject.GetComponent<Markers.MarkerMaterial>()) != null)
                    materialMarkers.Add(mt);
        }
        void AssignMatches()
        {
            foreach (TameMatch match in matches)
            {
                match.Match(tgos, tes);
            }
        }
        /// <summary>
        /// creates the <see cref="walkManager"/> 
        /// </summary>
        void IdentifyWalk()
        {
            TameFinder finder = new TameFinder();
            if (walkManifest.Count > 0)
            {
                foreach (ManifestHeader mh in walkManifest)
                {
                    finder.header = mh;
                    finder.PopulateObjects(tgos);
                }
                finder.RemoveDuplicate(TameFinder.Object);
                bool f = false;
                foreach (TameGameObject go in tgos)
                    if (go.gameObject.GetComponent<Markers.MarkerWalk>() != null)
                    {
                        f = false;
                        foreach (TameGameObject g in finder.objectList)
                            if (go.gameObject == g.gameObject)
                            { f = true; break; }
                        if (!f)
                            finder.objectList.Add(go);
                    }
                walkManager = new Walking.WalkManager(finder.objectList, true);
                //  Debug.Log("walk " + finder.objectList.Count);
                foreach (TameGameObject tg in finder.objectList)
                    if (tg.gameObject.name.StartsWith("_"))
                    {
                        tg.gameObject.SetActive(false);
                        //   Debug.Log("walk false " + tg.gameObject.name);
                    }
            }
        }

        void RedefineCues()
        {
            ManifestObject tom;
            TameFinder finder = new TameFinder() { header = new ManifestHeader() };
            TameObject to, sto;
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    if (te.manifest != null)
                    {
                        finder.owner = te;
                        finder.header.items.Clear();
                        finder.elementList.Clear();
                        tom = (ManifestObject)te.manifest;
                        foreach (string s in tom.cues)
                            finder.header.items.Add(s);
                        finder.PopulateElements(tes, tgos);
                        foreach (TameElement se in finder.elementList)
                            if (te.tameType == TameKeys.Object)
                            {
                                sto = (TameObject)se;
                                to.areas.AddRange(sto.areas);
                            }
                    }
                    to.CleanAreas();
                }
            }
        }
        void SetMaterialUpdate(string updateName, TameElement updateElement, ManifestBase tmb, TameMaterial tm, TameElement tameParent)
        {
            if (updateElement != null)
            {
                tmb.updates = null;
                tm.parents.Add(new TameEffect(ManifestKeys.Update, updateElement)
                {
                    child = tm
                });
                tmb.updateType = TrackBasis.Tame;
            }
            else if (updateName != "")
            {
                tm.parents.Add(new TameEffect(ManifestKeys.Update, tameParent)
                {
                    child = tm
                });
                ManifestHeader mh = ManifestHeader.Read("update " + updateName);
                tmb.updateType = TrackBasis.Tame;
                tmb.updates = mh;
            }
            else
            {
                tm.parents.Add(new TameEffect(ManifestKeys.Update, tameParent)
                {
                    child = tm
                });
            }
        }
        void AddTameMaterial(ManifestBase tmb, Material original, int index, TameElement firstOwner, Markers.MarkerChanger[] mcs)
        {
            Material clone;
            TameMaterial tm;
            //     Debug.Log(index + " " + (tmb == null ? "null" : "not"));

            if (tmb == null)
            {
                tmb = new ManifestMaterial();
                ((ManifestMaterial)tmb).ExternalChanger(mcs);
                ((ManifestMaterial)tmb).unique = materialMarkers[index].unique;
            }
            string updateName = index < 0 ? "" : materialMarkers[index].updateByName;
            TameElement updateElement = null;
            if (index >= 0)
            {
                updateElement = materialMarkers[index].updateByElement != null ? TameGameObject.Find(materialMarkers[index].updateByElement, tgos).tameParent : null;
            }
            bool unique = index < 0 ? ((ManifestMaterial)tmb).unique : materialMarkers[index].unique;
            if (unique)
                foreach (TameGameObject tgo in tgos)
                {
                    if ((clone = TameMaterial.SwitchMaterial(tgo.gameObject, original)) != null)
                    {
                        Debug.Log(clone.name + " " + tgo.gameObject.name + " " + tgo.tameParent.name);
                        tm = new TameMaterial()
                        {
                            name = clone.name,
                            original = clone,
                            manifest = tmb,
                            index = (ushort)tes.Count,
                            owner = tgo.gameObject,
                        };
                        ((ManifestMaterial)tmb).ExternalChanger(mcs);
                        tm.CheckEmission();
                        tm.basis = tgo.tameParent.tameType == TameKeys.Time ? TrackBasis.Time : TrackBasis.Tame;
                        SetMaterialUpdate(updateName, updateElement, tmb, tm, tgo.tameParent);

                        tes.Add(tm);
                        tmb.elements.Add(tm);
                    }
                }
            else
            {
                //         Debug.Log("not unique");
                tm = new TameMaterial()
                {
                    name = original.name,
                    original = original,
                    manifest = tmb,
                    index = (ushort)tes.Count
                };
                ((ManifestMaterial)tmb).ExternalChanger(mcs);
                SetMaterialUpdate(updateName, updateElement, tmb, tm, firstOwner);

                tm.CheckEmission();
                tes.Add(tm);
                tmb.elements.Add(tm);
                //         Debug.Log("let's see");
            }
        }
        void ListMaterials(List<Material> existing, List<TameElement> firstOwner)
        {
            Material[] ms;
            Renderer r;
            bool f;
            foreach (TameGameObject tgo in tgos)
            {
                if ((r = tgo.gameObject.GetComponent<Renderer>()) != null)
                {
                    ms = r.sharedMaterials;
                    foreach (Material m0 in ms)
                    {
                        f = false;
                        foreach (Material mat in existing)
                            if (m0 == mat) { f = true; break; }
                        if (!f)
                        {
                            existing.Add(m0);
                            firstOwner.Add(tgo.tameParent);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// creates <see cref="TameMaterial"/> elements based on the manifests loaded in <see cref="Read"/> and adds them to <see cref="tes"/>. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        void IdentifyMaterials()
        {
            // 7/25 12:24
            List<Material> existing = new List<Material>();
            List<TameElement> firstOwner = new List<TameElement>();
            ListMaterials(existing, firstOwner);
            // find all materials in the interactives

            TameMaterial tm;
            Material m, clone;
            Markers.MarkerMaterial mm;
            Markers.MarkerChanger[] mcs;
            bool unique;
            // create tame materials and associate manifests with them 
            bool[] mpass = new bool[materialMarkers.Count];
            for (int i = 0; i < materialMarkers.Count; i++)
                mpass[i] = false;

            foreach (ManifestBase tmb in manifests)
                if (tmb.header.key == TameKeys.Material)
                {
                    foreach (string item in tmb.header.items)
                    {
                        //     Debug.Log("changer " + item + " of " + existing.Count);
                        //   Debug.Log(firstOwner.Count + " " + existing.Count);
                        for (int i = 0; i < existing.Count; i++)
                            if (existing[i] != null)
                                if (item.ToLower().Equals((m = existing[i]).name.ToLower()))
                                {
                                    mcs = Markers.MarkerMaterial.FirstMatch(materialMarkers, m, out int index);
                                    if (index >= 0)
                                        mpass[index] = true;
                                    //           Debug.Log(index + " " + i + " " + existing.Count);
                                    AddTameMaterial(tmb, m, index, firstOwner[i], mcs);
                                }
                        for (int i = 0; i < mpass.Length; i++)
                            for (int j = 0; j < existing.Count; j++)
                                if ((!mpass[i]) && (materialMarkers[i].material != null))
                                    if (existing[j] == materialMarkers[i].material)
                                    {
                                        mpass[i] = true;
                                        mcs = materialMarkers[i].gameObject.GetComponents<Markers.MarkerChanger>();
                                        AddTameMaterial(null, materialMarkers[i].material, i, firstOwner[j], mcs);
                                        break;
                                    }
                    }
                }
            for (int i = 0; i < materialMarkers.Count; i++)
                if ((!mpass[i]) && (materialMarkers[i].material == null))
                {
                    Light light = materialMarkers[i].gameObject.GetComponent<Light>();
                    if (light != null)
                        lightMarkers.Add(materialMarkers[i]);
                }
        }

        /// <summary>
        /// finds and assigns every <see cref="TameElement"/> in <see cref="tes"/> to the manifest loaded by <see cref="Read"/>. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        void IdentifyElements()
        {
            // 7/25 12:26
            TameFinder finder = new TameFinder();
            foreach (ManifestBase tmb in manifests)
            {
                switch (tmb.header.key)
                {
                    case TameKeys.Light:
                    case TameKeys.Object:
                        finder.header = tmb.header;
                        finder.elementList.Clear();
                        finder.PopulateElements(tes, tgos);
                        finder.RemoveDuplicate(TameFinder.Tame);
                        foreach (TameElement te in finder.elementList)
                        {
                            te.manifest = tmb;
                            tmb.elements.Add(te);
                            //       Debug.Log("id: " + te.name);
                        }
                        break;
                }
            }
            Markers.MarkerObject mo;
            string[] lines;
            foreach (TameElement te in tes)
                if (te.tameType == TameKeys.Object)
                    if ((mo = te.owner.GetComponent<Markers.MarkerObject>()) != null)
                    {
                        if (mo.manifestLines != "")
                        {
                            lines = Utils.Split(mo.manifestLines, "\n\r:;").ToArray();
                            if (te.manifest == null)
                                te.manifest = new ManifestObject();
                 //           Debug.Log("read<: "+lines[0]);
                            ((ManifestObject)te.manifest).Read(lines, -1);
                        }
               //         Debug.Log("from<: "+te.name +" "+te.manifest.manager.Speed+ " > " + mo.manifestLines);
                    }
        }
        void IdentifyLights()
        {
            for (int i = 0; i < lightMarkers.Count; i++)
            {
                string updateName = lightMarkers[i].updateByName;
                TameElement updateElement = lightMarkers[i].updateByElement != null ? TameGameObject.Find(lightMarkers[i].updateByElement, tgos).tameParent : null;
                Light light = materialMarkers[i].gameObject.GetComponent<Light>();
                Markers.MarkerChanger[] mcs = light.gameObject.GetComponents<Markers.MarkerChanger>();
                foreach (TameElement te in tes)
                    if (te.tameType == TameKeys.Light)
                    {
                        TameLight tl = (TameLight)te;
                        if (tl.light == light)
                        {
                            if (tl.manifest == null)
                            {
                                ManifestLight tlm = new ManifestLight();
                                tlm.ExternalChanger(mcs);
                                tl.manifest = tlm;
                                tlm.elements.Add(tl);
                            }
                            else
                                ((ManifestLight)tl.manifest).ExternalChanger(mcs);
                            if (updateElement != null)
                            {
                                tl.manifest.updates = null;
                                tl.parents.Add(new TameEffect(ManifestKeys.Update, updateElement)
                                {
                                    child = tl
                                });
                                tl.manifest.updateType = TrackBasis.Tame;
                            }
                            else if (updateName != "")
                            {
                                ManifestHeader mh = ManifestHeader.Read("update " + updateName);
                                tl.manifest.updateType = TrackBasis.Tame;
                                tl.manifest.updates = mh;
                            }

                        }
                    }
            }

        }

        /// <summary>
        /// establishes the parents (<see cref="TameElement.parents"/>,<see cref="TameElement.slideParents"/> and <see cref="TameElement.rotateParents"/>) for all elements in <see cref="tes"/>. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        void PopulateUpdates()
        {
            // 7/25 12:26
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    if (!((TameObject)te).isGrippable)
                        te.PopulateUpdates(tes, tgos);
                }
                else
                    te.PopulateUpdates(tes, tgos);
            }
            TameFinder tf = new TameFinder();
            foreach (TameElement te in tes)
                if (te.manifest != null)
                    if (te.manifest.affected.Count > 0)
                    {
                        tf.header.items = te.manifest.affected;
                        tf.elementList.Clear();
                        tf.PopulateElements(tes, tgos);
                        Debug.Log(te.name + " enforce ");
                        foreach (TameElement te2 in tf.elementList)
                            if (te2.basis == TrackBasis.Time)
                            {
                                //          Debug.Log(te2.name + " enforced ");
                                te2.basis = TrackBasis.Tame;
                                te2.parents.Clear();
                                te2.parents.Add(new TameEffect(ManifestKeys.Update, te));
                                if (te.manifest.forceTrigger != null)
                                    te2.progress.trigger = te.manifest.forceTrigger;
                            }
                    }
        }
        void SetProgressProperties()
        {
            TameObject to;
            float init = 0;
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    if (to.isGrippable)
                        to.progress = new TameProgress(to);
                    init = te.manifest == null ? 0 : te.manifest.initial;
                    if (to.tameGameObject.markerProgress != null)
                        if (to.tameGameObject.markerProgress.initialStatus >= 0)
                            init = to.tameGameObject.markerProgress.initialStatus;
                    to.handle.CalculateHandles(init);
                }//    ((TameObject)te).handle.CalculateHandles(0);
                te.SetProgressProperties(tes, tgos);
            }
            TameFinder tf = new TameFinder();

        }
        void SetTo()
        {
            foreach (TameElement te in tes)
            {
                if (te.manifest != null)
                {
                    if (te.manifest.setTo > 0)
                    {
                        te.progress.Initialize(te.manifest.setTo);
                        te.Update(te.progress.progress);
                    }
                    else
                    {
                        if (te.manifest.initial > 0)
                        {
                            te.progress.Initialize(te.manifest.initial);
                            te.Update(te.progress.progress);
                        }
                    }

                }
            }
        }
        void SetCustomValues()
        {
            TameElement te;
            foreach (ManifestBase tmb in manifests)
            {
                if (tmb.inputHeader != null)
                {
                    if ((te = tmb.CreateInput("$$" + tes.Count)) != null) tes.Add(te);
                    // Debug.Log("input " + tmb.elements[0].name);
                }
            }
        }
        void PopulateLinked()
        {
            TameObject to;
            TameFinder finder = new TameFinder();
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    if (to.manifest != null)
                        if (to.manifest.linkType != LinkedKeys.None)
                        {
                            finder.objectList.Clear();
                            finder.owner = te;
                            finder.header = new ManifestHeader() { items = to.manifest.linked };
                            finder.PopulateObjects(tgos);
                            if (finder.objectList.Count > 0)
                                switch (to.manifest.linkType)
                                {
                                    case LinkedKeys.Clone:
                                        to.CreateClones(finder.objectList, tes);
                                        break;
                                    case LinkedKeys.Local:
                                        to.handle.AlignLinked(LinkedKeys.Local, null, finder.objectList);
                                        break;
                                    case LinkedKeys.Stack:
                                    case LinkedKeys.Cycle:
                                        to.handle.AlignLinked(to.manifest.linkType, null, finder.objectList);
                                        to.handle.linkedOffset = to.manifest.progressedDistance;
                                        break;
                                    case LinkedKeys.Progress:
                                        Transform t = Utils.FindStartsWith(finder.objectList[0].transform.parent, TameHandles.KeyLinker);
                                        //      Debug.Log("link: " + t.name);                             
                                        to.handle.AlignLinked(LinkedKeys.Progress, t != null ? t.gameObject : null, finder.objectList);
                                        to.handle.linkedOffset = to.manifest.progressedDistance;
                                        break;

                                }
                        }
                        else if (to.manifest.queued)
                        {
                            to.handle.AlignQueued(to.manifest);
                        }
                }
            }
        }
        void SetScaled()
        {
            TameFinder finder = new TameFinder();
            foreach (TameElement te in tes)
            {
                if (te.manifest != null)
                    if (te.manifest.scales)
                    {
                        finder.objectList.Clear();
                        finder.owner = te;
                        finder.header = new ManifestHeader() { items = te.manifest.scaledObjects };
                        finder.PopulateObjects(tgos);
                        if (finder.objectList.Count > 0)
                        {
                            te.scaledObjects.AddRange(TameGameObject.ToObjectList(finder.objectList));
                            te.scaledMaterials.AddRange(TameMaterial.LocalizeMaterials(te.scaledObjects, te.initialTiles));
                        }
                    }
            }
        }
    }
}
