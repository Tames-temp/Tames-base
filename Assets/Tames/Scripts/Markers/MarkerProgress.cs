using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tames;
using UnityEngine;
namespace Markers
{
    public class MarkerProgress : MonoBehaviour
    {
        public ContinuityMode continuity = ContinuityMode.Stop;
        public float initialStatus = 0;
        public float setAt = 0;
        public float duration = 1;
        public string slerp = "";
        public string trigger = "";
        public GameObject byElement = null;
        public Material byMaterial = null;
        public CoupledInput manualControl;
        public string update = "";
        public bool active = true;
        public MonoInput activationControl;
        public MonoInput visibilityControl;
        public TameElement element = null;
        private bool changed = false;
        public void LateUpdate()
        {
            if (changed)
            {
                changed = false;
               element.UpdateMarkerProgress();
            }
        }
        public void ChangedThisFrame(bool shouldChange)
        {
            changed = shouldChange;
        }
        public static void PopulateAll(List<Tames.TameGameObject> tgos)
        {
        }
        // public class 

        public string[] ToLines()
        {
            return new string[]
            {
                MarkerSettings.ObjectToLine(gameObject),
                continuity.ToString(),
                initialStatus + "",
                setAt + "",
                duration + "",
                slerp,
                trigger,
                MarkerSettings.ObjectToLine(byElement),
                MarkerSettings.FindMaterial(byMaterial),
                manualControl.maxDistance + "",
                manualControl.maxAngle + "",
                manualControl.hold.ToString(),
                manualControl.pair,
                update,
                active ? "1" : "0",
                activationControl.maxDistance + "",
                activationControl.maxAngle + "",
                activationControl.hold.ToString(),
                activationControl.press,
                visibilityControl.maxDistance + "",
                visibilityControl.maxAngle + "",
                visibilityControl.hold.ToString(),
                visibilityControl.press
            };
        }
        public static int FromLines(string[] line, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(line[index]);
            MarkerProgress mp;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((mp = go.AddComponent<MarkerProgress>()) == null) mp = go.AddComponent<MarkerProgress>();
                        mp.continuity = line[index + 1] == "Stop" ? ContinuityMode.Stop : (line[index + 1] == "Cycle" ? ContinuityMode.Cycle : ContinuityMode.Reverse);
                        mp.initialStatus = float.Parse(line[index + 2]);
                        mp.setAt = float.Parse(line[index + 3]);
                        mp.duration = float.Parse(line[index + 4]);
                        mp.slerp = line[index + 5];
                        mp.trigger = line[index + 6];
                        mp.byElement = MarkerSettings.LineToObject(line[index + 7]);
                        mp.byMaterial = MarkerSettings.FindMaterial(line[index + 8]);
                        mp.manualControl = new()
                        {
                            maxDistance = float.Parse(line[index + 9]),
                            maxAngle = float.Parse(line[index + 10]),
                            hold = TameInputControl.StringToHold(line[index + 11]),
                            pair = line[index + 12],
                        };
                        mp.update = line[index + 13];
                        mp.active = line[index + 14] == "1";
                        mp.activationControl = new MonoInput()
                        {
                            maxDistance = float.Parse(line[index + 15]),
                            maxAngle = float.Parse(line[index + 16]),
                            hold = TameInputControl.StringToHold(line[index + 17]),
                            press = line[index + 18],

                        }; 
                        mp.visibilityControl = new MonoInput()
                        {
                            maxDistance = float.Parse(line[index + 19]),
                            maxAngle = float.Parse(line[index + 20]),
                            hold = TameInputControl.StringToHold(line[index + 21]),
                            press = line[index + 22],

                        };

                        return index + 22;
                }
            return index;
        }
    }
    [System.Serializable]
    public class MonoInput
    {
        public float maxDistance = 0;
        public float maxAngle = 0;
        public InputControlHold hold = InputControlHold.None;
        public string press;
    }
    [System.Serializable]
    public class CoupledInput
    {
        public float maxDistance = 0;
        public float maxAngle = 0;
        public InputControlHold hold = InputControlHold.None;
        public string pair;
    }

}
