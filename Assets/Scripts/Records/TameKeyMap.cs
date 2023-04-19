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
        private const uint u0 = 1, u1 = 2, u2 = 4, u3 = 8, u4 = 16, u5 = 32, u6 = 64, u7 = 128, u8 = 256, u9 = 512, u10 = 1024, u11 = 2048, u12 = 4096, u13 = 8192, u14 = 16384, u15 = 32768, u16 = 65536, u17 = 131072, u18 = 262144, u19 = 524288, u20 = 1048576, u21 = 2097152, u22 = 4194304, u23 = 8388608, u24 = 16777216, u25 = 33554432, u26 = 67108864, u27 = 134217728, u28 = 268435456, u29 = 536870912, u30 = 1073741824, u31 = 2147483648;

        public VRMap()
        {
            trigger = new float[] { 0, 0 };
            grip = new float[] { 0, 0 };
            stick = new Vector2[] { Vector2.zero, Vector2.zero };
            thumb = new float[] { 0, 0 };
            A = new bool[] { false, false };
            B = new bool[] { false, false };
        }
        public uint UPressed = 0;
        public uint UHold
        {
            get
            {
                uint r = (trigger[0] > 0.5f ? u0 : 0u)
                    + (trigger[1] > 0.5f ? u1 : 0u)
                    + (grip[0] > 0.5f ? u2 : 0u)
                    + (grip[1] > 0.5f ? u3 : 0u)
                    + (stick[0].x > 0.5f ? u4 : 0u)
                    + (stick[0].x < -0.5f ? u5 : 0u)
                    + (stick[0].y > 0.5f ? u6 : 0u)
                    + (stick[0].y < -0.5f ? u7 : 0u)
                    + (stick[1].x > 0.5f ? u8 : 0u)
                    + (stick[1].x < -0.5f ? u9 : 0u)
                    + (stick[1].y > 0.5f ? u10 : 0u)
                    + (stick[1].y < -0.5f ? u11 : 0u)
                    + (thumb[0] > 0.5f ? u12 : 0u)
                    + (thumb[0] < 0.5f ? u13 : 0u)
                    + (thumb[1] > 0.5f ? u14 : 0u)
                    + (thumb[1] < 0.5f ? u15 : 0u)
                    + (A[0] ? u16 : 0u)
                    + (A[1] ? u17 : 0u)
                    + (B[0] ? u18 : 0u)
                    + (B[1] ? u19 : 0u);
                return r;
            }
            set
            {
                trigger[0] = (value & (u0)) > 0 ? 1 : 0;
                trigger[1] = (value & (u1)) > 0 ? 1 : 0;
                grip[0] = (value & (u2)) > 0 ? 1 : 0;
                grip[1] = (value & (u3)) > 0 ? 1 : 0;
                float x = (value & (u4)) > 0 ? 1 : 0;
                x = (value & (u5)) > 0 ? -1 : x;
                float y = (value & (u6)) > 0 ? 1 : 0;
                y = (value & (u7)) > 0 ? -1 : y;
                stick[0] = new Vector2(x, y);
                x = (value & (u8)) > 0 ? 1 : 0;
                x = (value & (u9)) > 0 ? -1 : x;
                y = (value & (u9)) > 0 ? 1 : 0;
                y = (value & (u11)) > 0 ? -1 : y;
                stick[1] = new Vector2(x, y);
                x = (value & (u12)) > 0 ? 1 : 0;
                x = (value & (u13)) > 0 ? -1 : x;
                thumb[0] = x;
                x = (value & (u14)) > 0 ? 1 : 0;
                x = (value & (u15)) > 0 ? -1 : x;
                thumb[1] = x;
                A[0] = (value & (u16)) > 0;
                A[1] = (value & (u17)) > 0;
                B[0] = (value & (u18)) > 0;
                B[1] = (value & (u19)) > 0;
            }
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
    }
    public class GPMap
    {
        public bool[] pressed = new bool[8];
        public bool[] hold = new bool[8];
        public Vector2[] stick = new Vector2[] { Vector2.zero, Vector2.zero };
        public Vector2 dpad = Vector2.zero;
        public float[] trigger = new float[] { 0, 0 };
        public float[] shoulder = new float[] { 0, 0 };
        private const uint u0 = 1, u1 = 2, u2 = 4, u3 = 8, u4 = 16, u5 = 32, u6 = 64, u7 = 128, u8 = 256, u9 = 512, u10 = 1024, u11 = 2048, u12 = 4096, u13 = 8192, u14 = 16384, u15 = 32768, u16 = 65536, u17 = 131072, u18 = 262144, u19 = 524288, u20 = 1048576, u21 = 2097152, u22 = 4194304, u23 = 8388608, u24 = 16777216, u25 = 33554432, u26 = 67108864, u27 = 134217728, u28 = 268435456, u29 = 536870912, u30 = 1073741824, u31 = 2147483648;
        public GPMap()
        {
            for (int i = 0; i < 8; i++)
            {
                pressed[i] = false;
                hold[i] = false;
            }
        }
        public ulong UPressed
        {
            get
            {
                ulong r = 0;
                for (int i = 0; i < pressed.Length; i++)
                    r += pressed[i] ? 1u << i : 0;
                return r;
            }
            set
            {
                for (int i = 0; i < pressed.Length; i++)
                    pressed[i] = (value & (1u << i)) > 0;

            }
        }
        public ulong UHold
        {
            get
            {
                ulong r = 0;
                for (int i = 0; i < hold.Length; i++)
                    r += hold[i] ? 1u << i : 0;
                return r;
            }
            set
            {
                for (int i = 0; i < hold.Length; i++)
                    hold[i] = (value & (1u << i)) > 0;

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
    public class MouseMap
    {
        public bool[] pressed = new bool[] { false, false };
        public bool[] hold = new bool[] { false, false };
        public float y = 0;
        public uint U
        {
            get
            {
                return (pressed[0] ? 1u : 0u) + (pressed[1] ? 2u : 0u) + (hold[0] ? 4u : 0u) + (hold[0] ? 8u : 0u);
            }
            set
            {
                pressed[0] = (value & 1) > 0;
                pressed[1] = (value & 2) > 0;
                hold[0] = (value & 4) > 0;
                hold[1] = (value & 8) > 0;
            }
        }
    }
    public class TameKeyMap
    {
        public VRMap vrMap;
        public GPMap gpMap;
        public MouseMap mouse;
        public int keyCount;
        public bool[] pressed;
        public bool[] hold;
        public float[] values;
        public bool forward = false;
        public bool back = false;
        public bool left = false;
        public bool right = false;
        public bool up = false;
        public bool down = false;
        public bool shift = false;
        bool passed = false;
        public TameKeyMap(int keyCount)
        {
            this.keyCount = keyCount;
            pressed = new bool[keyCount];
            hold = new bool[keyCount];
            vrMap = new VRMap();
            gpMap = new GPMap();
            mouse = new MouseMap();
        }
        public FrameShot Capture()
        {
            if (Keyboard.current != null)
            {
                forward = Keyboard.current.wKey.isPressed;
                back = Keyboard.current.sKey.isPressed;
                left = Keyboard.current.aKey.isPressed;
                right = Keyboard.current.dKey.isPressed;
                up = Keyboard.current.rKey.isPressed;
                down = Keyboard.current.fKey.isPressed;
                shift = Keyboard.current.leftShiftKey.isPressed;
                for (int i = 0; i < keyCount; i++)
                {
                    pressed[i] = TameInputControl.checkedKeys[i].wasPressedThisFrame;
                    hold[i] = TameInputControl.checkedKeys[i].isPressed;
                }
            }
            if (Mouse.current != null)
            {
                mouse.hold[0] = Mouse.current.leftButton.isPressed;
                mouse.hold[1] = Mouse.current.rightButton.isPressed;
                mouse.pressed[0] = Mouse.current.leftButton.wasPressedThisFrame;
                mouse.pressed[1] = Mouse.current.rightButton.wasPressedThisFrame;
                Vector2 mousePosition = new Vector2(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue());
                mouse.y = (mousePosition.y - 0.5f * MainScript.screenSize.y) / (0.5f * MainScript.screenSize.y);
            }
            vrMap.Capture();
            gpMap.Capture();
            return ToFrameShot();
        }
        public ulong UPressed
        {
            get
            {
                ulong r = 0;
                for (int i = 0; i < keyCount; i++)
                    r += (pressed[i] ? 1u << i : 0);
                return r;
            }
            set
            {
                for (int i = 0; i < keyCount; i++)
                    pressed[i] = (value & (1u << i)) != 0;
            }
        }
        public ulong UHold
        {
            get
            {
                ulong r = 0;
                for (int i = 0; i < keyCount; i++)
                    r += (hold[i] ? 1u << i : 0);
                return r;
            }
            set
            {
                for (int i = 0; i < keyCount; i++)
                    hold[i] = (value & (1u << i)) != 0;
            }
        }
        FrameShot ToFrameShot()
        {
            FrameShot f = new FrameShot();
            f.KBPressed = UPressed;
            f.KBHold = UHold;
            f.GPPressed = gpMap.UPressed;
            f.GPHold = gpMap.UHold;
            f.VRPressed = vrMap.UPressed;
            f.VRHold = vrMap.UHold;
            f.mouse = mouse.U;
            return f;
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
            bin.Write(mouse.y);
            bin.Write(UPressed);
            bin.Write(UHold);
            bin.Write(mouse.U);
            bin.Write(gpMap.UPressed);
            bin.Write(gpMap.UHold);
            bin.Write(vrMap.UPressed);
            bin.Write(vrMap.UHold);
        }
        public void Read(BinaryReader bin)
        {
            forward = bin.ReadBoolean();
            back = bin.ReadBoolean();
            left = bin.ReadBoolean();
            right = bin.ReadBoolean();
            up = bin.ReadBoolean();
            down = bin.ReadBoolean();
            mouse.y = bin.ReadSingle();
            UPressed = bin.ReadUInt64();
            UHold = bin.ReadUInt64();
            mouse.U = bin.ReadUInt32();
            gpMap.UPressed = bin.ReadUInt64();
            gpMap.UHold = bin.ReadUInt64();
            vrMap.UPressed = bin.ReadUInt32();
            vrMap.UHold = bin.ReadUInt32();
        }
        public FrameShot Aggregate(FrameShot[] fs, FrameShot local)
        {
            if (MainScript.multiPlayer)
            {
                FrameShot f = new FrameShot();
                ulong ul = 0;
                uint ui = 0;
                for (int i = 0; i < fs.Length; i++)
                    ul |= fs[i].KBPressed;
                f.KBPressed = ul;
                ul = 0;
                for (int i = 0; i < fs.Length; i++)
                    ul |= fs[i].KBHold;
                f.KBHold = ul;
                for (int i = 0; i < fs.Length; i++)
                    ui |= fs[i].mouse;
                f.mouse = ui;
                ul = 0;
                for (int i = 0; i < fs.Length; i++)
                    ul |= fs[i].GPPressed;
                f.GPPressed = ul;
                ul = 0;
                for (int i = 0; i < fs.Length; i++)
                    ul |= fs[i].GPHold;
                f.GPHold = ul;
                ui = 0;
                for (int i = 0; i < fs.Length; i++)
                    ui |= fs[i].VRPressed;
                f.VRPressed = ui;
                for (int i = 0; i < fs.Length; i++)
                    ui |= fs[i].VRHold;
                f.VRHold = ui;
                return f;
            }
            else
                return local;
        }
    }
}
