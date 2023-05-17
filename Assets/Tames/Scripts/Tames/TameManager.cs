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
        public const int Blender = 1;
        public const int Max3DS = 2;
        public const int Rhino = 3;
        /// <summary>
        /// The general settings as attached to the <see cref="RootObject"/>. 
        /// </summary>
        public static Markers.MarkerSettings settings;
        /// <summary>
        /// the path of mainfest file in the Resources folder
        /// </summary>
        public static string ManifestPath = "manifest";
        /// <summary>
        /// a root <see cref="GameObject"/> named "interactives" that contains all interactive elements 
        /// </summary>
        public static List<GameObject> RootObjects = null;
        public static GameObject DefObject = null;
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
        /// <summary>
        /// The list of object alternatives
        /// </summary>
        public List<TameAlternative> altering = new List<TameAlternative>();
        /// <summary>
        /// The list of material alternatives.
        /// </summary>
        public List<TameMaterialAlternative> alteringMaterial = new List<TameMaterialAlternative>();
        /// <summary>
        /// the walk manager for this manifest
        /// </summary>
        public static Walking.WalkManager walkManager = null;
        /// <summary>
        /// The list of correspondence logics. 
        /// </summary>
        public List<TameCorrespond> matches = new List<TameCorrespond>();
        /// <summary>
        /// The list of material markers used to distinguish dynamic materials
        /// </summary>
        public List<Markers.MarkerMaterial> materialMarkers = new List<Markers.MarkerMaterial>();
        public static List<GameObject> peoploids = new ();
        //    public List<Markers.MarkerMaterial> lightMarkers = new List<Markers.MarkerMaterial>();
        //       public static Walking.MoveGesture moveGesture;
        /// <summary>
        /// A game object that will contain <see cref="TameArea.relative"/> objects created during detecting interactive objects.
        /// </summary>
        public static GameObject FixedAreas = null;
        public TameManager()
        {
            manifests = new List<ManifestBase>();
            area = new List<TameArea>();
            tgos = new List<TameGameObject>();
            tes = new List<TameElement>();
            //       walkManifest = new List<ManifestHeader>();
        }
        /// <summary>
        /// imports the manifest lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        string[] Import(string[] lines, ManifestHeader header)
        {
            List<string> result = new List<string>();
            string[] l;
            result.AddRange(lines);
            foreach (string imn in header.items)
            {
                l = Identifier.LoadLines(imn);
                Debug.Log("import " + l.Length);
                result.AddRange(l);
            }
            return result.ToArray();
        }
        /// <summary>
        /// reads the manifest file and creates all custom abstract <see cref="TameArea"/>s, <see cref="TameMaterial"/>s and other modifies <see cref="TameElement"/>s. This method is called from within <see cref="Initialize"/>
        /// </summary>
        bool Read()
        {
            // 7/25 11:55
            if (RootObjects == null) return false;
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
                        //          case TameKeys.Alter: i = TameAltering.Read(header, altering, i); break;
                        case TameKeys.Match: i = ManifestMatch.Read(lines, i, matches); break;
                    }
                }
                i++;
            }
            return true;
        }
        /// <summary>
        /// Adds an area object under the <see cref="FixedAreas"/> game object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddArea(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = FixedAreas.transform;
            return go;
        }
        /// <summary>
        /// Converts the lines in the <see cref="Markers.MarkerSettings.customManifests"/> into an string array to be read by <see cref="Import(string[], ManifestHeader)"/>
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private string[] GetLines()
        {
            if (settings != null)
            {
                string[] s = settings.GetManifest().Split("\n");
                return s;
            }
            return new string[0];
        }
        /// <summary>
        /// loads the manifest
        /// </summary>
        /// <param name="l">not used anymore</param>
        public void Initialize(string[] l = null)
        {
            // 7/25 11:57
            // lines = l;
            //     Debug.Log("lines = " + l.Length);

            GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
            Markers.MarkerSettings ms;
            Markers.ExportOption eo;
            FixedAreas = null;
            RootObjects = new();
            Markers.MarkerRoot mr;
            foreach (GameObject rootObj in root)
            {
                if ((mr = rootObj.GetComponent<Markers.MarkerRoot>()) != null) if (mr.active) RootObjects.Add(rootObj);
                if ((ms = rootObj.GetComponent<Markers.MarkerSettings>()) != null) settings = ms;
                if ((eo = rootObj.GetComponent<Markers.ExportOption>()) != null) CoreTame.exportOption = eo;
                if (rootObj.name == "Fixed areas") FixedAreas = rootObj;
            }
            if (FixedAreas == null) FixedAreas = new GameObject("Fixed areas");
            lines = GetLines();
            SurveyCorrespondence();
            SurveyInteractives();
            //    SurveyMaterials();
            altering = TameAlternative.GetAlternatives(tgos);
            alteringMaterial = TameMaterialAlternative.GetAlternatives(tgos);
            if (lines.Length > 0)
                Read();
            CheckManualEligibility();
            TameCamera.AssignCamera(tgos);
            IdentifyWalk();
            if (walkManager != null)
            {
                TameCamera.SetFirstFace(walkManager.InitiatePosition(TameCamera.cameraTransform.position));
                walkManager.AssignAlternatives(altering);
            }
            else
                TameCamera.SetFirstFace(null);
            IdentifyElements();
            RedefineCues();
            IdentifyMaterials();
            IdentifyLights();
            PopulateUpdates();
            SetCustomValues();
            SetProgressProperties();
            PopulateLinked();
            SetScaled();
            SetFlicker();
            SetMaster();
            for (int i = 0; i < tes.Count; i++)
                if (tes[i].tameType == TameKeys.Material)
                    if (tes[i].manifest != null)
                        ((ManifestMaterial)tes[i].manifest).OrderChanger();
            SetTo();
            foreach (TameGameObject tgo in tgos)
                if (tgo.gameObject.GetComponent<Markers.MarkerGrass>() != null)
                {
                    new Others.Grass(tgo.gameObject);
                }
            TameCamera.ReadCamera(tgos);
            SetLink();

            Records.TameFullRecord.allRecords = new Records.TameFullRecord(CoreTame.people);
            Markers.MarkerPerson mp;
            foreach (TameGameObject tgo in tgos)
                if ((mp = tgo.gameObject.GetComponent<Markers.MarkerPerson>()) != null)
                    if (mp.treatAsPerson)
                        peoploids.Add(tgo.gameObject);
            // sort update time > find objects
        }

        void SurveyCorrespondence()
        {
            List<Markers.MarkerCorrespond> mcs = new();
            foreach (GameObject go in RootObjects) mcs.AddRange(go.GetComponentsInChildren<Markers.MarkerCorrespond>());
            foreach (Markers.MarkerCorrespond marker in mcs)
            {
                TameCorrespond tc = new TameCorrespond(marker);
                tc.Match();
            }
        }
        /// <summary>
        /// identifies all interactive objects and lights inside the <see cref="RootObject"/> and stores them in <see cref="tes"/>. For this process see <see cref="TameHandles"/>. In addition, it stores all game objects in <see cref="tgos"/>. This method is called from within <see cref="Initialize"/>
        /// </summary>
        /// <param name="root"></param>
        void SurveyInteractives()
        {
            // 7/25 11:56
            GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
            Markers.MarkerArea.PopulateAll(RootObjects.ToArray());
            foreach (GameObject go in RootObjects)
                tgos.AddRange(TameObject.CreateInteractive(TameTime.RootTame, go, tes, Blender));
            Markers.MarkerCustom mc;
            for (int i = 0; i < tgos.Count; i++)
                if ((mc = tgos[i].gameObject.GetComponent<Markers.MarkerCustom>()) != null)
                {
                    TameCustomValue.FromMarker(mc, tes);
                }
            Markers.MarkerProgress.PopulateAll(tgos);
            Markers.MarkerMaterial mt;
            foreach (TameGameObject tgo in tgos)
                if ((mt = tgo.gameObject.GetComponent<Markers.MarkerMaterial>()) != null)
                    materialMarkers.Add(mt);
            //    Debug.Log("material count " + materialMarkers.Count);
            foreach (GameObject rootObj in root)
                if (rootObj.name == "BASEmodel")
                {
                    Debug.Log("made");
                    //     moveGesture = new Walking.MoveGesture(rootObj);
                    break;
                }
        }


        void CheckManualEligibility()
        {
            foreach (TameElement te in tes)
                if (te.markerProgress != null)
                    if (te.markerProgress.manual)
                    {
                        te.ReadInput(te.markerProgress.update);
                    }
        }
        void AssignMatches()
        {
            foreach (TameCorrespond match in matches)
            {
                //         match.Match(tgos, tes);
            }
        }
        /// <summary>
        /// creates the <see cref="walkManager"/> 
        /// </summary>
        void IdentifyWalk()
        {
            TameFinder finder = new TameFinder();
            bool f = false;
            Markers.MarkerWalk mw;
            foreach (TameGameObject go in tgos)
                if ((mw = go.gameObject.GetComponent<Markers.MarkerWalk>()) != null)
                {
                    f = false;
                    foreach (TameGameObject g in finder.objectList)
                        if (go.gameObject == g.gameObject)
                        { f = true; break; }
                    if (!f)
                        finder.objectList.Add(go);
                }
            if (finder.objectList.Count > 0)
            {
                walkManager = new Walking.WalkManager(finder.objectList, true);
                foreach (TameGameObject tg in finder.objectList)
                    if ((mw = tg.gameObject.GetComponent<Markers.MarkerWalk>()) != null)
                        tg.gameObject.SetActive(mw.visible);
                    else if (tg.gameObject.name.StartsWith("_"))
                    {
                        tg.gameObject.SetActive(false);
                    }
            }
            else
                walkManager = null;
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
                }
                te.CleanAreas();
            }
        }
        TameMaterial AddTameMaterial(ManifestBase tmb, Material original, int index, TameElement firstOwner, Markers.MarkerChanger[] mcs, Markers.MarkerFlicker[] mfs)
        {
            Material clone;
            TameMaterial tm = null;
            //     Debug.Log(index + " " + (tmb == null ? "null" : "not"));

            if (tmb == null)
            {
                tmb = new ManifestMaterial();
                ((ManifestMaterial)tmb).ExternalChanger(mcs);
                ((ManifestMaterial)tmb).unique = materialMarkers[index].unique;
            }
            Markers.MarkerProgress mp = index >= 0 ? materialMarkers[index].gameObject.GetComponent<Markers.MarkerProgress>() : null;
            Markers.MarkerSpeed ms = index >= 0 ? materialMarkers[index].gameObject.GetComponent<Markers.MarkerSpeed>() : null;

            bool unique = index < 0 ? ((ManifestMaterial)tmb).unique : materialMarkers[index].unique;
            int k = 0;
            //    if (original.name == "barrier sign") Debug.Log("UP: uniq = " + unique);
            if (unique)
                foreach (TameGameObject tgo in tgos)
                {
                    if ((clone = TameMaterial.SwitchMaterial(tgo.gameObject, original)) != null)
                    {
                        tm = new TameMaterial()
                        {
                            name = clone.name + " " + k,
                            original = clone,
                            manifest = tmb,
                            index = (ushort)tes.Count,
                            owner = tgo.gameObject,
                            markerProgress = mp,
                            markerSpeed = ms,
                            cloned = true,
                            markerFlicker = mfs
                        };
                        k++;
                        ((ManifestMaterial)tmb).ExternalChanger(mcs);
                        tm.CheckEmission();
                        tm.basis = tgo.tameParent.tameType == TameKeys.Time ? TrackBasis.Time : TrackBasis.Tame;
                        tm.SetProvisionalUpdate(tgo.tameParent);
                        Debug.Log(tm.name + " " + tgo.gameObject.name + " " + tgo.tameParent.name);
                        tm.updatedUnique = true;
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
                    index = (ushort)tes.Count,
                    markerProgress = mp,
                    markerSpeed = ms,
                    markerFlicker = mfs
                };
                ((ManifestMaterial)tmb).ExternalChanger(mcs);
                //      if (original.name == "barrier sign") Debug.Log("UP: not uniq");
                //    tm.SetProvisionalUpdate(firstOwner);

                tm.CheckEmission();
                tes.Add(tm);
                tmb.elements.Add(tm);
                //    Debug.Log("UP: " + tm.name + " " + firstOwner.name);

                return tm;
            }
            return null;
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
        /// creates <see cref="TameMaterial"/> elements based on the manifests loaded in <see cref="Read"/> and adds them to <see cref="tes"/>. This method is called from within <see cref="Initialize"/>
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
            Markers.MarkerFlicker[] mf;
            bool unique;
            // create tame materials and associate manifests with them 
            bool[] mpass = new bool[materialMarkers.Count];
            for (int i = 0; i < materialMarkers.Count; i++)
                mpass[i] = false;
            Markers.MarkerProgress mp;
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
                                    mf = index >= 0 ? mcs[0].gameObject.GetComponents<Markers.MarkerFlicker>() : null;
                                    AddTameMaterial(tmb, m, index, firstOwner[i], mcs, mf);
                                }
                    }
                }
            for (int i = 0; i < mpass.Length; i++)
                if ((!mpass[i]) && (materialMarkers[i].material != null))
                    for (int j = 0; j < existing.Count; j++)
                        if (existing[j] == materialMarkers[i].material)
                        {
                            mpass[i] = true;
                            mcs = materialMarkers[i].gameObject.GetComponents<Markers.MarkerChanger>();
                            mf = mcs[0].gameObject.GetComponents<Markers.MarkerFlicker>();
                            if (mf.Length == 0) mf = null;
                            tm = AddTameMaterial(null, materialMarkers[i].material, i, firstOwner[j], mcs, mf);
                            //     Debug.Log("material added " + materialMarkers[i].material.name + " " + tm.basis);
                            break;
                        }



        }

        /// <summary>
        /// finds and assigns every <see cref="TameElement"/> in <see cref="tes"/> to the manifest loaded by <see cref="Read"/>. This method is called from within <see cref="Initialize"/>
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
                {
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
        }

        void IdentifyLights()
        {
            foreach (TameElement te in tes)
                if (te.tameType == TameKeys.Light)
                {
                    TameLight tl = (TameLight)te;
                    Markers.MarkerChanger[] mcs = te.owner.GetComponents<Markers.MarkerChanger>();
                    if (mcs != null)
                    {
                        if (tl.manifest == null)
                        {
                            ManifestLight tlm = new ManifestLight();
                            tl.manifest = tlm;
                            tlm.elements.Add(tl);
                        }
                        ((ManifestLight)tl.manifest).ExternalChanger(mcs);
                    }
                    else
                        ((ManifestLight)tl.manifest).ExternalChanger(mcs);
                }
        }

        /// <summary>
        /// establishes the parents (<see cref="TameElement.parents"/>,<see cref="TameElement.slideParents"/> and <see cref="TameElement.rotateParents"/>) for all elements in <see cref="tes"/>. This method is called from within <see cref="Initialize"/>
        /// </summary>
        void PopulateUpdates()
        {
            // 7/25 12:26
            foreach (TameElement te in tes)
            {
                if (!te.manual)
                {
                    if (te.tameType == TameKeys.Object)
                    {
                        if (!((TameObject)te).isGrippable)
                            te.PopulateUpdates(tes, tgos);
                    }
                    else
                        te.PopulateUpdates(tes, tgos);
                }
                else te.basis = TrackBasis.Manual;
            }
            TameFinder tf = new TameFinder();
            foreach (TameElement te in tes)
                if (!te.manual)
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
                    if (to.markerProgress != null)
                        if (to.markerProgress.initialStatus >= 0)
                            init = to.markerProgress.initialStatus;
                    //   Debug.Log("CSP: "+to.name+" init " + init);
                    to.handle.SetMover();
                    to.handle.CalculateHandles(init);
                }//    ((TameObject)te).handle.CalculateHandles(0);
                te.SetProgressProperties(tes, tgos);
                //         Debug.Log("TEN: " + te.name);
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
            List<string> linked = new List<string>();
            TameLinkManager tlm;
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    tlm = new TameLinkManager() { element = (TameObject)te };
                    tlm.Populate(tgos, tes);
                }
            }
        }
        void SetScaled()
        {
            TameFinder finder = new TameFinder();
            Markers.MarkerScale ms;
            TameGameObject tgo;
            foreach (TameElement te in tes)
            {
                List<GameObject> objects = new List<GameObject>();
                if (te.tameType == TameKeys.Object)
                {
                    TameObject to = (TameObject)te;
                    if ((ms = to.owner.GetComponent<Markers.MarkerScale>()) != null)
                    {
                        if (ms.byName != null)
                        {
                            finder.objectList.Clear();
                            finder.owner = te;

                            finder.header = ManifestHeader.Read("update " + ms.byName);
                            finder.PopulateObjects(tgos);
                            objects.AddRange(TameGameObject.ToObjectList(finder.objectList));
                        }
                        if (ms.byObject != null)
                            objects.Add(ms.byObject);
                        if (ms.childrenOf != null)
                            for (int i = 0; i < ms.childrenOf.transform.childCount; i++)
                                objects.Add(ms.childrenOf.transform.GetChild(i).gameObject);


                        if (objects.Count > 0)
                        {
                            te.scaledObjects = objects;
                            te.scaledMaterials = TameMaterial.LocalizeMaterials(objects, te.initialTiles);
                            to.scales = true;
                            to.scaleFrom = ms.from;
                            to.scaleTo = ms.to;
                            to.scaleUV = ms.affectedUV == Markers.MarkerScale.AffectUV.U ? 0 : 1;
                            to.scaleAxis = ms.axis switch { Markers.MarkerScale.ScaleAxis.X => 0, Markers.MarkerScale.ScaleAxis.Y => 1, _ => 2 };
                        }
                    }
                }
                /*  if (te.manifest != null)
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
                */
            }
        }
        void SetLink()
        {
            Markers.MarkerLink ml;
            List<Markers.MarkerLink> clones = new();
            List<Markers.MarkerLink> links = new();

            foreach (TameGameObject tgo in tgos)
                if ((ml = tgo.gameObject.GetComponent<Markers.MarkerLink>()) != null)
                {
                    if (ml.type != Markers.MarkerLink.CloneTypes.LinkMover)
                        clones.Add(ml);
                    else
                        links.Add(ml);
                }
            List<TameElement> parents = new();
            TameElement tep;
            TameGameObject tgp;
            int index;
            foreach (Markers.MarkerLink mli in clones)
                if (mli.parent != null)
                {
                    tgp = TameGameObject.Find(mli.parent, tgos);
                    if (tgp != null)
                        if (tgp.tameParent.tameType == TameKeys.Object)
                        {
                            if ((index = parents.IndexOf(tgp.tameParent)) < 0)
                            {
                                parents.Add(tgp.tameParent);
                                index = parents.Count - 1;
                            }
                            parents[index].AddClones(mli, true, tgos);
                        }
                }
                else
                {
                    tgp = TameGameObject.Find(mli.gameObject, tgos);
                    if (tgp != null)
                        if (tgp.tameParent.tameType == TameKeys.Object)
                        {
                            if ((index = parents.IndexOf(tgp.tameParent)) < 0)
                            {
                                parents.Add(tgp.tameParent);
                                index = parents.Count - 1;
                            }
                            parents[index].AddClones(mli, false, tgos);
                        }
                }
            foreach (TameElement e in parents)
                tes.AddRange(e.PopulateClones());
            parents.Clear();
            foreach (Markers.MarkerLink mli in links)
                if (mli.parent != null)
                {
                    tgp = TameGameObject.Find(mli.parent, tgos);
                    if (tgp != null)
                        if (tgp.tameParent.tameType == TameKeys.Object)
                        {
                            if ((index = parents.IndexOf(tgp.tameParent)) < 0)
                            {
                                parents.Add(tgp.tameParent);
                                index = parents.Count - 1;
                            }
                            parents[index].AddLinks(mli, true, tgos);
                        }
                }
                else
                {
                    tgp = TameGameObject.Find(mli.gameObject, tgos);
                    if (tgp != null)
                        if (tgp.tameParent.tameType == TameKeys.Object)
                        {
                            if ((index = parents.IndexOf(tgp.tameParent)) < 0)
                            {
                                parents.Add(tgp.tameParent);
                                index = parents.Count - 1;
                            }
                            parents[index].AddLinks(mli, false, tgos);
                        }
                }
            foreach (TameElement e in parents)
                e.PopulateLinks();
        }
        void SetFlicker()
        {
            TameChanger firstFlicker;
            List<TameChanger> list = new List<TameChanger>();
            List<Markers.MarkerFlicker> mfs = new();
            Markers.MarkerFlicker fmf = null;
            foreach (TameElement te in tes)
            {
                //        Debug.Log("flicker null " + (te.markerFlicker == null));

                if (te.markerFlicker != null)
                    if (te.manifest != null)
                    {
                        list.Clear();
                        mfs.Clear();
                        firstFlicker = null;
                        //       Debug.Log("flicker " + te.name + " " + te.manifest.properties.Count);
                        foreach (Markers.MarkerFlicker mf in te.markerFlicker)
                            foreach (TameChanger ch in te.manifest.properties)
                            {
                                //       Debug.Log("flicker checking " + te.name + " " + ch.property);
                                if (mf.GetProperty() == ch.property)
                                {
                                    ch.toggleType = ToggleType.Flicker;
                                    list.Add(ch);
                                    mfs.Add(mf);
                                    break;
                                }
                            }
                        for (int i = 0; i < list.Count; i++)
                            if ((firstFlicker = GetFlickerParent(mfs[i])) != null)
                                break;
                        for (int i = 0; i < list.Count; i++)
                            if (firstFlicker != null)
                                list[i].flickerParent = firstFlicker;
                            else
                            {
                                firstFlicker = list[i]; list[i].SetFlickerPlan(mfs[i]);
                            }
                    }
            }
        }
        TameChanger GetFlickerParent(Markers.MarkerFlicker mf)
        {
            if (mf.byMaterial != null)
            {
                TameMaterial tm = TameMaterial.Find(mf.byMaterial, tes);
                if (tm != null)
                    if (tm.manifest != null)
                    {
                        foreach (TameChanger ch in tm.manifest.properties)
                            if (ch.property == mf.GetProperty())
                                return ch;
                    }
            }
            if (mf.byLight != null)
            {
                foreach (TameElement te in tes)
                    if (te.tameType == TameKeys.Light)
                        if (te.owner == mf.byLight)
                            if (te.manifest != null)
                            {
                                foreach (TameChanger ch in te.manifest.properties)
                                    if (ch.property == mf.GetProperty())
                                        return ch;
                            }
            }
            return null;
        }
        void SetMaster()
        {
            List<Markers.MarkerMaster> mms = new List<Markers.MarkerMaster>();
            Markers.MarkerMaster mm;
            foreach (TameGameObject tgo in tgos)
                if ((mm = tgo.gameObject.GetComponent<Markers.MarkerMaster>()) != null)
                {
                    mms.Add(mm);
                }
            TameFinder finder = new TameFinder();
            Markers.MarkerProgress mp;
            foreach (Markers.MarkerMaster mi in mms)
                if ((mp = mi.gameObject.GetComponent<Markers.MarkerProgress>()) != null)
                {
                    TameKeys tk = mi.Type;
                    finder.elementList.Clear();
                    switch (tk)
                    {
                        case TameKeys.Object:
                            finder.header = ManifestHeader.Read("object " + mi.elements); break;
                        case TameKeys.Material:
                            finder.header = ManifestHeader.Read("material " + mi.elements); break;
                        case TameKeys.Light:
                            finder.header = ManifestHeader.Read("light " + mi.elements); break;
                    }
                    finder.PopulateElements(tes, tgos);
                    foreach (TameElement te in finder.elementList)
                        if (tk == te.tameType)
                        {
                            if (mi.durations)
                            {
                                te.SetDurations(mp);
                            }
                            if (mi.updates)
                            {
                                te.PopulateUpdateByMarker(tes, tgos, mp);
                            }
                            if (mi.showKeys)
                            {
                                te.SetShowKeys(mp);
                            }
                            if (mi.activeKeys)
                            {
                                te.SetActiveKeys(mp);
                            }
                        }
                }
        }
    }
}
