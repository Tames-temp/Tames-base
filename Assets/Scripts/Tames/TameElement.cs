using Markers;
using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Tames
{
    /// <summary>
    /// this is the base, somewhat abstract class for all interactive elements (including <see cref="TameObject"/>, <see cref="TameManterial"/> and <see cref="TameLight"/>
    /// </summary>
    public class TameElement
    {
        public bool updatedUnique = false;
        public MarkerProgress markerProgress = null;
        public MarkerEnvironment marketEnvironment = null;
        public MarkerSpeed markerSpeed = null;
        public static bool isPaused = false;
        public static float FrameValue = -1;
        public static float deltaTime;
        public static float lastDelta = 1;
        /// <summary>
        /// the universal project tick. It determines the number of frames (and hence updates) passed since the start of the application. It is used to check which progresses are alread updated. See <see cref="TameProgress.tick"/>
        /// </summary>
        public static int Tick = 0;
        /// <summary>
        /// the total time in seconds that the project has been responsive to navigational input as opposed to the <see cref="TotalTime"> that also inlcudes pauses
        /// </summary>
        public static float ActiveTime = 0;
        /// <summary>
        /// the total time in seconds that after the start of the project, inclusive of pauses, as opposed to <see cref="ActiveTime">
        /// </summary>
        public static float TotalTime = 0;
        /// <summary>
        /// the size of the array used to calculate average speed by <see cref="averageSpeed"/> 
        /// </summary>
        public const int DataHistoryCount = 10;
        /// <summary>
        /// name of the element, usually the same as the name of the associated game object or material
        /// </summary>
        public string name;
        /// <summary>
        /// the index of this element in <see cref="TameManager.tes"/>
        /// </summary>
        public ushort index = 0;
        /// <summary>
        /// type of the element, see <see cref="TameKeys"/>. The default value is <see cref="TameKeys.Object"/>
        /// </summary>
        public TameKeys tameType = TameKeys.Object;
        /// <summary>
        /// the manifest associated with this element, which will be an inheritor class of <see cref="ManifestBase"/>
        /// </summary>
        public ManifestBase manifest = null;
        /// <summary>
        /// the progress for each update mode. Index 0 is for update or slide, and index 1 is for rotate.
        /// </summary>
        public TameProgress progress = null;

        /// <summary>
        /// the parent game object of the moving part of the interactive object 
        /// </summary>
        public GameObject owner = null;
        /// <summary>
        /// the moving object of the interactive object
        /// </summary>
        public GameObject mover = null;
        //  public byte AuthorityType { get { return } }

        /// <summary>
        /// lis of parent effect/objects that control updating of this element. If the parent is another <see cref="TameElement"/>, this can only have one member. Normally, the update parent of an element is another element whose <see cref="GameObject"/> is the parent or first ancestor with a <see cref="TameElement"/> attached to it. But this can be changed in the manifest file (see <see cref="TameManager"/> and <see cref="ManifestHeader.subKey"/>) with subkey "update" inside a block that defines this element. In the update line, the names of the parents are listed, separated by comma and subject to naming conventions of the <see cref="TameFinder.Relations"/>. Position tracking (@) is only acceptable for <see cref="TameObject"/> elements. The same declaring pattern applies to subkeys "slide" and "rotate" which fill <see cref="slideParents"/> and <see cref="rotateParents"/> respectively.
        /// </summary>
        public List<TameEffect> parents = new List<TameEffect>();

        /// <summary>
        /// the parents for each update action in a given frame. For <see cref="TameMaterial"/>, <see cref="TameLight"/> and also for <see cref="TameObject"> with <see cref="ManifestKeys.Update"/> mode, only the first element would be valued. These values will be added to universal array of <see cref="TameEffect"/>s to be sorted to create a queue of updating elements in each frame
        /// </summary>
        // public TameEffect[] parent = new TameEffect[] { null, null, null };
        /// <summary>
        /// stores the index of progress if successful, (<see cref="Unsuccessful"/> if not)
        /// </summary>
        public byte basis = TrackBasis.Time;

        public List<GameObject> scaledObjects = new List<GameObject>();
        public List<Material> scaledMaterials = new List<Material>();
        public List<float> initialTiles = new List<float>();
        /// <summary>
        /// a basis for  <see cref="TameProgress.changingDirection"/> when the progress are updated based on interactors.
        /// </summary>
        public int changingDirection = 1;
        //   public int switchingKey = -1;
        public bool initialVisibility = true;
        /// <summary>
        /// the interaction areas attached to this elements. 
        /// </summary>
        public List<TameArea> areas = new List<TameArea>();
        public List<TameInputControl> control, actSwitch, visSwitch;
        public bool manual = false;
        /// <summary>
        /// whether the <see cref="TameArea.mode"/> of this element's <see cref="areas"/> is <see cref="InteractionMode.Grip"/>
        /// </summary>
        public bool isGrippable = false;
        /// <summary>
        /// whether the <see cref="TameArea.mode"/> of this element's <see cref="areas"/> is <see cref="InteractionMode"/>.Switch1, 2, or 3
        /// </summary>
        public bool isSwitch = false;
        public bool isDistance = false;
        public bool isTracking = false;
        public float directProgress = -1;
        public TameElement()
        {
            control = new List<TameInputControl>();
            actSwitch = new List<TameInputControl>();
            visSwitch = new List<TameInputControl>();
        }
        public void ReadInput(string s)
        {
            string[] list = s.ToLower().Split(' ');
            for (int i = 0; i < list.Length; i++)
            {
                TameInputControl tci = TameInputControl.ByStringDuo(list[i]);
                if (tci != null)
                {
                    //           Debug.Log("MANUL: " + list[i] + " " + (tci == null));
                    control.Add(tci);
                }
            }
            if (control.Count > 0)
            {
                manual = true;
                progress = new TameProgress(this);
            }
        }
        private TameEffect GetEffect(Person headOwner, Person handOwner, TameAreaTrack tat)
        {
            TameEffect r = null;
            //  if (name == "door1") Debug.Log("enfo z:" + basis + " "+parents.Count);
            if (TrackBasis.Time == basis)
                r = TameEffect.Time();
            else
            {
                // update
                int closest = -1;
                float min = float.PositiveInfinity;
                float d;
                byte tbas = TrackBasis.Error;
                Vector3 closestPosition = Vector3.positiveInfinity;
                if (TrackBasis.IsHead(basis) && (headOwner != null))
                {
                    closestPosition = headOwner.headPosition;
                    min = Vector3.Distance(headOwner.headPosition, mover.transform.position);
                    tbas = TrackBasis.Head;
                }
                if (TrackBasis.IsHand(basis) && (handOwner != null))
                    if ((d = Vector3.Distance(handOwner.position[tat.hand], mover.transform.position)) < min)
                    {
                        min = d;
                        closestPosition = handOwner.position[tat.hand];
                        tbas = TrackBasis.Hand;
                    }
                if (TrackBasis.IsObject(basis))
                    if (parents.Count > 0)
                        for (int i = 0; i < parents.Count; i++)
                            if (parents[i].type == TrackBasis.Object)
                                if ((d = Vector3.Distance(parents[i].gameObject.transform.position, mover.transform.position)) < min)
                                {
                                    min = d;
                                    closestPosition = parents[i].gameObject.transform.position;
                                    closest = i;
                                    tbas = TrackBasis.Object;
                                }
                if (closestPosition.x != float.PositiveInfinity)
                {
                    r = TameEffect.Position(closestPosition);
                    r.type = tbas;
                }
                if (closest >= 0) r.gameObject = parents[closest].gameObject;
            }
            if (TrackBasis.Tame == basis)
                if (parents.Count > 0) r = parents[0];
            return r;
        }
        private TameEffect GetEffect()
        {
            TameEffect r = null;
            //  if (name == "door1") Debug.Log("enfo z:" + basis + " "+parents.Count);
            if (TrackBasis.Time == basis)
                r = TameEffect.Time();
            else if (TrackBasis.Tame == basis)
                if (parents.Count > 0) r = parents[0];
            if (r != null)
                r.child = this;
            return r;
        }
        /// <summary>
        /// the base method for finding the action parents of the elements in each frame. The first element of the array indicate the update parent (that if is assigned, the other two elements would be null). The next elements contain the slide and rotate parents, respectively. 
        /// </summary>
        /// <returns></returns>
        public virtual TameEffect GetParent()
        {
            TameEffect r = null;
            int closest;
            float d;
            float dis;
            float min = float.PositiveInfinity;
            bool[] set = new bool[] { false, false, false };
            TameArea ti;
            Person pe;
            if (manual) return null;
            //   Debug.Log("before error 1");
            TameObject to;
            if (isGrippable)
            {
                r = TameArea.Grip(areas);
                if (r != null)
                {
                    to = (TameObject)this;
                    pe = r.personIndex == Person.LocalDefault ? Person.localPerson : Person.people[r.personIndex];
                    ti = areas[r.areaIndex];
                    ti.gripDisplacement = owner.transform.InverseTransformPoint(pe.hand[r.handIndex].gripCenter) - owner.transform.InverseTransformPoint(ti.relative.transform.position);
                    if (to.handle.RotationType == MovingTypes.Rotator)
                        ti.displacement = Utils.Angle(owner.transform.InverseTransformPoint(ti.relative.transform.position), Vector3.zero, to.handle.start - to.handle.pivot, to.handle.axis, true);
                    r.child = this;
                }
                return r;
            }
            if (isSwitch)
            {
                r = TameEffect.Time();
                r.child = this;
                changingDirection = areas[0].switchDirection;
                if (areas[0].geometry == InteractionGeometry.Remote)
                {
                    if (TameInputControl.keyMap.pressed[areas[0].key])
                    {
                        areas[0].Switch(true);
                        changingDirection = areas[0].switchDirection;
                        if (name == "_speed") Debug.Log("switched " + changingDirection);
                    }
                }
                else
                {
                    changingDirection = areas[0].switchDirection;
                    //       if (Person.localPerson.switchCount != 0) Debug.Log("SWC: o count");
                    int sd = TameArea.CheckSwitch(areas);
                    //    Debug.Log("switch "+sd);
                    r = TameEffect.Time();
                    if (sd != TameArea.NotSwitched)
                        if (changingDirection != sd)
                        {
                            Debug.Log("direction " + changingDirection + " > " + sd);
                            changingDirection = sd;
                        }
                    r.child = this;
                }
                //          Debug.Log("before error 2.3");
                return r;
            }
            if (isDistance)
            {
                r = TameEffect.Time();
                r.child = this;
                if (areas[0].range != null)
                {
                    d = areas[0].TrackDistance();
                    if (areas[0].directProgress)
                        directProgress = d;
                    else
                        changingDirection = d < 0 ? -1 : (d > 0 ? 1 : 0);
                }
                return r;
            }
            if (TrackBasis.IsTracking(basis) || (areas.Count > 0))
            {

                //      Debug.Log("before error 3");
                Person headOwner = null, handOwner = null;
                //    Debug.Log("name = " + areas.Count);
                TameAreaTrack tat = areas.Count > 0 ? TameArea.TrackWithAreas(areas, mover.transform.position) : TameArea.Track(mover.transform.position);
                byte tp = 0;
                if (tat.person >= 0)
                {
                    handOwner = tat.person == Person.LocalDefault ? Person.localPerson : Person.people[tat.person];
                    //     tp = TrackBasis.Hand;
                }
                if (tat.head >= 0)
                {
                    headOwner = tat.head == Person.LocalDefault ? Person.localPerson : Person.people[tat.head];
                    //    tp = TrackBasis.Head;
                }

                changingDirection = tat.direction;
                if (changingDirection != 0)
                {
                    r = GetEffect(headOwner, handOwner, tat);
                    if (r != null)
                    {
                        r.direction = tat.direction;
                        r.child = this;
                    }
                }
            }
            else
            {
                r = GetEffect();
                return r;
                //   if (name == "lift-last") Debug.Log("dir " + changingDirection + (r == null));
            }
            return r;
        }
        public static void PassTime()
        {
            lastDelta = deltaTime;
            deltaTime = FrameValue < 0 ? Time.deltaTime : FrameValue;
            if (!isPaused)
            {
                Tick++;
                ActiveTime += deltaTime;
            }
            TotalTime += deltaTime;
        }
        public virtual void AssignParent(TameEffect[] all, int index)
        {

        }
        /// <summary>
        /// checks if a game object is the parent or grandparent of the <see cref="owner"/> of this element.
        /// </summary>
        /// <param name="go">the game object to be checked</param>
        /// <param name="grand">checks for grandparent if true, or immediate parent if false</param>
        /// <returns></returns>
        public bool IsChildOf(GameObject go, bool grand)
        {
            GameObject p = owner;
            if (!grand)
                return go == p;
            else
                while (p != null)
                {
                    if (go == p)
                        return true;
                    else
                        p = p.transform.parent != null ? p.transform.parent.gameObject : null;
                }
            return false;
        }
        /// <summary>
        /// checks if a game object with a specific name is the parent or grandparent of the <see cref="owner"/> of this element.
        /// </summary>
        /// <param name="name">the name of the game object to be checked</param>
        /// <param name="starts">if the name is the game objects full name (false) or the start of it (true)</param>
        /// <param name="grand">checks for grandparents if true, or immediate parent if false</param>
        /// <returns></returns>
        public bool IsChildOf(string name, bool starts, bool grand)
        {
            GameObject p = owner;
            if (!grand)
                return starts ? p.name.StartsWith(name) : p.name.Equals(name);
            else
                while (p != null)
                {
                    if (starts ? p.name.StartsWith(name) : p.name.Equals(name))
                        return true;
                    else
                        p = p.transform.parent != null ? p.transform.parent.gameObject : null;
                }
            return false;
        }
        /// <summary>
        /// checks if a game object with a specific name is the sibling of the <see cref="owner"/> of this element.
        /// </summary>
        /// <param name="name">the name of the game object to be checked</param>
        /// <param name="starts">if the name is the game objects full name (false) or the start of it (true)</param>
        /// <returns></returns>
        public bool IsSiblingOf(string name, bool starts)
        {
            int cc = owner.transform.childCount;
            for (int i = 0; i < cc; i++)
                if (mover != owner.transform.GetChild(i).gameObject)
                    if (starts ? owner.transform.GetChild(i).name.StartsWith(name) : owner.transform.GetChild(i).name.Equals(name))
                        return true;
            return false;
        }
        public void SetProgress(float p)
        {
            if (progress != null) progress.SetProgress(p);
        }
        /// <summary>
        /// sets the progress at a specific index based on the parent progress 
        /// </summary>
        /// <param name="p">the parent progress</param>
        /// <param name="index">index of the progress in this element, 0 or 1 for the exact index, and 2 for both progresses</param>
        public void SetByParent(TameProgress p)
        {
            if (progress != null)
            {
                progress.interactDirection = changingDirection;
                progress.SetByParent(new float[] { p.lastProgress, p.progress }, new float[] { p.lastTotal, p.totalProgress }, p.passToChildren, deltaTime);
            }
        }
        public virtual void Scale()
        {
            if (tameType == TameKeys.Object)
            {
                TameObject to = (TameObject)this;
                if (to.scales)
                {
                    Vector3 ls;
                    float s = to.scaleFrom + (to.scaleTo - to.scaleFrom) * progress.slerpProgress;
                    foreach (GameObject go in scaledObjects)
                    {
                        //      Debug.Log("scale: " + go.name + " : " + manifest.scaleAxis+ " "+s);
                        ls = go.transform.localScale;
                        if (to.scaleAxis == 0) go.transform.localScale = new Vector3(s, ls.y, ls.z);
                        else if (to.scaleAxis == 1) go.transform.localScale = new Vector3(ls.x, s, ls.z);
                        else go.transform.localScale = new Vector3(ls.x, ls.y, s);
                    }
                    Vector2 tex;
                    for (int i = 0; i < initialTiles.Count; i++)
                        try
                        {
                            tex = scaledMaterials[i].GetTextureScale(Utils.ProperyKeywords[TameMaterial.MainTex]);
                            if (to.scaleUV == 0) tex.x = s * initialTiles[i]; else tex.y = s * initialTiles[i];
                            scaledMaterials[i].SetTextureScale(Utils.ProperyKeywords[TameMaterial.MainTex], tex);
                        }
                        catch { }
                }
            }
        }
        public void CheckStatus()
        {
            if (owner != null)
            {
                if (Tick <= 0)
                    owner.SetActive(initialVisibility);
                else
                {
                    if (visSwitch.Count > 0)
                        foreach (TameInputControl tci in visSwitch)
                            if (tci.Pressed())
                            {
                                owner.SetActive(!owner.activeSelf);
                                break;
                            }
                    if (actSwitch.Count > 0)
                        foreach (TameInputControl tci in actSwitch)
                            if (tci.Pressed())
                            {
                                progress.active = !progress.active;
                                break;
                            }
                }
            }
        }

        /// <summary>
        /// updates the progress of the element (this is used for remote update).
        /// </summary>
        /// <param name="p"></param>
        public virtual void Update(float p) { }
        public virtual void Rotate(float p, int i) { }
        /// <summary>
        /// updates the progress(es) in this element based on a parent progress. 
        /// </summary>
        /// <param name="p">the parent progress</param>
        public virtual void Update(TameProgress p) { }
        /// <summary>
        /// updates the progress(es) in this element based on a position
        /// </summary>
        /// <param name="p">the parent position</param>
        public virtual void Update(Vector3 p) { }
        /// <summary>
        /// sets the progress at a specific index based on time
        /// </summary>
        /// <param name="index">index of the progress in this element, 0 or 1 for the exact index, and 2 for both progresses</param>
        public void SetByTime()
        {
            if (progress != null)
            {
                progress.interactDirection = changingDirection;

                progress.SetByTime(TameElement.deltaTime);
            }
        }
        public void SetManually()
        {
            int dir = 0;
            foreach (TameInputControl tci in control)
            {
                dir = tci.Hold();
                if (dir != 0)
                    break;
            }
            if (dir != 0)
            {
                progress.interactDirection = dir;
                progress.SetByTime(deltaTime);
            }
        }
        /// <summary>
        /// updates the current element based on passage of time. 
        /// </summary>
        public virtual void UpdateManually()
        {
            SetManually();
        }   /// <summary>
            /// updates the current element based on passage of time. 
            /// </summary>
        public virtual void Update()
        {
        }
        /// <summary>
        /// adds interactors to the this elements. Currently, it only works on <see cref="TameObject"/>s, so please see <see cref="TameObject.AddArea(TameArea, GameObject)"/>
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="g"></param>
       // public virtual void AddArea(TameArea ti, GameObject g = null) { }
        /// <summary>
        /// clean disabled interactors. Currently, it only works on <see cref="TameObject"/>s, so please see <see cref="TameObject.CleanAreas"/>
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="g"></param>

        public void CleanAreas()
        {
            isGrippable = isSwitch = isDistance = false;
            int retain = 1;
            foreach (TameArea ti in areas)
                if (ti.mode == InteractionMode.Grip)
                {
                    isGrippable = true;
                    break;
                }
            if (!isGrippable)
            {
                foreach (TameArea ti in areas)
                    if (ti.geometry == InteractionGeometry.Remote)
                    {
                        retain = 1;
                        isSwitch = true;
                        break;
                    }
                if (!isSwitch)
                    foreach (TameArea ti in areas)
                        if (TameArea.IsSwitch(ti.mode))
                        {
                            retain = 2;
                            isSwitch = true;
                            break;
                        }
                if (!isSwitch)
                    foreach (TameArea ti in areas)
                        if (ti.geometry == InteractionGeometry.Distance)
                        {
                            retain = 1;
                            isDistance = true;
                            break;
                        }
            }
            if (isGrippable)
            {
                for (int i = areas.Count - 1; i >= 0; i--)
                    if (areas[i].mode != InteractionMode.Grip)
                        areas.RemoveAt(i);
                parents.Clear();
                basis = TrackBasis.Grip;
            }
            else if (isSwitch)
            {
                if (retain == 1)
                {
                    for (int i = areas.Count - 1; i >= 0; i--)
                        if (areas[i].geometry != InteractionGeometry.Remote)
                            areas.RemoveAt(i);
                }
                else
                    for (int i = areas.Count - 1; i >= 0; i--)
                        if ((!TameArea.IsSwitch(areas[i].mode)) || (areas[i].geometry == InteractionGeometry.Distance))
                            areas.RemoveAt(i);
                parents.Clear();
                basis = TrackBasis.Grip;
            }
            else if (isDistance)
            {
                for (int i = areas.Count - 1; i >= 0; i--)
                    if (areas[i].geometry != InteractionGeometry.Distance)
                        areas.RemoveAt(i);
                parents.Clear();
                basis = TrackBasis.Grip;
            }
        }

        /// <summary>
        /// add a time update control. By doing so, it removes all parents with shared effects (see <see cref="TameEffect.effect"/> for the notion of shared effect). This method is called by <see cref="PopulateUpdates"/>
        /// </summary>
        /// <param name="subtype">the effect</param>
        void AddTime(int subtype)
        {
            parents.Clear();
            basis = TrackBasis.Time;
            //basis[1] = basis[2] = TrackBasis.Error;

        }
        void MonoUpdate(int subtype, TameGameObject tgo)
        {
            TameEffect tp;
            List<TameEffect> p = null;
            p = parents;
            parents.Clear();
            basis = TrackBasis.Object;
            p.Add(new TameEffect(subtype, tgo));
        }
        void MonoUpdate(int subtype, TameElement te)
        {
            TameEffect tp;
            List<TameEffect> p = null;
            p = parents;
            parents.Clear();
            basis = TrackBasis.Tame;
            p.Add(new TameEffect(subtype, te));
        }
        /// <summary>
        /// Add update parents 
        /// </summary> add update parents with position tracking to this elements. It adds to the list of same effects but removes parents with other shared effects (see <see cref="TameEffect.effect"/> for the notion of shared effect). This method is called by <see cref="PopulateUpdates"/>
        /// <param name="subtype">the effect type</param>
        /// <param name="pos">the parent game objects whose position will be tracked</param>
        void AddUpdate(int subtype, List<TameGameObject> pos)
        {
            TameEffect tp;
            List<TameEffect> p = null;
            p = parents;
            parents.Clear();
            basis += TrackBasis.Object;
            //     basis[1] = basis[2] = TrackBasis.Error;

            for (int i = 0; i < pos.Count; i++) p.Add(new TameEffect(subtype, pos[i]));
        }
        /// <summary>
        /// add update parents with progress tracking to this elements. It adds to the list of same effects but removes parents with other shared effects (see <see cref="TameEffect.effect"/> for the notion of shared effect). This method is called by <see cref="PopulateUpdates"/>
        /// </summary>
        /// <param name="subtype">the effect type</param>
        /// <param name="prog">the parent elements whose progress are tracked</param>
        /// <param name="rot">if the tracked position is Rotate (true) or Update or Slide (false)</param>
        void AddUpdate(int subtype, List<TameElement> prog)
        {
            parents.Clear();
            basis = TrackBasis.Tame;
            // basis[1] = basis[2] = TrackBasis.Error;
            for (int i = 0; i < prog.Count; i++) parents.Add(new TameEffect(subtype, prog[i]));
        }
        public bool PopulateUpdateByMarker(List<TameElement> tes, List<TameGameObject> tgos, MarkerProgress mp)
        {
            TameMaterial tm;
            //   Debug.Log("pop " + name);
            if (mp != null)
            {
                if (mp.byElement != null)
                {
                    TameGameObject tmo, tgo = TameGameObject.Find(mp.byElement, tgos);
                    TameElement te = tgo == null ? null : tgo.tameParent;
                    if (tameType == TameKeys.Object)
                    {
                        switch (basis)
                        {
                            case TrackBasis.Object: MonoUpdate(TrackBasis.Object, tgo); break;
                            case TrackBasis.Mover:
                                if (te != this)
                                {
                                    tmo = TameGameObject.Find(te.mover, tgos);
                                    MonoUpdate(TrackBasis.Object, te.owner == tgo.gameObject ? tgo : tgo);
                                }
                                break;
                            case TrackBasis.Head: break;
                            default: if ((te != null) && (te != this)) MonoUpdate(TrackBasis.Tame, te); break;
                        }
                    }
                    else if (te != null) MonoUpdate(TrackBasis.Tame, te);
                    return true;
                }
                else if (mp.byMaterial != null)
                {
                    tm = TameMaterial.Find(mp.byMaterial, tes);
                    if (tm != null)
                    {
                        MonoUpdate(TrackBasis.Tame, tm);
                        return true;
                    }
                }
                else if (mp.update != "")
                {
                    TameFinder finder = new TameFinder();
                    finder.header = null;
                    ManifestHeader header = ManifestHeader.Read("update " + markerProgress.update);
                    finder.header = header;
                    finder.elementList.Clear();
                    finder.objectList.Clear();
                    finder.owner = this;
                    finder.trackMode = basis;
                    PopulateByFinder(finder, tes, tgos);
                }
            }
            return false;
        }
        private void PopulateByFinder(TameFinder finder, List<TameElement> tes, List<TameGameObject> tgos)
        {
            if (finder.header != null)
            {
                finder.Populate(tes, tgos);

                // if(name=="pipes")
                //        Debug.Log("inlx = " + name + " " + manifest.updateType + " " + manifest.updates.items[0] + " " + finder.elementList.Count);
                switch (finder.trackMode)
                {
                    case TrackBasis.Object:
                        if (tameType == TameKeys.Object)
                        {
                            basis = TrackBasis.Error;
                            if (finder.includes[TameFinder.Head]) basis = TrackBasis.Head;
                            if (finder.includes[TameFinder.Hand]) basis += TrackBasis.Hand;
                            AddUpdate(manifest.updates.subKey, finder.objectList);
                        }
                        break;
                    case TrackBasis.Tame:
                        basis = TrackBasis.Time;
                        if ((tameType == TameKeys.Object) || (manifest.updates.subKey == ManifestKeys.Update))
                        {
                            AddUpdate(manifest.updates.subKey, finder.elementList);
                            if (finder.elementList.Count > 0)
                                basis = TrackBasis.Tame;
                        }
                        break;
                    case TrackBasis.Mover:
                        if (tameType == TameKeys.Object)
                        {
                            basis = TrackBasis.Error;
                            AddUpdate(manifest.updates.subKey, finder.objectList);
                            if (finder.objectList.Count > 0) basis = TrackBasis.Mover;
                        }
                        break;
                }
            }
            else
            {
                if (tameType == TameKeys.Object)
                {
                    TameObject to = (TameObject)this;
                    if (to.handle.trackBasis == TrackBasis.Mover)
                    {
                        if (to.parentObject != null)
                        {
                            AddUpdate(ManifestKeys.Object, new List<TameGameObject>() { TameFinder.FindTGO(to.parentObject.mover, tgos) });
                            basis = TrackBasis.Object;
                        }
                    }
                    else if (to.parentObject != null)
                        AddUpdate(ManifestKeys.Update, new List<TameElement>() { to.parentObject });
                }
            }
        }
        /// <summary>
        /// identifies all possible parents for this elements based on its manifest.
        /// </summary>
        /// <param name="tes">list of all interactive elements in the project (see <see cref="TameManager.SurveyInteractives"/>")</param>
        /// <param name="tgos">list of all game objects related to the interactive elements (see <see cref="TameManager.SurveyInteractives"/>)</param>
        public void PopulateUpdates(List<TameElement> tes, List<TameGameObject> tgos)
        {
            //     if (name == "barrier sign") Debug.Log("UP: bef " + parents.Count);
            TameFinder finder = new TameFinder();
            progress = new TameProgress(this);
            if (updatedUnique) return;
            bool markerBased = !PopulateUpdateByMarker(tes, tgos, markerProgress);
            if (markerBased && (manifest != null))
            {
                finder.header = null;
                if (manifest.updates != null)
                {
                    finder.header = manifest.updates;
                    finder.elementList.Clear();
                    finder.objectList.Clear();
                    finder.owner = this;
                    finder.trackMode = manifest.updateType;
                }
                PopulateByFinder(finder, tes, tgos);
            }
            else
            {
                if (tameType == TameKeys.Object)
                {
                    TameObject to = (TameObject)this;
                    if (to.handle.trackBasis == TrackBasis.Mover)
                    {
                        if (to.parentObject != null)
                        {
                            AddUpdate(ManifestKeys.Object, new List<TameGameObject>() { TameFinder.FindTGO(to.parentObject.mover, tgos) });
                            basis = TrackBasis.Object;
                        }
                    }
                    else if (to.parentObject != null)
                        AddUpdate(ManifestKeys.Update, new List<TameElement>() { to.parentObject });
                }
            }
            //  if (name == "barrier sign") Debug.Log("UP: aft " + parents[0].parent.name);
            if (basis != TrackBasis.Tame)
                for (int i = parents.Count - 1; i >= 0; i--)
                    if (parents[i].type == TrackBasis.Tame)
                        parents.RemoveAt(i);
            // if (name == "barrier sign") Debug.Log("UP: aft1 " + parents.Count);
        }
        /// <summary>
        /// Gets the parents of all interactive elements in the project, this should be called during each frame if there is a chance that parents are changed (for example there are multiple objects or people being tracked for the same element, so their position affects which one would be the parent. The method also sorts the parents so they would be updated in order
        /// </summary>
        /// <param name="allEffects">an array including all the parents for all actions for all interactive elements. As mentioned in <see cref="TameElement.GetParent"/> for each element, there are three types of potential parents. Therefore, the length of this array is three times the count of elements</param>
        /// <param name="tes">the list of all interactive elements in the project</param>
        public static int GetAllParents(TameEffect[] allEffects, List<TameElement> tes)
        {
            for (int i = 0; i < tes.Count; i++)

                if (tes[i].manual)
                {
                    //       Debug.Log("MANUL " + tes[i].name);
                    tes[i].UpdateManually();
                }
                else
                {
                    tes[i].CheckStatus();
                    tes[i].AssignParent(allEffects, i);
                    //    if (tes[i].name == "room-fan") Debug.Log("fan: " + tes[i].progress.progress);
                    //   if (tes[i].name == "cooler") Debug.Log("cool: " + tes[i].progress.progress);
                    //      if (tes[i].name.ToLower() == "inlight")
                    //         Debug.Log("light " + tes[i].progress.progress + " "+tes[i].basis);
                    //         if (tes[i].name.ToLower() == "longbase")
                    //   Debug.Log("base " + tes[i].progress.progress+" "+ tes[i].progress.lastProgress);
                }
            return Order(allEffects);
        }
        /// <summary>
        /// sorts the parent effects in all effects array, so we can <see cref="Apply"/> them from index 0 and count of the returned value (everything after would be null or invalid).  
        /// </summary>
        /// <param name="allEffects">static array <see cref="TameEffect.AllEffects"/></param>
        /// <returns>the number of valid parents</returns>
        private static int Order(TameEffect[] allEffects)
        {
            TameEffect t;
            int count = 0;
            for (int i = 0; i < allEffects.Length; i++)
                if (allEffects[i] != null)
                {
                    if (TrackBasis.IsHand(allEffects[i].type) || TrackBasis.IsHead(allEffects[i].type) || (allEffects[i].type == TrackBasis.Grip))
                    {
                        t = allEffects[count];
                        allEffects[count] = allEffects[i];
                        allEffects[i] = t;
                        count++;
                    }
                }
            for (int i = count; i < allEffects.Length; i++)
                if (allEffects[i] != null)
                    if (allEffects[i].type == TrackBasis.Time)
                    {
                        t = allEffects[count];
                        allEffects[count] = allEffects[i];
                        allEffects[i] = t;
                        count++;
                    }
            TameElement ti, tj;
            int tame = count;
            for (int i = tame; i < allEffects.Length; i++)
                if (allEffects[i] != null)
                {
                    t = allEffects[count];
                    allEffects[count] = allEffects[i];
                    allEffects[i] = t;
                    count++;
                }

            for (int i = tame; i < count - 1; i++)
                for (int j = i + 1; j < count; j++)
                {
                    //       Debug.Log("nullref: " + allEffects[i].child.name+ );
                    if (allEffects[i].type == TrackBasis.Tame)
                        ti = allEffects[i].parent;
                    else
                        ti = allEffects[i].gameObject.tameParent;
                    //     ti = allEffects[i].type == TrackBasis.Tame ? allEffects[i].parent : allEffects[i].gameObject.tameParent;
                    if (ti == allEffects[i].child)
                    {
                        t = allEffects[i];
                        allEffects[i] = allEffects[j];
                        allEffects[j] = t;
                    }
                }
            return count;
        }
        private void RotateAreaFromBlender(Transform t)
        {
            t.RotateAround(t.parent.position, t.parent.right, 180);
            Debug.Log("rotate " + t.gameObject.name);
        }
        public void GetAreas(int software = -1)
        {
            int cc = owner.transform.childCount;
            List<GameObject> io = new List<GameObject>();
            TameArea ir;
            io.AddRange(MarkerArea.FindAreas(owner));
            if (name == "_speed") Debug.Log("ac: " + owner.name + " " + io.Count);
            for (int i = 0; i < cc; i++)
                if (TameArea.HasAreaKeyword(owner.transform.GetChild(i).name))
                {
                    io.Add(owner.transform.GetChild(i).gameObject);
                    if (software == TameManager.Blender) RotateAreaFromBlender(owner.transform.GetChild(i));
                }
            //      if (io.Count > 0) Debug.Log("getting area for " + owner.name + " " + io.Count);
            foreach (GameObject go in io)
                if ((ir = TameArea.ImportArea(go, this)) != null)
                {
                    //       Debug.Log("Area accepted " + owner.name + " " + ir.geometry);
                    ir.element = this;
                    areas.Add(ir);
                }
        }
        public void SetDurations(MarkerProgress mp)
        {
            if (mp.duration != -1) progress.manager.Duration = mp.duration;
            if (mp.trigger != "")
            {
                TameTrigger tt = ManifestBase.ReadTrigger(mp.trigger);
                if (tt != null) progress.trigger = tt;
            }
            progress.cycle = mp.continuity;
            progress.slerp = Slerp.FromString(markerProgress.slerp);
            Update(mp.setAt);
            /*      if (tameType == TameKeys.Object)
                              if (((TameObject)this).handle.cycleSet)
                              {
                                  progress.cycle = ((TameObject)this).handle.cycleType;
                                  if (((TameObject)this).handle.duration > 0)
                                      progress.manager.Duration = ((TameObject)this).handle.duration;
                              }
                      */
        }
        public void SetShowKeys(MarkerProgress mp)
        {
            initialVisibility = mp.gameObject.activeSelf;
            string[] ks = mp.showBy.Split(' ');
            TameInputControl tci;
            string sk = "";
            List<TameInputControl> tcis = new List<TameInputControl>();
            for (int j = 0; j < ks.Length; j++)
            {
                tci = TameInputControl.ByStringMono(ks[j]);
                if (tci != null)
                {
                    tcis.Add(tci);
                    sk += "-" + ks[j];
                }
            }
            if (tcis.Count > 0) visSwitch = tcis;
        }
        public void SetActiveKeys(MarkerProgress mp)
        {
            string[] ks = mp.activateBy.Split(' ');
            TameInputControl tci;
            string sk = "";
            List<TameInputControl> tcis = new List<TameInputControl>();
            for (int j = 0; j < ks.Length; j++)
            {
                tci = TameInputControl.ByStringMono(ks[j]);
                if (tci != null)
                {
                    tcis.Add(tci);
                    sk += "-" + ks[j];
                }
                if (tcis.Count > 0)
                {
                    progress.active = mp.active;
                    actSwitch = tcis;
                }
            }
        }
        /// <summary>
        /// sets the speed, duration, cycle and trigger properties of <see cref="progress"/>es in this element based on the <see cref="manifest"/>
        /// </summary>
        public virtual void SetProgressProperties(List<TameElement> tes, List<TameGameObject> tgos)
        {
            TameFinder finder = new TameFinder();
            if (progress != null)
            {
                if (manifest != null)
                {
                    progress.manager = manifest.manager;
                    if (manifest.mgrParent.Length > 0)
                    {
                        finder.elementList.Clear();
                        finder.header.items.Clear();
                        finder.header.items.Add(manifest.mgrParent);
                        finder.PopulateElements(tes, tgos);
                        if (finder.elementList.Count > 0)
                            progress.manager.parent = finder.elementList[0];
                    }
                    if (progress.trigger == null) progress.trigger = manifest.trigger;
                    progress.cycle = manifest.cycle;
                }
                if (markerProgress != null)
                {
                    SetDurations(markerProgress);
                    SetShowKeys(markerProgress);
                    SetActiveKeys(markerProgress);
                    //     if (name == "barrier sign") Debug.Log("UP: trig " + progress.trigger.value[0]);
                    //    if (visSwitch.Count > 0) Debug.Log("MKM: " + name + "> " + visSwitch[0].keyValue[0] + " " + visSwitch[0].hold + " " + visSwitch[0].direction + " " + visSwitch[0].control);
                    //    Debug.Log("marker progress: not null " + markerProgress.cycleType);

                }
                if (markerSpeed != null)
                {
                    if (markerSpeed.factor > 0)
                    {
                        if (markerSpeed.byElement != null)
                            progress.manager.parent = TameGameObject.Find(markerSpeed.byElement, tgos).tameParent;
                        else if (markerSpeed.byMaterial != null)
                        {
                            TameMaterial tm = TameMaterial.Find(markerSpeed.byMaterial, tes);
                            if (tm != null)
                                progress.manager.parent = tm;
                        }
                        else if (markerSpeed.byName != "")
                        {
                            finder.elementList.Clear();
                            finder.header.items.Clear();
                            finder.header.items.Add(markerSpeed.byName);
                            finder.PopulateElements(tes, tgos);
                            if (finder.elementList.Count > 0)
                                progress.manager.parent = finder.elementList[0];
                        }
                        if (progress.manager.parent == null)
                        {
                            progress.manager.factor = 1;
                            progress.manager.offset = -1;
                        }
                        else
                        {
                            progress.manager.factor = markerSpeed.factor;
                            progress.manager.offset = markerSpeed.offset;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// see <see cref="TameObject.Grip"/>
        /// </summary>
        public virtual void Grip(TameEffect tp) { }
        /// <summary>
        /// applies a parent effect on its child. This should be run for all parent effects after <see cref="GetAllParents"/> in order
        /// </summary>
        /// <param name="tp">the applied parent effect</param>
        public static void Apply(TameEffect tp)
        {
            TameProgress p = null;

            if (tp.type == TrackBasis.Tame)
            {
                if (tp.parent.progress != null)
                    p = tp.parent.progress;
                if (p == null) return;
            }

            switch (tp.type)
            {
                case TrackBasis.Tame: tp.child.Update(p); break;
                case TrackBasis.Object: tp.child.Update(tp.gameObject.transform.position); break;
                case TrackBasis.Hand:
                case TrackBasis.Head: tp.child.Update(tp.position); break;
                case TrackBasis.Time: tp.child.Update(); break;
                case TrackBasis.Grip: tp.child.Grip(tp); break;
            }

            tp.child.Scale();
        }
        public List<TameLink> clones = new(), links = new();
        public void AddClones(MarkerLink ml, bool ofChildren, List<TameGameObject> tgos)
        {
            if (ofChildren)
                clones.Add(new TameLink(ml));
            else
            {
                if (ml.childrenNames != "")
                {
                    TameFinder finder = new TameFinder() { owner = this };
                    finder.header = ManifestHeader.Read("update " + ml.childrenNames);
                    finder.PopulateObjects(tgos);
                    foreach (TameGameObject go in finder.objectList)
                        clones.Add(new TameLink(go.gameObject, ml));
                }
                if (ml.childrenOf != null)
                    for (int i = 0; i < ml.gameObject.gameObject.transform.childCount; i++)
                        clones.Add(new TameLink(ml.gameObject.gameObject.transform.GetChild(i).gameObject, ml));
            }
        }
        public void AddLinks(MarkerLink ml, bool ofChildren, List<TameGameObject> tgos)
        {
            if (ofChildren)
                links.Add(new TameLink(ml));
            else
            {
                if (ml.childrenNames != "")
                {
                    TameFinder finder = new TameFinder() { owner = this };
                    finder.header = ManifestHeader.Read("update " + ml.childrenNames);
                    finder.PopulateObjects(tgos);
                    foreach (TameGameObject go in finder.objectList)
                        links.Add(new TameLink(go.gameObject, ml));
                }
                if (ml.childrenOf != null)
                    for (int i = 0; i < ml.gameObject.gameObject.transform.childCount; i++)
                        links.Add(new TameLink(ml.gameObject.gameObject.transform.GetChild(i).gameObject, ml));
            }
        }
        private float GetLinkValue(MarkerLink.LinkTypes lt, float[] range)
        {
            return lt switch
            {
                MarkerLink.LinkTypes.Parent => range[0],
                MarkerLink.LinkTypes.Custom => range[1],
                _ => UnityEngine.Random.value * (range[3] - range[2]) + range[2],
            };
        }
        public List<TameElement> PopulateClones()
        {
            List<TameElement> r = new();
            float p;
            if (tameType == TameKeys.Object)
            {
                foreach (TameLink tl in clones)
                {
                    TameObject to = CloneAsObject(tl.gameObject.transform, tl.type == MarkerLink.CloneTypes.CloneEverything);
                    to.owner.transform.parent = tl.gameObject.transform.parent;
                    to.owner.transform.localPosition = tl.gameObject.transform.localPosition;
                    to.owner.transform.localRotation = tl.gameObject.transform.localRotation;
                    to.handle.Move(p = GetLinkValue(tl.offsetBase, new float[] { progress.progress, tl.offset, 0f, 1f }), 0);
                    to.progress.SetProgress(p);
                    to.progress.manager.Duration = GetLinkValue(tl.speedBase, new float[] { progress.manager.Duration, tl.factor, progress.manager.Duration / tl.factor, progress.manager.Duration * tl.factor });
                    r.Add(to);
                }
            }
            return r;
        }
        public void PopulateLinks()
        {
            GameObject go, po, bo;
            TamePath path;
            Transform tlt;
            TameLink tl;
            if (tameType == TameKeys.Object)
            {
                TameObject to = (TameObject)this;
                path = to.handle.path;
                path.linked = new Transform[links.Count];
                path.linkOffset = new float[links.Count];
                for (int i = 0; i < links.Count; i++)
                {
                    tl = links[i];
                    tlt = tl.gameObject.transform;
                    Vector3 p0 = path.Position(0);
                    Vector3 pm = tlt.position;
                    Quaternion qm = tlt.rotation;

                    Quaternion q0 = path.Rotation(0);
                    Quaternion qo = qm * Quaternion.Inverse(q0);
                    go = new GameObject(tl.gameObject.name + " - owner");
                    go.transform.parent = tlt.parent;
                    go.transform.rotation = qo;
                    bo = new GameObject(tl.gameObject.name + " - base");
                    bo.transform.parent = go.transform;
                    bo.transform.localRotation = q0;
                    bo.transform.position = tlt.position;
                    bo.transform.localPosition -= p0;
                    go.transform.position = bo.transform.position;
                    bo.transform.position = tlt.position;
                    tlt.parent = bo.transform;
                    tlt.position = pm;
                    tlt.rotation = qm;
                    path.linked[i] = bo.transform;
                    path.linkOffset[i] = GetLinkValue(tl.offsetBase, new float[] { progress.progress, tl.offset, 0f, 1f });
                }
            }
        }
        private TameObject CloneAsObject(Transform t, bool everything)
        {
            TameObject th = (TameObject)this;
            TameObject to = new TameObject();
            GameObject goc, go;
            to.owner = t.gameObject;
            if (everything)
                for (int i = 0; i < th.owner.transform.childCount; i++)
                    if (!th.handle.Interactive(go = th.owner.transform.GetChild(i).gameObject))
                    {
                        goc = GameObject.Instantiate(go);
                        goc.transform.parent = t;
                        goc.transform.localPosition = go.transform.localPosition;
                        goc.transform.localRotation = go.transform.localRotation;
                        goc.transform.localScale = go.transform.localScale;
                    }
            to.name = t.name;
            to.owner = t.gameObject;
            to.mover = GameObject.Instantiate(th.mover);
            to.mover.transform.parent = t;
            to.mover.transform.localPosition = mover.transform.localPosition;
            to.mover.transform.localRotation = mover.transform.localRotation;
            to.parents = th.parents;
            to.progress = th.progress.Clone(to);
            // Debug.Log("man dur: " + to.progress.manager.Speed);
            to.handle = th.handle.Clone(to.owner, to.mover);
            to.handle.path.element = to;
            to.areas = new();
            foreach (TameArea area in areas)
                to.areas.Add(area.Clone(to));
            to.isGrippable = isGrippable;
            to.isDistance = isDistance;
            to.isSwitch = isSwitch;
            to.manual = manual;
            to.actSwitch = actSwitch;
            to.basis = basis;
            to.changingDirection = changingDirection;
            to.initialVisibility = initialVisibility;
            to.tameType = tameType;
            to.visSwitch = visSwitch;
            to.control = control;
            return to;
        }
    }
}

