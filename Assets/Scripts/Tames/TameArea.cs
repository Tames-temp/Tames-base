using Assets.Script;
using Multi;
using System;
using System.Collections.Generic;
using UnityEngine;
using Markers;
namespace Tames
{
    /// <summary>
    /// This class helps to determine which person and how they are interacting with an area
    /// </summary>
    public class TameAreaTrack
    {
        public int head = Person.LocalDefault;
        public int person = Person.LocalDefault;
        public int hand = 0;
        public int direction = 1;
        public int handArea = -1;
        public int headArea = -1;
    }
    /// <summary>
    /// this class handles the interaction between the interactive elements and people or time. Instances of this class are called interactors (See <see cref="TameObject.areas"/> 
    /// </summary>
    public class TameArea
    {
        public static List<TameArea> switchers = new List<TameArea>();
        /// <summary>
        /// the initial gameobject of the area, that also contains its geometry. This would be set to invisible
        /// </summary>
        public GameObject gameObject = null;
        /// <summary>
        /// a new empty game object that is made for updating the area (if it is not fixed. See <see cref="InteractionUpdate"/>).
        /// </summary>
        public GameObject relative;
        /// <summary>
        /// the shape of the area
        /// </summary>
        public InteractionGeometry geometry;
        /// <summary>
        /// the mode of the interaction between a person and the area
        /// </summary>
        public InteractionMode mode;
        /// <summary>
        /// the update mode of the area's movement
        /// </summary>
        public InteractionUpdate update;
        /// <summary>
        /// the initial position of the interactor when it is constructed
        /// </summary>
        public Vector3 localGripVector;
        public float displacement = 0;
        /// <summary>
        /// the difference between the center of grip and the center of this area at the moment grip began.
        /// </summary>
        public Vector3 gripDisplacement = Vector3.zero;
        /// <summary>
        /// the local scale of the area (the <see cref="relative"/> object)
        /// </summary>
        public Vector3 size;
        /// <summary>
        /// the axes of the area's object transform, used to  
        /// </summary>
        public int upAxis = -1;
        public Vector3[] initialAxis;
        /// <summary>
        /// when this is true, if the are is active, no other area would be tested. 
        /// </summary>
        public bool exclusive = false;
        /// <summary>
        /// the index of the person interacting with the area
        /// </summary>
        public int personIndex = -1;
        /// <summary>
        /// the index of the hand (left or right)
        /// </summary>
        public int handIndex = -1;
        /// <summary>
        /// the current state of the area's switch mode 
        /// </summary>
        public int switchDirection = 1;
        /// <summary>
        /// the number of switch states
        /// </summary>
        public int switchStates = 1;
        /// <summary>
        /// if the area's switch was activated in this frame
        /// </summary>
        public int forcedSwitchThisFrame = -1;
        /// <summary>
        /// the name of objects in the interactor's manifest that are attached to the instances of this interactor. The objects are names of game objects with naming patterns explained in <see cref="TameFinder.Relations"/> though not accepting the rotation operator. This lise is based on the list of objects in the manifest line with subkey of "update" (see <see cref="ManifestHeader.subKey"/> with names separated by commas:
        ///     update names,name2,...
        /// </summary>
        public List<string> attachedObjects;
        public int key = -1;
        public TameElement element;
        public bool passedFirstPlane = false;
        private const int X = 0;
        private const int Y = 1;
        private const int Z = 2;
        public const string AreaInteraction = "iopng123";
        public const string AreaSpace = "exiog123";
        public const string AreaGeometry = "bcs";
        public const string AreaUpdate = "flmo";
        public string keyString = "";
        public static bool HasAreaKeyword(string name)
        {
            string tl = name.ToLower();
            return tl.StartsWith(TameHandles.KeyAreaBox) || tl.StartsWith(TameHandles.KeyAreaCube) || tl.StartsWith(TameHandles.KeyAreaCylinder) || tl.StartsWith(TameHandles.KeyAreaSphere);
        }
        public int OutsideDirection
        {
            get
            {
                switch (mode)
                {
                    case InteractionMode.Outside: return 1;
                    case InteractionMode.OutIn: return 1;
                    case InteractionMode.Inside: return 0;
                    case InteractionMode.InOut: return -1;
                }
                return 0;
            }
        }
        public int InsideDirection
        {
            get
            {
                switch (mode)
                {
                    case InteractionMode.Outside: return 0;
                    case InteractionMode.OutIn: return -1;
                    case InteractionMode.Inside: return 1;
                    case InteractionMode.InOut: return 1;
                }
                return 0;
            }
        }
        /// <summary>
        /// checks if a world-space point is inside the geometry of the area
        /// </summary>
        /// <param name="p"></param>
        /// <returns>true if p is inside</returns>
        public bool Inside(Vector3 p, bool calledFromOutside = false)
        {
            float m;
            Vector3 q;
            bool r = false;
            Vector3 center = relative.transform.position;
            Vector3[] axis = new Vector3[] { relative.transform.right, relative.transform.up, relative.transform.forward };
            Vector3 size = relative.transform.localScale * (calledFromOutside ? 1.05f : 1);
            switch (geometry)
            {
                case InteractionGeometry.Plane:
                    m = Utils.M(p, center, axis[Z]);
                    if (m <= 0) return false;
                    q = p + m * axis[Z];
                    Vector2 mn = Utils.MN(q - center, axis[X] * size[X] / 2, axis[Y] * size[Y] / 2);
                    float d = Vector3.Distance(p, center);
                    r = (Mathf.Abs(mn.x) < 1) && (Mathf.Abs(mn.y) < 1);
                    break;
                //     return r;
                case InteractionGeometry.Box:
                    r = (Mathf.Abs(Utils.M(p, center, axis[X])) < size.x / 2) && (Mathf.Abs(Utils.M(p, center, axis[Y])) < size.y / 2) && (Mathf.Abs(Utils.M(p, center, axis[Z])) < size.z / 2);
                    break;
                case InteractionGeometry.Sphere:
                    r = Vector3.Distance(p, center) < size[X] / 2; break;
                case InteractionGeometry.Cylinder:
                    m = Utils.M(p, center, axis[Y]);
                    if (Mathf.Abs(m) < size[Y])
                    {
                        q = p + m * axis[Y];
                        r = Vector3.Distance(q, center) < size.x / 2;
                    }
                    else
                        r = false;
                    break;
                default: r = false; break;
            }
            //       if (element.name == "door3") Debug.Log("door 3 " + p.ToString("0.00") + size.ToString("0.00") + center.ToString("0.00"));
            return r;
        }
        /// <summary>
        /// checks if a world-space point is outside the geometry of the area
        /// </summary>
        /// <param name="p"></param>
        /// <returns>true if p is outside</returns>
        public bool Outside(Vector3 p)
        {
            return !Inside(p, true);
        }
        /// <summary>
        /// checks if a transform has entered the area's geometry when moving from one point to another. This returns true only if the previous point was outside and current point is inside the geometry (not if the path from the two points intersects with it).
        /// </summary>
        /// <param name="a">the previous point</param>
        /// <param name="b">the current point</param>
        /// <returns></returns>
        public bool Entered(Vector3 a, Vector3 b)
        {
            return Inside(b) && Outside(a);
        }
        /// <summary>
        /// checks if a transform has left the area's geometry when moving from one point to another. This returns true only if the previous point was inside current point is outside the geometry (not if the path from the two points intersects with it).
        /// </summary>
        /// <param name="a">the previous point</param>
        /// <param name="b">the current point</param>
        /// <returns></returns>
        public bool Exited(Vector3 a, Vector3 b)
        {
            return Inside(a) && Outside(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool GripAround(Vector3 p, Vector3 v)
        {
            float rad = relative.transform.localScale.x / 2;
            float h2 = 0.05f;
            float m = Utils.M(p, relative.transform.position, v);
            if (Mathf.Abs(m) <= h2)
                if (Vector3.Distance(relative.transform.position, p + m * v) <= rad)
                    return true;
            return false;
        }

        /// <summary>
        /// duplicates an area for assigning to multiple objects
        /// </summary>
        /// <returns></returns>
        public TameArea Duplicate()
        {
            TameArea r = new TameArea();
            r.mode = mode;
            //    r.objectName = objectName;
            r.geometry = geometry;
            r.update = update;
            return r;
        }
        /// <summary>
        /// checks if an area is already occupied for a person
        /// </summary>
        /// <param name="i">the index of the person</param>
        /// <returns></returns>

        /// <summary>
        /// checks if any of the hands have gripped in the area <see cref="TameElement"/>.
        /// </summary>
        /// <param name="tis">list of the area of an element</param>
        /// <returns>a <see cref="TameEffect"/> object containing the person's index and the index of the gripping hand</returns>
        public static TameEffect Grip(List<TameArea> tis)
        {
            int from = MainScript.multiPlayer ? 0 : Person.LocalDefault;
            int to = MainScript.multiPlayer ? Person.people.Length : Person.LocalDefault + 1;
            Person person;
            for (int i = from; i < to; i++)
            {
                person = i == Person.LocalDefault ? MainScript.localPerson : Person.people[i];
                //       Debug.Log("person : " + person.id);
                if (person != null)
                    for (int j = 0; j < 2; j++)
                    {
                        //       Debug.Log("grip : " + person.grip[j]);
                        if (person.grip[j] > HandAsset.HandModel.GripThreshold)
                            for (int t = 0; t < tis.Count; t++)
                                if (tis[t].Inside(person.hand[j].gripCenter))
                                {
                                    return new TameEffect()
                                    {
                                        type = TrackBasis.Grip,
                                        personIndex = i,
                                        handIndex = j,
                                        areaIndex = t,
                                        position = person.hand[j].gripCenter,
                                    };
                                }
                    }
            }
            return null;
        }
        /// <summary>
        /// checks if an area mode is of a switch type
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public static bool IsSwitch(InteractionMode space)
        {
            return (space == InteractionMode.Switch1) || (space == InteractionMode.Switch2) || (space == InteractionMode.Switch3);
        }
        public const int NotSwitched = -3;
        /// <summary>
        /// checks if any of the hands has switched on or off.
        /// </summary>
        /// <param name="areas">list of the areas of an element</param>
        /// <returns>a <see cref="TameEffect"/> object containing the person's index and the index of the gripping hand</returns>
        public static int CheckSwitch(List<TameArea> areas)
        {
            for (int t = 0; t < areas.Count; t++)
                if (areas[t].forcedSwitchThisFrame == TameElement.Tick)
                    return areas[t].switchDirection;
            for (int i = 0; i < Person.people.Length; i++)
            {
                if (Person.people[i] != null)
                    for (int j = 0; j < 2; j++)
                        for (int t = 0; t < areas.Count; t++)
                            if (IsSwitch(areas[t].mode))
                                if (areas[t].Entered(Person.people[i].hand[j].lastTipPosition[HandAsset.HandModel.Middle], Person.people[i].hand[j].tipPosition[HandAsset.HandModel.Middle]))
                                {
                                    areas[i].Switch(false);
                                    return areas[i].switchDirection;
                                }
            }
            return NotSwitched;
        }
        /// <summary>
        /// switches an area to its next state
        /// </summary>
        /// <param name="forced"></param>
        public void Switch(bool forced)
        {
            Debug.Log("switch " + switchDirection);
            switch (mode)
            {
                case InteractionMode.Switch1: switchDirection = 1 - switchDirection; break;
                case InteractionMode.Switch2: switchDirection = -switchDirection; break;
                case InteractionMode.Switch3: switchDirection = switchDirection == 1 ? -1 : switchDirection + 1; break;
            }
            if (forced)
                forcedSwitchThisFrame = TameElement.Tick;
        }
        public static TameAreaTrack Track(Vector3 p)
        {
            int from = MainScript.multiPlayer ? 0 : Person.LocalDefault;
            int to = MainScript.multiPlayer ? Person.people.Length : Person.LocalDefault + 1;
            float d, minHead = float.PositiveInfinity, minHand = float.PositiveInfinity;
            Person pers;
            int pIndex = -1;
            int oIndex = -1;
            int hIndex = -1;
            for (int i = from; i < to; i++)
            {
                pers = i == Person.LocalDefault ? MainScript.localPerson : Person.people[i];

                if (pers != null)
                {
                    //      Debug.Log("raw: " + pers.headPosition.ToString("0.00"));
                    if ((d = Vector3.Distance(p, pers.headPosition)) < minHead)
                    {
                        minHead = d;
                        pIndex = i;
                    }
                    for (int j = 0; j < 2; j++)
                        if ((d = Vector3.Distance(p, pers.position[j])) < minHand)
                        {
                            minHand = d;
                            oIndex = i;
                            hIndex = j;
                        }
                }
            }
            return new TameAreaTrack() { head = pIndex, hand = hIndex, person = oIndex, direction = 1 };
        }
        public static TameAreaTrack TrackWithAreas(List<TameArea> tis, Vector3 p)
        {
            int from = MainScript.multiPlayer ? 0 : Person.LocalDefault;
            int to = MainScript.multiPlayer ? Person.people.Length : Person.LocalDefault + 1;
            float d, minHead = float.PositiveInfinity, minHand = float.PositiveInfinity;
            Person person;
            int pIndex = -1;
            int oIndex = -1;
            int hIndex = -1;
            int headarea = 0, handarea = 0;
            bool inside, aPersonInside = false;
            string s = "";
            int dir = tis[0].OutsideDirection;
            for (int i = from; i < to; i++)
            {
                for (int a = 0; a < tis.Count; a++)
                {
                    person = i == Person.LocalDefault ? MainScript.localPerson : Person.people[i];
                    if (person != null)
                    {
                        inside = tis[a].Inside(person.headPosition);
                        s += " " + inside + (inside ? tis[a].InsideDirection + "" : "-");
                        if (pIndex == -1)
                            pIndex = i;
                        d = Vector3.Distance(p, person.headPosition);
                        {
                            if (inside)
                            {
                                if ((d < minHead) || (!aPersonInside))
                                {
                                    minHead = d;
                                    pIndex = i;
                                    headarea = a;
                                    dir = tis[a].InsideDirection;
                                }
                                aPersonInside = true;
                            }
                            else
                            {
                                if ((d < minHead) && (!aPersonInside))
                                {
                                    minHead = d;
                                    pIndex = i;
                                    headarea = a;
                                    dir = tis[a].OutsideDirection;
                                }
                            }
                        }
                        for (int j = 0; j < 2; j++)
                            if ((d = Vector3.Distance(p, person.position[j])) < minHand)
                            {
                                minHand = d;
                                oIndex = i;
                                hIndex = j;
                                if (inside) handarea = a;
                            }
                        //      Debug.Log("area: " +dir+" "+ person.headPosition.ToString("0.00"));
                    }
                }
            }
            //     if (tis[0].element.name == "door3")                Debug.Log("owner " + dir + s);
            return new TameAreaTrack() { head = pIndex, hand = hIndex, person = oIndex, direction = dir, headArea = headarea, handArea = handarea };
        }
        /// <summary>
        /// finds the person or hand with the closest position to point, who is interacting with an interactor.
        /// </summary>
        /// <param name="tis">list of the interactors of an element</param>
        /// <param name="p">the position to which the closest is sought</param>
        /// <param name="closestPosition">outputs of the closest position (the position of the head or hand</param>
        /// <returns>returns three integers values which resectively contain, the person's index, the hand's index, and the changing direction of progress (see <see cref="InteractionMode"/> and <see cref="TameProgress.changingDirection"/>"/></returns>
        public static TameAreaTrack Track(List<TameArea> tis, Vector3 p)
        {
            float min = float.PositiveInfinity, d;
            int pIndex = -1;
            int hIndex = -1;
            int oIndex = -1;
            int dir = 0;
            int headArea = -1;
            int handArea = -1;
            TameArea t;
            int lastHolder = -1;
            //    closestPosition = Vector3.zero;
            TameAreaTrack tef;
            TameAreaTrack r = null;


            int from = MainScript.multiPlayer ? 0 : Person.LocalDefault;
            int to = MainScript.multiPlayer ? Person.people.Length : Person.LocalDefault + 1;
            Person person;
            if (r == null)
            {
                for (int k = 0; k < tis.Count; k++)
                    for (int i = from; i < to; i++)
                        if (!(t = tis[k]).exclusive)
                        {
                            person = i == Person.LocalDefault ? MainScript.localPerson : Person.people[i];

                            if (person != null)
                                if (t.Inside(person.headPosition))
                                {
                                    if (t.mode != InteractionMode.Outside)
                                        if ((d = Vector3.Distance(t.relative.transform.position, p)) < min)
                                        {
                                            min = d;
                                            pIndex = i;
                                            dir = t.mode == InteractionMode.OutIn ? -1 : 1;
                                            headArea = k;
                                        }
                                }
                                else if (t.mode != InteractionMode.Inside)
                                    if ((d = Vector3.Distance(t.relative.transform.position, p)) < min)
                                    {
                                        min = d;
                                        pIndex = i;
                                        dir = t.mode == InteractionMode.InOut ? -1 : 1;
                                        headArea = k;
                                    }
                        }
            }
            else
            {
                pIndex = r.head;
                dir = r.direction;
                headArea = r.headArea;
            }
            min = float.PositiveInfinity;
            for (int i = from; i < to; i++)
            {
                person = i == Person.LocalDefault ? MainScript.localPerson : Person.people[i];
                if (person != null)
                    for (int k = 0; k < tis.Count; k++)
                        for (int j = 0; j < 2; j++)
                        {
                            if ((t = tis[k]).Inside(person.position[j]))
                            {
                                if (t.mode != InteractionMode.Outside)
                                    if ((d = Vector3.Distance(t.relative.transform.position, p)) < min)
                                    {
                                        hIndex = j;
                                        min = d;
                                        oIndex = i;
                                        //                 dir = t.space == InteractionSpace.OutIn ? -1 : 1;
                                        handArea = k;
                                    }
                            }
                            else if (t.mode != InteractionMode.Inside)
                                if ((d = Vector3.Distance(t.relative.transform.position, p)) < min)
                                {
                                    min = d;
                                    oIndex = i;
                                    hIndex = j;
                                    //               dir = t.space == InteractionSpace.InOut ? -1 : 1;
                                    handArea = k;
                                }
                        }
            }
            return new TameAreaTrack() { head = pIndex, hand = hIndex, direction = dir, headArea = headArea, handArea = handArea, person = oIndex };
        }
        /// <summary>
        /// sets the transform properties of the <see cref="relative"/> object based on <see cref="update"/> field
        /// </summary>
        /// <param name="to">the element associated with this interactor. It can be null if <see cref="update"/> is not set to <see cref="InteractionUpdate.Mover"/></param>
        /// <param name="g">a game object for attaching to this interactor. It can be null it the <see cref="update"/> is not set to <see cref="InteractionUpdate.Object"/></param>
        public void SetUpdate(TameObject to, GameObject g)
        {
            Vector3 scale = Vector3.one, v;
            switch (geometry)
            {
                case InteractionGeometry.Sphere: scale = Utils.DetectSphere(g); break;
                case InteractionGeometry.Box:
                    scale = Utils.DetectBox(g);
                    //    Debug.Log("area " + scale.ToString("0.00"));
                    break;
                case InteractionGeometry.Cylinder:
                    scale = v = Utils.DetectCylinder(g, out upAxis);
                    if (upAxis >= 0)
                    {
                        scale.y = v[upAxis];
                        scale.x = scale.z = v[(upAxis + 1) % 3];
                    }
                    break;
            }

            relative = new GameObject();
            //   relative.SetActive(false);
            relative.transform.localScale = scale;
            relative.transform.rotation = gameObject.transform.rotation;
            if (to.name == "rotat") Debug.Log("switch " + gameObject.name + " " + update);
            switch (update)
            {
                case InteractionUpdate.Fixed:
                    relative.transform.position = gameObject.transform.position;
                    break;
                case InteractionUpdate.Parent:
                    relative.transform.parent = gameObject.transform.parent;
                    relative.transform.position = gameObject.transform.position;
                    break;
                case InteractionUpdate.Mover:
                    relative.transform.parent = to.mover.transform;
                    relative.transform.localPosition = to.mover.transform.InverseTransformPoint(gameObject.transform.position);
                    break;
                case InteractionUpdate.Object:
                    relative.transform.parent = g.transform;
                    relative.transform.localPosition = g.transform.InverseTransformPoint(gameObject.transform.position);
                    break;
            }

            float m;

            if (to.handle.DoesSlide)
            {
                displacement = Utils.M(relative.transform.position, to.handle.from, to.handle.vector);
            }
            else
            {
                m = Utils.SignedAngle(relative.transform.position, to.handle.pivot, to.handle.start, to.handle.axis);
                displacement = m / to.handle.Span;
            }
            //      Debug.Log("arix: gp befor " + gameObject.transform.position.ToString("0.00"));
            //  gameObject.transform.parent = relative.transform;
            //   gameObject.transform.localPosition = Vector3.zero;
            //       Debug.Log("arix: gp after " + gameObject.transform.position.ToString("0.00"));
        }
        static InteractionMode GetMode(int i)
        {
            switch (i)
            {
                case 0: return InteractionMode.Inside;
                case 1: return InteractionMode.Outside;
                case 2: return InteractionMode.InOut;
                case 3: return InteractionMode.OutIn;
                case 4: return InteractionMode.Grip;
                case 5: return InteractionMode.Switch1;
                case 6: return InteractionMode.Switch2;
                case 7: return InteractionMode.Switch3;
            }
            return InteractionMode.Inside;
        }
        public static TameArea ImportArea(GameObject g, TameObject to)
        {
            TameArea r = null;
            int m;
            InteractionGeometry geom;
            MarkerArea ma = g.transform.GetComponent<MarkerArea>();
            if (ma != null)
            {
                r = new TameArea()
                {
                    geometry = ma.GetGeometry(),
                    update = ma.GetUpdate(),
                    mode = ma.GetMode(),
                    gameObject = g,
                    element = to,
                };
                if (r.geometry == InteractionGeometry.Remote)
                {
                    r.key = ManifestCustom.FindKey(ma.input);
                    Debug.Log("area key = " + r);
                }
                if (r.geometry == InteractionGeometry.Cylinder)
                    r.upAxis = Utils.DetectCylinderVector(g);
                r.SetUpdate(to, g);
                g.SetActive(false);
            }
            else
            {
                string aname = g.name.ToLower();
                if (aname.StartsWith(TameHandles.KeyAreaBox) || aname.StartsWith(TameHandles.KeyAreaCube))
                    geom = InteractionGeometry.Box;
                else if (aname.StartsWith(TameHandles.KeyAreaCylinder))
                    geom = InteractionGeometry.Cylinder;
                else if (aname.StartsWith(TameHandles.KeyAreaSphere))
                    geom = InteractionGeometry.Sphere;
                else if (aname.StartsWith(TameHandles.KeyAreaSwitch))
                    geom = InteractionGeometry.Remote;
                else return null;
                int p = aname.IndexOf("_", 1);
                if ((p > 0) && (p < aname.Length - 1))
                {
                    if (geom == InteractionGeometry.Remote)
                    {
                        p = ManifestCustom.FindKey(aname[p + 1] + "");
                        if (p >= 0)
                        {
                            r = new TameArea()
                            {
                                geometry = geom,
                                key = p,
                                gameObject = g,
                                element = to,
                            };
                        }
                    }
                    else
                    {
                        m = AreaInteraction.IndexOf(aname[p + 1]);
                        if (m >= 0)
                        {
                            r = new TameArea()
                            {
                                geometry = geom,
                                mode = GetMode(m),
                                gameObject = g,
                                element = to,
                            };
                            if (r.geometry == InteractionGeometry.Cylinder)
                                r.upAxis = Utils.DetectCylinderVector(g);
                            switch (m)
                            {
                                case 0: case 1: case 2: case 3: r.update = InteractionUpdate.Fixed; break;
                                case 4: r.update = InteractionUpdate.Mover; break;
                                case 5: case 6: case 7: r.update = InteractionUpdate.Parent; break;
                            }
                            p = aname.IndexOf("_", p + 1);
                            if ((p > 0) && (p < aname.Length - 1))
                            {
                                p = AreaUpdate.IndexOf(aname[p + 1]);
                                switch (p)
                                {
                                    case 0: r.update = InteractionUpdate.Fixed; break;
                                    case 1: r.update = InteractionUpdate.Parent; break;
                                    case 2: r.update = InteractionUpdate.Mover; break;
                                }
                            }
                            //      if (to.name == "rotat")                                Debug.Log("switch: name = " + g.name);
                            r.SetUpdate(to, g);
                            g.SetActive(false);
                        }
                    }
                }
            }
            // Debug.Log("area return: "+r.element.name + " " + r.mode + " ");
            return r;
        }
        /// <summary>
        /// creates an area from a game object based on valid keys (see <see cref="TameHandles.HandleKey"/>
        /// </summary>
        /// <param name="g">the game object</param>
        /// <param name="to">the parent element to which the interactor is assigned to</param>
        /// <returns>the created interactor, null if the naming is not valid</returns>
        public static TameArea GetArea(GameObject g, TameObject to)
        {
            TameArea r = null;
            int upd, geom, space;
            string name;
            string iname = g.name.ToLower();
            if ((iname.Length > 8) && iname.StartsWith(TameHandles.KeyArea))
            {
                upd = AreaUpdate.IndexOf(iname[6]);
                geom = AreaGeometry.IndexOf(iname[7]);
                space = AreaSpace.IndexOf(iname[8]);
                //   Debug.Log("arex: " + g.name);
                if ((upd >= 0) && (geom >= 0) && (space >= 0))
                {
                    r = new TameArea();
                    //   r.objectName = iname;
                    r.mode = (InteractionMode)(Enum.GetValues(r.mode.GetType())).GetValue(space);
                    r.geometry = (InteractionGeometry)(Enum.GetValues(r.geometry.GetType())).GetValue(geom);
                    r.update = (InteractionUpdate)(Enum.GetValues(r.update.GetType())).GetValue(upd);
                    if (r.geometry == InteractionGeometry.Cylinder)
                        r.upAxis = Utils.DetectCylinderVector(g);
                    if (iname.Length > 9)
                        if (iname[9] == 'h')
                            r.exclusive = true;
                    r.gameObject = g;
                    r.element = to;
                    r.SetUpdate(to, g);
                    g.SetActive(false);
                    //       Debug.Log(g.name + " is an area");
                }
            }
            return r;
        }

        /// <summary>
        /// finds the closest interaction grip area in front of the camera. This is used for non-full-VR applications of Tames
        /// </summary>
        /// <param name="tes">list of all elements</param>
        /// <param name="camera">the camera's transform</param>
        /// <param name="maxDist">maximum distance allowed for hand's reach</param>
        /// <param name="grippableObject">outputs the the grippable objects</param>
        /// <returns>return the area aound the grippable objects. It returns null if no object is found</returns>
        public static TameArea ClosestGrip(List<TameElement> tes, Transform camera, float maxDist, out TameObject grippableObject)
        {
            grippableObject = null;
            TameObject to;
            float d, min = maxDist + 0.01f;
            TameArea r = null;
            foreach (TameElement te in tes)
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    //  Debug.Log("area looking: " + te.name);
                    if (to.isGrippable)
                    {
                        Debug.Log("area grippable: " + te.name);
                        foreach (TameArea area in to.areas)
                            if ((d = Vector3.Distance(area.relative.transform.position, camera.position)) < min)
                                if (Vector3.Angle(camera.forward, area.relative.transform.position - camera.position) < 90)
                                {
                                    r = area;
                                    min = d;
                                    grippableObject = to;
                                }
                    }
                }
            return r;
        }
        /// <summary>
        /// finds the closest switch area in front of the camera.
        /// </summary>
        /// <param name="tes">list of all elements</param>
        /// <param name="cam">camera's transform</param>
        /// <param name="maxDist">maximum distance for the hand's reach</param>
        /// <param name="switchObject">output the found switch objects</param>
        /// <returns></returns>
        public static TameArea ClosestSwitch(List<TameElement> tes, Transform cam, float maxDist, out TameObject switchObject)
        {
            switchObject = null;
            TameObject to;
            float d, min = maxDist + 0.01f;
            TameArea r = null;
            foreach (TameElement te in tes)
                if (te.tameType == TameKeys.Object)
                {
                    to = (TameObject)te;
                    if (to.isSwitch)
                    {
                        foreach (TameArea area in to.areas)
                        {
                            if ((d = Vector3.Distance(area.relative.transform.position, cam.position)) < min)
                                if (Vector3.Angle(cam.forward, area.relative.transform.position - cam.position) < 90)
                                {
                                    r = area;
                                    min = d;
                                    switchObject = to;
                                }
                            Debug.Log("switch is " + te.name + " " + area.relative.transform.position.ToString());
                        }
                    }
                }
            return r;
        }
    }
}