using System.Collections.Generic;
using UnityEngine;
namespace Tames
{


    /// <summary>
    /// the class for handling all physical movements of <see cref="TameObject"/>s. A valid handles object should include a mechanism with a rotating or/and a sliding keys. Hence it should be possible to construct the following keys (see <see cref="HandleKey"/>) from its parent game object: one <see cref="mover"/>, for sliding: <see cref="start"/> and <see cref="end"/>, and for rotation: <see cref="pivot"/> and <see cref="start"/>. 
    /// </summary>
    public class TameHandles
    {
        /// <summary>
        /// not currently in use. See <see cref="ChildMovement"/>
        /// </summary>
        internal class TameLinked
        {
            public GameObject gameObject;
            public float initial;
            public static void Sort(TameLinked[] children)
            {
                TameLinked tcm;
                for (int i = 0; i < children.Length - 1; i++)
                    for (int j = i + 1; j < children.Length; j++)
                        if (children[i].initial > children[j].initial)
                        {
                            tcm = children[i];
                            children[i] = children[j];
                            children[j] = tcm;
                        }
            }
            public static GameObject[] ToGArray(TameLinked[] tls)
            {
                GameObject[] r = new GameObject[tls.Length];
                for (int i = 0; i < tls.Length; i++)
                    r[i] = tls[i].gameObject;
                return r;
            }
        }
        /// <summary>
        /// the moving objects of a <see cref="TameObject"/>. It is the same is its mechanism
        /// </summary>
        public GameObject mover;
        /// <summary>
        /// not currently in use.  See <see cref="ChildMovement"/>
        /// </summary>
        private TameLinked[] child;
        // slide
        /// <summary>
        /// the sliding vector
        /// </summary>
        public Vector3 vector;
        // rotate
        /// <summary>
        /// the start of rotation (local to the <see cref="mover"/>'s parent). It moves by sliding.
        /// </summary>
        public Vector3 start = Vector3.negativeInfinity;
        /// <summary>
        /// the end point of rotation (local to the <see cref="mover"/>'s parent). It moves by sliding.
        /// </summary>
        public Vector3 end = Vector3.negativeInfinity;
        /// <summary>
        /// the middle point of rotation (local to the <see cref="mover"/>'s parent). Only used to determine the <see cref="end"/> for reflex angles
        /// </summary>
        public Vector3 middle = Vector3.negativeInfinity;
        /// <summary>
        /// the pivot of rotation (local to the <see cref="mover"/>'s parent). It moves by sliding.
        /// </summary>
        public Vector3 pivot = Vector3.negativeInfinity;
        /// <summary>
        /// the point used for determining the <see cref="axis"/> of rotation (hinge - <see cref="pivot"/>) The hinge is local to the <see cref="mover"/>'s parent. If this point is present in the 3D model the <see cref="RotationType"/> is deemed as <see cref="MovingTypes.Facer"/> instead of <see cref="MovingTypes.Rotator"/> 
        /// </summary>
        public Vector3 hinge = Vector3.negativeInfinity;
        /// <summary>
        /// this point is used to determing the local rotation or facing logic of the <see cref="mover"/> (see <see cref="FacingLogic"/>). If this point is absent from the 3D model (see <see cref="HandleKey"/>), the logic would be Free. If it's equal to the <see cref="pivot"/> the logic would be Fixed; otherwise the logic would be Axis and its value would be recalculated by subtracting <see cref="pivot"/> from it. The resulted axis will remain constant.  
        /// </summary>
        public Vector3 up = Vector3.negativeInfinity;
        /// <summary>
        /// the rotation axis (local to the <see cref="mover"/>'s parent). This is calculated as <see cref="hinge"/> - <see cref="pivot"/>
        /// </summary>
        public Vector3 axis = Vector3.negativeInfinity;
        public Vector3 face = Vector3.negativeInfinity;
        /// <summary>
        /// the span of rotation in degrees (0 if full circle span).
        /// </summary>
        public float span = 0;
        /// <summary>
        /// the rotation type (Error if not a rotator). The possibility of rotation is determined by the existence of both <see cref="pivot"/> and <see cref="start"/> in the 3D model (see <see cref="HandleKey"/>)
        /// </summary>
        public MovingTypes RotationType { get { return rotType; } }
        private MovingTypes rotType = MovingTypes.Error;
        /// <summary>
        /// not used.
        /// </summary>
        public TamePath path = null;
        public bool isSlider = false;
        /// <summary>
        /// the facing logic of the local rotation of the <see cref="mover"/>. See <see cref="up"/>.
        /// </summary>
        public FacingLogic facing = FacingLogic.Free;
        /// <summary>
        /// not used
        /// </summary>
//public bool closed = false;
        public bool facesToward = false;
        public LinkedKeys linkedType = LinkedKeys.None;
        public float linkedOffset = 0;
        public bool cycleSet = false;
        public ContinuityMode cycleType = ContinuityMode.Stop;
        public byte trackBasis = TrackBasis.Error;
        public float duration = -1;
        public GameObject linker;
        public float linkedScale = 1;
        Transform[] transforms; //    public bool walk = false;
        public GameObject childrenParent = null;
        public const string KeyArea = "_area_";
        public const string KeyAreaBox = "_box";
        public const string KeyAreaCube = "_cub";
        public const string KeyAreaSphere = "_sph";
        public const string KeyAreaSwitch = "_swi";
        public const string KeyAreaCylinder = "_cyl";
        public const string KeyWalk = "_walk";
        public const string KeyMarker = "_mark";

        public const string KeyLinker = "_link";
        private const string KeyFrom = "_from";
        private const string KeyTo = "_to";
        private const string KeyPivot = "_pivot";
        private const string KeyAxis = "_axis";
        private const string KeyStart = "_start";
        private const string KeyEnd = "_end";
        private const string KeyMiddle = "_mid";
        private const string KeyPath = "_path";
        private const string KeyUp = "_up";
        private const string KeyMover = "_mov";
        private const string KeyTracker = "_fol";
        private const string KeyHeadTracker = "_head";
        private static string[] All = new string[] { KeyFrom, KeyTo, KeyPivot, KeyAxis, KeyStart, KeyEnd, KeyMiddle, KeyPath, KeyUp, KeyAreaBox, KeyAreaCube, KeyAreaCylinder, KeyAreaSphere };
        /// <summary>
        /// checks whether the name matches criteria for being a handle object. Handle objects are objects with certain naming patern in the 3D model (as the immediate children of a potential interactive element. For mechanical keys, objects with names starting with keys will be considered as handles. Please note that while all objects with the keys are considered as handles, only the last one of each is included. The keys and their corresponding handle point are listed below: 
        /// _from : <see cref="start"/>
        /// _to : <see cref="end"/>
        /// _pivot : <see cref="pivot"/>
        /// _start : <see cref="start"/>
        /// _end : <see cref="end"/>
        /// _mid : <see cref="middle"/>
        /// _axis : <see cref="hinge"/>, the actual <see cref="axis"/> is calculate by hinge - <see cref="pivot"/>
        /// _up : <see cref="up"/>, this is the initial up point, the vector up is calculate by up - <see cref="pivot"/> (see <see cref="up"/>).
        /// 
        /// For interactors, the keys are at the end of their name in the pattern -int-[update,geometry,space] in which the properties in the brackets are defined by single characters listed below
        /// Update (<see cref="InteractionUpdate"/>): f: Fixed, p: Parent, m: Mover, o: Object
        /// Geometry (<see cref="InteractionGeometry"/>): b: Box, c: Cylinder, s: Sphere
        /// Space (<see cref="InteractionMode"/>): e: Inside, x: Outside, i: InOut, o: OutIn, g: Grip 
        /// </summary>
        /// <param name="name">the object's name</param>
        /// <returns>if the name contains handle keys</returns>
        public static bool HandleKey(string name)
        {
            for (int i = 0; i < All.Length; i++)
                if (name.StartsWith(All[i])) return true;
            return false;
        }
        /// <summary>
        /// creates a <see cref="TameHandles"/> for a game object (the handle key object should be immediate children of the game object, see <see cref="HandleKey"/>).
        /// </summary>
        /// <param name="g">the parent game object</param>
        /// <returns>returns a new handles object or null if a valid handles cannot be made (for validity see <see cref="TameHandles"/>)</returns>
        public static Transform[] ValidObject(GameObject g, out int followMode, out int type)
        {
            Markers.MarkerObject om = g.GetComponent<Markers.MarkerObject>();
            if (om != null) om.Set();
            Transform m = GetTransform(g.transform, om, KeyMover, 7);
            followMode = 0;
            if (m == null)
            {
                m = GetTransform(g.transform, om, KeyTracker, 6);
                if (m != null) followMode = 1;
                else
                {
                    m = GetTransform(g.transform, om, KeyHeadTracker, 9);
                    if (m != null) followMode = 2;
                }
            }
            type = 0;
            Transform[] ts = null;
            if (m != null)
            {
                ts = new Transform[8];
                ts[7] = m;
                // Debug.Log("rotx " + g.name + " has mover");
                if ((ts[0] = GetTransform(g.transform, om, KeyStart, 0)) == null) ts[0] = Utils.FindStartsWith(g.transform, KeyFrom);
                if ((ts[1] = GetTransform(g.transform, om, KeyEnd, 1)) == null) ts[1] = Utils.FindStartsWith(g.transform, KeyTo);
                ts[2] = GetTransform(g.transform, om, KeyPath, 8);
                ts[3] = GetTransform(g.transform, om, KeyPivot, 4);
                ts[4] = GetTransform(g.transform, om, KeyAxis, 3);
                ts[5] = GetTransform(g.transform, om, KeyMiddle, 2);
                ts[6] = GetTransform(g.transform, om, KeyUp, 5);
                type = (ts[0] != null ? 1 : 0) + (ts[1] != null ? 2 : 0) + (ts[2] != null ? 4 : 0) + (ts[3] != null ? 8 : 0) + (ts[4] != null ? 16 : 0);
                if (type == 0)
                    return null;
            }
            return ts;
        }
        public Vector3 Localize(Transform t)
        {
            if (t.parent == mover.transform.parent)
                return t.localPosition;
            else
                return mover.transform.parent.InverseTransformPoint(t.position);
        }
        public static TameHandles GetHandles(GameObject g)
        {
            int p, q;
            TameHandles r = null;
    //        if (g.name == "path") Debug.Log("path ");
            if (HandleKey(g.name.ToLower()))
                return null;
            //      Debug.Log(g.name + " is not handle");
            Transform[] ts = ValidObject(g, out int followMode, out int type);
            if (type != 0)
            {
                Transform m = ts[7];
                if (ts[3] == null)
                {
                    if ((type & 3) > 0)
                    {
                        //           Debug.Log("has slide");
                        r = new TameHandles() { mover = m.gameObject };
                        r.start = r.Localize(ts[0]);
                        r.end = r.Localize(ts[1]);
                        //       Debug.Log("from " + r.from.ToString("0.00"));
                        if (ts[2] != null)
                        {
                            r.path = new TameSlider(ts[2].gameObject, m.gameObject, r.start, r.end);
                            if (!r.path.valid) r.path = null;
                            r.isSlider = true;
                        }
                        else
                            r.path = new TameSlider(m, r.start, r.end);
                    }
                }
                else
                {
                    if ((type & 9) > 0)
                    {
                        r = new TameHandles() { mover = m.gameObject };
                        r.pivot = r.Localize(ts[3]);
                        r.start = r.Localize(ts[0]);
                        if (ts[5] != null)
                            r.middle = r.Localize(ts[5]);
                        if (ts[1] != null)
                            r.end = r.Localize(ts[1]);
                        if (ts[4] != null)
                            r.hinge = r.Localize(ts[4]);
                        r.SetTransform();
                    }
                }
                if ((r != null) && (ts[6] != null))
                    r.up = r.Localize(ts[6]);
                r.SetUp();
                r.path.facing = r.facing;
                if (r != null)
                {
                    if (followMode == 1)
                        r.trackBasis = TrackBasis.Mover;
                    if (followMode == 2)
                        r.trackBasis = TrackBasis.Head;
                    //        Debug.Log("transform " + g.name + " " + r.trackBasis);
                }
                r.transforms = ts;
            }
            return r;
        }
        private static Transform GetTransform(Transform t, Markers.MarkerObject om, string key, int index)
        {
            Transform tm = null;
            if (om != null) tm = om.GetTransform(index);
            if (tm == null) tm = Utils.FindStartsWith(t, key);
            return tm;

        }
        public void CalculateHandles(float initial)
        {
            //   SetTransform();
            SetInitial(initial);
        }
        public void Recreate()
        {
            SetTransform();
            SetInitial(0);
        }
        public static TameHandles Duplicate(TameHandles th, GameObject mover)
        {
            TameHandles r = new TameHandles();
            r.mover = mover;
            r.pivot = th.pivot;
            r.start = th.start;
            r.middle = th.middle;
            r.end = th.end;
            r.hinge = th.hinge;
            r.start = th.start;
            r.end = th.end;
            r.up = th.up;
            r.path = th.path;
            r.Recreate();
            return r;
        }
        /// <summary>
        /// sets the initial transform (<see cref="position"/> and <see cref="rotation"/>) of the <see cref="mover"/>.
        /// </summary>
        public void SetTransform()
        {
            if ((pivot.x != float.NegativeInfinity) && (start.x != float.NegativeInfinity))
            {
                if (hinge.x != float.NegativeInfinity)
                {
                    TameOrbit orbit;
                    path = orbit = new TameOrbit()
                    {
                        parent = mover.transform.parent,
                        self = mover.transform.parent,
                        axis = hinge - pivot,
                        pivot = pivot,
                        mover = mover.transform
                    };
                    orbit.start = Utils.On(start, pivot, orbit.axis);
                    facing = FacingLogic.Free;
                    if (end.x == float.NegativeInfinity)
                        span = 360;
                    else
                    {
                        span = Utils.Angle(end, pivot, start, axis = hinge - pivot, true);
                        if (middle.x != float.NegativeInfinity)
                        {
                            float ma = Utils.Angle(middle, pivot, start, axis, true);
                            if ((span < 0) && (ma > 0))
                                span = 360 + span;
                            if ((span > 0) && (ma < 0))
                                span -= 360;
                        }
                    }
                    orbit.span = span;
                    //     Debug.Log("sapn " + mover.transform.parent.name + " " + span);
                    //       Debug.Log("pre v  " + mover.transform.parent.name + " " + (path == null));
                }

                else
                {
                    TameFreeRotator tfr;
                    path = tfr = new TameFreeRotator()
                    {
                        parent = mover.transform.parent,
                        self = mover.transform.parent,
                        pivot = pivot,
                        mover = mover.transform,
                        start = start
                    };
                    facing = FacingLogic.Free;
                    if (end.x == float.NegativeInfinity)
                        span = 90;
                    else
                    {
                        span = Vector3.Angle(end - pivot, start - pivot);
                        if (span > 90)
                            span = 90;
                    }
           //         Debug.Log("rot span " + span);
                    tfr.span = span;
                }
            }
        }
        public void SetMover()
        {
            if (linkedType == LinkedKeys.None)
                path.AssignMovers(new GameObject[] { mover }, true);
        }
        /// <summary>
        /// sets the basic properties of sliding (<see cref="start"/>, <see cref="end"/>, <see cref="vector"/>, and <see cref="DoesSlide"/>).
        /// </summary>
        public void SetInitial(float initial)
        {
            path.SetInitial(0, initial);
        }
        /// <summary>
        /// sets the basic properties of rotation (<see cref="railToPivot"/>, <see cref="RotationType"/>, <see cref="axis"/>, <see cref="span"/>, <see cref="up"/>).
        /// </summary>
        private void SetUp()
        {
            if (up.x != float.NegativeInfinity)
            {
                if (pivot.x == float.NegativeInfinity)
                    up -= start;
                else
                    up -= pivot;
                if (up.magnitude > 0.01f)
                    facing = FacingLogic.Axis;
                else
                    facing = FacingLogic.Fixed;
            }
            else
                facing = FacingLogic.Free;
        }

        public void AlignQueued(ManifestBase tmb)
        {
            AlignQueued(tmb.queueStart, tmb.queueCount, tmb.queueInterval, tmb.queueUV);
        }
        public void AlignQueued(float start, int count, float interval, int uv)
        {
            if (path.freeRotator) return;
            float m = start;
            int n = count > 0 ? count : (int)((1 - m) / interval) + 1;
            float d = count <= 0 ? interval : (1 - m) / (count - 1);
            GameObject go;
            child = new TameLinked[n];
            linkedType = LinkedKeys.Cycle;
            linkedOffset = d;
            linkedScale = 1;
            path.bases = new Transform[n];
            path.attached = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                go = path.Clone(i, m);
                if (uv >= 0)
                    Utils.RandomizeUV(go, i, uv);
                child[i] = new TameLinked() { initial = m };
                m += d;
                if (m > 1) m = 1;

            }
            mover.SetActive(false);
        }
        /// <summary>
        /// links a number of objects to the handle of an element
        /// </summary>
        /// <param name="lk">the type of linking</param>
        /// <param name="linkage">not used</param>
        /// <param name="list"></param>
        public void AlignLinked(LinkedKeys lk, GameObject linkage, List<TameGameObject> list)
        {
            if (path.freeRotator) return;
            Quaternion[] qu;
            Quaternion[] quat = new Quaternion[list.Count];
            linkedType = lk;

            Vector3 p, q;
            Quaternion r;
            float[] m = new float[list.Count];
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            if (list.Count > 1)
            {
                for (int i = 0; i < list.Count; i++)
                    m[i] = path.GetM(list[i].transform.position);

                for (int i = 0; i < list.Count; i++)
                {
                    if (m[i] < min) min = m[i];
                    if (m[i] > max) max = m[i];
                }
                if (linkedType == LinkedKeys.Cycle) max += linkedOffset;
                linkedScale = max - min;
                // for (int i = 0; i < list.Count; i++)
                //   m[i] = (m[i] - min) / (max - min);
                child = new TameLinked[list.Count];
                for (int i = 0; i < list.Count; i++)
                    child[i] = new TameLinked() { gameObject = list[i].gameObject, initial = m[i] };
                TameLinked.Sort(child);
                path.AssignMovers(TameLinked.ToGArray(child));
            }
            else if (list.Count == 1)
            {
                linkedScale = 1;
                child = new TameLinked[] { new TameLinked() { gameObject = list[0].gameObject, initial = 0 } };
                path.AssignMovers(new GameObject[] { list[0].gameObject });
            }
        }
        /// <summary>
        /// slides the <see cref="mover"/> from its current to next porgress value. 
        /// </summary>
        /// <param name="next">next progress value</param>
        /// <param name="current">current progress value</param>
        /// <returns>return the new progress value</returns>
        public float Move(float next, float current)
        {
            float m = current;
            if (path.freeRotator) return m;
            m = next;
            switch (linkedType)
            {
                case LinkedKeys.None: path.Move(0, m); break;
                case LinkedKeys.Progress: MoveProgress(m, true); break;
                case LinkedKeys.Local: MoveProgress(m, false); break;
                case LinkedKeys.Cycle: MoveCycle(m, false); break;
                case LinkedKeys.Stack: MoveStacked(m, true); break;
            }
            //   path.MoveLinked(m);
            return m;
        }
        /// <summary>
        /// slides the mover(s) along the sliding path based on a point, limited by travelling speed 
        /// </summary>
        /// <param name="pGlobal"></param>
        /// <param name="current"></param>
        /// <param name="speed"></param>
        /// <param name="dT"></param>
        /// <returns></returns>
        public float Move(Vector3 pGlobal, float current, float speed, float dT)
        {
            if (speed < 0) speed = 1;
            float m = current;
            switch (linkedType)
            {
                case LinkedKeys.None: m = MoveSelf(pGlobal, current, speed, dT); break;
                case LinkedKeys.Progress:
                case LinkedKeys.Stack: m = MoveLinked(pGlobal, current, speed, dT, true); break;
                case LinkedKeys.Local: MoveLinked(pGlobal, current, speed, dT, false); break;
            }
            //     path.MoveLinked(m);
            return m;
        }

        private float MoveLinked(Vector3 pGlobal, float current, float speed, float time, bool relative)
        {
            float m = path.GetM(pGlobal);
            if (Mathf.Abs(m - current) > speed * time)
            {
                if (m > current) m = current + speed * time; else m = current - speed * time;
            }
            switch (linkedType)
            {
                case LinkedKeys.Stack: MoveStacked(m, relative); break;
                case LinkedKeys.Local: MoveLocal(m, relative); break;
                case LinkedKeys.Cycle: MoveCycle(m, relative); break;
                case LinkedKeys.Progress: MoveProgress(m, relative); break;
            }
            return m;
        }

        private void MoveLocal(float m, bool relative)
        {
            Vector3 p;
            TameOrbit q;
            for (int i = 0; i < child.Length; i++)
                path.Move(i, child[i].initial + m);
        }
        private void MoveCycle(float m, bool relative)
        {
            float mi = 0;
            for (int i = 0; i < child.Length; i++)
            {
                mi = (m + child[i].initial) % 1.0001f;
                path.Move(i, mi);
            }
        }
        private void MoveProgress(float m, bool relative)
        {
            float mi;
            for (int i = 0; i < child.Length; i++)
            {
                if (relative)
                {
                    mi = (1 - child[i].initial) * (1 - linkedOffset) * m;
                    path.Move(i, child[i].initial + mi * linkedScale);
                }
                else
                {
                    path.Move(i, child[i].initial + m);
                }
            }
        }

        private void MoveStacked(float m, bool relative)
        {
            Vector3 p;
            TameOrbit q;
            float width = linkedOffset / child.Length;
            float m0 = (1 - linkedOffset) * m;
            float mi;
            float mLast = m0;
            for (int i = 0; i < child.Length; i++)
            {
                if (child[i].initial <= mLast)
                {
                    mLast += width;
                    mi = mLast;
                    path.Move(i, mi);
                }
                else
                    path.Move(i, child[i].initial);
            }
        }
        /// <summary>
        /// slides the <see cref="mover"/> to the closest to a specified point, given the speed of the mover and the time passed 
        /// </summary>
        /// <param name="pGlobal">the specified point</param>
        /// <param name="current">current progress value</param>
        /// <param name="speed">the speed of moving, see <see cref="TameProgress.Speed"/></param>
        /// <param name="time">the delta time of the frame</param>
        /// <returns>the new progress value</returns>
        private float MoveSelf(Vector3 pGlobal, float current, float speed, float time)
        {
             float m = current;
            if (path.freeRotator)
            {
                TameFreeRotator tfr = (TameFreeRotator)path;                
                m = tfr.Move(pGlobal);
            }
            else
            {
                m = path.GetM(pGlobal);
                if (m < 0) m = 0;
                if (m > 1) m = 1;
                if (Mathf.Abs(m - current) > speed * time)
                {
                    if (m > current) m = current + speed * time; else m = current - speed * time;
                }
                path.Move(0, m);
            }
            return m;
        }
        public bool Interactive(GameObject g)
        {
            if (g == mover) return true;
            if (isSlider)
                if (g == ((TameSlider)path).gameObject) return true;
            foreach (Transform t in transforms)
                if (t != null)
                    if (t.gameObject == g) return true;
            if (g == childrenParent) return true;
            return false;

        }
        public TameHandles Clone(GameObject owner, GameObject mover)
        {
            TameHandles th = new TameHandles()
            {
                mover = mover,
                facing = facing,
                linkedType = linkedType,
                linkedOffset = linkedOffset,
                linkedScale = linkedScale,
                isSlider = isSlider,
            };
            th.path = path.Clone(owner, mover, linkedType);
            th.mover.transform.parent = owner.transform;
            th.mover.transform.localPosition = mover.transform.localPosition;
            th.mover.transform.localRotation = mover.transform.localRotation;
            th.child = new TameLinked[child.Length];
            for (int i = 0; i < child.Length; i++)
                th.child[i] = new TameLinked() { gameObject = path.attached[i].gameObject, initial = child[i].initial };
            return th;
        }
    }
}