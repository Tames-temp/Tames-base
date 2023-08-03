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
        public string steps = "0";
        public float preset = 0;
        public float setTo = 0;
        public float duration = 1;
        public string lerpXY = "";
        public string trigger = "";
        public GameObject parent = null;
        public Material byMaterial = null;
        //      public InputSetting manualControl;
        public string update = "";
        public bool active = true;
        //      public InputSetting activationControl;
        //      public InputSetting visibilityControl;
        public TameElement element = null;
        private bool changed = false;
        public void LateUpdate()
        {
            if (changed)
            {
          //      Debug.Log(gameObject.name);
                changed = false;
                if (element != null)
                    element.UpdateMarkerProgress();
            }
        }
        public void ChangedThisFrame(bool shouldChange)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            changed = shouldChange;
            else changed = false;
#endif
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
                preset + "",
                setTo + "",
                duration + "",
                lerpXY,
                trigger,
                MarkerSettings.ObjectToLine(parent),
                MarkerSettings.FindMaterial(byMaterial),
            //    manualControl.ToString(),
                update,
                active ? "1" : "0",
             //   activationControl.ToString(),
         //       visibilityControl.ToString(),
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
                        mp.preset = float.Parse(line[index + 2]);
                        mp.setTo = float.Parse(line[index + 3]);
                        mp.duration = float.Parse(line[index + 4]);
                        mp.lerpXY = line[index + 5];
                        mp.trigger = line[index + 6];
                        mp.parent = MarkerSettings.LineToObject(line[index + 7]);
                        mp.byMaterial = MarkerSettings.FindMaterial(line[index + 8]);
                        //    mp.manualControl = InputSetting.FromString(line[index + 9]);
                        mp.update = line[index + 10];
                        mp.active = line[index + 11] == "1";
                        //      mp.activationControl = InputSetting.FromString(line[index + 12]);
                        //        mp.visibilityControl = InputSetting.FromString(line[index + 13]);

                        return index + 13;
                }
            return index;
        }
    }


}
