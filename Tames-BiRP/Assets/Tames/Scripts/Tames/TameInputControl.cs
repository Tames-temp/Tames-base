using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Multi;

namespace Tames
{
    public enum InputTypes
    {
        VRController,
        GamePad,
        KeyboardMouse,
        None,
        Error
    }
    public enum InputDirections
    {
        MouseButton = 1,
        MouseWheel = 2,
        Key = 0
    }
    public enum InputControlHold
    {
        Ctrl, Shift, Alt,
        GTL, GTR, GTBoth,
        VRTL, VRTR, VRTBoth,
        None
    }
    public enum InputHoldType
    {
        // none
        None, Error,
        // control
        // single
        Key, GDXL, GDXR, GDYU, GDYD, GA, GB, GX, GY, GSL, GSR,
        // dual
        Vertical, Horizontal,
        Button,
        GDX, GDY, GS, GYA, GXB,


        VRScrollLeft,
        VRScrollRight,
        VRTrigger,
    }
    public static class InputBasis
    {
        public static TrackedPoseDriver driver;
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
            if (driver != null)
            {
                driver.enabled = move == VR || turn == VR;
                if (move == VR && turn == VR)
                    driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                else if (turn == VR) driver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
            }
        }

    }
    public class TameInputControl
    {
        public const float Threshold = 0.3f;
        public static Records.TameKeyMap keyMap;
        //   public static bool[] keyStatus = null;
        public static List<ButtonControl> checkedKeys = new List<ButtonControl>();
        public InputTypes control;
        public InputHoldType hold;
        public InputControlHold aux;
        public InputDirections direction;
        public bool mono;
        public int[] keyValue;
        public static bool Pressed(int index)
        {
            int i = index % 100;
            int t = index / 100;
            switch (t)
            {
                case 0: return keyMap.pressed[i];
                case 1: return keyMap.mouse.pressed[i];
                case 2: return keyMap.gpMap.pressed[i];
                    //         case 3:return keyMap.vrMap.pressed[i];
            }
            return false;
        }
        public static bool Hold(int index)
        {
            int i = index % 100;
            int t = index / 100;
            switch (t)
            {
                case 0: return keyMap.hold[i];
                case 1: return keyMap.mouse.hold[i];
                case 2: return keyMap.gpMap.hold[i];
                    //         case 3:return keyMap.vrMap.hold[i];
            }
            return false;
        }
        public static Records.TameKeyMap CheckKeys(int index = -1)
        {
            if (keyMap == null)
                keyMap = new Records.TameKeyMap(checkedKeys.Count);
            //           if (keyStatus == null)
            //               keyStatus = new bool[checkedKeys.Count];
            if (index >= 0)
                keyMap = Records.TameFullRecord.allRecords.frame[index].keyMap;
            else
            {
                keyMap.Capture();
                return keyMap;
            } //       for (int i = 0; i < keyStatus.Length; i++)
            //           {
            //           keyStatus[i] = checkedKeys[i].isPressed;
            //       Debug.Log("custom key "+i+" "+keyStatus[i]+" "+checkedKeys[i].name);
            //        }
            return null;
        }
        public static InputControlHold GetHolder(string s, out InputTypes it)
        {
            switch (s)
            {
                case "shift": it = InputTypes.KeyboardMouse; return InputControlHold.Shift;
                case "ctrl": it = InputTypes.KeyboardMouse; return InputControlHold.Ctrl;
                case "alt": it = InputTypes.KeyboardMouse; return InputControlHold.Alt;
                case "gtl": it = InputTypes.GamePad; return InputControlHold.GTL;
                case "gtr": it = InputTypes.GamePad; return InputControlHold.GTR;
            }
            it = InputTypes.Error;
            return InputControlHold.None;
        }
        public static InputControlHold StringToHold(string s)
        {
            return s switch
            {
                "Ctrl" => InputControlHold.Ctrl,
                "Shift" => InputControlHold.Shift,
                "Alt" => InputControlHold.Alt,
                "GTL" => InputControlHold.GTL,
                "GTR" => InputControlHold.GTR,
                "GTBoth" => InputControlHold.GTBoth,
                "VRTL" => InputControlHold.VRTL,
                "VRTR" => InputControlHold.VRTR,
                "VRTBoth" => InputControlHold.VRTBoth,
                _ => InputControlHold.None,
            };
        }
        public static List<TameInputControl> AllMonos(Markers.InputSetting keys)
        {
            List<TameInputControl> r = new List<TameInputControl>();
            TameInputControl tci;
            if (keys.key != "")
            {
                tci = TameInputControl.StringToMono(keys.key, InputTypes.KeyboardMouse, keys.Aux(0));
                if (tci != null) r.Add(tci);
            }
            if (keys.gamepad != "")
            {
                tci = TameInputControl.StringToMono(keys.gamepad, InputTypes.GamePad, keys.Aux(1));
                if (tci != null) r.Add(tci);
            }
            if (keys.controller != "")
            {
                tci = TameInputControl.StringToMono(keys.controller, InputTypes.VRController, keys.Aux(2));
                if (tci != null) r.Add(tci);
            }
            return r;
        }
        public static void AllDuos(Markers.InputSetting keys, List<TameInputControl> back, List<TameInputControl> forth)
        {
            TameInputControl[] tcs;
            if (keys.key != "")
            {
                tcs = StringToMonos(keys.key, InputTypes.KeyboardMouse, keys.Aux(0));
                if (tcs != null) { back.Add(tcs[0]); forth.Add(tcs[1]); }
            }
            if (keys.gamepad != "")
            {
                tcs = StringToMonos(keys.gamepad, InputTypes.GamePad, keys.Aux(1));
                if (tcs != null) { back.Add(tcs[0]); forth.Add(tcs[1]); }
            }
            if (keys.controller != "")
            {
                tcs = StringToMonos(keys.controller, InputTypes.VRController, keys.Aux(2));
                if (tcs != null) { back.Add(tcs[0]); forth.Add(tcs[1]); }
            }
        }
        public static void AllDuos(Markers.InputSetting keys, List<TameInputControl> back)
        {
            TameInputControl tcs;
            if (keys.key != "")
            {
                Debug.Log(keys.key);
                tcs = StringToDuo(keys.key, InputTypes.KeyboardMouse, keys.Aux(0));
                if (tcs != null) { back.Add(tcs); }
            }
            if (keys.gamepad != "")
            {
                tcs = StringToDuo(keys.gamepad, InputTypes.GamePad, keys.Aux(1));
                if (tcs != null) { back.Add(tcs); }
            }
            if (keys.controller != "")
            {
                tcs = StringToDuo(keys.controller, InputTypes.VRController, keys.Aux(2));
                if (tcs != null) { back.Add(tcs); }
            }
        }
        public static TameInputControl[] StringToMonos(string S, InputTypes expectedType, InputControlHold iht)
        {
            string s = S.ToLower();
            TameInputControl pair = StringToDuo(s, expectedType, iht);
            if (pair != null)
            {
                TameInputControl[] r = new TameInputControl[2];
                switch (pair.control)
                {
                    case InputTypes.KeyboardMouse:
                        if (pair.hold == InputHoldType.Button)
                        {
                            r[0] = new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Button, keyValue = new int[] { -1 }, aux = iht };
                            r[1] = new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Button, keyValue = new int[] { 1 }, aux = iht };
                            return r;
                        }
                        else
                        {
                            r[0] = new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { pair.keyValue[0] }, aux = iht };
                            r[1] = new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { pair.keyValue[1] }, aux = iht };
                            return r;
                        }
                    case InputTypes.GamePad:
                        r[0] = new TameInputControl() { control = InputTypes.GamePad, aux = iht };
                        r[1] = new TameInputControl() { control = InputTypes.GamePad, aux = iht };
                        switch (pair.hold)
                        {
                            case InputHoldType.GYA: r[0].hold = InputHoldType.GY; r[1].hold = InputHoldType.GA; break;
                            case InputHoldType.GXB: r[0].hold = InputHoldType.GX; r[1].hold = InputHoldType.GB; break;
                            case InputHoldType.GS: r[0].hold = InputHoldType.GSL; r[1].hold = InputHoldType.GSR; break;
                            case InputHoldType.GDX: r[0].hold = InputHoldType.GDXL; r[1].hold = InputHoldType.GDXR; break;
                            case InputHoldType.GDY: r[0].hold = InputHoldType.GDYD; r[1].hold = InputHoldType.GDYU; break;
                            default: return null;
                        }
                        return r;
                }
            }
            return null;
        }
        public static TameInputControl StringToMono(string S, InputTypes expecteType, InputControlHold holder)
        {
            int k;
            string s = S.ToLower();
            if (expecteType == InputTypes.KeyboardMouse)
            {
                k = FindKey(s);
                if (k >= 0) return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k }, aux = holder };
            }
            else if (expecteType == InputTypes.GamePad)
                switch (s)
                {
                    case "ga": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GA, aux = holder };
                    case "gb": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GB, aux = holder };
                    case "gx": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GX, aux = holder };
                    case "gy": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GY, aux = holder };
                    case "gsl": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GSL, aux = holder };
                    case "gsr": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GSR, aux = holder };
                    case "gdxl":
                    case "gdx-": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDXL, aux = holder };
                    case "gdxr":
                    case "gdx+": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDXR, aux = holder };
                    case "gdyd":
                    case "gdy-": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDYD, aux = holder };
                    case "gdyu":
                    case "gdy+": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDYU, aux = holder };
                }
            return null;
        }

        public static TameInputControl StringToDuo(string S, InputTypes expectedType, InputControlHold holder)
        {
            int a = -1, b = -1;
            string s = S.ToLower();
            string[] comma = s.Split(',');
            if (expectedType == InputTypes.KeyboardMouse)
            {
                if (comma.Length == 2)
                {
                    a = FindKey(comma[0]);
                    b = FindKey(comma[1]);
                    if (a >= 0 && b >= 0)
                        return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { a, b }, aux = holder };
                }
                else
                {
                    if (s.StartsWith("vert")) { a = FindKey("down"); b = FindKey("up"); }
                    if (s.StartsWith("hor")) { a = FindKey("left"); b = FindKey("right"); }
                    if (a >= 0 && b >= 0)
                        return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { a, b }, aux = holder };
                    if ((s == "mouse") || (s == "button"))
                        return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Button, aux = holder };
                }
            }
            else if (expectedType == InputTypes.GamePad)
                switch (s)
                {
                    case "gya": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GYA, aux = holder };
                    case "gxb": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GXB, aux = holder };
                    case "gs": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GS, aux = holder };
                    case "gdx": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDX, aux = holder };
                    case "gdy": return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GDY, aux = holder };
                }
            return null;
        }


        public static int FindKey(string key, bool byUser = true)
        {
            if (!byUser)
            {
                switch (key)
                {
                    case "c": return AddKey(Keyboard.current.cKey);
                    case "x": return AddKey(Keyboard.current.xKey);
                    case "z": return AddKey(Keyboard.current.zKey);
                }
            }
            switch (key)
            {
                case "1": return AddKey(Keyboard.current.digit1Key);
                case "2": return AddKey(Keyboard.current.digit2Key);
                case "3": return AddKey(Keyboard.current.digit3Key);
                case "4": return AddKey(Keyboard.current.digit4Key);
                case "5": return AddKey(Keyboard.current.digit5Key);
                case "6": return AddKey(Keyboard.current.digit6Key);
                case "7": return AddKey(Keyboard.current.digit7Key);
                case "8": return AddKey(Keyboard.current.digit8Key);
                case "9": return AddKey(Keyboard.current.digit9Key);
                case "0": return AddKey(Keyboard.current.digit0Key);
                case "b": return AddKey(Keyboard.current.bKey);
                case "e": return AddKey(Keyboard.current.eKey);
                case "f": return AddKey(Keyboard.current.fKey);
                case "g": return AddKey(Keyboard.current.gKey);
                case "h": return AddKey(Keyboard.current.hKey);
                case "i": return AddKey(Keyboard.current.iKey);
                case "j": return AddKey(Keyboard.current.jKey);
                case "k": return AddKey(Keyboard.current.kKey);
                case "l": return AddKey(Keyboard.current.lKey);
                case "m": return AddKey(Keyboard.current.mKey);
                case "n": return AddKey(Keyboard.current.nKey);
                case "o": return AddKey(Keyboard.current.oKey);
                case "p": return AddKey(Keyboard.current.pKey);
                case "q": return AddKey(Keyboard.current.qKey);
                case "r": return AddKey(Keyboard.current.rKey);
                case "t": return AddKey(Keyboard.current.tKey);
                case "u": return AddKey(Keyboard.current.uKey);
                case "v": return AddKey(Keyboard.current.vKey);
                case "y": return AddKey(Keyboard.current.yKey);
                //    case "space": return AddKey(Keyboard.current.spaceKey);
                case "left": return AddKey(Keyboard.current.leftArrowKey);
                case "right": return AddKey(Keyboard.current.rightArrowKey);
                case "up": return AddKey(Keyboard.current.upArrowKey);
                case "down": return AddKey(Keyboard.current.downArrowKey);
                case "comma": return AddKey(Keyboard.current.commaKey);
                case "semi": return AddKey(Keyboard.current.semicolonKey);
                case "semicolon": return AddKey(Keyboard.current.semicolonKey);
                case "[": return AddKey(Keyboard.current.leftBracketKey);
                case "]": return AddKey(Keyboard.current.rightBracketKey);
                case "slash": return AddKey(Keyboard.current.slashKey);
                case "backslash": return AddKey(Keyboard.current.backslashKey);
                case "back": return AddKey(Keyboard.current.backslashKey);
                case "period": return AddKey(Keyboard.current.periodKey);
                case "dot": return AddKey(Keyboard.current.periodKey);
                case "=": return AddKey(Keyboard.current.equalsKey);
                case "-": return AddKey(Keyboard.current.minusKey);
                case "quote": return AddKey(Keyboard.current.quoteKey);
                case "n-": return AddKey(Keyboard.current.numpadMinusKey);
                case "n+": return AddKey(Keyboard.current.numpadPlusKey);
                case "n*": return AddKey(Keyboard.current.numpadMultiplyKey);
                case "n/": return AddKey(Keyboard.current.numpadDivideKey);
                case "enter": return AddKey(Keyboard.current.enterKey);
                //
                default: return -1;
            }
        }
        public static int AddKey(ButtonControl b)
        {
            for (int i = 0; i < checkedKeys.Count; i++)
                if (checkedKeys[i] == b)
                    return i;
            checkedKeys.Add(b);
            return checkedKeys.Count - 1;
        }
        public int Hold(Records.TameKeyMap keyMap)
        {
            float f;
            int k;
            switch (control)
            {
                case InputTypes.VRController:
                    switch (hold)
                    {

                        case InputHoldType.VRScrollLeft:
                            f = keyMap.vrMap.thumb[0];
                            if (Mathf.Abs(f) > Threshold) return f < 0 ? -1 : 1;
                            break;
                        case InputHoldType.VRScrollRight:
                            f = keyMap.vrMap.thumb[1];
                            if (Mathf.Abs(f) > Threshold) return f < 0 ? -1 : 1;
                            break;
                        case InputHoldType.VRTrigger:
                            k = keyMap.vrMap.trigger[0] > keyMap.vrMap.trigger[1] ? 0 : 1;
                            if (keyMap.vrMap.trigger[k] > Threshold) return k == 0 ? -1 : 1;
                            break;
                    }
                    return 0;
                case InputTypes.GamePad:
                    return keyMap.gpMap.ButtonHold(aux, hold, 0.5f);
                default:
                    {
                        if (!keyMap.AuxHold(aux))
                            return 0;

                        if (hold == InputHoldType.Button)
                            return keyMap.mouse.hold[0] ? -1 : (keyMap.mouse.hold[1] ? 1 : 0);

                        if (keyValue == null)
                            return 0;

                        if (keyMap.hold[keyValue[0]]) return -1;
                        if (keyMap.hold[keyValue[1]]) return 1;

                        return 0;
                    }
            }
        }
        public int Hold()
        {
            return Hold(keyMap);
        }

        public bool Pressed(Records.TameKeyMap keyMap)
        {
            float f;
            int k;
            switch (control)
            {
                case InputTypes.VRController:
                    switch (hold)
                    {

                        case InputHoldType.VRScrollLeft:
                            f = keyMap.vrMap.thumb[0];
                            if (Mathf.Abs(f) > Threshold) return true;
                            break;
                        case InputHoldType.VRScrollRight:
                            f = keyMap.vrMap.thumb[1];
                            if (Mathf.Abs(f) > Threshold) return true;
                            break;
                        case InputHoldType.VRTrigger:
                            k = keyMap.vrMap.trigger[0] > keyMap.vrMap.trigger[1] ? 0 : 1;
                            if (keyMap.vrMap.trigger[k] > Threshold) return true;
                            break;
                    }
                    return false;
                case InputTypes.GamePad:
                    return keyMap.gpMap.ButtonPressed(aux, hold, 0.5f);
                default:
                    {
                        if (!keyMap.AuxHold(aux)) return false;
                        if (hold == InputHoldType.Key)
                        {
                            if (keyValue == null) return false;
                            else return keyMap.pressed[keyValue[0]];
                        }
                        else
                        {
                   //         Debug.Log(keyMap.mouse.pressed[0]+" "+keyMap.mouse.pressed[1]);
                            if (keyValue == null) return false;
                            else if (keyValue[0] == -1) return keyMap.mouse.pressed[0];
                            else return keyMap.mouse.pressed[1];
                        }
                    }
            }
        }
        public bool Pressed()
        {
            return Pressed(keyMap);
        }
    }
}
