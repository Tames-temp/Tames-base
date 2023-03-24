using Multi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Markers;
namespace Tames
{
    /// <summary>
    /// this class establishes a link between a <see cref="gameObject"/> and its parent <see cref="TameElement"/>. This is used to quickly understand what game object is connected to which element, and use this to find parental relationships between elements as well.
    /// </summary>
    public class TameGameObject
    {
        public MarkerProgress markerProgress;
        /// <summary>
        /// this is used to know if the object is already included in a name search
        /// </summary>
        public bool alreadyFound = false;
        /// <summary>
        /// the game object in the <see cref="TameManager.RootObject"/>
        /// </summary>
        public GameObject gameObject;
        /// <summary>
        /// the lowest elements whose corresponding gameobject is <see cref="gameObject"/> or a parent or ancestor of it. 
        /// </summary>
        public TameElement tameParent;
        /// <summary>
        /// transform of the <see cref="gameObject"/>.
        /// </summary>
        public Transform transform { get { return gameObject.transform; } }
        /// <summary>
        /// finds the first <see cref="TameGameObject"/> based on its <see cref="gameObject"/> name.
        /// </summary>
        /// <param name="name">the search name</param>
        /// <param name="tgos">the list of items</param>
        /// <returns>the found item, or null if not</returns>
        public static TameGameObject Find(string name, List<TameGameObject> tgos)
        {
            foreach (TameGameObject tame in tgos)
                if (tame.gameObject.name.ToLower().Equals(name.ToLower()))
                    return tame;
            return null;
        }
        /// <summary>
        /// finds the first <see cref="TameGameObject"/> based on its <see cref="gameObject"/> name.
        /// </summary>
        /// <param name="name">the search name</param>
        /// <param name="tgos">the list of items</param>
        /// <returns>the found item, or null if not</returns>
        public static TameGameObject Find(GameObject g, List<TameGameObject> tgos)
        {
            foreach (TameGameObject tgo in tgos)
                if (tgo.gameObject == g)
                    return tgo;
            return null;
        }
        /// <summary>
        /// lists the name of objects inside a list of TameGameObjects
        /// </summary>
        /// <param name="tgos">the list of TameGameObjects</param>
        /// <returns></returns>
        public static List<GameObject> ToObjectList(List<TameGameObject> tgos)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (TameGameObject tgo in tgos)
                list.Add(tgo.gameObject);
            return list;
        }
    }
    /// <summary>
    /// this class represents all <see cref="TameElement"/>s that are mechanical in nature.
    /// </summary>
    public class TameObject : TameElement
    {
        /// <summary>
        /// the movement handle of this element. This is responsible for all machanical movements within the element, usually dictated by a <see cref="TameProgress"/>.
        /// </summary>
        public TameHandles handle;
        /// <summary>
        /// whether the <see cref="TameArea.mode"/> of this element's <see cref="areas"/> is <see cref="InteractionMode.Grip"/>
        /// </summary>
        public bool isGrippable = false;
        /// <summary>
        /// whether the <see cref="TameArea.mode"/> of this element's <see cref="areas"/> is <see cref="InteractionMode"/>.Switch1, 2, or 3
        /// </summary>
        public bool isSwitch = false;
        /// <summary>
        /// the parent tame object of this object (this is the parent object in the 3D model, not in the update hierarchy 
        /// </summary>
        public TameObject parentObject = null;
        public TameGameObject tameGameObject = null;
        public TameObject()
        {
            handle = new TameHandles();
        }
        /// <summary>
        /// see <see cref="TameElement.Update(float)"/>
        /// </summary>
        /// <param name="p"></param>
        public override void Update(float p)
        {
            if (progress != null)
            {
                float pp = progress.progress;
                SetProgress(p);
                handle.Slide(p, pp);
                handle.Rotate(p, pp);
            }
        }
        /// <summary>
        /// updates the elements based on a parent element's progress. 
        /// </summary>
        /// <param name="p"></param>
        override public void Update(TameProgress p)
        {
            SetByParent(p);
            //     base.Update(p);
            if (progress != null)
            {
                handle.Slide(progress.progress, progress.lastProgress);
                if (handle.RotationType != MovingTypes.Facer)
                    handle.Rotate(progress.progress, progress.lastProgress);
            }
        }
        /// <summary>
        /// updates the object based on position
        /// </summary>
        /// <param name="p"></param>
        override public void Update(Vector3 p)
        {
            float m = progress.progress;
            if (progress != null)
            {
                if (handle.DoesSlide)
                    m = handle.Slide(p, progress.progress, progress.manager.Speed, TameElement.deltaTime);
                else if (handle.RotationType != MovingTypes.Error)
                    m = handle.Rotate(p, progress.progress, progress.manager.Speed, TameElement.deltaTime);
                progress.SetProgress(m);
            }
        }
        /// <summary>
        /// <see cref="TameElement.Update"/>
        /// </summary>
        override public void Update()
        {
            SetByTime();
            if (progress != null)
            {
                handle.Slide(progress.progress, progress.lastProgress);
                handle.Rotate(progress.progress, progress.lastProgress);
            }
            //              SetChildren();
        }
        /// <summary>
        /// updates the element based on gripping hand
        /// </summary>
        public override void Grip(TameEffect tp)
        {
            if (progress != null)
            {
                //         float m = handle.Grip(tp.position, areas[tp.areaIndex].localGripVector, areas[tp.areaIndex].displacement, p.progress);
                //          p.SetProgress(m);
            }
        }
        public void Grip(float delta)
        {
            Debug.Log(name + " " + (progress == null ? "null" : "-"));
            if (progress != null)
            {
                progress.SetProgress(progress.progress + delta);
                if (handle.DoesSlide) handle.Slide(progress.progress, progress.lastProgress);
                if (handle.RotationType == MovingTypes.Rotator) handle.Rotate(progress.progress, progress.lastProgress);
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
        override public TameEffect GetParent()
        {
            TameEffect r = null;
            int closest;
            float d;
            float dis;
            float min = float.PositiveInfinity;
            bool[] set = new bool[] { false, false, false };
            TameArea ti;
            Person pe;
            //   Debug.Log("before error 1");
            if (isGrippable)
            {
                r = TameArea.Grip(areas);
                if (r != null)
                {
                    pe = r.personIndex == Person.LocalDefault ? Person.localPerson : Person.people[r.personIndex];
                    ti = areas[r.areaIndex];
                    ti.gripDisplacement = owner.transform.InverseTransformPoint(pe.hand[r.handIndex].gripCenter) - owner.transform.InverseTransformPoint(ti.relative.transform.position);
                    if (handle.RotationType == MovingTypes.Rotator)
                        ti.displacement = Utils.SignedAngle(owner.transform.InverseTransformPoint(ti.relative.transform.position), Vector3.zero, handle.start - handle.pivot, handle.axis);
                    r.child = this;
                }
                return r;
            }
            //    Debug.Log("before error 2");
            if (isSwitch)
            {
                //      Debug.Log("before error 2.1 "+name +" "+areas.Count);
                if (areas[0].geometry == InteractionGeometry.Remote)
                {
                    //         Debug.Log("before error 2.15 "+ areas[0].key);
                    if (TameInputControl.checkedKeys[areas[0].key].wasPressedThisFrame)
                    {
                        areas[0].Switch(true);
                        r = TameEffect.Time();
                        r.child = this;
                        changingDirection = areas[0].switchDirection;
                    }
                }
                else
                {
                    //        Debug.Log("before error 2.2");
                    int sd = TameArea.CheckSwitch(areas);
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
            //      Debug.Log("before error 3");
            Person headOwner = null, handOwner = null;
            Vector3 closestPosition = Vector3.positiveInfinity;
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
            //      Debug.Log("before error 4");
            return r;
        }
        override public void CleanAreas()
        {
            isGrippable = isSwitch = false;
            int retain = 1;
            foreach (TameArea ti in areas)
                if (ti.mode == InteractionMode.Grip)
                {
                    isGrippable = true;
                    break;
                }
            if (!isGrippable)
                foreach (TameArea ti in areas)
                    if (ti.geometry == InteractionGeometry.Remote)
                    {
                        retain = 1;
                        isSwitch = true;
                        break;
                    }
                    else if (TameArea.IsSwitch(ti.mode))
                    {
                        retain = 2;
                        isSwitch = true;
                        break;
                    }

            if (isGrippable || isSwitch)
            {
                for (int i = areas.Count - 1; i >= 0; i--)
                    if (((retain == 1) && (areas[i].mode != InteractionMode.Grip) && (areas[i].geometry != InteractionGeometry.Remote)) || ((retain == 2) && (!TameArea.IsSwitch(areas[i].mode))))
                        areas.RemoveAt(i);
                parents.Clear();
                basis = TrackBasis.Grip;
                //       Debug.Log(name + " " + handle.DoesSlide + " " + handle.RotationType);
            }
        }
        override public void AddArea(TameArea ti, GameObject g = null)
        {
            TameArea ti2 = ti;
            if (ti.update == InteractionUpdate.Object)
            {
                ti2 = ti.Duplicate();
                ti2.gameObject = new GameObject(g.name);
                ti2.gameObject.transform.SetParent(g.transform);
                ti2.gameObject.transform.rotation = g.transform.rotation;
                ti2.gameObject.transform.localScale = g.transform.localScale;
                ti2.SetUpdate(this, g);
            }
            if (ti.update == InteractionUpdate.Mover)
            {
                ti2 = ti.Duplicate();
                ti2.gameObject = new GameObject();
                ti2.gameObject.transform.SetParent(mover.transform);
                ti2.gameObject.transform.rotation = ti.gameObject.transform.rotation;
                ti2.gameObject.transform.localScale = ti.gameObject.transform.localScale;
                ti2.SetUpdate(this, g);
            }
            if (ti.update != InteractionUpdate.Parent)
            {
                ti2 = ti.Duplicate();
                ti2.gameObject = new GameObject();
                ti2.gameObject.transform.SetParent(owner.transform);
                ti2.gameObject.transform.rotation = ti.gameObject.transform.rotation;
                ti2.gameObject.transform.localScale = ti.gameObject.transform.localScale;
                ti2.SetUpdate(this, g);
            }
            areas.Add(ti2);
        }
        public override void AssignParent(TameEffect[] all, int index)
        {
            TameEffect ps = GetParent();
            all[index] = ps;
        }
        public static TameObject Create(GameObject g)
        {
            TameHandles handle = TameHandles.GetHandles(g);
            if (handle != null)
            {
                TameObject to = new TameObject() { handle = handle };
                to.mover = handle.mover;
                to.owner = g;
                to.name = g.name;
                to.markerProgress = g.GetComponent<MarkerProgress>();
                return to;
            }
            return null;
        }
        public static List<TameGameObject> CreateInteractive(TameElement parentElement, GameObject owner, List<TameElement> tes, int software = -1)
        {
            MarkerOrigin mo;
            if ((mo = owner.GetComponent<MarkerOrigin>()) != null)
                software = mo.GetOrigin();
            if (owner.GetComponent<Light>() != null)
            {
                TameLight tl = new TameLight() { name = owner.name, owner = owner, light = owner.GetComponent<Light>(), index = (ushort)tes.Count };
                tl.GetAreas(software);
                tl.parents.Add(new TameEffect()
                {
                    type = parentElement.tameType == TameKeys.Time ? TrackBasis.Time : TrackBasis.Tame,
                    parent = parentElement,
                    child = tl,
                });
                MarkerProgress mp = owner.GetComponent<MarkerProgress>();
                if (mp != null) tl.markerProgress = mp;
                //     tl.changers = owner.GetComponents<MarkerChanger>();
                tes.Add(tl);
            }
            List<TameGameObject> tgo = new List<TameGameObject>();
            TameGameObject tg = null;
            GameObject gi;
            int cc = owner.transform.childCount;
            //    TameElement te;
            TameObject obj;
            //     TameElement leader = null;
            TameElement[] local = new TameElement[cc];
            //     Debug.Log("creating " + owner.name);
            //     Debug.Log("check: " + owner.name+" "+ cc);

            for (int i = 0; i < cc; i++)
            {
                local[i] = null;
                gi = owner.transform.GetChild(i).gameObject;
                //    Debug.Log("check: " + owner.name + " ?");
                if (!TameHandles.HandleKey(gi.name))
                {
                    obj = Create(gi);
                    if (obj != null)
                    {
                        obj.GetAreas();
                        obj.parentObject = parentElement.tameType == TameKeys.Object ? (TameObject)parentElement : null;
                        obj.index = (ushort)tes.Count;
                        tes.Add(obj);
                        local[i] = obj;
                        if (obj.handle.trackBasis == TrackBasis.Head)
                        {
                            obj.basis = TrackBasis.Head;
                        }
                        else if (parentElement.tameType == TameKeys.Time)
                            obj.basis = TrackBasis.Time;
                        else
                            obj.parents.Add(new TameEffect()
                            {
                                type = TrackBasis.Tame,
                                //           effect = 0,
                                parent = parentElement,
                                child = obj
                            });

                    }
                    tg = new TameGameObject()
                    {
                        gameObject = gi,
                        tameParent = obj == null ? parentElement : obj
                    };
                    tgo.Add(tg);
                    if (obj != null) obj.tameGameObject = tg;
                    if (gi.name == "goarea")
                        Debug.Log("TGO: " + tg.gameObject.name + (tg.gameObject.GetComponent<MarkerArea>() == null));
                }
            }
            //       Debug.Log("check: " + owner.name + " " + cc);
            for (int i = 0; i < cc; i++)
            {
                //       Debug.Log("check: " + owner.name + " at " + i + " of "+ cc);
                gi = owner.transform.GetChild(i).gameObject;
                tgo.AddRange(CreateInteractive(local[i] ?? parentElement, gi, tes, software));
            }

            return tgo;
        }
        public void CreateClones(List<TameGameObject> gos, List<TameElement> tes)
        {
            List<Vector3> linkedPositions = new List<Vector3>();
            List<Quaternion> linkedRotations = new List<Quaternion>();
            TameObject to;
            TameArea ta;
            Transform t;
            for (int i = 0; i < gos.Count; i++)
            {
                linkedPositions.Add(gos[i].transform.position - mover.transform.position);
                linkedRotations.Add(gos[i].transform.rotation * Quaternion.Inverse(mover.transform.rotation));
                to = new TameObject();
                to.name = gos[i].gameObject.name;
                to.progress = progress == null ? null : new TameProgress(progress);
                to.progress = progress == null ? null : new TameProgress(progress);
                to.parents = parents;
                to.basis = basis;
                to.isGrippable = isGrippable;
                to.handle = TameHandles.Duplicate(handle, gos[i].gameObject);
                to.index = (ushort)tes.Count;
                to.mover = gos[i].gameObject;
                to.owner = gos[i].transform.parent.gameObject;
                to.tameType = TameKeys.Object;
                foreach (TameArea a in areas)
                {
                    if (a.update == InteractionUpdate.Fixed)
                        to.areas.Add(a);
                    else
                    {
                        ta = a.Duplicate();
                        if (a.update == InteractionUpdate.Object)
                        {
                            t = gos[i].transform.Find(a.relative.name);
                            if (t != null)
                                ta.SetUpdate(to, t.gameObject);
                            else
                                ta.SetUpdate(to, gos[i].gameObject);
                        }
                        else
                            ta.SetUpdate(to, null);
                        to.areas.Add(ta);
                    }
                }
                tes.Add(to);
            }

        }
    }
}