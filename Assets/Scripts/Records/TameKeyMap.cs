using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Script;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Tames;

namespace Records
{
    public class VRMap
    {
        public float[] trigger;
        public float[] grip;
        public Vector2[] stick;
        public float[] thumb;
        public bool[] A;
        public bool[] B;
        public VRMap()
        {
            trigger = new float[] { 0, 0 };
            grip = new float[] { 0, 0 };
            stick = new Vector2[] { Vector2.zero, Vector2.zero };
            thumb = new float[] { 0, 0 };
            A = new bool[] { false, false };
            B = new bool[] { false, false };
        }
        public void Capture()
        {
            trigger[0] = MainScript.localPerson.hand[0].data.trigger.Value;
            trigger[1] = MainScript.localPerson.hand[1].data.trigger.Value;
            grip[0] = MainScript.localPerson.hand[0].data.grip.Value;
            grip[1] = MainScript.localPerson.hand[1].data.grip.Value;
            stick[0] = MainScript.localPerson.hand[0].data.stick.Vector;
            stick[1] = MainScript.localPerson.hand[1].data.stick.Vector;
            thumb[0] = MainScript.localPerson.hand[0].data.thumb.Value;
            thumb[1] = MainScript.localPerson.hand[1].data.thumb.Value;
            A[0] = MainScript.localPerson.hand[0].data.A.Pressed;
            A[1] = MainScript.localPerson.hand[1].data.A.Pressed;
            B[0] = MainScript.localPerson.hand[0].data.B.Pressed;
            B[1] = MainScript.localPerson.hand[1].data.B.Pressed;
        }
        public void Write(BinaryWriter bin)
        {
            bin.Write(trigger[0]);
            bin.Write(trigger[1]);
            bin.Write(grip[0]);
            bin.Write(grip[1]);
            Utils.Write2(bin, stick[0]);
            Utils.Write2(bin, stick[1]);
            bin.Write(thumb[0]);
            bin.Write(thumb[1]);
            bin.Write(A[0]);
            bin.Write(A[1]);
            bin.Write(B[0]);
            bin.Write(B[1]);
        }
        public void Read(BinaryReader bin)
        {
            trigger[0] = bin.ReadSingle();
            trigger[1] = bin.ReadSingle();
            grip[0] = bin.ReadSingle();
            grip[1] = bin.ReadSingle();
            stick[0] = Utils.Read2(bin);
            stick[1] = Utils.Read2(bin);
            thumb[0] = bin.ReadSingle();
            thumb[1] = bin.ReadSingle();
            A[0] = bin.ReadBoolean();
            A[1] = bin.ReadBoolean();
            B[0] = bin.ReadBoolean();
            B[1] = bin.ReadBoolean();

        }
    }
    public class GPMap
    {
        public bool[] pressed = new bool[8];
        public bool[] hold = new bool[8];
        public Vector2[] stick = new Vector2[] { Vector2.zero, Vector2.zero };
        public Vector2 dpad = Vector2.zero;
        public float[] trigger = new float[] { 0, 0 };
        public float[] shoulder = new float[] { 0, 0 };
        public GPMap()
        {
            for (int i = 0; i < 8; i++)
            {
                pressed[i] = false;
                hold[i] = false;
            }
        }
        public void Capture()
        {
            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                pressed[0] = gp.aButton.wasPressedThisFrame;
                pressed[1] = gp.bButton.wasPressedThisFrame;
                pressed[2] = gp.xButton.wasPressedThisFrame;
                pressed[3] = gp.yButton.wasPressedThisFrame;
                pressed[4] = gp.leftTrigger.wasPressedThisFrame;
                pressed[5] = gp.rightTrigger.wasPressedThisFrame;
                pressed[6] = gp.leftShoulder.wasPressedThisFrame;
                pressed[7] = gp.rightShoulder.wasPressedThisFrame;
                hold[0] = gp.aButton.isPressed;
                hold[1] = gp.bButton.isPressed;
                hold[2] = gp.xButton.isPressed;
                hold[3] = gp.yButton.isPressed;
                hold[4] = gp.leftTrigger.isPressed;
                hold[5] = gp.rightTrigger.isPressed;
                hold[6] = gp.leftShoulder.isPressed;
                hold[7] = gp.rightShoulder.isPressed;
                dpad = gp.dpad.ReadValue();
                stick[0] = gp.leftStick.ReadValue();
                stick[1] = gp.rightStick.ReadValue();
                trigger[0] = gp.leftTrigger.ReadValue();
                trigger[1] = gp.rightTrigger.ReadValue();
                shoulder[0] = gp.leftShoulder.ReadValue();
                shoulder[1] = gp.rightShoulder.ReadValue();
            }
        }
        public void Write(BinaryWriter bin)
        {
            for (int i = 0; i < 8; i++)
            {
                bin.Write(pressed[i]);
                bin.Write(hold[i]);
            }
            Utils.Write2(bin, dpad);
            for (int i = 0; i < 2; i++)
            {
                Utils.Write2(bin, stick[i]);
                bin.Write(trigger[i]);
                bin.Write(shoulder[i]);
            }
        }
        public void Read(BinaryReader bin)
        {
            for (int i = 0; i < 8; i++)
            {
                pressed[i] = bin.ReadBoolean();
                hold[i] = bin.ReadBoolean();
            }
            dpad = Utils.Read2(bin);
            for (int i = 0; i < 2; i++)
            {
                stick[i] = Utils.Read2(bin);
                trigger[i] = bin.ReadSingle();
                shoulder[i] = bin.ReadSingle();
            }
        }
    }
    public class TameKeyMap
    {
        public VRMap vrMap;
        public GPMap gpMap;
        public int keyCount;
        public bool[] pressed;
        public bool[] hold;
        public float[] values;
        public bool[] button;
        public float mouseY = 0;
        public bool forward = false;
        public bool back = false;
        public bool left = false;
        public bool right = false;
        public bool up = false;
        public bool down = false;

        bool passed = false;
        public TameKeyMap(int keyCount)
        {
            this.keyCount = keyCount;
            pressed = new bool[keyCount];
            hold = new bool[keyCount];
            vrMap = new VRMap();
            gpMap = new GPMap();
            button = new bool[] { false, false };
        }
        public void Capture()
        {
            if (Keyboard.current != null)
            {
                forward = Keyboard.current.wKey.isPressed;
                back = Keyboard.current.sKey.isPressed;
                left = Keyboard.current.aKey.isPressed;
                right = Keyboard.current.dKey.isPressed;
                up = Keyboard.current.rKey.isPressed;
                down = Keyboard.current.fKey.isPressed;
                for (int i = 0; i < keyCount; i++)
                {
                    pressed[i] = TameInputControl.checkedKeys[i].wasPressedThisFrame;
                    hold[i] = TameInputControl.checkedKeys[i].isPressed;
                }
            }
            if (Mouse.current != null)
            {
                button[0] = Mouse.current.leftButton.isPressed;
                button[1] = Mouse.current.rightButton.isPressed;
            }
            vrMap.Capture();
            gpMap.Capture();
            if (Mouse.current != null)
            {
                Vector2 mousePosition = new Vector2(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue());
                //  Debug.Log(mousePosition.ToString() + Screen.width + ", " + Screen.height);
                mouseY = (mousePosition.y - 0.5f * MainScript.screenSize.y) / (0.5f * MainScript.screenSize.y);
            }
        }
        public void Write(BinaryWriter bin)
        {
            bin.Write(keyCount);
            bin.Write(forward);
            bin.Write(back);
            bin.Write(left);
            bin.Write(right);
            bin.Write(up);
            bin.Write(down);
            bin.Write(mouseY);
            for (int i = 0; i < keyCount; i++)
            {
                bin.Write(pressed[i]);
                bin.Write(hold[i]);
            }
            bin.Write(button[0]);
            bin.Write(button[1]);
            gpMap.Write(bin);
            vrMap.Write(bin);
        }
        public void Read(BinaryReader bin)
        {
            forward = bin.ReadBoolean();
            back = bin.ReadBoolean();
            left = bin.ReadBoolean();
            right = bin.ReadBoolean();
            up = bin.ReadBoolean();
            down = bin.ReadBoolean();
            mouseY = bin.ReadSingle();
            for (int i = 0; i < keyCount; i++)
            {
                pressed[i] = bin.ReadBoolean();
                hold[i] = bin.ReadBoolean();
            }
            button[0] = bin.ReadBoolean();
            button[1] = bin.ReadBoolean();
            gpMap.Read(bin);
            vrMap.Read(bin);
        }
    }
}
