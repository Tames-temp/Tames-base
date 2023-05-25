using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerFlicker:MonoBehaviour
    {
        public MarkerChanger.EditorChangerType property;
        public Material byMaterial = null;
        public GameObject byLight = null;
        public float minFlicker = 0.1f;
        public float maxFlicker = 0.2f;
        public int flickerCount = 3;
        public bool steadyPortion = false;
        public MaterialProperty GetProperty()
        {
            switch (property)
            {
                case MarkerChanger.EditorChangerType.Color: return MaterialProperty.Color;
                case MarkerChanger.EditorChangerType.U_Offset: return MaterialProperty.MapX;
                case MarkerChanger.EditorChangerType.V_Offset: return MaterialProperty.MapY;
                case MarkerChanger.EditorChangerType.EmissiveColor: return MaterialProperty.Glow;
                case MarkerChanger.EditorChangerType.Focus: return MaterialProperty.Focus;
                case MarkerChanger.EditorChangerType.Intensity: return MaterialProperty.Bright;
                case MarkerChanger.EditorChangerType.Emissive_U: return MaterialProperty.LightX;
                default: return MaterialProperty.LightY; //EditorChangerType.Emissive_V
            }
        }
    }
}
