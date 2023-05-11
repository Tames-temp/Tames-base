using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
namespace Tames
{
    public class TameInputControl
    {
        public const float Threshold = 0.3f;
        public static Records.TameKeyMap keyMap;
        //   public static bool[] keyStatus = null;
        public static List<ButtonControl> checkedKeys = new List<ButtonControl>();
        public InputTypes control;
        public InputHoldType hold;
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
        public static int AddCtrl(ButtonControl b)
        {
            return -1;

        }
        public static TameInputControl ByStringMono(string S)
        {
            int k;
            string s = S.ToLower();
            switch (s)
            {
                case "grsx":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRX };
                case "grsy":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRY };
                case "gs":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPShoulder };
                //game controller pad
                case "gt":
                    Debug.Log("GT active");
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPTrigger };
                //game controller ya
                case "gdx":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDX };
                case "gdy":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDY };
                //VR controller trigger 
                case "vrt":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRTrigger };
                //VR controller stick left
                case "vrsl":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollLeft };           //VR controller stick right
                case "vrsr":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollRight };       //key mouse
                default:
                    string[] list = s.Split('+');
                    //  Debug.Log("MPM " + s + " " + list.Length);
                    if (list.Length == 1)
                    {
                        if (list[0].Equals("button"))
                            return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseButton };
                        else if (list[0].Equals("wheel"))
                            return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseWheel };
                        else
                        {
                            k = FindKey(list[0]);
                            //        if (k >= 0) Debug.Log("MPM " + k);
                            if (k >= 0)
                                return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k }, direction = InputDirections.Key };
                        }
                    }
                    else if (list.Length >= 2)
                    {
                        k = FindKey(list[0]);
                        if (k >= 0)
                        {
                            if (list[1].Equals("button"))
                                return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k }, direction = InputDirections.MouseButton };
                            else if (list[1].Equals("wheel"))
                                return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k }, direction = InputDirections.MouseWheel };
                        }
                    }

                    break;
            }
            return null;
        }
        public static TameInputControl ByStringDuo(string S)
        {
            string s = S.ToLower();
            switch (s)
            {
                case "grsx":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRX };
                case "grsy":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRY };
                case "gs":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPShoulder };
                //game controller pad
                case "gt":
                    Debug.Log("GT active");
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPTrigger };
                //game controller ya
                case "gdx":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDX };
                case "gdy":
                    return new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDY };
                //VR controller trigger 
                case "vrt":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRTrigger };
                //VR controller stick left
                case "vrsl":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollLeft };           //VR controller stick right
                case "vrsr":
                    return new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollRight };       //key mouse
                default:
                    string[] list = s.Split(',');
                    if (list.Length == 1)
                    {
                        if (list[0].Equals("button"))
                            return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseButton };
                        if (list[0].Equals("wheel"))
                            return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseWheel };
                    }
                    else if (list.Length >= 2)
                    {
                        //          Debug.Log("MANUL " + list[0]);
                        //        Debug.Log("MANUL " + list[1]);
                        int k1 = FindKey(list[0]);
                        if (k1 >= 0)
                        {
                            if (list[1].Equals("button"))
                                return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1 }, direction = InputDirections.MouseButton };
                            else if (list[1].Equals("wheel"))
                                return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1 }, direction = InputDirections.MouseWheel };
                            else
                            {
                                int k2 = FindKey(list[1]);
                                //           Debug.Log("MANUL " + k1 + " , "+k2);
                                if ((k2 != k1) && (k2 >= 0))
                                    return new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1, k2 }, direction = InputDirections.Key };
                            }
                        }
                    }
                    break;
            }
            return null;
        }
        public static int FindKey(string key)
        {
            switch (key)
            {
                case "ctrl": return AddKey(Keyboard.current.ctrlKey);
                case "shift": return AddKey(Keyboard.current.shiftKey);
                case "alt": return AddKey(Keyboard.current.altKey);
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
        public int Hold()
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
                    return InputBasis.GamePadButton(hold, 0.5f);
                default:
                    {
                        if (keyValue == null)
                        {
                            if (hold == InputHoldType.Key)
                                if (direction == InputDirections.MouseButton) return keyMap.mouse.hold[0] ? -1 : (keyMap.mouse.hold[1] ? 1 : 0);
                        }
                        else
                        {
                            if (keyValue.Length == 1)
                            {
                                if (keyMap.hold[keyValue[0]])
                                    if (direction == InputDirections.MouseButton) return keyMap.mouse.hold[0] ? -1 : (keyMap.hold[1] ? 1 : 0);
                            }
                            else
                            {
                                //             Debug.Log(keyStatus.Length);
                                if (keyMap.hold[keyValue[0]]) return -1;
                                if (keyMap.hold[keyValue[1]]) return 1;
                            }
                        }
                        return 0;
                    }
            }
        }
        public bool Pressed()
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
                    return InputBasis.GamePadButton(hold, 0.5f) != 0;
                default:
                    {
                        if (keyValue == null)
                        {
                            if (hold == InputHoldType.Key)
                                if (direction == InputDirections.MouseButton) return keyMap.mouse.pressed[0] || keyMap.mouse.pressed[1];
                        }
                        else
                        {
                            if (keyValue.Length == 1)
                                if (keyMap.pressed[keyValue[0]])
                                {
                                    if (direction == InputDirections.MouseButton) return keyMap.mouse.pressed[0] || keyMap.mouse.pressed[1];

                                    else return true;
                                }
                        }
                        return false;
                    }
            }
        }
    }
}
