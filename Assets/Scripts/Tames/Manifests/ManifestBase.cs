using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tames
{
    public class ManifestBase
    {
        public List<TameChanger> properties = new List<TameChanger>();
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
        public TameTrigger forceTrigger = null;
        public List<string> affected = new List<string>();
        public bool initialStatus = true;
        public int switchingKey = -1;
        public static int Read(ManifestHeader mh, string[] lines, int i, List<ManifestBase> items)
        {
            ManifestMaterial tmm;
            ManifestLight tlm;
            //   Debug.Log("mb: "+(mh.items.Count>0?mh.items[0]:""));
            switch (mh.key)
            {
                case TameKeys.Object:
                    ManifestObject tom = new ManifestObject() { header = mh };
                    items.Add(tom);
                    return tom.Read(lines, i);
                case TameKeys.Material:
                    //        Debug.Log("mat name: " + mh.items[0]);
                    tmm = new ManifestMaterial() { header = mh };
                    items.Add(tmm);
                    return tmm.Read(lines, i);
                case TameKeys.Light:
                    tlm = new ManifestLight() { header = mh };
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
       //             Debug.Log("duration " + manager.Duration + " " + mh.items[0]);
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
                case ManifestKeys.Enforce:
                    Debug.Log("enforce");
                    ReadEnforce(mh, true);
                    break;
                case ManifestKeys.Affect:
                    //       Debug.Log("enforce");
                    ReadEnforce(mh, false);
                    break;
                case ManifestKeys.Enable:
                case ManifestKeys.Disable:
                    ReadEnable(mh, mh.subKey == ManifestKeys.Enable);
                    break;
            }
        }
        public void ReadEnable(ManifestHeader mh, bool isEnable)
        {
            if (mh.items.Count > 0)
                switchingKey = TameInputControl.FindKey(mh.items[0]);
            initialStatus = isEnable;
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
               //     manager.speedBased = true;
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
           //     manager.speedBased = false;
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
            tcv.control = ManifestCustom.GetControl(inputHeader, start);
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
        public static TameTrigger ReadTrigger(string s)
        {

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
            TameTrigger tr = new TameTrigger()
            {
                mono = mono,
                value = value,
                sign = sign
            };
            return tr;
        }

        private TameTrigger ReadTrigger(ManifestHeader mh)
        {
            TameTrigger tr;
            if (mh.items.Count == 0)
                return null;
            string s = mh.items[0];
            if (mh.items.Count >= 2)
                s += " " + mh.items[1];
            return ReadTrigger(s);
        }
        private void ReadEnforce(ManifestHeader mh, bool trig)
        {
            if (trig)
            {
                if (mh.items.Count > 1)
                {
                    forceTrigger = ReadTrigger(mh.items[0]);
                    //      Debug.Log("enforce: " + mh.items[0] + (forceTrigger == null ? ":null" : "?"));
                    if (forceTrigger != null)
                        for (int i = 1; i < mh.items.Count; i++)
                        {
                            affected.Add(mh.items[i]);
                            //                Debug.Log("enforce x: " + mh.items[i]);
                        }
                }
            }
            else
            {
                for (int i = 0; i < mh.items.Count; i++)
                {
                    affected.Add(mh.items[i]);
                    //          Debug.Log("enforce x: " + mh.items[i]);
                }
            }
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
              //      manager.speedBased = false;
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





}