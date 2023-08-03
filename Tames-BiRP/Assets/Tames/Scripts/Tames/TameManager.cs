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
       // public List<ManifestBase> manifests;
        /// <summary>
        /// this is the list of successfully created manifests for <see cref="TameArea"/>s by blocks.
        /// </summary>
        public List<TameArea> area;
        /// <summary>
        /// the list of all non-key game objects that have <see cref="RootObject"/> as their parent or ancestor.
        /// </summary>
        public static List<TameGameObject> tgos;
        /// <summary>
        /// the list of <see cref="TameElement"/>s under the <see cref="RootObject"/>. 
        /// </summary>
        public static List<TameElement> tes;
        /// <summary>
        /// The list of object alternatives
        /// </summary>
        public static List<TameAlternative> altering = new List<TameAlternative>();
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
        public static List<InfoUI.InfoControl> info = new List<InfoUI.InfoControl>();
        public static List<TamePeoploid> peoploids = new();
        //    public List<Markers.MarkerMaterial> lightMarkers = new List<Markers.MarkerMaterial>();
        //       public static Walking.MoveGesture moveGesture;
        /// <summary>
        /// A game object that will contain <see cref="TameArea.relative"/> objects created during detecting interactive objects.
        /// </summary>
        public static GameObject FixedAreas = null;
        public TameManager()
        {
            //    manifests = new List<ManifestBase>();
            area = new List<TameArea>();
            tgos = new List<TameGameObject>();
            tes = new List<TameElement>();
            //       walkManifest = new List<ManifestHeader>();
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
            List<Markers.MarkerInfo> mis = new List<Markers.MarkerInfo>();
            foreach (GameObject rootObj in RootObjects)
                mis.AddRange(rootObj.GetComponentsInChildren<Markers.MarkerInfo>());
            if (FixedAreas == null) FixedAreas = new GameObject("Fixed areas");
            lines = GetLines();
            SurveyCorrespondence();
            SurveyInteractives();
            IdentifyTeleport();
            //   Debug.Log("step 1");      
            altering = TameAlternative.GetAlternatives(tgos);
            alteringMaterial = TameMaterialAlternative.GetAlternatives(tgos);
            //      if (lines.Length > 0)                Read();
            //    Debug.Log("step 2");
        //    CheckManualEligibility();
            TameCamera.AssignCamera(tgos);
            IdentifyWalk();
            if (walkManager != null)
            {
                TameCamera.SetFirstFace(walkManager.InitiatePosition(TameCamera.cameraTransform.position));
                walkManager.AssignAlternatives(altering);
            }
            else
                TameCamera.SetFirstFace(null);
            //    IdentifyElements();
            //       Debug.Log("step 3");
            RedefineCues();
            IdentifyMaterials();
            //  MaterialReference.Check();
            IdentifyLights();
            PopulateUpdates();
            //        SetCustomValues();
            SetProgressProperties();
            PopulateLinked();
            SetScaled();
            SetFlicker();
            //    SetMaster();
            //       Debug.Log("step 4");
            for (int i = 0; i < tes.Count; i++)
                if (tes[i].tameType == TameKeys.Material)
                    ((TameMaterial)tes[i]).OrderChanger();
            SetTo();
            foreach (TameGameObject tgo in tgos)
                if (tgo.gameObject.GetComponent<Markers.MarkerGrass>() != null)
                {
                    new Others.Grass(tgo.gameObject);
                }
            TameCamera.ReadCamera(tgos);
            SetLink();
            SetChangerParents();
            IdentifyScore();
            Records.TameFullRecord.allRecords = new Records.TameFullRecord(CoreTame.people);
            Markers.MarkerPerson mp;
            foreach (TameGameObject tgo in tgos)
                if ((mp = tgo.gameObject.GetComponent<Markers.MarkerPerson>()) != null)
                    if (mp.treatAsPerson)
                        peoploids.Add(new(tgo.gameObject));

            foreach (Markers.MarkerInfo inf in mis)
                info.Add(new InfoUI.InfoControl(inf));
            // sort update time > find objects
            //        Debug.Log("step 5");
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
            {
                //       Debug.Log("root " + go.name);
                tgos.AddRange(TameObject.CreateInteractive(TameTime.RootTame, go, tes, Blender));
            }
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
            {
                if (te.progMarker != null)
                    te.ReadInput(te.progMarker.control);
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
            foreach (TameElement te in tes)
                te.CleanAreas();
        }
        TameMaterial AddTameMaterial(Material original, int index, TameElement firstOwner, Markers.MarkerChanger[] mcs)
        {
            Material clone;
            TameMaterial tm = null;
            //     Debug.Log(index + " " + (tmb == null ? "null" : "not"));
            Markers.MarkerProgress mp = index >= 0 ? materialMarkers[index].gameObject.GetComponent<Markers.MarkerProgress>() : null;
            Markers.MarkerControl[] mctrl = index >= 0 ? materialMarkers[index].gameObject.GetComponents<Markers.MarkerControl>() : new Markers.MarkerControl[] { };
            Markers.MarkerSpeed ms = index >= 0 ? materialMarkers[index].gameObject.GetComponent<Markers.MarkerSpeed>() : null;

            bool unique = index < 0 ? false : materialMarkers[index].unique;
            int k = 0;
                if (original.name == "label") Debug.Log("UP: found = " + unique);
            if (unique)
                foreach (TameGameObject tgo in tgos)
                {
                    if ((clone = TameMaterial.SwitchMaterial(tgo.gameObject, original)) != null)
                    {
                        tm = new TameMaterial()
                        {
                            name = clone.name + " " + k,
                            original = clone,
                            properties = TameMaterial.ExternalChanger(mcs),
                            index = (ushort)tes.Count,
                            owner = tgo.gameObject,
                            markerProgress = mp,
                            markerSpeed = ms,
                            cloned = true,
                            //         markerFlicker = mfs
                        };
                        k++;
                        tm.SetControls(mctrl);
                        tm.CheckEmission();
                        tm.basis = tgo.tameParent.tameType == TameKeys.Time ? TrackBasis.Time : TrackBasis.Tame;
                        tm.SetProvisionalUpdate(tgo.tameParent);
                        tm.updatedUnique = true;
                        tes.Add(tm);
                    }
                }
            else
            {
                //         Debug.Log("not unique");
                tm = new TameMaterial()
                {
                    name = original.name,
                    original = original,
                    properties = TameMaterial.ExternalChanger(mcs),
                    owner = materialMarkers[index].gameObject,
                    index = (ushort)tes.Count,
                    markerProgress = mp,
                    markerSpeed = ms,
                    //           markerFlicker = mfs
                };
                Debug.Log("owner is "+tm.owner.name);
                TameGameObject tg = TameGameObject.Find(tm.owner, tgos);
                if (tg != null)
                {
                    tg.isElement = true;
                    tg.tameParent = tm;
                }
                tm.SetControls(mctrl);
                tm.CheckEmission();
                tes.Add(tm);
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
            for (int i = 0; i < mpass.Length; i++)
                if ((!mpass[i]) && (materialMarkers[i].material != null))
                    for (int j = 0; j < existing.Count; j++)
                        if (existing[j] == materialMarkers[i].material)
                        {
                            mpass[i] = true;
                            mcs = materialMarkers[i].gameObject.GetComponents<Markers.MarkerChanger>();
                            tm = AddTameMaterial(materialMarkers[i].material, i, firstOwner[j], mcs);
                            //     Debug.Log("material added " + materialMarkers[i].material.name + " " + tm.basis);
                            break;
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
                        tl.properties = TameLight.ExternalChanger(mcs);
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
                    init = 0;
                    if (to.markerProgress != null)
                        if (to.markerProgress.preset >= 0)
                            init = to.markerProgress.preset;
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

                        Debug.Log(to.name + " " + objects.Count);
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
            foreach (TameElement te in tes)
            {
                //        Debug.Log("flicker null " + (te.markerFlicker == null));

                if (te.properties.Count > 0)
                {
                    list.Clear();
                    firstFlicker = null;
                    //       Debug.Log("flicker " + te.name + " " + te.manifest.properties.Count);
                    foreach (TameChanger ch in te.properties)
                        if (ch.marker.flicker.active)
                        {
                            //           Debug.Log("flicker checking " + te.name + " " + ch.property);
                            ch.toggleType = ToggleType.Flicker;
                            list.Add(ch);
                            break;
                        }
                    for (int i = 0; i < list.Count; i++)
                        if ((firstFlicker = GetFlickerParent(list[i].marker)) != null)
                            break;
                    //    Debug.Log("flicker parent " + (firstFlicker == null) + " " + list.Count);
                    for (int i = 0; i < list.Count; i++)
                        if (firstFlicker != null)
                            list[i].flickerParent = firstFlicker;
                        else
                        {
                            firstFlicker = list[i];
                            list[i].SetFlickerPlan();
                        }


                }
            }
        }
        TameChanger GetFlickerParent(Markers.MarkerChanger mf)
        {
            if (mf.flicker.byMaterial != null)
            {
                TameMaterial tm = TameMaterial.Find(mf.flicker.byMaterial, tes);
                if (tm != null)
                    if (tm.properties.Count >= 0)
                    {
                        foreach (TameChanger ch in tm.properties)
                            if (ch.property == mf.GetProperty())
                                return ch;
                    }
            }
            if (mf.flicker.byLight != null)
            {
                foreach (TameElement te in tes)
                    if (te.tameType == TameKeys.Light)
                        if (te.owner == mf.flicker.byLight)
                            foreach (TameChanger ch in te.properties)
                                if (ch.property == mf.GetProperty())
                                    return ch;

            }
            return null;
        }
        void SetChangerParents()
        {
            foreach (TameElement te in tes)
                if ((te.tameType == TameKeys.Light) || (te.tameType == TameKeys.Material))
                    foreach (TameChanger ch in te.properties)
                        ch.FindParent(tes);
        }

        public static void UpdatePeoploids()
        {
            foreach (TamePeoploid p in peoploids)
                p.ChangeFrame();
        }
        public static List<TameScoreBasket> basket = new();
        public void IdentifyScore()
        {
            TameGameObject tg;
            List<TameScore> score = new List<TameScore>();
            Markers.MarkerScore ms;
            foreach (TameGameObject tgo in tgos)
                if ((ms = tgo.gameObject.GetComponent<Markers.MarkerScore>()) != null)
                {
                    if (ms.isBasket)
                    {
                        basket.Add(new TameScoreBasket(ms));
                        basket[^1].FindElements(tgos);
                    }
                    else
                        score.Add(new TameScore(ms));
                }
            foreach (TameScore ts in score)
                foreach (TameScoreBasket tb in basket) if (ts.marker.basket == tb.marker.gameObject)
                    {
                        tb.scores.Add(ts);
                        break;
                    }
            foreach (TameScore ts in score)
                ts.FindElements(tgos);
        }
        public static List<Markers.MarkerTeleport> teleport = new List<Markers.MarkerTeleport>();
        public void IdentifyTeleport()
        {
            Markers.MarkerTeleport ms;
            foreach (TameGameObject tgo in tgos)
                if ((ms = tgo.gameObject.GetComponent<Markers.MarkerTeleport>()) != null)
                {
                    Debug.Log(ms.name);
                    teleport.Add( ms);
                    ms.control.AssignControl(Markers.InputSetting.ControlTypes.DualPress);
                }
        }
        public static void UpdateScores()
        {
            foreach (TameScoreBasket ms in basket)
                ms.Update();
        }
        public static void RecalculateInfo()
        {
            foreach (InfoUI.InfoControl fo in info)
                fo.Calculate(false);
        }
    }
}
