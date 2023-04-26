
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Tames;
using Assets.Script;
/// <summary>
/// types of mainfest items. This roughly corresponds to the first word of a manifest line. This enum has several functions. It is used to define the subtypes in a <see cref="TameManager"/>. 
/// Three of them (Update, Slide and Rotate) also indicate the effects of a <see cref="TameEffect"/>.
/// </summary>

/// <summary>
/// type of linking
/// </summary>
public enum LinkedKeys
{
    None = 0,
    Progress = 1,
    Clone = 2,
    Local = 3,
    Stack = 4,
    Cycle = 5,
}
/// <summary>
/// the type of cycle in a progress (see <see cref="TameProgress"/>)
/// </summary>
public enum CycleTypes
{
    Stop = 0,
    Cycle = 1,
    Reverse = 2
}
/// <summary>
/// not currently use. It is intended to define the type of a path for moving objects
/// </summary>
public enum PathTypes
{
    Ring = 0,
    Poly = 1,
    Curve = 2,
    Lines = 3,
}
/// <summary>
///  not in use.
/// </summary>
public enum ParentType
{
    Object = 0,
    System = 1
}
/// <summary>
/// not in use.
/// </summary>
public enum ActionTypes
{
    Follow = 0,
    Closest = 1
}
/// <summary>
/// defines how an elements reacts to changes in its parent (see <see cref="TameProgress.follow"/>
/// </summary>
public enum FollowTypes
{
    Independent = 0,
    With = 1
}
/// <summary>
/// defines how the <see cref="TameElement.mover"/> rotates locally:
/// Free: no restriction in rotation (its rotation follows the elements rotation
/// Fixed: doesn't rotate at all (it always faces the same direction)
/// Axis: only revolves around a specific axis 
/// </summary>
public enum FacingLogic
{
    Free = 1,
    Fixed = 2,
    Axis = 3,
}
/// <summary>
/// this is used in <see cref="TameHandles"/> to find the span of rotation
/// </summary>
public enum RotatingLogic
{
    Middle = 1,
    End = 2,
    Open = 3,
}

/// <summary>
/// see <see cref="TameProgress.passToChildren"/>
/// </summary>
public enum PassTypes
{
    Progress = 0, Total = 1,
}
/// <summary>
/// types of possible tame elements when defining a <see cref="TameElement"/>.
/// </summary>
public enum TameKeys
{
    None = 0,
    Object = 1,
    Material = 2,
    Light = 3,
    Time = 4,
    Head = 5,
    Hand = 6,
    Area = 7,
    Walk = 8,
    Calendar = 9,
    Custom = 10,
    Import = 11,
    Camera = 12,
    Eye = 13,
    Mode = 14,
    Alter = 15,
    Match = 16,
}
/// <summary>
/// types of material properties used in <see cref="TameMaterial"/>.
/// </summary>
public enum MaterialProperty
{
    LightX = 8,
    LightY = 9,
    MapX = 10,
    MapY = 11,
    Glow = 12,
    Color = 13,
    Bright = 18,
    Focus = 19,
}
public enum EnvironmentProperty
{
    Tint = 1,
    Exposure = 2
}
/// <summary>
/// not in use. Light manifests use Spectrum (or Color) and Bright of <see cref="MaterialProperty"/> instead.
/// </summary>
public enum LightProperty
{
    Color = 0,
    Intensity = 1,
}
/// <summary>
/// types of moving. Only Rotator and Facer are used in <see cref="TameHandles"/> to determine the rotation type. For indicating Slider, the class uses field <see cref="TameHandles.DoesSlide"/>
/// </summary>
public enum MovingTypes
{
    Slider = 0,
    Rotator = 1,
    Facer = 2,
    Pather = 3,

    Error = 10
}
/// <summary>
/// Not currently in use. It is intended to distinguish between whole mechanisms and those which have their children moving instead (e.g. blinds).
/// </summary>
public enum ChildMovement
{
    Parent = 0,
    Local = 1
}
/// <summary>
/// not in use. The projects uses <see cref="ManifestKeys"/> instead
/// </summary>
public enum ChildTypes
{
    Update = 0,
    Slide = 1,
    Rotate = 2
}
/// <summary>
/// types of geometry for an interaction area (presence in or out of which determines whether an interaction has occurerd). Currently Plane mode is not used. 
/// </summary>
public enum InteractionGeometry
{
    Plane = 4,
    Box = 1,
    Cylinder = 2,
    Sphere = 3,
    Remote = 5,
    Distance = 6,
    Error = 10,
}
/// <summary>
/// types of presence inside the <see cref="InteractionGeometry"/>:
/// Inside: interaction occurs only when the person (or hand) is inside the geometry.
/// Outside: interaction occurs only when the person (or hand) is outside the geometry.
/// InOut: interaction occurs both inside and outside the geometry but the <see cref="TameProgress.changingDirection"/> is positive only when inside and negative when outside.
/// OutIn: interaction occurs both inside and outside the geometry but the <see cref="TameProgress.changingDirection"/> is positive only when outside and negative when inside.
/// Grip: interaction occurs only when the grip gesture is applied inside the geometry.
/// </summary>
public enum InteractionMode
{
    Inside = 1,
    Outside = 2,
    InOut = 3,
    OutIn = 4,
    Grip = 5,
    Switch1 = 6,
    Switch2 = 7,
    Switch3 = 8,
}
/// <summary>
/// defines how the interaction geometry is itself updated (by modifying its corresponding game object, see <see cref="TameArea.relative"/>:
/// Fixed: the geometry remains still (doesn't move or rotate)
/// Parent: the geometry moves and rotates based on the parent object (<see cref="TameElement.owner"/> of the associated <see cref="TameObject"/>
/// Mover: the geometry moves and rotates based on the mechanism (<see cref="TameElement.mover"/> of the associated <see cref="TameObject"/>
/// Object: the geometry moves and rotates based on the associated object with it. This object is a child or descendant of the parent (<see cref="TameElement.owner"/> but may be a sibling or descendant of the mechanism. An example is the Grip area around the handle object of a door. The Grip area should be associated with the handles not the door. 
/// </summary>
public enum InteractionUpdate
{
    Fixed = 1,
    Parent = 2,
    Mover = 3,
    Object = 4,
}
/// <summary>
/// relationships for finding objects and elements, see <see cref="TameFinder.Relations"/>
/// </summary>
public enum RelationTypes
{
    None = 0,
    Child = 1,
    Parent = 2,
    Nephew = 3,
    Uncle = 4,
    Sibling = 5,
    Grand = 10,
}
/// <summary>
/// type of a changer's changing
/// </summary>
public enum ToggleType
{
    Gradient = 0,
    Stepped = 1,
    Switch = 2
}
/// <summary>
/// the vector of walking surface push force
/// </summary>
public enum ForceType
{
    None = 0,
    Slide = 1,
    Rotate = 2,
}
public static class InputBasis
{
    public const int Mouse = 1;
    public const int Button = 2;
    public const int VR = 3;
    public static int tilt = Mouse;
    public static int turn = Button;
    public static int move = Button;
    static readonly int[] toggles = new int[]
    {
        Mouse, Button, Button,
        Button, Button, Button,
        VR, VR, Button,
        VR, VR, VR
    };
    static int current = 0;
    public static int ReadMode(ManifestHeader mh, int index)
    {
        int m;
        if (mh.items.Count > 0)
            if (Utils.SafeParse(mh.items[0], out m))
                current = m;
        return index;
    }
    public static void ToggleNext()
    {
        current = (current + 1) % (toggles.Length / 3);
        tilt = toggles[current * 3];
        turn = toggles[current * 3 + 1];
        move = toggles[current * 3 + 2];
        Debug.Log(current + " > " + tilt + ", " + turn + ", " + move);
    }
    public static int GamePadButton(InputHoldType iht, float threshold = 0)
    {
        float f;
        if (Gamepad.current != null)
        {
            //     Debug.Log(Gamepad.current.rightTrigger.isPressed ? 1 : (Gamepad.current.leftTrigger.isPressed ? -1 : 0));
            switch (iht)
            {
                case InputHoldType.GPDY:
                    f = Gamepad.current.dpad.y.ReadValue();
                    return Mathf.Abs(f) <= threshold ? 0 : (f < 0 ? -1 : 1);
                case InputHoldType.GPDX:
                    f = Gamepad.current.dpad.x.ReadValue();
                    return Mathf.Abs(f) <= threshold ? 0 : (f < 0 ? -1 : 1);
                case InputHoldType.GPSRY:
                    f = Gamepad.current.rightStick.y.ReadValue();
                    return Mathf.Abs(f) <= threshold ? 0 : (f < 0 ? -1 : 1);
                case InputHoldType.GPSRX:
                    f = Gamepad.current.rightStick.x.ReadValue();
                    return Mathf.Abs(f) <= threshold ? 0 : (f < 0 ? -1 : 1);
                case InputHoldType.GPShoulder:
                    return Gamepad.current.rightShoulder.isPressed ? 1 : (Gamepad.current.leftShoulder.isPressed ? -1 : 0);
                case InputHoldType.GPTrigger:
                    return Gamepad.current.rightTrigger.isPressed ? 1 : (Gamepad.current.leftTrigger.isPressed ? -1 : 0);
            }
        }
        return 0;
    }
}
/// <summary>
/// this class contains the types of parents for an element
/// </summary>
public class TrackBasis
{
    public const byte Error = 0;
    public const byte Hand = 1;
    public const byte Head = 2;
    public const byte Time = 4;
    public const byte Object = 8;
    public const byte Tame = 16;
    public const byte Grip = 32;
    public const byte Mover = 64;
    public const byte Manual = 128;
    public static bool IsHand(int tb)
    {
        return (tb & Hand) != 0;
    }
    public static bool IsHead(int tb)
    {
        return (tb & Head) != 0;
    }
    public static bool IsObject(int tb)
    {
        return (tb & Object) != 0;
    }
    public static bool IsTracking(int tb)
    {
        return (tb& Hand+Head+Object) != 0;
    }
}
public class Alias
{
    private static string[] _keys = new string[]
    {
            "Import",
            "Walk",
            "Camera",
            "Mode",
            "Eye",
            "object",
            "material",
            "light",
            "custom",
            "Update",
            "Follow",
            "Input",
            "Trigger",
            "Speed",
            "Duration/length",
            "Cycle",
            "Reverse/bounce",
            "Stop",
            "eu",
            "ev",
            "u",
            "v",
            "Color/colour",
            "Glow/spectrum",
            "Bright/brightness/intensity",
            "Unique",
            "Focus",
            "Link",
            "Clone",
            "Queue",
            "Scale",
            "Set",
            "Initial",
            "Area",
            "Local",
            "Ratio",
            "Stack",
            "Count",
            "By",
            "Move",
            "Turn",
            "Both/all",
            "Alter",
            "Track",
            "Factor",
            "Match",
            "Force",
            "Affect",
            "Disable",
            "Enable"
    };
    public static int KeyCount = _keys.Length;
    public string[] alias;

    public Alias(string s)
    {
        alias = s.Split('/');
        //      Debug.Log("queue: " + alias[0]);
    }
    public static Alias[] AllAliases()
    {
        Alias[] result = new Alias[_keys.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = new Alias(_keys[i]);
        return result;
    }
    public bool Has(string s)
    {
        for (int i = 0; i < alias.Length; i++)
            if (alias[i].ToLower() == s.ToLower())
                return true;
        return false;
    }
}
public class ManifestKeys
{

    public static Alias[] keys;
    public static string[] langs;
    public static int current = 0;
    public const int None = 0;
    public const int Import = 1;
    public const int Walk = 2;
    public const int Camera = 3;
    public const int Mode = 4;
    public const int Eye = 5;
    public const int Object = 6;
    public const int Material = 7;
    public const int Light = 8;
    public const int Custom = 9;
    public const int Update = 10;
    public const int Follow = 11;
    public const int Input = 12;
    public const int Trigger = 13;
    public const int Speed = 14;
    public const int Duration = 15;
    public const int Cycle = 16;
    public const int Reverse = 17;
    public const int Stop = 18;
    public const int LightX = 19;
    public const int LightY = 20;
    public const int MapX = 21;
    public const int MapY = 22;
    public const int Color = 23;
    public const int Glow = 24;
    public const int Bright = 25;
    public const int Unique = 26;
    public const int Focus = 27;
    public const int Linked = 28;
    public const int Clone = 29;
    public const int Queue = 30;
    public const int Scale = 31;
    public const int Set = 32;
    public const int Initial = 33;
    public const int Area = 34;

    public const int Local = 35;
    public const int Ratio = 36;
    public const int Stack = 37;


    public const int Count = 38;
    public const int By = 39;

    public const int Move = 40;
    public const int Turn = 41;
    public const int Both = 42;

    public const int Alter = 43;
    //  public const int Channel = 44;

    public const int Track = 44;
    public const int Factor = 45;
    public const int Match = 46;
    public const int Enforce = 47;
    public const int Affect = 48;
    public const int Disable = 49;
    public const int Enable = 50;

    public static void LoadCSV(string s)
    {
        //      string[] lines = Identifier.LoadLines(s);
        //     string first = lines[0];
        keys = Alias.AllAliases();
        //        Debug.Log(i + " enfo " + lines[i] + " " + keys[i - 1].alias[0]);

    }
    public static int GetKey(string s)
    {
        if (s.ToLower() == "force") Debug.Log("has enforce" + keys[Enforce - 1].Has(s) + " " + keys[Enforce - 1].alias[0]);
        for (int i = 0; i < Alias.KeyCount; i++)
            if (keys[i].Has(s))
            {
                if (i + 1 == Enforce) Debug.Log("enforce is");
                return i + 1;
            }
        return 0;
    }


}
/// <summary>
/// this class contains a number of miscellaneous methods useful for the project 
/// </summary>
public class Utils
{
    private static GameObject PO, CO;
    public static bool HDActive = false;
    public static string[] ProperyKeywords;
    public static void SetPOCO()
    {
        PO = new GameObject("PO");
        PO.transform.position = Vector3.zero;
        CO = new GameObject("CO");
        CO.transform.parent = PO.transform;
    }
    public static void SetPipelineLogics()
    {
        string pn = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name;
        if (pn.IndexOf("HD") >= 0)
        {
            HDActive = true;
            ProperyKeywords = new string[] { "_BaseColor", "_EmissiveColor", "_BaseColorMap", "_EmissiveColorMap" };
        }
        else
        {

            ProperyKeywords = new string[] { "_BaseColor", "_EmissionColor", "_MainTex", "_EmissionMap" };
        }
    }
    public static void Write3(BinaryWriter bw, Vector3 p)
    {
        bw.Write(p.x);
        bw.Write(p.y);
        bw.Write(p.z);
    }
    public static void Write2(BinaryWriter bw, Vector2 p)
    {
        bw.Write(p.x);
        bw.Write(p.y);
    }
    public static void Write4(BinaryWriter bw, Quaternion p)
    {
        bw.Write(p.x);
        bw.Write(p.y);
        bw.Write(p.z);
        bw.Write(p.w);
    }
    public static Vector2 Read2(BinaryReader bin)
    {
        Vector2 result = new Vector2();
        result.x = bin.ReadSingle();
        result.y = bin.ReadSingle();
        return result;
    }
    public static Vector3 Read3(BinaryReader bin)
    {
        Vector3 result = new Vector3();
        result.x = bin.ReadSingle();
        result.y = bin.ReadSingle();
        result.z = bin.ReadSingle();
        return result;
    }
    public static Quaternion Read4(BinaryReader bin)
    {
        Quaternion result = new Quaternion();
        result.x = bin.ReadSingle();
        result.y = bin.ReadSingle();
        result.z = bin.ReadSingle();
        result.w = bin.ReadSingle();
        return result;
    }
    /// <summary>
    /// returns the distance between a specific point and a plain based on its normal and a point of origin on it. 
    /// </summary>
    /// <param name="p">the point</param>
    /// <param name="o">the origin point</param>
    /// <param name="v">the normal vector (not necessariliy normalized)</param>
    /// <returns>the distance relative to the magnitude of the normal vector</returns>
    public static float M(Vector3 p, Vector3 o, Vector3 v)
    {
        float d = -Vector3.Dot(v, o);
        return -(Vector3.Dot(p, v) + d) / v.sqrMagnitude;
    }
    /// <summary>
    /// returns the collusion point on a plane from a specific point on space if it moved parallel to the planes normal vector 
    /// </summary>
    /// <param name="p">the point in space</param>
    /// <param name="origin">a point on the plane</param>
    /// <param name="normal">planes normal vector (not necessarily normalized)</param>
    /// <returns>a point on the plane</returns>
    public static Vector3 On(Vector3 p, Vector3 origin, Vector3 normal)
    {
        float d = -Vector3.Dot(normal, origin);
        float m = -(Vector3.Dot(p, normal) + d) / normal.sqrMagnitude;
        return p + m * normal;
    }
    /// <summary>
    /// puts a vector on the plane based on its normal and a point
    /// </summary>
    /// <param name="v">the vector</param>
    /// <param name="origin">the point on the plane</param>
    /// <param name="normal">plane's normal</param>
    /// <returns></returns>
    public static Vector3 VOn(Vector3 v, Vector3 origin, Vector3 normal)
    {
        if ((v - origin).magnitude == 0) return v;
        float d = -Vector3.Dot(normal, origin);
        float m = -(Vector3.Dot(v, normal) + d) / normal.sqrMagnitude;
        Vector3 q = (v + m * normal) - origin;
        if (q.magnitude == 0)
        {
            q = v - origin;
            if (Vector3.Angle(normal, q) > 90)
                q = -q;
        }
        return q;
    }
    /// <summary>
    /// converts a vector in the space to the two dimensional scaled coordinates on a plane (the vector should be on the plane)  
    /// </summary>
    /// <param name="p">the vector (departing from (0,0), or a point on the place</param>
    /// <param name="u">the "x" axis of the custom coordinate on the plane</param>
    /// <param name="v">the "y" axis of the custom coordinate on the plane</param>
    /// <returns>the factors of u and v by which one reaches to world-space p on the plane</returns>
    public static Vector2 MN(Vector3 p, Vector3 u, Vector3 v)
    {
        if (p.magnitude == 0)
            return Vector2.zero;
        float uA = Vector3.Angle(p, u);
        float vA = Vector3.Angle(p, v);
        float m = Mathf.Cos(uA);
        float n = Mathf.Cos(vA);
        return new Vector2(m * p.magnitude / u.magnitude, n * p.magnitude / v.magnitude);
    }
    /// <summary>
    /// rotates a point around a vector and an origin point by an angle
    /// </summary>
    /// <param name="p">the rotating point</param>
    /// <param name="o">origin</param>
    /// <param name="u">rotation axis</param>
    /// <param name="degrees">angle in degrees</param>
    /// <returns></returns>
    public static Vector3 Rot(Vector3 p, Vector3 o, Vector3 u, float degrees)
    {
        float angle = degrees * Mathf.Deg2Rad;
        Vector3 q = On(o, p, u);
        Vector3 pq = p - q;
        if (pq.magnitude == 0)
            return p;
        Vector3 v = pq.normalized;
        Vector3 w = Vector3.Cross(u.normalized, v);
        Vector2 r = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * pq.magnitude;
        return q + r.x * v + r.y * w;
    }
    public static Vector3 Rotate(Vector3 p, Vector3 o, Vector3 u, float degrees)
    {
        Vector3 op = p - o;
        float opm = op.magnitude;
        op.Normalize();
        PO.transform.LookAt(op, u);
        CO.transform.position = op;
        PO.transform.Rotate(Vector3.up, degrees);
        Vector3 q = CO.transform.position;
        return q * opm + o;
    }
    public static float Angle(Vector3 p, Vector3 pivot, Vector3 start, Vector3 axis, bool signed)
    {
        Vector3 pn = (p - pivot).normalized;
        Vector3 sn = (start - pivot).normalized;
        Vector3 axn = axis.normalized;
        PO.transform.rotation = Quaternion.identity;
        float angle = Vector3.Angle(pn, sn);
        PO.transform.LookAt(sn, axn);
        CO.transform.position = PO.transform.forward;
        PO.transform.Rotate(Vector3.up, angle);
        Vector3 pos = CO.transform.position;
        PO.transform.Rotate(Vector3.up, -2 * angle);
        Vector3 neg = CO.transform.position;
        if (Vector3.Distance(pos, pn) < Vector3.Distance(neg, pn))
            return angle;
        else
            return signed ? -angle : 360 - angle;
    }
    /// <summary>
    /// returns the rotation angle from a starting point to a destination point around an axis and an origin point. The angle corresponds to the rotation from the landed points (see <see cref="On"/>) of the starting and destination points on the plane with origin and the axis (normal vector). The returned angle is signed (-180 to 180 degrees). For the full angle see <see cref="FullAngle"/>
    /// </summary>
    /// <param name="p">the destination point</param>
    /// <param name="pivot">the origin</param>
    /// <param name="start">the starting point</param>
    /// <param name="axis">the rotation axis</param>
    /// <returns>the signed angle of the rotation in degrees</returns>
    public static float _SignedAngle(Vector3 p, Vector3 pivot, Vector3 start, Vector3 axis)
    {

        Vector3 a = On(start - pivot, Vector3.zero, axis).normalized;
        Vector3 b = On(p - pivot, Vector3.zero, axis).normalized;
        float f = Vector3.Angle(a, b);
        Vector3 c = Rotate(a, Vector3.zero, axis, f);
        return Vector3.Distance(c, b) > Vector3.Distance(a, b) ? -f : f;

    }
    /// <summary>
    /// returns the rotation angle from a starting point to a destination point around an axis and an origin point. The angle corresponds to the rotation from the landed points (see <see cref="On"/>) of the starting and destination points on the plane with origin and the axis (normal vector). The returned angle is always zero or positive. For the signed angle see <see cref="SignedAngle"/>
    /// </summary>
    /// <param name="p">the destination point</param>
    /// <param name="pivot">the origin</param>
    /// <param name="start">the starting point</param>
    /// <param name="axis">the rotation axis</param>
    /// <returns>the full angle of the rotation in degrees</returns>
    public static float _FullAngle(Vector3 p, Vector3 pivot, Vector3 start, Vector3 axis)
    {
        Vector3 a = On(start - pivot, Vector3.zero, axis).normalized;
        Vector3 b = On(p - pivot, Vector3.zero, axis).normalized;
        float f = Vector3.Angle(a, b);
        Vector3 c = Rotate(a, Vector3.zero, axis, f);
        return Vector3.Distance(c, b) > Vector3.Distance(a, b) ? 360 - f : f;
    }
    public static float _FullAngle(Vector3 p, Vector3 pivot, Vector3 start, Vector3 axis, Vector3 normal)
    {

        Vector3 a = On(start - pivot, Vector3.zero, axis).normalized;
        Vector3 n = On(normal - pivot, Vector3.zero, axis).normalized;
        Vector3 b = On(p - pivot, Vector3.zero, axis).normalized;
        float fa = Vector3.Angle(a, b);
        float fn = Vector3.Angle(b, n);
        if (fn <= 90) return fa;
        else return 360 - fa;
    }
    /// <summary>
    /// checks of two vectors are parallel. The vectors should be normalized.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>returns if a and b are parallel</returns>
    public static bool Parallel(Vector3 a, Vector3 b)
    {
        return a.Equals(b) || a.Equals(-b);
    }
    /// <summary>
    /// finds an axis for rotation from one vector to another. If the vectors are parallel and opposite, it generates a vector based on Up or Right directions
    /// </summary>
    /// <param name="a">starting vector</param>
    /// <param name="b">ending vector</param>
    /// <returns>returns the rotation axis</returns>
    public static Vector3 Axis(Vector3 a, Vector3 b)
    {
        int C = 0;
        Vector3 u = Vector3.Cross(b, a).normalized;
        if (u.magnitude == 0)
        {
            if (Vector3.Distance(b, a) < a.magnitude)
                C = 1;
            else
                C = -1;
        }
        if (C == 1)
            return Vector3.up;
        if (C == -1)
        {
            if (!Parallel(a.normalized, Vector3.up))
                u = On(Vector3.up, Vector3.zero, a);
            else
                u = On(Vector3.right, Vector3.zero, a);
        }
        return u;
    }
    public static Vector3 Perp(Vector3 u)
    {
        int max = Mathf.Abs(u.x) < Mathf.Abs(u.y) ? (Mathf.Abs(u.y) < Mathf.Abs(u.z) ? 2 : 1) : (Mathf.Abs(u.x) < Mathf.Abs(u.z) ? 2 : 0);
        Vector3 v = u;
        v[max] = 0;
        v[(max + 1)%3] = v[(max + 1) % 3]+1;
        return Vector3.Cross(u, v);
    }
    /// <summary>
    /// swaps two elements of a float array
    /// </summary>
    /// <param name="a">the array</param>
    /// <param name="i">index of the first element</param>
    /// <param name="j">index of the second element</param>
    public static void Swap(float[] a, int i, int j) { float k = a[i]; a[i] = a[j]; a[j] = k; }
    /// <summary>
    /// swaps two elements of a int array
    /// </summary>
    /// <param name="a">the array</param>
    /// <param name="i">index of the first element</param>
    /// <param name="j">index of the second element</param>
    public static void Swap(int[] a, int i, int j) { int k = a[i]; a[i] = a[j]; a[j] = k; }
    /// <summary>
    /// swaps two elements of a Vector3 array
    /// </summary>
    /// <param name="a">the array</param>
    /// <param name="i">index of the first element</param>
    /// <param name="j">index of the second element</param>
    public static void Swap(Vector3[] a, int i, int j) { Vector3 k = a[i]; a[i] = a[j]; a[j] = k; }
    /// <summary>
    /// checks if a number in a range (or equal to its ends). 
    /// </summary>
    /// <param name="x">the between number</param>
    /// <param name="a">one end of the range</param>
    /// <param name="b">the other end of the range, can be smaller or larger than the other end</param>
    /// <returns>returns true if x is equal to or between a and b</returns>
    public static bool Between(float x, float a, float b)
    {
        return ((x >= a) && (x <= b)) || ((x >= b) && (x <= a));
    }
    /// <summary>
    /// removes spaces and tabs at the beginning and end of a string
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static string Clean(string line)
    {
        string s = "";
        for (int i = 0; i < line.Length; i++)
            if (" \t".IndexOf(line[i]) < 0)
            {
                s = line.Substring(i);
                break;
            }
        for (int i = s.Length - 1; i > 0; i--)
            if (" \t".IndexOf(s[i]) < 0)
            {
                s = s.Substring(0, i + 1);
                break;
            }

        return s;
    }
    /// <summary>
    /// turn all adjacent instances of specified characters in a string into only one of them (by retaining the first instance) 
    /// </summary>
    /// <param name="s">the source string </param>
    /// <param name="dupl">the string containing the specified characters</param>
    /// <returns>the string without adjacent specified characters</returns>
    public static string RemoveDuplicate(string s, string dupl)
    {
        string r = "";
        bool pass = false;
        for (int i = 0; i < s.Length; i++)
            if (dupl.IndexOf(s[i]) < 0)
            { r += s[i]; pass = false; }
            else if (!pass)
            { r += s[i]; pass = true; }
        return r;
    }
    /// <summary>
    /// creates a list of strings by separating portions of the source string, delimited by specified characters. This method considers adjacent delimiters as separating zero length string. You should <see cref="Clean"/> and <see cref="RemoveDuplicate"/> to get a neat list. 
    /// </summary>
    /// <param name="s">the source string</param>
    /// <param name="delimit">the string specifying characters</param>
    /// <returns>the list of separated strings</returns>
    public static List<string> Split(string s, string delimit)
    {
        string r = "";
        List<string> list = new List<string>();
        for (int i = 0; i < s.Length; i++)
            if (delimit.IndexOf(s[i]) >= 0)
            {
                if (r.Length > 0) list.Add(r);
                r = "";
            }
            else
                r += s[i];
        if (r.Length > 0)
            list.Add(r);

        return list;
    }
    /// <summary>
    /// parses a string into a float. This is equivalent to <see cref="float.Parse"/> but has try-catch block included.
    /// </summary>
    /// <param name="s">the source string</param>
    /// <param name="f">the output of the parsed float. Only refer to its value if the method returned true</param>
    /// <returns>whetehr the parsing was successful</returns>
    public static bool SafeParse(string s, out float f)
    {
        try
        {
            f = float.Parse(s);
            return true;
        }
        catch
        {
            f = 0;
            return false;
        }
    }
    public static bool SafeParse(string s, out int f)
    {
        try
        {
            f = int.Parse(s);
            return true;
        }
        catch
        {
            f = 0;
            return false;
        }
    }
    /// <summary>
    /// parses a list of strings into a float array. 
    /// </summary>
    /// <param name="s">the source string list</param>
    /// <param name="f">the output of the parsed floats. Only refer to its values if the method returned true</param>
    /// <returns>whetehr the parsing was successful for all strings</returns>
    public static bool SafeParse(List<string> list, out float[] r)
    {
        r = new float[list.Count];
        float f;
        for (int i = 0; i < list.Count; i++)
            if (SafeParse(list[i], out f))
                r[i] = f;
            else
                return false;
        return true;
    }
    /// <summary>
    /// finds the first child of a transform whose name starts with a specific string
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static Transform FindStartsWith(Transform parent, string start)
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name.ToLower().StartsWith(start))
                return parent.GetChild(i);
        return null;
    }
    public static Vector3 DetectBox(Transform t, Mesh mesh)
    {
        float f;
        if (mesh == null)
            return Vector3.zero;
        else
        {
            Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
            Vector3[] v = mesh.vertices;
            Vector3 g;
            for (int i = 0; i < v.Length; i++)
            {
                //   g = t.TransformPoint(v[i]);
                g = v[i];
                if (g.x < min.x) min.x = g.x;
                if (g.y < min.y) min.y = g.y;
                if (g.z < min.z) min.z = g.z;
                if (g.x > max.x) max.x = g.x;
                if (g.y > max.y) max.y = g.y;
                if (g.z > max.z) max.z = g.z;
            }
            return max - min;
        }
    }
    /// <summary>
    /// detects the size of a box mesh
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static Vector3 DetectBox(GameObject g)
    {
        try
        {
            MeshFilter mf = g.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            //    Debug.Log("trying "+mesh.name);
            return DetectBox(g.transform, mesh);
        }
        catch { return Vector3.zero; }
    }
    /// <summary>
    /// detects the size of an spherical mesh
    /// </summary>
    /// <param name="g"></param>
    /// <returns>returns a vector with all elements equal to the distance from the center to the furthest vertex</returns>
    public static Vector3 DetectSphere(GameObject g)
    {
        try
        {
            MeshFilter mf = g.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            return DetectBox(g.transform, mesh);
        }
        catch { return Vector3.zero; }
    }
    public static Vector3 DetectCylinder(GameObject g, out int axis)
    {
        axis = -1;
        try
        {
            MeshFilter mf = g.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            Vector3 box = DetectBox(g.transform, mesh);
            float f;
            if (mesh == null)
                return Vector3.zero;
            else
            {
                Vector3[] tv = new Vector3[3];
                Vector3[] v = mesh.vertices;
                int[] t = mesh.triangles;
                int i0 = 0, j0 = -1, k1 = 0, t0 = 0;
                for (int i = 0; i < t.Length; i += 3)
                {
                    tv[0] = v[t[i]]; tv[1] = v[t[i + 1]]; tv[2] = v[t[i + 2]];
                    for (int j = 0; j < 3; j++)
                    {
                        f = Vector3.Angle(tv[(j + 1) % 3] - tv[j], tv[(j + 2) % 3] - tv[j]);
                        if (Mathf.Abs(f - 90) < 1)
                        { j0 = j; t0 = i; break; }
                    }
                    if (j0 >= 0) break;
                }
                bool b = false;
                for (int i = 0; i < t.Length; i += 3)
                    if (t0 != i)
                    {
                        tv[0] = v[t[i]]; tv[1] = v[t[i + 1]]; tv[2] = v[t[i + 2]];
                        b = false;
                        for (int j = 0; j < 3; j++)
                            if (t[i + j] == t[i0 + j0])
                                b = true;
                        if (b)
                        {
                            b = false;
                            for (int j = 0; j < 3; j++)
                            {
                                f = Vector3.Angle(tv[(j + 1) % 3] - tv[j], tv[(j + 2) % 3] - tv[j]);
                                if (Mathf.Abs(f - 90) < 1)
                                {
                                    b = true;
                                    if (t[i + j] == (i0 + j0 + 1) % 3) { k1 = t[i + j]; break; }
                                    if (t[i + j] == (i0 + j0 + 2) % 3) { k1 = t[i + j]; break; }
                                    break;
                                }
                            }

                        }
                        if (b)
                        {
                            Vector3 u = v[k1] - v[t[i0 + j0]];
                            float[] ang = new float[]
                            {
                                Vector3.Angle(u, g.transform.right),
                                Vector3.Angle(u, g.transform.up),
                                Vector3.Angle(u, g.transform.forward)
                            };
                            for (int j = 0; j < 3; j++) if (ang[j] > 90) ang[j] = 180 - ang[j];
                            i0 = 0;
                            if (ang[1] < ang[0]) i0 = 1;
                            if (ang[2] < ang[i0]) i0 = 2;
                            axis = i0;
                            return box;
                        }
                    }
                return Vector3.zero;
            }
        }
        catch { return Vector3.zero; }
    }
    /// <summary>
    /// finds the corrsponding tranform vector to the height vector of a cylinder mesh (from the center of one cap to the other) 
    /// </summary>
    /// <param name="g">the gameobject containing the mesh</param>
    /// <returns>0, 1, or 2 for right, up or forward, or a negative number for error</returns>
    public static int DetectCylinderVector(GameObject g)
    {
        try
        {
            MeshFilter mf = g.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            float f;
            if (mesh == null)
                return -1;
            else
            {
                Vector3[] tv = new Vector3[3];
                Vector3[] v = mesh.vertices;
                int[] t = mesh.triangles;
                int i0 = 0, j0 = -1, k1 = 0, t0 = 0;
                for (int i = 0; i < t.Length; i += 3)
                {
                    tv[0] = v[t[i]]; tv[1] = v[t[i + 1]]; tv[2] = v[t[i + 2]];
                    for (int j = 0; j < 3; j++)
                    {
                        f = Vector3.Angle(tv[(j + 1) % 3] - tv[j], tv[(j + 2) % 3] - tv[j]);
                        if (Mathf.Abs(f - 90) < 1)
                        { j0 = j; t0 = i; break; }
                    }
                    if (j0 >= 0) break;
                }
                bool b = false;
                for (int i = 0; i < t.Length; i += 3)
                    if (t0 != i)
                    {
                        tv[0] = v[t[i]]; tv[1] = v[t[i + 1]]; tv[2] = v[t[i + 2]];
                        b = false;
                        for (int j = 0; j < 3; j++)
                            if (t[i + j] == t[i0 + j0])
                                b = true;
                        if (b)
                        {
                            b = false;
                            for (int j = 0; j < 3; j++)
                            {
                                f = Vector3.Angle(tv[(j + 1) % 3] - tv[j], tv[(j + 2) % 3] - tv[j]);
                                if (Mathf.Abs(f - 90) < 1)
                                {
                                    b = true;
                                    if (t[i + j] == (i0 + j0 + 1) % 3) { k1 = t[i + j]; break; }
                                    if (t[i + j] == (i0 + j0 + 2) % 3) { k1 = t[i + j]; break; }
                                    break;
                                }
                            }

                        }
                        if (b)
                        {
                            Vector3 u = v[k1] - v[t[i0 + j0]];
                            float[] ang = new float[]
                            {
                                Vector3.Angle(u, g.transform.right),
                                Vector3.Angle(u, g.transform.up),
                                Vector3.Angle(u, g.transform.forward)
                            };
                            for (int j = 0; j < 3; j++) if (ang[j] > 90) ang[j] = 180 - ang[j];
                            i0 = 0;
                            if (ang[1] < ang[0]) i0 = 1;
                            if (ang[2] < ang[i0]) i0 = 2;
                            return i0;
                        }
                    }
                return -2;
            }
        }
        catch (Exception e)
        {
            return -3;
        }
    }
    public static List<Vector3> GetUnique(Vector3[] v, int[] rep)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < v.Length; i++)
        {
            rep[i] = -1;
            for (int j = 0; j < result.Count; j++)
                if (Vector3.Distance(result[j], v[i]) < 0.0005f)
                {
                    v[i] = result[j];
                    rep[i] = j;
                    break;
                }
            if (rep[i] < 0)
            {
                rep[i] = result.Count;
                result.Add(v[i]);
            }
            //    Debug.Log("path un: " + result.Count + " " + rep[i]);
        }
        return result;
    }
    public static Quaternion GetQuaternion(Transform t, Quaternion original, Vector3[] r)
    {
        Quaternion q0 = original;
        t.LookAt(t.parent.TransformPoint(r[0] + t.localPosition), t.parent.TransformPoint(r[1] + t.localPosition) - t.position);
        Quaternion q1 = t.localRotation;
        t.LookAt(t.parent.TransformPoint(r[2] + t.localPosition), t.parent.TransformPoint(r[3] + t.localPosition) - t.position);
        Quaternion q2 = t.localRotation;
        Quaternion r10 = Quaternion.Inverse(q1) * q0;
        q2 = q2 * r10;
        t.localRotation = original;
        return Quaternion.Inverse(original) * q2;
    }
    public static Quaternion GetQuaternion(Transform t, Vector3[] from, Vector3[] to)
    {
        Quaternion r0 = t.localRotation;
        t.LookAt(t.parent.TransformPoint(from[0] + t.localPosition), t.parent.TransformPoint(from[1] + t.localPosition) - t.position);
        Quaternion r1 = t.localRotation;
        t.LookAt(t.parent.TransformPoint(to[0] + t.localPosition), t.parent.TransformPoint(to[1] + t.localPosition) - t.position);
        Quaternion r2 = t.localRotation;
        t.localRotation = r0;
        return Quaternion.Inverse(r1) * r2;
    }
    public static void AddInt(List<int> list, int n)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == n)
                return;
        list.Add(n);
    }
    public static void RandomizeUV(GameObject go, int index, int dir)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf != null)
        {
            Mesh m = mf.mesh;
            float r = ((index * 3) % 5 + (53542524 % (index + 29))) % 29f;
            r /= 29f;
            Vector2[] uv = m.uv;
            for (int i = 0; i < uv.Length; i++)
                if (dir == 0)
                    uv[i].x = uv[i].x + r;
                else
                    uv[i].y = uv[i].y + r;
            m.uv = uv;
        }
    }
    public static Vector3 LocalizePoint(Vector3 p, Transform from, Transform to)
    {
        Vector3 q = from.TransformPoint(p);
        return to.InverseTransformPoint(q);
    }
    public static Vector3 LocalizeVector(Vector3 v, Transform a, Transform b)
    {
        Vector3 p, q;
        if (a == null)
        {
            p = b.position;
            q = p + v;
            return b.InverseTransformPoint(q) - b.InverseTransformPoint(p);
        }
        else if (b == null)
        {
            p = a.position;
            q = a.TransformPoint(v);
            return q - p;
        }
        else
        {
            p = a.position;
            q = a.TransformPoint(v);
            return b.InverseTransformPoint(q) - b.InverseTransformPoint(p);
        }
    }



}

