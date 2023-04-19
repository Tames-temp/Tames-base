using Assets.Script;
using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;
using Tames;
namespace Multi
{
    public class ITameEffect
    {
        public static ITameEffect[] AllEffects;
        public static int EffectCount;
        public ushort tameIndex;
        public const byte Progress = 1;
        public const byte Position = 2;
        public const byte Error = 3;
        //   public byte effect;
        public byte parent;
        public float progress;
        public Vector3 position;
        public bool forcedDirectionThisFrame = false;
        public int newDirection = 0;
        public int forcedArea = -1;
        public void AddToMessage(Message m)
        {
            //     m.AddByte(effect);
            m.AddByte(parent);
            m.AddFloat(progress);
            m.AddVector3(position);
        }
        public void Write(Message m)
        {
            m.AddUShort(tameIndex);
            //   m.AddByte(effect);
            m.AddByte(parent);
            m.AddFloat(progress);
            m.AddVector3(position);
        }
        public static void Initialize()
        {
            for (int i = 0; i < AllEffects.Length; i++)
                AllEffects[i] = new ITameEffect();
        }
       
        public void Apply(List<TameElement> tes)
        {
            TameElement te = tes[tameIndex];
            if(forcedDirectionThisFrame)
            {
                TameObject to = (TameObject)te;
                to.areas[forcedArea].switchDirection = newDirection;
                to.areas[forcedArea].forcedSwitchThisFrame = TameElement.Tick;
                forcedDirectionThisFrame = false;
            }
            if (parent == Progress) te.Update(progress); else te.Update(position);
        }
        public void Set(TameEffect te)
        {
            tameIndex = te.child.index;
            parent = (te.type == TrackBasis.Object) || (te.type == TrackBasis.Head) || (te.type == TrackBasis.Hand) ? Position : Progress;
            if (parent == Progress)
            {
                if (te.child.progress != null) progress = te.child.progress.progress;
            }
            else
                position = te.position;
        }
    }
}