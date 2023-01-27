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
        public static bool isPaused = false;
        public static float FrameValue = -1;
        public static float deltaTime;
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
        /// the index of this element in <see cref="TameManifest.tes"/>
        /// </summary>
        public ushort index = 0;
        /// <summary>
        /// type of the element, see <see cref="TameKeys"/>. The default value is <see cref="TameKeys.Object"/>
        /// </summary>
        public TameKeys tameType = TameKeys.Object;
        /// <summary>
        /// the manifest associated with this element, which will be an inheritor class of <see cref="TameManifestBase"/>
        /// </summary>
        public TameManifestBase manifest = null;
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
        /// lis of parent effect/objects that control updating of this element. If the parent is another <see cref="TameElement"/>, this can only have one member. Normally, the update parent of an element is another element whose <see cref="GameObject"/> is the parent or first ancestor with a <see cref="TameElement"/> attached to it. But this can be changed in the manifest file (see <see cref="TameManifest"/> and <see cref="ManifestHeader.subKey"/>) with subkey "update" inside a block that defines this element. In the update line, the names of the parents are listed, separated by comma and subject to naming conventions of the <see cref="TameFinder.Relations"/>. Position tracking (@) is only acceptable for <see cref="TameObject"/> elements. The same declaring pattern applies to subkeys "slide" and "rotate" which fill <see cref="slideParents"/> and <see cref="rotateParents"/> respectively.
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

        public TameElement()
        {

        }
        /// <summary>
        /// the base method for finding the action parents of the elements in each frame. The first element of the array indicate the update parent (that if is assigned, the other two elements would be null). The next elements contain the slide and rotate parents, respectively. 
        /// </summary>
        /// <returns></returns>
        public virtual TameEffect GetParent()
        {
            return null;
        }
        public static void PassTime()
        {
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
            if (manifest != null)
                if (manifest.scales)
                    if (progress != null)
                    {
                        Vector3 ls;
                        float s = manifest.scaleFrom + (manifest.scaleTo - manifest.scaleFrom) * progress.progress;
                        foreach (GameObject go in scaledObjects)
                        {
                            //      Debug.Log("scale: " + go.name + " : " + manifest.scaleAxis+ " "+s);
                            ls = go.transform.localScale;
                            if (manifest.scaleAxis == 0) go.transform.localScale = new Vector3(s, ls.y, ls.z);
                            else if (manifest.scaleAxis == 1) go.transform.localScale = new Vector3(ls.x, s, ls.z);
                            else go.transform.localScale = new Vector3(ls.x, ls.y, s);
                        }
                        Vector2 tex;
                        for (int i = 0; i < initialTiles.Count; i++)
                            try
                            {
                                tex = scaledMaterials[i].GetTextureScale(Utils.ProperyKeywords[TameMaterial.MainTex]);
                                if (manifest.scaleUV == 0) tex.x = s * initialTiles[i]; else tex.y = s * initialTiles[i];
                                scaledMaterials[i].SetTextureScale(Utils.ProperyKeywords[TameMaterial.MainTex], tex);
                            }
                            catch { }
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
        /// <summary>
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
        public virtual void AddArea(TameArea ti, GameObject g = null) { }
        /// <summary>
        /// clean disabled interactors. Currently, it only works on <see cref="TameObject"/>s, so please see <see cref="TameObject.CleanAreas"/>
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="g"></param>

        public virtual void CleanAreas() { }

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
            TameEffect tp;
            List<TameEffect> p = null;

            p = parents;
            parents.Clear();
            basis = TrackBasis.Tame;
            // basis[1] = basis[2] = TrackBasis.Error;
            for (int i = 0; i < prog.Count; i++) p.Add(new TameEffect(subtype, prog[i]));
        }
        /// <summary>
        /// identifies all possible parents for this elements based on its manifest.
        /// </summary>
        /// <param name="tes">list of all interactive elements in the project (see <see cref="TameManifest.SurveyInteractives"/>")</param>
        /// <param name="tgos">list of all game objects related to the interactive elements (see <see cref="TameManifest.SurveyInteractives"/>)</param>
        public void PopulateUpdates(List<TameElement> tes, List<TameGameObject> tgos)
        {
            TameFinder finder = new TameFinder();
            //   Debug.Log("pop " + name);
            if (manifest != null)
            {
                if (manifest.updates != null)
                {
                    finder.header = manifest.updates;
                    finder.elementList.Clear();
                    finder.objectList.Clear();
                    finder.owner = this;
                    finder.trackMode = manifest.updateType;
                    finder.Populate(tes, tgos);
                    //    Debug.Log("inlx = " + name + " " + manifest.updateType + " " + manifest.updates.items[0] + " " + finder.elementList.Count);
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
            if (basis != TrackBasis.Tame)
                for (int i = parents.Count - 1; i >= 0; i--)
                    if (parents[i].type == TrackBasis.Tame)
                        parents.RemoveAt(i);

            if (basis != TrackBasis.Error)
                progress = new TameProgress(this);

        }
        /// <summary>
        /// Gets the parents of all interactive elements in the project, this should be called during each frame if there is a chance that parents are changed (for example there are multiple objects or people being tracked for the same element, so their position affects which one would be the parent. The method also sorts the parents so they would be updated in order
        /// </summary>
        /// <param name="allEffects">an array including all the parents for all actions for all interactive elements. As mentioned in <see cref="TameElement.GetParent"/> for each element, there are three types of potential parents. Therefore, the length of this array is three times the count of elements</param>
        /// <param name="tes">the list of all interactive elements in the project</param>
        public static int GetAllParents(TameEffect[] allEffects, List<TameElement> tes)
        {
            //  Debug.Log("findin parents for " + tes.Count);
            for (int i = 0; i < tes.Count; i++)
            {
                tes[i].AssignParent(allEffects, i);
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
        /// <summary>
        /// sets the speed, duration, cycle and trigger properties of <see cref="progress"/>es in this element based on the <see cref="manifest"/>
        /// </summary>
        public virtual void SetProgressProperties(List<TameElement> tes, List<TameGameObject> tgos)
        {
            TameFinder finder = new TameFinder();
            for (int i = 0; i < 2; i++)
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
                        progress.trigger = manifest.trigger;
                        progress.cycle = manifest.cycle;
                    }
                    if (tameType == TameKeys.Object)
                        if (((TameObject)this).handle.cycleSet)
                        {
                            progress.cycle = ((TameObject)this).handle.cycleType;
                            if (((TameObject)this).handle.duration > 0)
                                progress.manager.Duration = ((TameObject)this).handle.duration;
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
    }
}

