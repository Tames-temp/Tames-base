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
            switch (property)
            {
                case EditorChangerType.Color: return MaterialProperty.Color;
                case EditorChangerType.U_Offset: return MaterialProperty.MapX;
                case EditorChangerType.V_Offset: return MaterialProperty.MapY;
                case EditorChangerType.EmissiveColor: return MaterialProperty.Glow;
                case EditorChangerType.Focus: return MaterialProperty.Focus;
                case EditorChangerType.Intensity: return MaterialProperty.Bright;
                case EditorChangerType.Emissive_U: return MaterialProperty.LightX;
                default : return MaterialProperty.LightY; //EditorChangerType.Emissive_V
            }
        }
        public ToggleType GetToggle()
        {
            switch (mode)
            {
                case EditorChangeStep.Gradual: return ToggleType.Gradient;
                case EditorChangeStep.Switch: return ToggleType.Switch;
                default: return ToggleType.Stepped;
            }
        }
       
    }
}
