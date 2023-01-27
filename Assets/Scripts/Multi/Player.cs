using Assets.Script;
using Multi;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using Tames;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public static ushort bossId = 255;
    public static ushort Id = (ushort)255;
    private float x = 0, y = 0;
    public bool IsLocal { get; private set; }

    private string username;
    //   public static GameObject go = null;
    private static bool WaitForMessage = false;

    public static int Index(ushort id)
    {
        for (int i = 0; i < MainScript.people.Length; i++)
            if (MainScript.people[i] != null)
                if (MainScript.people[i].id == id)
                    return i;
        return -1;
    }

    [MessageHandler((ushort)ServerToClientId.playerLeft)]
    private static void PlayerLeft(Message mr)
    {
        ushort id = mr.GetUShort();
        bossId = mr.GetUShort();
        int i = Index(id);
        if (i >= 0)
        {
            Destroy(MainScript.people[i].head);
            Destroy(MainScript.people[i].hand[0].wrist);
            Destroy(MainScript.people[i].hand[1].wrist);
            MainScript.people[i] = null;
        }
    }
    [MessageHandler((ushort)ServerToClientId.initiateSelf)]
    private static void MakeSelf(Message mr)
    {
        int index = mr.GetInt();
        ushort id = Id = mr.GetUShort();
        bool boss = mr.GetBool();
        if (boss) bossId = id;
        MainScript.localPerson.id = Id;
        MainScript.people[index] = MainScript.localPerson;
        MainScript.localPerson.initiated = true;
        MainScript.localPerson.isLocal = true;
        NetworkManager.Singleton.Client.Send(Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.personInitiated));
    }
    [MessageHandler((ushort)ServerToClientId.addPerson)]
    private static void AddPerson(Message mr)
    {
        int count = mr.GetInt();
        ushort id;
        int index;
        for (int i = 0; i < count; i++)
        {
            index = mr.GetInt();
            id = mr.GetUShort();
            bool isBoss = mr.GetBool();
            if (id != Id)
            {
                MainScript.people[index] = new Multi.Person(id) { isLocal = false };
                MainScript.people[index].CreateModel(MainScript.fingerHeader);
                MainScript.people[index].initiated = true;
                if (isBoss)
                    bossId = id;
            }
        }
    }
    // a request to send interactive data to the server
    [MessageHandler((ushort)ServerToClientId.requestInteractives)]
    private static void SendInteractives(Message mr)
    {
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.listInteractives);
        m.AddInt(MainScript.tes.Count);
        for (int i = 0; i < MainScript.tes.Count; i++)
        {
            m.AddVector3(MainScript.ies[i].position);
        }
        NetworkManager.Singleton.Client.Send(m);
    }
    // setting the player as the boss


    // receives update for all interactives
    [MessageHandler((ushort)ServerToClientId.updateInteractives)]
    private static void UpdateInteractives(Message mr)
    {
        mr.GetFloat();
        int count = ITameEffect.EffectCount = mr.GetInt();
        ushort id;
        for (int i = 0; i < count; i++)
        {
            ITameEffect.AllEffects[i].tameIndex = mr.GetUShort();
            //  ITameEffect.AllEffects[i].effect = mr.GetByte();
            ITameEffect.AllEffects[i].parent = mr.GetByte();
            ITameEffect.AllEffects[i].progress = mr.GetFloat();
            ITameEffect.AllEffects[i].position = mr.GetVector3();
        }
    }
    // receives update for all clients
    [MessageHandler((ushort)ServerToClientId.updatePeople)]
    private static void UpdateClients(Message m)
    {
        m.GetFloat();
        int index = Index(Id);
        for (int i = 0; i < MainScript.people.Length; i++)
        {
            bool initiated = m.GetBool();
            if (initiated)
                if ((MainScript.people[i] != null) && (i != index))
                {
                    MainScript.people[i].ReadMessage(m);
                }
                else
                    Person.Skip(m);
        }
    }
    [MessageHandler((ushort)ServerToClientId.directionChange)]
    private static void DirectionChange(Message m)
    {
        m.GetFloat();
        int index = m.GetInt();
        int dir = m.GetInt();
        int area = m.GetInt();
        MainScript.ies[index].forcedDirectionThisFrame = true;
        MainScript.ies[index].newDirection = dir;
        MainScript.ies[index].forcedArea = area;
    }

    public static void SendPersonUpdate()
    {
        Message m = Message.Create(MessageSendMode.unreliable, (ushort)ClientToServerId.updatePerson);
        m.AddFloat(Time.time);
        MainScript.localPerson.AddToMessage(m);
        NetworkManager.Singleton.Client.Send(m);
    }

    public static void BeginGrip(int index)
    {
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.beginGrip);
        m.AddFloat(Time.time);
        m.AddInt(index);
        NetworkManager.Singleton.Client.Send(m);
    }
    public static void EndGrip(int index)
    {
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.endGrip);
        m.AddFloat(Time.time);
        m.AddInt(index);
        NetworkManager.Singleton.Client.Send(m);
    }
    public static void UpdateInteractives()
    {
        Message m = Message.Create(MessageSendMode.unreliable, (ushort)ClientToServerId.updateInteractives);
        m.AddFloat(Time.time);
        m.AddInt(ITameEffect.EffectCount);
        for (int i = 0; i < ITameEffect.EffectCount; i++)
            MainScript.ies[i].Write(m);
        NetworkManager.Singleton.Client.Send(m);
    }
    public static void AskDirectionChange(int index, int direction, int area)
    {
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.directionChange);
        m.AddFloat(Time.time);
        m.AddInt(index);
        m.AddInt(direction);
        m.AddInt(area);
        NetworkManager.Singleton.Client.Send(m);
    }
}
