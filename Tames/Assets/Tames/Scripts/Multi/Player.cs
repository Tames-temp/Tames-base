
using Multi;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using Tames;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
   public static Records.FrameShot[] frames = new Records.FrameShot[CoreTame.people.Length];
    public static ushort Id = (ushort)255;
    public static ushort index = (ushort)255;
    public static bool assigned = false;
    public const ushort FrameData = 1;
    public const ushort IDAssigned = 2;
    public const ushort Name = 3;
    public static bool isServer = false;
    private string username;
    //   public static GameObject go = null;
    private static bool WaitForMessage = false;

    public static int Index(ushort id)
    {
        for (int i = 0; i < CoreTame.people.Length; i++)
            if (CoreTame.people[i] != null)
                if (CoreTame.people[i].id == id)
                    return i;
        return -1;
    }

    [MessageHandler(IDAssigned)]
    private static void AssignIndex(Message mr)
    {
        Id = mr.GetUShort();
        index = mr.GetByte();
        Person.people[index] = Person.localPerson;
        assigned = true;
    }
    [MessageHandler(FrameData)]
    private static void ReceiveFrame(Message m)
    { 
        float time = m.GetFloat();
        for (int i = 0; i < frames.Length; i++)
            if (m.GetBool())
            {
                frames[i] = new Records.FrameShot();
                frames[i].cpos = m.GetVector3();
                frames[i].crot = m.GetQuaternion();
                frames[i].hpos[0] = m.GetVector3();
                frames[i].hrot[0] = m.GetQuaternion();
                frames[i].hpos[1] = m.GetVector3();
                frames[i].hrot[1] = m.GetQuaternion();
                frames[i].grip[0] = m.GetFloat();
                frames[i].grip[1] = m.GetFloat();
                frames[i].KBPressed = m.GetULong();
                frames[i].KBHold = m.GetULong();
                frames[i].mouse = m.GetUInt();
                frames[i].GPPressed = m.GetULong();
                frames[i].GPHold = m.GetULong();
                frames[i].VRPressed = m.GetUInt();
                frames[i].VRHold = m.GetUInt();
            }
            else frames[i] = null;
        Person.UpdateAll(frames);
    }
    public static void SendFrame(int index, Records.FrameShot frame)
    {
        Message m = Message.Create(MessageSendMode.unreliable, FrameData);
        m.AddFloat(frame.time);
        m.AddInt(index);
        m.AddVector3(frame.cpos);
        m.AddQuaternion(frame.crot);
        m.AddVector3(frame.hpos[0]);
        m.AddQuaternion(frame.hrot[0]);
        m.AddVector3(frame.hpos[1]);
        m.AddQuaternion(frame.hrot[1]);
        m.AddFloat(frame.grip[0]);
        m.AddFloat(frame.grip[1]);
        m.AddULong(frame.KBPressed);
        m.AddULong(frame.KBHold);
        m.AddUInt(frame.mouse);
        m.AddULong(frame.GPPressed);
        m.AddULong(frame.GPHold);
        m.AddUInt(frame.VRPressed);
        m.AddUInt(frame.VRHold);
        NetworkManager.Singleton.Client.Send(m);
    }
}
