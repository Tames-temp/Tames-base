using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tames;
using System.IO;
using System;
using Markers;
namespace Records
{

    public class TameRecord
    {
        public Vector3 position = Vector3.zero;
        public float progress;
        public float total;
        public void Write(BinaryWriter bin)
        {
            Utils.Write3(bin, position);
            bin.Write(progress);
            bin.Write(total);
        }
    }
    public class TameHandRecord
    {
        public Vector3 position;
        public Quaternion rotation;
        public float grip;
        public void Write(BinaryWriter bin)
        {
            Utils.Write3(bin, position);
            Utils.Write4(bin, rotation);
            bin.Write(grip);
        }
        public static TameHandRecord Read(BinaryReader bin)
        {
            if (bin.ReadBoolean())
            {
                TameHandRecord r = new();
                r.position = Utils.Read3(bin);
                r.rotation = Utils.Read4(bin);
                r.grip = bin.ReadSingle();
                return r;
            }
            else return null;
        }
    }
    public class TamePersonRecord
    {
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;
        public TameHandRecord[] hand;
        public void Write(BinaryWriter bin)
        {
            Vector3 p;
            Utils.Write3(bin, position);
            Utils.Write3(bin, p=rotation * Vector3.forward);
            Debug.Log(p.ToString());
            Utils.Write4(bin, rotation);
            for (int i = 0; i < hand.Length; i++)
                if (hand[i] != null)
                    bin.Write(false);
                else
                {
                    bin.Write(true);
                    hand[i].Write(bin);
                }
        }
        public static TamePersonRecord Read(BinaryReader bin)
        {
            TamePersonRecord r = new TamePersonRecord();
            r.position = Utils.Read3(bin);
            r.forward = Utils.Read3(bin);
            r.rotation = Utils.Read4(bin);
            r.hand = new TameHandRecord[2];
            for (int i = 0; i < r.hand.Length; i++)
                r.hand[i] = TameHandRecord.Read(bin);
            return r;
        }
    }
    public class TameFrameRecord
    {
        public TameKeyMap keyMap;
        public TamePersonRecord person;
        public float time;
        public bool passed = false;
        public void Write(BinaryWriter bin)
        {
            bin.Write(time);
            keyMap.Write(bin);
            person.Write(bin);
        }
        public static TameFrameRecord Read(BinaryReader bin, int kc)
        {
            TameFrameRecord record = new TameFrameRecord();
            record.time = bin.ReadSingle();
            record.keyMap = new TameKeyMap(kc);
            record.keyMap.Read(bin);
            record.person = TamePersonRecord.Read(bin);
            return record;
        }
        public void Unpress()
        {
            for (int i = 0; i < keyMap.keyCount; i++)
                keyMap.pressed[i] = false;
        }
    }
    public class TameFullRecord
    {
        public static TameFullRecord allRecords;
        public int personCount;
        public int elementCount;
        public string[] keyNames;
        public Multi.Person[] persons;
        public List<TameFrameRecord> frame = new List<TameFrameRecord>();
        public TameFullRecord(Multi.Person[] persons)
        {
            //    this.tes = tes;
            this.persons = persons;
        }
        public void Capture(float time, TameKeyMap km = null)
        {
            TameFrameRecord fr = new TameFrameRecord()
            {
                time = time,
            };
            if (km == null)
            {
                fr.keyMap = new TameKeyMap(TameInputControl.checkedKeys.Count);
                fr.keyMap.Capture();
            }
            else fr.keyMap = km;
            fr.person = new TamePersonRecord()
            {
                position = CoreTame.localPerson.headPosition,
                rotation = CoreTame.localPerson.headRotation,                
                hand = new TameHandRecord[2]
            };
            for (int j = 0; j < 2; j++)
                if (CoreTame.localPerson.hand[j] != null)
                    fr.person.hand[j] = new TameHandRecord()
                    {
                        position = CoreTame.localPerson.position[j],
                        rotation = CoreTame.localPerson.hand[j].wrist.transform.localRotation,
                        grip = CoreTame.localPerson.hand[j].data.grip.Value
                    };
                else
                    fr.person.hand[j] = null;
            frame.Add(fr);
        }
        public bool Save(string url)
        {
            try
            {
                FileStream file = File.Create(url);
                BinaryWriter bin = new BinaryWriter(file);
                bin.Write(persons.Length);
                bin.Write(frame.Count);
                bin.Write(TameInputControl.keyMap.keyCount);
                TameInputControl.keyMap.WriteDescription(bin);

                for (int i = 0; i < frame.Count; i++)
                    frame[i].Write(bin);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool Load(string url)
        {
            try
            {
                FileStream file = File.OpenRead(url);
                BinaryReader bin = new BinaryReader(file);
                Debug.Log(0);
                int pc = bin.ReadInt32();
                int fc = bin.ReadInt32();
                Debug.Log(1);
                int kcount = bin.ReadInt32();
                string s = bin.ReadString();
                keyNames = s.Split(',');
                Debug.Log(2);
                for (int i = 0; i < fc; i++)
                    frame.Add(TameFrameRecord.Read(bin, kcount));
                Debug.Log(3);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }
        private bool IsChanged(TameFrameRecord a, TameFrameRecord b, ExportOption eo)
        {
            if (a.person == null) return true;

            if (eo.headPosition) if (Vector3.Distance(a.person.position, b.person.position) > 0.0001) return true;
            if (eo.lookDirection) if (Vector3.Distance(a.person.forward, b.person.forward) > 0.0001) return true;
            for (int h = 0; h < (eo.bothHands ? 2 : 1); h++)
            {
                if ((a.person.hand[h] != null) && (b.person.hand[h] != null))
                {
                    if (eo.handPosition)
                        if (Vector3.Distance(a.person.hand[h].position, b.person.hand[h].position) > 0.0001) return true;
                    if (eo.handRotation)
                        if (Quaternion.Angle(a.person.hand[h].rotation, b.person.hand[h].rotation) > 0.001) return true;
                }
                if (eo.actionKeys)
                    if ((a.keyMap.UHold != b.keyMap.UHold) || (a.keyMap.UPressed != b.keyMap.UPressed)) return true;
                if (eo.actionMouse)
                    if (a.keyMap.mouse.U != b.keyMap.mouse.U) return true;
                if (eo.actionGamePad)
                    if (b.keyMap.gpMap.ChangedFrom(a.keyMap.gpMap)) return true;
                if (eo.actionVRController)
                    if (b.keyMap.vrMap.ChangedFrom(a.keyMap.vrMap)) return true;
            }
            return false;
        }
          public void ExportToCSV(string folder, ExportOption eo)
        {
             //  bool allPeople = false;
            List<string> lines = new();
            bool changed;
            TameFrameRecord f;
            TamePersonRecord p;
            string s;

            lines.Clear();

            Debug.Log("count = " + allRecords.frame.Count);
            // Header
            s = (eo.time ? "Time," : "") + (eo.headPosition ? "X,Y,Z," : "") + (eo.lookDirection ? "To X,To Y,To Z," : "");
            for (int h = 0; h < (eo.bothHands ? 2 : 1); h++)
                s += eo.handPosition ? "H" + h + " X," + "H" + h + " Y," + "H" + h + " Z," : "";
            for (int h = 0; h < (eo.bothHands ? 2 : 1); h++)
                s += eo.handRotation ? "R" + h + " X," + "R" + h + " Y," + "R" + h + " Z," + "R" + h + " W," : "";
            s += (eo.actionKeys ? "Aux,KeyPress,KeyHold," : "") + (eo.actionMouse ? "Mouse," : "") + (eo.actionGamePad ? "GAux,GPress,GHold," : "") + (eo.actionVRController ? "VR," : "");
            lines.Add(s);
            // Data
            for (int j = 0; j < allRecords.frame.Count; j++)
            {
                f = allRecords.frame[j];
                p = f.person;
                if (p == null) { lines.Add(""); continue; }

                if (!eo.onlyIfChanged) changed = true;
                else changed = j == 0 ? true : IsChanged(allRecords.frame[j - 1], allRecords.frame[j], eo);

                if (changed)
                {
                    s = "";
                    if (eo.time) s += f.time + ",";
                    if (eo.headPosition) s += p.position.x + "," + p.position.y + "," + p.position.z + ",";
                    if (eo.lookDirection) s += p.forward.x + "," + p.forward.y + "," + p.forward.z + ",";
                    for (int h = 0; h < (eo.bothHands ? 2 : 1); h++)
                    {
                        if (eo.handPosition)
                        {
                            if (p.hand[h] != null)
                                s += p.hand[h].position.x + "," + p.hand[h].position.y + "," + p.hand[h].position.z + ",";
                            else s += ",,,";
                        }
                        if (eo.handPosition)
                        {
                            if (p.hand[h] != null)
                                s += p.hand[h].rotation.x + "," + p.hand[h].rotation.y + "," + p.hand[h].rotation.z + "," + p.hand[h].rotation.z + ",";
                            else s += ",,,,";
                        }
                    }
                    if (eo.actionKeys)
                        s += f.keyMap.ExportAux()+","+ f.keyMap.Export(f.keyMap.pressed, keyNames) + "," + f.keyMap.Export(f.keyMap.hold, keyNames) + ",";
                    if (eo.actionMouse)
                        s += f.keyMap.mouse.Export(f.keyMap.mouse.pressed) + "," + f.keyMap.mouse.Export(f.keyMap.mouse.hold) + ",";
                    if (eo.actionGamePad)
                        s +=f.keyMap.gpMap.ExportAux()+","+ f.keyMap.gpMap.ExportStatus(f.keyMap.gpMap.pressed) + "," + f.keyMap.gpMap.ExportStatus(f.keyMap.gpMap.hold) + "," + f.keyMap.gpMap.ExportValue() + ",";
                    if (eo.actionVRController)
                        s += f.keyMap.vrMap.Export();

                    lines.Add(s);
                }
            }
            File.WriteAllLines(folder + "person local - " + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".csv", lines.ToArray());


        }
    }
}