using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tames;
using System.IO;
using System;
using Assets.Script;
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
    }
    public class TamePersonRecord
    {
        public Vector3 position;
        public Quaternion rotation;
        public TameHandRecord[] hand;
        public void Write(BinaryWriter bin)
        {
            Utils.Write3(bin, position);
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
    }

  
    public class TameFrameRecord
    {
        public TameKeyMap keyMap;
        public TamePersonRecord[] person;
        public float time;
        public bool passed = false;
        public void Write(BinaryWriter bin)
        {
            bin.Write(time);
            keyMap.Write(bin);
            for (int i = 0; i < person.Length; i++)
                if (person[i] == null)
                    bin.Write(false);
                else
                {
                    bin.Write(true);
                    person[i].Write(bin);
                }
        }
        public static TameFrameRecord Read(BinaryReader bin)
        {
            TameFrameRecord record = new TameFrameRecord();
            record.time = bin.ReadSingle();
            record.keyMap = new TameKeyMap(bin.ReadInt32());
            record.keyMap.Read(bin);
            return record;
        }
        public void Unpress()
        {
            for(int i = 0; i < keyMap.keyCount; i++)
                keyMap.pressed[i] = false;
        }
    }
    public class TameFullRecord
    {
        public static TameFullRecord allRecords;
        public int personCount;
        public int elementCount;
        public List<TameElement> tes;
        public List<Multi.Person> persons;
        public List<TameFrameRecord> frame = new List<TameFrameRecord>();
        public TameFullRecord(List<TameElement> tes, List<Multi.Person> persons)
        {
            this.tes = tes;
            this.persons = persons;
        }
        public void Capture(float time)
        {
            TameFrameRecord fr = new TameFrameRecord()
            {
                time = time,
                person = new TamePersonRecord[persons.Count]
            };
            fr.keyMap = new TameKeyMap(TameInputControl.checkedKeys.Count);
            fr.keyMap.Capture();
            for (int i = 0; i < persons.Count; i++)
                if (persons[i] != null)
                {
                    fr.person[i] = new TamePersonRecord()
                    {
                        position = persons[i].headPosition,
                        rotation = persons[i].head.transform.localRotation,
                        hand = new TameHandRecord[2]
                    };
                    for (int j = 0; j < 2; j++)
                        if (fr.person[i].hand[j] != null)
                            fr.person[i].hand[j] = new TameHandRecord()
                            {
                                position = persons[i].position[j],
                                rotation = persons[i].hand[j].wrist.transform.localRotation,
                                grip = persons[i].hand[j].data.grip.Value
                            };
                        else
                            fr.person[i].hand[j] = null;
                }
                else
                    fr.person[i] = null;
            frame.Add(fr);
        }
        public bool Save(string url)
        {
            try
            {
                FileStream file = File.Create(url);
                BinaryWriter bin = new BinaryWriter(file);
                bin.Write(persons.Count);
                bin.Write(frame.Count);
                for (int i = 0; i < frame.Count; i++)
                {
                    frame[i].Write(bin);
                }
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
                int pc = bin.ReadInt32();

                string s;
                for (int i = 0; i < tes.Count; i++)
                {
                    s = bin.ReadString();
                    s = bin.ReadString();
                    bin.ReadBoolean();
                }
                int fc = bin.ReadInt32();
                for (int i = 0; i < fc; i++)
                {
                    frame.Add(TameFrameRecord.Read(bin));
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}