using System.Collections.Generic;
using UnityEngine;
namespace Tames
{
    /// <summary>
    /// not currently in use. See <see cref="ChildMovement"/>
    /// </summary>
    public class TameLinked
    {
        public GameObject gameObject;
        public float initial, progress;
        public Quaternion rotation, pathRotation;
        public Vector3 position;
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
    }

    /// <summary>
    /// the class for handling all physical movements of <see cref="TameObject"/>s. A valid handles object should include a mechanism with a rotating or/and a sliding keys. Hence it should be possible to construct the following keys (see <see cref="HandleKey"/>) from its parent game object: one <see cref="mover"/>, for sliding: <see cref="from"/> and <see cref="to"/>, and for rotation: <see cref="pivot"/> and <see cref="start"/>. 
    /// </summary>
    public class TameHandles
    {
        /// <summary>
        /// the moving objects of a <see cref="TameObject"/>. It is the same is its mechanism
        /// </summary>
        public GameObject mover;
        /// <summary>
        /// not currently in use.  See <see cref="ChildMovement"/>
        /// </summary>
        public TameLinked[] children;
        /// <summary>
        /// this initial local position of the <see cref="mover"/>
        /// </summary>
        private Vector3 position;
        /// <summary>
        /// if the elements has rotation, this contains the displaceement from the <see cref="pivot"/> to the <see cref="mover"/>'s local position. The vector is updated after each rotation 
        /// </summary>
        private Vector3 pivotToMover = Vector3.zero;
        /// <summary>
        /// the displacement vector from the <see cref="position"/> to the <see cref="pivot"/>. This remains constant. 
        /// </summary>
    //    private Vector3 railToPivot = Vector3.zero;
        /// <summary>
        /// the displacement vector from the <see cref="position"/> to the <see cref="pivot"/> at the start. This remains constant. 
        /// </summary>
        private Vector3 startToInitial = Vector3.zero;
        /// <summary>
        /// the displaceent vector from the <see cref="start"/> to the <see cref="pivot"/> at the start. This remains constant.
        /// </summary>
        private Vector3 pivotToStart = Vector3.zero;
        /// <summary>
        /// the last rotation of <see cref="start"/>
        /// </summary>
        private Vector3 toRotatedStart = Vector3.zero;
        /// <summary>
        /// last position on rail
        /// </summary>
        private Vector3 lastPosition;
        /// <summary>
        /// displacement vector from <see cref="from"/> to the mover's initial <see cref="position"/>
        /// </summary>
        private Vector3 railToMover;
        /// <summary>
        /// displacement vector from <see cref="start"/> to the mover's initial <see cref="position"/>
        /// </summary>
        private Vector3 startToMover;
        /// <summary>
        /// last position on orbit 
        /// </summary>
        private Vector3 lastRotation;
        /// <summary>
        /// the initial rotation of the <see cref="mover"/>
        /// </summary>
        private Quaternion rotation;
        /// <summary>
        /// not in use. See <see cref="ChildMovement"/>
        /// </summary>
        public ChildMovement childRotation = ChildMovement.Parent;
        /// <summary>
        /// not in use. See <see cref="ChildMovement"/>
        /// </summary>
        public ChildMovement childSlide = ChildMovement.Parent;
        /// <summary>
        /// not in use. See <see cref="ChildMovement"/>
        /// </summary>
        public float minimumAngle = 1;
        /// <summary>
        /// not in use. See <see cref="ChildMovement"/>
        /// </summary>
        public float minimumPortion = 1;
        /// <summary>
        /// if the handle incldes sliding
        /// </summary>
        public bool DoesSlide { get { return doesSlide; } }
        private bool doesSlide = false;
        // slide
        /// <summary>
        /// the origin of sliding (local to the <see cref="mover"/>'s parent). Remains constant.
        /// </summary>
        public Vector3 from = Vector3.negativeInfinity;
        /// <summary>
        /// the destination of sliding (local to the <see cref="mover"/>'s parent). Remains constant.
        /// </summary>
        public Vector3 to = Vector3.negativeInfinity;
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
        public float Span { get { return span; } }
        private float span = 0;
        /// <summary>
        /// the rotation type (Error if not a rotator). The possibility of rotation is determined by the existence of both <see cref="pivot"/> and <see cref="start"/> in the 3D model (see <see cref="HandleKey"/>)
        /// </summary>
        public MovingTypes RotationType { get { return rotType; } }
        private MovingTypes rotType = MovingTypes.Error;
        /// <summary>
        /// not used.
        /// </summary>
        public TamePath path = null;
        /// <summary>
        /// the facing logic of the local rotation of the <see cref="mover"/>. See <see cref="up"/>.
        /// </summary>
        public FacingLogic facing = FacingLogic.Free;
        /// <summary>
        /// not used
        /// </summary>
        public bool closed = false;
        public bool facesToward = false;
        public LinkedKeys linkedType = LinkedKeys.None;
        public float linkedOffset = 0;
        public bool cycleSet = false;
        public CycleTypes cycleType = CycleTypes.Stop;
        public byte trackBasis = TrackBasis.Error;
        public float duration = -1;
        //        public List<GameObject> linkedObjects = new List<GameObject>();
        public GameObject linker;
        public float linkedScale = 1;
        public bool walk = false;
        public const string KeyArea = "_area_";
        public const string KeyAreaBox = "_box";
        public const string KeyAreaCube = "_cub";
        public const string KeyAreaSphere = "_sph";
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
        private const string KeyFace = "_face";
        private const string KeyProgCycle = "_cyc";
        private const string KeyProgBounce = "_rev";
        private const string KeyProgStop = "_stop";
        private const string All = KeyFrom + "/" + KeyTo + "/" + KeyPivot + "/" + KeyAxis + "/" + KeyStart + "/" + KeyEnd + "/" + KeyMiddle + "/" + KeyPath + "/" + KeyUp + "/";
        /// <summary>
        /// checks whether the name matches criteria for being a handle object. Handle objects are objects with certain naming patern in the 3D model (as the immediate children of a potential interactive element. For mechanical keys, objects with names starting with keys will be considered as handles. Please note that while all objects with the keys are considered as handles, only the last one of each is included. The keys and their corresponding handle point are listed below: 
        /// _from : <see cref="from"/>
        /// _to : <see cref="to"/>
        /// _pivot : <see cref="pivot"/>
        /// _start : <see cref="start"/>
        /// _end : <see cref="end"/>
        /// _mid : <see cref="middle"/>
        /// _axis : <see cref="hinge"/>, the actual <see cref="axis"/> is calculate by hinge - <see cref="pivot"/>
        /// _up : <see cref="up"/>, this is the initial up point, the vector up is calculate by up - <see cref="pivot"/> (see <see cref="up"/>).
        /// 
        /// For interactors, the keys are at the end of their name in the pattern -int-[update][geometry][space] in which the properties in the brackets are defined by single characters listed below
        /// Update (<see cref="InteractionUpdate"/>): f: Fixed, p: Parent, m: Mover, o: Object
        /// Geometry (<see cref="InteractionGeometry"/>): b: Box, c: Cylinder, s: Sphere
        /// Space (<see cref="InteractionMode"/>): e: Inside, x: Outside, i: InOut, o: OutIn, g: Grip 
        /// </summary>
        /// <param name="name">the object's name</param>
        /// <returns>if the name contains handle keys</returns>
        public static bool HandleKey(string name)
        {
            return (All.IndexOf(name + "/") >= 0) || (name.IndexOf(KeyArea) == 0);
        }
        /// <summary>
        /// creates a <see cref="TameHandles"/> for a game object (the handle key object should be immediate children of the game object, see <see cref="HandleKey"/>).
        /// </summary>
        /// <param name="g">the parent game object</param>
        /// <returns>returns a new handles object or null if a valid handles cannot be made (for validity see <see cref="TameHandles"/>)</returns>
        public static TameHandles GetHandles(GameObject g)
        {
            int p, q;
            TameHandles r = null;
            if (HandleKey(g.name))
                return null;
            //      Debug.Log(g.name + " is not handle");
            bool followMode = false;
            MarkerObject om = g.GetComponent<MarkerObject>();
            Transform m =GetTransform(g.transform,om, KeyMover,7);
            if (m == null)
            {
                m = GetTransform(g.transform, om, KeyTracker, 6);
                if (m != null) followMode = true;
            }
            Transform[] ts = new Transform[7];
            if (m != null)
            {
                // Debug.Log("rotx " + g.name + " has mover");
                if ((ts[0] = GetTransform(g.transform, om, KeyStart, 0)) == null) ts[0] = Utils.FindStartsWith(g.transform, KeyFrom);
                if ((ts[1] = GetTransform(g.transform, om, KeyEnd, 1)) == null) ts[1] = Utils.FindStartsWith(g.transform, KeyTo);
                ts[2] = GetTransform(g.transform, om, KeyPath, 8);
                ts[3] = GetTransform(g.transform, om, KeyPivot, 4);
                ts[4] = GetTransform(g.transform, om, KeyAxis, 3);
                ts[5] = GetTransform(g.transform, om, KeyMiddle, 2);
                ts[6] = GetTransform(g.transform, om, KeyUp, 5);
                int type = (ts[0] != null ? 1 : 0) + (ts[1] != null ? 2 : 0) + (ts[2] != null ? 4 : 0) + (ts[3] != null ? 8 : 0) + (ts[4] != null ? 16 : 0);
                if (ts[3] == null)
                {
                    if ((type & 3) > 0)
                    {
                        //           Debug.Log("has slide");
                        r = new TameHandles() { from = ts[0].localPosition, to = ts[1].localPosition, mover = m.gameObject };
                        //       Debug.Log("from " + r.from.ToString("0.00"));
                        if (ts[2] != null)
                        {
                            r.path = new TamePath(ts[2].gameObject, m.gameObject, r.from, r.to) { handle = r };
                            if (!r.path.valid) r.path = null;
                            if (ts[2].name.IndexOf("x") < 0)
                                ts[2].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if ((type & 9) > 0)
                    {
                        r = new TameHandles() { mover = m.gameObject };
                        r.pivot = ts[3].localPosition;
                        r.start = ts[0].localPosition;
                        if (ts[5] != null)
                            r.middle = ts[5].localPosition;
                        if (ts[1] != null)
                            r.end = ts[1].localPosition;
                        if (ts[4] != null)
                            r.hinge = ts[4].localPosition;
                    }
                }
                if ((r != null) && (ts[6] != null))
                    r.up = ts[6].localPosition;
                if (r != null)
                {
                    int k = -1;
                    if ((m = Utils.FindStartsWith(g.transform, KeyProgCycle)) != null) k = 2;
                    else if ((m = Utils.FindStartsWith(g.transform, KeyProgBounce)) != null) k = 1;
                    else if ((m = Utils.FindStartsWith(g.transform, KeyProgStop)) != null) k = 0;
                    r.cycleType = k switch
                    {
                        1 => CycleTypes.Reverse,
                        2 => CycleTypes.Cycle,
                        _ => CycleTypes.Stop
                    };
                    if (k >= 0)
                    {
                        r.cycleSet = true;
                        string dur;
                        p = m.name.IndexOf("_", 1);
                        if (p >= 0)
                        {
                            q = m.name.IndexOf("_", p + 1);
                            dur = q >= 0 ? m.name.Substring(p + 1, q - p - 1) : m.name.Substring(p + 1);
                            if (Utils.SafeParse(dur, out float f))
                                r.duration = f;
                        }
                    }
                    if (followMode)
                    {
                        r.trackBasis = TrackBasis.Mover;
                    }
                }
            }

            return r;
        }
        private static Transform GetTransform(Transform t, MarkerObject om, string key, int index)
        {
            if (om == null)
                return Utils.FindStartsWith(t, key);
            else
                return om.all[index].transform;

        }
        public void CalculateHandles(float initial)
        {
            SetTransform();
            SetUp();
            if (doesSlide)
                SetSlide(initial);
            else if (rotType != MovingTypes.Error)
                SetRotate(initial);
        }
        public void Recreate()
        {
            SetTransform();
            if (doesSlide)
                SetSlide(0);
            if (rotType != MovingTypes.Error)
                SetRotate(0);

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
            r.from = th.from;
            r.to = th.to;
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
            position = mover.transform.localPosition;
            rotation = mover.transform.localRotation;
            if ((from.x != float.NegativeInfinity) && (to.x != float.NegativeInfinity))
                doesSlide = true;
            else if ((pivot.x != float.NegativeInfinity) && (start.x != float.NegativeInfinity))
            {
                if (hinge.x == float.NegativeInfinity)
                    rotType = MovingTypes.Facer;
                else
                    rotType = MovingTypes.Rotator;
            }
        }
        /// <summary>
        /// sets the basic properties of sliding (<see cref="from"/>, <see cref="to"/>, <see cref="vector"/>, and <see cref="DoesSlide"/>).
        /// </summary>
        public void SetSlide(float initial)
        {
            vector = to - from;
            doesSlide = true;

            Vector3 p;
            if (path == null)
            {
                p = initial * vector + from;
                railToMover = position - p;
            }
            else
            {
                // HERE   path.PandR(initial, facing, out p, out q);
                path.PR(initial, out p, out TameRotator q);
                railToMover = position - p;
            }
            //          Debug.Log("slide/ " + from.ToString("0.00") + " > " + to.ToString("0.00"));
        }
        /// <summary>
        /// sets the basic properties of rotation (<see cref="railToPivot"/>, <see cref="RotationType"/>, <see cref="axis"/>, <see cref="span"/>, <see cref="up"/>).
        /// </summary>
        public void SetRotate(float initial)
        {
            Vector3 ax;
            Vector3 e;
            float a;
            //          railToPivot = pivot - from;

            toRotatedStart = pivotToStart = start - pivot;
            startToInitial = position - start;
            startToMover = position - start;

            if (rotType == MovingTypes.Rotator)
            {
                //     rotating = RotatingLogic.End;
                axis = hinge - pivot;
                if (end.x != float.NegativeInfinity)
                {
                    e = Utils.On(end, pivot, axis);
                    span = Utils.FullAngle(e, pivot, start, axis);
                    if (span > 180) span -= 360f;
                    if (middle.x != float.PositiveInfinity)
                    {
                        e = Utils.On(middle, pivot, axis);
                        a = Utils.FullAngle(e, pivot, start, axis);
                        if (a > 180) a -= 360;
                        if ((a < 0) && (span > 0))
                            span = 360f - span;
                        else if ((a > 0) && (span < 0))
                            span = 360f - span;
                    }
                }
                else if (middle.x != float.NegativeInfinity)
                {
                    e = Utils.On(middle, pivot, axis);
                    span = Utils.FullAngle(e, pivot, start, axis);
                    //          Debug.Log("spank " + span);
                    if (span > 180)
                        span = (span - 360f) * 2;
                    else
                        span *= 2;
                }
                else span = 360f;

                Vector3 v1, v2;
                v1 = Utils.Rotate(start, pivot, axis, initial * span);
                v2 = Utils.Rotate(position - v1, Vector3.zero, axis, -initial * span);
                startToInitial = v2;
                Vector3 op = position;
                position = pivot + pivotToStart + startToInitial;
                Quaternion r = rotation;
                switch (facing)
                {
                    case FacingLogic.Free:
                        mover.transform.Rotate(axis, -initial * span);
                        rotation = mover.transform.localRotation;
                        mover.transform.localRotation = r;
                        break;
                    case FacingLogic.Axis:
                        v1 = Utils.On(op, Vector3.zero, up);
                        v2 = Utils.On(position, Vector3.zero, up);
                        float ang = Utils.SignedAngle(v2, Vector3.zero, v1, up);
                        mover.transform.Rotate(axis, ang);
                        rotation = mover.transform.localRotation;
                        mover.transform.localRotation = r;
                        break;
                }

            }
            else
            {
                if (end.x != float.NegativeInfinity)
                {
                    span = Vector3.Angle(start - pivot, end - pivot);
                    if (span > 90) span = 90;
                }
                else
                    span = 180;
                //           Debug.Log("rotx is " + span);
            }
        }
        private void SetUp()
        {
            if (up.x != float.NegativeInfinity)
            {
                if (doesSlide)
                    up -= from;
                else if (rotType != MovingTypes.Error)
                    up -= pivot;
                if (up.magnitude > 0.01f)
                    facing = FacingLogic.Axis;
                else
                    facing = FacingLogic.Fixed;
            }
            else
                facing = FacingLogic.Free;
     //       Debug.Log("up " + mover.transform.parent.name + " " + facing);
        }
        private float FindM(Vector3 p)
        {
            if (path == null)
                return Utils.M(from, p, vector);
            else
                // HERE  return path.PandR(p, facing, out Vector3 q);
                return path.PR(p, out Vector3 q);
        }

        public void AlignQueued(TameManifestBase tmb)
        {
            float m = tmb.queueStart;
            int n = tmb.queueCount > 0 ? tmb.queueCount : (int)((1 - m) / tmb.queueInterval) + 1;
            float d = tmb.queueCount <= 0 ? tmb.queueInterval : (1 - m) / (tmb.queueCount - 1);
            GameObject go;
            children = new TameLinked[n];
            linkedType = LinkedKeys.Cycle;
            linkedOffset = d;
            linkedScale = 1;

            for (int i = 0; i < n; i++)
            {
                PandRSelf(m, out Vector3 p, out TameRotator q);
                go = GameObject.Instantiate(mover);
                if (tmb.queueUV >= 0)
                    Utils.RandomizeUV(go, i, tmb.queueUV);
                go.transform.parent = mover.transform.parent;
                go.transform.localPosition = p;
                go.transform.localRotation = rotation;
                TameRotator.Rotate(go.transform, rotation, q);
                children[i] = new TameLinked() { gameObject = go, initial = m, progress = m, position = go.transform.localPosition, rotation = go.transform.localRotation };
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
            linker = linkage;
            Quaternion[] qu;
            Quaternion[] quat = new Quaternion[list.Count];
            linkedType = lk;
            //    TameLinked tcm;
            //   linkedObjects = list;
            if (list.Count > 1)
            {
                Vector3 p, q;
                float[] m = new float[list.Count];
                float min = float.PositiveInfinity, max = float.NegativeInfinity;
                if (doesSlide)
                {
                    for (int i = 0; i < list.Count; i++)
                        m[i] = FindM(list[i].transform.localPosition);

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (m[i] < min) min = m[i];
                        if (m[i] > max) max = m[i];
                    }
                    if (linkedType == LinkedKeys.Cycle) max += linkedOffset;
                    linkedScale = max - min;
                    // for (int i = 0; i < list.Count; i++)
                    //   m[i] = (m[i] - min) / (max - min);
                    children = new TameLinked[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        children[i] = new TameLinked() { gameObject = list[i].gameObject, initial = m[i], progress = m[i], position = list[i].transform.localPosition, rotation = list[i].transform.localRotation };
                    }
                    TameLinked.Sort(children);
                }
                else if (rotType == MovingTypes.Rotator)
                {
                    children = new TameLinked[list.Count];
                    if (lk == LinkedKeys.Local)
                        for (int i = 0; i < list.Count; i++)
                        {
                            children[i] = new TameLinked() { gameObject = list[i].gameObject, initial = 0, progress = 0, position = list[i].transform.localPosition, rotation = list[i].transform.localRotation };
                        }
                    else
                    {
                        p = linkage.transform.localPosition - pivotToMover;

                        for (int i = 0; i < list.Count; i++)
                        {
                            q = list[i].transform.localPosition - p;
                            q = Utils.On(q, p, axis);
                            m[i] = Utils.FullAngle(q, Vector3.zero, pivotToStart, axis);
                            if (span == 360f)
                                m[i] /= 360f;
                            else if (span < 0)
                            {
                                if ((m[i] < span / 2 + 180f))
                                    m[i] = -m[i] + span / 2;
                                else if (m[i] < 360f + span)
                                    m[i] = 360f - m[i] + span;
                                else
                                    m[i] = m[i] - 360f - span / 2;
                            }
                            else
                            {
                                if (m[i] < span)
                                    m[i] -= span / 2;
                                else if (m[i] > span / 2 + 180f)
                                    m[i] = m[i] - 360f - span / 2;
                                else
                                    m[i] -= span / 2;
                            }
                        }
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (m[i] < min) min = m[i];
                            if (m[i] > max) max = m[i];
                        }
                        if (linkedType == LinkedKeys.Cycle) max += linkedOffset;
                        linkedScale = (max - min) / Mathf.Abs(span);
                        for (int i = 0; i < list.Count - 1; i++)
                            m[i] /= max - min;
                        for (int i = 0; i < list.Count; i++)
                        {
                            children[i] = new TameLinked() { gameObject = list[i].gameObject, initial = m[i], progress = m[i], position = list[i].transform.localPosition, rotation = list[i].transform.localRotation };
                        }
                        TameLinked.Sort(children);
                    }
                }
            }
            else if (list.Count == 1)
                if (doesSlide || (rotType == MovingTypes.Rotator))
                {
                    linkedScale = 1;
                    children = new TameLinked[]
                    {
                        new TameLinked() { gameObject = list[0].gameObject, initial = 0, progress = 0, position = list[0].transform.localPosition, rotation = list[0].transform .localRotation }
                    };
                }
        }

        private void PandRSelf(float m, out Vector3 p, out TameRotator q)
        {
            if (path == null) { p = from + m * vector; q = null; }
            else
            {
                path.PR(m, out p, out q);
            }
        }
        private void PandR(int i, float m, out Vector3 p, out TameRotator q)
        {
            if (path == null) { p = from + m * vector; q = null; }
            else
            {
                path.PR(m, children[i].initial, out p, out q);
            }
        }
        private void SlideSelf(float m)
        {
            PandRSelf(m, out lastPosition, out TameRotator q);
            if (facing != FacingLogic.Fixed) TameRotator.Rotate(mover.transform, rotation, q);// mover.transform.localRotation = rot;
            Vector3 p2m = railToMover;
            mover.transform.localPosition = lastPosition + p2m;
            if (rotType != MovingTypes.Error)
            {
                start = pivot + pivotToStart;
            }

        }
        /// <summary>
        /// slides the <see cref="mover"/> from its current to next porgress value. 
        /// </summary>
        /// <param name="next">next progress value</param>
        /// <param name="current">current progress value</param>
        /// <returns>return the new progress value</returns>
        public float Slide(float next, float current)
        {
            float m = current;
            if (doesSlide)
            {
                m = next;
                switch (linkedType)
                {
                    case LinkedKeys.None: SlideSelf(m); break;
                    case LinkedKeys.Progress: SlideProgress(m, true); break;
                    case LinkedKeys.Local: SlideProgress(m, false); break;
                    case LinkedKeys.Cycle: SlideCycle(m, false); break;
                    case LinkedKeys.Stack: SlideStacked(m, true); break;
                }
            }
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
        public float Slide(Vector3 pGlobal, float current, float speed, float dT)
        {
            if (speed < 0) speed = 1;
            float m = current;
            if (doesSlide)
            {
                switch (linkedType)
                {
                    case LinkedKeys.None: m = SlideSelf(pGlobal, current, speed, dT); break;
                    case LinkedKeys.Progress:
                    case LinkedKeys.Stack: m = SlideLinked(pGlobal, current, speed, dT, true); break;
                    case LinkedKeys.Local: SlideLinked(pGlobal, current, speed, dT, false); break;
                }
            }
     //       Debug.Log("mover: "+ mover.transform.parent.gameObject.name+" " + m);
      //      if (mover.transform.parent.gameObject.name == "longbase")                Debug.Log("mover: " + m);
            return m;
        }

        private float SlideLinked(Vector3 pGlobal, float current, float speed, float time, bool relative)
        {
            Vector3 p = mover.transform.parent.InverseTransformPoint(pGlobal);
            float m = current;
            if (doesSlide)
            {
                m = path.Progress(p);
                if (m < 0) m = 0;
                if (m > 1) m = 1;
                if (Mathf.Abs(m - current) > speed * time)
                {
                    if (m > current) m = current + speed * time; else m = current - speed * time;
                }
                switch (linkedType)
                {
                    case LinkedKeys.Stack: SlideStacked(m, relative); break;
                    case LinkedKeys.Local: SlideLocal(m, relative); break;
                    case LinkedKeys.Cycle: SlideCycle(m, relative); break;
                    case LinkedKeys.Progress: SlideProgress(m, relative); break;
                }
            }
            return m;
        }

        private void SlideLocal(float m, bool relative)
        {
            Vector3 p;
            TameRotator q;
            if (doesSlide)
                for (int i = 0; i < children.Length; i++)
                {
                    PandR(i, children[i].initial + m, out p, out q);
                    children[i].gameObject.transform.localPosition = p;
                    if (facing != FacingLogic.Fixed) TameRotator.Rotate(children[i].gameObject.transform, children[i].rotation, q);
                }
        }
        private void SlideCycle(float m, bool relative)
        {
            Vector3 p;
            TameRotator q;
            string s = "";
            float mi;
            if (doesSlide)
                for (int i = 0; i < children.Length; i++)
                {
                    mi = (m + children[i].initial) % 1.0001f;
                    PandR(i, mi, out p, out q);
                    s += mi + " ";
                    children[i].gameObject.transform.localPosition = p;
                    if (facing != FacingLogic.Fixed) TameRotator.Rotate(children[i].gameObject.transform, children[i].rotation, q);
                }
            //     Debug.Log("upo: " + mover.transform.parent.name + " " + facing); ;

        }
        private void SlideProgress(float m, bool relative)
        {
            if (doesSlide)
            {
                Vector3 p;
                TameRotator q;
                float mi;
                for (int i = 0; i < children.Length; i++)
                {
                    if (relative)
                    {
                        mi = (1 - children[i].initial) * (1 - linkedOffset) * m;
                        PandR(i, children[i].initial + mi * linkedScale, out p, out q);
                        children[i].gameObject.transform.localPosition = p;
                        if (facing != FacingLogic.Fixed) TameRotator.Rotate(children[i].gameObject.transform, children[i].rotation, q);
                    }
                    else
                    {
                        PandR(i, children[i].initial + m, out p, out q);
                        children[i].gameObject.transform.localPosition = p;
                        if (facing != FacingLogic.Fixed) TameRotator.Rotate(children[i].gameObject.transform, children[i].rotation, q);
                    }
                }
            }
        }
        private void SlideStacked(float m, bool relative)
        {
            if (doesSlide)
            {
                Vector3 p;
                TameRotator q;
                float width = linkedOffset / children.Length;
                float om = (1 - linkedOffset) * m;
                float mi;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].initial <= om + i * width)
                    {
                        mi = i * width + om;
                        PandR(i, children[0].initial + mi * linkedScale, out p, out q);
                        children[i].gameObject.transform.localPosition = p;
                        children[i].gameObject.transform.localPosition = p;
                        if (facing != FacingLogic.Fixed) TameRotator.Rotate(children[i].gameObject.transform, children[i].rotation, q);
                    }
                    else
                        children[i].gameObject.transform.localPosition = children[i].position;
                }

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
        private float SlideSelf(Vector3 pGlobal, float current, float speed, float time)
        {
            Vector3 p = mover.transform.parent.InverseTransformPoint(pGlobal);
            float m = current;
            if (doesSlide)
            {
                m = FindM(p);
                if (m < 0) m = 0;
                if (m > 1) m = 1;
                if (Mathf.Abs(m - current) > speed * time)
                {
                    if (m > current) m = current + speed * time; else m = current - speed * time;
                }
                SlideSelf(m);
            }
            return m;
        }
        /// <summary>
        /// checks if an angles is within the rotation span
        /// </summary>
        /// <param name="a">the angle</param>
        /// <returns></returns>
        bool InSpan(float a)
        {
            return span > 0 ? (a >= 0) && (a <= span) : ((a <= 0) && (a >= span));
        }
        void Face(float m)
        {
            Quaternion r;
            Vector3 v1, v2;
            switch (facing)
            {
                case FacingLogic.Free:
                    mover.transform.localRotation = Quaternion.identity;
                    mover.transform.Rotate(axis, m * span);
                    mover.transform.localRotation *= rotation;
                    break;
                case FacingLogic.Axis:
                    v1 = Utils.On(pivotToStart, Vector3.zero, up);
                    v2 = Utils.On(toRotatedStart, Vector3.zero, up);
                    float ang = Utils.SignedAngle(v2, Vector3.zero, v1, up);
                    mover.transform.localRotation = rotation;
                    mover.transform.Rotate(axis, ang);
                    break;
            }
        }
        void FaceLinked(int index, float m)
        {
            Quaternion r;
            Vector3 v1, v2;
            Vector3 axis;
            switch (facing)
            {
                case FacingLogic.Free:
                    children[index].gameObject.transform.localRotation = children[index].rotation;
                    axis = children[index].gameObject.transform.InverseTransformVector(mover.transform.TransformVector(this.axis));
                    children[index].gameObject.transform.Rotate(axis, m * span);
                    break;
                case FacingLogic.Axis:
                    v1 = Utils.On(children[index].position - linker.transform.localPosition, Vector3.zero, up);
                    v2 = Utils.On(children[index].gameObject.transform.localPosition - linker.transform.localPosition, Vector3.zero, up);
                    float ang = Utils.SignedAngle(v2, Vector3.zero, v1, up);
                    axis = children[index].gameObject.transform.InverseTransformVector(mover.transform.TransformVector(this.axis));
                    children[index].gameObject.transform.localRotation = rotation;
                    children[index].gameObject.transform.Rotate(axis, ang);
                    break;
            }
        }
        float CloserEnd(float angle)
        {
            float mid = span / 2 + 180;
            if (span < 0)
                return angle < mid ? 0f : 1f;
            else
                return angle > mid ? 0f : 1f;
        }
        /// <summary>
        /// rotates the <see cref="mover"/> from its current to next porgress value. This method is only called if the <see cref="RotationType"/> is Rotator not Facer
        /// </summary>
        /// <param name="next">next progress value</param>
        /// <param name="current">current progress value</param>
        /// <returns>return the new progress value</returns>
        public float Rotate(float next, float current)
        {
            float m = current;
            if (rotType == MovingTypes.Rotator)
            {
                m = next;
                if (m > 1) m = 1;
                if (m < 0) m = 0;
                switch (linkedType)
                {
                    case LinkedKeys.None:
                        startToMover = Utils.Rotate(startToInitial, Vector3.zero, axis, m * span);
                        toRotatedStart = Utils.Rotate(pivotToStart, Vector3.zero, axis, m * span);
                        mover.transform.localPosition = pivot + toRotatedStart + startToMover;
                        Face(m);
                        break;
                    case LinkedKeys.Local:
                        break;
                    case LinkedKeys.Progress:
                        RotateProgress(next);
                        break;
                    case LinkedKeys.Stack:
                        RotateStacked(next);
                        break;
                    case LinkedKeys.Cycle:
                        RotateCycle(next);
                        break;
                }
                if (linkedType != LinkedKeys.None)
                {
                    for (int i = 0; i < children.Length; i++)
                        FaceLinked(i, next);
                }
            }
            return m;
        }
        private void RotateStacked(float m)
        {
            if (doesSlide)
            {
                float width = linkedOffset / children.Length;
                float om = (1 - linkedOffset) * m;
                float mi, a;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].initial <= om + i * width)
                    {
                        mi = i * width + om;
                        a = mi * linkedScale * span;
                        children[i].gameObject.transform.localPosition = Utils.Rotate(children[i].position, children[0].position - pivotToMover, axis, a);
                    }
                    else
                        children[i].gameObject.transform.localPosition = children[i].position;
                }

            }
        }
        private void RotateCycle(float m)
        {
            float om, a;
            for (int i = 0; i < children.Length; i++)
            {
                om = (m + children[i].initial) % 1.001f;
                a = om * linkedScale * span;
                children[i].gameObject.transform.localPosition = Utils.Rotate(children[i].position, children[0].position - pivotToMover, axis, a);
                FaceLinked(i, om);
            }

        }
        private void RotateProgress(float m)
        {
            float om, a;
            for (int i = 0; i < children.Length; i++)
            {
                om = children[i].initial + (linkedOffset - 1) * m * children[i].initial;
                a = om * linkedScale * span;
                children[i].gameObject.transform.localPosition = Utils.Rotate(children[i].position, children[0].position - pivotToMover, axis, a);
                FaceLinked(i, om);
            }
        }
        private float RotateByLocalPoint(Vector3 p, float current, float time = 1, float speed = 1)
        {
            Quaternion r;
            Vector3 p1, v;
            float m = current, angle, b, c, delta;
            if (rotType == MovingTypes.Rotator)
            {
                p1 = Utils.On(p, pivot, axis);
                angle = Utils.FullAngle(p1, pivot, start, axis);
                if (span != 360f)
                {
                    if (span < 0) angle -= 360f;
                    if (InSpan(angle))
                        m = angle / span;
                    else
                        m = CloserEnd(angle);
                    delta = m - current;
                }
                else
                {
                    m = angle / span;
                    delta = Mathf.Abs(m - current) < 0.5f ? m - current : (current <= 0.5f ? m - current - 1 : 1 + m - current);
                }
                if (Mathf.Abs(delta) > speed * time) delta = delta > 0 ? speed * time : -speed * time;
                if (current + delta < 0) m = 1 + (current + delta);
                else if (current + delta > 1) m = current + delta - 1;
                else m = current + delta;
                Rotate(m, current);
            }
            else if (rotType == MovingTypes.Facer)
            {
                angle = Vector3.Angle(start - pivot, p - pivot);
                if (angle == 0)
                {
                    mover.transform.localPosition = position;
                    mover.transform.localRotation = rotation;
                    toRotatedStart = Vector3.zero;
                    startToMover = startToInitial;
                }
                else if (angle == 180)
                {
                    mover.transform.localPosition = position + (pivot - position) * 2;
                    mover.transform.localRotation = Quaternion.Inverse(rotation);
                    toRotatedStart = -pivotToStart;
                    startToMover = -startToInitial;
                }
                else
                {
                    v = Vector3.Cross(start - pivot, p - pivot);
                    toRotatedStart = Utils.Rotate(pivotToStart, pivot, v, angle);
                    startToMover = Utils.Rotate(startToInitial, Vector3.zero, v, angle);
                    mover.transform.localPosition = pivot + toRotatedStart + startToMover;
                }
                m = angle / span;
                Face(m);
            }
            return m;
        }
        /// <summary>
        /// rotates the <see cref="mover"/> to the closest to a specified point, given the speed of the mover and the time passed 
        /// </summary>
        /// <param name="pGlobal">the specified point in world space</param>
        /// <param name="current">current progress value</param>
        /// <param name="speed">the speed of moving, see <see cref="TameProgress.Speed"/></param>
        /// <param name="time">the delta time of the frame</param>
        /// <returns>the new progress value</returns>
        public float Rotate(Vector3 pGlobal, float current, float time = 0, float speed = 0)
        {
            return RotateByLocalPoint(mover.transform.parent.InverseTransformPoint(pGlobal), current, time, speed);

        }
        /// <summary>
        /// moves the mover based on the grip change
        /// </summary>
        /// <param name="pGlobal">the position of the grip center, in world space</param>
        /// <param name="disp">the displacement between the grip center and the progress, for sliding</param>
        /// <param name="angDif">the displacement between the grip center and the progress for rotation</param>
        /// <param name="current">the current progress</param>
        /// <returns></returns>
        public float Grip(Vector3 pGlobal, float disp, float angDif, float current)
        {
            float m = current;
            Vector3 p;
            if (doesSlide)
            {
                p = mover.transform.parent.InverseTransformPoint(pGlobal) - disp * vector;
                m = Slide(p, current, 1, 1);
            }
            else if (rotType != MovingTypes.Error)
            {
                p = mover.transform.parent.InverseTransformPoint(pGlobal);
                p = Utils.Rotate(p, pivot, axis, angDif);
                m = RotateByLocalPoint(p, current, 1, 1);
            }
            return m;
        }
        /// <summary>
        /// not used.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pt"></param>


    }
}