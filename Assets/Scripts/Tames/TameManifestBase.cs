using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tames
{
    public class TameManifestBase
    {
        /// <summary>
        /// the manifest header of this manifest
        /// </summary>
        public ManifestHeader header;
        public TameTrigger trigger = null;
        public ManifestHeader inputHeader = null;
        public float setTo = 0;
        public float initial = 0;
        public TameDurationManager manager = new TameDurationManager();
        public string mgrParent = "";
        public bool independent = false;
        public CycleTypes cycle = CycleTypes.Stop;
        public ManifestHeader updates = null;
        public List<TameArea> areas = new List<TameArea>();
        public List<TameElement> elements = new List<TameElement>();

        public List<string> linked = new List<string>();
        public List<float> linkedOffset = new List<float>();
        public List<LinkedKeys> linkedTypes = new List<LinkedKeys>();
        public float progressedDistance;
        public LinkedKeys linkType = LinkedKeys.None;
        public List<string> scaledObjects = new List<string>();
        public float scaleFrom, scaleTo, scaleMaterial;
        public bool scales = false;
        public int scaleAxis = -1;
        public int scaleUV = 0;
        public bool queued = false;
        public int queueCount = -1;
        public float queueStart;
        public float queueInterval = -1;
        public int queueUV = -1;
        public byte updateType = TrackBasis.Time;
        public static int Read(ManifestHeader mh, string[] lines, int i, List<TameManifestBase> items)
        {
            TameMaterialManifest tmm;
            TameLightManifest tlm;
         //   Debug.Log("mb: "+(mh.items.Count>0?mh.items[0]:""));
            switch (mh.key)
            {
                case TameKeys.Object:
                    TameObjectManifest tom = new TameObjectManifest() { header = mh };
                    items.Add(tom);
                    return tom.Read(lines, i);
                case TameKeys.Material:
                    Debug.Log("mat name: " + mh.items[0]);
                    tmm = new TameMaterialManifest() { header = mh };
                    items.Add(tmm);
                    return tmm.Read(lines, i);
                case TameKeys.Light:
                    tlm = new TameLightManifest() { header = mh };
                    items.Add(tlm);
                    return tlm.Read(lines, i);
                default:
                    return i;
            }
        }
        public void ReadShared(ManifestHeader mh)
        {
            float f;
            float[] f2;
            int c;
            string i0 = mh.items.Count > 0 ? mh.items[0].ToLower() : "";
            switch (mh.subKey)
            {
                case ManifestKeys.Speed:
                case ManifestKeys.Duration:
                    ReadDuration(mh);
                    break;
                case ManifestKeys.Trigger:
                    trigger = ReadTrigger(mh);
                    break;
                case ManifestKeys.Cycle:
                case ManifestKeys.Reverse:
                case ManifestKeys.Stop:
                    ReadCycle(mh);
                    break;
                case ManifestKeys.Input:
                    inputHeader = mh;
                    break;
                case ManifestKeys.Set:
                    if (Utils.SafeParse(i0, out f)) setTo = f;
                    break;
                case ManifestKeys.Initial:
                    if (Utils.SafeParse(i0, out f)) initial = f;
                    break;
            }
        }
        public void ReadDuration(ManifestHeader mh)
        {
            float f;
            if (mh.subKey == ManifestKeys.Speed)
            {
                if (Utils.SafeParse(mh.items[0], out f))
                    manager.Speed = manager.offset = f;
                if (mh.items.Count > 2)
                {
                    manager.speedBased = true;
                    if (Utils.SafeParse(mh.items[1], out f))
                        if (f > 0)
                        {
                            manager.factor = f;
                            string s = mh.items[2];
                            for (int i = 3; i < mh.items.Count; i++)
                                s += " " + mh.items[i];
                            mgrParent = s;
                            Debug.Log("dur: " + manager.offset + " " + manager.factor + " " + s);
                        }
                }
            }
            else
            {
                if (Utils.SafeParse(mh.items[0], out f)) manager.Duration = manager.offset = f;
                manager.speedBased = false;
                if (mh.items.Count > 2)
                {
                    if (Utils.SafeParse(mh.items[1], out f))
                        if (f > 0)
                        {
                            manager.factor = f;
                            string s = mh.items[2];
                            for (int i = 3; i < mh.items.Count; i++)
                                s += " " + mh.items[i];
                            mgrParent = s;
                            Debug.Log("dur: " + manager.offset + " " + manager.factor + " " + s);
                        }
                }

            }
        }
        public TameCustomValue CreateInput(string name)
        {

            TameCustomValue tcv = new TameCustomValue() { name = name };
            float dur;
            int start = 0;
            if (inputHeader.items.Count >= 1)
            {
                if (Utils.SafeParse(inputHeader.items[0], out dur))
                {
                    start = 1;
                    manager.Duration = dur;
                    mgrParent = "";
                }
            }
            else
                return null;
            tcv.progress = new TameProgress(tcv)
            {
                cycle = CycleTypes.Cycle
            };
            tcv.progress.manager = manager;
            tcv.control = TameCustomManifest.GetControl(inputHeader, start);
            if (tcv.control.Count > 0)
            {
                foreach (TameElement te in elements)
                {
                    te.basis = TrackBasis.Tame;
                    te.parents.Clear();
                    te.parents.Add(new TameEffect() { child = te, direction = 1, type = TrackBasis.Tame, parent = tcv });
                    te.progress = new TameProgress(te);
                }
                //      Debug.Log("input " + tcv.name);
                return tcv;
            }
            return null;

        }
        private TameTrigger ReadTrigger(ManifestHeader mh)
        {
            TameTrigger tr;
            if (mh.items.Count == 0)
                return null;
            string s = mh.items[0];
            if (mh.items.Count >= 2)
                s += " " + mh.items[1];
            short[] sign = new short[3];
            float[] value = new float[2];
            bool mono = true;
            int si = 0;
            int vi = 0;
            int p;
            string sv = "";
            if ("-+".IndexOf(s[0]) < 0)
                s = " " + s;
            if ("-+".IndexOf(s[s.Length - 1]) < 0)
                s += " ";
            float f;
            int[] pos = new int[3];
            for (int i = 0; i < s.Length; i++)
                if ((p = "-  +".IndexOf(s[i])) >= 0)
                {
                    pos[si] = i;
                    sign[si] = (short)(p - 1);
                    si++;
                    if (si == 3) break;
                }
            if (si > 1)
            {
                sv = s.Substring(pos[0] + 1, pos[1] - pos[0] - 1);
                if (Utils.SafeParse(sv, out f)) value[0] = f;
                else
                    return null;
                if (si > 2)
                {
                    mono = false;
                    sv = s.Substring(pos[1] + 1, pos[2] - pos[1] - 1);
                    if (Utils.SafeParse(sv, out f)) value[1] = f;
                    else
                        return null;
                }
            }
            else return null;
            tr = new TameTrigger()
            {
                mono = mono,
                value = value,
                sign = sign
            };
            return tr;
        }

        private void ReadCycle(ManifestHeader mh)
        {
            // CycleTypes[] cycle = new CycleTypes[2];
            float f;
            cycle = mh.subKey switch
            {
                ManifestKeys.Cycle => CycleTypes.Cycle,
                ManifestKeys.Reverse => CycleTypes.Reverse,
                _ => CycleTypes.Stop
            };
            if (mh.items.Count >= 1)
            {
                if (Utils.SafeParse(mh.items[0], out f))
                {
                    manager.Duration = manager.offset = f;
                    manager.speedBased = false;
                    mgrParent = "";
                    if (mh.items.Count > 2)
                    {
                        if (Utils.SafeParse(mh.items[1], out f))
                        {
                            manager.factor = f;
                            string s = mh.items[2];
                            for (int i = 3; i < mh.items.Count; i++)
                                s += " " + mh.items[i];
                            mgrParent = s;
                        }
                    }
                }
            }
        }
    }
    public class TameMatchManifest 
    {
        public List<string> a = new List<string>();
        public List<string> b = new List<string>();
        public static int Read(string[] lines, int index, List<TameMatch> tms)
        {
            string s = lines[index];
            List<string> ss = new List<string>();
            bool txt = false,  first = true, afinished = false;
            int bstarts = -1;
            string tmp = "";
            string clean;
           List<string> a = new List<string>();
       List<string> b = new List<string>();
        TameMatchManifest tmm = new TameMatchManifest();
            for (int i = 0; i < s.Length; i++)
            {
                if (" \t".IndexOf(s[i]) >= 0)
                { if (!first) tmp += s[i]; }
                else if (s[i] == ',')
                {
                    if (txt)
                    {
                        txt = false;
                        ss.Add(tmp);
                    }
                }
                else if (s[i] == ';')
                {
                    if (afinished) break;
                    else
                    {
                        afinished = true;
                        bstarts = ss.Count;
                        if (txt)
                            ss.Add(tmp);
                    }
                }
                else tmp += s[i];
            }
            if(bstarts > 0)
            {
                for(int i = 0; i < bstarts; i++)
                {
                    clean=Utils.Clean(ss[i]);
                    if(clean.Length>0)
                        a.Add(clean);
                }
                if(a.Count > 0)
                {
                    for (int i = bstarts; i < ss.Count; i++)
                    {
                        clean = Utils.Clean(ss[i]);
                        if (clean.Length > 0)
                            b.Add(clean);
                    }
                }
                if (b.Count == 0)
                    a.Clear();
            }
            if(b.Count>0)
            {
                tmm.a = a;
                tmm.b = b;
                TameMatch tm = new TameMatch() { manifest=tmm};
                tms.Add(tm);
            }
            return index;
        }
    }
    public class TameObjectManifest : TameManifestBase
    {
        public List<string> cues = new List<string>();
        //     7/25 12:40
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float f;
            float[] f2;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
               // Debug.Log(mh.key+" object prop: " + mh.header + " >> " + lines[i]);
                if (mh.key == TameKeys.None)
                {
               //     Debug.Log(" subkey: " + mh.header +" "+ mh.subKey);
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Update: updates = mh; updateType = TrackBasis.Tame; break;
                        case ManifestKeys.Follow: updates = mh; updateType = TrackBasis.Mover; break;
                        case ManifestKeys.Track: updates = mh; updateType = TrackBasis.Object;
                      //      Debug.Log("tracked");
                            break;
                        case ManifestKeys.Area:
                            cues.AddRange(mh.items);
                            break;
                        case ManifestKeys.Queue: ReadQueue(mh); break;
                        case ManifestKeys.Linked:
                        case ManifestKeys.Clone: ReadLink(mh); break;
                        case ManifestKeys.Scale: ReadScale(mh); break;
                        default: ReadShared(mh); break;
                    }
                }
                else
                {
                    i--;
                    break;
                }
                i++;
            }
            Debug.Log("update " + updateType);
            return i;
        }
        private void ReadQueue(ManifestHeader mh)
        {
            float start;
            float by;
            int count = 10;
            bool isCount;
            Debug.Log("queue: " + mh.header + ManifestKeys.keys[ManifestKeys.By - 1].alias[0]);

            if (mh.items.Count < 3)
                return;
            if (Utils.SafeParse(mh.items[0], out start))
            {
                if (Utils.SafeParse(mh.items[2], out by))
                {
                    if (isCount = ManifestKeys.keys[ ManifestKeys.Count - 1].Has(mh.items[1]))
                        count = (int)by;
                    if (isCount || ManifestKeys.keys[ ManifestKeys.By - 1].Has(mh.items[1]))
                    {
                        queued = true;
                        queueCount = isCount ? count : -1;
                        queueInterval = by;
                        queueStart = start;
                        if (mh.items.Count > 3)
                            if ("uxUX".IndexOf(mh.items[3]) >= 0) queueUV = 0; else if ("vyVY".IndexOf(mh.items[3]) >= 0) queueUV = 1;
                    }
                }
            }
        }
        private void ReadScale(ManifestHeader mh)
        {
            if (mh.items.Count < 4)
                return;
            int axis = mh.items[0].ToLower() switch
            {
                "x" => 0,
                "y" => 1,
                "z" => 2,
                _ => -1
            };
            if (axis < 0) return;

            string[] sp = mh.items[1].Split(',');
            if (sp.Length < 2) return;
            if (!(Utils.SafeParse(sp[0], out scaleFrom) && Utils.SafeParse(sp[1], out scaleTo))) return;
            if (mh.items[2].ToLower().Equals("x")) scaleUV = 0; else scaleUV = 1;
            string s = "";
            for (int i = 3; i < mh.items.Count; i++)
                s += mh.items[i] + " ";
            string[] so = s.Split(',');
            for (int i = 0; i < so.Length; i++)
                scaledObjects.Add(Utils.Clean(so[i]));
            scaleAxis = axis;
            scales = true;
        }
        private void ReadLink(ManifestHeader mh)
        {
            float f = 0;
            int start = 2;
            int k;
            linkType = LinkedKeys.None;
            if (mh.subKey == ManifestKeys.Clone)
            {
                linkType = LinkedKeys.Clone;
                start = 0;
            }
            else if (mh.items.Count > 1)
            {
                //    Debug.Log("item0 = " + mh.items[0]);
                k = ManifestKeys.GetKey(mh.items[0].ToLower());
                switch (k)
                {
                    case ManifestKeys.Local:
                        start = 1;
                        linkType = LinkedKeys.Local;
                        break;
                    case ManifestKeys.Ratio:
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Progress;
                        break;
                    case ManifestKeys.Stack:
                        //     Debug.Log("is stack");
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Stack;
                        break;
                    case ManifestKeys.Cycle:
                        if (Utils.SafeParse(mh.items[1], out progressedDistance))
                            linkType = LinkedKeys.Cycle;
                        break;
                }
            }
            if (linkType != LinkedKeys.None)
            {
                string s = "";
                for (int i = start; i < mh.items.Count; i++)
                    s += " " + mh.items[i];
                s = Utils.Clean(s);
                string[] a = s.Split(',');
                int added = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    linked.Add(Utils.Clean(a[i]));
                    linkedOffset.Add(f);
                    linkedTypes.Add(linkType);
                    added++;
                }
                if (linked.Count == 0)
                    linkType = LinkedKeys.None;
            }
        }

    }
    public class TameMaterialManifest : TameManifestBase
    {
        public bool unique = false;
        public List<TameChanger> properties = new List<TameChanger>();
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float f;
            float[] f2;
            TameChanger tc;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                if (mh.key == TameKeys.None)
                {
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Update:
                            //            Debug.Log("update " + mh.header);
                            updateType = TrackBasis.Tame;
                            updates = mh; break;
                        case ManifestKeys.Color:
                            tc = TameColor.Read(mh, false);
                       //     Debug.Log("chor "+mh.items[1] + (tc==null?" null":" not"));
                            if (tc != null)
                                properties.Add(tc);
                            break;

                        case ManifestKeys.Glow:
                            tc = TameColor.Read(mh, true);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Glow;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.MapX:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                 tc.property = MaterialProperty.MapX;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.MapY:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.MapY;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.LightX:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.LightX;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.LightY:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.LightY;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.Bright:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Bright;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.Unique:
                            unique = true;
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

    }
    public class TameLightManifest : TameManifestBase
    {
        public List<TameChanger> properties = new List<TameChanger>();
        public int Read(string[] lines, int index)
        {
            int i = index + 1;
            ManifestHeader mh;
            float f;
            float[] f2;
            TameChanger tc;
            while (i < lines.Length)
            {
                mh = ManifestHeader.Read(lines[i]);
                if (mh.key == TameKeys.None)
                {
                    switch (mh.subKey)
                    {
                        case ManifestKeys.Update:
                            //            Debug.Log("update " + mh.header);
                            updates = mh;
                            updateType = TrackBasis.Tame;
                            break;
                        case ManifestKeys.Color:
                        case ManifestKeys.Glow:
                            tc = TameColor.Read(mh, true);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Glow;
                                properties.Add(tc);
                            }
                            break;

                        case ManifestKeys.Bright:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Bright;
                                properties.Add(tc);
                            }
                            break;
                        case ManifestKeys.Focus:
                            tc = TameChanger.Read(mh, 1);
                            if (tc != null)
                            {
                                tc.property = MaterialProperty.Focus;
                                properties.Add(tc);
                            }
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
    }
    public class TameCustomManifest : TameManifestBase
    {
        public float[] range = new float[] { 0, 1 };
        public List<TameInputControl> tics = new List<TameInputControl>();
        public static int Create(ManifestHeader header, string[] lines, int index, out TameCustomValue tcv)
        {
            TameCustomManifest tcm = new TameCustomManifest();
            int i = tcm.Read(lines, index);
            if (tcm.tics.Count > 0)
                tcv = new TameCustomValue()
                {
                    control = tcm.tics,
                    name = header.items[0],
                    range = tcm.range,
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