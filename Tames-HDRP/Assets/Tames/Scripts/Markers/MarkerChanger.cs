using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerChanger : MonoBehaviour
    {
        public enum EditorChangerType
        {
            Color, U_Offset, V_Offset, EmissiveColor, Emissive_U, Emissive_V, Focus, Intensity
        }
        public EditorChangerType property;
        public enum EditorChangeStep
        {
            Stepped, Gradual, Switch
        }
        public GameObject byElement = null;
        public EditorChangeStep mode;
        public float switchValue;
        public string steps;
        public Color[] colorSteps;
        public float factor = 0;
        public Flicker flicker;
        public Tames.TameChanger changer = null;
        private bool changed = false;
        public void ChangedThisFrame(bool shouldChange)
        {
            if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
                changed = shouldChange;
            else changed = false;
        }
        private void LateUpdate()
        {
            if (changed)
            {
                changed = false;
                if (changer != null) changer.UpdateMarker();
            }
        }
        public MaterialProperty GetProperty()
        {
            return property switch
            {
                EditorChangerType.Color => MaterialProperty.Color,
                EditorChangerType.U_Offset => MaterialProperty.MapX,
                EditorChangerType.V_Offset => MaterialProperty.MapY,
                EditorChangerType.EmissiveColor => MaterialProperty.Glow,
                EditorChangerType.Focus => MaterialProperty.Focus,
                EditorChangerType.Intensity => MaterialProperty.Bright,
                EditorChangerType.Emissive_U => MaterialProperty.LightX,
                _ => MaterialProperty.LightY,//EditorChangerType.Emissive_V
            };
        }
        private static EditorChangerType CT(string s)
        {
            return s switch
            {
                "Color" => EditorChangerType.Color,
                "U_Offset" => EditorChangerType.U_Offset,
                "V_Offset" => EditorChangerType.V_Offset,
                "EmissiveColor" => EditorChangerType.EmissiveColor,
                "Focus" => EditorChangerType.Focus,
                "Intensity" => EditorChangerType.Intensity,
                "Emissive" => EditorChangerType.Emissive_U,
                _ => EditorChangerType.Emissive_V,
            };
        }
        public ToggleType GetToggle()
        {
            return mode switch
            {
                EditorChangeStep.Gradual => ToggleType.Gradual,
                EditorChangeStep.Switch => ToggleType.Switch,
                _ => ToggleType.Stepped,
            };
        }
        private static EditorChangeStep TT(string s)
        {
            return s switch
            {
                "Gradual" => EditorChangeStep.Gradual,
                "Switch" => EditorChangeStep.Switch,
                _ => EditorChangeStep.Stepped,

            };
        }
        private string ColorToString()
        {
            string s = "";
            for (int i = 0; i < colorSteps.Length; i++)
                s += (i != 0 ? "," : "") + colorSteps[i].r + ";" + colorSteps[i].g + ";" + colorSteps[i].b + ";" + colorSteps[i].a;
            return s;
        }
        private static Color[] StringToColor(string line)
        {
            string[] colors = line.Split(',');
            Color[] r = new Color[colors.Length];
            string[] rgba;
            for (int i = 0; i < colors.Length; i++)
            {
                rgba = colors[i].Split(';');
                r[i] = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
            }
            return r;
        }
        public string[] ToLines()
        {
            List<string> lines = new List<string>()
            {
                ":changer",
                MarkerSettings.ObjectToLine(gameObject),
                property.ToString(),
                mode.ToString() ,
                switchValue+"",
                steps,
                factor+"",
                MarkerSettings.ObjectToLine(byElement),
                ColorToString()
         };
            lines.AddRange(flicker.ToLines());
            return lines.ToArray();
        }
        public static int FromLines(string[] lines, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(lines[index]);
            MarkerChanger[] mas;
            MarkerChanger ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                    case 2:
                        mas = go.GetComponents<MarkerChanger>();
                        ma = null;
                        EditorChangerType type = CT(lines[index + 1]);
                        foreach (MarkerChanger changer in mas) if (changer.property == type)
                            { ma = changer; break; }
                        if (ma != null)
                            ma = go.AddComponent<MarkerChanger>();
                        ma.property = type;
                        ma.mode = TT(lines[index + 2]);
                        ma.switchValue = float.Parse(lines[index + 3]);
                        ma.steps = lines[index + 4];
                        if (version == 2)
                        {
                            ma.factor = float.Parse(lines[index + 5]);
                            ma.byElement = MarkerSettings.LineToObject(lines[index + 6]);
                            ma.colorSteps = StringToColor(lines[index + 7]);
                            ma.flicker.active = lines[index + 8] == "1";
                            ma.flicker.byMaterial = MarkerSettings.FindMaterial(lines[index + 9]);
                            ma.flicker.byLight = MarkerSettings.LineToObject(lines[index + 10]);
                            ma.flicker.minFlicker = float.Parse(lines[index + 11]);
                            ma.flicker.maxFlicker = float.Parse(lines[index + 12]);
                            ma.flicker.flickerCount = int.Parse(lines[index + 13]);
                            ma.flicker.steadyPortion = lines[index + 14] == "1";
                        }
                        return index + 4 + (version == 2 ? 10 : 0);
                }
            return index;
        }
    }
    [System.Serializable]
    public class Flicker
    {
        public bool active = false;
        public Material byMaterial = null;
        public GameObject byLight = null;
        public float minFlicker = 0.1f;
        public float maxFlicker = 0.2f;
        public int flickerCount = 3;
        public bool steadyPortion = false;
        public string[] ToLines()
        {
            return new string[]
            {
                active?"1":"0",
                MarkerSettings.FindMaterial(byMaterial),
                MarkerSettings.ObjectToLine(byLight),
                minFlicker+"",
                maxFlicker+"",
                flickerCount+"",
                steadyPortion?"1":"0",
            };
        }
    }
}
