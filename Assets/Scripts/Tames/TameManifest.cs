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
    /// this class compiles a line of the manifest file for further use
    /// </summary>
    public class ManifestHeader
    {
        /// <summary>
        /// the header string. See <see cref="key"/> and <see cref="subKey"/> for valid header strings
        /// </summary>
        public string header = "";
        /// <summary>
        /// the major key of the line that declares start of a block for a <see cref="TameElement"/> or <see cref="TameArea"/>. The default is None, meaning that the line is not the start of a block. If the first line of the word is one of the following, the line is deemed as the start of a block corresponding to <see cref="TameKeys"/> of the same name: object, material, light, interact. 
        /// After the key, for the <see cref="TameElement"/> types their names (see <see cref="TameFinder.Relations"/> for patterns) will be written separated by commas. For <see cref="TameArea"/> the <see cref="TameArea.mode"/> is defined (inside, outside, inout, outin, grip, see <see cref="InteractionMode"/>).
        /// </summary>
        public TameKeys key = TameKeys.None;
        /// <summary>
        /// the subkey of the line that declares a feature or property of the major type. Accepted subkeys include:
        /// For all classes based on <see cref="TameElement"/>:
        /// update (<see cref="ManifestKeys.Update"/>): defines the slide parents of a <see cref="TameElement"/>. See <see cref="TameElement.parents"/> and for interactors see <see    cref="TameArea.attachedObjects"/>.
        /// trigger (<see cref="ManifestKeys.Trigger"/>): defines trigger for the element progress. If this is not defined or is invalid, the element is updated along with its parent. Otherwise it is updated according to the trigger values (see <see cref="TameProgress.trigger"/>). The trigger is defined by two numbers separated by comma.
        /// 
        /// For <see cref="TameObject"/>:
        /// slide (<see cref="ManifestKeys.Slide"/>): defines the slide parents of a <see cref="TameObject"/>. See <see cref="TameElement.slideParents"/>
        /// rotate (<see cref="ManifestKeys.Slide"/>): defines the rotate parents of a <see cref="TameObject"/>. See <see cref="TameElement.rotateParents"/>
        /// int (<see cref="ManifestKeys.Int"/>): adds interactors to a <see cref="TameObject"/>. See <see cref="TameElement.AddUpdate(ManifestKeys, List{TameGameObject})"/>
        /// 
        /// For <see cref="TameMaterial"/> and <see cref="TameLight"/>:
        /// intensity (<see cref="ManifestKeys.Bright"/>): defines a [float] <see cref="TameChanger"/> the intensity of light or the brightness of the material emission color. For the syntax see <see cref="TameChanger.Read"/>
        /// color (<see cref="ManifestKeys.Color"/>): defines a <see cref="TameColor"/> for the base color of the material or light. For lights, it is the same as "spectrum".  
        /// spectrum (<see cref="ManifestKeys.Glow"/>): defines a <see cref="TameColor"/> for the emission color of the material or light. For lights, it is the same as "color"
        /// 
        /// For <see cref="TameMaterial"/>
        /// mapx (<see cref="ManifestKeys.MapX"/>): defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the main texture of a material.
        /// mapy (<see cref="ManifestKeys.MapY"/>): defines a [float] <see cref="TameChanger"/> for the y element of the texture offset for the main texture of a material.
        /// lightx (<see cref="ManifestKeys.LightX"/>):defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the emission map of a material.
        /// lighty (<see cref="ManifestKeys.LightY"/>): defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the emission map of a material.
        /// 
        /// For <see cref="TameArea"/>:
        /// size (<see cref="ManifestKeys.Size"/>):defines the object or size and shape of the interactor. It can have either the name of a game object or of up to three floats separated by commas. If it is the game object, the <see cref="TameArea.geometry"/> is considered <see cref="InteractionGeometry.Box"/> with the gameobject's transform. Otherwise, the number of floats defines the geometry (1: as the radius of a sphere, 2: as the radius and height of a cylinder, 3: as x, y, z scale of a box).
        /// attach (<see cref="ManifestKeys.Attach"/>): the name of game object that the interactor attaches to, subject to <see cref="TameFinder.Relations"/> patterns (without rotation ~ or position @ operators). The names are separated by comma and stored in <see cref="TameArea.attachedObjects"/>
        /// follow (<see cref="ManifestKeys.Follow"/>): the value of <see cref="TameArea.update"/> (fixed, parent, object or mover). 
        /// </summary>
        public int subKey = ManifestKeys.None;
        /// <summary>
        /// the list of delimited strings after a key or subkey (the key is not included). If the list includes names of objects, interactors or elements, they are delimited by comma, otherwise by space. 
        /// </summary>
        public List<string> items = new List<string>();
        /// <summary>
        /// creates a manifest header based on a line. It assigns the <see cref="key"/>, <see cref="subKey"/> and <see cref="items"/>. 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static ManifestHeader Read(string line)
        {
            // 7/25 11:43
            ManifestHeader r = new ManifestHeader();
            string cleanLine = Utils.Clean(line);
            //    Debug.Log("cleanline = " + cleanLine + ": " + line);
            cleanLine = Utils.RemoveDuplicate(cleanLine, " \t");
            cleanLine = Utils.RemoveDuplicate(cleanLine, ",");
            string header = cleanLine;
            r.header = header;
            string content = "";
            List<string> contentList;
            if (cleanLine.Length > 0)
            {
                for (int i = 0; i < cleanLine.Length; i++)
                    if (" \t".IndexOf(cleanLine[i]) >= 0)
                    {
                        header = cleanLine.Substring(0, i);
                        r.header = header;
                        content = cleanLine.Substring(i + 1);
                        break;
                    }
                //  Debug.Log(":::"+cleanLine + "+" + r.header + "+" + content);
                string delim = " ";
                if (header.Length > 0)
                    r.key = GetType(r.header);
                if (r.key == TameKeys.None)
                {
                    r.subKey = GetSubType(r.header);
                    switch (r.subKey)
                    {
                        case ManifestKeys.Update:
                        case ManifestKeys.Track:
                        case ManifestKeys.Follow:
                        case ManifestKeys.Area:
                            //  case ManifestKeys.Rotate:
                            delim = ",";
                            break;
                    }
                }
                else
                    delim = ",";

                //     Debug.Log("subkey = " + r.subKey + " " + cleanLine);
                if ((r.subKey != ManifestKeys.None) || (r.key != TameKeys.None))
                {
                    contentList = Utils.Split(content, delim);
                    for (int i = 0; i < contentList.Count; i++)
                    {
                        cleanLine = Utils.Clean(contentList[i]);
                        r.items.Add(cleanLine);
                    }
                    //             Debug.Log("content: " + contentList.Count + " " + content);
                }
            }
            return r;
        }

        private static TameKeys GetType(string s)
        {
            int k = ManifestKeys.GetKey(s);
            //   Debug.Log("key = " + k + " " + s);
            if (k != 0)
                // 7/25 11:50
                return k switch
                {
                    ManifestKeys.Object => TameKeys.Object,
                    ManifestKeys.Material => TameKeys.Material,
                    ManifestKeys.Light => TameKeys.Light,
                    ManifestKeys.Custom => TameKeys.Custom,
                    ManifestKeys.Import => TameKeys.Import,
                    ManifestKeys.Walk => TameKeys.Walk,
                    ManifestKeys.Camera => TameKeys.Camera,
                    ManifestKeys.Eye => TameKeys.Eye,
                    ManifestKeys.Mode => TameKeys.Mode,
                    ManifestKeys.Alter => TameKeys.Alter,
                    ManifestKeys.Match => TameKeys.Match,
                    _ => TameKeys.None
                };
            else return TameKeys.None;
        }
        private static int GetSubType(string s)
        {
            return ManifestKeys.GetKey(s);
            // 7/25 11:50

        }
        /// <summary>
        /// reads two numbers separated by comma
        /// </summary>
        /// <param name="s"></param>
        /// <returns>an array containg the parsed floats, or null if invalid</returns>
        public static float[] Read2(string s)
        {
            // 7/25 11:52
            List<string> list = Utils.Split(s, ",");
            float f;
            if (list.Count > 1)
            {
                float[] result = new float[2];
                result[0] = Utils.SafeParse(list[0], out f) ? f : float.NegativeInfinity;
                result[1] = Utils.SafeParse(list[1], out f) ? f : float.NegativeInfinity;
                if ((result[0] != float.NegativeInfinity) && (result[1] != float.NegativeInfinity)) return result;
            }
            return null;
        }
        public void Resplit(char original)
        {
            string s = "";
            for (int i = 0; i < items.Count; i++)
                s += (i == 0 ? "" : original + "") + items[i];
            int p = s.IndexOf(' ');
            items.Clear();
            if (p > 0)
                items.Add(s.Substring(0, p));
            items.AddRange(s.Split(','));
        }
    }
    /// <summary>
    /// the class contains the whole of the manifest file ("manifest.txt" stored in Resources folder) and the creation and update of the interactives.
    /// </summary>
    public class TameManifest
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
        public List<TameManifestBase> manifests;
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
        public TameManifest()
        {
            manifests = new List<TameManifestBase>();
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
                        case TameKeys.Object: i = TameManifestBase.Read(header, lines, i, manifests); break;
                        case TameKeys.Material: i = TameManifestBase.Read(header, lines, i, manifests); break;
                        case TameKeys.Light: i = TameManifestBase.Read(header, lines, i, manifests); break;
                        //   case TameKeys.Area: i = TameArea.GetArea(header, lines, i, area); break;
                        case TameKeys.Custom:
                            i = TameCustomManifest.Create(header, lines, i, out TameCustomValue tcv);
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
                        case TameKeys.Match: i = TameMatchManifest.Read(lines, i, matches); break;
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
                    Debug.Log("import ints: " + tgos.Count);
                    break;
                }
            foreach (GameObject rootObj in root) if (rootObj.name.Equals("interactives")) { RootObject = rootObj; break; }
            if (RootObject != null)
                tgos.AddRange(TameObject.CreateInteractive(TameTime.RootTame, RootObject, tes));
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
                walkManager = new Walking.WalkManager(finder.objectList, true);
                foreach (TameGameObject tg in finder.objectList)
                    if (tg.gameObject.name.StartsWith("_"))
                        tg.gameObject.SetActive(false);
            }
        }

        void RedefineCues()
        {
            TameObjectManifest tom;
            TameFinder finder = new TameFinder() { header = new ManifestHeader() };
            TameObject to, sto;
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                    if (te.manifest != null)
                    {
                        to = (TameObject)te;
                        finder.owner = te;
                        finder.header.items.Clear();
                        finder.elementList.Clear();
                        tom = (TameObjectManifest)te.manifest;
                        foreach (string s in tom.cues)
                            finder.header.items.Add(s);
                        finder.PopulateElements(tes, tgos);
                        foreach (TameElement se in finder.elementList)
                            if (te.tameType == TameKeys.Object)
                            {
                                sto = (TameObject)se;
                                to.areas.AddRange(sto.areas);
                            }
                        to.CleanAreas();
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
            Renderer r;
            Material[] ms;
            bool f;
            // find all materials in the interactives
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
            TameMaterial tm;
            Material m, clone;
            // create tame materials and associate manifests with them 
            foreach (TameManifestBase tmb in manifests)
                if (tmb.header.key == TameKeys.Material)
                {
                    foreach (string item in tmb.header.items)
                    {
                        //     Debug.Log("changer " + item + " of " + existing.Count);
                        for (int i = 0; i < existing.Count; i++)
                            if (existing[i] != null)
                                if (item.ToLower().Equals((m = existing[i]).name.ToLower()))
                                {
                                    Debug.Log("identified " + tmb.header.items[0]);
                                    // if detach clone materials as separate objects
                                    if (((TameMaterialManifest)tmb).unique)
                                        foreach (TameGameObject tgo in tgos)
                                        {
                                            if ((clone = TameMaterial.SwitchMaterial(tgo.gameObject, m)) != null)
                                            {
                                                Debug.Log(clone.name + " " + tgo.gameObject.name + " " + tgo.tameParent.name);
                                                tm = new TameMaterial()
                                                {
                                                    name = clone.name,
                                                    original = clone,
                                                    manifest = tmb,
                                                    index = (ushort)tes.Count,
                                                    owner = tgo.gameObject
                                                };
                                                tm.CheckEmission();
                                                tm.basis = tgo.tameParent.tameType == TameKeys.Time ? TrackBasis.Time : TrackBasis.Tame;
                                                tm.parents.Add(new TameEffect(ManifestKeys.Update, tgo.tameParent)
                                                {
                                                    child = tm
                                                });
                                                tes.Add(tm);
                                                tmb.elements.Add(tm);
                                            }
                                        }
                                    else
                                    {
                                        tm = new TameMaterial()
                                        {
                                            name = m.name,
                                            original = m,
                                            manifest = tmb,
                                            index = (ushort)tes.Count
                                        };
                                        //        Debug.Log("changer " + m.name + " " + ((TameMaterialManifest)tmb).properties.Count);
                                        tm.CheckEmission();
                                        tm.parents.Add(new TameEffect(ManifestKeys.Update, firstOwner[i]));
                                        tes.Add(tm);
                                        tmb.elements.Add(tm);
                                    }
                                    break;
                                }
                    }
                }
        }
        /// <summary>
        /// finds and assigns every <see cref="TameElement"/> in <see cref="tes"/> to the manifest loaded by <see cref="Read"/>. This method is called from within <see cref="LoadManifest"/>
        /// </summary>
        void IdentifyElements()
        {
            // 7/25 12:26
            TameFinder finder = new TameFinder();
            foreach (TameManifestBase tmb in manifests)
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
                //    Debug.Log(te.name + " " + te.updateParents.Count + " " + te.basis[0]);
            }
        }
        void SetProgressProperties()
        {
            TameObject to;
            foreach (TameElement te in tes)
            {
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    if (to.isGrippable)
                        to.progress = new TameProgress(to);
                    to.handle.CalculateHandles(te.manifest == null ? 0 : te.manifest.initial);
                }//    ((TameObject)te).handle.CalculateHandles(0);
                te.SetProgressProperties(tes, tgos);
            }
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
            foreach (TameManifestBase tmb in manifests)
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
