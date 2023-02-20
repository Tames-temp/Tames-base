using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Tames
{
    public class ManifestCustom : ManifestBase
    {
        public float[] range = new float[] { 0, 1 };
        public List<TameInputControl> tics = new List<TameInputControl>();
        public static int Create(ManifestHeader header, string[] lines, int index, out TameCustomValue tcv)
        {
            ManifestCustom tcm = new ManifestCustom();
            int i = tcm.Read(lines, index);
            if (tcm.tics.Count > 0)
                tcv = new TameCustomValue()
                {
                    control = tcm.tics,
                    name = header.items[0],
                    range = tcm.range,
                    manifest = tcm
                };
            else
                tcv = null;
            return i;
        }
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float a, b;
            float[] f2;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                if (mh.key == TameKeys.None)
                {
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Input:
                            tics.AddRange(GetControl(mh, 0));
                            break;
                        case ManifestKeys.Factor:
                            if (mh.items.Count == 2)
                                if (Utils.SafeParse(mh.items[0], out a) && Utils.SafeParse(mh.items[1], out b))
                                    range = new float[] { a, b };
                            break;
                        default:
                            ReadShared(mh);
                            break;
                    }
                }
                else
                {
                    i--;
                    break;
                }
                i++;
            }
            return i;
        }
        public static List<TameInputControl> GetControl(ManifestHeader header, int start)
        {
            List<TameInputControl> r = new List<TameInputControl>();
            string[] list;
            int k1, k2;
            string s;
            for (int i = start; i < header.items.Count; i++)
            {
                s = header.items[i].ToLower();
                switch (s)
                {
                    case "grsx":
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRX }); break;
                    case "grsy":
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPSRY }); break;
                    case "gs":
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPShoulder }); break;
                    //game controller pad
                    case "gt":
                        Debug.Log("GT active");
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPTrigger }); break;
                    //game controller ya
                    case "gdx":
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDX }); break;
                    case "gdy":
                        r.Add(new TameInputControl() { control = InputTypes.GamePad, hold = InputHoldType.GPDY }); break;
                    //VR controller trigger 
                    case "vrt":
                        r.Add(new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRTrigger }); break;
                    //VR controller stick left
                    case "vrsl":
                        r.Add(new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollLeft }); break;
                    //VR controller stick right
                    case "vrsr":
                        r.Add(new TameInputControl() { control = InputTypes.VRController, hold = InputHoldType.VRScrollRight }); break;
                    //key mouse
                    default:
                        list = s.Split('+');
                        if (list.Length == 1)
                        {
                            if (list[0].Equals("button"))
                                r.Add(new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseButton });
                            if (list[0].Equals("wheel"))
                                r.Add(new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = null, direction = InputDirections.MouseWheel });
                        }
                        else if (list.Length >= 2)
                        {
                            k1 = FindKey(list[0]);
                            if (k1 >= 0)
                            {
                                if (list[1].Equals("button"))
                                    r.Add(new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1 }, direction = InputDirections.MouseButton });
                                else if (list[1].Equals("wheel"))
                                    r.Add(new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1 }, direction = InputDirections.MouseWheel });
                                else
                                {
                                    k2 = FindKey(list[1]);
                                    if ((k2 != k1) && (k2 >= 0))
                                        r.Add(new TameInputControl() { control = InputTypes.KeyboardMouse, hold = InputHoldType.Key, keyValue = new int[] { k1, k2 }, direction = InputDirections.Key });
                                }
                            }
                        }
                        break;
                }
            }
            return r;
        }
        public static int AddKey(UnityEngine.InputSystem.Controls.ButtonControl b)
        {
            for (int i = 0; i < TameInputControl.checkedKeys.Count; i++)
                if (TameInputControl.checkedKeys[i] == b)
                    return i;
            TameInputControl.checkedKeys.Add(b);
            return TameInputControl.checkedKeys.Count - 1;
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
                case "space": return AddKey(Keyboard.current.spaceKey);
                case "left": return AddKey(Keyboard.current.leftArrowKey);
                case "right": return AddKey(Keyboard.current.rightArrowKey);
                case "up": return AddKey(Keyboard.current.upArrowKey);
                case "down": return AddKey(Keyboard.current.downArrowKey);
                default: return -1;
            }
        }
    }
}
