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
        public EditorChangeStep mode;
        public float switchValue;
        public string steps;

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
                EditorChangeStep.Gradual => ToggleType.Gradient,
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
        public string[] ToLines()
        {
            return new string[]
            {
                ":changer",
                MarkerSettings.ObjectToLine(gameObject),
                property.ToString(),
                mode.ToString() ,
                switchValue+"",
                steps
            };
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
                        mas= go.GetComponents<MarkerChanger>();
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
                        return index + 5;
                }
            return index;
        }
    }
}
