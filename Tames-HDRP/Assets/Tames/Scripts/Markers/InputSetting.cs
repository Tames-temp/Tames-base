using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tames;
using UnityEngine;

namespace Markers
{
    [System.Serializable]
    public class InputSetting
    {
        public enum Axis { None, X, Y, Z, NegX, NegY, NegZ }
        public enum KeyHold { Shift, Alt, Ctrl, None }
        public enum GPHold { LeftTrigger, RightTrigger, None }
        public float maxDistance = 0;
        public float maxAngle = 0;
        public Axis axis = Axis.None;
        public KeyHold auxKey = KeyHold.None;
        public string key = "";
        public GPHold auxGamepad = GPHold.None;
        public string gamepad = "";
        public GPHold auxVR = GPHold.None;
        public string controller = "";
        public List<TameInputControl> mono = new(), back = new(), forth = new();
        private ControlTypes type = ControlTypes.Mono;
        public InputControlHold Aux(int it)
        {
            return Aux(it == 0 ? InputTypes.KeyboardMouse : (it == 1 ? InputTypes.GamePad : InputTypes.VRController));
        }
        public InputControlHold Aux(InputTypes it)
        {
            switch (it)
            {
                case InputTypes.KeyboardMouse:
                    return auxKey switch
                    {
                        KeyHold.Shift => InputControlHold.Shift,
                        KeyHold.Alt => InputControlHold.Alt,
                        KeyHold.Ctrl => InputControlHold.Ctrl,
                        _ => InputControlHold.None
                    };
                case InputTypes.VRController:
                    return auxVR switch
                    {
                        GPHold.LeftTrigger => InputControlHold.VRTL,
                        GPHold.RightTrigger => InputControlHold.VRTR,
                        _ => InputControlHold.None
                    };
                default:
                    return auxGamepad switch
                    {
                        GPHold.LeftTrigger => InputControlHold.GTL,
                        GPHold.RightTrigger => InputControlHold.GTR,
                        _ => InputControlHold.None
                    };
            }
        }
        static KeyHold StringToKey(string s)
        {
            return s switch
            {
                "Shift" => KeyHold.Shift,
                "Alt" => KeyHold.Alt,
                "Ctrl" => KeyHold.Ctrl,
                _ => KeyHold.None,
            };
        }
        static GPHold StringToGP(string s)
        {
            return s switch
            {
                "LeftTrigger" => GPHold.LeftTrigger,
                "RightTrigger" => GPHold.RightTrigger,
                _ => GPHold.None,
            };
        }
        new public string ToString()
        {
            return maxDistance + ";" + maxAngle + ";" + auxKey.ToString() + ";" + key + ";" + auxGamepad.ToString() + ";" + gamepad + ";" + auxVR.ToString() + ";" + controller;
        }

        public static InputSetting FromString(string s)
        {
            InputSetting input = new InputSetting();
            string[] a = s.Split(';');
            input.maxDistance = float.Parse(a[0]);
            input.maxAngle = float.Parse(a[1]);
            input.auxKey = StringToKey(a[2]);
            input.key = a[3];
            input.auxGamepad = StringToGP(a[4]);
            input.gamepad = a[5];
            input.auxVR = StringToGP(a[6]);
            input.controller = a[7];
            return input;
        }
        public enum ControlTypes
        {
            Mono, DualPress, DualHold
        }

        public void AssignControl(ControlTypes type)
        {
            this.type = type;
            switch (type)
            {
                case ControlTypes.Mono:
                    mono = TameInputControl.AllMonos(this);
                    break;
                case ControlTypes.DualPress:
                    back = new List<TameInputControl>();
                    forth = new List<TameInputControl>();
                    TameInputControl.AllDuos(this, back, forth);
                    break;
                case ControlTypes.DualHold:
                    mono = new List<TameInputControl>();
                    TameInputControl.AllDuos(this, mono);
                    break;
            }
        }
        public bool CheckMono(GameObject g)
        {
            int d;
            if (CoreTame.multiPlayer)
            {
                List<Person> persons = new List<Person>();
                for (int i = 0; i < Person.people.Length; i++)
                    if (g == null) persons.Add(Person.people[i]);
                    else if (Person.people[i] != null)
                        if (TameCamera.CheckDistanceAndAngle(g, Person.people[i], maxDistance, maxAngle, axis))
                            persons.Add(Person.people[i]);
                if (persons.Count == 0) return false;

                foreach (TameInputControl tci in mono)
                    foreach (Person p in persons)
                        if (tci.Pressed(p.keyMap)) return true;
            }
            else
            {
                bool possible = true;
                if (g != null) possible = TameCamera.CheckDistanceAndAngle(g, maxDistance, maxAngle, axis);
                if (g.name.StartsWith("1") && possible) Debug.Log("checkmono: " + g.name + " " + possible);
                if (mono.Count == 0) return possible;
                else foreach (TameInputControl tci in mono)
                        if (tci.Pressed()) return true;

            }
            return false;
        }
        public int CheckDualPressed(GameObject g)
        {
            int d;
            if (CoreTame.multiPlayer)
            {
                List<Person> persons = new List<Person>();
                for (int i = 0; i < Person.people.Length; i++)
                    if (g == null) persons.Add(Person.people[i]);
                    else if (Person.people[i] != null)
                        if (TameCamera.CheckDistanceAndAngle(g, Person.people[i], maxDistance, maxAngle, axis))
                            persons.Add(Person.people[i]);
                if (persons.Count == 0) return 0;
                foreach (TameInputControl tci in back)
                    foreach (Person p in persons)
                        if (tci.Pressed(p.keyMap)) return -1;
                foreach (TameInputControl tci in forth)
                    foreach (Person p in persons)
                        if (tci.Pressed(p.keyMap)) return 1;
            }
            else
            {
                bool possible = true;
                if (g != null) possible = TameCamera.CheckDistanceAndAngle(g, maxDistance, maxAngle, axis);
                if (back.Count == 0) return 0;
                else
                {
                    foreach (TameInputControl tci in back)
                        if (tci.Pressed()) return -1;
                    foreach (TameInputControl tci in forth)
                        if (tci.Pressed()) return 1;
                }
            }
            return 0;
        }
        public int CheckDualHeld(GameObject g)
        {
            int dp, dir = 0;
            float dist, min = float.PositiveInfinity;
            if (CoreTame.multiPlayer)
            {
                foreach (TameInputControl tci in mono)
                    for (int i = 0; i < Person.people.Length; i++)
                        if (Person.people[i] != null)
                        {
                            if (TameCamera.CheckDistanceAndAngle(g, Person.people[i], maxDistance, maxAngle, axis))
                            {
                                dp = tci.Hold(Person.people[i].keyMap);
                                if (dir == 0)
                                {
                                    min = Vector3.Distance(Person.people[i].headPosition, g.transform.position);
                                    dir = dp;
                                }
                                else if (dp != 0)
                                    if ((dist = Vector3.Distance(Person.people[i].headPosition, g.transform.position)) < min)
                                    {
                                        min = dist;
                                        dir = dp;
                                    }
                            }
                        }
            }
            else if (TameCamera.CheckDistanceAndAngle(g, maxDistance, maxAngle, axis))
                if (mono.Count > 0)
                    foreach (TameInputControl tci in mono)
                    {
                        dir = tci.Hold();
                        if (dir != 0)
                            break;
                    }
            return dir;
        }
        public int Check(GameObject g)
        {
            int d;
            if (CoreTame.multiPlayer)
            {
                List<Person> persons = new List<Person>();
                for (int i = 0; i < Person.people.Length; i++)
                    if (g == null) persons.Add(Person.people[i]);
                    else if (Person.people[i] != null)
                        if (TameCamera.CheckDistanceAndAngle(g, Person.people[i], maxDistance, maxAngle, axis))
                            persons.Add(Person.people[i]);
                if (persons.Count == 0) return 0;
                switch (type)
                {
                    case ControlTypes.Mono:
                        foreach (TameInputControl tci in mono)
                            foreach (Person p in persons)
                                if (tci.Pressed(p.keyMap)) return 1;
                        return 0;
                    case ControlTypes.DualPress:
                        foreach (TameInputControl tci in back)
                            foreach (Person p in persons)
                                if (tci.Pressed(p.keyMap)) return -1;
                        foreach (TameInputControl tci in forth)
                            foreach (Person p in persons)
                                if (tci.Pressed(p.keyMap)) return 1;
                        return 0;
                    case ControlTypes.DualHold:
                        foreach (TameInputControl tci in mono)
                            foreach (Person p in persons)
                                if ((d = tci.Hold(p.keyMap)) != 0) return d;
                        return 0;
                }
            }
            else
            {
                bool possible = true;
                if (g != null) possible = TameCamera.CheckDistanceAndAngle(g, maxDistance, maxAngle, axis);

                if (possible)

                    switch (type)
                    {
                        case ControlTypes.Mono:
                            foreach (TameInputControl tci in mono)
                                if (tci.Pressed()) return 1;
                            return 0;
                        case ControlTypes.DualPress:
                            foreach (TameInputControl tci in back)
                                if (tci.Pressed()) return -1;
                            foreach (TameInputControl tci in forth)

                                if (tci.Pressed()) return 1;
                            return 0;
                        case ControlTypes.DualHold:
                            foreach (TameInputControl tci in mono)
                                if ((d = tci.Hold()) != 0) return d;
                            return 0;
                    }
            }
            return 0;
        }
    }
}
