using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace Tames
{
    /// <summary>
    /// A basic element representing time. 
    /// </summary>
    public class TameTime : TameElement
    {
        /// <summary>
        /// the basic time element
        /// </summary>
        public static TameTime RootTame = new TameTime();
        public TameTime()
        {
            tameType = TameKeys.Time;
            progress = new TameProgress(this);
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
            progress.passToChildren = PassTypes.Total;
            //     progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
    }

    public class TameHead : TameElement
    {
        public static TameHead Heads = new TameHead();
        public Person[] people;
        public TameHead()
        {
            tameType = TameKeys.Head;
            progress = new TameProgress(this);
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
            progress.passToChildren = PassTypes.Total;
            //   progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
    }
    public class TameHand : TameElement
    {
        public static TameHand Hands = new TameHand();
        public Person[] people;
        public TameHand()
        {
            tameType = TameKeys.Head;
            progress = new TameProgress(this);
            progress.passToChildren = PassTypes.Total;
            //    progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
        }
    }
    public class TameCalendar : TameElement
    {
        public TameCalendar()
        {
            tameType = TameKeys.Calendar;
            progress = new TameProgress(this);
            progress.passToChildren = PassTypes.Total;
            //   progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
        public override void Update(TameProgress p)
        {
            SetSunPosition();
        }
        public void IncreaseHour(int x)
        {
            progress.SetProgress(progress.totalProgress + x / 24f);
        }
        public void IncreaseMonth(int x)
        {
            progress.SetProgress(progress.totalProgress + x / 12f);
        }
        public void SetSunPosition()
        {

        }
    }
    /// <summary>
    /// this class is used to create custom parameters that are linked to the input devices
    /// </summary>
    public class TameCustomValue : TameElement
    {
        public List<TameInputControl> control = new List<TameInputControl>();
        public float[] range = new float[] { 0, 1 };
        public TameCustomValue()
        {
            tameType = TameKeys.Custom;
            progress = new TameProgress(this);

            basis = TrackBasis.Time;
        }
        public override void AssignParent(TameEffect[] all, int index)
        {
            //     Debug.Log("custom parent " + name);
            all[index] = TameEffect.Time();
            all[index].child = this;
        }
        public override void Update()
        {
            //     Debug.Log("custom updating");
            int d = 0;
            foreach (TameInputControl c in control)
                if ((d = c.Hold()) != 0)
                    break;
            if (d != 0)
                progress.SetProgress(progress.totalProgress + d * deltaTime * (progress.manager.Speed == -1 ? 1 : progress.manager.Speed));
            else progress.Retain(deltaTime);
        }
    }
    public enum InputTypes
    {
        VRController = 1,
        GamePad = 2,
        KeyboardMouse = 3
    }
    public enum InputDirections
    {
        MouseButton = 1,
        MouseWheel = 2,
        Key = 0
    }
    public enum InputHoldType
    {
        Key = 1,

        VRScrollLeft = 10,
        VRScrollRight = 11,
        VRTrigger = 12,

        GPSRX = 21,
        GPSRY = 22,
        GPDX = 23,
        GPDY = 24,
        GPTrigger = 25,
        GPShoulder = 26,
    }
    public class TameInputControl
    {
        public static bool[] keyStatus = null;
        public static List<ButtonControl> checkedKeys = new List<ButtonControl>();
        public InputTypes control;
        public InputHoldType hold;
        public InputDirections direction;
        public int[] keyValue;
        public static void CheckKeys()
        {
            if (keyStatus == null)
                keyStatus = new bool[checkedKeys.Count];
            for (int i = 0; i < keyStatus.Length; i++)
            {
                keyStatus[i] = checkedKeys[i].isPressed;
                //       Debug.Log("custom key "+i+" "+keyStatus[i]+" "+checkedKeys[i].name);
            }
        }
        public int Hold()
        {
            float f;
            switch (control)
            {
                case InputTypes.VRController:
                    if ((!Assets.Script.MainScript.multiPlayer) || (Player.bossId == Assets.Script.MainScript.localPerson.id))
                        switch (hold)
                        {

                            case InputHoldType.VRScrollLeft:
                                f = Assets.Script.MainScript.localPerson.hand[0].data.stick.Vector.y;
                                if (Mathf.Abs(f) > Assets.Script.MainScript.localPerson.hand[0].data.stick.threshold)
                                    return f < 0 ? -1 : 1;
                                break;
                            case InputHoldType.VRScrollRight:
                                f = Assets.Script.MainScript.localPerson.hand[1].data.stick.Vector.y;
                                if (Mathf.Abs(f) > Assets.Script.MainScript.localPerson.hand[1].data.stick.threshold)
                                    return f < 0 ? -1 : 1;
                                break;
                            case InputHoldType.VRTrigger:
                                f = Assets.Script.MainScript.localPerson.hand[0].data.trigger.Value;
                                if (f > Assets.Script.MainScript.localPerson.hand[0].data.trigger.threshold)
                                    return -1;
                                f = Assets.Script.MainScript.localPerson.hand[1].data.trigger.Value;
                                if (f > Assets.Script.MainScript.localPerson.hand[1].data.trigger.threshold)
                                    return 1;
                                break;
                        }
                    return 0;
                case InputTypes.GamePad:
                    if ((!Assets.Script.MainScript.multiPlayer) || (Player.bossId == Assets.Script.MainScript.localPerson.id))
                        return InputBasis.GamePadButton(hold, 0.5f);
                    return 0;
                default:
                    if ((!Assets.Script.MainScript.multiPlayer) || (Player.bossId == Assets.Script.MainScript.localPerson.id))
                    {
                        if (keyValue == null)
                        {
                            if (hold == InputHoldType.Key)
                            {
                                if (direction == InputDirections.MouseButton) return Assets.Script.MainScript.MouseButton;
                                if (direction == InputDirections.MouseWheel) return Assets.Script.MainScript.WheelDirection;
                            }
                        }
                        else
                        {
                            if (keyValue.Length == 1)
                            {
                                if (keyStatus[keyValue[0]])
                                {
                                    if (direction == InputDirections.MouseButton) return Assets.Script.MainScript.MouseButton;
                                    if (direction == InputDirections.MouseWheel) return Assets.Script.MainScript.WheelDirection;
                                }
                            }
                            else
                            {
                   //             Debug.Log(keyStatus.Length);
                                if (keyStatus[keyValue[0]]) return -1;
                                if (keyStatus[keyValue[1]]) return 1;
                            }
                        }
                    }
                    return 0;
            }
        }
    }
    public class TameMatch
    {
        public ManifestMatch manifest = null;
        List<GameObject> gameObjects = new List<GameObject>();
        List<GameObject> matched = new List<GameObject>();
        //public List<GameObject> mover = new List<GameObject>();
        List<Vector3> mposition = new List<Vector3>();
        List<Quaternion> mrotation = new List<Quaternion>();
        List<Vector3> gposition = new List<Vector3>();
        List<Quaternion> grotation = new List<Quaternion>();

        public void Match(List<TameGameObject> tgos, List<TameElement> tes)
        {
            float d, min;
            TameFinder finder = new TameFinder();
            finder.header = new ManifestHeader() { items = manifest.a };
            List<GameObject> ms = new List<GameObject>();
            finder.PopulateObjects(tgos);
            GameObject go = null;
            if (finder.objectList.Count > 0)
            {
                for (int i = 0; i < finder.objectList.Count; i++)
                    ms.Add(finder.objectList[i].gameObject);
                if (ms.Count > 0)
                {
                    finder.header.items = manifest.b;
                    finder.objectList.Clear();
                    finder.PopulateObjects(tgos);
                    if (finder.objectList.Count > 0)
                        for (int i = 0; i < finder.objectList.Count; i++)
                        {
                            matched.Add(finder.objectList[i].gameObject);
                            mposition.Add(matched[i].transform.localPosition);
                            mrotation.Add(matched[i].transform.localRotation);
                        }
                    for (int i = 0; i < matched.Count; i++)
                    {
                        min = float.PositiveInfinity;
                        foreach (GameObject obj in ms)
                        {
                            if ((d = Vector3.Distance(obj.transform.position, matched[i].transform.position)) < min)
                            {
                                d = min;
                                go = obj;
                            }
                        }
                        gameObjects.Add(go);
                        gposition.Add(go.transform.localPosition);
                        grotation.Add(go.transform.localRotation);
                    }
                }
            }
        }
        public void Update()
        {
            for (int i = 0; i < matched.Count; i++)
            {
                matched[i].transform.localPosition = mposition[i] + gameObjects[i].transform.localPosition - gposition[i];
                matched[i].transform.localRotation = mrotation[i] * (grotation[i] * Quaternion.Inverse(gameObjects[i].transform.localRotation));
            }
        }

    }
}
